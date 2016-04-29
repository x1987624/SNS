//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 日志推荐Url获取器
    /// </summary>
    public class PointMallRecommendUrlGetter : IRecommendUrlGetter
    {
        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().PointGift(); }
        }

        /// <summary>
        /// 详细页面地址
        /// </summary>
        /// <param name="itemId">推荐内容Id</param>
        /// <returns></returns>
        public string RecommendItemDetail(long itemId)
        {
            PointGift pointGift = new PointMallService().GetGift(itemId);
            if (pointGift == null)
                return string.Empty;
            return SiteUrls.Instance().GiftDetail(itemId);
        }
    }
}