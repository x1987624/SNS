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
    /// 帖吧仓储接口
    /// </summary>
    public interface IBarSectionRepository : IRepository<BarSection>
    {
        /// <summary>
        /// 更新吧管理员列表
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <param name="managerIds">吧管理员用户Id集合</param>
        void UpdateManagerIds(long sectionId, IEnumerable<long> managerIds);

        /// <summary>
        /// 获取帖吧的管理员用户Id列表
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <returns>吧管理员用户Id列表</returns>
        IEnumerable<long> GetSectionManagerIds(long sectionId);

        /// <summary>
        /// 依据OwnerId获取单个帖吧（用于OwnerId与帖吧一对一关系）
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <returns>帖吧</returns>
        BarSection GetByOwnerId(string tenantTypeId, long ownerId);

        /// <summary>
        /// 获取用户申请过的帖吧列表（仅适用于租户类型Id为帖吧应用的情况）
        /// </summary>
        /// <param name="userId">创建者Id</param>
        /// <returns>帖吧列表</returns>
        IEnumerable<BarSection> GetsByUserId(long userId);

        /// <summary>
        /// 获取帖吧的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="sortBy">帖吧排序依据</param>
        /// <returns></returns>
        IEnumerable<BarSection> GetTops(string tenantTypeId, int topNumber, long? categoryId, SortBy_BarSection sortBy);

        /// <summary>
        /// 获取帖吧列表
        /// </summary>
        /// <remarks>在频道帖吧分类页使用</remarks>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="nameKeyword"></param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="sortBy">帖吧排序依据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖吧列表</returns>
        PagingDataSet<BarSection> Gets(string tenantTypeId, string nameKeyword, long? categoryId, SortBy_BarSection sortBy, int pageIndex);

        /// <summary>
        /// 帖吧管理时查询帖吧分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="query">帖吧查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖吧分页集合</returns>
        PagingDataSet<BarSection> Gets(string tenantTypeId, BarSectionQuery query, int pageSize, int pageIndex);

        /// <summary>
        /// 删除帖吧管理员
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="userId"></param>
        void DeleteManager(long sectionId, long userId);

        /// <summary>
        /// 获取帖吧管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        Dictionary<string, long> GetManageableDatas(string tenantTypeId = null);

        /// <summary>
        /// 获取帖吧统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null);

        /// <summary>
        /// 获取前N个贴吧标签ID（TagInOwner类型）
        /// </summary>
        /// <param name="topNumber"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        IEnumerable<long> GetTopTagsForBar(int topNumber, SortBy_Tag? sortBy);
    }
}