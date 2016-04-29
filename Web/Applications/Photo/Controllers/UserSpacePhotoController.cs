//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;
using Tunynet.Utilities;
using Spacebuilder.Search;
using Tunynet.Search;
using System.Text;
using System.Web;
using System.Web.Routing;
using Tunynet.Common.Configuration;

namespace Spacebuilder.Photo.Controllers
{
    /// <summary>
    /// 相册用户控件控制器
    /// </summary>
    [TitleFilter(TitlePart = "相册", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.UserSpace, IsApplication = true)]
    [AnonymousBrowseCheck]
    [UserSpaceAuthorize]
    public class UserSpacePhotoController : Controller
    {
        public IPageResourceManager pageResourceManager { get; set; }
        public PhotoService photoService { get; set; }
        public UserService userService { get; set; }
        public ContentPrivacyService contentPrivacyService { get; set; }
        public Authorizer authorizer { get; set; }
        private AttitudeService attitudeService = new AttitudeService(TenantTypeIds.Instance().Photo()); 
        public ApplicationService applicationService { get; set; }
        private TagService tagService = new TagService(TenantTypeIds.Instance().User());
        private int pageSize = 20;
        private int maxPageIndex = 3;


        /// <summary>
        /// 获取Title前缀
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        public string GetTitle(string spaceKey)
        {
            string title = null;
            IUser user = userService.GetUser(spaceKey);

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null && currentUser.UserName == spaceKey)
            {
                title = "我";
            }
            else
            {
                title = user.DisplayName;
            }
            return title;
        }

        /// <summary>
        /// 相册首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Home(string spaceKey)
        {
            pageResourceManager.InsertTitlePart("相册首页");
            var currentuser = UserContext.CurrentUser;
            if (currentuser == null)
            {
                return Redirect(SiteUrls.Instance().Login(true));
            }

            if (currentuser.UserName != spaceKey)
            {
                return RedirectToAction("Ta", new { spaceKey = spaceKey });
            }
            return View();
        }

        /// <summary>
        /// 最新照片
        /// </summary>
        /// <returns></returns>
        public ActionResult Photos(string spaceKey)
        {
            IUser user = userService.GetUser(spaceKey);
            if (user == null)
            {
                return HttpNotFound();
            }

            pageResourceManager.InsertTitlePart(GetTitle(spaceKey) + "的最新照片");
            return View();
        }

        /// <summary>
        /// 相册列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Albums(string spaceKey, SortBy_Album sortBy = SortBy_Album.DateCreated_Desc, int pageIndex = 1, int pageSize = 20)
        {
            IUser user = userService.GetUser(spaceKey);
            if (user == null)
            {
                return HttpNotFound();
            }

            bool ignoreAuditAndPrivacy = false;
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null && currentUser.UserName == spaceKey)
            {
                ignoreAuditAndPrivacy = true;
            }

            PagingDataSet<Album> albumList = photoService.GetUserAlbums(TenantTypeIds.Instance().User(), user.UserId, ignoreAuditAndPrivacy, sortBy, pageSize, pageIndex);

            pageResourceManager.InsertTitlePart(GetTitle(spaceKey) + "的相册");

            return View(albumList);
        }

        /// <summary>
        /// 我的喜欢
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Favorites(string spaceKey)
        {
            User user = userService.GetFullUser(spaceKey);

            if (user == null)
            {
                return HttpNotFound();
            }

            pageResourceManager.InsertTitlePart(GetTitle(spaceKey) + "喜欢的照片");
            return View();
        }

        /// <summary>
        /// 我的其他相册
        /// </summary>
        /// <param name="spaceKey">用户标识</param>
        /// <param name="albumId">相册Id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _MyOtherAlbums(string spaceKey, long albumId)
        {
            string title = string.Empty;
            bool ignoreAuditAndPrivacy = false;

            User user = userService.GetFullUser(spaceKey);
            if (user == null)
            {
                return HttpNotFound();
            }

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null && currentUser.UserName == spaceKey)
            {
                ignoreAuditAndPrivacy = true;
            }

            title = user.Profile.ThirdPerson();

            IEnumerable<Album> albums = photoService.GetUserAlbums(TenantTypeIds.Instance().User(), user.UserId, ignoreAuditAndPrivacy, pageSize: 100, pageIndex: 1).Where(n => n.AlbumId != albumId).Take(4);
            ViewData["title"] = title;
            return View(albums);
        }

        /// <summary>
        /// 相册详细显示-列表模式
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AlbumDetailList(string spaceKey, long albumId, int pageSize = 20, int pageIndex = 1)
        {
            IUser user = userService.GetUser(spaceKey);
            if (user == null)
            {
                return HttpNotFound();
            }

            Album album = photoService.GetAlbum(albumId);
            if (album == null)
            {
                return HttpNotFound();
            }

            //验证是否通过审核
            long currentSpaceUserId = UserIdToUserNameDictionary.GetUserId(spaceKey);
            if (!authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId) && album.UserId != currentSpaceUserId
                && (int)album.AuditStatus < (int)(new AuditService().GetPubliclyAuditStatus(PhotoConfig.Instance().ApplicationId)))
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Title = "尚未通过审核",
                    Body = "由于当前问题尚未通过审核，您无法浏览当前内容。",
                    StatusMessageType = StatusMessageType.Hint
                }));

            IUser currentUser = UserContext.CurrentUser;
            bool ignoreAuditAndPrivacy = false;
            if (currentUser != null && currentUser.UserId == album.UserId)
            {
                ignoreAuditAndPrivacy = true;
            }

            if (!authorizer.Album_View(album))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有查看该相册的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            PagingDataSet<Photo> photos = photoService.GetPhotosOfAlbum(TenantTypeIds.Instance().User(), albumId, ignoreAuditAndPrivacy, pageSize: pageSize, pageIndex: pageIndex);
            ViewData["album"] = album;

            pageResourceManager.InsertTitlePart(album.AlbumName);

            return View(photos);
        }

        /// <summary>
        /// 相册详细显示-阅读模式
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AlbumDetailView(string spaceKey, long albumId, int pageSize = 10, int pageIndex = 1)
        {
            return AlbumDetailList(spaceKey, albumId, pageSize, pageIndex);
        }

        /// <summary>
        /// 点击更多时
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _PhotoList(string spaceKey, long albumId, int pageSize = 10, int pageIndex = 1)
        {
            ViewData["pageIndex"] = pageIndex;
            return AlbumDetailList(spaceKey, albumId, pageSize, pageIndex);
        }

        /// <summary>
        /// 照片管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PhotoManage(string spaceKey, long albumId, int pageSize = 20, int pageIndex = 1)
        {
            Album album = photoService.GetAlbum(albumId);
            if (album == null)
            {
                return HttpNotFound();
            }

            if (!authorizer.Album_Edit(album))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有管理照片的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            PagingDataSet<Photo> photos = photoService.GetPhotosOfAlbum(TenantTypeIds.Instance().User(), albumId, true, pageSize: pageSize, pageIndex: pageIndex);
            ViewData["album"] = album;

            pageResourceManager.InsertTitlePart("照片管理");

            return View(photos);
        }

        /// <summary>
        /// 标签下的照片
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Tag(string spaceKey, string tagName)
        {
            pageResourceManager.InsertTitlePart(tagName + "标签下的照片");
            ViewData["tagName"] = tagName;
            return View();
        }

        /// <summary>
        /// 他的相册
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Ta(string spaceKey, int pageIndex = 1)
        {
            IUser user = userService.GetUser(spaceKey);
            if (user == null)
            {
                return HttpNotFound();
            }

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null && currentUser.UserName == spaceKey)
            {
                return RedirectToAction("Home");
            }

            PagingDataSet<Album> albumList = photoService.GetUserAlbums(TenantTypeIds.Instance().User(), user.UserId, false, SortBy_Album.DateCreated_Desc, 5, 1);
            ViewData["newPhotos"] = photoService.GetUserPhotos(TenantTypeIds.Instance().User(), user.UserId, false, null, null, pageIndex: pageIndex);

            pageResourceManager.InsertTitlePart(user.DisplayName + "的相册");
            return View(albumList);
        }

        /// <summary>
        /// 编辑/创建相册
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _EditAlbum(string spaceKey, long albumId = 0, string callBack = null)
        {
            IUser user = userService.GetUser(spaceKey);
            Album album = null;
            if (user == null)
            {
                return HttpNotFound();
            }

            if (albumId == 0)
            {
                if (!authorizer.Album_Create())
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "没有创建相册的权限",
                        Title = "没有权限",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }
                album = Album.New();
                album.PrivacyStatus = PrivacyStatus.Public;
            }
            else
            {
                album = photoService.GetAlbum(albumId);
                if (album == null)
                {
                    return HttpNotFound();
                }
                if (!authorizer.Album_Edit(album))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "没有编辑相册的权限",
                        Title = "没有权限",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }
                Dictionary<int, IEnumerable<ContentPrivacySpecifyObject>> privacySpecifyObjects = contentPrivacyService.GetPrivacySpecifyObjects(TenantTypeIds.Instance().Album(), album.AlbumId);
                if (privacySpecifyObjects.ContainsKey(SpecifyObjectTypeIds.Instance().User()))
                {
                    IEnumerable<ContentPrivacySpecifyObject> userPrivacySpecifyObjects = privacySpecifyObjects[SpecifyObjectTypeIds.Instance().User()];
                    ViewData["userPrivacySpecifyObjects"] = string.Join(",", userPrivacySpecifyObjects.Select(n => n.SpecifyObjectId));
                }
                if (privacySpecifyObjects.ContainsKey(SpecifyObjectTypeIds.Instance().UserGroup()))
                {
                    IEnumerable<ContentPrivacySpecifyObject> userGroupPrivacySpecifyObjects = privacySpecifyObjects[SpecifyObjectTypeIds.Instance().UserGroup()];
                    ViewData["userGroupPrivacySpecifyObjects"] = string.Join(",", userGroupPrivacySpecifyObjects.Select(n => n.SpecifyObjectId));
                }
            }

            AlbumEditModel albumEditModel = album.AsEditModel();

            return View(albumEditModel);
        }

        /// <summary>
        /// 编辑/创建相册
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _EditAlbum(string spaceKey, AlbumEditModel albumEditModel)
        {
            IUser user = userService.GetUser(spaceKey);
            if (user == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "当前空间主人不存在！"));
            }

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Hint, "您尚未登录！"));
            }

            StatusMessageData message = null;
            Album album = albumEditModel.AsAlbum();

            if (albumEditModel.AlbumId == 0)//创建
            {
                if (!authorizer.Album_Create())
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "没有创建或编辑相册的权限",
                        Title = "没有权限",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }

                bool result = photoService.CreateAlbum(album);
                if (result)
                {
                    //message = new StatusMessageData(StatusMessageType.Success, album.AlbumId.ToString() + "&" + album.AlbumName);
                    //设置隐私状态
                    UpdatePrivacySettings(album, albumEditModel.PrivacyStatus1, albumEditModel.PrivacyStatus2);
                    return Json(new { MessageType = StatusMessageType.Success, MessageContent = "创建相册成功！", AlbumId = album.AlbumId, AlbumName = album.AlbumName });
                }
                else
                {
                    message = new StatusMessageData(StatusMessageType.Error, "创建相册失败！");
                }
            }
            else//编辑
            {
                if (!authorizer.Album_Edit(album))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "没有编辑相册的权限",
                        Title = "没有权限",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }
                photoService.UpdateAlbum(album, currentUser.UserId);
                message = new StatusMessageData(StatusMessageType.Success, "编辑相册成功！");
            }
            //设置隐私状态
            UpdatePrivacySettings(album, albumEditModel.PrivacyStatus1, albumEditModel.PrivacyStatus2);
            return Json(message);
        }

        /// <summary>
        /// 删除相册
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _DeleteAlbum(string spaceKey, long albumId)
        {
            long userId = UserIdToUserNameDictionary.GetUserId(spaceKey);
            Album album = photoService.GetAlbum(albumId);
            long albumCount = photoService.GetAlbums(TenantTypeIds.Instance().User(), userId).TotalRecords;
            StatusMessageData message = null;
            try
            {
                if (albumCount > 1)
                {
                    if (!authorizer.Album_Edit(album))
                    {
                        return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                        {
                            Body = "没有删除相册的权限",
                            Title = "没有权限",
                            StatusMessageType = StatusMessageType.Hint
                        }));
                    }
                    photoService.DeleteAlbum(album);
                    message = new StatusMessageData(StatusMessageType.Success, "删除相册成功！");
                }
                else
                {
                    message = new StatusMessageData(StatusMessageType.Hint, "至少要保留一个相册！");
                }
            }
            catch
            {
                message = new StatusMessageData(StatusMessageType.Error, "删除相册失败！");
            }

            return Json(message);
        }

        /// <summary>
        /// 设置相册封面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SetAlbumCover(string spaceKey, long photoId, bool isCover = true)
        {
            Photo cover = photoService.GetPhoto(photoId);

            StatusMessageData message = null;
            try
            {
                if (!authorizer.Album_Edit(cover.Album))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "没有设置封面的权限",
                        Title = "没有权限",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }
                photoService.SetCover(cover, isCover);
                message = new StatusMessageData(StatusMessageType.Success, "设置封面成功！");
            }
            catch
            {
                message = new StatusMessageData(StatusMessageType.Error, "设置封面失败！");
            }

            return Json(message);
        }

        /// <summary>
        /// 编辑照片
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _EditPhoto(string spaceKey, long photoId)
        {
            Photo photo = photoService.GetPhoto(photoId);
            ViewData["tags"] = tagService.GetItemInTagsOfItem(photoId).Select(n => n.TagName);
            return View(photo.AsPhotoEditModel());
        }

        /// <summary>
        /// 编辑照片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _EditPhoto(string spaceKey, PhotoEditModel photoEditModel)
        {
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Hint, "您尚未登录！"));
            }

            Photo photo = photoEditModel.AsPhoto();
            if (!authorizer.Photo_Edit(photo))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有编辑照片的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            //设置标签
            string relatedTags = Request.Form.Get<string>("RelatedTags");

            if (!string.IsNullOrEmpty(relatedTags))
            {
                tagService.ClearTagsFromItem(photo.PhotoId, photo.UserId);
                tagService.AddTagsToItem(relatedTags, photo.UserId, photo.PhotoId);
            }

            photoService.UpdatePhoto(photo, currentUser.UserId);

            return Json(new { description = photo.Description });
        }

        /// <summary>
        /// 删除照片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _DeletePhoto(string spaceKey, List<long> photoIds)
        {
            if (photoIds != null && photoIds.Count == 0)
            {
                photoIds = Request.Form.Gets<long>("photoIds").ToList();
            }

            if (photoIds != null && photoIds.Count > 0)
            {
                foreach (long photoId in photoIds)
                {
                    Photo photo = photoService.GetPhoto(photoId);
                    if (!authorizer.Photo_Edit(photo))
                    {
                        return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                        {
                            Body = "没有编辑照片的权限",
                            Title = "没有权限",
                            StatusMessageType = StatusMessageType.Hint
                        }));
                    }
                    else
                    {
                        photoService.DeletePhoto(photo);
                    }
                }
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "删除照片成功！"));
        }

        /// <summary>
        /// 移动照片
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _MovePhoto(string spaceKey, List<long> photoIds)
        {
            photoIds = Request.QueryString.Gets<long>("photoIds").ToList();
            long albumId = 0;
            long userId = 0;
            if (photoIds != null && photoIds.Count > 0)
            {
                var photo = photoService.GetPhoto(photoIds.FirstOrDefault());
                userId = photo.UserId;
                albumId = photo.AlbumId;
            }

            IEnumerable<Album> albums = photoService.GetUserAlbums(TenantTypeIds.Instance().User(), userId, true, SortBy_Album.DisplayOrder, 10000, 1).AsEnumerable<Album>();
            SelectList albumList = new SelectList(albums.Where(n => n.AlbumId != albumId).Select(n => new { text = StringUtility.Trim(n.AlbumName, 6), value = n.AlbumId }), "value", "text");
            ViewData["albumId"] = albumList;

            return View();
        }

        /// <summary>
        /// 移动照片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _MovePhoto(string spaceKey, long albumId = 0)
        {
            List<long> photoIds = Request.Form.Gets<long>("photoIds").ToList();

            albumId = Request.Form.Get<long>("albumId");
            Album album = photoService.GetAlbum(albumId);
            if (album == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "目标相册不存在！"));
            }

            if (photoIds != null && photoIds.Count > 0)
            {
                IEnumerable<Photo> photos = photoService.GetPhotos(photoIds);
                foreach (var photo in photos)
                {
                    if (!authorizer.Photo_Edit(photo))
                    {
                        return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                        {
                            Body = "没有编辑照片的权限",
                            Title = "没有权限",
                            StatusMessageType = StatusMessageType.Hint
                        }));
                    }
                    else
                    {
                        bool result = photoService.MovePhoto(photo, album);
                    }
                }
            }

            return Json(new StatusMessageData(StatusMessageType.Success, "移动照片成功！"));
        }

        /// <summary>
        /// 给照片贴标签
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _SetPhotoTag(string spaceKey)
        {
            string photoIds = Request.QueryString.Get<string>("photoIds");
            ViewData["photoIds"] = photoIds;
            return View();
        }

        /// <summary>
        /// 给照片贴标签
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SetPhotoTag(string spaceKey, string tags)
        {
            List<long> photoIds = Request.Form.Gets<long>("photoIds").ToList();
            if (photoIds != null && photoIds.Count > 0)
            {
                long userId = photoService.GetPhoto(photoIds.FirstOrDefault()).UserId;
                tags = Request.Form.Get<string>("tags", string.Empty);
                if (!string.IsNullOrEmpty(tags))
                {
                    IEnumerable<Photo> photos = photoService.GetPhotos(photoIds);
                    foreach (var photo in photos)
                    {
                        if (!authorizer.Photo_Edit(photo))
                        {
                            return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                            {
                                Body = "没有编辑照片的权限",
                                Title = "没有权限",
                                StatusMessageType = StatusMessageType.Hint
                            }));
                        }
                        else
                        {
                            tagService.AddTagsToItem(tags, userId, photo.PhotoId);
                        }
                    }
                }
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "贴标签成功！"));
        }

        #region 私有方法

        /// <summary>
        /// 设置隐私状态
        /// </summary>
        private void UpdatePrivacySettings(Album album, string privacyStatus1, string privacyStatus2)
        {
            Dictionary<int, IEnumerable<ContentPrivacySpecifyObject>> privacySpecifyObjects = Utility.GetContentPrivacySpecifyObjects(privacyStatus1, privacyStatus2, ((IPrivacyable)album).TenantTypeId, album.AlbumId);

            if (privacySpecifyObjects.Count > 0)
            {
                contentPrivacyService.UpdatePrivacySettings(album, privacySpecifyObjects);
            }

        }

        #endregion



        /// <summary>
        /// 上传照片(页面)
        /// </summary>
        public ActionResult Upload(string spaceKey, long albumId = 0)
        {
            IUser owner = userService.GetUser(spaceKey);
            if (owner == null)
            {
                return HttpNotFound();
            }

            var user = UserContext.CurrentUser;
            if (user == null)
            {
                return Redirect(SiteUrls.Instance().Login(true));
            }
            string errorMessage = string.Empty;
            if (!authorizer.Photo_Upload(spaceKey, out errorMessage))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = errorMessage,
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            TenantAttachmentSettings tenantAttachmentSettings = TenantAttachmentSettings.GetRegisteredSettings(TenantTypeIds.Instance().Photo());
            //提示可上传的图片大小、类型
            ViewData["attachmentLength"] = tenantAttachmentSettings.MaxAttachmentLength;
            ViewData["allowedFileExtensions"] = tenantAttachmentSettings.AllowedFileExtensions;
            //相册下拉框
            GetAlbumList(albumId);
            pageResourceManager.InsertTitlePart("上传照片");
            return View();
        }

        /// <summary>
        /// 上传照片
        /// </summary>
        [HttpPost]
        public ActionResult UploadPhoto(string spaceKey, long albumId = 0)
        {
            if (albumId <= 0)
            {
                return Json("请选择相册！");
            }
            string errorMessage = string.Empty;
            if (!authorizer.Photo_Create(photoService.GetAlbum(albumId), out errorMessage))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = errorMessage,
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            //上传照片及创建照片记录
            HttpPostedFileBase postFile = Request.Files["fileData"];
            PhotoEditModel photoEditModel = new PhotoEditModel();
            photoEditModel.AlbumId = albumId;
            Photo photo = photoEditModel.AsPhoto();
            try
            {
                photoService.CreatePhoto(photo, postFile);
            }
            catch (System.Exception e)
            {
                photoService.DeletePhoto(photo);
                return Json(e.Message);
            }

            return Json(photo.PhotoId);
        }

        /// <summary>
        /// 完成上传
        /// </summary>
        [HttpPost]
        public ActionResult CompleteUpload(string spaceKey, IEnumerable<string> photoDesctitions, IEnumerable<long> photoIds)
        {
            string errorMessage = string.Empty;
            if (!authorizer.Photo_Create(photoService.GetAlbum(photoService.GetPhoto(photoIds.First()).AlbumId), out errorMessage))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = errorMessage,
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            long albumId = 0;
            for (int i = 0; i < photoIds.Count(); i++)
            {
                Photo photo = photoService.GetPhoto(photoIds.ElementAt(i));
                albumId = photo.AlbumId;
                photo.Description = photoDesctitions.ElementAt(i);
                photoService.UpdatePhoto(photo, UserIdToUserNameDictionary.GetUserId(spaceKey));
                //设标签
                string tags = Request.Form.Get<string>("commonTag");
                tags += Request.Form.Get<string>("photoTags" + photoIds.ElementAt(i));
                if (!string.IsNullOrEmpty(tags))
                {
                    tagService.AddTagsToItem(tags, UserIdToUserNameDictionary.GetUserId(spaceKey), photoIds.ElementAt(i));
                }
            }

            return Redirect(SiteUrls.Instance().AlbumDetailList(spaceKey, albumId));
        }

        /// <summary>
        /// 相片局部页
        /// </summary>
        public ActionResult _PhotoItem(string spacekey, long photoId = 0)
        {
            Photo photo = photoService.GetPhoto(photoId);
            if (photo == null)
            {
                return View();
            }
            //获取当前相册的封面ID
            ViewData["CoverId"] = photoService.GetAlbum(photo.AlbumId).CoverId;
            return View(photo.AsPhotoEditModel());
        }

        /// <summary>
        /// 上传照片-普通上传(页面)
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadNormal(long albumId = 0, string spaceKey = null)
        {
            var user = UserContext.CurrentUser;
            string photoIds = Request.QueryString["photoIds"];
            if (user == null)
            {
                return Redirect(SiteUrls.Instance().Login(true));
            }
            if (user.UserName == spaceKey || authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
            {
                //相册下拉框
                albumId = TempData.Get<long>("albumId", 0);
                GetAlbumList(albumId);
                pageResourceManager.InsertTitlePart("上传照片");
                ViewData["photoIds"] = photoIds;
                return View();
            }
            else
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有创建或编辑照片的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
        }

        /// <summary>
        /// 上传照片-普通上传
        /// </summary>
        [HttpPost]
        public ActionResult UploadNormal(string spaceKey, long albumId = 0)
        {
            string errorMessage = string.Empty;
            if (!authorizer.Photo_Create(photoService.GetAlbum(albumId), out errorMessage))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = errorMessage,
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            string photoIds = Request.Form["photoIds"];
            RouteValueDictionary dic = new RouteValueDictionary();
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase postFile = Request.Files[i];
                if (!string.IsNullOrEmpty(postFile.FileName))
                {
                    PhotoEditModel photoEditModel = new PhotoEditModel();
                    photoEditModel.AlbumId = albumId;
                    Photo photo = photoEditModel.AsPhoto();
                    try
                    {
                        photoService.CreatePhoto(photo, postFile);
                        photoIds += photo.PhotoId + ",";
                        TempData["photoCount"] = i + 1;
                    }
                    catch (System.Exception e)
                    {
                        photoService.DeletePhoto(photo);
                        TempData["photoIds"] = photoIds;
                        TempData["errorMessage"] = e.Message.Replace("\r\n", "");
                        TempData["albumId"] = albumId;
                        TempData["isContinue"] = bool.Parse(Request.Form["isContinue"]);
                        return RedirectToAction("UploadEdit");
                    }
                }
            }
            TempData["photoIds"] = photoIds;
            TempData["errorMessage"] = "success";
            TempData["albumId"] = albumId;
            TempData["isContinue"] = bool.Parse(Request.Form["isContinue"]);
            return RedirectToAction("UploadEdit");
        }

        /// <summary>
        /// 上传照片-完善照片描述
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadEdit(string spaceKey, string photoIds = null)
        {
            if (photoIds == null)
            {
                photoIds = TempData.Get<string>("photoIds", null);
            }
            List<Photo> photos = new List<Photo>();
            if (photoIds != null)
            {
                foreach (var photoId in photoIds.TrimEnd(',').Split(','))
                {
                    Photo photo = photoService.GetPhoto(long.Parse(photoId));
                    if (photo != null)
                    {
                        photos.Add(photo);
                    }
                }
                //获取当前相册的封面ID
                ViewData["CoverId"] = photoService.GetAlbum(photos.First().AlbumId).CoverId;
            }
            pageResourceManager.InsertTitlePart("完善照片描述");
            return View(photos);
        }

        /// <summary>
        /// 侧边栏-标签云
        /// </summary>
        /// <returns></returns>
        public ActionResult _Tags(string spaceKey, int num = 30)
        {
            Dictionary<TagInOwner, int> tags = null;
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null && currentUser.UserName == spaceKey)
            {
                tags = tagService.GetOwnerTopTags(num, currentUser.UserId);
            }
            else
            {
                IUser user = userService.GetUser(spaceKey);
                tags = tagService.GetOwnerTopTags(num, user.UserId);
            }

            return View(tags);
        }

        /// <summary>
        /// 空间首页瀑布流局部页(相册首页)
        /// </summary>
        public ActionResult _HomeWaterFall(string spaceKey, int pageIndex = 1)
        {
            IUser user = userService.GetUser(spaceKey);
            if (user == null)
            {
                return HttpNotFound();
            }

            var currentuser = UserContext.CurrentUser;
            if (currentuser == null)
            {
                return Redirect(SiteUrls.Instance().Login(true));
            }

            if (currentuser.UserName == spaceKey || authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
            {
                PagingDataSet<Photo> photos = photoService.GetPhotosOfFollowedUsers(user.UserId, pageSize, pageIndex);
                return View(photos);
            }
            else
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有查看首页照片的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
        }

        /// <summary>
        /// 空间瀑布流局部页(最新相册)
        /// </summary>
        /// <param name="spaceKey">用户标识</param>
        /// <param name="tagName">标签名</param>
        /// <param name="isFavorite">是否是“我的喜欢”</param>
        /// <param name="showMore">是否显示更多链接</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public ActionResult _NewWaterFall(string spaceKey, string tagName = null, bool isFavorite = false, bool showMore = false, int pageIndex = 1)
        {
            IUser user = userService.GetUser(spaceKey);
            bool ignoreAuditAndPricy = false;
            PagingDataSet<Photo> photos = new PagingDataSet<Photo>(new List<Photo>());
            if (user == null)
            {
                return HttpNotFound();
            }
            if (UserContext.CurrentUser != null && user.UserId == UserContext.CurrentUser.UserId)
            {
                ignoreAuditAndPricy = true;
            }
            if (!string.IsNullOrEmpty(tagName))
                tagName = WebUtility.UrlDecode(tagName);
            if (!isFavorite)
            {
                if (!showMore)
                {
                    photos = photoService.GetUserPhotos(TenantTypeIds.Instance().User(), user.UserId, ignoreAuditAndPricy, tagName, null, SortBy_Photo.DateCreated_Desc, 20, pageIndex);
                }
                else
                {
                    if (pageIndex <= maxPageIndex)
                    {
                        photos = photoService.GetUserPhotos(TenantTypeIds.Instance().User(), user.UserId, ignoreAuditAndPricy, tagName, null, SortBy_Photo.DateCreated_Desc, 20, pageIndex);
                    }
                }
            }
            else
            {
                PagingEntityIdCollection EntityIdCollection = attitudeService.GetPageObjectIdsByUserId(user.UserId, pageSize, pageIndex);
                IEnumerable<long> ids = EntityIdCollection.GetPagingEntityIds(pageSize, pageIndex).Select(n => (long)n);
                IEnumerable<Photo> favorites = photoService.GetPhotos(ids);
                photos = new PagingDataSet<Photo>(favorites);
            }
            return View(photos);
        }

        /// <summary>
        /// 相册下拉框
        /// </summary>
        private void GetAlbumList(long albumId)
        {
            IEnumerable<Album> albums = photoService.GetUserAlbums(TenantTypeIds.Instance().User(), UserIdToUserNameDictionary.GetUserId(Url.SpaceKey()), true, SortBy_Album.DisplayOrder, 10000, 1).AsEnumerable<Album>();
            SelectList albumList = new SelectList(albums.Select(n => new { text = n.AlbumName, value = n.AlbumId }), "value", "text", albumId);
            ViewData["albumList"] = albumList;
            ViewData["albumId"] = albumId;
            if (albumId == 0 && albumList.Count() > 0)
            {
                ViewData["albumId"] = albumList.First().Value;
            }
        }

    }
}