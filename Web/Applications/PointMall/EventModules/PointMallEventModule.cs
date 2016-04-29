//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Tunynet.Common;
using Tunynet.Events;
using Tunynet.Globalization;
using Spacebuilder.Common;
using Tunynet.Utilities;
using Spacebuilder.PointMall.Resources;

namespace Spacebuilder.PointMall.EventModules
{
    /// <summary>
    /// 处理商城动态、通知的EventMoudle
    /// </summary>
    public class PointMallEventModule : IEventMoudle
    {
        private ActivityService activityService = new ActivityService();
        private AuditService auditService = new AuditService();
        private NoticeService noticeService = new NoticeService();
        private PointMallService pointMallService = new PointMallService();
        private PointService pointService = new PointService();

        /// <summary>
        /// 注册事件处理程序
        /// </summary>
        void IEventMoudle.RegisterEventHandler()
        {
            //兑换商品
            EventBus<PointGiftExchangeRecord>.Instance().After += new CommonEventHandler<PointGiftExchangeRecord, CommonEventArgs>(RecordAcitivityEventModule_After);
        }

        /// <summary>
        /// 兑换商品时处理积分，动态
        /// </summary>
        /// <param name="record">兑换申请</param>
        /// <param name="eventArgs"></param>
        public void RecordAcitivityEventModule_After(PointGiftExchangeRecord record, CommonEventArgs eventArgs)
        {
            //生成动态
            ActivityService activityService = new ActivityService();

            if (eventArgs.EventOperationType == EventOperationType.Instance().Approved())
            {
                //初始化Owner为用户的动态
                Activity activity = Activity.New();
                activity.ActivityItemKey = ActivityItemKeys.Instance().ExchangeGift();
                activity.ApplicationId = PointMallConfig.Instance().ApplicationId;
                activity.HasImage = true;
                activity.HasMusic = false;
                activity.HasVideo = false;
                activity.IsOriginalThread = true;
                activity.IsPrivate = false;
                activity.UserId = record.PayerUserId;
                activity.ReferenceId = record.GiftId;
                activity.ReferenceTenantTypeId = TenantTypeIds.Instance().PointGift();
                activity.SourceId = record.RecordId;
                activity.TenantTypeId = TenantTypeIds.Instance().PointGiftExchangeRecord();
                activity.OwnerId = record.PayerUserId;
                activity.OwnerName = record.Payer;
                activity.OwnerType = ActivityOwnerTypes.Instance().User();  
                
                activityService.Generate(activity, true);

                //通知
                Notice notice = Notice.New();
                notice.UserId = record.PayerUserId;
                notice.ApplicationId = PointMallConfig.Instance().ApplicationId;
                notice.TypeId = NoticeTypeIds.Instance().Hint();
                notice.LeadingActor = record.Payer;
                notice.LeadingActorUrl = SiteUrls.FullUrl(SiteUrls.Instance().SpaceHome(record.PayerUserId));
                notice.RelativeObjectName = record.GiftName;
                notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().GiftDetail(record.GiftId));
                notice.TemplateName = NoticeTemplateNames.Instance().ApplyRecord();
                noticeService.Create(notice);
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Disapproved())
            {
                //通知
                Notice notice = Notice.New();
                notice.UserId = record.PayerUserId;
                notice.ApplicationId = PointMallConfig.Instance().ApplicationId;
                notice.TypeId = NoticeTypeIds.Instance().Hint();
                notice.LeadingActor = record.Payer;
                notice.LeadingActorUrl = SiteUrls.FullUrl(SiteUrls.Instance().SpaceHome(record.PayerUserId));
                notice.RelativeObjectName = record.GiftName;
                notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().GiftDetail(record.GiftId));
                notice.TemplateName = NoticeTemplateNames.Instance().CancelRecord();
                noticeService.Create(notice);
            }
        }
    }
}