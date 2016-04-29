//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Caching;
using Spacebuilder.CMS;
using Tunynet.Common;
using Tunynet;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// 栏目管理员业务逻辑
    /// </summary>
    public class ContentFolderModeratorService
    {
        private ContentFolderModeratorRepository contentFolderModeratorRepository;

        /// <summary>
        /// 构造器
        /// </summary>
        public ContentFolderModeratorService()
        {
            contentFolderModeratorRepository = new ContentFolderModeratorRepository();
        }

        /// <summary>
        /// 基于用户设置栏目管理员
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="contentFolderIds"></param>
        public void SetModeratorBaseUser(long userId, IEnumerable<int> contentFolderIds)
        {
            IUserService userService = DIContainer.Resolve<IUserService>();
            IUser user = userService.GetUser(userId);
            if (user != null)
                contentFolderModeratorRepository.SetModeratorByUser(userId, contentFolderIds);
        }

        /// <summary>
        /// 基于栏目设置栏目管理员
        /// </summary>
        /// <param name="contentFolderId"></param>
        /// <param name="userIds"></param>
        public void SetModeratorBaseFolder(int contentFolderId, IEnumerable<long> userIds)
        {
            contentFolderModeratorRepository.SetModeratorByFolder(contentFolderId, userIds);
        }

        /// <summary>
        /// 用户管理的栏目Id集合
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<int> GetModeratedFolderIds(long userId)
        {
            return contentFolderModeratorRepository.GetModeratedFolderIds(userId);
        }

        /// <summary>
        /// 获取栏目的管理员
        /// </summary>
        /// <param name="contentFolderId"></param>
        /// <returns></returns>
        public IEnumerable<IUser> GetModerators(int contentFolderId)
        {
            return contentFolderModeratorRepository.GetModerators(contentFolderId);
        }

    }
}
