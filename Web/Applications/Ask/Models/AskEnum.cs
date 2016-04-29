//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问题排序依据
    /// </summary>
    public enum SortBy_AskQuestion
    {
        /// <summary>
        /// 发布时间倒序
        /// </summary>
        DateCreated_Desc,

        /// <summary>
        /// 回答数
        /// </summary>
        AnswerCount,
        
        /// <summary>
        /// 阶段浏览数
        /// </summary>
        StageHitTimes,

        /// <summary>
        /// 悬赏分值
        /// </summary>
        Reward,
    }

    /// <summary>
    /// 回答排序依据
    /// </summary>
    public enum SortBy_AskAnswer
    {
        /// <summary>
        /// 发布时间倒序
        /// </summary>
        DateCreated_Desc,

        /// <summary>
        /// 赞同数
        /// </summary>
        SupportCount
    }
    /// <summary>
    /// 问题状态
    /// </summary>
    public enum QuestionStatus
    {
        /// <summary>
        /// 未解决
        /// </summary>
        [Display(Name = "未解决")]
        Unresolved,

        /// <summary>
        /// 已解决
        /// </summary>
        [Display(Name = "已解决")]
        Resolved,

        /// <summary>
        /// 已取消
        /// </summary>
        [Display(Name = "已取消")]
        Canceled
    }
}