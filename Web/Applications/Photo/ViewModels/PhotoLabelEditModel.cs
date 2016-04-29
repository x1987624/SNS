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

using Tunynet;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// PhotoEditModel
    /// </summary>
    public class PhotoLabelEditModel
    {
        #region 需持久化属性

        /// <summary>
        ///圈人Id
        /// </summary>
        public long LabelId { get; set; }

        /// <summary>
        ///照片ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        ///租户类型ID
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///被圈人（对象）ID
        /// </summary>
        public long ObjetId { get; set; }

        /// <summary>
        ///被圈人（对象）名称
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///描述
        /// </summary>
        [DataType(DataType.Text)]
        public string Description { get; set; }

        /// <summary>
        ///被圈区域X坐标
        /// </summary>
        public int AreaX { get; set; }

        /// <summary>
        ///被圈区域Y坐标
        /// </summary>
        public int AreaY { get; set; }

        /// <summary>
        ///被圈区域宽度
        /// </summary>
        public int AreaWidth { get; set; }

        /// <summary>
        ///被圈区域高度
        /// </summary>
        public int AreaHeight { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///修改时间
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// 是否能够编辑
        /// </summary>
        public Boolean CanEdit { get; set; }

        /// <summary>
        /// 用户的连接
        /// </summary>
        public string UserLink { get; set; }

        #endregion

        /// <summary>
        /// 转换为数据存储实体
        /// </summary>
        /// <returns></returns>
        public PhotoLabel AsPhotoLabel()
        {
            return new PhotoLabel
            {
                AreaHeight = this.AreaHeight,
                AreaWidth = this.AreaWidth,
                AreaX = this.AreaX,
                AreaY = this.AreaY,
                Description = this.Description,
                ObjectName = this.ObjectName,
                ObjetId = this.ObjetId,
                PhotoId = this.PhotoId,
                TenantTypeId = this.TenantTypeId ?? TenantTypeIds.Instance().User(),
                UserId = UserContext.CurrentUser.UserId
            };
        }

    }

    /// <summary>
    /// 圈人编辑
    /// </summary>
    public static class PhotoLabelEditModelExtense
    {
        //转换为可以编辑的实体
        public static PhotoLabelEditModel AsPhotoEditModel(this PhotoLabel label)
        {
            return new PhotoLabelEditModel
            {
                AreaHeight = label.AreaHeight,
                AreaWidth = label.AreaWidth,
                AreaX = label.AreaX,
                AreaY = label.AreaY,
                DateCreated = label.DateCreated,
                Description = label.Description,
                LabelId = label.LabelId,
                LastModified = label.LastModified,
                ObjectName = label.ObjectName,
                ObjetId = label.ObjetId,
                PhotoId = label.PhotoId,
                TenantTypeId = label.TenantTypeId,
                UserId = label.UserId,
                CanEdit = DIContainer.Resolve<Authorizer>().PhotoLabel_Delete(label),
                UserLink = SiteUrls.Instance().SpaceHome(label.ObjetId)
            };
        }
    }
}