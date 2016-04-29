//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Repositories;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答应用的问题仓储接口
    /// </summary>
    public interface IAskQuestionRepository : IRepository<AskQuestion>
    {
        /// <summary>
        /// 设置/取消精华问题（管理员操作）
        /// </summary>
        /// <param name="question">问题实体</param>
        /// <param name="isEssential">设置/取消精华</param>
        void SetEssential(AskQuestion question, bool isEssential);

        /// <summary>
        /// 指定用户接管当前用户的问题
        /// </summary>
        /// <param name="userId">当前用户id</param>
        /// <param name="takeOverUser">指定用户</param>
        void TakeOver(long userId, User takeOverUser);

        /// <summary>
        /// 获取感兴趣的问题
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="userId">用户id</param>
        /// <param name="topNumber">查询条数</param>
        /// <returns>问题列表</returns>
        IEnumerable<AskQuestion> GetTopInterestedQuestions(string tenantTypeId, long userId, int topNumber);

        /// <summary>
        /// 根据不同条件获取前n条的问题列表（用于前台）
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="status">问题状态</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序条件</param>
        /// <returns>问题列表</returns>
        IEnumerable<AskQuestion> GetTopQuestions(string tenantTypeId, int topNumber, QuestionStatus? status, bool? isEssential, SortBy_AskQuestion sortBy);

        /// <summary>
        /// 根据不同条件分页获取问题列表（用于前台）
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="tagName">标签</param>
        /// <param name="status">问题状态</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">排序条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        PagingDataSet<AskQuestion> GetQuestions(string tenantTypeId, string tagName, QuestionStatus? status, bool? isEssential, SortBy_AskQuestion sortBy, int pageSize, int pageIndex);

        /// <summary>
        /// 分页获取拥有者的问题列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        PagingDataSet<AskQuestion> GetOwnerQuestions(string tenantTypeId, long ownerId, bool ignoreAudit, int pageSize, int pageIndex);

        /// <summary>
        /// 分页获取用户的问题列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="userId">问题的作者用户id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        PagingDataSet<AskQuestion> GetUserQuestions(string tenantTypeId, long userId, bool ignoreAudit, int pageSize, int pageIndex);

        /// <summary>
        /// 获取零回答的问题（用于前台）
        /// </summary>
        /// <remarks>
        /// 缓存周期：常用对象集合，不用维护即时性
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="tagName">标签</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        PagingDataSet<AskQuestion> GetNoAnswerQuestions(string tenantTypeId, string tagName, int pageSize, int pageIndex);

        /// <summary>
        /// 管理员获取问题分页集合
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="questionStatus">问题状态</param>
        /// <param name="ownerId">所属id</param>
        /// <param name="userId">用户id</param>
        /// <param name="keyword">查询关键字</param>
        /// <param name="tagName">标签</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>问题分页列表</returns>
        PagingDataSet<AskQuestion> GetQuestionsForAdmin(string tenantTypeId, AuditStatus? auditStatus, QuestionStatus? questionStatus, long? ownerId, long? userId, string keyword, string tagName, int pageSize, int pageIndex);

        /// <summary>
        /// 获取问题管理数据
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>问题管理数据</returns>
        Dictionary<string, long> GetManageableData(string tenantTypeId = null);

        /// <summary>
        /// 获取问题应用统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>问题统计数据</returns>
        Dictionary<string, long> GetApplicationStatisticData(string tenantTypeId = null);

        /// <summary>
        /// 通过问题Id获取该问题下所有回答的顶踩数
        /// </summary>
        /// <param name="questionId">问题Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        int? GetAnswersAttitudesCount(long questionId,string tenantTypeId);

    }
}