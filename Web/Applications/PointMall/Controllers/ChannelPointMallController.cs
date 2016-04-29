//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;
using Tunynet.Utilities;
using Spacebuilder.Search;
using Tunynet.Search;
using System.Web;
using System;
using DevTrends.MvcDonutCaching;

namespace Spacebuilder.PointMall.Controllers
{
    /// <summary>
    /// 积分商城频道控制器
    /// </summary>
    [TitleFilter(TitlePart = "积分商城", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.Channel, IsApplication = true)]
    [AnonymousBrowseCheck]
    public class ChannelPointMallController : Controller
    {
        #region 私有
        public IPageResourceManager pageResourceManager { get; set; }
        public PointMallService pointMallService { get; set; }
        public RecommendService recommendService { get; set; }
        public MailAddressService mailAddressService { get; set; }
        public CategoryService categoryService { get; set; }
        private PointMallSettings pointMallSettings = DIContainer.Resolve<ISettingsManager<PointMallSettings>>().Get();
        private FavoriteService favoriteService = new FavoriteService(TenantTypeIds.Instance().PointGift());
        #endregion

        #region 首页
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Home()
        {
            return View();
        }

        /// <summary>
        /// 子菜单
        /// </summary>
        /// <returns></returns>        
        public ActionResult _Submenu()
        {
            return View();
        }

        #endregion

        #region 商品排行

        /// <summary>
        /// 商品排行
        /// </summary>
        /// <param name="nameKeyword">关键字</param>
        /// <param name="categoryId">类别id</param>
        /// <param name="sortBy">排序</param>
        /// <param name="maxPrice">某个区间的最大价格</param>
        /// <param name="minPrice">某个区间的最小价格</param>
        /// <param name="pageSize">每页多少条</param>
        /// <param name="pageIndex">第几页</param>
        /// <returns></returns>
        public ActionResult Rank(string nameKeyword = null, long? categoryId = null, SortBy_PointGift sortBy = SortBy_PointGift.Sales_Desc, int maxPrice = 0, int minPrice = 0, int pageSize = 20, int pageIndex = 1)
        {
            //获取用户配置的价格区间
            Dictionary<int, int> price = PriceSetting.Get();
            ViewData["price"] = price;

            pageResourceManager.InsertTitlePart("商品排行");

            //获取类别
            IEnumerable<Category> childCategories = null;
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var category = categoryService.Get(categoryId.Value);
                if (category != null)
                {

                    if (category.ChildCount > 0)
                    {
                        childCategories = category.Children;
                    }
                    else//若是叶子节点，则取同辈分类
                    {
                        if (category.Parent != null)
                            childCategories = category.Parent.Children;
                    }
                    List<Category> allParentCategories = new List<Category>();

                    //递归获取所有父级类别，若不是叶子节点，则包含其自身
                    RecursiveGetAllParentCategories(category.ChildCount > 0 ? category : category.Parent, ref allParentCategories);
                    ViewData["allParentCategories"] = allParentCategories;
                    ViewData["currentCategory"] = category;

                }
            }

            if (childCategories == null)
            {
                childCategories = categoryService.GetRootCategories(TenantTypeIds.Instance().PointGift());
            }

            ViewData["childCategories"] = childCategories;

            PagingDataSet<PointGift> gifts = pointMallService.GetGifts(nameKeyword, categoryId, sortBy, maxPrice, minPrice, pageSize, pageIndex);
            return View(gifts);
        }

        /// <summary>
        /// 迭代获取类别
        /// </summary>
        /// <param name="category"></param>
        /// <param name="allParentCategories"></param>
        private void RecursiveGetAllParentCategories(Category category, ref List<Category> allParentCategories)
        {
            if (category == null)
                return;
            allParentCategories.Insert(0, category);
            Category parent = category.Parent;
            if (parent != null)
                RecursiveGetAllParentCategories(parent, ref allParentCategories);
        }


        #endregion

        #region 商品详细

        /// <summary>
        /// 商品描述
        /// </summary>
        /// <returns></returns>
        public ActionResult GiftDetail(long giftId)
        {
            ViewData["giftId"] = giftId;
            //获取商品
            PointGift gift = pointMallService.GetGift(giftId);

            //更新计数
            CountService countService = new CountService(TenantTypeIds.Instance().PointGift());
            countService.ChangeCount(CountTypes.Instance().HitTimes(), giftId, gift.UserId, 1);


            pageResourceManager.InsertTitlePart(gift.Name);

            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null)
            {
                //设置最近浏览
                HttpCookie cookie = Request.Cookies["LastViewedGifts" + currentUser.UserId];
                if (cookie != null)
                {
                    string[] cookieGiftIds = cookie.Value.ToString().Split(',');
                    cookie.Value = "";
                    foreach (string cookieGiftId in cookieGiftIds)
                    {
                        if (!string.IsNullOrWhiteSpace(cookieGiftId))
                        {
                            if (Convert.ToInt64(cookieGiftId) != giftId)
                            {
                                cookie.Value = cookie.Value + "," + cookieGiftId;
                            }
                        }
                    }
                    cookie.Value = giftId + "," + cookie.Value;
                    Response.Cookies.Set(cookie);
                }
                else
                {
                    cookie = new HttpCookie("LastViewedGifts" + currentUser.UserId);
                    cookie.Value = giftId.ToString();
                    Response.Cookies.Add(cookie);
                }
            }
            //获取
            ViewData["successCommentsCount"] = pointMallService.GetRecordsCount(giftId, ApproveStatus.Approved).TotalRecords;
            ViewData["pendingCommentsCount"] = pointMallService.GetRecordsCount(giftId, ApproveStatus.Pending).TotalRecords;
            return View(gift);
        }

        /// <summary>
        /// 创建兑换记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _CreateRecord(long giftId)
        {
            //获取默认邮寄地址
            MailAddress mailAddress = null;
            IEnumerable<MailAddress> mails = new MailAddressService().GetsOfUser(UserContext.CurrentUser.UserId);
            //从数据库中取实体，取不到则创建
            mailAddress = mails.Count() > 0 ? mails.First() : MailAddress.New();
            var mailAddressEditModel = mailAddress.AsEditModel();

            //创建兑换记录的EditModel
            IUser currentUser = UserContext.CurrentUser;
            PointGift gift = pointMallService.GetGift(giftId);
            var recordEditModel = new RecordEditModel();
            recordEditModel.Address = mailAddressEditModel.Address ?? string.Empty;
            recordEditModel.Addressee = mailAddressEditModel.Addressee ?? string.Empty;
            recordEditModel.PostCode = mailAddressEditModel.PostCode ?? string.Empty;
            recordEditModel.Tel = mailAddressEditModel.Tel ?? string.Empty;
            recordEditModel.GiftId = giftId;
            recordEditModel.Payer = currentUser.DisplayName;
            recordEditModel.PayerUserId = currentUser.UserId;
            recordEditModel.GiftName = gift.Name;
            ViewData["recordEditModel"] = recordEditModel;

            return View(mailAddressEditModel);
        }

        /// <summary>
        /// 创建兑换记录的Post方法
        /// </summary>
        ///<param name="recordEditModel">数据库编辑实体</param>
        /// <returns>Json</returns>
        [HttpPost]
        public ActionResult _CreateRecord(RecordEditModel recordEditModel)
        {
            PointGiftExchangeRecord record = recordEditModel.AsPointGiftExchangeRecord();
            if (record.PointGift == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到商品"));

            if (!record.PointGift.IsEnabled)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "该商品已下架,不允许兑换！"));
            }

            if (record.Number <= 0)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "兑换数量必须为正整数！"));
            }

            var user = DIContainer.Resolve<UserService>().GetUser(recordEditModel.PayerUserId);

            var totalPrice = record.Number * record.PointGift.Price;
            record.Price = totalPrice;
            if (totalPrice > user.TradePoints)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "你没有这么多积分！"));
            }

            pointMallService.CreateRecord(record);

            if (record.RecordId <= 0)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "兑换商品失败！"));
            }

            //兑换数增加
            record.PointGift.ExchangedCount += record.Number;
            pointMallService.UpdateGift(record.PointGift);
            return Json(new StatusMessageData(StatusMessageType.Success, "兑换商品成功！"));


        }

        /// <summary>
        /// 设置邮寄地址的Post方法
        /// </summary>
        /// <param name="mailAddressEditModel">邮寄地址的editModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SetMailAddress(MailAddressEditModel mailAddressEditModel)
        {
            var mailAddress = mailAddressEditModel.AsMailAddress();
            if (mailAddressEditModel.AddressId > 0)
            {   //编辑邮寄地址
                mailAddressService.Edit(mailAddress);
            }
            //添加邮寄地址
            mailAddressService.Create(mailAddress);
            return Json(new StatusMessageData(StatusMessageType.Success, "设置默认邮寄地址成功！"));
        }



        #endregion

        #region 局部页

        /// <summary>
        /// 商品类别
        /// </summary>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _GiftCategories()
        {
            IEnumerable<Category> categories = categoryService.GetRootCategories(TenantTypeIds.Instance().PointGift());
            return View(categories);
        }

        /// <summary>
        /// 热销商品
        /// </summary>
        /// <param name="topNumber">显示个数</param>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _BestSellGifts(int topNumber = 6)
        {
            IEnumerable<PointGift> gifts = pointMallService.GetGifts(SortBy_PointGift.Sales_Desc, topNumber);
            return View(gifts);
        }

        /// <summary>
        /// 人气商品
        /// </summary>
        /// <param name="topNumber">显示个数</param>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _HotGifts(int topNumber = 5)
        {
            IEnumerable<PointGift> gifts = pointMallService.GetGifts(SortBy_PointGift.HitTimes_Desc, topNumber);
            return View(gifts);
        }

        /// <summary>
        /// 最新商品
        /// </summary>
        /// <param name="topNumber">个数</param>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _NewestGifts(int topNumber = 10)
        {
            IEnumerable<PointGift> gifts = pointMallService.GetGifts(SortBy_PointGift.DateCreated_Desc, topNumber);
            return View(gifts);
        }

        /// <summary>
        /// 积分商城首页推荐商品幻灯片
        /// </summary>
        /// <param name="topNumber">显示个数</param>
        /// <param name="recommendTypeId">类型</param>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _RecommendGiftsHome(int topNumber = 6, string recommendTypeId = null)
        {
            IEnumerable<RecommendItem> recommendGifts = recommendService.GetTops(topNumber, recommendTypeId);
            return View(recommendGifts);
        }

        /// <summary>
        /// 推荐商品
        /// </summary>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _RecommendGiftsSide()
        {
            //推荐商品
            IEnumerable<RecommendItem> recommendItems = recommendService.GetTops(6, pointMallSettings.RecommendGiftTypeId);
            return View(recommendItems);
        }

        /// <summary>
        /// 浏览过的商品
        /// </summary>
        /// <returns></returns>
        public ActionResult _LastViewedGifts()
        {
            List<PointGift> pointGifts = new List<PointGift>();
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null)
            {
                //读取浏览过的商品
                HttpCookie cookie = Request.Cookies["LastViewedGifts" + currentUser.UserId];
                if (cookie != null)
                {
                    string[] cookieGiftIds = cookie.Value.ToString().Split(',');
                    foreach (string cookieGiftId in cookieGiftIds)
                    {
                        if (!string.IsNullOrWhiteSpace(cookieGiftId))
                        {
                            pointGifts.Add(pointMallService.GetGift(Convert.ToInt64(cookieGiftId)));
                        }
                    }
                }
            }

            return View(pointGifts.Take(6));
        }

        /// <summary>
        /// 商品兑换记录
        /// </summary>
        /// <param name="giftId">商品Id</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页多少条</param>
        /// <param name="approveStatus">审核状态</param>
        /// <returns></returns>
        public ActionResult _GiftComments(long giftId, int pageIndex = 1, int pageSize = 20, ApproveStatus? approveStatus = null)
        {
            //获取评论
            PagingDataSet<PointGiftExchangeRecord> comments = pointMallService.GetRecords(giftId, pageSize, pageIndex, approveStatus);

            ViewData["giftId"] = giftId;
            return View(comments);
        }

        /// <summary>
        /// 收藏商品按钮
        /// </summary>
        public ActionResult _FavoriteButton(long giftId)
        {
            if (UserContext.CurrentUser != null)
            {
                ViewData["isFavorited"] = favoriteService.IsFavorited(giftId, UserContext.CurrentUser.UserId);
            }
            ViewData["giftId"] = giftId;
            return View();
        }

        /// <summary>
        /// 收藏商品操作
        /// </summary>
        /// <param name="giftId">商品</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _Favorite(long giftId)
        {
            IUser currentUser = UserContext.CurrentUser;

            if (currentUser == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "必须先登录，才能继续操作"));
            }

            PointGift pointGift = pointMallService.GetGift(giftId);

            if (pointGift == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到要被收藏的商品"));

            }

            if (favoriteService.IsFavorited(giftId, currentUser.UserId))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "您已经收藏过该商品"));

            }

            favoriteService.Favorite(giftId, currentUser.UserId);

            return Json(new StatusMessageData(StatusMessageType.Success, "关注成功"));

        }

        /// <summary>
        /// 取消收藏商品操作
        /// </summary>
        [HttpPost]
        public JsonResult _FavoriteCancel(long giftId)
        {
            if (UserContext.CurrentUser == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "必须先登录，才能继续操作"));
            }

            long userId = UserContext.CurrentUser.UserId;

            if (!favoriteService.IsFavorited(giftId, userId))
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "您没有收藏过该商品"));
            }

            PointGift pointGift = pointMallService.GetGift(giftId);

            if (pointGift == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到要被收藏的商品"));
            }

            favoriteService.CancelFavorite(giftId, userId);

            return Json(new StatusMessageData(StatusMessageType.Success, "取消关注操作成功"));

        }

        #endregion



    }
}