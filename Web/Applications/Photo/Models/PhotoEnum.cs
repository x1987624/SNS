//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Spacebuilder.Photo
{

    /// <summary>
    /// 相册排序依据
    /// </summary>
    public enum SortBy_Album
    {
        /// <summary>
        /// 发布时间倒序
        /// </summary>
        [Display(Name = "最新创建在前")]
        DateCreated_Desc,

        /// <summary>
        /// 发布时间正序
        /// </summary>
        [Display(Name = "最新创建在后")]
        DateCreated_Asc,

        /// <summary>
        /// 最后上传时间倒序
        /// </summary>
        [Display(Name = "最近上传在前")]
        LastUploadDate_Desc,

        /// <summary>
        /// 最后上传时间正序
        /// </summary>
        [Display(Name = "最近上传在后")]
        LastUploadDate_Asc,

        /// <summary>
        /// 根据显示顺序
        /// </summary>
        [Display(Name = "自定义")]
        DisplayOrder

    }

    /// <summary>
    /// 照片排序依据
    /// </summary>
    public enum SortBy_Photo
    {
        /// <summary>
        /// 发布时间倒序（最新照片）
        /// </summary>
        DateCreated_Desc,

        //阶段点击数倒排序（热门图片、热点图片）
        HitTimes_Desc,

        //阶段评论数倒排序（热评图片）
        CommentCount_Desc,

        //喜欢数倒排序（喜欢的图片）
        SupportCount_Desc,
    }
}