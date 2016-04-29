//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 问答定义的租户类型
    /// </summary>
    public static class TenantTypeIdsExtension
    {
        /// <summary>
        /// 百科应用
        /// </summary>
        public static string Wiki(this TenantTypeIds TenantTypeIds)
        {
            return "101600";
        }

        /// <summary>
        /// 词条
        /// </summary>
        public static string WikiPage(this TenantTypeIds TenantTypeIds)
        {
            return "101601";
        }

        /// <summary>
        /// 词条版本
        /// </summary>
        public static string WikiPageVersion(this TenantTypeIds TenantTypeIds)
        {
            return "101602";
        }
    }
}