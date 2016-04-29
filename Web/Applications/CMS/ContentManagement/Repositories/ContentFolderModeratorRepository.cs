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
using Tunynet.Common;
using Tunynet;
using PetaPoco;

namespace Spacebuilder.CMS
{
    public class ContentFolderModeratorRepository : Repository<ContentFolderModerator>
    {
        /// <summary>
        /// 设置用户管理哪些栏目
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="contentFolderIds"></param>
        public void SetModeratorByUser(long userId, IEnumerable<int> contentFolderIds)
        {
            var sql_delete = PetaPoco.Sql.Builder.Append("DELETE FROM spb_cms_ContentFolderModerators where UserId=@0", userId);

            List<PetaPoco.Sql> sql_inserts = new List<PetaPoco.Sql>();
            foreach (var contentFolderId in contentFolderIds)
            {
                var sql_insert = PetaPoco.Sql.Builder.Append("INSERT INTO spb_cms_ContentFolderModerators (ContentFolderId,UserId) VALUES (@0,@1)", contentFolderId, userId);
                sql_inserts.Add(sql_insert);
            }

            IEnumerable<int> oldFolderIds = GetModeratedFolderIds(userId);
            IEnumerable<int> unionFolderIds = oldFolderIds.Union(contentFolderIds);

            Database database = CreateDAO();

            //加入事务
            using (var scope = database.GetTransaction())
            {
                database.Execute(sql_delete);
                database.Execute(sql_inserts);
                scope.Complete();
            }

            //递增缓存分区版本号(UserId)
            RealTimeCacheHelper.IncreaseAreaVersion("UserId", userId);
            foreach (var contentFolderId in unionFolderIds)
            {
                //去除contentFolderIds及oldFolderIds交集
                if (contentFolderIds.Contains(contentFolderId) && oldFolderIds.Contains(contentFolderId))
                    continue;

                //递增缓存分区版本号(ContentFolderId)
                RealTimeCacheHelper.IncreaseAreaVersion("ContentFolderId", contentFolderId);
            }
        }

        /// <summary>
        /// 设置栏目有哪些管理员
        /// </summary>
        /// <param name="contentFolderId"></param>
        /// <param name="userIds"></param>
        public void SetModeratorByFolder(int contentFolderId, IEnumerable<long> userIds)
        {
            var sql_delete = PetaPoco.Sql.Builder.Append("delete from spb_cms_ContentFolderModerators where ContentFolderId=@0", contentFolderId);

            List<PetaPoco.Sql> sql_inserts = new List<PetaPoco.Sql>();
            foreach (var userId in userIds)
            {
                var sql_insert = PetaPoco.Sql.Builder.Append("INSERT INTO spb_cms_ContentFolderModerators (ContentFolderId,UserId) VALUES (@0,@1)", contentFolderId, userId);
                sql_inserts.Add(sql_insert);
            }

            IEnumerable<long> oldUserIds = GetModerators(contentFolderId).Select(n => n.UserId);
            IEnumerable<long> unionUserIds = oldUserIds.Union(userIds);

            Database database = CreateDAO();
            using (var scope = database.GetTransaction())
            {
                database.Execute(sql_delete);
                database.Execute(sql_inserts);
                scope.Complete();
            }

            //递增缓存分区版本号(ContentFolderId)
            RealTimeCacheHelper.IncreaseAreaVersion("ContentFolderId", contentFolderId);
            foreach (var userId in unionUserIds)
            {
                //去除userIds及oldUserIds交集
                if (userIds.Contains(userId) && oldUserIds.Contains(userId))
                    continue;

                //递增缓存分区版本号(UserId)
                RealTimeCacheHelper.IncreaseAreaVersion("UserId", userId);
            }
        }


        /// <summary>
        /// 获取栏目的管理员
        /// </summary>
        /// <param name="contentFolderId"></param>
        /// <returns></returns>
        public IEnumerable<IUser> GetModerators(int contentFolderId)
        {

            string cacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "ContentFolderId", contentFolderId);
            List<long> userIds = cacheService.Get<List<long>>(cacheKey);
            if (userIds == null)
            {
                var sql = PetaPoco.Sql.Builder
                    .Select("UserId")
                    .From("spb_cms_ContentFolderModerators")
                    .Where("ContentFolderId=@0", contentFolderId);

                userIds = CreateDAO().FetchFirstColumn(sql).Cast<long>().ToList();
                cacheService.Add(cacheKey, userIds, CachingExpirationType.UsualObjectCollection);
            }

            return DIContainer.Resolve<IUserService>().GetUsers(userIds);
        }


        /// <summary>
        /// 用户管理的栏目Id集合
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<int> GetModeratedFolderIds(long userId)
        {
            var sql = PetaPoco.Sql.Builder
                .Select("ContentFolderId")
                .From("spb_cms_ContentFolderModerators")
                .Where("UserId=@0", userId);

            ICacheService cacheService = DIContainer.Resolve<ICacheService>();

            string cacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId);
            List<int> contentFolderIds = cacheService.Get<List<int>>(cacheKey);
            if (contentFolderIds == null)
            {
                IEnumerable<object> ids = CreateDAO().FetchFirstColumn(sql);
                contentFolderIds = ids.Cast<int>().ToList();
                cacheService.Add(cacheKey, contentFolderIds, CachingExpirationType.UsualObjectCollection);
            }

            return contentFolderIds;
        }

    }
}
