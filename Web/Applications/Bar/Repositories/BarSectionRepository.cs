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
using System.Text;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Repositories;
using Tunynet.Common;
using System.Linq;
using Tunynet.Utilities;

namespace Spacebuilder.Bar
{
    /// <summary>
    ///帖吧Repository
    /// </summary>
    public class BarSectionRepository : Repository<BarSection>, IBarSectionRepository
    {
        private int pageSize = 20;

        /// <summary>
        /// 创建帖吧
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override object Insert(BarSection entity)
        {
            var sectionId_object = base.Insert(entity);
            //更新用户申请过的帖吧列表缓存
            RealTimeCacheHelper.IncreaseAreaVersion("UserId", entity.UserId);
            return sectionId_object;
        }

        /// <summary>
        /// 删除帖吧
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override int Delete(BarSection entity)
        {
            
            //1、执行sql数组没必要先打开数据库连接
            //2、对应的计数数据需要删除吗？（删除主题帖等内容时也有同样问题）
            
            var sql = Sql.Builder.Append("delete from spb_BarSectionManagers where SectionId = @0", entity.SectionId);

            CreateDAO().Execute(sql);
            int affectCount = base.Delete(entity);
            return affectCount;
        }

        /// <summary>
        /// 删除帖吧管理员
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="userId"></param>
        public void DeleteManager(long sectionId, long userId)
        {
            var sql = Sql.Builder.Append("delete from spb_BarSectionManagers").Where("SectionId=@0", sectionId).Where("UserId=@0", userId);
            CreateDAO().Execute(sql);

            //更新帖吧实体缓存
            IEnumerable<long> managerIds = GetSectionManagerIds(sectionId);
            if (managerIds != null)
            {
                IList<long> userIds = managerIds.ToList();
                userIds.Remove(userId);
                cacheService.Set(GetCacheKey_SectionManagerIds(sectionId), userIds, CachingExpirationType.UsualObjectCollection);
            }
        }

        /// <summary>
        /// 更新吧管理员列表
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <param name="managerIds">吧管理员用户Id集合</param>
        public void UpdateManagerIds(long sectionId, IEnumerable<long> managerIds)
        {
            List<Sql> sqls = new List<Sql>();
            sqls.Add(Sql.Builder.Append("delete from spb_BarSectionManagers").Where("SectionId=@0", sectionId));

            if (managerIds != null)
            {
                foreach (var userId in managerIds)
                {
                    sqls.Add(Sql.Builder.Append("insert into spb_BarSectionManagers(SectionId,UserId) values(@0,@1)", sectionId, userId));
                }
            }
            CreateDAO().Execute(sqls);

            //更新缓存
            if (managerIds != null)
                cacheService.Set(GetCacheKey_SectionManagerIds(sectionId), managerIds, CachingExpirationType.UsualObjectCollection);
        }

        /// <summary>
        /// 获取帖吧的管理员用户Id列表
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <returns>吧管理员用户Id列表</returns>
        public IEnumerable<long> GetSectionManagerIds(long sectionId)
        {
            string cacheKey = GetCacheKey_SectionManagerIds(sectionId);
            List<long> sectionManagerIds = cacheService.Get<List<long>>(cacheKey);
            if (sectionManagerIds == null)
            {
                var sql = PetaPoco.Sql.Builder;
                sql.Select("UserId")
                   .From("spb_BarSectionManagers")
                   .Where("SectionId=@0", sectionId);
                sectionManagerIds = CreateDAO().Fetch<long>(sql);
                if (sectionManagerIds == null)
                    sectionManagerIds = new List<long>();
                cacheService.Add(cacheKey, sectionManagerIds, CachingExpirationType.UsualObjectCollection);
            }
            return sectionManagerIds;
        }

        /// <summary>
        /// 依据OwnerId获取单个帖吧（用于OwnerId与帖吧一对一关系）
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <returns>帖吧</returns>
        public BarSection GetByOwnerId(string tenantTypeId, long ownerId)
        {
            string cacheKey = GetCacheKey_BarSection(ownerId, tenantTypeId);
            BarSection section = cacheService.Get<BarSection>(cacheKey);
            if (section == null)
            {
                var sql = PetaPoco.Sql.Builder;
                sql.Select("*")
                   .From("spb_BarSections")
                   .Where("TenantTypeId=@0", tenantTypeId)
                   .Where("OwnerId=@0", ownerId);
                section = CreateDAO().FirstOrDefault<BarSection>(sql);
                if (section != null)
                    cacheService.Add(cacheKey, section, CachingExpirationType.UsualSingleObject);
            }
            return section;
        }

        /// <summary>
        /// 获取用户申请过的帖吧列表（仅适用于租户类型Id为帖吧应用的情况）
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="userId">创建者Id</param>
        /// <returns>帖吧列表</returns>
        public IEnumerable<BarSection> GetsByUserId(long userId)
        {
            string cacheKey = RealTimeCacheHelper.GetAreaVersion("UserId", userId) + userId + "SectionsOfUser";

            List<long> sectionIds = cacheService.Get<List<long>>(cacheKey);
            if (sectionIds == null)
            {
                var sql = PetaPoco.Sql.Builder;
                sql.Select("SectionId")
                   .From("spb_BarSections")
                   .Where("TenantTypeId=@0", TenantTypeIds.Instance().Bar())
                   .Where("UserId=@0", userId);
                sectionIds = CreateDAO().Fetch<long>(sql);
                if (sectionIds != null)
                    cacheService.Add(cacheKey, sectionIds, CachingExpirationType.UsualObjectCollection);
            }
            return PopulateEntitiesByEntityIds<long>(sectionIds);
        }

        /// <summary>
        /// 获取帖吧的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="sortBy">帖吧排序依据</param>
        /// <returns></returns>
        public IEnumerable<BarSection> GetTops(string tenantTypeId, int topNumber, long? categoryId, SortBy_BarSection sortBy)
        {
            return GetTopEntities(topNumber, CachingExpirationType.UsualObjectCollection, () =>
            {
                StringBuilder cacheKey = new StringBuilder();
                
                
                cacheKey.AppendFormat("BarSectionRanks::T-{0}:C-{1}:S-{2}", tenantTypeId, categoryId, (int)sortBy);
                return cacheKey.ToString();
            }
                        , () =>
                        {
                            var sql = GetSql_SectionsByCategoryId(tenantTypeId, string.Empty, categoryId, sortBy);
                            return sql;
                        });
        }

        /// <summary>
        /// 获取帖吧列表
        /// </summary>
        /// <remarks>在频道帖吧分类页使用</remarks>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="nameKeyword"></param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="sortBy">帖吧排序依据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖吧列表</returns>
        public PagingDataSet<BarSection> Gets(string tenantTypeId, string nameKeyword, long? categoryId, SortBy_BarSection sortBy, int pageIndex)
        {
            if (string.IsNullOrEmpty(nameKeyword))
                return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.UsualObjectCollection, () =>
                {
                    StringBuilder cacheKey = new StringBuilder();
                    cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.None));
                    cacheKey.AppendFormat("BarSections::T-{0}:C-{1}:S-{2}", tenantTypeId, categoryId, sortBy);
                    return cacheKey.ToString();
                }
                                        , () =>
                                        {
                                            return GetSql_SectionsByCategoryId(tenantTypeId, string.Empty, categoryId, sortBy);
                                        });
            else
            {
                var sql = GetSql_SectionsByCategoryId(tenantTypeId, nameKeyword, categoryId, sortBy);
                return GetPagingEntities(pageSize, pageIndex, sql);
            }
        }

        /// <summary>
        /// 获取类别下帖吧列表的SQL块
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="nameKeyword"></param>
        /// <param name="categoryId"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        private Sql GetSql_SectionsByCategoryId(string tenantTypeId, string nameKeyword, long? categoryId, SortBy_BarSection sortBy)
        {
            var sql = Sql.Builder;
            var whereSql = Sql.Builder;
            var orderSql = Sql.Builder;

            sql.Select("spb_BarSections.*")
            .From("spb_BarSections");

            whereSql.Where("TenantTypeId = @0", tenantTypeId)
            .Where("AuditStatus=@0", AuditStatus.Success)
            .Where("IsEnabled=@0", true);
            if (!string.IsNullOrEmpty(nameKeyword))
            {
                whereSql.Where("Name like @0", StringUtility.StripSQLInjection(nameKeyword) + "%");
            }
            if (categoryId != null && categoryId.Value > 0)
            {
                CategoryService categoryService = new CategoryService();
                IEnumerable<Category> categories = categoryService.GetDescendants(categoryId.Value);
                List<long> categoryIds = new List<long> { categoryId.Value };
                if (categories != null && categories.Count() > 0)
                    categoryIds.AddRange(categories.Select(n => n.CategoryId));
                sql.InnerJoin("tn_ItemsInCategories")
               .On("spb_BarSections.SectionId = tn_ItemsInCategories.ItemId");
                whereSql.Where("tn_ItemsInCategories.CategoryId in(@categoryIds)", new { categoryIds = categoryIds });
            }
            CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
            string countTableName = countService.GetTableName_Counts();

            switch (sortBy)
            {
                case SortBy_BarSection.DateCreated_Desc:
                    orderSql.OrderBy("DisplayOrder,SectionId desc");
                    break;
                case SortBy_BarSection.ThreadCount:
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().ThreadCount()))
                    .On("SectionId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                case SortBy_BarSection.ThreadAndPostCount:
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().ThreadAndPostCount()))
                    .On("SectionId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                case SortBy_BarSection.StageThreadAndPostCount:
                    StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().BarSection());
                    int stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().ThreadAndPostCount());
                    string stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().ThreadAndPostCount(), stageCountDays);
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                    .On("SectionId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                case SortBy_BarSection.FollowedCount:
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().FollowedCount()))
                    .On("SectionId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                default:
                    orderSql.OrderBy("SectionId desc");
                    break;
            }

            sql.Append(whereSql).Append(orderSql);
            return sql;
        }

        /// <summary>
        /// 帖吧管理时查询帖吧分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="query">帖吧查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖吧分页集合</returns>
        public PagingDataSet<BarSection> Gets(string tenantTypeId, BarSectionQuery query, int pageSize, int pageIndex)
        {
            var sql = Sql.Builder;
            sql.Select("spb_BarSections.*")
            .From("spb_BarSections");

            if (query.CategoryId != null && query.CategoryId.Value > 0)
            {
                CategoryService categoryService = new CategoryService();
                IEnumerable<Category> categories = categoryService.GetDescendants(query.CategoryId.Value);
                List<long> categoryIds = new List<long> { query.CategoryId.Value };
                if (categories != null && categories.Count() > 0)
                    categoryIds.AddRange(categories.Select(n => n.CategoryId));
                sql.InnerJoin("tn_ItemsInCategories")
               .On("spb_BarSections.SectionId = tn_ItemsInCategories.ItemId")
               .Where("tn_ItemsInCategories.CategoryId in(@categoryIds)", new { categoryIds = categoryIds });
            }

            sql.Where("TenantTypeId = @0", tenantTypeId);

            if (query.UserId != null && query.UserId.Value > 0)
                sql.Where("UserId = @0", query.UserId);

            if (query.AuditStatus != null)
                sql.Where("AuditStatus = @0", query.AuditStatus);

            if (query.IsEnabled != null)
                sql.Where("IsEnabled = @0", query.IsEnabled.Value);

            
            

            //todo:libsh,by zhengw:等宝声加上工具方法后，重新调整此处
            if (!string.IsNullOrEmpty(query.NameKeyword))
                sql.Where("Name like @0", StringUtility.StripSQLInjection(query.NameKeyword) + "%");


            sql.OrderBy("DisplayOrder,SectionId desc");

            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取帖吧管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            var dao = CreateDAO();
            Dictionary<string, long> manageableDatas = new Dictionary<string, long>();

            Sql sql = Sql.Builder;
            sql.Select("count(*)")
                .From("spb_BarSections")
                .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId=@0", tenantTypeId);
            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().SectionPendingCount(), dao.FirstOrDefault<long>(sql));
            sql = Sql.Builder;
            sql.Select("count(*)")
                .From("spb_BarSections")
                .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
                sql.Where("TenantTypeId=@0", tenantTypeId);

            manageableDatas.Add(ApplicationStatisticDataKeys.Instance().SectionAgainCount(), dao.FirstOrDefault<long>(sql));

            return manageableDatas;
        }

        /// <summary>
        /// 获取前N个贴吧标签ID（TagInOwner类型）
        /// </summary>
        /// <param name="topNumber"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        public IEnumerable<long> GetTopTagsForBar(int topNumber, SortBy_Tag? sortBy)
        {
            var sql = PetaPoco.Sql.Builder;
            sql.Select("Id")
                .From("tn_TagsInOwners")
                .InnerJoin("spb_BarSections")
                .On("tn_TagsInOwners.OwnerId = spb_BarSections.SectionId and spb_BarSections.TenantTypeId = @0 and tn_TagsInOwners.TenantTypeId = @1", 101200, 101202);

            switch (sortBy)
            {
                case SortBy_Tag.ItemCountDesc:
                    sql.OrderBy("ItemCount desc");
                    break;
                default:
                    sql.OrderBy("Id desc");
                    break;
            };

            IEnumerable<object> ids = CreateDAO().FetchTopPrimaryKeys<TagInOwner>(topNumber, sql);

            return ids.Cast<long>();

        }

        /// <summary>
        /// 获取帖吧统计数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            var dao = CreateDAO();
            string cacheKey = "BarSectionStatisticData";
            Dictionary<string, long> statisticDatas = null;
            if (statisticDatas == null)
            {
                statisticDatas = new Dictionary<string, long>();
                Sql sql = Sql.Builder;
                sql.Select("count(*)")
                    .From("spb_BarSections");
                if (!string.IsNullOrEmpty(tenantTypeId))
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), dao.FirstOrDefault<long>(sql));

                sql = Sql.Builder;
                sql.Select("count(*)")
                    .From("spb_BarSections")
                    .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
                if (!string.IsNullOrEmpty(tenantTypeId))
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                statisticDatas.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), dao.FirstOrDefault<long>(sql));
                cacheService.Add(cacheKey, statisticDatas, CachingExpirationType.UsualSingleObject);
            }

            return statisticDatas;
        }

        #region CacheKey

        /// <summary>
        /// 获取帖吧缓存Key
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="tenantTypeId"></param>
        /// <returns></returns>
        private string GetCacheKey_BarSection(long ownerId, string tenantTypeId)
        {
            return string.Format("BarSection::O-{0}:T-{1}", ownerId, tenantTypeId);
        }

        /// <summary>
        /// 帖吧管理员缓存
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        private string GetCacheKey_SectionManagerIds(long sectionId)
        {
            return string.Format("SectionManagerIds::S-{0}", sectionId);
        }

        #endregion




    }
}