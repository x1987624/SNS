//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城应用动态项
    /// </summary>
    public static class ActivityItemKeysExtension
    {
        /// <summary>
        /// 兑换商品
        /// </summary>
        public static string ExchangeGift(this ActivityItemKeys activityItemKeys)
        {
            return "ExchangeGift";
        }      
    }
}
