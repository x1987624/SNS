//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答定义的租户类型
    /// </summary>
    public static class TenantTypeIdsExtension
    {

        /// <summary>
        /// 问答应用
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string Ask(this TenantTypeIds TenantTypeIds)
        {
            return "101300";
        }

        /// <summary>
        /// 问题
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string AskQuestion(this TenantTypeIds TenantTypeIds)
        {
            return "101301";
        }

        /// <summary>
        /// 回答
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string AskAnswer(this TenantTypeIds TenantTypeIds)
        {
            return "101302";
        }

        /// <summary>
        /// 关注问题标签（订阅服务）
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string AskTag(this TenantTypeIds TenantTypeIds)
        {
            return "101303";
        }
    }
}