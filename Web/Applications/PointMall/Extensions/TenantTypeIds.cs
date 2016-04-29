//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城定义的租户类型
    /// </summary>
    public static class TenantTypeIdsExtension
    {

        /// <summary>
        /// 积分商城应用
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string PointMallApplication(this TenantTypeIds TenantTypeIds)
        {
            return "200100";
        }

        /// <summary>
        /// 商品
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string PointGift(this TenantTypeIds TenantTypeIds)
        {
            return "200101";
        }

        /// <summary>
        /// 兑换申请
        /// </summary>
        /// <param name="TenantTypeIds"></param>
        /// <returns></returns>
        public static string PointGiftExchangeRecord(this TenantTypeIds TenantTypeIds)
        {
            return "200202";
        }
    }
}