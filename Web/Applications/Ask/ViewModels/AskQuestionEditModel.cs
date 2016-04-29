//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet.Common;


namespace Spacebuilder.Ask
{
    /// <summary>
    /// 编辑问题的EditModel
    /// </summary>
    public class AskQuestionEditModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public long QuestionId { get; set; }

        /// <summary>
        /// 定向提问用户ID
        /// </summary>
        public long AskUserId { get; set; }

        /// <summary>
        /// 问题状态
        /// </summary>
        public QuestionStatus QuestionStatus { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [WaterMark(Content = "在此输入问题")]
        [Required(ErrorMessage = "请输入问题内容")]
        [StringLength(TextLengthSettings.TEXT_SUBJECT_MAXLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        public string Subject { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        [StringLength(TextLengthSettings.TEXT_BODY_MAXLENGTH * 5, ErrorMessage = "最多可以输入{1}个字")]
        [AllowHtml]
        [DataType(DataType.Html)]
        public string Body { get; set; }

        /// <summary>
        /// 悬赏分值
        /// </summary>
        private int reward;
        [Range(0, 100, ErrorMessage = "悬赏分值范围是0-100")]
        public int Reward { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public IEnumerable<string> TagNames { get; set; }

        /// <summary>
        /// 问题作者用户id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 所有者id
        /// </summary>
        public long? OwnerId { get; set; }

        /// <summary>
        /// 转换成AskQuestion类型
        /// </summary>
        /// <returns>问题实体</returns>
        public AskQuestion AsAskQuestion()
        {
            AskQuestion askQuestion = null;

            //发布问题
            if (this.QuestionId == 0)
            {
                askQuestion = AskQuestion.New();
                askQuestion.UserId = UserContext.CurrentUser.UserId;
                askQuestion.Author = UserContext.CurrentUser.DisplayName;
                askQuestion.Status = QuestionStatus.Unresolved;

                if (this.AskUserId > 0)
                {
                    askQuestion.AskUserId = this.AskUserId;
                }
                if (this.OwnerId.HasValue)
                {
                    askQuestion.OwnerId = this.OwnerId.Value;
                    askQuestion.TenantTypeId = TenantTypeIds.Instance().Group();
                }
                else
                {
                    askQuestion.OwnerId = 0;
                    askQuestion.TenantTypeId = TenantTypeIds.Instance().User();
                }

            }
            //编辑问题
            else
            {
                AskService askService = new AskService();
                askQuestion = askService.GetQuestion(this.QuestionId);
                askQuestion.LastModified = DateTime.UtcNow;
            }

            askQuestion.Subject = this.Subject;
            askQuestion.Body = this.Body;
            askQuestion.LastModifiedUserId = UserContext.CurrentUser.UserId;
            askQuestion.LastModifier = UserContext.CurrentUser.DisplayName;
            if (this.Reward > askQuestion.Reward && this.Reward <= askQuestion.User.TradePoints)
            {
                askQuestion.AddedReward = this.Reward - askQuestion.Reward;
                askQuestion.Reward = this.Reward;
            }
            return askQuestion;
        }
    }

    /// <summary>
    /// 问题实体的扩展类
    /// </summary>
    public static class AskQuestionExtensions
    {
        /// <summary>
        /// 将数据库中的信息转换成EditModel实体
        /// </summary>
        /// <param name="askQuestion">问题实体</param>
        /// <returns>编辑问题的EditModel</returns>
        public static AskQuestionEditModel AsEditModel(this AskQuestion askQuestion)
        {
            return new AskQuestionEditModel
            {
                QuestionId = askQuestion.QuestionId,
                Subject = askQuestion.Subject,
                Body = askQuestion.Body,
                Reward = askQuestion.Reward,
                UserId = askQuestion.UserId,
                QuestionStatus = askQuestion.Status
            };
        }
    }
}
