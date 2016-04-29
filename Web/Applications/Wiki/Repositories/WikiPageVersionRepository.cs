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
using Tunynet.Caching;
using PetaPoco;
using Tunynet.Utilities;
using System.Linq;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 词条版本仓储接口
    /// </summary>
    public class WikiPageVersionRepository : Repository<WikiPageVersion>, IWikiPageVersionRepository
    {
        /// <summary>
        /// 获取解析的正文
        /// </summary>
        /// <param name="versionId">词条版本id</param>
        /// <returns>解析后的VersionId正文</returns>
        public string GetResolvedBody(long versionId)
        {
            string cacheKey = GetCacheKeyOfResolvedBody(versionId);
            string resolveBody = cacheService.Get<string>(cacheKey);

            if (resolveBody == null)
            {
                resolveBody = GetBody(versionId);
                if (!string.IsNullOrEmpty(resolveBody))
                {
                    IBodyProcessor versionBodyProcessor = DIContainer.ResolveNamed<IBodyProcessor>(TenantTypeIds.Instance().WikiPage());
                    WikiPageVersion wikiPageVersion = Get(versionId);
                    resolveBody = versionBodyProcessor.Process(resolveBody, TenantTypeIds.Instance().WikiPage(), wikiPageVersion.PageId, wikiPageVersion.UserId);
                    cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
                }
            }

            return resolveBody;
        }

        /// <summary>
        /// 获取词条版本正文内容
        /// </summary>
        /// <param name="versionId">日志id</param>
        /// <returns>词条版本正文内容</returns>
        public string GetBody(long versionId)
        {
            string cacheKey = RealTimeCacheHelper.GetCacheKeyOfEntityBody(versionId);
            string body = cacheService.Get<string>(cacheKey);

            if (body == null)
            {
                WikiPageVersion wikiPageVersion = CreateDAO().SingleOrDefault<WikiPageVersion>(versionId);
                if (wikiPageVersion == null)
                {
                    return string.Empty;
                }

                body = wikiPageVersion.Body;
                cacheService.Add(cacheKey, body, CachingExpirationType.SingleObject);
            }

            return body;
        }

        /// <summary>
        /// 获取词条的最新版本
        /// </summary>
        /// <param name="pageId">词条Id</param>
        /// <returns></returns>
        public WikiPageVersion GetLastestVersion(long pageId)
        {
            string cacheKey = GetCacheKeyOfLastestVersion(pageId);
            long? versionId = cacheService.Get(cacheKey) as long?;
            if (versionId == null)
            {
                versionId = 0;
                var sql = Sql.Builder.Select("VersionId")
                .From("spb_WikiPageVersions")
                .Where("PageId = @0", pageId)
                .Where("AuditStatus=@0", AuditStatus.Success)
                .OrderBy("VersionId desc");
                var ids_object = CreateDAO().FetchTopPrimaryKeys<WikiPageVersion>(1, sql);
                if (ids_object.Count() > 0)
                    versionId = ids_object.Cast<long>().First();
                cacheService.Add(cacheKey, versionId, CachingExpirationType.SingleObject);
            }
            if (versionId.HasValue && versionId.Value > 0)
                return Get(versionId.Value);
            return null;
        }

        /// <summary>
        /// 获取词条的版本分页集合
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<WikiPageVersion> GetPageVersionsOfPage(long pageId, int pageSize = 20, int pageIndex = 1)
        {
            var sql = Sql.Builder.Select("VersionId").From("spb_WikiPageVersions").Where("PageId = @0", pageId).OrderBy("VersionId desc");
            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="auditStatus"></param>
        /// <param name="categoryId"></param>
        /// <param name="ownerId"></param>
        /// <param name="titleKeywords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<WikiPageVersion> GetPageVersionsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, long? ownerId, string titleKeywords, int pageSize = 20, int pageIndex = 1)
        {
            var sql = Sql.Builder.Select("spb_WikiPageVersions.VersionId").From("spb_WikiPageVersions");
            var whereSql = Sql.Builder;

            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                whereSql.Where("spb_WikiPageVersions.TenantTypeId=@0", tenantTypeId);
            }

            if (auditStatus.HasValue)
            {
                whereSql.Where("spb_WikiPageVersions.AuditStatus=@0", auditStatus.Value);
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                sql.InnerJoin("tn_ItemsInCategories").On("spb_WikiPageVersions.PageId=tn_ItemsInCategories.ItemId");
                whereSql.Where("tn_ItemsInCategories.CategoryId=@0", categoryId.Value);
            }

            if (ownerId.HasValue)
            {
                whereSql.Where("spb_WikiPageVersions.OwnerId=@0", ownerId);
            }

            if (!string.IsNullOrEmpty(titleKeywords))
            {
                whereSql.Where("spb_WikiPageVersions.Title like @0", "%" + StringUtility.StripSQLInjection(titleKeywords) + "%");
            }

            sql.Append(whereSql).OrderBy("spb_WikiPageVersions.VersionId desc");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取解析正文缓存Key
        /// </summary>
        /// <param name="versionId">词条版本id</param>
        /// <returns>正文缓存key</returns>
        private string GetCacheKeyOfResolvedBody(long versionId)
        {
            return "WikiPageVersionResolvedBody-" + versionId;
        }

        /// <summary>
        /// 获取词条的最新版本缓存Key
        /// </summary>
        /// <param name="pageId">词条id</param>
        /// <returns>缓存key</returns>
        private string GetCacheKeyOfLastestVersion(long pageId)
        {
            return RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "PageId", pageId) + "LastestVersion";
        }

        /// <summary>
        /// 获取词条管版本理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>词条版本管理数据</returns>
        public Dictionary<string, long> GetManageableDatasForWikiVersion(string tenantTypeId = null)
        {
            Dictionary<string, long> manageableDatas = new Dictionary<string, long>();

            //查询待审核数
            Sql sql = Sql.Builder.Select("count(*)")
                                 .From("spb_WikiPageVersions")
                                 .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), CreateDAO().FirstOrDefault<long>(sql));

            //查询需再审核数
            sql = Sql.Builder.Select("count(*)")
                             .From("spb_WikiPageVersions")
                             .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), CreateDAO().FirstOrDefault<long>(sql));

            return manageableDatas;
        }

    }
}
