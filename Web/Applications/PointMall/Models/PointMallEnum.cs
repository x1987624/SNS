//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Spacebuilder.PointMall
{

    /// <summary>
    /// 商品排序依据
    /// </summary>
    public enum SortBy_PointGift
    {
        /// <summary>
        /// 发布时间倒序
        /// </summary>
        [Display(Name = "发布时间倒序")]
        DateCreated_Desc,

        /// <summary>
        /// 阶段点击数倒排序
        /// </summary>
        [Display(Name = "点击数倒排序")]
        HitTimes_Desc,

        /// <summary>
        /// 阶段性销量倒排序
        /// </summary>
        [Display(Name = "销量倒排序")]
        Sales_Desc,

        /// <summary>
        /// 价格倒排序
        /// </summary>
        [Display(Name = "价格倒排序")]
        Price_Desc,

        /// <summary>
        /// 价格正排序
        /// </summary>
        [Display(Name = "价格正排序")]
        Price_Asc
    }

    /// <summary>
    /// 申请状态
    /// </summary>
    public enum ApproveStatus
    {
        /// <summary>
        /// 待批准
        /// </summary>
        [Display(Name = "待批准")]
        Pending,

        /// <summary>
        /// 已批准
        /// </summary>
        [Display(Name = "已批准")]
        Approved,

        /// <summary>
        /// 未批准
        /// </summary>
        [Display(Name = "未批准")]
        Rejected
    }
}