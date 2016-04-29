//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spacebuilder.CMS;
using Tunynet.Caching;
using Tunynet.Repositories;

namespace Spacebuilder.CMS
{
    public class ContentFolderRepository : Repository<ContentFolder>
    {
        /// <summary>
        /// 获取所有栏目（按DisplayOrder排序）
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContentFolder> GetAll()
        {
            string cacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.GlobalVersion) + "All";
            List<int> folderIds = cacheService.Get<List<int>>(cacheKey);
            if (folderIds == null)
            {
                var sql = PetaPoco.Sql.Builder
                  .Select("ContentFolderId")
                  .From("spb_cms_ContentFolders")
                  .OrderBy("DisplayOrder");

                folderIds = CreateDAO().FetchFirstColumn(sql).Cast<int>().ToList();
                cacheService.Add(cacheKey, folderIds, CachingExpirationType.UsualObjectCollection);
            }
            IEnumerable<ContentFolder> contentTypeDefinitions = PopulateEntitiesByEntityIds(folderIds);
            return contentTypeDefinitions;
        }
    }
}