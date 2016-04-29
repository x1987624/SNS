//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Repositories;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城应用的商品仓储接口
    /// </summary>
    public interface IPointGiftRepository : IRepository<PointGift>
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
        /// <returns></returns>
        PagingDataSet<PointGift> GetPointGifts(string nameKeyword, long? categoryId, SortBy_PointGift sortBy, int maxPrice, int minPrice, int pageSize, int pageIndex);

        /// <summary>
        /// 管理员获取商品数据
        /// </summary>
        /// <param name="nameKeyword">商品名称关键字</param>
        /// <param name="isEnabled">是否上架</param>
        /// <param name="maxPrice">最大单价</param>
        /// <param name="minPrice">最小单价</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        PagingDataSet<PointGift> GetGiftsForAdmin(string nameKeyword, long? categoryId,bool? isEnabled, int maxPrice, int minPrice, int pageSize, int pageIndex);

        /// <summary>
        /// 获取商品列表
        /// </summary>       
        /// <param name="sortby">排序规则</param>
        /// <param name="topNumber">条数</param>
        IEnumerable<PointGift> GetPointGifts(SortBy_PointGift sortby, int topNumber);

        Dictionary<string, long> GetPointGiftApplicationStatisticData(string tenantTypeId);

        /// <summary>
        /// 获取商品兑换次数
        /// </summary>
        /// <param name="giftId">商品ID</param>
        /// <returns></returns>
        int GetGiftExchangeNumber(long giftId);
    }
}