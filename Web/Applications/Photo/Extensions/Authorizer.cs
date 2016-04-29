//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using System;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册权限验证
    /// </summary>
    public static class AuthorizerExtension
    {
        /// <summary>
        /// 创建相册
        /// </summary>
        /// <remarks>
        /// 登录用户可以创建相册
        /// </remarks>
        public static bool Album_Create(this Authorizer authorizer)
        {
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 编辑相册/删除相册/设置相册封面
        /// </summary>
        /// <remarks>
        /// 相册主人或管理员可以操作
        /// </remarks>
        public static bool Album_Edit(this Authorizer authorizer, Album album)
        {
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return false;
            }

            if (album.UserId == currentUser.UserId || authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 查看相册
        /// 仅自己可见的只能是相册作者或管理员可以查看
        /// 部分可见的只能是相册作者、指定可见的用户或管理员可以查看
        /// </summary>
        public static bool Album_View(this Authorizer authorizer, Album album)
        {
            if (album == null)
            {
                return false;
            }
            if (album.PrivacyStatus == PrivacyStatus.Public)
            {
                return true;
            }

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                return false;
            }

            if (album.UserId == currentUser.UserId || authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
            {
                return true;
            }

            if (album.PrivacyStatus == PrivacyStatus.Private)
            {
                return false;
            }

            ContentPrivacyService contentPrivacyService = new ContentPrivacyService();
            if (contentPrivacyService.Validate(album, currentUser.UserId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 查看搜索
        /// 仅自己可见的只能是相册作者或管理员可以查看
        /// 部分可见的只能是相册作者、指定可见的用户或管理员可以查看
        /// </summary>
        public static bool Photo_Search(this Authorizer authorizer, Album album)
        {
            if (album == null)
            {
                return false;
            }
            if (album.PrivacyStatus == PrivacyStatus.Public)
            {
                return true;
            }

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null)
            {
                if (album.UserId == currentUser.UserId || authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
                {
                    return true;
                }
            }

            if (album.PrivacyStatus == PrivacyStatus.Private)
            {
                return false;
            }

            ContentPrivacyService contentPrivacyService = new ContentPrivacyService();
            if (currentUser != null)
            {
                if (contentPrivacyService.Validate(album, currentUser.UserId))
                {
                    return true;
                }
            }
            if (currentUser == null && album.PrivacyStatus == PrivacyStatus.Part)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// 相册频道
        /// 只显示公开的
        /// </summary>
        public static bool Photo_Channel(this Authorizer authorizer, Album album)
        {

            if (album.PrivacyStatus == PrivacyStatus.Public)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 上传照片
        /// </summary>
        /// <param name="authorizer"></param>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        public static bool Photo_Upload(this Authorizer authorizer, string spaceKey)
        {
            string errorMessage = string.Empty;
            return authorizer.Photo_Upload(spaceKey, out errorMessage);
        }

        /// <summary>
        /// 上传照片
        /// </summary>
        /// <param name="authorizer"></param>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        public static bool Photo_Upload(this Authorizer authorizer, string spaceKey, out string errorMessage)
        {
            IUser currentUser = UserContext.CurrentUser;
            errorMessage = "没有权限上传照片";

            if (currentUser == null)
            {
                errorMessage = "您必须先登录，才能上传照片";
                return false;
            }

            if (authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
                return true;

            if (currentUser.UserName.Equals(spaceKey, StringComparison.CurrentCultureIgnoreCase) && authorizer.AuthorizationService.Check(currentUser, PermissionItemKeys.Instance().Photo_Create()))
                return true;
            if (currentUser.IsModerated)
                errorMessage = Resources.Resource.Description_ModeratedUser_CreatePhotoDenied;
            return false;
        }

        /// <summary>
        /// 上传照片/照片采集(暂时用于移动端)
        /// </summary>
        /// <remarks>
        /// 相册主人或管理员可以上传指定相册的图片
        /// </remarks>
        public static bool Photo_Create(this Authorizer authorizer, Album album, IUser currentUser, out string errorMessage)
        {
            //IUser currentUser = UserContext.CurrentUser;
            errorMessage = "没有权限上传照片";

            if (currentUser == null)
            {
                errorMessage = "您必须先登录，才能上传照片";
                return false;
            }

            if (authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
                return true;

            if (album.UserId == currentUser.UserId && authorizer.AuthorizationService.Check(currentUser, PermissionItemKeys.Instance().Photo_Create()))
                return true;

            if (currentUser.IsModerated)
                errorMessage = Resources.Resource.Description_ModeratedUser_CreatePhotoDenied;
            return false;
        }

        /// <summary>
        /// 上传照片
        /// </summary>
        /// <param name="authorizer"></param>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        public static bool Photo_Create(this Authorizer authorizer, Album album)
        {
            string errorMessage = string.Empty;
            return authorizer.Photo_Create(album, out errorMessage);
        }



        /// <summary>
        /// 上传照片/照片采集
        /// </summary>
        /// <remarks>
        /// 相册主人或管理员可以上传指定相册的图片
        /// </remarks>
        public static bool Photo_Create(this Authorizer authorizer, Album album, out string errorMessage)
        {
            IUser currentUser = UserContext.CurrentUser;
            errorMessage = "没有权限上传照片";

            if (currentUser == null)
            {
                errorMessage = "您必须先登录，才能上传照片";
                return false;
            }

            if (authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
                return true;

            if (album.UserId == currentUser.UserId && authorizer.AuthorizationService.Check(currentUser, PermissionItemKeys.Instance().Photo_Create()))
                return true;

            if (currentUser.IsModerated)
                errorMessage = Resources.Resource.Description_ModeratedUser_CreatePhotoDenied;
            return false;
        }

        /// <summary>
        /// 编辑照片/移动照片/删除照片/贴标签
        /// </summary>
        /// <remarks>
        /// 照片主人（即所属相册主人）或管理员可以操作
        /// </remarks>
        public static bool Photo_Edit(this Authorizer authorizer, Photo photo)
        {
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return false;
            }

            if (photo.UserId == currentUser.UserId || authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 查看照片
        /// </summary>
        /// <remarks>
        /// 符合照片隐私设置（所属相册的隐私设置）的人或管理员可以查看照片
        /// </remarks>
        public static bool Photo_View(this Authorizer authorizer, Photo photo)
        {
            PhotoService photoService = new PhotoService();
            Album album = photoService.GetAlbum(photo.AlbumId);
            if (album == null)
            {
                return false;
            }
            if (album.PrivacyStatus == PrivacyStatus.Public)
            {
                return true;
            }

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                return false;
            }

            if (album.UserId == currentUser.UserId || authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
            {
                return true;
            }

            if (album.PrivacyStatus == PrivacyStatus.Private)
            {
                return false;
            }

            ContentPrivacyService contentPrivacyService = new ContentPrivacyService();
            if (contentPrivacyService.Validate(album, currentUser.UserId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 照片加精/取消精华/照片推荐/相册推荐
        /// </summary>
        /// <remarks>
        /// 管理员可以操作
        /// </remarks>
        public static bool Photo_Manage(this Authorizer authorizer)
        {
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return false;
            }

            if (authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 是否有圈人的权限
        /// </summary>
        /// <param name="authorizer"></param>
        /// <param name="photo">被圈的照片</param>
        /// <returns></returns>
        public static bool PhotoLabel_Creat(this Authorizer authorizer, Photo photo)
        {
            if (photo == null)
                return false;

            if (UserContext.CurrentUser == null)
                return false;

            if (photo.UserId == UserContext.CurrentUser.UserId)
                return true;

            if (new FollowService().IsMutualFollowed(photo.UserId, UserContext.CurrentUser.UserId))
                return true;

            return authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId);
        }

        /// <summary>
        /// 删除圈人
        /// </summary>
        /// <param name="authorizer"></param>
        /// <param name="label">圈人信息</param>
        /// <returns></returns>
        public static bool PhotoLabel_Delete(this Authorizer authorizer, PhotoLabel label)
        {
            if (label == null)
                return false;

            if (UserContext.CurrentUser == null)
                return false;

            if (label.UserId == UserContext.CurrentUser.UserId)
                return true;

            if (label.Photo.UserId == UserContext.CurrentUser.UserId)
                return true;

            return authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId);
        }
    }
}