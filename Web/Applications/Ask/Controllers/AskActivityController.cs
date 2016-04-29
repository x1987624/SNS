//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet.UI;
using Tunynet.Common;
using DevTrends.MvcDonutCaching;

namespace Spacebuilder.Ask.Controllers
{
    /// <summary>
    /// 问答动态控制器
    /// </summary>
    [Themed(PresentAreaKeysOfBuiltIn.Channel, IsApplication = true)]

    public class AskActivityController : Controller
    {
        public ActivityService activityService { get; set; }
        public AskService askService { get; set; }
        public CommentService commentService { get; set; }
        public UserService userService { get; set; }
        private AttachmentService questionAttachementService = new AttachmentService(TenantTypeIds.Instance().AskQuestion());
        private AttachmentService answerAttachementService = new AttachmentService(TenantTypeIds.Instance().AskAnswer());

        /// <summary>
        /// 发布问题的动态
        /// </summary>
        /// <param name="ActivityId"></param>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _CreateAskQuestion(long ActivityId)
        {
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            AskQuestion question = askService.GetQuestion(activity.SourceId);
            if (question == null)
            {
                return Content(string.Empty);
            }

            IEnumerable<Attachment> attachments = questionAttachementService.GetsByAssociateId(question.QuestionId);
            if (attachments != null && attachments.Count() > 0)
            {
                IEnumerable<Attachment> attachmentImages = attachments.Where(n => n.MediaType == MediaType.Image);
                ViewData["attachments"] = attachmentImages;
            }

            return View(question);
        }

        /// <summary>
        /// 回答问题的动态
        /// </summary>
        /// <param name="ActivityId"></param>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _CreateAskAnswer(long ActivityId)
        {
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            AskAnswer answer = askService.GetAnswer(activity.SourceId);
            if (answer == null)
            {
                return Content(string.Empty);
            }

            IEnumerable<Attachment> attachments = answerAttachementService.GetsByAssociateId(answer.AnswerId);
            if (attachments != null && attachments.Count() > 0)
            {
                IEnumerable<Attachment> attachmentImages = attachments.Where(n => n.MediaType == MediaType.Image);
                ViewData["attachments"] = attachmentImages;
            }

            return View(answer);
        }

        /// <summary>
        /// 赞成回答的动态
        /// </summary>
        /// <param name="ActivityId"></param>
        public ActionResult _SupportAskAnswer(long ActivityId)
        {
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            AskAnswer answer = askService.GetAnswer(activity.SourceId);
            if (answer == null)
            {
                return Content(string.Empty);
            }

            IEnumerable<Attachment> attachments = answerAttachementService.GetsByAssociateId(answer.AnswerId);
            if (attachments != null && attachments.Count() > 0)
            {
                IEnumerable<Attachment> attachmentImages = attachments.Where(n => n.MediaType == MediaType.Image);
                ViewData["attachments"] = attachmentImages;
            }

            IUser user = userService.GetUser(activity.UserId);
            ViewData["user"] = user;
            return View(answer);
        }

        /// <summary>
        /// 评论问题的动态
        /// </summary>
        /// <param name="ActivityId"></param>
        public ActionResult _CommentAskQuestion(long ActivityId)
        {
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            Comment comment = commentService.Get(activity.SourceId);
            if (comment == null)
            {
                return Content(string.Empty);
            }

            AskQuestion question = askService.GetQuestion(comment.CommentedObjectId);
            if (question == null)
            {
                return Content(string.Empty);
            }
            ViewData["question"] = question;


            IUser user = userService.GetUser(activity.UserId);
            ViewData["user"] = user;
            return View(comment);
        }

        /// <summary>
        /// 评论回答的动态
        /// </summary>
        /// <param name="ActivityId"></param>
        public ActionResult _CommentAskAnswer(long ActivityId)
        {
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            Comment comment = commentService.Get(activity.SourceId);
            if (comment == null)
            {
                return Content(string.Empty);
            }

            AskAnswer answer = askService.GetAnswer(comment.CommentedObjectId);
            if (answer == null)
            {
                return Content(string.Empty);
            }
            ViewData["answer"] = answer;


            IUser user = userService.GetUser(activity.UserId);
            ViewData["user"] = user;
            return View(comment);
        }
    }
}
