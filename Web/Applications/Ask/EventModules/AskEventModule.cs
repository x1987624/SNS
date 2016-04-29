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
using Spacebuilder.Ask.Resources;
using Tunynet;

namespace Spacebuilder.Ask.EventModules
{
    /// <summary>
    /// 处理问答动态、积分、通知的EventMoudle
    /// </summary>
    public class AskEventModule : IEventMoudle
    {
        public AskService askService { get; set; }
        private ActivityService activityService = new ActivityService();
        private AuditService auditService = new AuditService();
        private AttitudeService attitudeService = new AttitudeService(TenantTypeIds.Instance().AskAnswer());
        private CountService countService = new CountService(TenantTypeIds.Instance().AskQuestion());
        private NoticeService noticeService = new NoticeService();
        private OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
        private PointService pointService = new PointService();
        private SubscribeService subscribeService = new SubscribeService(TenantTypeIds.Instance().AskQuestion());
        private TagService tagService = new TagService(TenantTypeIds.Instance().AskQuestion());
        private IUserService userService = DIContainer.Resolve<IUserService>();

        /// <summary>
        /// 注册事件处理程序
        /// </summary>
        void IEventMoudle.RegisterEventHandler()
        {
            //提问
            EventBus<AskQuestion>.Instance().After += new CommonEventHandler<AskQuestion, CommonEventArgs>(AskQuestionEventModule_After);
            EventBus<AskQuestion, AuditEventArgs>.Instance().After += new CommonEventHandler<AskQuestion, AuditEventArgs>(AskQuestionAuditEventModule_After);

            //回答
            EventBus<AskAnswer>.Instance().After += new CommonEventHandler<AskAnswer, CommonEventArgs>(AskAnswerEventModule_After);
            EventBus<AskAnswer, AuditEventArgs>.Instance().After += new CommonEventHandler<AskAnswer, AuditEventArgs>(AskAnswerAuditEventModule_After);

            //顶踩
            EventBus<long, SupportOpposeEventArgs>.Instance().After += new CommonEventHandler<long, SupportOpposeEventArgs>(AskAnswerSupportEventModule_After);

            //评论
            EventBus<Comment, AuditEventArgs>.Instance().After += new CommonEventHandler<Comment, AuditEventArgs>(CommentEventModule_After);
        }

        #region 回答被赞同/反对的相关事件处理

        /// <summary>
        /// 回答被赞同的相关事件处理
        /// </summary>
        private void AskAnswerSupportEventModule_After(long objectId, SupportOpposeEventArgs eventArgs)
        {
            if (eventArgs.TenantTypeId == TenantTypeIds.Instance().AskAnswer())
            {
                //如果不是第一次顶踩，则不处理
                if (!eventArgs.FirstTime)
                {
                    return;
                }

                if (eventArgs.EventOperationType == EventOperationType.Instance().Support())
                {
                    AskAnswer answer = askService.GetAnswer(objectId);
                    AskQuestion question = answer.Question;

                    //创建顶回答的动态[关注回答者的粉丝可以看到该顶信息]
                    Activity activityOfFollower = Activity.New();
                    activityOfFollower.ActivityItemKey = ActivityItemKeys.Instance().SupportAskAnswer();
                    activityOfFollower.ApplicationId = AskConfig.Instance().ApplicationId;
                    activityOfFollower.IsOriginalThread = true;
                    activityOfFollower.IsPrivate = false;
                    activityOfFollower.TenantTypeId = TenantTypeIds.Instance().AskAnswer();
                    activityOfFollower.SourceId = answer.AnswerId;
                    activityOfFollower.UserId = eventArgs.UserId;
                    activityOfFollower.ReferenceId = answer.AnswerId;
                    activityOfFollower.ReferenceTenantTypeId = TenantTypeIds.Instance().AskAnswer();
                    activityOfFollower.OwnerId = eventArgs.UserId;
                    activityOfFollower.OwnerName = userService.GetFullUser(eventArgs.UserId).DisplayName;
                    activityOfFollower.OwnerType = ActivityOwnerTypes.Instance().User();
                    activityService.Generate(activityOfFollower, true);

                    //创建顶回答的动态[关注该问题的用户可以看到该顶信息]
                    Activity activityOfQuestionSubscriber = Activity.New();
                    activityOfQuestionSubscriber.ActivityItemKey = ActivityItemKeys.Instance().SupportAskAnswer();
                    activityOfQuestionSubscriber.ApplicationId = AskConfig.Instance().ApplicationId;
                    activityOfQuestionSubscriber.IsOriginalThread = true;
                    activityOfQuestionSubscriber.IsPrivate = false;
                    activityOfQuestionSubscriber.TenantTypeId = TenantTypeIds.Instance().AskAnswer();
                    activityOfQuestionSubscriber.SourceId = answer.AnswerId;
                    activityOfQuestionSubscriber.UserId = eventArgs.UserId;
                    activityOfQuestionSubscriber.ReferenceId = answer.AnswerId;
                    activityOfQuestionSubscriber.ReferenceTenantTypeId = TenantTypeIds.Instance().AskAnswer();
                    activityOfQuestionSubscriber.OwnerId = question.QuestionId;
                    activityOfQuestionSubscriber.OwnerName = question.Subject;
                    activityOfQuestionSubscriber.OwnerType = ActivityOwnerTypes.Instance().AskQuestion();
                    activityService.Generate(activityOfQuestionSubscriber, false);

                    //处理积分和威望
                    string pointItemKey = PointItemKeys.Instance().Ask_BeSupported();

                    //回答收到赞同时产生积分
                    string eventOperationType = EventOperationType.Instance().Support();
                    string description = string.Format(ResourceAccessor.GetString("PointRecord_Pattern_" + eventOperationType), "回答", question.Subject);
                    pointService.GenerateByRole(answer.UserId, pointItemKey, description);

                    //记录用户的威望
                    PointItem pointItem = pointService.GetPointItem(pointItemKey);
                    ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().UserAskReputation(), pointItem.ReputationPoints);


                    //赞同者自动关注问题
                    if (!subscribeService.IsSubscribed(question.QuestionId, eventArgs.UserId))
                    {
                        subscribeService.Subscribe(question.QuestionId, eventArgs.UserId);

                        //问题关注数计数，用于“可能感兴趣的问题”关联表查询
                        countService.ChangeCount(CountTypes.Instance().QuestionFollowerCount(), question.QuestionId, question.UserId, 1, false);
                    }

                    //增加赞同的用户计数
                    ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().AnswerSupportCount(), 1);
                }
                else if (eventArgs.EventOperationType == EventOperationType.Instance().Oppose())
                {
                    AskAnswer answer = askService.GetAnswer(objectId);
                    AskQuestion question = answer.Question;

                    //处理积分和威望
                    string pointItemKey = PointItemKeys.Instance().Ask_BeOpposed();

                    //回答收到反对时产生积分
                    string eventOperationType = EventOperationType.Instance().Oppose();
                    string description = string.Format(ResourceAccessor.GetString("PointRecord_Pattern_" + eventOperationType), "回答", question.Subject);
                    pointService.GenerateByRole(answer.UserId, pointItemKey, description);

                    //记录用户的威望
                    PointItem pointItem = pointService.GetPointItem(pointItemKey);
                    ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().UserAskReputation(), pointItem.ReputationPoints);


                    //反对者自动关注问题
                    if (!subscribeService.IsSubscribed(question.QuestionId, eventArgs.UserId))
                    {
                        subscribeService.Subscribe(answer.QuestionId, eventArgs.UserId);

                        //问题关注数计数，用于“可能感兴趣的问题”关联表查询
                        countService.ChangeCount(CountTypes.Instance().QuestionFollowerCount(), question.QuestionId, question.UserId, 1, false);
                    }

                    //增加反对的用户计数
                    ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().AnswerOpposeCount(), 1);
                }
            }
        }

        #endregion

        #region 评论的相关事件处理

        /// <summary>
        /// 评论的相关事件处理
        /// </summary>
        private void CommentEventModule_After(Comment comment, AuditEventArgs eventArgs)
        {
            if (comment.TenantTypeId != TenantTypeIds.Instance().AskQuestion() && comment.TenantTypeId != TenantTypeIds.Instance().AskAnswer())
            {
                return;
            }

            bool? auditDirection = auditService.ResolveAuditDirection(eventArgs.OldAuditStatus, eventArgs.NewAuditStatus);
            AskQuestion question = null;

            //评论问题
            if (comment.TenantTypeId == TenantTypeIds.Instance().AskQuestion())
            {
                //生成动态
                if (auditDirection == true)
                {
                    question = askService.GetQuestion(comment.CommentedObjectId);
                    if (question.UserId != comment.UserId)
                    {
                        //创建评论的动态[关注评论者的粉丝可以看到该评论]
                        Activity activityOfFollower = Activity.New();
                        activityOfFollower.ActivityItemKey = ActivityItemKeys.Instance().CommentAskQuestion();
                        activityOfFollower.ApplicationId = AskConfig.Instance().ApplicationId;
                        activityOfFollower.IsOriginalThread = true;
                        activityOfFollower.IsPrivate = false;
                        activityOfFollower.TenantTypeId = TenantTypeIds.Instance().Comment();
                        activityOfFollower.SourceId = comment.Id;
                        activityOfFollower.UserId = comment.UserId;
                        activityOfFollower.ReferenceId = question.QuestionId;
                        activityOfFollower.ReferenceTenantTypeId = TenantTypeIds.Instance().AskQuestion();
                        activityOfFollower.OwnerId = comment.UserId;
                        activityOfFollower.OwnerName = comment.Author;
                        activityOfFollower.OwnerType = ActivityOwnerTypes.Instance().User();
                        activityService.Generate(activityOfFollower, true);

                        //创建评论的动态[关注该问题的用户可以看到该评论]
                        Activity activityOfQuestionSubscriber = Activity.New();
                        activityOfQuestionSubscriber.ActivityItemKey = ActivityItemKeys.Instance().CommentAskQuestion();
                        activityOfQuestionSubscriber.ApplicationId = AskConfig.Instance().ApplicationId;
                        activityOfQuestionSubscriber.IsOriginalThread = true;
                        activityOfQuestionSubscriber.IsPrivate = false;
                        activityOfQuestionSubscriber.TenantTypeId = TenantTypeIds.Instance().Comment();
                        activityOfQuestionSubscriber.SourceId = comment.Id;
                        activityOfQuestionSubscriber.UserId = comment.UserId;
                        activityOfQuestionSubscriber.ReferenceId = question.QuestionId;
                        activityOfQuestionSubscriber.ReferenceTenantTypeId = TenantTypeIds.Instance().AskQuestion();
                        activityOfQuestionSubscriber.OwnerId = question.QuestionId;
                        activityOfQuestionSubscriber.OwnerName = question.Subject;
                        activityOfQuestionSubscriber.OwnerType = ActivityOwnerTypes.Instance().AskQuestion();
                        activityService.Generate(activityOfQuestionSubscriber, false);
                    }
                }
                else if (auditDirection == false)
                {
                    activityService.DeleteSource(TenantTypeIds.Instance().Comment(), comment.Id);
                }
            }
            //评论回答
            else if (comment.TenantTypeId == TenantTypeIds.Instance().AskAnswer())
            {
                //生成动态
                if (auditDirection == true)
                {
                    AskAnswer answer = askService.GetAnswer(comment.CommentedObjectId);
                    question = answer.Question;
                    if (answer.UserId != comment.UserId)
                    {
                        //创建评论的动态[关注评论者的粉丝可以看到该评论]
                        Activity activityOfFollower = Activity.New();
                        activityOfFollower.ActivityItemKey = ActivityItemKeys.Instance().CommentAskAnswer();
                        activityOfFollower.ApplicationId = AskConfig.Instance().ApplicationId;
                        activityOfFollower.IsOriginalThread = true;
                        activityOfFollower.IsPrivate = false;
                        activityOfFollower.TenantTypeId = TenantTypeIds.Instance().Comment();
                        activityOfFollower.SourceId = comment.Id;
                        activityOfFollower.UserId = comment.UserId;
                        activityOfFollower.ReferenceId = answer.AnswerId;
                        activityOfFollower.ReferenceTenantTypeId = TenantTypeIds.Instance().AskAnswer();
                        activityOfFollower.OwnerId = comment.UserId;
                        activityOfFollower.OwnerName = comment.Author;
                        activityOfFollower.OwnerType = ActivityOwnerTypes.Instance().User();
                        activityService.Generate(activityOfFollower, true);

                        //创建评论的动态[关注该问题的用户可以看到该评论]
                        Activity activityOfQuestionSubscriber = Activity.New();
                        activityOfQuestionSubscriber.ActivityItemKey = ActivityItemKeys.Instance().CommentAskAnswer();
                        activityOfQuestionSubscriber.ApplicationId = AskConfig.Instance().ApplicationId;
                        activityOfQuestionSubscriber.IsOriginalThread = true;
                        activityOfQuestionSubscriber.IsPrivate = false;
                        activityOfQuestionSubscriber.TenantTypeId = TenantTypeIds.Instance().Comment();
                        activityOfQuestionSubscriber.SourceId = comment.Id;
                        activityOfQuestionSubscriber.UserId = comment.UserId;
                        activityOfQuestionSubscriber.ReferenceId = answer.AnswerId;
                        activityOfQuestionSubscriber.ReferenceTenantTypeId = TenantTypeIds.Instance().AskAnswer();
                        activityOfQuestionSubscriber.OwnerId = question.QuestionId;
                        activityOfQuestionSubscriber.OwnerName = question.Subject;
                        activityOfQuestionSubscriber.OwnerType = ActivityOwnerTypes.Instance().AskQuestion();
                        activityService.Generate(activityOfQuestionSubscriber, false);
                    }
                }
                else if (auditDirection == false)
                {
                    activityService.DeleteSource(TenantTypeIds.Instance().Comment(), comment.Id);
                }
            }

            ////关注的问题有新评论时，自动通知关注者
            //IEnumerable<long> userIds = subscribeService.GetUserIdsOfObject(question.QuestionId);
            //foreach (var userId in userIds)
            //{
            //    //通知的对象排除掉自己
            //    if (userId == comment.UserId)
            //    {
            //        continue;
            //    }

            //    Notice notice = Notice.New();
            //    notice.UserId = userId;
            //    notice.ApplicationId = AskConfig.Instance().ApplicationId;
            //    notice.TypeId = NoticeTypeIds.Instance().Hint();
            //    notice.LeadingActor = comment.Author;
            //    notice.LeadingActorUrl =SiteUrls.FullUrl(SiteUrls.Instance().AskUser(UserIdToUserNameDictionary.GetUserName(comment.UserId)));
            //    notice.RelativeObjectName = HtmlUtility.TrimHtml(question.Subject, 64);
            //    notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskQuestionDetail(question.QuestionId));
            //    notice.TemplateName = NoticeTemplateNames.Instance().NewAskComment();
            //    noticeService.Create(notice);
            //}

            //评论者自动关注该问题
            if (!subscribeService.IsSubscribed(question.QuestionId, comment.UserId))
            {
                subscribeService.Subscribe(question.QuestionId, comment.UserId);

                //问题关注数计数，用于“可能感兴趣的问题”关联表查询
                countService.ChangeCount(CountTypes.Instance().QuestionFollowerCount(), question.QuestionId, question.UserId, 1, false);
            }
        }

        #endregion

        #region 提问的相关事件处理

        /// <summary>
        /// 问题审核状态变更事件
        /// </summary>
        private void AskQuestionAuditEventModule_After(AskQuestion question, AuditEventArgs eventArgs)
        {
            bool? auditDirection = auditService.ResolveAuditDirection(eventArgs.OldAuditStatus, eventArgs.NewAuditStatus);

            //审核状态发生变化时处理积分
            string pointItemKey = string.Empty;
            string eventOperationType = string.Empty;

            if (auditDirection == true) //加积分
            {
                pointItemKey = PointItemKeys.Instance().Ask_CreateQuestion();
                if (eventArgs.OldAuditStatus == null)
                {
                    eventOperationType = EventOperationType.Instance().Create();
                }
                else
                {
                    eventOperationType = EventOperationType.Instance().Approved();
                }
            }
            else if (auditDirection == false) //减积分
            {
                pointItemKey = PointItemKeys.Instance().Ask_DeleteQuestion();
                if (eventArgs.NewAuditStatus == null)
                {
                    eventOperationType = EventOperationType.Instance().Delete();
                }
                else
                {
                    eventOperationType = EventOperationType.Instance().Disapproved();
                }
            }

            if (!string.IsNullOrEmpty(pointItemKey))
            {
                string description = string.Format(ResourceAccessor.GetString("PointRecord_Pattern_" + eventOperationType), "问题", question.Subject);
                pointService.GenerateByRole(question.UserId, pointItemKey, description, eventOperationType == EventOperationType.Instance().Create() || eventOperationType == EventOperationType.Instance().Delete() && eventArgs.OperatorInfo.OperatorUserId == question.UserId);

                //记录用户的威望
                PointItem pointItem = pointService.GetPointItem(pointItemKey);
                ownerDataService.Change(question.UserId, OwnerDataKeys.Instance().UserAskReputation(), pointItem.ReputationPoints);
            }

            //审核状态发生变化时生成动态
            if (auditDirection == true)
            {
                //1.创建发布问题的动态[关注发布者的粉丝可以看到该动态]
                Activity activityOfFollower = Activity.New();
                activityOfFollower.ActivityItemKey = ActivityItemKeys.Instance().CreateAskQuestion();
                activityOfFollower.ApplicationId = AskConfig.Instance().ApplicationId;
                //判断是否有图片、音频、视频
                AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().AskQuestion());
                IEnumerable<Attachment> attachments = attachmentService.GetsByAssociateId(question.QuestionId);
                if (attachments != null && attachments.Any(n => n.MediaType == MediaType.Image))
                {
                    activityOfFollower.HasImage = true;
                }
                activityOfFollower.IsOriginalThread = true;
                activityOfFollower.IsPrivate = false;
                activityOfFollower.TenantTypeId = TenantTypeIds.Instance().AskQuestion();
                activityOfFollower.SourceId = question.QuestionId;
                activityOfFollower.UserId = question.UserId;
                activityOfFollower.ReferenceId = 0;
                activityOfFollower.ReferenceTenantTypeId = string.Empty;
                activityOfFollower.OwnerId = question.UserId;
                activityOfFollower.OwnerName = question.Author;
                activityOfFollower.OwnerType = ActivityOwnerTypes.Instance().User();
                activityService.Generate(activityOfFollower, true);

                //2.创建发布问题的动态[关注问题所属标签的用户可以看到该动态]
                Activity activityOfTagSubscriber = Activity.New();
                activityOfTagSubscriber.ActivityItemKey = ActivityItemKeys.Instance().CreateAskQuestion();
                activityOfTagSubscriber.ApplicationId = AskConfig.Instance().ApplicationId;
                activityOfTagSubscriber.HasImage = activityOfFollower.HasImage;
                activityOfTagSubscriber.IsOriginalThread = true;
                activityOfTagSubscriber.IsPrivate = false;
                activityOfTagSubscriber.TenantTypeId = TenantTypeIds.Instance().AskQuestion();
                activityOfTagSubscriber.SourceId = question.QuestionId;
                activityOfTagSubscriber.UserId = question.UserId;
                activityOfTagSubscriber.ReferenceId = 0;
                activityOfTagSubscriber.ReferenceTenantTypeId = string.Empty;

                IEnumerable<Tag> tags = tagService.GetTopTagsOfItem(question.QuestionId, 100);
                foreach (var tag in tags)
                {
                    activityOfTagSubscriber.OwnerId = tag.TagId;
                    activityOfTagSubscriber.OwnerName = tag.TagName;
                    activityOfTagSubscriber.OwnerType = ActivityOwnerTypes.Instance().AskTag();
                    activityService.Generate(activityOfTagSubscriber, false);
                }
            }
            //删除动态
            else if (auditDirection == false)
            {
                activityService.DeleteSource(TenantTypeIds.Instance().AskQuestion(), question.QuestionId);
            }
        }

        /// <summary>
        /// 问题增删改等触发的事件
        /// </summary>
        private void AskQuestionEventModule_After(AskQuestion question, CommonEventArgs eventArgs)
        {
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {
                //冻结用户的积分
                if (question.AddedReward > 0)
                {
                    userService.FreezeTradePoints(question.UserId, question.AddedReward);
                }

                //向定向提问的目标用户发送通知
                if (question.AskUserId > 0)
                {
                    Notice notice = Notice.New();
                    notice.UserId = question.AskUserId;
                    notice.ApplicationId = AskConfig.Instance().ApplicationId;
                    notice.TypeId = NoticeTypeIds.Instance().Hint();
                    notice.LeadingActor = question.Author;
                    notice.LeadingActorUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskUser(UserIdToUserNameDictionary.GetUserName(question.UserId)));
                    notice.RelativeObjectName = HtmlUtility.TrimHtml(question.Subject, 64);
                    notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskQuestionDetail(question.QuestionId));
                    notice.TemplateName = NoticeTemplateNames.Instance().AskUser();
                    noticeService.Create(notice);
                }

                //自动关注（提问者自动关注[订阅]该问题）
                if (!subscribeService.IsSubscribed(question.QuestionId, question.UserId))
                {
                    subscribeService.Subscribe(question.QuestionId, question.UserId);

                    //问题关注数计数，用于“可能感兴趣的问题”关联表查询
                    countService.ChangeCount(CountTypes.Instance().QuestionFollowerCount(), question.QuestionId, question.UserId, 1, false);
                }
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Delete())
            {
                //已解决问题的积分不返还，未解决/已取消的问题应该解除冻结积分
                if (question.Reward > 0 && question.Status != QuestionStatus.Resolved)
                {
                    userService.UnfreezeTradePoints(question.UserId, question.Reward);
                }

                //删除所有用户对该问题的关注(订阅)
                IEnumerable<long> userIds = subscribeService.GetUserIdsOfObject(question.QuestionId);
                foreach (var userId in userIds)
                {
                    subscribeService.CancelSubscribe(question.QuestionId, userId);
                }
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Update())
            {
                //冻结用户的积分
                if (question.AddedReward != 0)
                {
                    userService.FreezeTradePoints(question.UserId, question.AddedReward);
                }
                if (question.Status == QuestionStatus.Canceled)
                {
                    subscribeService.CancelSubscribe(question.QuestionId, question.UserId);
                    int pageSize = 100;
                    for (int i = 1; i <= question.AnswerCount; i = i + pageSize)
                    {
                        PagingDataSet<AskAnswer> answers = askService.GetAnswersByQuestionId(question.QuestionId, SortBy_AskAnswer.DateCreated_Desc, pageSize, i);
                        foreach (AskAnswer answer in answers)
                        {
                            subscribeService.CancelSubscribe(question.QuestionId, answer.UserId);
                        }
                    }
                }
            }
            //加精时处理积分和威望并产生通知
            else if (eventArgs.EventOperationType == EventOperationType.Instance().SetEssential())
            {
                string pointItemKey = PointItemKeys.Instance().EssentialContent();
                string description = string.Format(ResourceAccessor.GetString("PointRecord_Pattern_" + eventArgs.EventOperationType), "问题", question.Subject);
                pointService.GenerateByRole(question.UserId, pointItemKey, description);

                if (question.UserId > 0)
                {
                    Notice notice = Notice.New();
                    notice.UserId = question.UserId;
                    notice.ApplicationId = AskConfig.Instance().ApplicationId;
                    notice.TypeId = NoticeTypeIds.Instance().Hint();
                    notice.LeadingActor = question.Author;
                    notice.LeadingActorUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskUser(UserIdToUserNameDictionary.GetUserName(question.UserId)));
                    notice.RelativeObjectName = HtmlUtility.TrimHtml(question.Subject, 64);
                    notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskQuestionDetail(question.QuestionId));
                    notice.TemplateName = NoticeTemplateNames.Instance().ManagerSetEssential();
                    noticeService.Create(notice);
                }
                //记录用户的威望
                PointItem pointItem = pointService.GetPointItem(pointItemKey);
                ownerDataService.Change(question.UserId, OwnerDataKeys.Instance().UserAskReputation(), pointItem.ReputationPoints);
            }
        }

        #endregion

        #region 回答的相关事件处理

        /// <summary>
        /// 回答审核状态变更事件
        /// </summary>
        private void AskAnswerAuditEventModule_After(AskAnswer answer, AuditEventArgs eventArgs)
        {
            bool? auditDirection = auditService.ResolveAuditDirection(eventArgs.OldAuditStatus, eventArgs.NewAuditStatus);

            //审核状态发生变化时处理积分
            string pointItemKey = string.Empty;
            string eventOperationType = string.Empty;

            if (auditDirection == true) //加积分
            {
                pointItemKey = PointItemKeys.Instance().Ask_CreateAnswer();
                if (eventArgs.OldAuditStatus == null)
                {
                    eventOperationType = EventOperationType.Instance().Create();
                }
                else
                {
                    eventOperationType = EventOperationType.Instance().Approved();
                }
            }
            else if (auditDirection == false) //减积分
            {
                pointItemKey = PointItemKeys.Instance().Ask_DeleteAnswer();
                if (eventArgs.NewAuditStatus == null)
                {
                    eventOperationType = EventOperationType.Instance().Delete();
                }
                else
                {
                    eventOperationType = EventOperationType.Instance().Disapproved();
                }
            }

            if (!string.IsNullOrEmpty(pointItemKey))
            {
                AskQuestion question = answer.Question;
                string description = string.Format(ResourceAccessor.GetString("PointRecord_Pattern_" + eventOperationType), "回答", HtmlUtility.TrimHtml(answer.Body, 10));
                pointService.GenerateByRole(answer.UserId, pointItemKey, description, eventOperationType == EventOperationType.Instance().Create() || eventOperationType == EventOperationType.Instance().Delete());

                //记录用户的威望
                PointItem pointItem = pointService.GetPointItem(pointItemKey);
                ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().UserAskReputation(), pointItem.ReputationPoints);
            }

            //审核状态发生变化时生成动态
            if (auditDirection == true)
            {
                AskQuestion question = answer.Question;

                //创建回答的动态[关注回答者的粉丝可以看到该回答]
                Activity activityOfFollower = Activity.New();
                activityOfFollower.ActivityItemKey = ActivityItemKeys.Instance().CreateAskAnswer();
                activityOfFollower.ApplicationId = AskConfig.Instance().ApplicationId;
                //判断是否有图片、音频、视频
                AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().AskAnswer());
                IEnumerable<Attachment> attachments = attachmentService.GetsByAssociateId(answer.AnswerId);
                if (attachments != null && attachments.Any(n => n.MediaType == MediaType.Image))
                {
                    activityOfFollower.HasImage = true;
                }
                activityOfFollower.IsOriginalThread = true;
                activityOfFollower.IsPrivate = false;
                activityOfFollower.TenantTypeId = TenantTypeIds.Instance().AskAnswer();
                activityOfFollower.SourceId = answer.AnswerId;
                activityOfFollower.UserId = answer.UserId;
                activityOfFollower.ReferenceId = answer.QuestionId;
                activityOfFollower.ReferenceTenantTypeId = TenantTypeIds.Instance().AskQuestion();
                activityOfFollower.OwnerId = answer.UserId;
                activityOfFollower.OwnerName = answer.Author;
                activityOfFollower.OwnerType = ActivityOwnerTypes.Instance().User();
                activityService.Generate(activityOfFollower, true);

                //创建回答的动态[关注该问题的用户可以看到该回答]
                Activity activityOfQuestionSubscriber = Activity.New();
                activityOfQuestionSubscriber.ActivityItemKey = ActivityItemKeys.Instance().CreateAskAnswer();
                activityOfQuestionSubscriber.ApplicationId = AskConfig.Instance().ApplicationId;
                activityOfQuestionSubscriber.HasImage = activityOfFollower.HasImage;
                activityOfQuestionSubscriber.IsOriginalThread = true;
                activityOfQuestionSubscriber.IsPrivate = false;
                activityOfQuestionSubscriber.TenantTypeId = TenantTypeIds.Instance().AskAnswer();
                activityOfQuestionSubscriber.SourceId = answer.AnswerId;
                activityOfQuestionSubscriber.UserId = answer.UserId;
                activityOfQuestionSubscriber.ReferenceId = answer.QuestionId;
                activityOfQuestionSubscriber.ReferenceTenantTypeId = TenantTypeIds.Instance().AskQuestion();
                activityOfQuestionSubscriber.OwnerId = question.QuestionId;
                activityOfQuestionSubscriber.OwnerName = question.Subject;
                activityOfQuestionSubscriber.OwnerType = ActivityOwnerTypes.Instance().AskQuestion();
                activityService.Generate(activityOfQuestionSubscriber, false);
            }
            else if (auditDirection == false)
            {
                activityService.DeleteSource(TenantTypeIds.Instance().AskAnswer(), answer.AnswerId);
            }
        }

        /// <summary>
        /// 回答增删改等触发的事件
        /// </summary>
        private void AskAnswerEventModule_After(AskAnswer answer, CommonEventArgs eventArgs)
        {
            if (eventArgs.EventOperationType == EventOperationType.Instance().Create())
            {
                //向关注该问题的用户发送通知
                IEnumerable<long> userIds = subscribeService.GetUserIdsOfObject(answer.QuestionId);
                AskQuestion question = answer.Question;
                foreach (var userId in userIds)
                {
                    //通知的对象排除掉自己
                    if (userId == answer.UserId)
                    {
                        continue;
                    }

                    Notice notice = Notice.New();
                    notice.UserId = userId;
                    notice.ApplicationId = AskConfig.Instance().ApplicationId;
                    notice.TypeId = NoticeTypeIds.Instance().Reply();
                    notice.LeadingActor = answer.Author;
                    notice.LeadingActorUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskUser(UserIdToUserNameDictionary.GetUserName(answer.UserId)));
                    notice.RelativeObjectName = HtmlUtility.TrimHtml(question.Subject, 64);
                    notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskQuestionDetail(question.QuestionId));
                    notice.TemplateName = NoticeTemplateNames.Instance().NewAnswer();
                    noticeService.Create(notice);
                }

                //自动关注（回答者自动关注[订阅]该问题）
                if (!subscribeService.IsSubscribed(question.QuestionId, answer.UserId))
                {
                    subscribeService.Subscribe(question.QuestionId, answer.UserId);

                    //问题关注数计数，用于“可能感兴趣的问题”关联表查询
                    countService.ChangeCount(CountTypes.Instance().QuestionFollowerCount(), question.QuestionId, question.UserId, 1, false);
                }
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Delete())
            {
                //删除回答者对问题的关注
                subscribeService.CancelSubscribe(answer.QuestionId, answer.UserId);
            }
            else if (eventArgs.EventOperationType == EventOperationType.Instance().Update())
            {
                AskQuestion question = new AskService().GetQuestion(answer.QuestionId);

                if (answer.IsBest && question.Status == QuestionStatus.Resolved && !question.IsTrated)
                {
                    //如果问题有悬赏则悬赏分值转移到回答者的帐户（如有交易税，需要扣除）
                    if (question.Reward > 0)
                    {
                        pointService.Trade(question.UserId, answer.UserId, question.Reward, string.Format(Resource.PointRecord_Pattern_QuestionReward, question.Subject), false);
                    }
                    askService.SetTradeStatus(question, true);

                    //处理采纳回答相关的积分和威望
                    string pointItemKey_AcceptedAnswer = PointItemKeys.Instance().Ask_AcceptedAnswer();

                    //采纳回答时产生积分
                    string description_AcceptedAnswer = string.Format(Resource.PointRecord_Pattern_AcceptedAnswer, question.Subject);
                    pointService.GenerateByRole(question.UserId, pointItemKey_AcceptedAnswer, description_AcceptedAnswer);

                    //记录用户的威望
                    PointItem pointItem_AcceptedAnswer = pointService.GetPointItem(pointItemKey_AcceptedAnswer);
                    ownerDataService.Change(question.UserId, OwnerDataKeys.Instance().UserAskReputation(), pointItem_AcceptedAnswer.ReputationPoints);


                    //处理回答被采纳相关的积分和威望
                    string pointItemKey_AnswerWereAccepted = PointItemKeys.Instance().Ask_AnswerWereAccepted();

                    //回答被采纳时产生积分
                    string description_AnswerWereAccepted = string.Format(Resource.PointRecord_Pattern_AnswerWereAccepted, question.Subject);
                    pointService.GenerateByRole(answer.UserId, pointItemKey_AnswerWereAccepted, description_AnswerWereAccepted);

                    //记录用户的威望
                    PointItem pointItem_AnswerWereAccepted = pointService.GetPointItem(pointItemKey_AnswerWereAccepted);
                    ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().UserAskReputation(), pointItem_AnswerWereAccepted.ReputationPoints);

                    //向关注该问题的用户发送通知
                    IEnumerable<long> userIds = subscribeService.GetUserIdsOfObject(answer.QuestionId);
                    foreach (var userId in userIds)
                    {
                        //通知的对象排除掉自己
                        if (userId == question.UserId)
                        {
                            continue;
                        }

                        Notice notice = Notice.New();
                        notice.UserId = userId;
                        notice.ApplicationId = AskConfig.Instance().ApplicationId;
                        notice.TypeId = NoticeTypeIds.Instance().Hint();
                        notice.LeadingActor = question.Author;
                        notice.LeadingActorUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskUser(UserIdToUserNameDictionary.GetUserName(question.UserId)));
                        notice.RelativeObjectName = HtmlUtility.TrimHtml(question.Subject, 64);
                        notice.RelativeObjectUrl = SiteUrls.FullUrl(SiteUrls.Instance().AskQuestionDetail(question.QuestionId));
                        notice.TemplateName = NoticeTemplateNames.Instance().SetBestAnswer();
                        noticeService.Create(notice);
                    }
                }
            }
        }

        #endregion


        //动态
        //1.	关注标签下的问题
        //    1)	提问时生成动态；
        //    2)	问题增加回答、问题的回答被赞同、问题的回答被评论生成动态；
        //2.	关注用户
        //    1)	提问时生成动态；
        //    2)	回答问题、赞同回答、评论问题或回答时生成动态；



        //通知
        //1.	关注的问题：
        //    1)	有新回答，通知内容：[回答者] 回答了问题 [问题名称]
        //    2） 采纳满意回答，通知内容：[提问者] 为问题 [问题名称] 采纳了 [回答者] 的回答
        //    3)	关注的问题有新的回答、评论时，自动通知关注者
        //2.	向他提问（定向提问）：
        //    1)	问题提交后，自动给被提问用户发送通知；通知内容：[提问者]邀请您对[问题名称]进行解答
        //



        //积分
        //冻结积分
        //取消冻结
        //积分交易
        //产生积分
        //    提问
        //    回答
        //    回答收到赞同
        //    回答收到反对
        //    回答被采纳
        //    问题加精/取消精华



        //自动关注
        //问题的所有参与人员（提问者、回答者、投票者、评论者）自动关注该问题
    }
}