//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答计数类型扩展类
    /// </summary>
    public static class CountTypesExtension
    {
        /// <summary>
        /// 被关注次数
        /// </summary>
        public static string QuestionFollowerCount(this CountTypes countTypes)
        {
            return "QuestionFollowerCount";
        }

    }
}