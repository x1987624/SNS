//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// 资讯定义的租户类型
    /// </summary>
    public static class TenantTypeIdsExtension
    {
        /// <summary>
        /// 资讯应用
        /// </summary>
        public static string CMS(this TenantTypeIds TenantTypeIds)
        {
            return "101500";
        }

        /// <summary>
        /// 资讯
        /// </summary>
        public static string ContentItem(this TenantTypeIds TenantTypeIds)
        {
            return "101501";
        }

        /// <summary>
        /// 资讯附件
        /// </summary>
        public static string ContentAttachment(this TenantTypeIds TenantTypeIds)
        {
            return "101502";
        }
    }
}