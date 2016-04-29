//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Web.Routing;
using Spacebuilder.PointMall;
using Tunynet.Common;

using Tunynet.Utilities;
using System;
using System.Collections.Generic;

namespace Spacebuilder.Common
{
    /// <summary>
    /// 商城链接管理扩展
    /// </summary>
    public static class SiteUrlsExtension
    {
        private static readonly string PointMallAreaName = PointMallConfig.Instance().ApplicationKey;

        #region 频道

        /// <summary>
        /// 频道首页
        /// </summary>
        /// <returns></returns>
        public static string Home(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("Home", "ChannelPointMall", PointMallAreaName);
        }


        /// <summary>
        /// 商品明细
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="giftId">商品ID</param>
        /// <returns></returns>
        public static string GiftDetail(this SiteUrls siteUrls, long giftId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("giftId", giftId);
            return CachedUrlHelper.Action("GiftDetail", "ChannelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 商品评价
        /// </summary>
        public static string _GiftComments(this SiteUrls siteUrls, long giftId=0, ApproveStatus? approveStatus = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (giftId > 0)
            {
                dic.Add("giftId", giftId);
            }
            if (approveStatus.HasValue)
            {
                dic.Add("approveStatus",approveStatus);
            }
            return CachedUrlHelper.Action("_GiftComments", "ChannelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 创建兑换记录
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="giftId">商品ID</param>
        /// <returns></returns>
        public static string _CreateRecord(this SiteUrls siteUrls, long giftId = 0)
        {
            if (UserContext.CurrentUser == null)        //跳转到登录页
                return siteUrls.Login(true, SiteUrls.LoginModal._LoginInModal, null);
            RouteValueDictionary dic = new RouteValueDictionary();
            if (giftId > 0)
            {
                dic.Add("giftId", giftId);
            }
            return CachedUrlHelper.Action("_CreateRecord", "ChannelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 商品排行
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="nameKeyword">商品名称</param>
        /// <param name="categoryId">商品类别Id</param>
        /// <param name="sortBy">排序</param>
        /// <param name="maxPrice">某个区间的最大价格</param>
        /// <param name="minPrice">某个区间的最低价格</param>
        /// <returns></returns>
        public static string Rank(this SiteUrls siteUrls, string nameKeyword = null, long? categoryId = null, SortBy_PointGift sortBy = SortBy_PointGift.Sales_Desc, int maxPrice = 0, int minPrice = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(nameKeyword))
                dic.Add("nameKeyword", nameKeyword);
            if (categoryId.HasValue)
                dic.Add("categoryId", categoryId);
            dic.Add("sortBy", sortBy);
            dic.Add("maxPrice", maxPrice);
            dic.Add("minPrice", minPrice);
            return CachedUrlHelper.Action("Rank", "ChannelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 收藏商品操作
        /// </summary>
        public static string _Favorite(this SiteUrls siteUrls, long giftId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("giftId", giftId);
            return CachedUrlHelper.Action("_Favorite", "ChannelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 取消收藏操作
        /// </summary>
        public static string _FavoriteCancel(this SiteUrls siteUrls, long giftId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("giftId", giftId);
            return CachedUrlHelper.Action("_FavoriteCancel", "ChannelPointMall", PointMallAreaName, dic);
        }

        #endregion

        #region 空间

        /// <summary>
        /// 用户空间首页(兑换申请)
        /// </summary>
        /// <returns></returns>
        public static string PointMallHome(this SiteUrls siteUrls, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("Home", "UserSpacePointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 用户空间首页(兑换申请)
        /// </summary>
        /// <param name="date">查看时间</param>
        /// <param name="approveStatus">申请状态</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public static string PointMallHome(this SiteUrls siteUrls, string spaceKey, string date, ApproveStatus? approveStatus, int pageIndex = 1)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            dic.Add("date", date);
            dic.Add("approveStatus", approveStatus);
            dic.Add("pageIndex", pageIndex);
            return CachedUrlHelper.Action("Home", "UserSpacePointMall", PointMallAreaName, dic);
        }


        /// <summary>
        /// 取消收藏
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="objectId">商品Id</param>
        /// <returns></returns>
        public static string _CancelFavorite(this SiteUrls siteUrls, string spaceKey, long objectId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            if (objectId > 0)
            {
                dic.Add("objectId", objectId);
            }
            return CachedUrlHelper.Action("_CancelFavorite", "UserSpacePointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 取消兑换
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="recordId">兑换记录Id</param>
        /// <returns></returns>
        public static string _CancelExchange(this SiteUrls siteUrls, string spaceKey, long recordId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            if (recordId > 0)
            {
                dic.Add("recordId", recordId);
            }
            return CachedUrlHelper.Action("_CancelExchange", "UserSpacePointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 评价
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="spaceKey"></param>
        /// <param name="recordId">兑换记录Id</param>
        /// <returns></returns>
        public static string _Appraise(this SiteUrls siteUrls, string spaceKey, long recordId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            dic.Add("recordId", recordId);
            return CachedUrlHelper.Action("_Appraise", "UserSpacePointMall", PointMallAreaName, dic);
        }
        #endregion

        #region 后台

        /// <summary>
        /// 添加/修改商品
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="giftId">商品Id</param>
        /// <returns></returns>
        public static string EditGift(this SiteUrls siteUrls, long giftId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (giftId > 0)
            {
                dic.Add("giftId", giftId);
            }
            return CachedUrlHelper.Action("EditGift", "ControlPanelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 管理商品
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="name">名称关键字</param>
        /// <param name="categoryId">分类Id</param>
        /// <param name="maxPrice">最小价格</param>
        /// <param name="minPrice">最大价格</param>
        /// <param name="isEnabled">是否上架</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        public static string ManagePointGifts(this SiteUrls siteUrls, string name = null, long? categoryId = null, int? maxPrice = null, int? minPrice = null, bool? isEnabled = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (name != null)
            {
                dic.Add("name", name);
            }
            if (maxPrice != null)
            {
                dic.Add("maxPrice", maxPrice);
            }
            if (minPrice != null)
            {
                dic.Add("minPrice", minPrice);
            }
            if (isEnabled.HasValue)
            {
                dic.Add("isEnabled", isEnabled);
            }
            if (categoryId != null)
            {
                dic.Add("categoryId", categoryId);
            }
            return CachedUrlHelper.Action("ManagePointGifts", "ControlPanelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 更改商品的状态
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="giftId">商品Id，用于单个更改</param>
        /// <param name="isEnabled">是否上架</param>
        /// <returns></returns>
        public static string _ChangeGiftStatus(this SiteUrls siteUrls, bool isEnabled, long? giftId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (giftId != null)
            {
                dic.Add("giftIds", giftId);
            }
            dic.Add("isEnabled", isEnabled);
            return CachedUrlHelper.Action("_ChangeGiftStatus", "ControlPanelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 管理申请
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="beginDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="userId">用于Id</param>
        /// <param name="approveStatus">申请状态</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        public static string ManageRecords(this SiteUrls siteUrls, DateTime? beginDate = null, DateTime? endDate = null, long? userId = null, ApproveStatus? approveStatus = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (beginDate != null)
            {
                dic.Add("beginDate", beginDate);
            }
            if (endDate != null)
            {
                dic.Add("endDate", endDate);
            }
            if (userId != null)
            {
                dic.Add("userId", userId);
            }
            if (approveStatus != null)
            {
                dic.Add("approveStatus", approveStatus);
            }
            return CachedUrlHelper.Action("ManageRecords", "ControlPanelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 设置商品类别
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _SetGiftCategories(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_SetGiftCategories", "ControlPanelPointMall", PointMallAreaName);
        }

        /// <summary>
        /// 管理商品
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="name">商品名</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="isEnabled">是否上架</param>
        /// <param name="maxPrice">最大价格</param>
        /// <param name="minPrice">最小价格</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        public static string ManagePointGifts(this SiteUrls siteUrls, string name, long? categoryId, bool? isEnabled, int? maxPrice, int? minPrice, int pageIndex = 1, int pageSize = 20)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("name", name);
            if (categoryId != null)
            {
                dic.Add("categoryId", categoryId);
            }
            if (isEnabled != null)
            {
                dic.Add("isEnabled", isEnabled);
            }
            if (maxPrice != null)
            {
                dic.Add("maxPrice", maxPrice);
            }
            if (minPrice != null)
            {
                dic.Add("minPrice", minPrice);
            }
            dic.Add("pageIndex", pageIndex);
            dic.Add("pageSize", pageSize);
            return CachedUrlHelper.Action("ManagePointGifts", "ControlPanelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 设置跟踪信息
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="recordId">兑换记录Id</param>
        /// <returns></returns>
        public static string _SetTrackInfo(this SiteUrls siteUrls, long? recordId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (recordId.HasValue)
            {
                dic.Add("recordId", recordId);
            }
            return CachedUrlHelper.Action("_SetTrackInfo", "ControlPanelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 设置邮寄地址
        /// </summary>
        /// <returns></returns>
        public static string _SetMailAddress(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_SetMailAddress", "ChannelPointMall", PointMallAreaName);
        }

        /// <summary>
        /// 删除兑换记录
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _DeleteRecords(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_DeleteRecords", "ControlPanelPointMall", PointMallAreaName);
        }

        /// <summary>
        /// 修改兑换申请状态
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApprove">是否接受申请</param>
        /// <returns></returns>
        public static string _ChangeRecordStatus(this SiteUrls siteUrls, bool isApprove)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("isApprove", isApprove);
            return CachedUrlHelper.Action("_ChangeRecordStatus", "ControlPanelPointMall", PointMallAreaName, dic);
        }

        /// <summary>
        /// 展示联系方式
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="recordId">兑换记录Id</param>
        /// <returns></returns>
        public static string _ShowMailAddressInfo(this SiteUrls siteUrls, long recordId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("recordId", recordId);
            return CachedUrlHelper.Action("_ShowMailAddressInfo", "ControlPanelPointMall", PointMallAreaName, dic);
        }

        #endregion
    }
}
