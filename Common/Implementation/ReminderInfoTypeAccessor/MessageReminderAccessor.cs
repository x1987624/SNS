﻿//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Tunynet.Common;
using Tunynet.Utilities;
using Tunynet;

namespace Spacebuilder.Common
{
    /// <summary>
    /// 私信提醒信息查询类
    /// </summary>
    public class MessageReminderAccessor : IReminderInfoAccessor
    {
        /// <summary>
        /// 提醒信息类型Id
        /// </summary>
        public int ReminderInfoTypeId
        {
            get { return ReminderInfoTypeIds.Instance().Message(); }
        }

        /// <summary>
        /// 获取所有用户提醒信息集合
        /// </summary>
        /// <returns>用户提醒信息集合</returns>
        public IEnumerable<UserReminderInfo> GetUserReminderInfos()
        {
            MessageService messageService = new MessageService();
            return messageService.GetUserReminderInfos();
        }

        /// <summary>
        /// 处理地址
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetProcessUrl(long userId)
        {   
            return SiteUrls.FullUrl(SiteUrls.Instance().ListMessageSessions(UserIdToUserNameDictionary.GetUserName(userId), null));            
        }
    }
}
