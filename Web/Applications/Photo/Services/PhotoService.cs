//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Events;
using System.Web;
using Tunynet.Caching;
using Tunynet.Common.Configuration;
using System.IO;
using Tunynet.FileStore;
using Tunynet.Imaging;
using System;

namespace Spacebuilder.Photo
{

    /// <summary>
    /// 相册业务逻辑类
    /// </summary>
    public class PhotoService
    {
        private IAlbumRepository albumRepository;
        private IPhotoRepository photoRepository;
        private IPhotoLabelRepository photoLabelRepository;

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public PhotoService()
            : this(new AlbumRepository(), new PhotoRepository(), new PhotoLabelRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="albumRepository">相册仓储实现</param>
        /// <param name="photoRepository">相册仓储实现</param>
        /// <param name="photoLabelRepository">相册圈人实现</param>
        public PhotoService(IAlbumRepository albumRepository, IPhotoRepository photoRepository, IPhotoLabelRepository photoLabelRepository)
        {
            this.albumRepository = albumRepository;
            this.photoRepository = photoRepository;
            this.photoLabelRepository = photoLabelRepository;
        }

        #endregion

        #region 相册维护
        /// <summary>
        /// 创建相册
        /// </summary>
        /// <remarks>
        /// 1.注意Insert之后需要调用Update方法更新一下DisplayOrder字段
        /// 2.使用AuditService.ChangeAuditStatusForCreate设置审核状态；
        /// </remarks>
        /// <param name="album">相册对象</param>
        /// <returns>返回Bool值</returns>
        public bool CreateAlbum(Album album)
        {
            EventBus<Album>.Instance().OnBefore(album, new CommonEventArgs(EventOperationType.Instance().Create()));

            //设置审核状态
            new AuditService().ChangeAuditStatusForCreate(album.UserId, album);

            albumRepository.Insert(album);
            if (album.AlbumId > 0)
            {
                album.DisplayOrder = album.AlbumId;
                albumRepository.Update(album);
            }

            EventBus<Album>.Instance().OnAfter(album, new CommonEventArgs(EventOperationType.Instance().Create()));
            EventBus<Album, AuditEventArgs>.Instance().OnAfter(album, new AuditEventArgs(null, album.AuditStatus));

            return album.AlbumId > 0;
        }

        /// <summary>
        /// 编辑相册
        /// </summary>
        /// <remarks>
        /// 调用私有方法UpdateAlbum
        /// </remarks>
        /// <param name="album">相册对象</param>
        /// <param name="userId">当前操作人</param>
        public void UpdateAlbum(Album album, long userId)
        {
            UpdateAlbum(album, userId, true);
        }

        /// <summary>
        /// 编辑相册
        /// </summary>
        /// <remarks>
        /// 1.使用AuditService.ChangeAuditStatusForUpdate设置审核状态； 
        /// 2.判断PrivacyStatus是否有变化，如有变化，调用PhotoRepository的UpdatePrivacyStatus方法更新照片的隐私状态
        /// 3.需要触发的事件:1)Update的OnBefore、OnAfter；2)审核状态变更
        /// </remarks>
        /// <param name="album">相册对象</param>
        /// <param name="userId">当前操作人</param>
        /// <param name="changeAuditStatusForUpdate">是否更新审核状态</param>
        private void UpdateAlbum(Album album, long userId, bool changeAuditStatusForUpdate)
        {
            EventBus<Album>.Instance().OnBefore(album, new CommonEventArgs(EventOperationType.Instance().Update()));

            //设置审核状态
            AuditStatus prevAuditStatus = album.AuditStatus;
            if (changeAuditStatusForUpdate)
            {
                new AuditService().ChangeAuditStatusForUpdate(userId, album);
            }

            //设置隐私状态      
            photoRepository.UpdatePrivacyStatus(album.AlbumId);

            albumRepository.Update(album);

            EventBus<Album>.Instance().OnAfter(album, new CommonEventArgs(EventOperationType.Instance().Update()));
            EventBus<Album, AuditEventArgs>.Instance().OnAfter(album, new AuditEventArgs(prevAuditStatus, album.AuditStatus));
        }

        /// <summary>
        /// 删除相册
        /// </summary>
        /// <remarks>
        /// 1.删除相册时同时删除相册下的所有照片以及照片的圈人；
        /// 2.注意需要删除动态，通过EventModule处理；
        /// 3.注意需要扣除新建相册时增加的积分，通过EventModule处理；
        /// 4.需要触发的事件:1)Delete的OnBefore、OnAfter；2)审核状态变更
        /// </remarks>
        /// <param name="album">相册对象</param>
        public void DeleteAlbum(Album album)
        {
            //删除相册下相片
            int pageCount = 1;
            int pageIndex = 1;
            do
            {
                PagingDataSet<Photo> photos = this.GetPhotosOfAlbum(null, album.AlbumId, true, SortBy_Photo.DateCreated_Desc, null, 100, pageIndex);
                foreach (Photo photo in photos)
                {
                    this.DeletePhoto(photo);
                }
                pageCount = photos.PageCount;
                pageIndex++;
            } while (pageIndex <= pageCount);

            //删除相册的推荐
            RecommendService recommendService = new RecommendService();
            recommendService.Delete(album.AlbumId, TenantTypeIds.Instance().Album());

            EventBus<Album>.Instance().OnBefore(album, new CommonEventArgs(EventOperationType.Instance().Delete()));

            albumRepository.Delete(album);

            EventBus<Album>.Instance().OnAfter(album, new CommonEventArgs(EventOperationType.Instance().Delete()));
            EventBus<Album, AuditEventArgs>.Instance().OnAfter(album, new AuditEventArgs(album.AuditStatus, null));
        }

        /// <summary>
        /// 审核相册
        /// </summary>
        /// <remarks>
        /// 1.需要触发的事件：1) 批准或不批准；2) 审核状态变更；
        /// </remarks>
        /// <param name="albumId">相册ID</param>
        /// <param name="isApproved">是否通过审核</param>
        public void ApproveAlbum(long albumId, bool isApproved)
        {
            Album album = albumRepository.Get(albumId);
            AuditStatus prevAuditStatus = album.AuditStatus;
            AuditStatus auditStatus = AuditStatus.Fail;
            if (isApproved)
            {
                auditStatus = AuditStatus.Success;
            }
            if (album.AuditStatus != auditStatus)
            {
                album.AuditStatus = auditStatus;
                albumRepository.Update(album);
            }

            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            EventBus<Album>.Instance().OnAfter(album, new CommonEventArgs(operationType));
            EventBus<Album, AuditEventArgs>.Instance().OnAfter(album, new AuditEventArgs(prevAuditStatus, auditStatus));

        }

        /// <summary>
        /// 删除用户时处理其相册内容
        /// </summary>
        /// <remarks>
        /// 1.如果需要接管，直接执行Repository的方法更新数据库记录
        /// 2.如果不需要接管，调用DeleteAlbum、DeletePhoto、DeleteLabel循环删除该用户下的所有相册、照片、圈人
        /// </remarks>
        /// <param name="userId">用户ID</param>
        /// <param name="takeOverUserName">指定接管用户的用户名</param>
        /// <param name="isTakeOver">是否接管</param>
        public void DeleteUser(long userId, string takeOverUserName, bool isTakeOver)
        {
            if (isTakeOver)
            {
                IUserService userService = DIContainer.Resolve<IUserService>();
                User takeOverUser = userService.GetFullUser(takeOverUserName);
                albumRepository.TakeOver(userId, takeOverUser);
            }
            else
            {
                //删除用户下相册
                int pageCount = 1;
                int pageIndex = 1;
                do
                {
                    PagingDataSet<Album> albums = this.GetUserAlbums(null, userId, true, SortBy_Album.DateCreated_Desc, 100, pageIndex);
                    foreach (Album album in albums)
                    {
                        this.DeleteAlbum(album);
                    }
                    pageCount = albums.PageCount;
                    pageIndex++;
                } while (pageIndex <= pageCount);
            }
        }
        #endregion

        #region 相册查询
        /// <summary>
        /// 获取相册对象
        /// </summary>
        /// <param name="albumId">相册ID</param>
        /// <returns>返回相册对象</returns>
        public Album GetAlbum(long albumId)
        {
            return albumRepository.Get(albumId);
        }

        /// <summary>
        /// 获取相册列表（用于频道）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        public PagingDataSet<Album> GetAlbums(string tenantTypeId, long? ownerId, SortBy_Album sortBy = SortBy_Album.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return albumRepository.GetAlbums(tenantTypeId, ownerId, sortBy, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取相册列表（用于用户空间）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="userId">用户ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        public PagingDataSet<Album> GetUserAlbums(string tenantTypeId, long userId, bool ignoreAuditAndPrivacy, SortBy_Album sortBy = SortBy_Album.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return albumRepository.GetUserAlbums(tenantTypeId, userId, ignoreAuditAndPrivacy, sortBy, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取相册列表（用于群组）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回相册分页列表</returns>
        public PagingDataSet<Album> GetOwnerAlbums(string tenantTypeId, long ownerId, bool ignoreAuditAndPrivacy, SortBy_Album sortBy = SortBy_Album.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return albumRepository.GetOwnerAlbums(tenantTypeId, ownerId, ignoreAuditAndPrivacy, sortBy, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取相册列表（根据Id组装实体）
        /// </summary>
        /// <param name="albumIds">相册ID列表</param>
        /// <returns>返回相册列表</returns>
        public IEnumerable<Album> GetAlbums(IEnumerable<long> albumIds)
        {
            return albumRepository.PopulateEntitiesByEntityIds(albumIds);
        }

        /// <summary>
        /// 管理员获取相册数据
        /// </summary>
        /// <remarks>无需缓存</remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="nameKeyword">相册名称</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<Album> GetAlbumsForAdmin(string tenantTypeId, string nameKeyword, long? userId, long? ownerId, AuditStatus? auditStatus, int pageSize, int pageIndex)
        {
            return albumRepository.GetAlbumsForAdmin(tenantTypeId, nameKeyword, userId, ownerId, auditStatus, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取相册统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回相册统计数据</returns>
        public Dictionary<string, long> GetAlbumApplicationStatisticData(string tenantTypeId = null)
        {
            return albumRepository.GetAlbumApplicationStatisticData(tenantTypeId);
        }

        /// <summary>
        /// 获取相册管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回相册管理数据</returns>
        public Dictionary<string, long> GetAlbumManageableData(string tenantTypeId = null)
        {
            return albumRepository.GetAlbumManageableData(tenantTypeId);
        }
        #endregion

        #region 照片维护
        /// <summary>
        /// 创建照片
        /// </summary>
        /// <remarks>
        /// 1.更新相册的PhotoCount+1
        /// 2.更新用户内容计数OwnerData+1
        /// 3.需要同步相册的TenantTypeId、OwnerId、UserId、Author、PrivacyStatus字段
        /// 4.注意在EventModule中处理动态、积分；
        /// 5.使用AuditService.ChangeAuditStatusForCreate设置审核状态；
        /// 6.需要触发的事件:1)Create的OnBefore、OnAfter；2)审核状态变更；
        /// </remarks>
        /// <param name="photo">照片对象</param>
        /// <param name="file">上传文件</param>
        /// <returns>返回是否成功创建</returns>
        public bool CreatePhoto(Photo photo, HttpPostedFileBase file)
        {
            EventBus<Photo>.Instance().OnBefore(photo, new CommonEventArgs(EventOperationType.Instance().Create()));

            Album album = photo.Album;
            //需要同步相册的TenantTypeId、OwnerId、UserId、Author、PrivacyStatus字段
            photo.TenantTypeId = album.TenantTypeId;
            photo.OwnerId = album.OwnerId;
            photo.UserId = album.UserId;
            photo.Author = photo.User != null ? photo.User.DisplayName : album.Author;
            photo.PrivacyStatus = album.PrivacyStatus;

            //设置审核状态
            new AuditService().ChangeAuditStatusForCreate(album.UserId, photo);

            //创建照片
            photoRepository.Insert(photo);
            this.UploadPhoto(photo, file);
            photoRepository.Update(photo);

            //更新相册PhotoCount,LastUploadDate
            album.PhotoCount++;
            album.LastUploadDate = photo.DateCreated;
            albumRepository.Update(album);

            //用户内容计数OwnerData+1
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
            ownerDataService.Change(photo.UserId, OwnerDataKeys.Instance().PhotoCount(), 1);

            //处理积分动态
            EventBus<Album>.Instance().OnAfter(album, new CommonEventArgs(EventOperationType.Instance().Create()));
            EventBus<Album, AuditEventArgs>.Instance().OnAfter(album, new AuditEventArgs(null, photo.AuditStatus));
            EventBus<Photo, AuditEventArgs>.Instance().OnAfter(photo, new AuditEventArgs(null, photo.AuditStatus));
            EventBus<Photo>.Instance().OnAfter(photo, new CommonEventArgs(EventOperationType.Instance().Create()));

            return photo.PhotoId > 0;
        }

        /// <summary>
        /// 照片上传
        /// </summary>
        /// <param name="photo">照片对象</param>
        /// <param name="file">需上传的文件</param>
        private void UploadPhoto(Photo photo, HttpPostedFileBase file)
        {
            if (file == null)
            {
                throw new ExceptionFacade("请上传文件");
            }

            TenantAttachmentSettings tenantAttachmentSettings = TenantAttachmentSettings.GetRegisteredSettings(TenantTypeIds.Instance().Photo());

            //验证图片类型
            if (!tenantAttachmentSettings.ValidateFileExtensions(file.FileName))
            {
                throw new ExceptionFacade(string.Format("只允许上传后缀名为{0}的文件", tenantAttachmentSettings.AllowedFileExtensions));
            }

            //验证图片大小
            if (!tenantAttachmentSettings.ValidateFileLength(file.ContentLength))
            {
                throw new ExceptionFacade(string.Format("文件大小不允许超过{0}", tenantAttachmentSettings.MaxAttachmentLength));
            }

            IStoreProvider storeProvider = DIContainer.ResolveNamed<IStoreProvider>(tenantAttachmentSettings.StoreProviderName);

            //根据AlbumId生成图片存储路径和新的文件名
            string idString = photo.AlbumId.ToString().PadLeft(15, '0');
            string relativePath = storeProvider.JoinDirectory(tenantAttachmentSettings.TenantAttachmentDirectory, idString.Substring(0, 5), idString.Substring(5, 5), idString.Substring(10, 5));
            string extension = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1).ToLower();
            string fileName = string.Format("{0}.{1}", Guid.NewGuid().ToString("N"), extension);

            //上传原始图片
            using (Stream stream = file.InputStream)
            {
                storeProvider.AddOrUpdateFile(relativePath, fileName, stream);

                if (extension != "gif")
                {
                    //根据设置生成不同尺寸的图片
                    if (tenantAttachmentSettings.ImageSizeTypes != null)
                    {
                        foreach (var imageSizeType in tenantAttachmentSettings.ImageSizeTypes)
                        {
                            Stream resizedStream = ImageProcessor.Resize(stream, imageSizeType.Size.Width, imageSizeType.Size.Height, imageSizeType.ResizeMethod);
                            storeProvider.AddOrUpdateFile(relativePath, storeProvider.GetSizeImageName(fileName, imageSizeType.Size, imageSizeType.ResizeMethod), resizedStream);
                            if (resizedStream != stream)
                            {
                                resizedStream.Dispose();
                            }
                        }
                    }
                }
            }

            photo.RelativePath = relativePath + "\\" + fileName;
        }


        /// <summary>
        /// 编辑照片
        /// </summary>
        /// <remarks>
        /// 调用私有方法UpdatePhoto
        /// </remarks>
        /// <param name="photo">照片对象</param>
        /// <param name="userId">当前操作者id</param>
        public void UpdatePhoto(Photo photo, long userId)
        {
            UpdatePhoto(photo, userId, true);
        }

        /// <summary>
        /// 编辑照片
        /// </summary>
        /// <remarks>
        /// 1.使用AuditService.ChangeAuditStatusForUpdate设置审核状态；
        /// 2.需要触发的事件:1)Update的OnBefore、OnAfter；2)审核状态变更；
        /// </remarks>
        /// <param name="photo">照片对象</param>
        /// <param name="userId">当前操作者id</param>
        /// <param name="changeAuditStatusForUpdate">是否更新审核字段</param>
        private void UpdatePhoto(Photo photo, long userId, bool changeAuditStatusForUpdate)
        {
            EventBus<Photo>.Instance().OnBefore(photo, new CommonEventArgs(EventOperationType.Instance().Update()));

            //设置审核状态
            AuditStatus prevAuditStatus = photo.AuditStatus;

            //设置审核状态
            if (changeAuditStatusForUpdate)
            {
                new AuditService().ChangeAuditStatusForUpdate(userId, photo);
            }

            photoRepository.Update(photo);

            EventBus<Photo>.Instance().OnAfter(photo, new CommonEventArgs(EventOperationType.Instance().Update()));
            EventBus<Photo, AuditEventArgs>.Instance().OnAfter(photo, new AuditEventArgs(prevAuditStatus, photo.AuditStatus));
        }

        /// <summary>
        /// 删除照片
        /// </summary>
        /// <remarks>
        /// 1.更新所属相册计数PhotoCount-1
        /// 2.更新用户内容计数OwnerData-1
        /// 3.如果照片是封面，需要将相册的CoverId属性重置为0
        /// 4.需要调用TagService.ClearTagsFromItem删除标签关联
        /// 5.需要同步删除照片圈人
        /// 6.通过EventModule处理动态和积分的变化；
        /// 7.需要触发的事件:1)Delete的OnBefore、OnAfter；2)审核状态变更
        /// </remarks>
        /// <param name="photo">照片对象</param>
        public void DeletePhoto(Photo photo)
        {
            //删除与标签的关联
            TagService tagService = new TagService(TenantTypeIds.Instance().Photo());
            tagService.ClearTagsFromItem(photo.PhotoId, photo.UserId);

            //删除照片推荐
            RecommendService recommendService = new RecommendService();
            recommendService.Delete(photo.PhotoId, TenantTypeIds.Instance().Photo());

            //相册计数PhotoCount-1及封面
            Album album = photo.Album;
            album.PhotoCount--;
            if (photo.PhotoId == album.CoverId)
            {
                album.CoverId = 0;
            }
            albumRepository.Update(album);

            //删除圈人
            IEnumerable<PhotoLabel> photoLabels = this.GetLabelsOfPhoto(null, photo.PhotoId);
            foreach (PhotoLabel photolabel in photoLabels)
            {
                photoLabelRepository.Delete(photolabel);
            }

            //删除照片磁盘物理文件
            if (!string.IsNullOrEmpty(photo.RelativePath))
            {
                TenantAttachmentSettings tenantAttachmentSettings = TenantAttachmentSettings.GetRegisteredSettings(TenantTypeIds.Instance().Photo());
                IStoreProvider storeProvider = DIContainer.ResolveNamed<IStoreProvider>(tenantAttachmentSettings.StoreProviderName);
                string relativePath = storeProvider.GetRelativePath(photo.RelativePath, true);
                string fileName = photo.RelativePath.Remove(0, relativePath.Length).Trim('\\').Trim('/');
                storeProvider.DeleteFiles(relativePath, fileName);
            }

            photoRepository.Delete(photo);

            EventBus<Photo>.Instance().OnBefore(photo, new CommonEventArgs(EventOperationType.Instance().Delete()));

            //用户内容计数OwnerData-1
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
            ownerDataService.Change(photo.UserId, OwnerDataKeys.Instance().PhotoCount(), -1);

            //通过EventModule处理动态和积分的变化；
            EventBus<Photo>.Instance().OnAfter(photo, new CommonEventArgs(EventOperationType.Instance().Delete()));
            EventBus<Photo, AuditEventArgs>.Instance().OnAfter(photo, new AuditEventArgs(photo.AuditStatus, null));
        }

        /// <summary>
        /// 更新审核状态
        /// </summary>
        ///<remarks>
        /// 1.需要触发的事件：1) 批准或不批准；2) 审核状态变更；
        /// 2.审核状态的变化影响动态的生成与删除、积分的变化
        /// </remarks>
        /// <param name="photoId">照片ID</param>
        /// <param name="isApproved">是否通过审核</param>
        public void ApprovePhoto(long photoId, bool isApproved)
        {
            Photo photo = photoRepository.Get(photoId);
            AuditStatus prevAuditStatus = photo.AuditStatus;
            AuditStatus auditStatus = AuditStatus.Fail;
            if (isApproved)
            {
                auditStatus = AuditStatus.Success;
            }
            if (photo.AuditStatus != auditStatus)
            {
                photo.AuditStatus = auditStatus;

                photoRepository.Update(photo);

                string eventOperationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
                EventBus<Photo>.Instance().OnAfter(photo, new CommonEventArgs(eventOperationType));
                EventBus<Photo, AuditEventArgs>.Instance().OnAfter(photo, new AuditEventArgs(prevAuditStatus, photo.AuditStatus));
            }
        }

        /// <summary>
        /// 设置照片精华
        /// </summary>
        /// <remarks>
        /// 1.精华状态未变化不用进行任何操作
        /// 2.在EventModule里处理积分
        /// 3.需要触发的事件：加精或取消精华SetEssential的OnAfter
        /// </remarks>
        /// <param name="photo">照片ID</param>
        /// <param name="isEssential">是否精华</param>
        public void SetEssential(Photo photo, bool isEssential)
        {
            if (photo.IsEssential != isEssential)
            {
                photoRepository.SetEssential(photo.PhotoId, isEssential);

                string eventOperationType = isEssential ? EventOperationType.Instance().SetEssential() : EventOperationType.Instance().CancelEssential();
                EventBus<Photo>.Instance().OnAfter(photo, new CommonEventArgs(eventOperationType));
            }
        }

        /// <summary>
        /// 移动照片
        /// </summary>
        /// <remarks>
        /// 1.更新原相册的PhotoCount-1
        /// 2.如果照片是封面，需要将原相册的CoverId属性重置为0
        /// 3.更新照片所属相册ID
        /// 4.审核状态不改变、精华不改变
        /// 5.更新目标相册的PhotoCount+1
        /// </remarks>
        /// <param name="photo">需要移动的照片对象</param>
        /// <param name="targetAlbum">目标相册对象</param>
        public bool MovePhoto(Photo photo, Album targetAlbum)
        {
            return photoRepository.MovePhoto(photo, targetAlbum);
        }

        /// <summary>
        /// 设置封面
        /// </summary>
        /// <param name="photo">照片对象</param>
        public void SetCover(Photo photo, bool isCover = true)
        {
            Album album = this.GetAlbum(photo.AlbumId);

            if (isCover)
                album.CoverId = photo.PhotoId;
            else if (album.CoverId == photo.PhotoId)
                album.CoverId = 0;

            albumRepository.Update(album);
        }
        #endregion

        #region 照片查询
        /// <summary>
        /// 获取照片对象
        /// </summary>
        /// <param name="photoId">照片ID</param>
        /// <returns>返回照片对象</returns>
        public Photo GetPhoto(long photoId)
        {
            return photoRepository.Get(photoId);
        }

        /// <summary>
        /// 获取照片列表（用于频道页面）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="tagName">标签</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        public PagingDataSet<Photo> GetPhotos(string tenantTypeId, string tagName, bool? isEssential, SortBy_Photo sortBy = SortBy_Photo.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return photoRepository.GetPhotos(tenantTypeId, tagName, isEssential, sortBy, null, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取用户照片列表（用于用户空间）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="tagName">标签</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        public PagingDataSet<Photo> GetUserPhotos(string tenantTypeId, long userId, bool ignoreAuditAndPrivacy, string tagName, bool? isEssential, SortBy_Photo sortBy = SortBy_Photo.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return photoRepository.GetUserPhotos(tenantTypeId, userId, ignoreAuditAndPrivacy, tagName, null, isEssential, sortBy, null, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取用户相册照片列表（用于用户空间）
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="albumId">相册ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        public PagingDataSet<Photo> GetPhotosOfAlbum(string tenantTypeId, long albumId, bool ignoreAuditAndPrivacy, SortBy_Photo sortBy = SortBy_Photo.DateCreated_Desc, DateTime? createDateTime = null, int pageSize = 20, int pageIndex = 1)
        {
            return photoRepository.GetUserPhotos(tenantTypeId, null, ignoreAuditAndPrivacy, null, albumId, null, sortBy, createDateTime, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取所有者照片列表（用于群组）
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合，使用分区缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型</param>
        /// <param name="ownerId">拥有者ID</param>
        /// <param name="ignoreAuditAndPrivacy">是否忽略审核状态和隐私，相册主人查看时为true</param>
        /// <param name="tagName">标签</param>
        /// <param name="albumId">相册ID</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回照片分页列表</returns>
        public PagingDataSet<Photo> GetOwnerPhotos(string tenantTypeId, long ownerId, bool ignoreAuditAndPrivacy, string tagName, long? albumId, bool? isEssential, SortBy_Photo sortBy = SortBy_Photo.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return photoRepository.GetOwnerPhotos(tenantTypeId, ownerId, ignoreAuditAndPrivacy, tagName, albumId, isEssential, sortBy, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取照片列表（根据Id组装实体）
        /// </summary>
        /// <param name="photoIds">照片ID集合</param>
        /// <returns>照片列表</returns>
        public IEnumerable<Photo> GetPhotos(IEnumerable<long> photoIds)
        {
            return photoRepository.PopulateEntitiesByEntityIds(photoIds);
        }

        /// <summary>
        /// 管理员获取照片数据
        /// </summary>
        /// <param name="tenantTypeIds">租户类型ID</param>
        /// <param name="descriptionKeyword">照片描述</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>返回管理员分页照片数据</returns>
        public PagingDataSet<Photo> GetPhotosForAdmin(string tenantTypeIds, string descriptionKeyword, long? userId, long? ownerId, bool? isEssential, AuditStatus? auditStatus, int pageSize, int pageIndex)
        {
            return photoRepository.GetPhotosForAdmin(tenantTypeIds, descriptionKeyword, userId, ownerId, isEssential, auditStatus, pageSize, pageIndex);
        }

        /// <summary>
        /// 照片统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回照片统计数据</returns>
        public Dictionary<string, long> GetPhotoApplicationStatisticData(string tenantTypeId = null)
        {
            return photoRepository.GetPhotoApplicationStatisticData(tenantTypeId);
        }

        /// <summary>
        /// 照片管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回照片管理数据</returns>
        public Dictionary<string, long> GetPhotoManageableData(string tenantTypeId = null)
        {
            return photoRepository.GetPhotoManageableData(tenantTypeId);
        }

        /// <summary>
        /// 获取用户关注的人的照片
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<Photo> GetPhotosOfFollowedUsers(long userId, int pageSize, int pageIndex)
        {
            return photoRepository.GetPhotosOfFollowedUsers(userId, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取照片的EXIF数据
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <returns>返回照片的EXIF数据</returns>
        public Dictionary<int, string> GetPhotoEXIFMetaData(long photoId)
        {
            Dictionary<int, string> EXIFMetaData = new Dictionary<int, string>();
            Photo photo = this.GetPhoto(photoId);
            if (photo == null || string.IsNullOrEmpty(photo.RelativePath) || (!photo.RelativePath.ToLower().EndsWith(".jpg") && !photo.RelativePath.ToLower().EndsWith(".jpeg")))
            {
                return EXIFMetaData;
            }
            TenantAttachmentSettings tenantAttachmentSettings = TenantAttachmentSettings.GetRegisteredSettings(TenantTypeIds.Instance().Photo());
            IStoreProvider storeProvider = DIContainer.ResolveNamed<IStoreProvider>(tenantAttachmentSettings.StoreProviderName);
            IStoreFile file = storeProvider.GetFile(photo.RelativePath, "");
            if (file != null)
                using (Stream stream = file.OpenReadStream())
                {
                    EXIFMetaData = new EXIFMetaDataService().Read(stream);
                }

            return EXIFMetaData;
        }
        #endregion

        #region 圈人查询
        /// <summary>
        /// 获取圈人对象
        /// </summary>
        /// <param name="labelId">圈人ID</param>
        /// <returns>返回圈人对象</returns>
        public PhotoLabel GetLabel(long labelId)
        {
            return photoLabelRepository.Get(labelId);
        }

        /// <summary>
        /// 获取圈人对象列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="tenantTypeId">租户类型ID</param>
        /// <param name="photoId">照片ID</param>
        /// <returns>返回圈人列表</returns>
        public IEnumerable<PhotoLabel> GetLabelsOfPhoto(string tenantTypeId, long photoId)
        {
            IEnumerable<PhotoLabel> LabelsOfPhoto = photoLabelRepository.GetLabelsOfPhoto(photoId, tenantTypeId);
            if (LabelsOfPhoto == null)
                return new List<PhotoLabel>();
            return LabelsOfPhoto;

        }

        #endregion

        #region 圈人维护

        /// <summary>
        /// 创建圈人
        /// </summary>
        /// <param name="label">圈人对象</param>
        /// <returns>返回创建是否成功</returns>
        public bool CreateLabel(PhotoLabel label)
        {
            photoLabelRepository.Insert(label);
            EventBus<PhotoLabel>.Instance().OnAfter(label, new CommonEventArgs(EventOperationType.Instance().Create()));
            return label.LabelId > 0;
        }

        /// <summary>
        /// 编辑圈人
        /// </summary>
        /// <param name="label">圈人对象</param>
        public void UpdateLabel(PhotoLabel label)
        {
            photoLabelRepository.Update(label);
        }

        /// <summary>
        /// 删除圈人
        /// </summary>
        /// <param name="label">圈人对象</param>
        public void DeleteLabel(PhotoLabel label)
        {
            photoLabelRepository.Delete(label);
            EventBus<PhotoLabel>.Instance().OnAfter(label, new CommonEventArgs(EventOperationType.Instance().Delete()));
        }
        #endregion
    }
}