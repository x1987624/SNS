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
using Spacebuilder.Photo.Resources;
using Tunynet.Common.Repositories;
using Tunynet;

namespace Spacebuilder.Photo.EventModules
{
    /// <summary>
    /// 处理相册动态、积分、通知的EventMoudle
    /// </summary>
    public class PhotoEventModule : IEventMoudle
    {
        private ActivityService activityService = new ActivityService();
        private AuditService auditService = new AuditService();
        private NoticeService noticeService = new NoticeService();
        private PhotoService photoService = new PhotoService();
        private PointService pointService = new PointService();

        /// <summary>
        /// 注册事件处理程序
        /// </summary>
        void IEventMoudle.RegisterEventHandler()
        {
            //照片圈人积分
            EventBus<PhotoLabel>.Instance().After += new CommonEventHandler<PhotoLabel, CommonEventArgs>(PhotoLabelEventModule_After);
            //照片圈人通知
            EventBus<PhotoLabel>.Instance().After += new CommonEventHandler<PhotoLabel, CommonEventArgs>(PhotoLabelNotice_After);
            //照片圈人通知
            EventBus<PhotoLabel>.Instance().After += new CommonEventHandler<PhotoLabel, CommonEventArgs>(PhotoLabeledNotice_After);
            //照片积分
            EventBus<Photo, AuditEventArgs>.Instance().After += new CommonEventHandler<Photo, AuditEventArgs>(PhotoPointModule_After);
            //照片动态
            EventBus<Album, AuditEventArgs>.Instance().After += new CommonEventHandler<Album, AuditEventArgs>(PhotoActivityModule_After);
            //圈人动态
            EventBus<PhotoLabel>.Instance().After += new CommonEventHandler<PhotoLabel, CommonEventArgs>(LabelPhotoActivityModule_After);
            //照片隐私
            EventBus<Album>.Instance().After += new CommonEventHandler<Album, CommonEventArgs>(PhotoAcitivityPrivicyChangeEventModule_After);
            //照片加精
            EventBus<Photo>.Instance().After += new CommonEventHandler<Photo, CommonEventArgs>(PhotoPointModuleForManagerOperation_After);
            //照片顶踩
            EventBus<long, SupportOpposeEventArgs>.Instance().After += new CommonEventHandler<long, SupportOpposeEventArgs>(PhotoSupportEventModule_After);

            //EventBus<long, SupportOpposeEventArgs>.Instance().After += new CommonEventHandler<long, SupportOpposeEventArgs>(PhotoPointSupportModule_After);
            //评论通知、动态
            //EventBus<Comment, AuditEventArgs>.Instance().After += new CommonEventHandler<Comment, AuditEventArgs>(CommentEventModule_After);
            EventBus<Comment, AuditEventArgs>.Instance().After += new CommonEventHandler<Comment, AuditEventArgs>(CommentActivityEventModule_After);
        }

        /// <summary>
        /// 照片圈人积分
        /// </summary>
        public void PhotoLabelEventModule_After(PhotoLabel photoLabel, CommonEventArgs eventArgs)
        {
            string pointItemKey = string.Empty;
            string eventOperationType = string.Empty;
            //加积分
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {
                pointItemKey = PointItemKeys.Instance().Photo_BeLabelled();
                eventOperationType = EventOperationType.Instance().Create();
            }
            //减积分
            if (eventArgs.EventOperationType == EventOperationType.Instance().Delete())
            {
                pointItemKey = PointItemKeys.Instance().Photo_BeLabelled_Delete();
                eventOperationType = EventOperationType.Instance().Delete();
            }

            if (!string.IsNullOrEmpty(pointItemKey))
            {
                string description = string.Format("照片圈人", "", HtmlUtility.TrimHtml(photoLabel.Description, 64));
                pointService.GenerateByRole(photoLabel.ObjetId, pointItemKey, description);
            }
        }

        /// <summary>
        /// 照片圈人通知
        /// </summary>
        /// <param name="photoLabel"></param>
        /// <param name="eventArgs"></param>
        public void PhotoLabelNotice_After(PhotoLabel photoLabel, CommonEventArgs eventArgs)
        {
            if (photoLabel.Photo == null)
            {
                return;
            }
            //圈人的操作人
            IUser user = DIContainer.Resolve<UserService>().GetUser(photoLabel.UserId);

            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {
                //排除掉照片主人自己
                if (photoLabel.UserId == photoLabel.Photo.UserId)
                {
                    return;
                }
                //通知照片作者
                Notice notice = Notice.New();
                notice.UserId = photoLabel.Photo.UserId;
                notice.ApplicationId = PhotoConfig.Instance().ApplicationId;
                notice.TypeId = NoticeTypeIds.Instance().Hint();
                notice.LeadingActor = user.DisplayName;
                notice.LeadingActorUrl = SiteUrls.FullUrl(SiteUrls.Instance().Home(user.UserName));
                notice.RelativeObjectName = photoLabel.Photo.Description;
                notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().PhotoDetail(photoLabel.Photo.PhotoId));
                notice.TemplateName = NoticeTemplateNames.Instance().PhotoLabelNotice();
                noticeService.Create(notice);
            }
        }

        /// <summary>
        /// 照片圈人通知被圈人
        /// </summary>
        /// <param name="photoLabel"></param>
        /// <param name="eventArgs"></param>
        public void PhotoLabeledNotice_After(PhotoLabel photoLabel, CommonEventArgs eventArgs)
        {
            if (photoLabel.Photo == null)
            {
                return;
            }
            //圈人的操作人
            IUser user = DIContainer.Resolve<UserService>().GetUser(photoLabel.UserId);
            //给被圈的人发送通知
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {
                Notice notice = Notice.New();
                notice.UserId = photoLabel.ObjetId;
                notice.ApplicationId = PhotoConfig.Instance().ApplicationId;
                notice.TypeId = NoticeTypeIds.Instance().Hint();
                notice.LeadingActor = user.DisplayName;
                notice.LeadingActorUrl = SiteUrls.FullUrl(SiteUrls.Instance().Home(user.UserName));
                notice.RelativeObjectName = photoLabel.Photo.Description;
                notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().PhotoDetail(photoLabel.Photo.PhotoId));
                notice.TemplateName = NoticeTemplateNames.Instance().PhotoLabeledNotice();
                noticeService.Create(notice);
            }
        }

        /// <summary>
        /// 照片审核触发的事件
        /// </summary>
        private void PhotoPointModule_After(Photo photo, AuditEventArgs eventArgs)
        {
            AuditService auditService = new AuditService();

            string pointItemKey = string.Empty;
            string eventOperationType = string.Empty;

            bool? auditDirection = auditService.ResolveAuditDirection(eventArgs.OldAuditStatus, eventArgs.NewAuditStatus);

            if (auditDirection == true) //加积分
            {
                pointItemKey = PointItemKeys.Instance().Photo_UploadPhoto();
                if (eventArgs.OldAuditStatus == null)
                    eventOperationType = EventOperationType.Instance().Create();
                else
                    eventOperationType = EventOperationType.Instance().Approved();
            }
            else if (auditDirection == false) //减积分
            {
                pointItemKey = PointItemKeys.Instance().Photo_DeletePhoto();
                if (eventArgs.NewAuditStatus == null)
                    eventOperationType = EventOperationType.Instance().Delete();
                else
                    eventOperationType = EventOperationType.Instance().Disapproved();
            }

            if (!string.IsNullOrEmpty(pointItemKey))
            {
                string description = string.Format(ResourceAccessor.GetString("PointRecord_Pattern_" + eventOperationType), "照片", HtmlUtility.TrimHtml(photo.Description, 64));
                pointService.GenerateByRole(photo.UserId, pointItemKey, description, eventOperationType == EventOperationType.Instance().Create() || eventOperationType == EventOperationType.Instance().Delete() && eventArgs.OperatorInfo.OperatorUserId == photo.UserId);
            }
        }

        /// <summary>
        /// 处理加精操作加积分
        /// </summary>
        private void PhotoPointModuleForManagerOperation_After(Photo photo, CommonEventArgs eventArgs)
        {
            string pointItemKey = string.Empty;
            if (eventArgs.EventOperationType == EventOperationType.Instance().SetEssential())
            {
                pointItemKey = PointItemKeys.Instance().EssentialContent();

                PointService pointService = new PointService();
                string description = string.Format(ResourceAccessor.GetString("PointRecord_Pattern_" + eventArgs.EventOperationType), "照片", HtmlUtility.TrimHtml(photo.Description, 64));
                pointService.GenerateByRole(photo.UserId, pointItemKey, description);
            }
        }


        /// <summary>
        /// 隐私状态发生变化时，同时更新动态的私有状态
        /// </summary>
        private void PhotoAcitivityPrivicyChangeEventModule_After(Album album, CommonEventArgs eventArgs)
        {
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create() ||
            eventArgs.EventOperationType == EventOperationType.Instance().Update())
            {
                ActivityService activityService = new ActivityService();
                Activity activity = activityService.Get(TenantTypeIds.Instance().Photo(), album.AlbumId);
                if (activity == null)
                {
                    return;
                }

                bool newIsPrivate = album.PrivacyStatus == PrivacyStatus.Private ? true : false;

                //是否是公开的（用于是否推送站点动态）
                bool isPublic = album.PrivacyStatus == PrivacyStatus.Public ? true : false;
                if (activity.IsPrivate != newIsPrivate)
                {
                    activityService.UpdatePrivateStatus(activity.ActivityId, newIsPrivate, isPublic);
                }
            }
        }

        /// <summary>
        /// 动态处理程序
        /// </summary>
        private void PhotoActivityModule_After(Album album, AuditEventArgs eventArgs)
        {
            //生成动态
            ActivityService activityService = new ActivityService();
            AuditService auditService = new AuditService();

            bool? auditDirection = auditService.ResolveAuditDirection(eventArgs.OldAuditStatus, eventArgs.NewAuditStatus);

            if (auditDirection == true)
            {
                //如果是新上传了照片，则更新最后修改时间
                if (activityService.UpdateLastModified(TenantTypeIds.Instance().Album(), album.AlbumId))
                {
                    return;
                }
                //初始化Owner为用户的动态
                Activity activity = Activity.New();
                activity.ActivityItemKey = ActivityItemKeys.Instance().CreatePhoto();
                activity.ApplicationId = PhotoConfig.Instance().ApplicationId;
                activity.HasImage = true;
                activity.HasMusic = false;
                activity.HasVideo = false;
                activity.IsOriginalThread = true; 
                activity.IsPrivate = album.PrivacyStatus == PrivacyStatus.Private ? true : false;
                activity.UserId = album.UserId;
                activity.ReferenceId = 0;
                activity.ReferenceTenantTypeId = string.Empty;
                activity.SourceId = album.AlbumId;
                activity.TenantTypeId = TenantTypeIds.Instance().Album();
                activity.OwnerId = album.UserId;
                activity.OwnerName = album.Author;
                activity.OwnerType = ActivityOwnerTypes.Instance().User();
                //是否是公开的（用于是否推送站点动态）
                bool isPublic = album.PrivacyStatus == PrivacyStatus.Public ? true : false;

                activityService.Generate(activity, true, isPublic);
            }
            else if (auditDirection == false) //删除动态
            {
                activityService.DeleteSource(TenantTypeIds.Instance().Album(), album.AlbumId);
            }
        }

        /// <summary>
        /// 圈人动态处理
        /// </summary>
        private void LabelPhotoActivityModule_After(PhotoLabel photoLabel, CommonEventArgs eventArgs)
        {
            Photo photo = photoService.GetPhoto(photoLabel.PhotoId);
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {
                //初始化Owner为用户的动态
                Activity activity = Activity.New();
                activity.ActivityItemKey = ActivityItemKeys.Instance().LabelPhoto();
                activity.ApplicationId = PhotoConfig.Instance().ApplicationId;
                activity.HasImage = true;
                activity.HasMusic = false;
                activity.HasVideo = false;
                activity.IsOriginalThread = true;
                activity.IsPrivate = photoLabel.Photo.PrivacyStatus == PrivacyStatus.Private ? true : false;
                activity.UserId = photo.UserId;
                activity.ReferenceId = photoLabel.PhotoId;
                activity.ReferenceTenantTypeId = TenantTypeIds.Instance().Photo();
                activity.SourceId = photoLabel.LabelId;
                activity.TenantTypeId = TenantTypeIds.Instance().PhotoLabel();
                activity.OwnerId = photo.UserId;
                activity.OwnerName = photo.Author;
                activity.OwnerType = ActivityOwnerTypes.Instance().User();

                //是否是公开的（用于是否推送站点动态）
                bool isPublic = photoLabel.Photo.PrivacyStatus == PrivacyStatus.Public ? true : false;

                activityService.Generate(activity, true, isPublic);

                //再为被圈用户生成动态
                new ActivityRepository().InsertUserInboxs(activity.ActivityId, new long[] { photoLabel.ObjetId });
            }
            else
            {
                activityService.DeleteSource(TenantTypeIds.Instance().PhotoLabel(), photoLabel.LabelId);
            }
        }

        /// <summary>
        /// 评论动态处理
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="eventArgs"></param>
        private void CommentActivityEventModule_After(Comment comment, AuditEventArgs eventArgs)
        {
            bool? auditDirection = auditService.ResolveAuditDirection(eventArgs.OldAuditStatus, eventArgs.NewAuditStatus);
            Photo photo = null;
            if (comment.TenantTypeId == TenantTypeIds.Instance().Photo() && comment.UserId != comment.ToUserId)
            {
                if (auditDirection == true)
                {
                    photo = photoService.GetPhoto(comment.CommentedObjectId);
                    Activity activityOfFollower = Activity.New();
                    activityOfFollower.ActivityItemKey = ActivityItemKeys.Instance().CommentPhoto();
                    activityOfFollower.ApplicationId = PhotoConfig.Instance().ApplicationId;
                    activityOfFollower.HasImage = false;
                    activityOfFollower.HasMusic = false;
                    activityOfFollower.HasVideo = false;
                    activityOfFollower.IsOriginalThread = true;
                    activityOfFollower.IsPrivate = photo.PrivacyStatus == PrivacyStatus.Private ? true : false;
                    activityOfFollower.UserId = comment.UserId;
                    activityOfFollower.ReferenceId = photo.PhotoId;
                    activityOfFollower.ReferenceTenantTypeId = TenantTypeIds.Instance().Photo();
                    activityOfFollower.SourceId = comment.Id;
                    activityOfFollower.TenantTypeId = TenantTypeIds.Instance().Comment();
                    activityOfFollower.OwnerId = comment.UserId;
                    activityOfFollower.OwnerName = comment.Author;
                    activityOfFollower.OwnerType = ActivityOwnerTypes.Instance().User();

                    //是否是公开的（用于是否推送站点动态）
                    bool isPublic = photo.PrivacyStatus == PrivacyStatus.Public ? true : false;

                    activityService.Generate(activityOfFollower, true, isPublic);
                }
                else
                {
                    activityService.DeleteSource(TenantTypeIds.Instance().Comment(), comment.Id);
                }
            }
        }

        /// <summary>
        /// 照片顶踩
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="eventArgs"></param>
        private void PhotoSupportEventModule_After(long objectId, SupportOpposeEventArgs eventArgs)
        {
            if (eventArgs.TenantTypeId == TenantTypeIds.Instance().Photo())
            {
                //如果不是第一次顶踩，则不处理
                if (!eventArgs.FirstTime)
                {
                    return;
                }

                if (eventArgs.EventOperationType == EventOperationType.Instance().Support())
                {

                    Photo photo = photoService.GetPhoto(objectId);

                    //处理积分和威望
                    string pointItemKey = PointItemKeys.Instance().Photo_BeLiked();

                    //喜欢时产生积分
                    string eventOperationType = EventOperationType.Instance().Support();
                    string description = string.Format(ResourceAccessor.GetString("PointRecord_Pattern_" + eventOperationType), "照片", HtmlUtility.TrimHtml(photo.Description, 64));
                    pointService.GenerateByRole(photo.UserId, pointItemKey, description);

                }
            }
        }
    }
}