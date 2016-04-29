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

namespace Spacebuilder.Bar
{
    /// <summary>
    ///帖子评分Repository
    /// </summary>
    public class BarRatingRepository : Repository<BarRating>, IBarRatingRepository
    {
        private int pageSize = 10;

        /// <summary>
        /// 评分
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override object Insert(Spacebuilder.Bar.BarRating entity)
        {
            //1.先检查用户今日大于零的评分总和是否超出限制；
            //2.对主题帖作者加威望




            ISettingsManager<BarSettings> barSettingsManager = DIContainer.Resolve<ISettingsManager<BarSettings>>();
            BarSettings barSettings = barSettingsManager.Get();
            bool result = false;
            var sql = Sql.Builder;
            sql.Select("sum(ReputationPoints)")
            .From("spb_BarRatings")
            .Where("UserId=@0", entity.UserId)
            .Where("DateCreated>@0", new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day))
            .Where("ReputationPoints>0");
            long? sumReputationPoints = CreateDAO().FirstOrDefault<long?>(sql);
            if (!sumReputationPoints.HasValue || sumReputationPoints.Value <= barSettings.UserReputationPointsPerDay)
            {
                base.Insert(entity);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 获取主题帖的评分记录分页集合
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>评分列表</returns>
        public PagingDataSet<BarRating> Gets(long threadId, int pageIndex)
        {
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.ObjectCollection,
                              () =>
                              {
                                  StringBuilder cacheKey = new StringBuilder();
                                  cacheKey.Append(RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "ThreadId", threadId));
                                  cacheKey.Append("BarRatingsOfThread");
                                  return cacheKey.ToString();
                              },
                              () =>
                              {
                                  var sql = Sql.Builder;
                                  sql.Select("*")
                                  .From("spb_BarRatings")
                                  .Where("ThreadId = @0", threadId);
                                  sql.OrderBy("RatingId desc");
                                  return sql;
                              });
        }

        /// <summary>
        /// 根据用户获取所有评过分的帖子列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="beforeDays">最近多少天内</param>
        /// <returns></returns>
        public IEnumerable<long> GetThreadIdsByUser(long userId, int beforeDays = 30)
        {
            string cacheKey = RealTimeCacheHelper.GetListCacheKeyPrefix(CacheVersionType.AreaVersion, "UserId", userId) + "BarRatingThreadIdsOfUser";
            List<long> threadIds = cacheService.Get<List<long>>(cacheKey);
            if (threadIds == null)
            {
                var sql = Sql.Builder;
                sql.Select("ThreadId")
                .From("spb_BarRatings")
                .Where("UserId = @0", userId)
                .Where("DateCreated > @0", DateTime.UtcNow.AddDays(-beforeDays));
                sql.OrderBy("RatingId desc");


                IEnumerable<object> threadIds_object = CreateDAO().FetchFirstColumn(sql);
                threadIds = threadIds_object.Cast<long>().ToList();
                cacheService.Add(cacheKey, threadIds, CachingExpirationType.UsualObjectCollection);
            }
            return threadIds;
        }

        /// <summary>
        /// 获取用户今天评过的分数
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns>今天评分的总和</returns>
        public int GetUserTodayRatingSum(long userId)
        {
            string cacheKey = GetCacheKey_UserTodayRatingSum(userId);
            var userTodayRatingSumObject = cacheService.Get(cacheKey);
            if (userTodayRatingSumObject != null)
                return (int)userTodayRatingSumObject;

            Database dao = CreateDAO();
            dao.OpenSharedConnection();

            var sql_Select = PetaPoco.Sql.Builder;
            sql_Select.Select("sum(ReputationPoints)")
                .From("spb_BarRatings")
                .Where("UserId=@0", userId)
                .Where("ReputationPoints>0")
                .Where("DateCreated>@0", new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day));
            int? UserTodayRatingSum = dao.FirstOrDefault<int?>(sql_Select);
            sql_Select = PetaPoco.Sql.Builder;
            sql_Select.Select("sum(ReputationPoints)")
                .From("spb_BarRatings")
                .Where("UserId=@0", userId)
                .Where("ReputationPoints<0")
                .Where("DateCreated>@0", new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day));
            int? UserTodayPoorRatingSum = dao.FirstOrDefault<int?>(sql_Select);

            dao.CloseSharedConnection();

            int sum = UserTodayRatingSum ?? 0 - UserTodayPoorRatingSum ?? 0;

            cacheService.Set(cacheKey, sum, CachingExpirationType.SingleObject);
            return sum;
        }

        /// <summary>
        /// 获取用户今天评分的cachekey
        /// </summary>
        /// <returns>用户今天评分的cachekey</returns>
        private string GetCacheKey_UserTodayRatingSum(long userId)
        {
            int userIdAreaVersion = RealTimeCacheHelper.GetAreaVersion("UserId", userId);
            return string.Format("UserTodayRatingSum::UserId-{0};UserIdAreaVersion-{1};", userId, userIdAreaVersion);
        }
    }
}