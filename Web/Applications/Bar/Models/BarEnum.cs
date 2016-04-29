//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-23</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-08-23" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Repositories;
using Tunynet;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖吧排序依据
    /// </summary>
    public enum SortBy_BarSection
    {
        /// <summary>
        /// 创建时间倒序
        /// </summary>
        DateCreated_Desc,
        /// <summary>
        /// 主题帖数
        /// </summary>
        ThreadCount,

        /// <summary>
        /// 主题帖和回帖总数
        /// </summary>
        ThreadAndPostCount,

        /// <summary>
        /// 阶段主题帖和回帖总数
        /// </summary>
        StageThreadAndPostCount,
        /// <summary>
        /// 被关注数
        /// </summary>
        FollowedCount
    }

    /// <summary>
    /// 帖子排序依据
    /// </summary>
    public enum SortBy_BarThread
    {
        /// <summary>
        /// 发布时间倒序
        /// </summary>
        DateCreated_Desc,
        /// <summary>
        /// 更新时间倒序
        /// </summary>
        LastModified_Desc,
        /// <summary>
        /// 浏览数
        /// </summary>
        HitTimes,
        /// <summary>
        /// 阶段浏览数
        /// </summary>
        StageHitTimes,
        /// <summary>
        /// 回帖数
        /// </summary>
        PostCount
    }

    /// <summary>
    /// 回帖排序依据
    /// </summary>
    public enum SortBy_BarPost
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateCreated,
        /// <summary>
        /// 创建时间倒序
        /// </summary>
        DateCreated_Desc
    }

    /// <summary>
    /// 主题分类状态
    /// </summary>
    public enum ThreadCategoryStatus
    {
        //0=禁用；1=启用（不强制）；2=启用（强制）
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// 启用（不强制）
        /// </summary>
        NotForceEnabled = 1,
        /// <summary>
        /// 启用（强制）
        /// </summary>
        ForceEnabled = 2

    }
}