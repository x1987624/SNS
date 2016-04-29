//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// 用户计数类型扩展类
    /// </summary>
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 投稿数
        /// </summary>
        public static string ContributeCount(this OwnerDataKeys ownerDataKeys)
        {
            return "CMS-ContributeCount";
        }
    }
}