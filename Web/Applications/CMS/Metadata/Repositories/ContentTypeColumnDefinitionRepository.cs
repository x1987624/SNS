//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Repositories;

namespace Spacebuilder.CMS.Metadata
{
    public class ContentTypeColumnDefinitionRepository : Repository<ContentTypeColumnDefinition>
    {
        /// <summary>
        /// 获取ContentType的所有列元数据
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <returns></returns>
        public IEnumerable<ContentTypeColumnDefinition> GetColumnsOfContentType(int contentTypeId)
        {
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();

            string cacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "ContentTypeId", contentTypeId);
            List<int> columnIds = cacheService.Get<List<int>>(cacheKey);
            if (columnIds == null)
            {
                var sql = PetaPoco.Sql.Builder
                  .Select("ColumnId")
                  .From("spb_cms_ContentTypeColumnDefinitions")
                  .Where("ContentTypeId=@0", contentTypeId);

                columnIds = CreateDAO().FetchFirstColumn(sql).Cast<int>().ToList();
                cacheService.Add(cacheKey, columnIds, CachingExpirationType.UsualObjectCollection);
            }
            IEnumerable<ContentTypeColumnDefinition> contentTypeDefinitions = PopulateEntitiesByEntityIds<int>(columnIds);
            return contentTypeDefinitions;
        }

    }
}
