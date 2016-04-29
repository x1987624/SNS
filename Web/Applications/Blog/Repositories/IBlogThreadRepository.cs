//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-10-16</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2012-10-16" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Repositories;

namespace Spacebuilder.Blog
{
    /// <summary>
    /// 日志仓储接口
    /// </summary>
    public interface IBlogThreadRepository : IRepository<BlogThread>
    {
        /// <summary>
        /// 加精/取消精华
        /// </summary>
        /// <param name="threadId">日志ID</param>
        /// <param name="isEssential">是否精华</param>
        void SetEssential(long threadId, bool isEssential);

        /// <summary>
        /// 删除某用户时指定其他用户接管数据
        /// </summary>
        /// <param name="userId">要删日志的用户Id</param>
        /// <param name="takeOverUser">要接管日志的用户</param>
        void TakeOver(long userId, User takeOverUser);

        /// <summary>
        /// 上一日志ThreadId
        /// </summary>
        /// <param name="blogThread">当前日志</param>
        /// <returns>上一日志ThreadId</returns>
        long GetPrevThreadId(BlogThread blogThread);

        /// <summary>
        /// 下一日志ThreadId
        /// </summary>
        /// <param name="blogThread">当前日志</param>
        /// <returns>下一日志ThreadId</returns>
        long GetNextThreadId(BlogThread blogThread);

        /// <summary>
        /// 获取Owner的日志
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="ignorePrivate">是否忽略私有</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isSticky">是否置顶的排在前面</param>
        /// <param name="pageSize">pageSize</param> 
        /// <param name="pageIndex">页码</param>        
        /// <returns>主题帖列表</returns>
        PagingDataSet<BlogThread> GetOwnerThreads(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate, long? categoryId, string tagName, bool isSticky, int pageSize = 20, int pageIndex = 1);

        /// <summary>
        /// 获取Owner的草稿
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="ownerId">所有者id</param>
        /// <returns>日志草稿列表</returns>
        IEnumerable<BlogThread> GetDraftThreads(string tenantTypeId, long ownerId);

        /// <summary>
        /// 获取归档项目
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">OwnerId</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="ignorePrivate">是否忽略私有</param>
        /// <returns>归档项目列表</returns>
        IEnumerable<ArchiveItem> GetArchiveItems(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate);

        /// <summary>
        /// 获取存档的日志分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">ownerId</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="ignorePrivate">是否忽略私有</param>
        /// <param name="archivePeriod">归档阶段</param>
        /// <param name="archiveItem">归档日期标识</param>
        /// <param name="pageSize">pageSize</param> 
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        PagingDataSet<BlogThread> GetsForArchive(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate, ArchivePeriod archivePeriod, ArchiveItem archiveItem, int pageSize = 20, int pageIndex = 1);

        /// <summary>
        /// 获取日志的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">日志排序条件</param>
        /// <returns>日志列表</returns>
        IEnumerable<BlogThread> GetTops(string tenantTypeId, int topNumber, long? categoryId, bool? isEssential, SortBy_BlogThread sortBy);

        /// <summary>
        /// 获取日志排行分页集合
        /// </summary>
        /// <remarks>rss订阅也使用方法</remarks>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">OwnerId</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageSize">分页大小</param> 
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        PagingDataSet<BlogThread> Gets(string tenantTypeId, long? ownerId, bool ignoreAudit, bool ignorePrivate, long? categoryId, string tagName, bool? isEssential, SortBy_BlogThread sortBy = SortBy_BlogThread.DateCreated_Desc, int pageSize = 20, int pageIndex = 1);


        /// <summary>
        /// 获取日志排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="categoryId">站点类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="ownerId">OwnerId</param>
        /// <param name="subjectKeywords">标题关键字</param>
        /// <param name="pageSize">分页大小</param> 
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        PagingDataSet<BlogThread> GetsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, bool? isEssential, long? ownerId, string subjectKeywords, int pageSize = 20, int pageIndex = 1);

        /// <summary>
        /// 获取日志管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>日志管理数据</returns>
        Dictionary<string, long> GetManageableDatas(string tenantTypeId = null);

        /// <summary>
        /// 获取日志统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>日志统计数据</returns>
        Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null);
    }

}
