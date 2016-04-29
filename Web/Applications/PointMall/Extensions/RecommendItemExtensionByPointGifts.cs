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

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 推荐项类型扩展类
    /// </summary>
    public static class RecommendItemExtensionByPointGifts
    {
        /// <summary>
        /// 获取商品
        /// </summary>
        public static PointGift GetPointGifts(this RecommendItem item)
        {
            return new PointMallService().GetGift(item.ItemId);
        }
    }
}
