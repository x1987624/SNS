//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 扩展NoticeTemplateNames
    /// </summary>
    public static class NoticeTemplateNamesExtension
    {
        /// <summary>
        /// 问题有新回答
        /// </summary>
        public static string NewAnswer(this NoticeTemplateNames noticeTemplateNames)
        {
            return "NewAnswer";
        }

        /// <summary>
        /// 问题有新评论
        /// </summary>
        public static string NewAskComment(this NoticeTemplateNames noticeTemplateNames)
        {
            return "NewAskComment";
        }

        /// <summary>
        /// 采纳满意回答
        /// </summary>
        public static string SetBestAnswer(this NoticeTemplateNames noticeTemplateNames)
        {
            return "SetBestAnswer";
        }

        /// <summary>
        /// 向他提问（定向提问）
        /// </summary>
        public static string AskUser(this NoticeTemplateNames noticeTemplateNames)
        {
            return "AskUser";
        }
    }
}