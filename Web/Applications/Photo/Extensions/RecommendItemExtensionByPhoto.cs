//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 推荐项类型扩展类
    /// </summary>
    public static class RecommendItemExtensionByPhoto
    {
        /// <summary>
        /// 获取照片
        /// </summary>
        public static Photo GetPhoto(this RecommendItem item)
        {
            return new PhotoService().GetPhoto(item.ItemId);
        }

        /// <summary>
        /// 获取相册
        /// </summary>
        public static Album GetAlbum(this RecommendItem item)
        {
            return new PhotoService().GetAlbum(item.ItemId);
        }

        /// <summary>
        /// 获取标签
        /// </summary>
        public static Tag GetTag(this RecommendItem item)
        {
            TagService tagService = new TagService (TenantTypeIds.Instance().Photo());
            return tagService.Get(item.ItemId);
        }
    }
}
