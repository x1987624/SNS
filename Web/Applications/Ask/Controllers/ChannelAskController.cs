//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;
using Tunynet.Utilities;
using Spacebuilder.Search;
using Tunynet.Search;
using DevTrends.MvcDonutCaching;

namespace Spacebuilder.Ask.Controllers
{
    /// <summary>
    /// 问答控制器
    /// </summary>
    [TitleFilter(TitlePart = "问答", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.Channel, IsApplication = true)]
    [AnonymousBrowseCheck]
    public class ChannelAskController : Controller
    {
        public Authorizer authorizer { get; set; }
        public IPageResourceManager pageResourceManager { get; set; }
        public AskService askService { get; set; }
        public UserService userService { get; set; }
        public SearchHistoryService searchHistoryService { get; set; }
        public SearchedTermService searchedTermService { get; set; }
        public PointService pointService { get; set; }
        private SubscribeService subscribeQuestionService =new SubscribeService(TenantTypeIds.Instance().AskQuestion());
        private TagService tagService = new TagService(TenantTypeIds.Instance().AskQuestion());
        private SubscribeService subscribeTagService = new SubscribeService(TenantTypeIds.Instance().AskTag());
        private CountService countService = new CountService(TenantTypeIds.Instance().AskQuestion());
        private OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());


        #region 侧边栏局部页面

        /// <summary>
        /// 侧边栏-问题状态页
        /// </summary>
        public ActionResult _QuestionStatus(long questionId)
        {
            AskQuestion question = askService.GetQuestion(questionId);

            IEnumerable<long> userIds = subscribeQuestionService.GetTopUserIdsOfObject(questionId, 28);
            ViewData["followers"] = userService.GetUsers(userIds);
            ViewData["followerCount"] = subscribeQuestionService.GetSubscribedUserCount(questionId);

            return View(question);
        }

        /// <summary>
        /// 侧边栏-相关问题页
        /// </summary>
        [DonutOutputCache(CacheProfile = "Usual")]
        public ActionResult _RelevantQuestions(long questionId)
        {
            AskQuestion question = askService.GetQuestion(questionId);

            List<string> keywords = new List<string>();

            //对标题分词
            foreach (var item in ClauseScrubber.TitleToKeywords(question.Subject))
            {
                keywords.Add(item);
            }
            //取标签名
            foreach (var item in question.Tags.Select(n => n.TagName))
            {
                keywords.Add(item);
            }

            //调用搜索器进行搜索
            AskSearcher askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
            AskFullTextQuery query = new AskFullTextQuery();
            query.PageSize = 20;
            query.Keywords = keywords;
            query.IsRelation = true;
            query.sortBy = SortBy_AskQuestion.DateCreated_Desc;
            IEnumerable<AskQuestion> askQuestions = askSearcher.Search(query).Where(n => n.QuestionId != questionId);

            return View(askQuestions);
        }

        /// <summary>
        /// 侧边栏-相似问题页
        /// </summary>
        public ActionResult _SimilarQuestions(string subject)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return View();
            }

            //对标题分词
            string[] keywords = ClauseScrubber.TitleToKeywords(subject);
            //调用搜索器进行搜索
            AskSearcher askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
            AskFullTextQuery query = new AskFullTextQuery();
            query.PageSize = 10;
            query.Keywords = keywords;
            query.IsAlike = true;
            query.sortBy = SortBy_AskQuestion.DateCreated_Desc;
            PagingDataSet<AskQuestion> askQuestions = askSearcher.Search(query);

            return View(askQuestions.AsEnumerable());
        }

        /// <summary>
        /// 侧边栏-相关标签
        /// </summary>
        /// <param name="tagId">标签ID</param>
        /// <returns></returns>
        public ActionResult _RelatedTags(long tagId)
        {
            IEnumerable<Tag> tags = tagService.GetRelatedTags(tagId);

            return View(tags);
        }

        /// <summary>
        /// 侧边栏-关注标签的用户
        /// </summary>
        public ActionResult _TagFollowersSide(long tagId, int topNumber = 28)
        {
            ViewData["tagId"] = tagId;

            IEnumerable<long> userIds = subscribeTagService.GetTopUserIdsOfObject(tagId, topNumber);
            IEnumerable<IUser> users = null;
            if (userIds != null && userIds.Count() > 0)
            {
                users = userService.GetUsers(userIds.Take(topNumber));
            }

            ViewData["followerCount"] = subscribeTagService.GetSubscribedUserCount(tagId);

            return View(users);
        }
        #endregion

        #region 问答列表及详情页

        /// <summary>
        /// 问答首页
        /// </summary>
        public ActionResult Home()
        {
            pageResourceManager.InsertTitlePart("问答首页");
            IUser currentUser = UserContext.CurrentUser;

            //感兴趣的问题
            if (currentUser != null)
            {
                IEnumerable<AskQuestion> interestedQuestions = askService.GetTopInterestedQuestions(TenantTypeIds.Instance().User(), currentUser.UserId, 6);
                ViewData["interestedQuestions"] = interestedQuestions;
            }

            //待解决的问题
            IEnumerable<AskQuestion> unresolvedQuestions = askService.GetTopQuestions(TenantTypeIds.Instance().User(), 6, QuestionStatus.Unresolved, null, SortBy_AskQuestion.DateCreated_Desc);
            ViewData["unresolvedQuestions"] = unresolvedQuestions;

            //精华问题
            IEnumerable<AskQuestion> essentialQuestions = askService.GetTopQuestions(TenantTypeIds.Instance().User(), 6, null, true, SortBy_AskQuestion.DateCreated_Desc);
            ViewData["essentialQuestions"] = essentialQuestions;

            return View();
        }

        /// <summary>
        /// 问题详情页
        /// </summary>
        public ActionResult QuestionDetail(long questionId)
        {
            AskQuestion askQuestion = askService.GetQuestion(questionId);
            if (askQuestion == null || askQuestion.User == null)
            {
                return HttpNotFound();
            }

            //验证是否通过审核
            long currentSpaceUserId = UserIdToUserNameDictionary.GetUserId(askQuestion.User.UserName);
            if (!authorizer.IsAdministrator(AskConfig.Instance().ApplicationId) && askQuestion.UserId != currentSpaceUserId
                && (int)askQuestion.AuditStatus < (int)(new AuditService().GetPubliclyAuditStatus(AskConfig.Instance().ApplicationId)))
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Title = "尚未通过审核",
                    Body = "由于当前问题尚未通过审核，您无法浏览当前内容。",
                    StatusMessageType = StatusMessageType.Hint
                }));

            AttachmentService<Attachment> attachmentService = new AttachmentService<Attachment>(TenantTypeIds.Instance().AskQuestion());
            //附件信息
            IEnumerable<Attachment> attachments = attachmentService.GetsByAssociateId(questionId);
            if (attachments != null && attachments.Count() > 0)
            {
                ViewData["attachments"] = attachments.Where(n => n.MediaType == MediaType.Other);
            }

            //更新浏览计数
            CountService countService = new CountService(TenantTypeIds.Instance().AskQuestion());
            countService.ChangeCount(CountTypes.Instance().HitTimes(), askQuestion.QuestionId, askQuestion.UserId, 1, false);

            //设置SEO信息
            pageResourceManager.InsertTitlePart(askQuestion.Subject);

            List<string> keywords = new List<string>();
            keywords.AddRange(askQuestion.Tags.Select(n => n.DisplayName));
            string keyword = string.Join(" ", keywords.Distinct());
            keyword += " " + string.Join(" ", ClauseScrubber.TitleToKeywords(askQuestion.Subject));
            pageResourceManager.SetMetaOfKeywords(keyword);

            if (!string.IsNullOrEmpty(askQuestion.Body))
            {
                pageResourceManager.SetMetaOfDescription(HtmlUtility.StripHtml(askQuestion.Body, true, false));
            }

            return View(askQuestion);
        }

        /// <summary>
        /// 问题标签详情页
        /// </summary>
        public ActionResult TagDetail(string tagName)
        {
            var tag = tagService.Get(tagName);

            if (tag == null)
            {
                return HttpNotFound();
            }

            //设置title和meta
            pageResourceManager.InsertTitlePart(tag.TagName);
            if (!string.IsNullOrEmpty(tag.Description))
            {
                pageResourceManager.SetMetaOfDescription(HtmlUtility.TrimHtml(tag.Description, 100));
            }
            return View(tag);
        }

        /// <summary>
        /// 某个问题的回答列表
        /// </summary>
        public ActionResult _AnswerList(long questionId, SortBy_AskAnswer sortBy = SortBy_AskAnswer.DateCreated_Desc, long answerId = 0, int pageIndex = 1, int pageSize = 7)
        {
            AskQuestion question = askService.GetQuestion(questionId);
            if (question == null || question.Status == QuestionStatus.Canceled)
            {
                return HttpNotFound();
            }

            PagingDataSet<AskAnswer> askAnswers = askService.GetAnswersByQuestionId(questionId, sortBy, pageSize, pageIndex);
            ViewData["question"] = question;
            ViewData["sortBy"] = sortBy;
            ViewData["answerId"] = answerId;

            AttachmentService<Attachment> attachmentService = new AttachmentService<Attachment>(TenantTypeIds.Instance().AskAnswer());
            //附件信息
            Dictionary<long, IEnumerable<Attachment>> attachmentsDic = new Dictionary<long, IEnumerable<Attachment>>();
            foreach (var askAnswer in askAnswers)
            {
                IEnumerable<Attachment> attachments = attachmentService.GetsByAssociateId(askAnswer.AnswerId);
                if (attachments != null && attachments.Count() > 0)
                {
                    attachmentsDic.Add(askAnswer.AnswerId, attachments.Where(n => n.MediaType == MediaType.Other));
                }
            }
            ViewData["attachmentsDic"] = attachmentsDic;

            return View(askAnswers);
        }

        /// <summary>
        /// 他的问答/我的问答
        /// </summary>
        public ActionResult AskUser(string spaceKey)
        {
            IUser user = null;
            if (string.IsNullOrEmpty(spaceKey))
            {
                user = UserContext.CurrentUser;
                if (user == null)
                {
                    return Redirect(SiteUrls.Instance().Login(true));
                }
                pageResourceManager.InsertTitlePart("我的问答");
            }
            else
            {
                user = userService.GetFullUser(spaceKey);
                if (user == null)
                {
                    return HttpNotFound();
                }

                if (!new PrivacyService().Validate(user.UserId, UserContext.CurrentUser != null ? UserContext.CurrentUser.UserId : 0, PrivacyItemKeys.Instance().VisitUserSpace()))
                {
                    if (UserContext.CurrentUser == null)
                        return Redirect(SiteUrls.Instance().Login(true));
                    else
                        return Redirect(SiteUrls.Instance().PrivacyHome(user.UserName));
                }

                if (UserContext.CurrentUser != null && user.UserId == UserContext.CurrentUser.UserId)
                {
                    pageResourceManager.InsertTitlePart("我的问答");
                }
                else
                {
                    pageResourceManager.InsertTitlePart(user.DisplayName + "的问答");
                }
            }

            //是否允许被别人提问
            bool isAcceptQuestion = askService.IsAcceptQuestion(user.UserId);
            ViewData["isAcceptQuestion"] = isAcceptQuestion;

            string userDescription = askService.GetUserDescription(user.UserId);
            ViewData["userDescription"] = userDescription;

            return View(user);
        }

        /// <summary>
        /// 他的回答/我的回答
        /// </summary>
        public ActionResult _UserAnswers(long userId, int pageSize = 10, int pageIndex = 1)
        {
            IUser currentUser = UserContext.CurrentUser;
            PagingDataSet<AskAnswer> answers = null;
            if (currentUser != null && currentUser.UserId == userId)
            {
                answers = askService.GetUserAnswers(currentUser.UserId, true, pageSize, pageIndex);
            }
            else
            {
                answers = askService.GetUserAnswers(userId, false, pageSize, pageIndex);
            }

            return View(answers);
        }

        /// <summary>
        /// 他的问题/我的问题
        /// </summary>
        public ActionResult _UserQuestions(long userId, int pageSize = 10, int pageIndex = 1)
        {
            IUser currentUser = UserContext.CurrentUser;
            PagingDataSet<AskQuestion> questions = null;

            if (currentUser != null && currentUser.UserId == userId)
            {
                questions = askService.GetUserQuestions(TenantTypeIds.Instance().User(), currentUser.UserId, true, pageSize, pageIndex);
            }
            else
            {
                questions = askService.GetUserQuestions(TenantTypeIds.Instance().User(), userId, false, pageSize, pageIndex);
            }

            return View(questions);
        }

        /// <summary>
        /// 他关注的问题/我关注的问题
        /// </summary>
        public ActionResult _UserFollowedQuestions(long userId, int pageSize = 10, int pageIndex = 1)
        {
            PagingDataSet<long> followedquestionIds = subscribeQuestionService.GetPagingObjectIds(userId, pageIndex, pageSize);
            IEnumerable<AskQuestion> followedQuestions = askService.GetQuestions(followedquestionIds);
            PagingDataSet<AskQuestion> followedQuestionsList = new PagingDataSet<AskQuestion>(followedQuestions)
            {
                TotalRecords = followedquestionIds.TotalRecords,
                PageSize = followedquestionIds.PageSize,
                PageIndex = followedquestionIds.PageIndex,
                QueryDuration = followedquestionIds.QueryDuration
            };

            return View(followedQuestionsList);
        }

        /// <summary>
        /// 他关注的标签/我关注的标签
        /// </summary>
        public ActionResult _UserFollowedTags(long userId, int pageSize = 10, int pageIndex = 1)
        {
            PagingDataSet<long> tagIds = subscribeTagService.GetPagingObjectIds(userId, pageIndex, pageSize);
            IEnumerable<Tag> tags = tagService.GetTags(tagIds);

            //标签下的回答数
            Dictionary<long, long> answerCountDic = new Dictionary<long, long>();
            foreach (Tag tag in tags)
            {
                answerCountDic.Add(tag.TagId, askService.GetAnswerCountOfTag(tag.TagName));
            }
            ViewData["answerCountDic"] = answerCountDic;

            PagingDataSet<Tag> TagsList = new PagingDataSet<Tag>(tags)
            {
                TotalRecords = tagIds.TotalRecords,
                PageSize = tagIds.PageSize,
                PageIndex = tagIds.PageIndex,
                QueryDuration = tagIds.QueryDuration
            };

            return View(TagsList);
        }

        #endregion

        #region 问答增删改

        /// <summary>
        /// 发布问题/编辑问题(页面)
        /// </summary>
        [HttpGet]
        public ActionResult EditQuestion(long? questionId, long? ownerId)
        {
            AskQuestionEditModel model = null;

            //编辑问题_AnswerEdit_AnswerEdit
            if (questionId.HasValue && questionId.Value > 0)
            {
                AskQuestion askQuestion = null;
                askQuestion = askService.GetQuestion(questionId.Value);

                if (!authorizer.Question_Edit(askQuestion))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "没有权限",
                        Body = "没有权限编辑问题",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }

                if (askQuestion == null)
                {
                    return HttpNotFound();
                }
                model = askQuestion.AsEditModel();

                pageResourceManager.InsertTitlePart("编辑问题");
            }
            //添加问题
            else
            {
                string errorMessage = string.Empty;
                if (!authorizer.Question_Create(out errorMessage))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "没有权限",
                        Body = errorMessage,
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }

                model = new AskQuestionEditModel();
                if (ownerId.HasValue)
                {
                    model.OwnerId = ownerId;
                }

                pageResourceManager.InsertTitlePart("提问");
            }

            //悬赏下拉框
            ViewData["askRewards"] = GetRewardList(model.Reward);

            return View(model);
        }

        /// <summary>
        /// 发布问题/编辑问题(提交表单)
        /// </summary>
        [HttpPost]
        public ActionResult EditQuestion(AskQuestionEditModel model)
        {
            string errorMessage = null;
            if (ModelState.HasBannedWord(out errorMessage))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, errorMessage));
            }

            AskQuestion askQuestion = model.AsAskQuestion();

            //发布问答
            if (model.QuestionId == 0)
            {
                if (!authorizer.Question_Create(out errorMessage))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "没有权限",
                        Body = errorMessage,
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }

                bool isCreated = askService.CreateQuestion(askQuestion);

                if (!isCreated)
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "发布失败",
                        Body = "发布失败，请稍后再试！",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }
            }
            //编辑问答
            else
            {
                if (!authorizer.Question_Edit(askQuestion))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "没有权限",
                        Body = "没有权限编辑问题",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }

                askService.UpdateQuestion(askQuestion);

                //清除标签
                tagService.ClearTagsFromItem(askQuestion.QuestionId, askQuestion.UserId);
            }

            //设置标签
            string tagNames = string.Join(",", model.TagNames);
            if (!string.IsNullOrEmpty(tagNames))
            {
                tagService.AddTagsToItem(tagNames, askQuestion.UserId, askQuestion.QuestionId);
            }

            return Redirect(SiteUrls.Instance().AskQuestionDetail(askQuestion.QuestionId));
        }

        /// <summary>
        /// 关注问题按钮
        /// </summary>
        public ActionResult _SubscribeQuestionButton(long questionId)
        {
            if (UserContext.CurrentUser != null)
            {
                ViewData["isSubscribed"] = subscribeQuestionService.IsSubscribed(questionId, UserContext.CurrentUser.UserId);
            }

            ViewData["questionId"] = questionId;
            return View();
        }

        /// <summary>
        /// 关注问题操作
        /// </summary>
        [HttpPost]
        public JsonResult _SubscribeQuestion(long questionId)
        {
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "必须先登录，才能继续操作"));
            }

            AskQuestion askQuestion = askService.GetQuestion(questionId);

            if (askQuestion == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到要被关注的问题"));
            }

            if (subscribeQuestionService.IsSubscribed(questionId, currentUser.UserId))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "您已经关注过该问题"));
            }

            subscribeQuestionService.Subscribe(questionId, currentUser.UserId);

            //问题关注数计数，用于“可能感兴趣的问题”关联表查询
            countService.ChangeCount(CountTypes.Instance().QuestionFollowerCount(), askQuestion.QuestionId, askQuestion.UserId, 1, false);

            return Json(new StatusMessageData(StatusMessageType.Success, "关注成功"));
        }

        /// <summary>
        /// 取消关注问题操作
        /// </summary>
        [HttpPost]
        public JsonResult _SubscribeQuestionCancel(long questionId)
        {
            if (UserContext.CurrentUser == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "必须先登录，才能继续操作"));
            }

            long userId = UserContext.CurrentUser.UserId;

            if (!subscribeQuestionService.IsSubscribed(questionId, userId))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "您没有关注过该问题"));
            }

            AskQuestion askQuestion = askService.GetQuestion(questionId);

            if (askQuestion == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到要被关注的问题"));
            }

            subscribeQuestionService.CancelSubscribe(questionId, userId);

            //问题关注数计数，用于“可能感兴趣的问题”关联表查询
            countService.ChangeCount(CountTypes.Instance().QuestionFollowerCount(), askQuestion.QuestionId, askQuestion.UserId, -1, false);

            return Json(new StatusMessageData(StatusMessageType.Success, "取消关注操作成功"));
        }

        /// <summary>
        /// 批量设置/取消精华
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="isEssential">是否精华，true为加精，false为取消精华</param>
        [HttpPost]
        public ActionResult _SetEssential(long questionId, bool isEssential)
        {
            if (!authorizer.Question_SetEssential())
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "没有权限设置精华"));
            }

            AskQuestion askQuestion = askService.GetQuestion(questionId);
            askService.SetEssential(askQuestion, isEssential);

            return Json(new StatusMessageData(StatusMessageType.Success, isEssential ? "设置精华成功" : "取消精华成功"));
        }

        /// <summary>
        /// 取消问题
        /// </summary>
        /// <param name="questionId">问题Id</param>
        [HttpPost]
        public ActionResult _Cancel(long questionId)
        {
            AskQuestion askQuestion = askService.GetQuestion(questionId);

            if (!authorizer.Question_Edit(askQuestion))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "没有权限取消问题"));
            }
            askService.CancelQuestion(askQuestion);

            return Json(new StatusMessageData(StatusMessageType.Success, "取消成功"));
        }

        /// <summary>
        /// 删除回答
        /// </summary>
        [HttpPost]
        public ActionResult _DeleteAnswer(long answerId)
        {
            AskAnswer askAnswer = askService.GetAnswer(answerId);

            if (!authorizer.Answer_Delete())
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "没有权限删除回答"));
            }
            askService.DeleteAnswer(askAnswer);

            return Json(new StatusMessageData(StatusMessageType.Success, "删除成功"));
        }

        /// <summary>
        /// 采纳为满意回答
        /// </summary>
        [HttpPost]
        public ActionResult _SetBestAnswer(long answerId)
        {
            AskAnswer askAnswer = askService.GetAnswer(answerId);
            AskQuestion askQuestion = askService.GetQuestion(askAnswer.QuestionId);

            if (!authorizer.Question_Edit(askQuestion))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "没有权限采纳满意回答"));
            }

            askService.SetBestAnswer(askQuestion, askAnswer);
            return Json(new StatusMessageData(StatusMessageType.Success, "采纳成功"));
        }

        /// <summary>
        /// 我来回答局部页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _AnswerEdit(long questionId)
        {
            AskAnswerEditModel askAnswerEditModel = new AskAnswerEditModel();
            askAnswerEditModel.QuestionId = questionId;

            return View(askAnswerEditModel);
        }

        /// <summary>
        /// 发布回答/编辑回答(提交表单)
        /// </summary>
        [HttpPost]
        public ActionResult _AnswerEdit(AskAnswerEditModel model)
        {

            string errorMessage = null;
            if (ModelState.HasBannedWord(out errorMessage))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, errorMessage));
            }

            AskAnswer answer = model.AsAskAnswer();
            AskQuestion question = askService.GetQuestion(answer.QuestionId);
            //发布回答
            if (model.AnswerId == 0)
            {
                if (!authorizer.Answer_Create(question, out errorMessage))
                {
                    return Json(new StatusMessageData(StatusMessageType.Error, errorMessage));
                }

                bool isCreated = askService.CreateAnswer(answer);
                if (!isCreated)
                {
                    return Json(new StatusMessageData(StatusMessageType.Error, "操作失败，请稍后再试！"));
                }
                return Json(new StatusMessageData(StatusMessageType.Success, "回答成功！"));
            }
            //编辑回答
            else
            {
                if (!authorizer.Answer_Edit(question, answer))
                {
                    return Json(new StatusMessageData(StatusMessageType.Error, "没有权限编辑回答"));
                }
                askService.UpdateAnswer(answer);
                return Json(new StatusMessageData(StatusMessageType.Success, "编辑成功！"));
            }
        }

        /// <summary>
        /// 关注标签按钮
        /// </summary>
        public ActionResult _SubscribeTagButton(long tagId)
        {
            if (UserContext.CurrentUser != null)
            {
                ViewData["isSubscribed"] = subscribeTagService.IsSubscribed(tagId, UserContext.CurrentUser.UserId);
            }

            ViewData["tagId"] = tagId;

            return View();
        }

        /// <summary>
        /// 关注标签操作
        /// </summary>
        [HttpPost]
        public JsonResult _SubscribeTag(long tagId)
        {
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "必须先登录，才能继续操作"));
            }

            var getTag = tagService.Get(tagId);

            if (getTag == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到要被关注的标签"));
            }

            if (subscribeTagService.IsSubscribed(tagId, currentUser.UserId))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "您已经关注过该标签"));
            }

            subscribeTagService.Subscribe(tagId, currentUser.UserId);

            return Json(new StatusMessageData(StatusMessageType.Success, "关注成功"));
        }

        /// <summary>
        /// 取消关注标签操作
        /// </summary>
        [HttpPost]
        public JsonResult _SubscribeTagCancel(long tagId)
        {
            if (UserContext.CurrentUser == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "必须先登录，才能继续操作"));
            }

            long userId = UserContext.CurrentUser.UserId;

            if (!subscribeTagService.IsSubscribed(tagId, userId))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "您没有关注过该标签"));
            }

            var getTag = tagService.Get(tagId);

            if (getTag == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到要被关注的标签"));
            }

            subscribeTagService.CancelSubscribe(tagId, userId);

            return Json(new StatusMessageData(StatusMessageType.Success, "取消关注操作成功"));
        }



        /// <summary>
        /// 侧边栏-他关注的标签/我关注的标签
        /// </summary>
        public ActionResult _UserFollowedTagsSide(long userId, int topNum = 6, string ta = "我")
        {
            IEnumerable<long> tagIds = subscribeTagService.GetTopObjectIds(userId, topNum);
            IEnumerable<Tag> tags = tagService.GetTags(tagIds);

            ViewData["ta"] = ta;

            return View(tags);
        }

        /// <summary>
        /// 侧边栏-他的成就/我的成就
        /// </summary>
        public ActionResult _UserAchievement(long userId, string ta = "我")
        {
            Dictionary<string, long> userAchievement = askService.GetUserStatisticData(userId);

            ViewData["ta"] = ta;

            return View(userAchievement);
        }

        /// <summary>
        /// 我的设置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _AskUserSettings(long userId)
        {
            bool isAcceptQuestion = askService.IsAcceptQuestion(userId);
            ViewData["userId"] = userId;
            ViewData["isAcceptQuestion"] = isAcceptQuestion;

            return View();
        }

        /// <summary>
        /// 我的设置提交
        /// </summary>
        /// <param name="accept">是否允许向他提问</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _AskUserSettings(long userId, bool accept = false)
        {
            if (authorizer.Ask_UserSetting(userId))
            {
                askService.SetAcceptQuestion(userId, accept);
                return Json(new StatusMessageData(StatusMessageType.Success, "设置成功！"));
            }
            else
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "无权操作！"));
            }
        }

        /// <summary>
        /// 向他提问
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _AskTa(long userId)
        {
            string errorMessage = string.Empty;
            if (!authorizer.Question_Create(out errorMessage) || !askService.IsAcceptQuestion(userId))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Title = "没有权限",
                    Body = errorMessage,
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            IUser user = userService.GetUser(userId);
            ViewData["user"] = user;
            ViewData["askRewards"] = GetRewardList(0);

            return View(new AskQuestionEditModel());
        }

        /// <summary>
        /// //向他提问
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _AskTa(AskQuestionEditModel model)
        {
            string errorMessage = string.Empty;
            if (!authorizer.Question_Create(out errorMessage) || !askService.IsAcceptQuestion(model.AskUserId))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, errorMessage));
            }

            bool isCreated = false;
            IUser currentUser = UserContext.CurrentUser;
            long userId = model.AskUserId;

            if (currentUser != null && currentUser.UserId != userId)
            {
                AskQuestion askQuestion = model.AsAskQuestion();
                isCreated = askService.CreateQuestion(askQuestion);
                if (isCreated)
                {
                    string tagNames = string.Join(",", model.TagNames);
                    if (!string.IsNullOrEmpty(tagNames))
                    {
                        tagService.AddTagsToItem(tagNames, askQuestion.UserId, askQuestion.QuestionId);
                    }
                }
            }

            if (isCreated)
            {
                return Json(new StatusMessageData(StatusMessageType.Success, "提问成功！"));
            }
            else
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "操作失败，请稍后再试！"));
            }
        }

        /// <summary>
        /// 保存用户简介
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _SaveUserDescription(long userId, string description)
        {
            string errorMessage = null;
            if (ModelState.HasBannedWord(out errorMessage))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, errorMessage));
            }

            if (authorizer.Ask_UserSetting(userId))
            {
                if (description.Length > 200)
                {
                    return Json("1");
                }
                askService.SetUserDescription(userId, description);
                return Json(new { description = description });
            }
            else
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "无权操作！"));
            }
        }

        #endregion

        #region 问答全文检索

        /// <summary>
        /// 问答搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public ActionResult Search(AskFullTextQuery query)
        {
            query.PageSize = 20;
            //调用搜索器进行搜索
            AskSearcher askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
            PagingDataSet<AskQuestion> askQuestions = askSearcher.Search(query);

            //添加到用户搜索历史
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Keyword))
                {
                    searchHistoryService.SearchTerm(currentUser.UserId, AskSearcher.CODE, query.Keyword);
                }
            }

            //添加到热词
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                searchedTermService.SearchTerm(AskSearcher.CODE, query.Keyword);
            }

            //设置页面Meta
            if (string.IsNullOrWhiteSpace(query.Keyword))
            {
                pageResourceManager.InsertTitlePart("问答搜索");
            }
            else
            {
                pageResourceManager.InsertTitlePart(query.Keyword + "的相关问题");
            }

            return View(askQuestions);
        }

        /// <summary>
        /// 问答全局搜索
        /// </summary>
        /// <param name="query"></param>
        /// <param name="topNumber"></param>
        /// <returns></returns>
        public ActionResult _GlobalSearch(AskFullTextQuery query, int topNumber)
        {
            query.Keyword = WebUtility.UrlDecode(query.Keyword);
            query.PageIndex = 1;
            query.PageSize = topNumber;
            AskSearcher askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
            PagingDataSet<AskQuestion> questions = askSearcher.Search(query);

            return PartialView(questions);
        }

        /// <summary>
        /// 问答快捷搜索
        /// </summary>
        /// <param name="query"></param>
        /// <param name="topNumber"></param>
        /// <returns></returns>
        public ActionResult _QuickSearch(AskFullTextQuery query, int topNumber)
        {
            query.PageIndex = 1;
            query.PageSize = topNumber;
            query.Range = AskSearchRange.SUBJECT;
            query.Keyword = WebUtility.UrlDecode(query.Keyword);

            AskSearcher askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
            PagingDataSet<AskQuestion> questions = askSearcher.Search(query);

            return PartialView(questions);
        }

        /// <summary>
        /// 问答搜索自动完成
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="topNumber"></param>
        /// <returns></returns>
        public JsonResult SearchAutoComplete(string keyword, int topNumber)
        {
            AskSearcher askSearcher = (AskSearcher)SearcherFactory.GetSearcher(AskSearcher.CODE);
            IEnumerable<string> terms = askSearcher.AutoCompleteSearch(keyword, topNumber);

            return Json(terms.Select(t => new { tagName = t, tagNameWithHighlight = SearchEngine.Highlight(keyword, string.Join("", t.Take(34)), 100) }), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region

        /// <summary>
        /// 侧边栏问题标签分组
        /// </summary>
        [DonutOutputCache(CacheProfile = "Usual")]
        public ActionResult _TagGroups(int topNum = 4, bool isShowBubble = true, bool isShowMore = false)
        {
            IEnumerable<TagGroup> tagGroups = tagService.GetGroups(TenantTypeIds.Instance().AskQuestion());

            if (tagGroups != null && tagGroups.Count() > 0)
            {
                Dictionary<long, IEnumerable<string>> tagsOfGroup = new Dictionary<long, IEnumerable<string>>();
                foreach (TagGroup tagGroup in tagGroups.Take(topNum))
                {
                    tagsOfGroup[tagGroup.GroupId] = tagService.GetTopTagsOfGroup(tagGroup.GroupId, 30);
                }
                ViewData["tagsOfGroup"] = tagsOfGroup;
            }
            else
            {
                ViewData["topTags"] = tagService.GetTopTags(20, null);
            }
            ViewData["isShowBubble"] = isShowBubble;
            ViewData["isShowMore"] = isShowMore;

            return View(tagGroups);
        }

        /// <summary>
        /// 侧边栏高分悬赏
        /// </summary>
        public ActionResult _HighRewardQuestions(int topNum = 5)
        {
            IEnumerable<AskQuestion> questions = askService.GetTopQuestions(TenantTypeIds.Instance().User(), topNum, QuestionStatus.Unresolved, null, SortBy_AskQuestion.Reward).Where(n => n.Reward > 0);
            return View(questions);
        }

        /// <summary>
        /// 侧边栏用户回答排行（根据回答数倒排序）
        /// </summary>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _AnswerUserRank(int topNum = 5)
        {
            IEnumerable<long> userIds = ownerDataService.GetTopOwnerIds(OwnerDataKeys.Instance().AnswerCount(), topNum, OwnerData_SortBy.LongValue_DESC);
            IEnumerable<IUser> users = userService.GetFullUsers(userIds);

            //查询用户解答数
            Dictionary<long, long> userAnswerCount = new Dictionary<long, long>();
            foreach (long userId in userIds)
            {
                long answerCount = ownerDataService.GetLong(userId, OwnerDataKeys.Instance().AnswerCount());
                userAnswerCount[userId] = answerCount;
            }

            ViewData["userAnswerCount"] = userAnswerCount;

            return View(users);
        }

        /// <summary>
        /// 侧边栏用户贡献排行（根据威望排序）
        /// </summary>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _ContributionUserRank(int topNum = 5)
        {
            IEnumerable<long> userIds = ownerDataService.GetTopOwnerIds(OwnerDataKeys.Instance().UserAskReputation(), topNum, OwnerData_SortBy.LongValue_DESC);
            IEnumerable<IUser> users = userService.GetFullUsers(userIds);

            //查询用户问答威望数
            Dictionary<long, long> userAskReputationCount = new Dictionary<long, long>();
            foreach (long userId in userIds)
            {
                long reputationCount = ownerDataService.GetLong(userId, OwnerDataKeys.Instance().UserAskReputation());
                userAskReputationCount[userId] = reputationCount;
            }

            ViewData["userAskReputationCount"] = userAskReputationCount;

            return View(users);
        }

        /// <summary>
        /// 问题列表
        /// </summary>
        public ActionResult Questions(string tab, string tagName = null)
        {
            pageResourceManager.InsertTitlePart("问题");
            return View();
        }

        /// <summary>
        /// 标签浮动内容块
        /// </summary>
        public ActionResult _TagContents(string tagName)
        {
            Tag tag = tagService.Get(tagName);
            if (tag == null)
            {
                return HttpNotFound();
            }

            ViewData["followerCount"] = subscribeTagService.GetSubscribedUserCount(tag.TagId);
            ViewData["answerCount"] = askService.GetAnswerCountOfTag(tag.TagName);

            return View(tag);
        }

        /// <summary>
        /// 最新问题列表
        /// </summary>
        public ActionResult _LatestQuestions(int pageSize = 10, int pageIndex = 1, string tagName = null)
        {
            PagingDataSet<AskQuestion> questions = askService.GetQuestions(TenantTypeIds.Instance().User(), tagName, null, null, SortBy_AskQuestion.DateCreated_Desc, pageSize, pageIndex);

            return View("_QuestionList", questions);
        }

        /// <summary>
        /// 待解决问题列表
        /// </summary>
        public ActionResult _UnresolvedQuestions(int pageSize = 10, int pageIndex = 1, string tagName = null)
        {
            PagingDataSet<AskQuestion> questions = askService.GetQuestions(TenantTypeIds.Instance().User(), tagName, QuestionStatus.Unresolved, null, SortBy_AskQuestion.DateCreated_Desc, pageSize, pageIndex);
            ViewData["resolvedQuestions"] = true;
            return View("_QuestionList", questions);
        }

        /// <summary>
        /// 已解决问题列表
        /// </summary>
        public ActionResult _ResolvedQuestions(int pageSize = 10, int pageIndex = 1, string tagName = null)
        {
            PagingDataSet<AskQuestion> questions = askService.GetQuestions(TenantTypeIds.Instance().User(), tagName, QuestionStatus.Resolved, null, SortBy_AskQuestion.DateCreated_Desc, pageSize, pageIndex);

            return View("_QuestionList", questions);
        }

        /// <summary>
        /// 零回答问题列表
        /// </summary>
        public ActionResult _NoAnswerQuestions(int pageSize = 10, int pageIndex = 1, string tagName = null)
        {
            PagingDataSet<AskQuestion> questions = askService.GetNoAnswerQuestions(TenantTypeIds.Instance().User(), tagName, pageSize, pageIndex);

            return View("_QuestionList", questions);
        }

        /// <summary>
        /// 精华问题列表
        /// </summary>
        public ActionResult _EssentialQuestions(int pageSize = 10, int pageIndex = 1, string tagName = null)
        {
            pageResourceManager.InsertTitlePart("精华");

            PagingDataSet<AskQuestion> questions = askService.GetQuestions(TenantTypeIds.Instance().User(), tagName, null, true, SortBy_AskQuestion.DateCreated_Desc, pageSize, pageIndex);

            return View("_QuestionList", questions);
        }

        /// <summary>
        /// 高分悬赏
        /// </summary>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _HighRewardsQuestions(int pageSize = 10, int pageIndex = 1, string tagName = null)
        {
            pageResourceManager.InsertTitlePart("高分悬赏");

            PagingDataSet<AskQuestion> questions = askService.GetQuestions(TenantTypeIds.Instance().User(), tagName, QuestionStatus.Unresolved, null, SortBy_AskQuestion.Reward, pageSize, pageIndex);

            return View("_QuestionList", questions);
        }

        /// <summary>
        /// 关注问题的用户
        /// </summary>
        public ActionResult QuestionFollowers(long questionId)
        {
            AskQuestion question = askService.GetQuestion(questionId);
            if (question == null)
            {
                return HttpNotFound();
            }

            pageResourceManager.InsertTitlePart(question.Subject + "的关注用户");

            return View(question);
        }

        /// <summary>
        /// 关注问题的用户列表
        /// </summary>
        public ActionResult _QuestionFollowers(long questionId, int pageIndex = 1)
        {
            PagingDataSet<long> userIds = subscribeQuestionService.GetPagingUserIdsOfObject(questionId, pageIndex);

            IEnumerable<User> users = userService.GetFullUsers(userIds);
            PagingDataSet<User> pagingUsers = new PagingDataSet<User>(users)
            {
                TotalRecords = userIds.TotalRecords,
                PageSize = userIds.PageSize,
                PageIndex = userIds.PageIndex,
                QueryDuration = userIds.QueryDuration
            };

            Dictionary<long, string> userDescription = new Dictionary<long, string>();
            Dictionary<long, Dictionary<string, long>> askStatisticData = new Dictionary<long, Dictionary<string, long>>();
            foreach (long userId in userIds)
            {
                userDescription.Add(userId, askService.GetUserDescription(userId));

                Dictionary<string, long> userStatisticData = new Dictionary<string, long>();
                userStatisticData.Add(OwnerDataKeys.Instance().QuestionCount(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().QuestionCount()));
                userStatisticData.Add(OwnerDataKeys.Instance().AnswerCount(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().AnswerCount()));
                askStatisticData.Add(userId, userStatisticData);
            }

            ViewData["userDescription"] = userDescription;
            ViewData["askStatisticData"] = askStatisticData;
            return View(pagingUsers);
        }

        /// <summary>
        /// 关注标签的用户列表
        /// </summary>
        public ActionResult _TagFollowers(long tagId, int pageIndex = 1)
        {
            PagingDataSet<long> userIds = subscribeTagService.GetPagingUserIdsOfObject(tagId, pageIndex);

            IEnumerable<User> users = userService.GetFullUsers(userIds);
            PagingDataSet<User> pagingUsers = new PagingDataSet<User>(users)
            {
                TotalRecords = userIds.TotalRecords,
                PageSize = userIds.PageSize,
                PageIndex = userIds.PageIndex,
                QueryDuration = userIds.QueryDuration
            };

            Dictionary<long, string> userDescription = new Dictionary<long, string>();
            Dictionary<long, Dictionary<string, long>> askStatisticData = new Dictionary<long, Dictionary<string, long>>();
            foreach (long userId in userIds)
            {
                userDescription.Add(userId, askService.GetUserDescription(userId));

                Dictionary<string, long> userStatisticData = new Dictionary<string, long>();
                userStatisticData.Add(OwnerDataKeys.Instance().QuestionCount(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().QuestionCount()));
                userStatisticData.Add(OwnerDataKeys.Instance().AnswerCount(), ownerDataService.GetLong(userId, OwnerDataKeys.Instance().AnswerCount()));
                askStatisticData.Add(userId, userStatisticData);
            }

            ViewData["userDescription"] = userDescription;
            ViewData["askStatisticData"] = askStatisticData;
            return View(pagingUsers);
        }

        /// <summary>
        /// 问题标签聚合页
        /// </summary>
        public ActionResult Tags()
        {
            pageResourceManager.InsertTitlePart("标签");
            IEnumerable<TagGroup> tagGroups = tagService.GetGroups(TenantTypeIds.Instance().AskQuestion());
            TagService tagServiceForAsk = new TagService(TenantTypeIds.Instance().AskQuestion());
            ViewData["allTags"] = tagServiceForAsk.GetTopTags(100, SortBy_Tag.DateCreatedDesc);

            return View(tagGroups);
        }

        /// <summary>
        /// 局部页-问题标签聚合
        /// </summary>
        /// <returns></returns>
        public ActionResult _TagList(long groupId, int pageIndex = 1)
        {
            PagingDataSet<Tag> pagingTag = tagService.GetTagsOfGroup(groupId, TenantTypeIds.Instance().AskQuestion(), pageIndex);
            TagGroup tagGroup = tagService.GetGroup(groupId);

            ViewData["tagGroup"] = tagGroup;

            return View(pagingTag);
        }

        /// <summary>
        /// 侧边栏热门标签
        /// </summary>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _HotTags(int topNumber = 12)
        {
            IEnumerable<Tag> tags = tagService.GetTopTags(topNumber, null, SortBy_Tag.ItemCountDesc);
            return View(tags);
        }

        /// <summary>
        /// 关注标签的用户
        /// </summary>
        public ActionResult TagFollowers(long tagId, int pageIndex = 1)
        {
            Tag tag = tagService.Get(tagId);

            if (tag == null)
            {
                return HttpNotFound();
            }

            ViewData["followerCount"] = subscribeTagService.GetSubscribedUserCount(tag.TagId);

            pageResourceManager.InsertTitlePart(tag.TagName + "的关注用户");

            return View(tag);
        }

        /// <summary>
        /// 用户排行榜
        /// </summary>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult Rank(int topNum = 100)
        {
            //贡献排行
            IEnumerable<long> reputationUserIds = ownerDataService.GetTopOwnerIds(OwnerDataKeys.Instance().UserAskReputation(), topNum, OwnerData_SortBy.LongValue_DESC);
            IEnumerable<IUser> reputationUsers = userService.GetFullUsers(reputationUserIds);
            Dictionary<long, long> userReputationCount = new Dictionary<long, long>();
            foreach (long userId in reputationUserIds)
            {
                long reputationCount = ownerDataService.GetLong(userId, OwnerDataKeys.Instance().UserAskReputation());
                userReputationCount[userId] = reputationCount;
            }

            ViewData["userReputationCount"] = userReputationCount;
            ViewData["reputationUsers"] = reputationUsers;

            //解答排行
            IEnumerable<long> answerUserIds = ownerDataService.GetTopOwnerIds(OwnerDataKeys.Instance().AnswerCount(), topNum, OwnerData_SortBy.LongValue_DESC);
            IEnumerable<IUser> answerUsers = userService.GetFullUsers(answerUserIds);
            Dictionary<long, long> userAnswerCount = new Dictionary<long, long>();
            foreach (long userId in answerUserIds)
            {
                long answerCount = ownerDataService.GetLong(userId, OwnerDataKeys.Instance().AnswerCount());
                userAnswerCount[userId] = answerCount;
            }

            ViewData["userAnswerCount"] = userAnswerCount;
            ViewData["answerUsers"] = answerUsers;

            //提问排行
            IEnumerable<long> questionUserIds = ownerDataService.GetTopOwnerIds(OwnerDataKeys.Instance().QuestionCount(), topNum, OwnerData_SortBy.LongValue_DESC);
            IEnumerable<IUser> questionUsers = userService.GetFullUsers(questionUserIds);
            Dictionary<long, long> userQuestionCount = new Dictionary<long, long>();
            foreach (long userId in questionUserIds)
            {
                long questionCount = ownerDataService.GetLong(userId, OwnerDataKeys.Instance().QuestionCount());
                userQuestionCount[userId] = questionCount;
            }

            ViewData["userQuestionCount"] = userQuestionCount;
            ViewData["questionUsers"] = questionUsers;

            pageResourceManager.InsertTitlePart("用户排行");

            return View();
        }
        
        #endregion

        #region PrivateMethod

        /// <summary>
        /// 获取悬赏下拉框对象
        /// </summary>
        /// <param name="defaultReward">默认的悬赏值</param>
        /// <returns></returns>
        private SelectList GetRewardList(int defaultReward)
        {
            int tradePoints = userService.GetFullUser(UserContext.CurrentUser.UserId).TradePoints;
            Dictionary<int, int> rewardDic = new Dictionary<int, int>();
            if (tradePoints <= 0)
            {
                rewardDic.Add(0, 0);
            }
            else
            {
                rewardDic.Add(defaultReward, defaultReward);
                int loopCount = tradePoints / 5;
                int loopMaxCount = 100 / 5;
                if (loopCount > loopMaxCount)
                {
                    loopCount = loopMaxCount;
                }
                int reward = 0;
                for (int i = 0; i <= loopCount; i++)
                {
                    if (reward > defaultReward)
                    {
                        rewardDic.Add(reward, reward);
                    }
                    reward += 5;
                }
            }
            SelectList rewards = new SelectList(rewardDic, "Value", "Key");
            return rewards;
        }

        #endregion

    }
}