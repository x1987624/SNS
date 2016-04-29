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
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 提问数
        /// </summary>
        public static string QuestionCount(this OwnerDataKeys ownerDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-ThreadCount";
        }

        /// <summary>
        /// 回答数
        /// </summary>
        public static string AnswerCount(this OwnerDataKeys ownerDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AnswerCount";
        }

        /// <summary>
        /// 回答被采纳数
        /// </summary>
        public static string AnswerAcceptCount(this OwnerDataKeys ownerDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AnswerAcceptCount";
        }

        /// <summary>
        /// 回答被赞同数
        /// </summary>
        public static string AnswerSupportCount(this OwnerDataKeys ownerDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AnswerSupportCount";
        }

        /// <summary>
        /// 回答被反对数
        /// </summary>
        public static string AnswerOpposeCount(this OwnerDataKeys ownerDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AnswerOpposeCount";
        }

        /// <summary>
        /// 是否接受定向提问
        /// </summary>
        /// <param name="ownerDataKeys"></param>
        /// <returns></returns>
        public static string AcceptQuestion(this OwnerDataKeys ownerDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-AcceptQuestion";
        }

        /// <summary>
        /// 用户在用户在问答中的简介
        /// </summary>
        /// <param name="ownerDataKeys"></param>
        /// <returns></returns>
        public static string UserDescription(this OwnerDataKeys ownerDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-UserDescription";
        }

        /// <summary>
        /// 用户问答威望
        /// </summary>
        /// <param name="ownerDataKeys"></param>
        /// <returns></returns>
        public static string UserAskReputation(this OwnerDataKeys ownerDataKeys)
        {
            return AskConfig.Instance().ApplicationKey + "-UserAskReputation";
        }
    }
}