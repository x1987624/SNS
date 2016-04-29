//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 关注问题动态接收人获取器
    /// </summary>
    public class SubscribeQuestionActivityReceiverGetter : IActivityReceiverGetter
    {
        private SubscribeService subscribeService = new SubscribeService(TenantTypeIds.Instance().AskQuestion());
        private FollowService followService = new FollowService();
        private bool isUserReceived = true;

        /// <summary>
        /// 获取接收人UserId集合
        /// </summary>
        /// <param name="activityService">动态业务逻辑类</param>
        /// <param name="activity">动态对象</param>
        /// <returns></returns>
        IEnumerable<long> IActivityReceiverGetter.GetReceiverUserIds(ActivityService activityService, Activity activity)
        {
            List<long> followerUserIds = subscribeService.GetUserIdsOfObject(activity.OwnerId).ToList();
            if (followerUserIds == null)
            {
                followerUserIds= new List<long>();
            }

            //将动态发送者（回答的作者）从动态的接收对象中移除（否则如果动态发送者也关注了该问题，就会产生重复的动态数据）
            if (followerUserIds.Contains(activity.UserId))
            {
                followerUserIds.Remove(activity.UserId);
            }

            //如果用户没有设置从默认设置获取
            ActivityItem activityItem = activityService.GetActivityItem(activity.ActivityItemKey);
            if (activityItem != null)
            {
                isUserReceived = activityItem.IsUserReceived;
            }

            return followerUserIds.Where(n => IsReceiveActivity(activityService, n, activity));
        }

        /// <summary>
        /// 检查用户是否接收动态
        /// </summary>
        /// <param name="activityService"></param>
        /// <param name="userId">UserId</param>
        /// <param name="activity">动态</param>
        /// <returns>接收动态返回true，否则返回false</returns>
        private bool IsReceiveActivity(ActivityService activityService, long userId, Activity activity)
        {
            //检查用户是否已在信息发布者的粉丝圈里面
            if (followService.IsFollowed(userId, activity.UserId))
            {
                return false;
            }

            //检查用户是否接收该动态项目
            Dictionary<string, bool> userSettings = activityService.GetActivityItemUserSettings(userId);
            if (userSettings.ContainsKey(activity.ActivityItemKey))
            {
                return userSettings[activity.ActivityItemKey];
            }
            else
            {
                return isUserReceived;
            }
        }

    }
}
