//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using System;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科权限验证
    /// </summary>
    public static class AuthorizerExtension
    {
        /// <summary>
        /// 创建词条
        /// </summary>
        public static bool Page_Create(this Authorizer authorizer)
        {
            string errorMessage = string.Empty;
            return authorizer.Page_Create(out errorMessage);
        }

        /// <summary>
        /// 创建词条
        /// </summary>
        /// <remarks>
        /// 登录用户可以创建词条
        /// </remarks>
        public static bool Page_Create(this Authorizer authorizer, out string errorMessage)
        {
            errorMessage = "没有权限创建词条";
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                errorMessage = "您需要先登录，才能创建词条";
                return false;
            }
            bool result = authorizer.AuthorizationService.Check(currentUser, PermissionItemKeys.Instance().WikiPage_Create());
            if (!result && currentUser.IsModerated)
                errorMessage = Resources.Resource.Description_ModeratedUser_CreatePageDenied;
            return result;
        }

        /// <summary>
        /// 删除词条
        /// </summary>
        /// <remarks>
        /// 管理员可以删除词条
        /// </remarks>
        public static bool Page_Delete(this Authorizer authorizer, WikiPage page)
        {
            if (authorizer.IsAdministrator(WikiConfig.Instance().ApplicationId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 管理词条的权限
        /// </summary>
        public static bool Page_Manage(this Authorizer authorizer, WikiPage page)
        {
            if (authorizer.IsAdministrator(WikiConfig.Instance().ApplicationId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 编辑词条
        /// </summary>
        public static bool WikiPageVersion_Create(this Authorizer authorizer, WikiPage page, out string errorMessage)
        {
            errorMessage = "没有权限编辑词条";
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
            {
                errorMessage = "您需要先登录，才能编辑词条";
                return false;
            }
            bool result = authorizer.AuthorizationService.Check(currentUser, PermissionItemKeys.Instance().WikiPageVersion_Create());
            if (!result && currentUser.IsModerated)
                errorMessage = Resources.Resource.Description_ModeratedUser_CreatePageVersionDenied;
            return result;
        }
    }
}