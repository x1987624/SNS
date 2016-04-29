//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 权限项标识扩展类
    /// </summary>
    public static class PermissionItemKeysExtension
    {
        /// <summary>
        /// 创建词条
        /// </summary>
        /// <param name="pik"><see cref="PermissionItemKeys"/></param>
        /// <returns></returns>
        public static string WikiPage_Create(this PermissionItemKeys pik)
        {
            return "WikiPage_Create";
        }

        /// <summary>
        /// 编辑词条
        /// </summary>
        /// <param name="pik"><see cref="PermissionItemKeys"/></param>
        /// <returns></returns>
        public static string WikiPageVersion_Create(this PermissionItemKeys pik)
        {
            return "WikiPageVersion_Create";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pik"></param>
        /// <returns></returns>
        public static string WikiManageApply(this PermissionItemKeys pik)
        {
            return "WikiManageApply";
        }

        
    }
}