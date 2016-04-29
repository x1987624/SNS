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
using Spacebuilder.Common;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖子仓储接口
    /// </summary>
    public interface IBarThreadRepository : IRepository<BarThread>
    {
        /// <summary>
        /// 移动帖子
        /// </summary>
        /// <param name="threadId">要移动帖子的ThreadId</param>
        /// <param name="moveToSectionId">转移到帖吧的SectionId</param>
        void MoveThread(long threadId, long moveToSectionId);

        /// <summary>
        /// 删除用户记录（删除用户时使用）
        /// </summary>
        /// <param name="userId">被删除用户</param>
        /// <param name="takeOver">接管用户</param>
        /// <param name="takeOverAll">是否接管被删除用户的所有内容</param>
        void DeleteUser(long userId, User takeOver, bool takeOverAll);

        /// <summary>
        /// 获取某个帖吧下的所有帖子（用于删除帖子）
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        IEnumerable<BarThread> GetAllThreadsOfSection(long sectionId);

        /// <summary>
        /// 获取某个用户的所有帖子（用于删除用户）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<BarThread> GetAllThreadsOfUser(long userId);

        /// <summary>
        /// 获取BarThread内容
        /// </summary>
        /// <param name="threadId"></param>
        string GetBody(long threadId);

        /// <summary>
        /// 获取解析过正文
        /// </summary>
        /// <returns></returns>
        string GetResolvedBody(long threadId);

        /// <summary>
        /// 获取用户的主题帖分页集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="isPosted">是否是取我回复过的</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>主题帖列表</returns>
        PagingDataSet<BarThread> GetUserThreads(string tenantTypeId, long userId, bool ignoreAudit, bool isPosted, int pageIndex, long? sectionId);

        /// <summary>
        /// 获取主题帖的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="sortBy">主题帖排序依据</param>
        /// <returns></returns>
        IEnumerable<BarThread> GetTops(string tenantTypeId, int topNumber, bool? isEssential, SortBy_BarThread sortBy);

        /// <summary>
        /// 获取群组贴吧的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="sortBy">主题帖排序依据</param>
        /// <returns></returns>
        IEnumerable<BarThread> GetTopsThreadOfGroup(string tenantTypeId, int topNumber, bool? isEssential, SortBy_BarThread sortBy);

        /// <summary>
        /// 根据标签名获取主题帖排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="categoryId">主题帖分类Id</param>
        /// <param name="isEssential">是否为精华帖</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>主题帖列表</returns>
        PagingDataSet<BarThread> Gets(string tenantTypeId, string tageName, bool? isEssential, SortBy_BarThread sortBy, int pageIndex);
        /// <summary>
        /// 根据帖吧获取主题帖分页集合
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="isEssential">是否为精华帖</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>主题帖列表</returns>
        PagingDataSet<BarThread> Gets(long sectionId, long? categoryId, bool? isEssential, SortBy_BarThread sortBy, int pageIndex);

        /// <summary>
        /// 帖子管理时查询帖子分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="query">帖子查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖子分页集合</returns>
        PagingDataSet<BarThread> Gets(string tenantTypeId, BarThreadQuery query, int pageSize, int pageIndex);


        /// <summary>
        /// 更新置顶到期的帖子
        /// </summary>
        void ExpireStickyThreads();

        /// <summary>
        /// 获取帖子管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        Dictionary<string, long> GetManageableDatas(string tenantTypeId = null);

        /// <summary>
        /// 获取帖子统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null);

    }
}
