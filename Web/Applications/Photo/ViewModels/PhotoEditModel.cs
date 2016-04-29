//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;
using System.ComponentModel.DataAnnotations;
using Spacebuilder.Common;


namespace Spacebuilder.Photo
{
    /// <summary>
    /// PhotoEditModel
    /// </summary>
    public class PhotoEditModel
    {
        /// <summary>
        /// 照片ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 相册ID
        /// </summary>
        public long AlbumId { get; set; }

        /// <summary>
        /// 拥有者Id
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        /// 相对地址
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(TextLengthSettings.TEXT_DESCRIPTION_MAXLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        [WaterMark(Content = "添加描述")]
        [DataType(DataType.Text)]
        public string Description { get; set; }

        public Photo AsPhoto()
        {
            Photo photo = Photo.New();
            PhotoService photoService = new PhotoService();
            //创建
            if (PhotoId == 0)
            {
                Album album = photoService.GetAlbum(this.AlbumId);
                photo.AlbumId = this.AlbumId;
                photo.TenantTypeId = album.TenantTypeId;
                photo.OwnerId = album.OwnerId;
                photo.UserId = album.UserId;
                photo.Author = photo.User != null ? photo.User.DisplayName : album.Author;
                photo.OriginalUrl = string.Empty;
                photo.RelativePath = string.Empty;
                photo.AuditStatus = AuditStatus.Pending;
                photo.PrivacyStatus = album.PrivacyStatus;
                photo.IsEssential = false;
            }
            else
            {
                photo = photoService.GetPhoto(this.PhotoId);
            }
            photo.Description = Formatter.FormatMultiLinePlainTextForStorage(this.Description, false) ?? string.Empty;
            return photo;
        }
    }

    /// <summary>
    /// PhotoEditModelExtense
    /// </summary>
    public static class PhotoEditModelExtense
    {
        public static PhotoEditModel AsPhotoEditModel(this Photo photo)
        {
            PhotoEditModel photoEditModel = new PhotoEditModel();
            photoEditModel.PhotoId = photo.PhotoId;
            photoEditModel.AlbumId = photo.AlbumId;
            photoEditModel.RelativePath = photo.RelativePath;
            photoEditModel.Description = Formatter.FormatMultiLinePlainTextForEdit(photo.Description,false);
            return photoEditModel;
        }
    }
}