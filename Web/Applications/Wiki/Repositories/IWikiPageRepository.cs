//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2013</copyright>
//<version>V0.5</verion>
//<createdate>2013-06-22</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2013-06-22" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Repositories;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科仓储接口
    /// </summary>
    public interface IWikiPageRepository : IRepository<WikiPage>
    {
        /// <summary>
        /// 删除某用户时指定其他用户接管数据
        /// </summary>
        /// <param name="userId">要删百科的用户Id</param>
        /// <param name="takeOverUser">要接管百科的用户</param>
        void TakeOver(long userId, User takeOverUser);

        /// <summary>
        /// 获取Owner的百科
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="pageSize">pageSize</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>主题帖列表</returns>
        PagingDataSet<WikiPage> GetOwnerPages(string tenantTypeId, long ownerId, bool ignoreAudit, string tagName, int pageSize = 20, int pageIndex = 1);

        /// <summary>
        /// 获取百科的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">百科排序条件</param>
        /// <returns>百科列表</returns>
        IEnumerable<WikiPage> GetTops(string tenantTypeId, int topNumber, long? categoryId, bool? isEssential, SortBy_WikiPage sortBy);

        /// <summary>
        /// 获取百科排行分页集合
        /// </summary>
        /// <remarks>rss订阅也使用方法</remarks>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">OwnerId</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageSize">分页大小</param> 
        /// <param name="pageIndex">页码</param>
        /// <returns>百科分页列表</returns>
        PagingDataSet<WikiPage> Gets(string tenantTypeId, long? ownerId, bool ignoreAudit, long? categoryId, string tagName, bool? isEssential, SortBy_WikiPage sortBy = SortBy_WikiPage.DateCreated_Desc, int pageSize = 20, int pageIndex = 1);


        /// <summary>
        /// 获取百科排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="categoryId">站点类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="ownerId">OwnerId</param>
        /// <param name="subjectKeywords">标题关键字</param>
        /// <param name="pageSize">分页大小</param> 
        /// <param name="pageIndex">页码</param>
        /// <returns>百科分页列表</returns>
        PagingDataSet<WikiPage> GetsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, bool? isEssential, long? ownerId, string subjectKeywords, int pageSize = 20, int pageIndex = 1);

        /// <summary>
        /// 获取百科管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>百科管理数据</returns>
        Dictionary<string, long> GetManageableDatas(string tenantTypeId = null);

        /// <summary>
        /// 获取百科统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>百科统计数据</returns>
        Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null);

        /// <summary>
        /// 根据词条名获取词条Id
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        long GetPageIdByTitle(string title);

        /// <summary>
        /// 获取问答的标题和内容
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        void GetAskTitleAndBody(long questionId, out string title, out string body);

        PagingDataSet<WikiPage> GetUserEditedPages(string tenantTypeId, long userId, bool ignoreAudit, int pageSize, int pageIndex);

        PagingDataSet<WikiPage> GetPerfectPages(string tenantTypeId, long userId, int pageSize, int pageIndex);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        PagingDataSet<WikiPage> Gets(string keyWord, string tenantTypeId, long? ownerId, bool ignoreAudit, long? categoryId, string tagName, bool? isEssential, SortBy_WikiPage sortBy = SortBy_WikiPage.DateCreated_Desc, int pageSize = 20, int pageIndex = 1);

    }
}
