//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-28</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-08-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Repositories;
using Tunynet;
using Tunynet.Common;
using Tunynet.Caching;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 回帖仓储接口
    /// </summary>
    public interface IBarPostRepository : IRepository<BarPost>
    {
        /// <summary>
        /// 获取BarPost内容
        /// </summary>
        /// <param name="threadId"></param>
        string GetBody(long threadId);

        /// <summary>
        /// 获取解析过正文
        /// </summary>
        /// <returns></returns>
        string GetResolvedBody(long threadId);

        /// <summary>
        /// 获取主题帖最新的一条回复
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns>回复贴（若没有，则返回null）</returns>
        BarPost GetNewestPost(long threadId);

        /// <summary>
        /// 获取某个帖子下的所有回帖（用于删除帖子）
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        IEnumerable<BarPost> GetAllPostsOfThread(long threadId);

        /// <summary>
        /// 获取某个用户的所有回帖（用于删除用户）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<BarPost> GetAllPostsOfUser(long userId);

        /// <summary>
        /// 获取我的回复贴分页集合
        /// </summary>
        /// <param name="userId">回复贴作者Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回复贴列表</returns>
        PagingDataSet<BarPost> GetMyPosts(long userId, string tenantTypeId = null, int pageIndex = 1);

        /// <summary>
        /// 获取主题帖的回帖排行分页集合
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <param name="onlyStarter">仅看楼主</param>
        /// <param name="starterId">楼主Id</param>
        /// <param name="sortBy">回帖排序依据（默认为按创建时间正序排序）</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回复贴列表</returns>
        PagingDataSet<BarPost> Gets(long threadId, bool onlyStarter, long starterId, SortBy_BarPost sortBy, int pageIndex);

        /// <summary>
        /// 获取回帖的管理列表
        /// </summary>
        /// <param name="query">帖子查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖子分页集合</returns>
        PagingDataSet<BarPost> Gets(string tenantTypeId, BarPostQuery query, int pageSize, int pageIndex);

        /// <summary>
        /// 获取子级回帖列表
        /// </summary>
        /// <param name="parentId">父回帖Id</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="sortBy">排序字段</param> 
        /// <returns></returns>
        PagingDataSet<BarPost> GetChildren(long parentId, int pageIndex, SortBy_BarPost sortBy);

        /// <summary>
        /// 用户某一段时间内对哪些帖子回过帖
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="beforeDays"></param>
        /// <returns></returns>
        IEnumerable<long> GetThreadIdsByUser(long userId, int beforeDays);

        /// <summary>
        /// 获取回复在帖子回复列表中的页码数
        /// </summary>
        /// <param name="threadId">帖子id</param>
        /// <param name="postId">回复id</param>
        /// <returns>回复在帖子回复列表中的页码数</returns>
        int GetPageIndexForPostInThread(long threadId, long postId);

        /// <summary>
        /// 获取二级回复在二级回复列表中的页码数
        /// </summary>
        /// <param name="parentId">父级回复id</param>
        /// <param name="postId">回复id</param>
        /// <returns>二级回复在二级回复列表中的页码数</returns>
        int GetPageIndexForChildrenPost(long parentId, long postId);

        /// <summary>
        /// 获取统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns></returns>
        Dictionary<string, long> GetManageableDatas(string tenantTypeId);
    }

}
