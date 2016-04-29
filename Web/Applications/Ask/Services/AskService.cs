//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Events;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答业务逻辑类
    /// </summary>
    public class AskService
    {
        private IAskAnswerRepository askAnswerRepository;
        private IAskQuestionRepository askQuestionRepository;

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public AskService()
            : this(new AskAnswerRepository(), new AskQuestionRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="askAnswerRepository">问答仓储实现</param>
        /// <param name="askQuestionRepository">问答仓储实现</param>
        public AskService(IAskAnswerRepository askAnswerRepository, IAskQuestionRepository askQuestionRepository)
        {
            this.askAnswerRepository = askAnswerRepository;
            this.askQuestionRepository = askQuestionRepository;
        }

        #endregion

        #region 问答维护

        /// <summary>
        /// 发布问题
        /// </summary>
        /// <remarks>
        /// 1.用户提问时，悬赏的积分会被冻结，需要先扣除相应的用户积分；同时发布问题会产生新积分
        /// 2.注意在EventModule中处理动态、积分、通知、自动关注（提问者自动关注该问题）；
        /// 3.使用AuditService.ChangeAuditStatusForCreate设置审核状态；
        /// 4.注意调用AttachmentService转换临时附件；
        /// 5.需要触发的事件:1)Create的OnBefore、OnAfter；2)审核状态变更；
        /// </remarks>
        /// <param name="question">问题实体</param>
        /// <returns>是否创建成功</returns>
        public bool CreateQuestion(AskQuestion question)
        {
            EventBus<AskQuestion>.Instance().OnBefore(question, new CommonEventArgs(EventOperationType.Instance().Create()));

            //设置审核状态
            new AuditService().ChangeAuditStatusForCreate(question.UserId, question);

            askQuestionRepository.Insert(question);
            if (question.QuestionId > 0)
            {
                //将临时附件转换为正式附件
                AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().AskQuestion());
                attachmentService.ToggleTemporaryAttachments(question.UserId, TenantTypeIds.Instance().AskQuestion(), question.QuestionId);

                //用户计数
                OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                ownerDataService.Change(question.UserId, OwnerDataKeys.Instance().QuestionCount(), 1);

                AtUserService atUserService = new AtUserService(TenantTypeIds.Instance().AskQuestion());
                atUserService.ResolveBodyForEdit(question.Body, question.UserId, question.QuestionId);


                EventBus<AskQuestion>.Instance().OnAfter(question, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<AskQuestion, AuditEventArgs>.Instance().OnAfter(question, new AuditEventArgs(null, question.AuditStatus));
            }

            return question.QuestionId > 0;
        }

        /// <summary>
        /// 编辑问题
        /// </summary>
        /// <remarks>
        /// 1.使用AuditService.ChangeAuditStatusForUpdate设置审核状态；
        /// 2.Repository更新时不处理：IsEssential
        /// 3.需要触发的事件:Update的OnBefore、OnAfter；
        /// </remarks>
        /// <param name="question">问题实体</param>
        /// <param name="changeAuditStatusForUpdate">是否更新审核状态</param>
        public void UpdateQuestion(AskQuestion question, bool changeAuditStatusForUpdate = true)
        {
            AskQuestion question_beforeUpDate = GetQuestion(question.QuestionId);
            if (question.Reward != question_beforeUpDate.Reward && question.Reward < (question.User.TradePoints + question_beforeUpDate.Reward))
            {
                question.AddedReward = question.Reward - question_beforeUpDate.Reward;
            }
            else
            {
                question.AddedReward = 0;
            }

            EventBus<AskQuestion>.Instance().OnBefore(question, new CommonEventArgs(EventOperationType.Instance().Update()));
            AuditStatus prevAuditStatus = question.AuditStatus;

            //设置审核状态
            if (changeAuditStatusForUpdate)
            {
                new AuditService().ChangeAuditStatusForUpdate(question.UserId, question);
            }

            askQuestionRepository.Update(question);

            AtUserService atUserService = new AtUserService(TenantTypeIds.Instance().AskQuestion());
            atUserService.ResolveBodyForEdit(question.Body, question.UserId, question.QuestionId);
            
            EventBus<AskQuestion>.Instance().OnAfter(question, new CommonEventArgs(EventOperationType.Instance().Update()));
            EventBus<AskQuestion, AuditEventArgs>.Instance().OnAfter(question, new AuditEventArgs(prevAuditStatus, question.AuditStatus));
        }

        /// <summary>
        /// 取消问题
        /// </summary>
        /// <param name="question">问题实体</param>
        public void CancelQuestion(AskQuestion question)
        {
            question.Status = QuestionStatus.Canceled;

            //调用Service中的Update方法，以触发相应的事件，但是不更新审核状态
            this.UpdateQuestion(question, false);
        }

        /// <summary>
        /// 删除问题
        /// </summary>
        /// <remarks>
        /// 1.只有管理员可删除问题，已解决问题的积分不返还，未解决的问题应该解除冻结积分，通过EventModule处理；
        /// 2.注意需要删除动态，通过EventModule处理；
        /// 3.注意需要扣除发布问题时增加的积分，通过EventModule处理；
        /// 4.注意需要删除所有用户对该问题的关注，通过EventModule处理；
        /// 5.注意删除与标签的关联，其它数据的删除由各模块自动处理
        /// 6.需要触发的事件:1)Delete的OnBefore、OnAfter；2)审核状态变更
        /// </remarks>
        /// <param name="question">问题实体</param>
        public void DeleteQuestion(AskQuestion question)
        {
            EventBus<AskQuestion>.Instance().OnBefore(question, new CommonEventArgs(EventOperationType.Instance().Delete()));

            //删除与标签的关联
            TagService tagService = new TagService(TenantTypeIds.Instance().AskQuestion());
            tagService.ClearTagsFromItem(question.QuestionId, question.UserId);

            //删除问题的所有回答
            int pageSize = 100;
            int pageIndex = 1;
            int pageCount = 1;
            do
            {
                PagingDataSet<AskAnswer> answers = this.GetAnswersByQuestionId(question.QuestionId, SortBy_AskAnswer.DateCreated_Desc, pageSize, pageIndex);
                foreach (AskAnswer answer in answers)
                {
                    this.DeleteAnswer(answer);
                }
                pageCount = answers.PageCount;
                pageIndex++;
            } while (pageIndex <= pageCount);

            //删除问题
            askQuestionRepository.Delete(question);

            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
            ownerDataService.Change(question.UserId, OwnerDataKeys.Instance().QuestionCount(), -1);

            EventBus<AskQuestion>.Instance().OnAfter(question, new CommonEventArgs(EventOperationType.Instance().Delete()));
            EventBus<AskQuestion, AuditEventArgs>.Instance().OnAfter(question, new AuditEventArgs(question.AuditStatus, null));
        }

        /// <summary>
        /// 设置/取消精华问题（管理员操作）
        /// </summary>
        /// <remarks>
        /// 1.精华状态未变化不用进行任何操作
        /// 2.在EventModule里处理积分
        /// 3.需要触发的事件：加精或取消精华SetEssential的OnAfter
        /// </remarks>
        /// <param name="question">问题实体</param>
        /// <param name="isEssential">是否精华</param>
        public void SetEssential(AskQuestion question, bool isEssential)
        {
            if (question.IsEssential != isEssential)
            {
                askQuestionRepository.SetEssential(question, isEssential);

                string eventOperationType = isEssential ? EventOperationType.Instance().SetEssential() : EventOperationType.Instance().CancelEssential();
                EventBus<AskQuestion>.Instance().OnAfter(question, new CommonEventArgs(eventOperationType));
            }
        }
        public void SetTradeStatus(AskQuestion question, bool tradeStatus)
        {
            question.IsTrated = tradeStatus;
            askQuestionRepository.Update(question);
        }

        /// <summary>
        /// 发布回答
        /// </summary>
        /// <remarks>
        /// 1.每个回答者针对每个问题只能回答一次，创建回答前需做重复检查
        /// 2.注意维护相关问题的回答数，AnswerCount++
        /// 3.注意需要在EventModule中处理动态、积分、通知、自动关注（回答者自动关注该问题）
        /// 4.注意使用AuditService.ChangeAuditStatusForCreate设置审核状态；
        /// 5.注意调用AttachmentService转换临时附件；
        /// 6.需要触发的事件:1)Create的OnBefore、OnAfter；2)审核状态变更；
        /// </remarks>
        /// <param name="answer">回答实体</param>
        /// <returns>是否创建成功</returns>
        public bool CreateAnswer(AskAnswer answer)
        {
            //先查询当前用户是否已经回答了指定问题
            if (GetUserAnswerByQuestionId(answer.UserId, answer.QuestionId) == null)
            {
                EventBus<AskAnswer>.Instance().OnBefore(answer, new CommonEventArgs(EventOperationType.Instance().Create()));

                //设置审核状态
                new AuditService().ChangeAuditStatusForCreate(answer.UserId, answer);

                askAnswerRepository.Insert(answer);

                if (answer.AnswerId > 0)
                {
                    //维护相关问题的内容
                    AskQuestion question = askQuestionRepository.Get(answer.QuestionId);
                    question.LastAnswerUserId = answer.UserId;
                    question.LastAnswerAuthor = answer.Author;
                    question.LastAnswerDate = answer.DateCreated;
                    question.AnswerCount++;

                    //调用Service中的Update方法，以触发相应的事件，但是不更新审核状态
                    this.UpdateQuestion(question, false);

                    //将临时附件转换为正式附件
                    AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().AskAnswer());
                    attachmentService.ToggleTemporaryAttachments(answer.UserId, TenantTypeIds.Instance().AskAnswer(), answer.AnswerId);

                    //用户回答数计数
                    OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                    ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().AnswerCount(), 1);

                    EventBus<AskAnswer>.Instance().OnAfter(answer, new CommonEventArgs(EventOperationType.Instance().Create()));
                    EventBus<AskAnswer, AuditEventArgs>.Instance().OnAfter(answer, new AuditEventArgs(null, answer.AuditStatus));
                }
            }

            return answer.AnswerId > 0;
        }

        /// <summary>
        /// 编辑回答
        /// </summary>
        /// <remarks>
        /// 1.使用AuditService.ChangeAuditStatusForUpdate设置审核状态；
        /// 2.需要触发的事件:Update的OnBefore、OnAfter；
        /// </remarks>
        /// <param name="answer">回答实体</param>
        /// <param name="changeAuditStatusForUpdate">是否更新审核状态</param>
        public void UpdateAnswer(AskAnswer answer, bool changeAuditStatusForUpdate = true)
        {
            EventBus<AskAnswer>.Instance().OnBefore(answer, new CommonEventArgs(EventOperationType.Instance().Update()));
            AuditStatus prevAuditStatus = answer.AuditStatus;

            //设置审核状态
            if (changeAuditStatusForUpdate)
            {
                new AuditService().ChangeAuditStatusForUpdate(answer.UserId, answer);
            }

            askAnswerRepository.Update(answer);

            //更新相关问题的内容
            AskQuestion question = askQuestionRepository.Get(answer.QuestionId);
            question.LastAnswerUserId = answer.UserId;
            question.LastAnswerAuthor = answer.Author;
            question.LastAnswerDate = answer.DateCreated;
            if (answer.IsBest)
            {
                question.Status = QuestionStatus.Resolved;
            }
            //将临时附件转换为正式附件
            AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().AskAnswer());
            attachmentService.ToggleTemporaryAttachments(answer.UserId, TenantTypeIds.Instance().AskAnswer(), answer.AnswerId);
            this.UpdateQuestion(question, false);
            //调用Service中的Update方法，以触发相应的事件，但是不更新审核状态
            EventBus<AskAnswer>.Instance().OnAfter(answer, new CommonEventArgs(EventOperationType.Instance().Update()));
            EventBus<AskAnswer, AuditEventArgs>.Instance().OnAfter(answer, new AuditEventArgs(prevAuditStatus, answer.AuditStatus));


        }

        /// <summary>
        /// 删除回答
        /// </summary>
        /// <remarks>
        /// 1.管理员删除被采纳的回答时，需要将问题状态变更为“待解决”
        /// 2.注意维护相关问题的回答数，调用UpdateAskQuestion，AnswerCount--
        /// 3.注意需要删除动态，通过EventModule处理；
        /// 4.注意需要扣除增加的积分，通过EventModule处理；
        /// 5.注意需要删除回答者对问题的关注，通过EventModule处理；
        /// 6.需要触发的事件:1)Delete的OnBefore、OnAfter；2)审核状态变更
        /// </remarks>
        /// <param name="answer">回答实体</param>
        public void DeleteAnswer(AskAnswer answer)
        {
            EventBus<AskAnswer>.Instance().OnBefore(answer, new CommonEventArgs(EventOperationType.Instance().Delete()));

            askAnswerRepository.Delete(answer);

            //更新相关问题的内容
            AskQuestion question = askQuestionRepository.Get(answer.QuestionId);
            question.AnswerCount--;
            if (answer.IsBest)
            {
                question.Status = QuestionStatus.Unresolved;
            }
            //调用Service中的Update方法，以触发相应的事件，但是不更新审核状态
            this.UpdateQuestion(question, false);

            //用户计数
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
            ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().AnswerCount(), -1);

            EventBus<AskAnswer>.Instance().OnAfter(answer, new CommonEventArgs(EventOperationType.Instance().Delete()));
            EventBus<AskAnswer, AuditEventArgs>.Instance().OnAfter(answer, new AuditEventArgs(answer.AuditStatus, null));
        }

        /// <summary>
        /// 采纳满意回答
        /// </summary>
        /// <remarks>
        /// 1.如果问题有悬赏则悬赏分值转移到回答者的帐户（如有交易税，需要扣除），通过EventModule实现
        /// 2.除了悬赏分的交易，回答被采纳，回答者还会获得新产生的积分，通过EventModule实现
        /// 3.注意需要发送通知给问题的关注者，通过EventModule实现
        /// 4.需要触发的事件:SetBestAnswer的OnAfter
        /// </remarks>
        /// <param name="question">问题实体</param>
        /// <param name="answer">回答实体</param>
        public void SetBestAnswer(AskQuestion question, AskAnswer answer)
        {
            if (!answer.IsBest)
            {
                answer.IsBest = true;

                //调用Service中的Update方法，以触发相应的事件，但是不更新审核状态
                this.UpdateAnswer(answer, false);
                //todo:jiangshl，改为EventModule处理
                //处理威望
                OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());

                //用户获得威望值
                PointService pointService = new PointService();
                //提问者获取威望值
                PointItem questionPointItem = pointService.GetPointItem(PointItemKeys.Instance().Ask_AcceptedAnswer());
                ownerDataService.Change(question.UserId, OwnerDataKeys.Instance().UserAskReputation(), questionPointItem.ReputationPoints);
                //回答者获得威望
                PointItem AnswerPointItem = pointService.GetPointItem(PointItemKeys.Instance().Ask_AnswerWereAccepted());
                ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().UserAskReputation(), AnswerPointItem.ReputationPoints);

                //用户计数
                ownerDataService.Change(answer.UserId, OwnerDataKeys.Instance().AnswerAcceptCount(), 1);
            }
        }

        /// <summary>
        /// 删除用户时处理其问答数据
        /// </summary>
        /// <remarks>
        /// 1.如果需要接管，直接执行Repository的方法更新数据库记录
        /// 2.如果不需要接管，调用DeleteAskQuestion、DeleteAskAnswer循环删除该用户下的所有问题和回答
        /// </remarks>
        /// <param name="userId">原用户id</param>
        /// <param name="takeOverUserName">指定接管用户的用户名</param>
        /// <param name="isTakeOver">是否接管</param>
        public void DeleteUser(long userId, string takeOverUserName, bool isTakeOver)
        {
            if (isTakeOver)
            {
                IUserService userService = DIContainer.Resolve<IUserService>();
                User takeOverUser = userService.GetFullUser(takeOverUserName);
                askQuestionRepository.TakeOver(userId, takeOverUser);
                askAnswerRepository.TakeOver(userId, takeOverUser);
            }
            else
            {
                int pageSize = 100;     //批量删除，每次删100条

                //删除问题
                int pageIndex = 1;
                int pageCount = 1;
                do
                {
                    PagingDataSet<AskQuestion> questions = this.GetUserQuestions(null, userId, true, pageSize, pageIndex);
                    foreach (AskQuestion question in questions)
                    {
                        this.DeleteQuestion(question);
                    }
                    pageCount = questions.PageCount;
                    pageIndex++;
                } while (pageIndex <= pageCount);

                //删除回答
                pageIndex = 1;
                pageCount = 1;
                do
                {
                    PagingDataSet<AskAnswer> answers = this.GetUserAnswers(userId, true, pageSize, pageIndex);
                    foreach (AskAnswer answer in answers)
                    {
                        this.DeleteAnswer(answer);
                    }
                    pageCount = answers.PageCount;
                    pageIndex++;
                } while (pageIndex <= pageCount);
            }
        }

        /// <summary>
        /// 设置用户是否接受定向提问
        /// </summary>
        /// <remarks>
        /// 使用拥有者数据（OwnerData）存储
        /// </remarks>
        /// <param name="userId">用户id</param>
        /// <param name="accept">接受定向提问</param>
        public void SetAcceptQuestion(long userId, bool accept)
        {
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
            ownerDataService.Change(userId, OwnerDataKeys.Instance().AcceptQuestion(), accept.ToString());
        }

        /// <summary>
        /// 获取用户是否接受定向提问
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns>是否接受定向提问</returns>
        public bool IsAcceptQuestion(long userId)
        {
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());

            string accept = ownerDataService.GetString(userId, OwnerDataKeys.Instance().AcceptQuestion());

            return string.IsNullOrEmpty(accept) ? false : bool.Parse(accept);
        }

        /// <summary>
        /// 设置用户在用户在问答中的简介
        /// </summary>
        /// <remarks>
        /// 使用用户数据（OwnerData）存储
        /// </remarks>
        /// <param name="userId">用户id</param>
        /// <param name="description">用户在问答中的简介</param>
        public void SetUserDescription(long userId, string description)
        {
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());

            ownerDataService.Change(userId, OwnerDataKeys.Instance().UserDescription(), description);
        }

        /// <summary>
        /// 获取用户在用户在问答中的简介
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns>用户在问答中的简介</returns>
        public string GetUserDescription(long userId)
        {
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());

            return ownerDataService.GetString(userId, OwnerDataKeys.Instance().UserDescription());
        }

        /// <summary>
        /// 管理员审核问题
        /// </summary>
        /// <remarks>
        /// 1.审核状态未变化不用进行任何操作；
        /// 2.审核状态的变化影响动态的生成与删除、积分的变化
        /// 3.需要触发的事件：1) 批准或不批准；2) 审核状态变更；
        /// </remarks>
        /// <param name="questiongId">问题id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void ApproveQuestion(long questiongId, bool isApproved)
        {
            AskQuestion question = askQuestionRepository.Get(questiongId);
            AuditStatus prevAuditStatus = question.AuditStatus;
            AuditStatus auditStatus = AuditStatus.Fail;
            if (isApproved)
            {
                auditStatus = AuditStatus.Success;
            }
            if (question.AuditStatus != auditStatus)
            {
                question.AuditStatus = auditStatus;

                this.UpdateQuestion(question, false);

                string eventOperationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
                EventBus<AskQuestion>.Instance().OnAfter(question, new CommonEventArgs(eventOperationType));
                EventBus<AskQuestion, AuditEventArgs>.Instance().OnAfter(question, new AuditEventArgs(prevAuditStatus, question.AuditStatus));
            }
        }

        /// <summary>
        /// 管理员审核回答
        /// </summary>
        /// <remarks>
        /// 1.审核状态未变化不用进行任何操作；
        /// 2.审核状态的变化影响动态的生成与删除、积分的变化
        /// 3.需要触发的事件：1) 批准或不批准；2) 审核状态变更；
        /// </remarks>
        /// <param name="answerId">问题id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void ApproveAnswer(long answerId, bool isApproved)
        {
            AskAnswer answer = askAnswerRepository.Get(answerId);
            AuditStatus prevAuditStatus = answer.AuditStatus;
            AuditStatus auditStatus = AuditStatus.Fail;
            if (isApproved)
            {
                auditStatus = AuditStatus.Success;
            }
            if (answer.AuditStatus != auditStatus)
            {
                answer.AuditStatus = auditStatus;

                this.UpdateAnswer(answer, false);

                string eventOperationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
                EventBus<AskAnswer>.Instance().OnAfter(answer, new CommonEventArgs(eventOperationType));
                EventBus<AskAnswer, AuditEventArgs>.Instance().OnAfter(answer, new AuditEventArgs(prevAuditStatus, answer.AuditStatus));
            }
        }

        #endregion

        #region 问答查询与显示

        /// <summary>
        /// 获取感兴趣的问题
        /// </summary>
        /// <remarks>
        /// 1.数据来源：1)关注的标签下问题；2)关注用户提出的问题；
        /// 2.获取时优先获取待解决问题（时间倒序），其次是热门问题（关注数倒序）；
        /// 3.缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="userId">用户id</param>
        /// <param name="topNumber">查询条数</param>
        /// <returns>问题列表</returns>
        public IEnumerable<AskQuestion> GetTopInterestedQuestions(string tenantTypeId, long userId, int topNumber)
        {
            return askQuestionRepository.GetTopInterestedQuestions(tenantTypeId, userId, topNumber);
        }

        /// <summary>
        /// 根据不同条件获取前n条的问题列表（用于前台）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="status">问题状态</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序条件</param>
        /// <returns>问题列表</returns>
        public IEnumerable<AskQuestion> GetTopQuestions(string tenantTypeId, int topNumber, QuestionStatus? status, bool? isEssential, SortBy_AskQuestion sortBy)
        {
            return askQuestionRepository.GetTopQuestions(tenantTypeId, topNumber, status, isEssential, sortBy);
        }

        /// <summary>
        /// 根据不同条件分页获取问题列表（用于前台）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="tagName">标签</param>
        /// <param name="status">问题状态</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public PagingDataSet<AskQuestion> GetQuestions(string tenantTypeId, string tagName, QuestionStatus? status, bool? isEssential, SortBy_AskQuestion sortBy, int pageSize, int pageIndex)
        {
            return askQuestionRepository.GetQuestions(tenantTypeId, tagName, status, isEssential, sortBy, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据问题id列表批量获取问题实体
        /// </summary>
        /// <param name="questionIds">问题id列表</param>
        /// <returns>问题实体列表</returns>
        public IEnumerable<AskQuestion> GetQuestions(IEnumerable<long> questionIds)
        {
            return askQuestionRepository.PopulateEntitiesByEntityIds(questionIds);
        }

        /// <summary>
        /// 分页获取拥有者的问题列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public PagingDataSet<AskQuestion> GetOwnerQuestions(string tenantTypeId, long ownerId, bool ignoreAudit, int pageSize, int pageIndex)
        {
            return askQuestionRepository.GetOwnerQuestions(tenantTypeId, ownerId, ignoreAudit, pageSize, pageIndex);
        }

        /// <summary>
        /// 分页获取用户的问题列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="userId">问题的作者用户id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public PagingDataSet<AskQuestion> GetUserQuestions(string tenantTypeId, long userId, bool ignoreAudit, int pageSize, int pageIndex)
        {
            return askQuestionRepository.GetUserQuestions(tenantTypeId, userId, ignoreAudit, pageSize, pageIndex);
        }

        /// <summary>
        /// 分页获取用户的回答列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="userId">回答的作者用户id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        public PagingDataSet<AskAnswer> GetUserAnswers(long userId, bool ignoreAudit, int pageSize, int pageIndex)
        {
            return askAnswerRepository.GetUserAnswers(userId, ignoreAudit, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取零回答的问题（用于前台）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="tagName">标签</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public PagingDataSet<AskQuestion> GetNoAnswerQuestions(string tenantTypeId, string tagName, int pageSize, int pageIndex)
        {
            return askQuestionRepository.GetNoAnswerQuestions(tenantTypeId, tagName, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据id获取问题实体
        /// </summary>
        /// <param name="questionId">问题id</param>
        /// <returns>问题实体</returns>
        public AskQuestion GetQuestion(long questionId)
        {
            return askQuestionRepository.Get(questionId);
        }

        /// <summary>
        /// 根据id获取答案实体
        /// </summary>
        /// <param name="answerId">问题id</param>
        /// <returns>问题实体</returns>
        public AskAnswer GetAnswer(long answerId)
        {
            return askAnswerRepository.Get(answerId);
        }

        /// <summary>
        /// 根据用户id和问题id获取回答实体
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="questionId">问题id</param>
        /// <returns>问题实体</returns>
        public AskAnswer GetUserAnswerByQuestionId(long userId, long questionId)
        {
            return askAnswerRepository.GetUserAnswerByQuestionId(userId, questionId);
        }

        /// <summary>
        /// 根据问题id获取回答实体列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="questionId">问题id</param>
        /// <param name="sortBy">排序条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        public PagingDataSet<AskAnswer> GetAnswersByQuestionId(long questionId, SortBy_AskAnswer sortBy, int pageSize, int pageIndex)
        {
            return askAnswerRepository.GetAnswersByQuestionId(questionId, sortBy, pageSize, pageIndex);
        }

        /// <summary>
        /// 管理员获取问题分页集合
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="questionStatus">问题状态</param>
        /// <param name="ownerId">所属id</param>
        /// <param name="userId">用户id</param>
        /// <param name="keyword">查询关键字</param>
        /// <param name="tagName">标签</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        public PagingDataSet<AskQuestion> GetQuestionsForAdmin(string tenantTypeId, AuditStatus? auditStatus, QuestionStatus? questionStatus, long? ownerId, long? userId, string keyword, string tagName, int pageSize, int pageIndex)
        {
            return askQuestionRepository.GetQuestionsForAdmin(tenantTypeId, auditStatus, questionStatus, ownerId, userId, keyword, tagName, pageSize, pageIndex);
        }

        /// <summary>
        /// 管理员获取回答分页集合
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="userId">用户id</param>
        /// <param name="keyword">查询关键字</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        public PagingDataSet<AskAnswer> GetAnswersForAdmin(AuditStatus? auditStatus, long? userId, string keyword, int pageSize, int pageIndex)
        {
            return askAnswerRepository.GetAnswersForAdmin(auditStatus, userId, keyword, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取问题管理数据
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>问题管理数据</returns>
        public Dictionary<string, long> GetQuestionManageableData(string tenantTypeId = null)
        {
            return askQuestionRepository.GetManageableData(tenantTypeId);
        }

        /// <summary>
        /// 获取回答管理数据
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <returns>回答管理数据</returns>
        public Dictionary<string, long> GetAnswerManageableData()
        {
            return askAnswerRepository.GetManageableData();
        }

        /// <summary>
        /// 获取问题应用统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>问题统计数据</returns>
        public Dictionary<string, long> GetQuestionApplicationStatisticData(string tenantTypeId = null)
        {
            return askQuestionRepository.GetApplicationStatisticData(tenantTypeId);
        }

        /// <summary>
        /// 获取回答应用统计数据
        /// </summary>
        /// <returns>回答统计数据</returns>
        public Dictionary<string, long> GetAnswerApplicationStatisticData()
        {
            return askAnswerRepository.GetApplicationStatisticData();
        }

        /// <summary>
        /// 获取问答用户统计数据
        /// </summary>
        /// <remarks>
        /// 需统计：问题数、回答数、回答被采纳数、赞同数
        /// 无需缓存
        /// </remarks>
        /// <param name="userId">用户id</param>
        /// <returns>用户统计数据</returns>
        public Dictionary<string, long> GetUserStatisticData(long userId)
        {
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());

            Dictionary<string, long> userStatisticData = new Dictionary<string, long>();

            userStatisticData.Add(OwnerDataKeys.Instance().QuestionCount(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().QuestionCount()));
            userStatisticData.Add(OwnerDataKeys.Instance().AnswerCount(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().AnswerCount()));
            userStatisticData.Add(OwnerDataKeys.Instance().AnswerAcceptCount(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().AnswerAcceptCount()));
            userStatisticData.Add(OwnerDataKeys.Instance().AnswerSupportCount(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().AnswerSupportCount()));
            userStatisticData.Add(OwnerDataKeys.Instance().UserAskReputation(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().UserAskReputation()));

            return userStatisticData;
        }

        /// <summary>
        /// 统计某个标签的回答数
        /// </summary>
        /// <param name="tagName">标签名</param>
        /// <returns>回答数</returns>
        public long GetAnswerCountOfTag(string tagName)
        {
            return askAnswerRepository.GetAnswerCountOfTag(tagName);
        }

        /// <summary>
        /// 通过问题Id获取该问题下所有回答的顶踩数
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public int GetAnswersAttitudesCount(long questionId, string tenantTypeId)
        {
            int? number = askQuestionRepository.GetAnswersAttitudesCount(questionId, tenantTypeId);
            if (number == null)
            {
                return 0;
            }
            return (int)number;
        }

        #endregion
    }
}