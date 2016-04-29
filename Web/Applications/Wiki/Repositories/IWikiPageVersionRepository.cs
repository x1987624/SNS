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
    /// 词条版本仓储接口
    /// </summary>
    public interface IWikiPageVersionRepository : IRepository<WikiPageVersion>
    {

        PagingDataSet<WikiPageVersion> GetPageVersionsOfPage(long pageId, int pageSize = 20, int pageIndex = 1);

        PagingDataSet<WikiPageVersion> GetPageVersionsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, long? ownerId, string titleKeywords, int pageSize = 20, int pageIndex = 1);

        /// <summary>
        /// 获取词条管版本理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>词条版本管理数据</returns>
        Dictionary<string, long> GetManageableDatasForWikiVersion(string tenantTypeId = null);
    }

}
