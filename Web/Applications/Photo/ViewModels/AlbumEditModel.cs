//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet.Common;

using System.Collections;
using System.Collections.Generic;
using Spacebuilder.Search;
using System.Text;
using Tunynet.Utilities;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 编辑相册的EditModel
    /// </summary>
    public class AlbumEditModel
    {
        /// <summary>
        ///相册ID
        /// </summary>
        public long AlbumId { get; set; }

        /// <summary>
        ///相册名称
        /// </summary>
        [WaterMark(Content = "在此输入相册名称")]
        [Required(ErrorMessage = "请输入相册名称")]
        [StringLength(TextLengthSettings.TEXT_SUBJECT_MAXLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        [DataType(DataType.Text)]
        public string AlbumName { get; set; }

        /// <summary>
        ///所有者ID，用户的相册，OwnerId=UserId
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        ///租户类型
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///相册作者ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///作者名称
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///相册描述
        /// </summary>
        [WaterMark(Content = "在此输入相册描述")]
        [StringLength(TextLengthSettings.TEXT_DESCRIPTION_MAXLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        [DataType(DataType.Text)]
        public string Description { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///修改时间
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        ///隐私状态
        /// </summary>
        public PrivacyStatus PrivacyStatus { get; set; }

        /// <summary>
        /// 隐私设置用户列表
        /// </summary>
        public string PrivacyStatus1 { get; set; }

        /// <summary>
        /// 隐私设置分组列表
        /// </summary>
        public string PrivacyStatus2 { get; set; }

        /// <summary>
        /// 转换为相册实体
        /// </summary>
        /// <returns></returns>
        public Album AsAlbum()
        {
            Album album = null;
            IUser currentUser = UserContext.CurrentUser;
            PhotoService photoService = new PhotoService();
            if (AlbumId == 0)
            {
                album = Album.New();
                album.DateCreated = DateTime.UtcNow;
                album.TenantTypeId = TenantTypeIds.Instance().User();
                album.UserId = currentUser == null ? 0 : currentUser.UserId;
                album.OwnerId = currentUser == null ? 0 : currentUser.UserId;
                album.Author = currentUser == null ? string.Empty : currentUser.DisplayName;
                album.CoverId = 0;
                album.LastUploadDate = DateTime.UtcNow;
            }
            else
            {
                album = photoService.GetAlbum(this.AlbumId);
            }
            album.LastModified = DateTime.UtcNow;
            album.Description = Description ?? string.Empty;
            album.AlbumName = AlbumName;
            album.PrivacyStatus = PrivacyStatus;
            return album;
        }
    }
    /// <summary>
    /// 相册实体的扩展类
    /// </summary>
    public static class AlbumExtensions
    {
        /// <summary>
        /// 将数据库中的信息转换成EditModel实体
        /// </summary>
        /// <param name="album">日志实体</param>
        /// <returns>编辑相册的EditModel</returns>
        public static AlbumEditModel AsEditModel(this Album album)
        {
            return new AlbumEditModel
            {
                AlbumId = album.AlbumId,
                AlbumName = album.AlbumName,
                OwnerId = album.OwnerId,
                UserId = album.UserId,
                Author = album.Author,
                Description = album.Description,
                DateCreated = album.DateCreated,
                LastModified = album.LastModified,
                PrivacyStatus = album.PrivacyStatus,
                TenantTypeId = album.TenantTypeId
            };
        }
    }
}