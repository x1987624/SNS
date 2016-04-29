//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 编辑回答的EditModel
    /// </summary>
    public class AskAnswerEditModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public long AnswerId { get; set; }

        /// <summary>
        /// QuestionId
        /// </summary>
        public long QuestionId { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        [Required(ErrorMessage = "请输入回答")]
        [StringLength(TextLengthSettings.TEXT_BODY_MAXLENGTH, ErrorMessage = "最多可以输入{1}个字")]
        [AllowHtml]
        [DataType(DataType.Html)]
        public string Body { get; set; }

        /// <summary>
        /// 转换成AskAnswer类型
        /// </summary>
        /// <returns>回答实体</returns>
        public AskAnswer AsAskAnswer()
        {
            AskAnswer askAnswer = null;

            //发布回答
            if (this.AnswerId == 0)
            {
                askAnswer = AskAnswer.New();
                askAnswer.UserId = UserContext.CurrentUser.UserId;
                askAnswer.Author = UserContext.CurrentUser.DisplayName;
                askAnswer.IsBest = false;
                askAnswer.QuestionId = this.QuestionId;
            }
            //编辑回答
            else
            {
                AskService askService = new AskService();
                askAnswer = askService.GetAnswer(this.AnswerId);
                askAnswer.LastModified = DateTime.UtcNow;
            }

            askAnswer.Body = this.Body;

            return askAnswer;
        }
    }
}