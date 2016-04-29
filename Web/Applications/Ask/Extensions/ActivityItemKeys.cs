//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Ask
{

    /// <summary>
    /// 问答动态项
    /// </summary>
    public static class ActivityItemKeysExtension
    {
        /// <summary>
        /// 发布问题
        /// </summary>
        public static string CreateAskQuestion(this ActivityItemKeys activityItemKeys)
        {
            return "CreateAskQuestion";
        }

        /// <summary>
        /// 发布回答
        /// </summary>
        public static string CreateAskAnswer(this ActivityItemKeys activityItemKeys)
        {
            return "CreateAskAnswer";
        }

        /// <summary>
        /// 评论问题
        /// </summary>
        public static string CommentAskQuestion(this ActivityItemKeys activityItemKeys)
        {
            return "CommentAskQuestion";
        }

        /// <summary>
        /// 评论回答
        /// </summary>
        public static string CommentAskAnswer(this ActivityItemKeys activityItemKeys)
        {
            return "CommentAskAnswer";
        }

        /// <summary>
        /// 赞同回答
        /// </summary>
        public static string SupportAskAnswer(this ActivityItemKeys activityItemKeys)
        {
            return "SupportAskAnswer";
        }
    }

}
