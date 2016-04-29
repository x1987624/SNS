//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;

namespace Spacebuilder.Ask.Controllers
{
    /// <summary>
    /// 问答管理控制器
    /// </summary>
    [ManageAuthorize(CheckCookie = false)]
    [TitleFilter(TitlePart = "问答管理", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.ControlPanel, IsApplication = true)]
    public class ControlPanelAskController : Controller
    {
        public IPageResourceManager pageResourceManager { get; set; }
        public AskService askSevice { get; set; }

        #region 问题View

        /// <summary>
        /// 管理问题
        /// </summary>
        /// <param name="auditStatus">审批状态</param>
        /// <param name="subjectKeyword">标题关键字</param>
        /// <param name="tagKeyword">标签关键字</param>
        /// <param name="userId">作者id</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="questionStatus">问题状态</param>
        public ActionResult ManageQuestions(AuditStatus? auditStatus = null, string subjectKeyword = null, QuestionStatus? questionStatus = null, long? ownerId = null, string tagKeyword = null, string userId = null, int pageSize = 20, int pageIndex = 1)
        {
            long? questionUserId = null;

            if (!string.IsNullOrEmpty(userId))
            {
                userId = userId.Trim(',');
                if (!string.IsNullOrEmpty(userId))
                {
                    questionUserId = long.Parse(userId);
                }
            }

            ViewData["userId"] = questionUserId;

            PagingDataSet<AskQuestion> questionsList = askSevice.GetQuestionsForAdmin(null, auditStatus, questionStatus, ownerId, questionUserId, subjectKeyword, tagKeyword, pageSize, pageIndex);

            pageResourceManager.InsertTitlePart("问题管理");

            return View(questionsList);
        }

        /// <summary>
        /// 删除问题
        /// </summary>
        /// <param name="questionIds">问题id列表</param>
        [HttpPost]
        public JsonResult _DeleteQuestion(IEnumerable<long> questionIds)
        {
            AskQuestion question = null;
            foreach (var questionId in questionIds)
            {
                if (questionId <= 0)
                    continue;
                question = askSevice.GetQuestion(questionId);
                if (question == null)
                    continue;
                askSevice.DeleteQuestion(question);
            }

            return Json(new StatusMessageData(StatusMessageType.Success, "删除问题成功！"));
        }

        /// <summary>
        /// 批量设置/取消精华
        /// </summary>
        /// <param name="questionId">问题id列表</param>
        /// <param name="isEssential">是否精华，true为加精，false为取消精华</param>
        [HttpPost]
        public ActionResult _SetEssential(List<long> questionIds, bool isEssential)
        {
            foreach (long item in questionIds)
            {
                AskQuestion askQuestion = askSevice.GetQuestion(item);
                askSevice.SetEssential(askQuestion,isEssential);
            }
            return Json(new StatusMessageData(StatusMessageType.Success, isEssential ? "设置精华成功" : "取消精华成功"));

        }

        /// <summary>
        /// 更新问题审核状态
        /// </summary>
        /// <param name="questionIds">问题id</param>
        /// <param name="isApprove">审核状态，true为通过审核，false为不通过审核</param>
        [HttpPost]
        public JsonResult _ApproveQuestion(IEnumerable<long> questionIds, bool isApprove)
        {
            foreach (var questionId in questionIds)
            {
                askSevice.ApproveQuestion(questionId, isApprove);
            }

            return Json(new StatusMessageData(StatusMessageType.Success, "更新审核状态成功！"));
        }

        /// <summary>
        /// 更新问题审核状态
        /// </summary>
        /// <param name="questionIds">问题id</param>
        /// <param name="isApprove">审核状态，true为通过审核，false为不通过审核</param>
        [HttpPost]
        public JsonResult _ApproveQuestione(long questionId, bool isApprove)
        {
            askSevice.ApproveQuestion(questionId, isApprove);
            return Json(new StatusMessageData(StatusMessageType.Success, "更新审核状态成功！"));
        }

        #endregion

        #region 回答View
        /// <summary>
        /// 回答管理
        /// </summary>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="subjectKeyword">标题关键字</param>
        /// <param name="userId">作者ID</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public ActionResult ManageAnswers(AuditStatus? auditStatus = null, string subjectKeyword = null, string userId = null, int pageSize = 20, int pageIndex = 1)
        {
            long? answerUserId = null;

            if (!string.IsNullOrEmpty(userId))
            {
                userId = userId.Trim(',');
                if (!string.IsNullOrEmpty(userId))
                {
                    answerUserId = long.Parse(userId);
                }
            }

            ViewData["userId"] = answerUserId;

            PagingDataSet<AskAnswer> AskAnswerList = askSevice.GetAnswersForAdmin(auditStatus, answerUserId, subjectKeyword, pageSize, pageIndex);

            pageResourceManager.InsertTitlePart("回答管理");

            return View(AskAnswerList);
        }

        /// <summary>
        /// 删除回答
        /// </summary>
        /// <param name="answerIds">回答id列表</param>
        [HttpPost]
        public JsonResult _DeleteAnswer(IEnumerable<long> answerIds)
        {
            foreach (var answerId in answerIds)
            {
                askSevice.DeleteAnswer(askSevice.GetAnswer(answerId));
            }

            return Json(new StatusMessageData(StatusMessageType.Success, "删除回答成功！"));
        }

        /// <summary>
        /// 更新回答审核状态
        /// </summary>
        /// <param name="answerIds">回答id</param>
        /// <param name="isApprove">审核状态，true为通过审核，false为不通过审核</param>
        [HttpPost]
        public JsonResult _ApproveAnswer(IEnumerable<long> answerIds, bool isApprove)
        {
            foreach (var answerId in answerIds)
            {
                askSevice.ApproveAnswer(answerId, isApprove);
            }

            return Json(new StatusMessageData(StatusMessageType.Success, "更新审核状态成功！"));
        }

        /// <summary>
        /// 编辑回答
        /// </summary>
        /// <param name="answerId">回答id</param>
        [HttpGet]
        public ActionResult _EditAnswer(long answerId)
        {
            AskAnswer askAnswer = askSevice.GetAnswer(answerId);

            return View(askAnswer.AsEditModel());
        }

        /// <summary>
        /// 编辑回答
        /// </summary>
        /// <param name="model">编辑回答的EditModel</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _EditAnswer(AskAnswerEditModel model)
        {
            AskAnswer answer = model.AsAskAnswer();
            askSevice.UpdateAnswer(answer);

            return Json(new StatusMessageData(StatusMessageType.Success, "更新成功！"));
        }
        #endregion
    }
}