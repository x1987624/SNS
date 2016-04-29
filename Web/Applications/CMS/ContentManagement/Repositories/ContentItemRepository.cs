//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Repositories;
using Spacebuilder.CMS.Metadata;
using PetaPoco;
using System.Data;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Utilities;
using System.Dynamic;
using Tunynet;
using Spacebuilder.Common;

namespace Spacebuilder.CMS
{
    public class ContentItemRepository : Repository<ContentItem>
    {
        /// <summary>
        /// 创建ContentItem
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        public override object Insert(ContentItem contentItem)
        {
            base.Insert(contentItem);

            ContentTypeDefinition contentType = contentItem.ContentType;
            if (contentType != null)
            {
                List<Sql> sqls = new List<Sql>();

                StringBuilder sqlBuilder = new StringBuilder();
                List<object> values = new List<object>();

                sqlBuilder.AppendFormat("INSERT INTO {0} (", contentType.TableName);

                int columnIndex = 0;

                sqlBuilder.Append(contentType.ForeignKey);
                values.Add(contentItem.ContentItemId);

                foreach (var column in contentType.Columns)
                {
                    if (contentType.ForeignKey == column.ColumnName)
                    {
                        continue;
                    }

                    if (contentItem.AdditionalProperties.Keys.Contains(column.ColumnName))
                    {
                        columnIndex++;
                        sqlBuilder.Append("," + column.ColumnName);
                        if (contentItem.AdditionalProperties[column.ColumnName] == null)
                            values.Add(column.DefaultValue);
                        else
                            values.Add(contentItem.AdditionalProperties[column.ColumnName]);
                    }
                    else
                    {
                        sqlBuilder.Append("," + column.ColumnName);
                        values.Add(column.DefaultValue);
                    }
                }
                sqlBuilder.Append(")");

                sqls.Add(Sql.Builder.Append(sqlBuilder.Append("values (@0)").ToString(), values));
                sqls.Add(Sql.Builder.Append("UPDATE spb_cms_ContentFolders SET ContentItemCount=ContentItemCount+1 WHERE ContentFolderId=@0", contentItem.ContentFolderId));

                CreateDAO().Execute(sqls);
            }

            string cacheKey = "ContentItemAP:" + RealTimeCacheHelper.GetCacheKeyOfEntity(contentItem.ContentItemId);
            cacheService.Remove(cacheKey);

            return contentItem.ContentItemId;
        }

        /// <summary>
        /// 更新ContentItem
        /// </summary>
        /// <param name="contentItem"></param>
        public override void Update(ContentItem contentItem)
        {
            ContentTypeDefinition contentType = contentItem.ContentType;
            if (contentType != null)
            {
                StringBuilder sqlBuilder = new StringBuilder();
                sqlBuilder.AppendFormat("UPDATE {0} SET ", contentType.TableName);

                List<object> values = new List<object>();

                int columnIndex = 0;
                foreach (var column in contentType.Columns)
                {
                    if (column.EnableEdit)
                    {
                        sqlBuilder.AppendFormat(" {0}=@{1},", column.ColumnName, columnIndex);
                        values.Add(contentItem.AdditionalProperties[column.ColumnName]);
                        columnIndex++;
                    }
                }
                //去除末尾","
                sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
                sqlBuilder.AppendFormat(" WHERE {0}=@{1}", contentType.ForeignKey, columnIndex);
                values.Add(contentItem.ContentItemId);

                Sql sql = Sql.Builder.Append(sqlBuilder.ToString(), values.ToArray());
                CreateDAO().Execute(sql);
            }

            string cacheKey = "ContentItemAP:" + RealTimeCacheHelper.GetCacheKeyOfEntity(contentItem.ContentItemId);
            cacheService.Remove(cacheKey);

            base.Update(contentItem);
            //更新解析正文缓存
            cacheKey = GetCacheKeyOfResolvedBody(contentItem.ContentItemId);
            string resolveBody = cacheService.Get<string>(cacheKey);
            if (resolveBody != null)
            {
                resolveBody = contentItem.AdditionalProperties.Get<string>("Body", string.Empty);
                resolveBody = DIContainer.ResolveNamed<IBodyProcessor>(TenantTypeIds.Instance().ContentItem()).Process(resolveBody, TenantTypeIds.Instance().ContentItem(), contentItem.ContentItemId, contentItem.UserId);
                cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
        }

        /// <summary>
        /// 删除ContentItem
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        public override int Delete(ContentItem contentItem)
        {
            ContentTypeDefinition contentType = contentItem.ContentType;
            if (contentType != null)
            {
                List<Sql> sqls = new List<Sql>();
                sqls.Add(Sql.Builder.Append(string.Format("DELETE FROM {0} WHERE {1}=@0", contentType.TableName, contentType.ForeignKey), contentItem.ContentItemId));
                sqls.Add(Sql.Builder.Append("UPDATE spb_cms_ContentFolders SET ContentItemCount=ContentItemCount-1 WHERE ContentFolderId=@0", contentItem.ContentFolderId));
                CreateDAO().Execute(sqls);
            }
            string cacheKey = "ContentItemAP:" + RealTimeCacheHelper.GetCacheKeyOfEntity(contentItem.ContentItemId);
            cacheService.Remove(cacheKey);
            return base.Delete(contentItem);
        }

        /// <summary>
        /// 删除某用户时指定其他用户接管数据
        /// </summary>
        /// <param name="userId">要删日志的用户Id</param>
        /// <param name="takeOverUser">要接管日志的用户</param>
        public void TakeOver(long userId, User takeOverUser)
        {
            List<Sql> sqls = new List<Sql>();

            //更新所属为用户的日志（UserId=OwnerId）
            sqls.Add(Sql.Builder.Append("update spb_cms_ContentItems set UserId = @0,Author=@1 where UserId=@2", takeOverUser.UserId, takeOverUser.DisplayName, userId));

            CreateDAO().Execute(sqls);
        }

        /// <summary>
        /// 批量移动ContentItem
        /// </summary>
        /// <param name="contentItems"></param>
        /// <param name="toContentFolderId"></param>
        public void Move(IEnumerable<ContentItem> contentItems, int toContentFolderId)
        {
            List<Sql> sqls = new List<Sql>();

            foreach (var contentFolderId in contentItems.Select(c => c.ContentFolderId))
            {
                sqls.Add(PetaPoco.Sql.Builder.Append("UPDATE spb_cms_ContentFolders Set ContentItemCount=ContentItemCount-1").Where("ContentFolderId = @0", contentFolderId));
            }
            sqls.Add(PetaPoco.Sql.Builder.Append("UPDATE spb_cms_ContentFolders Set ContentItemCount=ContentItemCount+@0", contentItems.Count()).Where("ContentFolderId = @0", toContentFolderId));

            CreateDAO().Execute(sqls);

            foreach (var contentItem in contentItems)
            {
                contentItem.ContentFolderId = toContentFolderId;
                Update(contentItem);
            }
        }

        /// <summary>
        /// 获取上一篇
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        public long GetPrevContentItemId(ContentItem contentItem)
        {
            Database dao = CreateDAO();
            string cacheKey = string.Format("CmsPrevContentItemId-{0}", contentItem.ContentItemId);
            long? prevContentItemId = cacheService.Get(cacheKey) as long?;
            if (prevContentItemId == null)
            {
                prevContentItemId = 0;
                var sql = Sql.Builder;
                sql.Select("ContentItemId")
                .From("spb_cms_ContentItems")
                .Where("LastModified > @0", contentItem.LastModified)
                .Where("ContentFolderId = @0", contentItem.ContentFolderId);

                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        sql.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        sql.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }

                sql.OrderBy("LastModified");
                var ids_object = dao.FetchTopPrimaryKeys<ContentItem>(1, sql);
                if (ids_object.Count() > 0)
                    prevContentItemId = ids_object.Cast<long>().First();
                cacheService.Add(cacheKey, prevContentItemId, CachingExpirationType.SingleObject);
            }
            return prevContentItemId ?? 0;
        }

        /// <summary>
        /// 获取下一篇
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        public long GetNextContentItemId(ContentItem contentItem)
        {
            Database dao = CreateDAO();
            string cacheKey = string.Format("CmsNextContentItemId-{0}", contentItem.ContentItemId);
            long? nextContentItemId = cacheService.Get(cacheKey) as long?;
            if (nextContentItemId == null)
            {
                nextContentItemId = 0;
                var sql = Sql.Builder;
                sql.Select("ContentItemId")
                .From("spb_cms_ContentItems")
                .Where("LastModified < @0", contentItem.LastModified)
                .Where("ContentFolderId = @0", contentItem.ContentFolderId);
                switch (this.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        sql.Where("AuditStatus=@0", this.PubliclyAuditStatus);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        sql.Where("AuditStatus>@0", this.PubliclyAuditStatus);
                        break;
                    default:
                        break;
                }
                sql.OrderBy("LastModified desc");
                var ids_object = dao.FetchTopPrimaryKeys<ContentItem>(1, sql);
                if (ids_object.Count() > 0)
                    nextContentItemId = ids_object.Cast<long>().First();
                cacheService.Add(cacheKey, nextContentItemId, CachingExpirationType.SingleObject);
            }
            return nextContentItemId ?? 0;
        }

        /// <summary>
        /// 获取解析的正文
        /// </summary>
        /// <param name="contentItemId"></param>
        /// <returns></returns>
        public string GetResolvedBody(long contentItemId)
        {
            ContentItem contentItem = Get(contentItemId);
            if (contentItem == null || string.IsNullOrEmpty(contentItem.AdditionalProperties.Get<string>("Body", string.Empty)))
                return string.Empty;

            string cacheKey = GetCacheKeyOfResolvedBody(contentItemId);
            string resolveBody = cacheService.Get<string>(cacheKey);
            if (resolveBody == null)
            {
                resolveBody = contentItem.AdditionalProperties.Get<string>("Body", string.Empty);
                resolveBody = DIContainer.ResolveNamed<IBodyProcessor>(TenantTypeIds.Instance().ContentItem()).Process(resolveBody, TenantTypeIds.Instance().ContentItem(), contentItem.ContentItemId, contentItem.UserId); cacheService.Set(cacheKey, resolveBody, CachingExpirationType.SingleObject);
            }
            return resolveBody;
        }


        /// <summary>
        /// 获取ContentItem附表数据
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="contentItemId"></param>
        /// <returns></returns>
        public IDictionary<string, object> GetContentItemAdditionalProperties(int contentTypeId, long contentItemId)
        {
            string cacheKey = "ContentItemAP:" + RealTimeCacheHelper.GetCacheKeyOfEntity(contentItemId);

            IDictionary<string, object> additionalProperties = cacheService.Get<IDictionary<string, object>>(cacheKey);
            if (additionalProperties == null)
            {
                ContentTypeDefinition contentType = new MetadataService().GetContentType(contentTypeId);
                if (contentType != null)
                {
                    additionalProperties = new Dictionary<string, object>();

                    Database database = CreateDAO();
                    database.OpenSharedConnection();
                    try
                    {
                        using (var cmd = database.CreateCommand(database.Connection, string.Format("SELECT * FROM  {0} WHERE {1} = @0", contentType.TableName, contentType.ForeignKey), contentItemId))
                        {
                            using (IDataReader dr = cmd.ExecuteReader())
                            {
                                if (dr.Read())
                                {
                                    foreach (var column in contentType.Columns)
                                    {
                                        if (dr[column.ColumnName] == null)
                                            additionalProperties.Add(column.ColumnName, column.DefaultValue);
                                        else
                                            additionalProperties.Add(column.ColumnName, dr[column.ColumnName]);
                                    }
                                }
                                dr.Close();
                            }
                        }
                    }
                    finally
                    {
                        database.CloseSharedConnection();
                    }

                    cacheService.Add(cacheKey, additionalProperties, CachingExpirationType.SingleObject);
                }
            }

            return additionalProperties;
        }

        /// <summary>
        /// 依据查询条件获取ContentItem列表
        /// </summary>
        public PagingDataSet<ContentItem> GetContentItems(bool enableCaching, ContentItemQuery query, int pageSize, int pageIndex)
        {
            var sql = Sql.Builder.Select("spb_cms_ContentItems.*").From("spb_cms_ContentItems");
            var whereSql = Sql.Builder;
            var orderSql = Sql.Builder;

            if (query.ContentFolderId.HasValue && query.ContentFolderId.Value > 0)
            {
                if (query.IncludeFolderDescendants.HasValue && query.IncludeFolderDescendants.Value)
                {
                    ContentFolderService contentFolderService = new ContentFolderService();
                    IEnumerable<ContentFolder> contentFolders = contentFolderService.GetDescendants(query.ContentFolderId.Value);

                    IEnumerable<int> descendantFolderIds = contentFolders == null ? new List<int>() : contentFolders.Select(f => f.ContentFolderId);

                    List<int> folderIds = new List<int>(descendantFolderIds);
                    folderIds.Add(query.ContentFolderId.Value);

                    whereSql.Where("spb_cms_ContentItems.ContentFolderId in (@ContentFolderIds)", new { ContentFolderIds = folderIds });
                }
                else
                {
                    whereSql.Where("spb_cms_ContentItems.ContentFolderId=@0", query.ContentFolderId.Value);
                }
            }
            else if (query.ModeratorUserId.HasValue && query.ModeratorUserId.Value > 0)
            {
                ContentFolderService contentFolderService = new ContentFolderService();
                ContentFolderModeratorService contentFolderModeratorService = new ContentFolderModeratorService();
                IEnumerable<int> moderatedFolderIds = contentFolderModeratorService.GetModeratedFolderIds(query.ModeratorUserId.Value);
                List<int> folderIds = new List<int>(moderatedFolderIds);
                if (query.IncludeFolderDescendants.HasValue && query.IncludeFolderDescendants.Value)
                {
                    foreach (var folderId in moderatedFolderIds)
                    {
                        IEnumerable<ContentFolder> contentFolders = contentFolderService.GetDescendants(folderId);
                        IEnumerable<int> descendantFolderIds = contentFolders == null ? new List<int>() : contentFolders.Select(f => f.ContentFolderId);
                        folderIds.AddRange(descendantFolderIds);
                    }
                }
                if (folderIds.Count > 0)
                    whereSql.Where("spb_cms_ContentItems.ContentFolderId in (@ContentFolderIds)", new { ContentFolderIds = folderIds });
            }

            if (query.ContentTypeId.HasValue && query.ContentTypeId.Value > 0)
                whereSql.Where("spb_cms_ContentItems.ContentTypeId=@0", query.ContentTypeId.Value);

            if (query.UserId.HasValue && query.UserId.Value > 0)
                whereSql.Where("spb_cms_ContentItems.UserId=@0", query.UserId.Value);

            if (query.IsContributed.HasValue)
            {
                whereSql.Where("spb_cms_ContentItems.IsContributed=@0", query.IsContributed.Value);
            }

            if (query.IsEssential.HasValue)
            {
                whereSql.Where("spb_cms_ContentItems.IsContributed=@0", query.IsEssential.Value);
            }

            if (query.IsSticky.HasValue)
            {
                whereSql.Where("spb_cms_ContentItems.IsGlobalSticky=@0", query.IsSticky.Value);
            }

            query.SubjectKeyword = StringUtility.StripSQLInjection(query.SubjectKeyword);
            if (!string.IsNullOrWhiteSpace(query.SubjectKeyword))
                whereSql.Where("spb_cms_ContentItems.Title like @0", "%" + query.SubjectKeyword + "%");

            query.TagNameKeyword = StringUtility.StripSQLInjection(query.TagNameKeyword);


            if (!string.IsNullOrWhiteSpace(query.TagName) || !string.IsNullOrWhiteSpace(query.TagNameKeyword))
            {
                sql.InnerJoin("tn_ItemsInTags").On("spb_cms_ContentItems.ContentItemId = tn_ItemsInTags.ItemId");
                if (!string.IsNullOrWhiteSpace(query.TagName))
                {
                    whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().ContentItem())
                            .Where("tn_ItemsInTags.TagName = @0", query.TagName);
                }
                else if (!string.IsNullOrWhiteSpace(query.TagNameKeyword))
                {
                    whereSql.Where("tn_ItemsInTags.TenantTypeId = @0", TenantTypeIds.Instance().ContentItem())
                            .Where("tn_ItemsInTags.TagName like @0", "%" + query.TagNameKeyword + "%");
                }
            }

            if (query.PubliclyAuditStatus.HasValue)
            {
                switch (query.PubliclyAuditStatus)
                {
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Success:
                        whereSql.Where("spb_cms_ContentItems.AuditStatus=@0", (int)query.PubliclyAuditStatus.Value);
                        break;
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                        whereSql.Where("spb_cms_ContentItems.AuditStatus>@0", (int)query.PubliclyAuditStatus.Value);
                        break;
                    default:
                        break;
                }
            }

            if (query.MinDate != null)
            {
                whereSql.Where("spb_cms_ContentItems.ReleaseDate >= @0", query.MinDate);
            }
            DateTime maxDate = DateTime.UtcNow;
            if (query.MaxDate != null)
            {
                maxDate = query.MaxDate.Value.AddDays(1);
            }
            whereSql.Where("spb_cms_ContentItems.ReleaseDate < @0", maxDate);

            CountService countService = new CountService(TenantTypeIds.Instance().ContentItem());
            string countTableName = countService.GetTableName_Counts();
            StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().ContentItem());
            int stageCountDays = 0;
            string stageCountType = string.Empty;
            switch (query.SortBy)
            {
                case ContentItemSortBy.ReleaseDate_Desc:
                    orderSql.OrderBy("spb_cms_ContentItems.IsGlobalSticky desc");
                    if (query.ContentFolderId.HasValue && query.ContentFolderId.Value > 0)
                        orderSql.OrderBy("spb_cms_ContentItems.IsFolderSticky desc");
                    orderSql.OrderBy("spb_cms_ContentItems.ReleaseDate desc");
                    break;
                case ContentItemSortBy.HitTimes:
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().HitTimes()))
                              .On("ContentItemId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                case ContentItemSortBy.StageHitTimes:
                    stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                    stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                    .On("ContentItemId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                case ContentItemSortBy.CommentCount:
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().CommentCount()))
                              .On("ContentItemId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;
                case ContentItemSortBy.StageCommentCount:
                    stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().CommentCount());
                    stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().CommentCount(), stageCountDays);
                    sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                    .On("ContentItemId = c.ObjectId");
                    orderSql.OrderBy("c.StatisticsCount desc");
                    break;

                case ContentItemSortBy.DisplayOrder:
                    orderSql.OrderBy("spb_cms_ContentItems.DisplayOrder, spb_cms_ContentItems.ContentItemId desc");
                    break;
                default:
                    orderSql.OrderBy("spb_cms_ContentItems.ReleaseDate desc");
                    break;
            }
            sql.Append(whereSql).Append(orderSql);
            PagingDataSet<ContentItem> pds = null;
            if (enableCaching && string.IsNullOrEmpty(query.SubjectKeyword) && string.IsNullOrEmpty(query.TagNameKeyword) && query.MinDate == null && query.MaxDate == null)
            {
                pds = GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                   () =>
                   {
                       StringBuilder cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(query));
                       cacheKey.AppendFormat("contentFolderId-{0}:includeFolderDescendants-{1}:contentTypeId-{2}:userId-{3}:isContributed-{4}:isEssential-{5}:isSticky-{6}:PubliclyAuditStatus-{7}:TagName-{8}:ModeratorUserId-{9}",
                           query.ContentFolderId.ToString(), query.IncludeFolderDescendants.ToString(), query.ContentTypeId.ToString(), query.UserId.ToString(),
                           query.IsContributed.ToString(), query.IsEssential.ToString(), query.IsSticky.ToString(), query.PubliclyAuditStatus.ToString(), query.TagName, query.ModeratorUserId);

                       return cacheKey.ToString();
                   },
                   () =>
                   {
                       return sql;
                   });
            }
            else
            {
                pds = GetPagingEntities(pageSize, pageIndex, sql);
            }

            return pds;
        }

        /// <summary>
        /// 获取前topNumber条ContentItem
        /// </summary>
        public IEnumerable<ContentItem> GetTops(int topNumber, int? contentFolderId, ContentItemSortBy sortBy)
        {
            return GetTopEntities(topNumber, CachingExpirationType.UsualObjectCollection,
                 () =>
                 {
                     StringBuilder cacheKey;
                     if (contentFolderId.HasValue)
                         cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "ContentFolderId", contentFolderId));
                     else
                         cacheKey = new StringBuilder(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.GlobalVersion));

                     cacheKey.AppendFormat("ContentFolderId-{0}:SortBy-{1}", contentFolderId.ToString(), (int)sortBy);
                     return cacheKey.ToString();
                 },
                 () =>
                 {
                     var sql = Sql.Builder.Select("spb_cms_ContentItems.*").From("spb_cms_ContentItems");
                     var whereSql = Sql.Builder;
                     var orderSql = Sql.Builder;

                     if (contentFolderId.HasValue && contentFolderId.Value > 0)
                     {
                         ContentFolderService contentFolderService = new ContentFolderService();
                         IEnumerable<ContentFolder> contentFolders = contentFolderService.GetDescendants(contentFolderId.Value);
                         IEnumerable<int> descendantFolderIds = contentFolders == null ? new List<int>() : contentFolders.Select(f => f.ContentFolderId);
                         List<int> folderIds = new List<int>(descendantFolderIds);
                         folderIds.Add(contentFolderId.Value);
                         whereSql.Where("spb_cms_ContentItems.ContentFolderId in (@ContentFolderIds)", new { ContentFolderIds = folderIds });
                     }

                     switch (PubliclyAuditStatus)
                     {
                         case PubliclyAuditStatus.Fail:
                         case PubliclyAuditStatus.Pending:
                         case PubliclyAuditStatus.Again:
                         case PubliclyAuditStatus.Success:
                             whereSql.Where("spb_cms_ContentItems.AuditStatus=@0", (int)PubliclyAuditStatus);
                             break;
                         case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                         case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                             whereSql.Where("spb_cms_ContentItems.AuditStatus>@0", (int)PubliclyAuditStatus);
                             break;
                         default:
                             break;
                     }

                     CountService countService = new CountService(TenantTypeIds.Instance().ContentItem());
                     string countTableName = countService.GetTableName_Counts();
                     StageCountTypeManager stageCountTypeManager = StageCountTypeManager.Instance(TenantTypeIds.Instance().ContentItem());
                     int stageCountDays = 0;
                     string stageCountType = string.Empty;
                     switch (sortBy)
                     {
                         case ContentItemSortBy.ReleaseDate_Desc:
                             orderSql.OrderBy("spb_cms_ContentItems.IsGlobalSticky desc");
                             if (contentFolderId.HasValue && contentFolderId.Value > 0)
                                 orderSql.OrderBy("spb_cms_ContentItems.IsFolderSticky desc");
                             orderSql.OrderBy("spb_cms_ContentItems.ReleaseDate desc");
                             break;
                         case ContentItemSortBy.HitTimes:
                             sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().HitTimes()))
                                       .On("ContentItemId = c.ObjectId");
                             orderSql.OrderBy("c.StatisticsCount desc");
                             break;
                         case ContentItemSortBy.StageHitTimes:
                             stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().HitTimes());
                             stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().HitTimes(), stageCountDays);
                             sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                             .On("ContentItemId = c.ObjectId");
                             orderSql.OrderBy("c.StatisticsCount desc");
                             break;
                         case ContentItemSortBy.CommentCount:
                             sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, CountTypes.Instance().CommentCount()))
                                       .On("ContentItemId = c.ObjectId");
                             orderSql.OrderBy("c.StatisticsCount desc");
                             break;
                         case ContentItemSortBy.StageCommentCount:
                             stageCountDays = stageCountTypeManager.GetMaxDayCount(CountTypes.Instance().CommentCount());
                             stageCountType = stageCountTypeManager.GetStageCountType(CountTypes.Instance().CommentCount(), stageCountDays);
                             sql.LeftJoin(string.Format("(select * from {0} WHERE ({0}.CountType = '{1}')) c", countTableName, stageCountType))
                             .On("ContentItemId = c.ObjectId");
                             orderSql.OrderBy("c.StatisticsCount desc");
                             break;

                         case ContentItemSortBy.DisplayOrder:
                             orderSql.OrderBy("spb_cms_ContentItems.DisplayOrder, spb_cms_ContentItems.ContentItemId desc");
                             break;
                         default:
                             orderSql.OrderBy("spb_cms_ContentItems.ReleaseDate desc");
                             break;
                     }
                     whereSql.Where("spb_cms_ContentItems.ReleaseDate < @0", DateTime.UtcNow);
                     sql.Append(whereSql).Append(orderSql);
                     return sql;
                 });
        }

        /// <summary>
        /// 获取资讯管理数据
        /// </summary>
        /// <remarks>
        /// 无需缓存
        /// </remarks>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>资讯管理数据</returns>
        public Dictionary<string, long> GetManageableData(string tenantTypeId = null)
        {
            Dictionary<string, long> manageableData = new Dictionary<string, long>();

            //查询待审核数
            Sql sql = Sql.Builder.Select("count(*)")
                                 .From("spb_cms_ContentItems")
                                 .Where("AuditStatus=@0", AuditStatus.Pending);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableData.Add(ApplicationStatisticDataKeys.Instance().PendingCount(), CreateDAO().FirstOrDefault<long>(sql));

            //查询需再审核数
            sql = Sql.Builder.Select("count(*)")
                             .From("spb_cms_ContentItems")
                             .Where("AuditStatus=@0", AuditStatus.Again);
            if (!string.IsNullOrEmpty(tenantTypeId))
            {
                sql.Where("TenantTypeId=@0", tenantTypeId);
            }
            manageableData.Add(ApplicationStatisticDataKeys.Instance().AgainCount(), CreateDAO().FirstOrDefault<long>(sql));

            return manageableData;
        }

        /// <summary>
        /// 更新置顶到期的资讯
        /// </summary>
        public void ExpireStickyContentItems()
        {
            var sql = Sql.Builder.Append("update spb_cms_ContentItems set IsGlobalSticky = 0 where GlobalStickyDate < @0", DateTime.UtcNow);
            sql.Append("update spb_cms_ContentItems set IsFolderSticky = 0 where FolderStickyDate < @0", DateTime.UtcNow);
            CreateDAO().Execute(sql);
        }

        /// <summary>
        /// 获取资讯应用统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>资讯统计数据</returns>
        public Dictionary<string, long> GetApplicationStatisticData(string tenantTypeId = null)
        {
            StringBuilder cacheKey = new StringBuilder();
            cacheKey.AppendFormat("GetApplicationStatisticData::tenantTypeId-{0}:table-ContentItem", tenantTypeId);
            Dictionary<string, long> statisticData = cacheService.Get<Dictionary<string, long>>(cacheKey.ToString());
            if (statisticData == null)
            {
                statisticData = new Dictionary<string, long>();

                //查询总数
                Sql sql = Sql.Builder.Select("count(*)")
                                     .From("spb_cms_ContentItems");
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }
                statisticData.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), CreateDAO().FirstOrDefault<long>(sql));

                //查询24小时新增数
                sql = Sql.Builder.Select("count(*)")
                                 .From("spb_cms_ContentItems")
                                 .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
                if (!string.IsNullOrEmpty(tenantTypeId))
                {
                    sql.Where("TenantTypeId=@0", tenantTypeId);
                }
                statisticData.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), CreateDAO().FirstOrDefault<long>(sql));

                cacheService.Add(cacheKey.ToString(), statisticData, CachingExpirationType.UsualSingleObject);
            }

            return statisticData;
        }

        /// <summary>
        /// 获取解析正文缓存Key
        /// </summary>
        /// <param name="contentItemId"></param>
        /// <returns></returns>
        private string GetCacheKeyOfResolvedBody(long contentItemId)
        {
            return "ContentItemResolvedBody" + contentItemId;
        }

        /// <summary>
        /// 资讯应用可对外显示的审核状态
        /// </summary>
        private PubliclyAuditStatus? publiclyAuditStatus;
        /// <summary>
        /// 资讯应用可对外显示的审核状态
        /// </summary>
        protected PubliclyAuditStatus PubliclyAuditStatus
        {
            get
            {
                if (publiclyAuditStatus == null)
                {
                    AuditService auditService = new AuditService();
                    publiclyAuditStatus = auditService.GetPubliclyAuditStatus(ApplicationIds.Instance().CMS());
                }
                return publiclyAuditStatus.Value;
            }
            set
            {
                this.publiclyAuditStatus = value;
            }
        }
    }
}
