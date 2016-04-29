//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 用户计数类型扩展类
    /// </summary>
    public static class UserDataKeysExtension
    {
        /// <summary>
        /// 提问数
        /// </summary>
        public static string QuestionCount(this UserDataKeys userDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-QuestionCount";
        }

        /// <summary>
        /// 回答数
        /// </summary>
        public static string AnswerCount(this UserDataKeys userDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AnswerCount";
        }

        /// <summary>
        /// 回答被采纳数
        /// </summary>
        public static string AnswerAcceptCount(this UserDataKeys userDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AnswerAcceptCount";
        }

        /// <summary>
        /// 回答被赞同数
        /// </summary>
        public static string AnswerSupportCount(this UserDataKeys userDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AnswerSupportCount";
        }

        /// <summary>
        /// 是否接受定向提问
        /// </summary>
        /// <param name="userDataKeys"></param>
        /// <returns></returns>
        public static string AcceptQuestion(this UserDataKeys userDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AcceptQuestion";
        }

        /// <summary>
        /// 用户在用户在问答中的简介
        /// </summary>
        /// <param name="userDataKeys"></param>
        /// <returns></returns>
        public static string UserDescription(this UserDataKeys userDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-UserDescription";
        }
    }
}