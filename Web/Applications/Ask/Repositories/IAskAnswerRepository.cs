//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Repositories;
using Tunynet;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答应用的回答仓储接口
    /// </summary>
    public interface IAskAnswerRepository : IRepository<AskAnswer>
    {
        /// <summary>
        /// 指定用户接管当前用户的回答
        /// </summary>
        /// <param name="userId">当前用户id</param>
        /// <param name="takeOverUser">指定用户</param>
        void TakeOver(long userId, User takeOverUser);

        /// <summary>
        /// 分页获取用户的回答列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="userId">回答的作者用户id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        PagingDataSet<AskAnswer> GetUserAnswers(long userId, bool ignoreAudit, int pageSize, int pageIndex);

        /// <summary>
        /// 根据问题id获取回答实体列表
        /// </summary>
        /// <remarks>
        /// 缓存周期：对象集合
        /// </remarks>
        /// <param name="questionId">问题id</param>
        /// <param name="sortBy">排序条件</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        PagingDataSet<AskAnswer> GetAnswersByQuestionId(long questionId, SortBy_AskAnswer sortBy, int pageSize, int pageIndex);

        /// <summary>
        /// 管理员获取回答分页集合
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="userId">用户id</param>
        /// <param name="keyword">查询关键字</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回答分页列表</returns>
        PagingDataSet<AskAnswer> GetAnswersForAdmin(AuditStatus? auditStatus, long? userId, string keyword, int pageSize, int pageIndex);

        /// <summary>
        /// 根据用户id和问题id获取回答实体
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="questionId">问题id</param>
        /// <returns>问题实体</returns>
        AskAnswer GetUserAnswerByQuestionId(long userId, long questionId);

        /// <summary>
        /// 获取回答管理数据
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <returns>回答管理数据</returns>
        Dictionary<string, long> GetManageableData();

        /// <summary>
        /// 获取回答应用统计数据
        /// </summary>
        /// <returns>回答统计数据</returns>
        Dictionary<string, long> GetApplicationStatisticData();

        /// <summary>
        /// 统计某个标签的回答数
        /// </summary>
        /// <param name="tagName">标签名</param>
        /// <returns>回答数</returns>
        long GetAnswerCountOfTag(string tagName);
    }
}