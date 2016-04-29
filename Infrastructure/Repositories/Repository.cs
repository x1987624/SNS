//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-8</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-8" version="0.5">创建</log>
//<log date="2011-10-27" version="0.51" reviewer="libsh">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Tunynet.Caching;
using Tunynet;
using PetaPoco;
using PetaPoco.Internal;

namespace Tunynet.Repositories
{
    /// <summary>
    /// Repository实现
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class,IEntity
    {
        /// <summary>
        /// 缓存设置
        /// </summary>
        protected static RealTimeCacheHelper RealTimeCacheHelper { get { return EntityData.ForType(typeof(TEntity)).RealTimeCacheHelper; } }

        /// <summary>
        /// 缓存服务
        /// </summary> 
        public ICacheService cacheService = DIContainer.Resolve<ICacheService>();

        /// <summary>
        /// 数据库DAO对象
        /// </summary>
        private Database database;
        /// <summary>
        /// 默认PetaPocoDatabase实例
        /// </summary>
        protected virtual Database CreateDAO()
        {
            if (database == null)
            {
                database = Database.CreateInstance();
            }

            return database;
        }

        #region IRepository<TEntity> 成员

        /// <summary>
        /// 把实体entity添加到数据库
        /// </summary>
        /// <param name="entity">实体</param>
        public virtual object Insert(TEntity entity)
        {
            //处理可序列化属性
            if (entity is ISerializableProperties)
            {
                ISerializableProperties extendedProperties = entity as ISerializableProperties;
                if (extendedProperties != null)
                    extendedProperties.Serialize();
            }

            var id = CreateDAO().Insert(entity);

            OnInserted(entity);

            return id;
        }

        /// <summary>
        /// 把实体entiy更新到数据库
        /// </summary>
        /// <param name="entity">实体</param>
        public virtual void Update(TEntity entity)
        {
            Database dao = CreateDAO();

            int affectedRecords;

            //处理可序列化属性
            if (entity is ISerializableProperties)
            {
                ISerializableProperties extendedProperties = entity as ISerializableProperties;
                if (extendedProperties != null)
                    extendedProperties.Serialize();
            }

            //设置实体正文缓存时如果未给实体正文赋值则不更新
            if (RealTimeCacheHelper.PropertyNameOfBody != null && RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) == null)
            {
                var pd = PocoData.ForType(typeof(TEntity));
                List<string> columns = new List<string>();
                foreach (var column in pd.Columns)
                {
                    //去掉主键
                    if (string.Compare(column.Key, pd.TableInfo.PrimaryKey, true) == 0)
                        continue;

                    //去掉不允许update的属性
                    if ((SqlBehaviorFlags.Update & column.Value.SqlBehavior) == 0)
                        continue;

                    //去掉实体正文
                    if (string.Compare(column.Key, RealTimeCacheHelper.PropertyNameOfBody.Name, true) == 0)
                        continue;

                    // Dont update result only columns
                    if (column.Value.ResultColumn)
                        continue;

                    columns.Add(column.Key);
                }

                affectedRecords = dao.Update(entity, columns);
            }
            else
            {
                affectedRecords = dao.Update(entity);
            }

            if (affectedRecords > 0)
                OnUpdated(entity);
        }

        /// <summary>
        /// 从数据库删除实体(by EntityId)
        /// </summary>
        /// <param name="entityId">主键</param>
        /// <returns>影响的记录数</returns>
        public virtual int DeleteByEntityId(object entityId)
        {
            TEntity entity = Get(entityId);
            if (entity == null)
                return 0;
            else
                return Delete(entity);
        }

        /// <summary>
        /// 从数据库删除实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>影响的记录数</returns>
        public virtual int Delete(TEntity entity)
        {
            if (entity == null)
                return 0;

            int affectedRecords = CreateDAO().Delete(entity);
            if (affectedRecords > 0)
                OnDeleted(entity);

            return affectedRecords;
        }

        /// <summary>
        /// 依据主键检查实体是否存在于数据库
        /// </summary>
        /// <param name="entityId">主键</param>
        public bool Exists(object entityId)
        {
            return CreateDAO().Exists<TEntity>(entityId);
        }

        /// <summary>
        /// 依据EntityId获取单个实体
        /// </summary>
        /// <remarks>
        /// 自动对实体进行缓存（除非实体配置为不允许缓存）
        /// </remarks>
        /// <param name="entityId">实体Id</param>
        public virtual TEntity Get(object entityId)
        {
            TEntity entity = null;
            if (RealTimeCacheHelper.EnableCache)
                entity = cacheService.Get<TEntity>(RealTimeCacheHelper.GetCacheKeyOfEntity(entityId));

            if (entity == null)
            {
                entity = CreateDAO().SingleOrDefault<TEntity>(entityId);

                #region 处理缓存
                if (RealTimeCacheHelper.EnableCache && entity != null)
                {
                    if (RealTimeCacheHelper.PropertyNameOfBody != null)
                    {
                        //启用实体正文缓存时，不在实体缓存中存储实体正文
                        RealTimeCacheHelper.PropertyNameOfBody.SetValue(entity, null, null);
                    }
                    cacheService.Add(RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, RealTimeCacheHelper.CachingExpirationType);
                }
                #endregion
            }

            if (entity == null || entity.IsDeletedInDatabase)
                return null;

            return entity;
        }

        /// <summary>
        /// 获取所有实体（仅用于数据量少的情况）
        /// </summary>
        /// <remarks>
        /// 自动对进行缓存（缓存策略与实体配置的缓存策略相同）
        /// </remarks>
        /// <returns>返回所有实体集合</returns>
        public IEnumerable<TEntity> GetAll()
        {
            return GetAll(null);
        }

        /// <summary>
        /// 获取所有实体（仅用于数据量少的情况）
        /// </summary>        
        /// <remarks>
        /// 自动对进行缓存（缓存策略与实体配置的缓存策略相同）
        /// </remarks>
        /// <param name="orderBy">排序字段（多个字段用逗号分隔）</param>
        /// <returns>返回按照orderBy排序的所有实体集合</returns>
        public IEnumerable<TEntity> GetAll(string orderBy)
        {
            IEnumerable<object> entityIds = null;
            string cacheKey = null;

            if (RealTimeCacheHelper.EnableCache)
            {
                cacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.GlobalVersion);
                if (!string.IsNullOrEmpty(orderBy))
                    cacheKey += "SB-" + orderBy;

                entityIds = cacheService.Get<IEnumerable<object>>(cacheKey);
            }
            if (entityIds == null)
            {
                PocoData pocoData = PocoData.ForType(typeof(TEntity));

                var sql = PetaPoco.Sql.Builder
                    .Select(pocoData.TableInfo.PrimaryKey)
                    .From(pocoData.TableInfo.TableName);

                if (!string.IsNullOrEmpty(orderBy))
                    sql.OrderBy(orderBy);

                entityIds = CreateDAO().FetchFirstColumn(sql);

                if (RealTimeCacheHelper.EnableCache)
                    cacheService.Add(cacheKey, entityIds, RealTimeCacheHelper.CachingExpirationType);
            }
            IEnumerable<TEntity> entities = PopulateEntitiesByEntityIds(entityIds);
            return entities;
        }

        #endregion

        #region 配置属性

        private int cacheablePageCount = 30;
        /// <summary>
        /// 可缓存的列表缓存页数
        /// </summary>
        protected virtual int CacheablePageCount
        {
            get { return cacheablePageCount; }
        }

        private int primaryMaxRecords = 50000;
        /// <summary>
        /// 主流查询最大允许返回记录数
        /// </summary>
        protected virtual int PrimaryMaxRecords
        {
            get { return primaryMaxRecords; }
        }

        private int secondaryMaxRecords = 1000;
        /// <summary>
        /// 非主流查询最大允许返回记录数
        /// </summary>
        /// <remarks>
        /// 例如：排行数据
        /// </remarks>
        protected virtual int SecondaryMaxRecords
        {
            get { return secondaryMaxRecords; }
        }


        #endregion


        #region Help Methods

        /// <summary>
        /// 获取分页查询数据
        /// </summary>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">当前页码(从1开始)</param>
        /// <param name="sql">获取当前页码的数据的<see cref="PetaPoco.Sql">PetaPoco.Sql</see></param>
        /// <returns></returns>
        protected virtual PagingDataSet<TEntity> GetPagingEntities(int pageSize, int pageIndex, PetaPoco.Sql sql)
        {
            PagingEntityIdCollection peic = CreateDAO().FetchPagingPrimaryKeys<TEntity>(PrimaryMaxRecords, pageSize, pageIndex, sql);
            IEnumerable<TEntity> entitiesOfPage = PopulateEntitiesByEntityIds(peic.GetPagingEntityIds(pageSize, pageIndex));
            PagingDataSet<TEntity> pagingEntities = new PagingDataSet<TEntity>(entitiesOfPage)
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = peic.TotalRecords
            };

            return pagingEntities;
        }

        /// <summary>
        /// 获取分页查询数据（启用缓存）
        /// </summary>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">当前页码(从1开始)</param>
        /// <param name="cachingExpirationTypes">缓存策略</param>
        /// <param name="getCacheKey">生成cacheKey的委托</param>
        /// <param name="generateSql">生成PetaPoco.Sql的委托</param>        
        /// <returns></returns>
        protected virtual PagingDataSet<TEntity> GetPagingEntities(int pageSize, int pageIndex, CachingExpirationType cachingExpirationTypes, Func<string> getCacheKey, Func<PetaPoco.Sql> generateSql)
        {
            PagingEntityIdCollection peic = null;

            //modified by jiangshl:分页过大时缓存多页没有意义，所以加了pageSize <= SecondaryMaxRecords的限制
            if (pageIndex < CacheablePageCount && pageSize <= SecondaryMaxRecords)
            {
                string cacheKey = getCacheKey();
                peic = cacheService.Get<PagingEntityIdCollection>(cacheKey);
                if (peic == null)
                {
                    peic = CreateDAO().FetchPagingPrimaryKeys<TEntity>(PrimaryMaxRecords, pageSize * CacheablePageCount, 1, generateSql());
                    peic.IsContainsMultiplePages = true;
                    cacheService.Add(cacheKey, peic, cachingExpirationTypes);
                }
            }
            else
            {
                peic = CreateDAO().FetchPagingPrimaryKeys<TEntity>(PrimaryMaxRecords, pageSize, pageIndex, generateSql());
            }

            IEnumerable<TEntity> entitiesOfPage = PopulateEntitiesByEntityIds(peic.GetPagingEntityIds(pageSize, pageIndex));
            PagingDataSet<TEntity> pagingEntities = new PagingDataSet<TEntity>(entitiesOfPage)
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRecords = peic.TotalRecords
            };

            return pagingEntities;
        }


        /// <summary>
        /// 获取前topNumber条Entity（启用缓存）
        /// </summary>
        /// <remarks>
        /// 一次性取出前SecondaryMaxRecords条记录
        /// </remarks>
        /// <param name="topNumber"></param>
        /// <param name="cachingExpirationTypes">缓存策略</param>
        /// <param name="getCacheKey">生成cacheKey的委托（CacheKey不要与topNumber相关）</param>
        /// <param name="generateSql">生成PetaPoco.Sql的委托</param>     
        /// <returns></returns>
        protected virtual IEnumerable<TEntity> GetTopEntities(int topNumber, CachingExpirationType cachingExpirationTypes, Func<string> getCacheKey, Func<PetaPoco.Sql> generateSql)
        {
            PagingEntityIdCollection peic = null;
            string cacheKey = getCacheKey();
            peic = cacheService.Get<PagingEntityIdCollection>(cacheKey);
            if (peic == null)
            {
                IEnumerable<object> entityIds = CreateDAO().FetchTopPrimaryKeys<TEntity>(SecondaryMaxRecords, generateSql());
                peic = new PagingEntityIdCollection(entityIds);
                cacheService.Add(cacheKey, peic, cachingExpirationTypes);
            }

            IEnumerable<object> topEntityIds = peic.GetTopEntityIds(topNumber);
            return PopulateEntitiesByEntityIds(topEntityIds);
        }


        /// <summary>
        /// 依据EntityId集合组装成实体集合（自动缓存）
        /// </summary>
        /// <param name="entityIds">主键集合</param>
        public virtual IEnumerable<TEntity> PopulateEntitiesByEntityIds<T>(IEnumerable<T> entityIds)
        {
            TEntity[] entityArray = new TEntity[entityIds.Count()];
            Dictionary<object, int> entityId2ArrayIndex = new Dictionary<object, int>();
            for (int i = 0; i < entityIds.Count(); i++)
            {
                TEntity entity = cacheService.Get<TEntity>(RealTimeCacheHelper.GetCacheKeyOfEntity(entityIds.ElementAt(i)));
                if (entity != null)
                {
                    entityArray[i] = entity;
                }
                else
                {
                    entityArray[i] = null;
                    entityId2ArrayIndex[entityIds.ElementAt(i)] = i;
                }
            }

            //缓存中取不到的实体集中从数据库获取
            if (entityId2ArrayIndex.Count > 0)
            {
                IEnumerable<TEntity> entitiesFromDatabase = CreateDAO().FetchByPrimaryKeys<TEntity>(entityId2ArrayIndex.Keys);
                foreach (var entityFromDatabase in entitiesFromDatabase)
                {
                    entityArray[entityId2ArrayIndex[entityFromDatabase.EntityId]] = entityFromDatabase;

                    #region 处理缓存
                    if (RealTimeCacheHelper.EnableCache && entityFromDatabase != null)
                    {
                        if (RealTimeCacheHelper.PropertyNameOfBody != null)
                        {
                            if (RealTimeCacheHelper.PropertyNameOfBody != null)
                            {
                                //启用实体正文缓存时，不在实体缓存中存储实体正文
                                RealTimeCacheHelper.PropertyNameOfBody.SetValue(entityFromDatabase, null, null);
                            }
                        }
                        cacheService.Set(RealTimeCacheHelper.GetCacheKeyOfEntity(entityFromDatabase.EntityId), entityFromDatabase, RealTimeCacheHelper.CachingExpirationType);
                    }
                    #endregion
                }
            }

            List<TEntity> entities = new List<TEntity>();
            foreach (var entity in entityArray)
            {
                if (entity != null && !entity.IsDeletedInDatabase)
                    entities.Add(entity);
            }

            return entities;
        }

        #endregion


        #region 回调或触发事件

        /// <summary>
        /// 数据库新增实体后自动调用该方法
        /// </summary>
        protected virtual void OnInserted(TEntity entity)
        {
            #region 处理缓存
            if (RealTimeCacheHelper.EnableCache)
            {
                //更新列表缓存版本
                RealTimeCacheHelper.IncreaseListCacheVersion(entity);

                if (RealTimeCacheHelper.PropertyNameOfBody != null)
                {
                    string body = RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) as string;
                    cacheService.Add(RealTimeCacheHelper.GetCacheKeyOfEntityBody(entity.EntityId), body, RealTimeCacheHelper.CachingExpirationType);

                    //启用实体正文缓存时，不在实体缓存中存储实体正文
                    RealTimeCacheHelper.PropertyNameOfBody.SetValue(entity, null, null);
                }
                cacheService.Add(RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, RealTimeCacheHelper.CachingExpirationType);
            }
            #endregion
        }

        /// <summary>
        /// 数据库更新实体后自动调用该方法
        /// </summary>
        protected virtual void OnUpdated(TEntity entity)
        {
            #region 处理缓存
            if (RealTimeCacheHelper.EnableCache)
            {
                //更新实体缓存版本
                RealTimeCacheHelper.IncreaseEntityCacheVersion(entity.EntityId);
                //更新列表缓存版本
                RealTimeCacheHelper.IncreaseListCacheVersion(entity);

                if (RealTimeCacheHelper.PropertyNameOfBody != null && RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) != null)
                {
                    string body = RealTimeCacheHelper.PropertyNameOfBody.GetValue(entity, null) as string;
                    cacheService.Set(RealTimeCacheHelper.GetCacheKeyOfEntityBody(entity.EntityId), body, RealTimeCacheHelper.CachingExpirationType);

                    //启用实体正文缓存时，不在实体缓存中存储实体正文
                    RealTimeCacheHelper.PropertyNameOfBody.SetValue(entity, null, null);
                }
                cacheService.Set(RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, RealTimeCacheHelper.CachingExpirationType);
            }

            #endregion
        }

        /// <summary>
        /// 数据库删除实体后自动调用该方法
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void OnDeleted(TEntity entity)
        {
            #region 处理缓存
            if (RealTimeCacheHelper.EnableCache)
            {
                //递增实体缓存版本
                RealTimeCacheHelper.IncreaseEntityCacheVersion(entity.EntityId);
                //更新列表缓存版本
                RealTimeCacheHelper.IncreaseListCacheVersion(entity);
                cacheService.MarkDeletion(RealTimeCacheHelper.GetCacheKeyOfEntity(entity.EntityId), entity, CachingExpirationType.SingleObject);
            }
            #endregion
        }

        #endregion
    }
}