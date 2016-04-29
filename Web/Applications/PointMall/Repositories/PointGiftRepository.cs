//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Repositories;
using System.Linq;
using Tunynet.Utilities;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城应用的商品仓储实现
    /// </summary>
    public class PointGiftRepository : Repository<PointGift>, IPointGiftRepository
    {
        /// <summary>
        /// 获取商品列表（用于频道）
        /// </summary>
        /// <param name="nameKeyword">商品名称关键字</param>
        /// <param name="categoryId">类别ID</param>
        /// <param name="sortBy">排序规则</param>
        /// <param name="maxPrice">最大单价</param>
        /// <param name="minPrice">最小单价</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        public PagingDataSet<PointGift> GetPointGifts(string nameKeyword, long? categoryId, SortBy_PointGift sortBy, int maxPrice, int minPrice, int pageSize, int pageIndex)
        {
            Sql sql = GetSql_GetPointGifts(nameKeyword, categoryId, true, maxPrice, minPrice, sortBy, 0);
            return GetPagingEntities(pageSize, pageIndex, CachingExpirationType.UsualObjectCollection, () =>
            {
                return string.Format("GetPointGifts::CategoryId-{0};SortBy_PointGift-{1},NameKeyword-{2},MaxPrice-{3},MinPrice-{4}", categoryId, sortBy, nameKeyword, maxPrice, minPrice);
            }, () =>
            {
                return sql;
            });
        }

        /// <summary>
        /// 管理员获取商品数据
        /// </summary>
        /// <param name="nameKeyword">商品名称关键字</param>
        /// <param name="categoryId">类别</param>
        /// <param name="isEnabled">是否上架</param>
        /// <param name="maxPrice">最大单价</param>
        /// <param name="minPrice">最小单价</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<PointGift> GetGiftsForAdmin(string nameKeyword, long? categoryId, bool? isEnabled, int maxPrice, int minPrice, int pageSize, int pageIndex)
        {
            Sql sql = GetSql_GetPointGifts(nameKeyword, categoryId, isEnabled, maxPrice, minPrice, SortBy_PointGift.DateCreated_Desc, 0);
            return GetPagingEntities(pageSize, pageIndex, sql);
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>       
        /// <param name="sortby">排序规则</param>
        /// <param name="topNumber">条数</param>
        /// <returns></returns>
        public IEnumerable<PointGift> GetPointGifts(SortBy_PointGift sortby, int topNumber)
        {
            Sql sql = GetSql_GetPointGifts(string.Empty, null, true, 0, 0, sortby, topNumber);
            return GetTopEntities(topNumber, CachingExpirationType.UsualObjectCollection, () =>
            {
                return string.Format("GetPointGifts::SortBy_PointGift-{0},TopNumber-{1}", sortby, topNumber);
            }, () =>
            {
                return sql;
            });
        }

        #region 获取SQL
        /// <summary>
        /// 获取Sql语句
        /// </summary>
        /// <param name="nameKeyword">商品名称关键字</param>
        /// <param name="categoryId">类别ID</param>
        /// <param name="isEnabled">是否上架</param>
        /// <param name="maxPrice">最大单价</param>
        /// <param name="minPrice">最小单价</param>
        /// <param name="sortBy">排序依据</param>
        /// <param name="topNumber">取出所需条数</param>
        /// <returns></returns>
        private Sql GetSql_GetPointGifts(string nameKeyword, long? categoryId, bool? isEnabled, int maxPrice, int minPrice, SortBy_PointGift sortBy, int topNumber)
        {
            Sql sql = Sql.Builder;
            Sql sql_Where = Sql.Builder;
            Sql sql_Orderby = Sql.Builder;

            sql.Select("spb_PointGifts.*")
               .From("spb_PointGifts");

            if (!string.IsNullOrEmpty(nameKeyword))
                sql_Where.Where("spb_PointGifts.Name like @0", "%" + StringUtility.StripSQLInjection(nameKeyword) + "%");

            if (categoryId.HasValue)
            {
                if (categoryId != 0)
                {
                    IEnumerable<Category> categories = new CategoryService().GetDescendants(categoryId.Value);

                    List<long> categoryIds = new List<long> { categoryId.Value };
                    if (categories != null && categories.Count() > 0)
                        categoryIds.AddRange(categories.Select(n => n.CategoryId));

                    sql.LeftJoin(string.Format("(select tn_ItemsInCategories.*,tn_Categories.CategoryName from tn_ItemsInCategories left join tn_Categories on tn_ItemsInCategories.CategoryId=tn_Categories.CategoryId where tn_ItemsInCategories.CategoryId in({0})) tn_ItemsInCategories", string.Join(",", categoryIds)))
                        .On("spb_PointGifts.GiftId=tn_ItemsInCategories.ItemId");
                    sql_Where.Where("tn_ItemsInCategories.CategoryId in (@categoryIds)", new { categoryIds = categoryIds });
                }
            }

            if (isEnabled.HasValue)
                sql_Where.Where("spb_PointGifts.IsEnabled = @0", isEnabled);

            if (maxPrice != 0 && minPrice != 0)
                sql_Where.Where("spb_PointGifts.Price>= @0 and spb_PointGifts.Price<=@1", minPrice, maxPrice);

            if (minPrice != 0 && maxPrice == 0)
                sql_Where.Where("spb_PointGifts.Price>= @0", minPrice);

            if (minPrice == 0 && maxPrice != 0)
                sql_Where.Where("spb_PointGifts.Price>= 0 and spb_PointGifts.Price<=@0", maxPrice);

            CountService countService = new CountService(TenantTypeIds.Instance().PointGift());
            string countTableName = countService.GetTableName_Counts();

            switch (sortBy)
            {
                case SortBy_PointGift.DateCreated_Desc:
                    sql_Orderby.OrderBy("spb_PointGifts.LastModified desc");
                    break;
                case SortBy_PointGift.HitTimes_Desc:
                    sql.LeftJoin(countTableName).On("spb_PointGifts.GiftId = " + countTableName + ".ObjectId");
                    sql_Where.Where(countTableName + ".CountType = @0 or " + countTableName + ".CountType is null", CountTypes.Instance().HitTimes());
                    sql_Orderby.OrderBy(countTableName + ".StatisticsCount desc");
                    break;
                case SortBy_PointGift.Price_Asc:
                    sql_Orderby.OrderBy("spb_PointGifts.Price");
                    break;
                case SortBy_PointGift.Price_Desc:
                    sql_Orderby.OrderBy("spb_PointGifts.Price desc");
                    break;
                case SortBy_PointGift.Sales_Desc:
                    sql_Orderby.OrderBy("spb_PointGifts.ExchangedCount desc");
                    break;
            }

            sql.Append(sql_Where)
               .Append(sql_Orderby);

            return sql;
        }
        #endregion

        /// <summary>
        /// 获取商品统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns>返回应用统计数据</returns>
        public Dictionary<string, long> GetPointGiftApplicationStatisticData(string tenantTypeId)
        {
            string cacheKey = string.Format("GetPointGiftApplicationStatisticData-{0}", tenantTypeId);
            Dictionary<string, long> statisticData = cacheService.Get<Dictionary<string, long>>(cacheKey);

            if (statisticData != null)
                return statisticData;

            statisticData = new Dictionary<string, long>();

            Sql sql = Sql.Builder
                .Select("count(*)")
                .From("spb_PointGifts");

            var dao = CreateDAO();
            dao.OpenSharedConnection();
            statisticData.Add(ApplicationStatisticDataKeys.Instance().TotalCount(), dao.FirstOrDefault<long>(sql));
            sql.Where("DateCreated > @0", DateTime.UtcNow.AddDays(-1));
            statisticData.Add(ApplicationStatisticDataKeys.Instance().Last24HCount(), dao.FirstOrDefault<long>(sql));
            dao.CloseSharedConnection();

            cacheService.Set(cacheKey, statisticData, CachingExpirationType.SingleObject);

            return statisticData;
        }

        /// <summary>
        /// 获取商品兑换次数
        /// </summary>
        /// <param name="giftId">商品ID</param>
        /// <returns></returns>        
        public int GetGiftExchangeNumber(long giftId)
        {
            string cacheKey = string.Format("GetGiftExchangeNumber-{0}", giftId);
            string giftExchangeNumber = cacheService.Get<string>(cacheKey);

            Sql sql = Sql.Builder
                .Select("count(distinct PayerUserId) as CountPayer")
                .From("spb_PointGiftExchangeRecords")
                .Where("GiftId=@0", giftId);
            var dao = CreateDAO();
            dao.OpenSharedConnection();
            giftExchangeNumber = dao.FirstOrDefault<string>(sql);

            return Convert.ToInt32(giftExchangeNumber);
        }
    }
}