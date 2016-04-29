//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;
using Spacebuilder.Common;
using System;
using Tunynet;

namespace Spacebuilder.Photo
{
    [Serializable]
    public class PhotoApplication : ApplicationBase
    {
        protected PhotoApplication(ApplicationModel model)
            : base(model)
        { }

        /// <summary>
        /// 删除用户在应用中的数据
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="takeOverUserName">用于接管删除用户时不能删除的内容(例如：用户创建的群组)</param>
        /// <param name="isTakeOver">是否接管被删除用户可被接管的内容</param>
        protected override void DeleteUser(long userId, string takeOverUserName, bool isTakeOver)
        {
            PhotoService photoService = new PhotoService();
            photoService.DeleteUser(userId, takeOverUserName, isTakeOver);
        }

        protected override bool Install(string presentAreaKey, long ownerId)
        {
            PhotoService photoService = new PhotoService();
            string author = DIContainer.Resolve<UserService>().GetFullUser(ownerId).DisplayName;
            Album album = Album.New();
            album.OwnerId = ownerId;
            album.UserId = ownerId;
            album.Author = author;
            album.AlbumName = "默认相册";
            album.Description = "默认相册";
            album.AuditStatus = AuditStatus.Success;
            album.PrivacyStatus = PrivacyStatus.Public;
            album.TenantTypeId = TenantTypeIds.Instance().User();
            album.LastUploadDate = DateTime.UtcNow;
            photoService.CreateAlbum(album);
            return true;
        }

        protected override bool UnInstall(string presentAreaKey, long ownerId)
        {
            PhotoService photoService = new PhotoService();
            photoService.DeleteUser(ownerId, string.Empty, false);
            return true;
        }
    }
}