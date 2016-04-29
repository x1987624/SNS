//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;

namespace Spacebuilder.PointMall.Controllers
{
    /// <summary>
    /// 积分商城用户控件控制器
    /// </summary>
    [TitleFilter(TitlePart = "积分商城", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.UserSpace, IsApplication = true)]
    [AnonymousBrowseCheck]
    [UserSpaceAuthorize]
    public partial class UserSpacePointMallController : Controller
    {
        public Authorizer authorizer { get; set; }
        public ContentPrivacyService contentPrivacyService { get; set; }
        public IPageResourceManager pageResourceManager { get; set; }
        public PointMallService pointMallService { get; set; }
        public UserService userService { get; set; }
        private FavoriteService favoriteService = new FavoriteService(TenantTypeIds.Instance().PointGift());

        #region 兑换申请

        /// <summary>
        /// 兑换申请
        /// </summary>
        /// <param name="date">查看时间</param>
        /// <param name="approveStatus">申请状态</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Home(string date, ApproveStatus? approveStatus=null, int pageIndex = 1)
        {
            //判断用户是否登陆
            IUser user = UserContext.CurrentUser;
            if (user == null)
            {
                return Redirect(SiteUrls.Instance().Login(true));
            }
            if (user.UserName != Url.SpaceKey())
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有访问的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            pageResourceManager.InsertTitlePart("我的商品申请");

            //时间下拉表
            List<SelectListItem> selectListItems = new List<SelectListItem>();
            for (int i = 0; i < 5; i++)
            {
                SelectListItem item = new SelectListItem();
                item.Text = DateTime.Now.AddYears(-i).Year.ToString();
                item.Value = DateTime.Now.AddYears(-i).Year.ToString();
                selectListItems.Add(item);
            }
            SelectList selectList = new SelectList(selectListItems, "Value", "Text");
            ViewData["selectList"] = selectList;

            //时间选择器时间处理（选中年份的1月1日到下一年的1月1日）
            long userId = user.UserId;
            DateTime beginDate = DateTime.Now.AddYears(-100);
            DateTime endDate = DateTime.Now;
            if (!string.IsNullOrEmpty(date))
            {
                beginDate = Convert.ToDateTime(date + "/01/01");
                endDate = Convert.ToDateTime((Convert.ToInt32(date) + 1) + "/01/01");
            }

            //获取兑换申请
            PagingDataSet<PointGiftExchangeRecord> records = pointMallService.GetRecordsOfUser(userId, beginDate, endDate, approveStatus, 20, pageIndex);
            return View(records);
        }

        /// <summary>
        /// 评价模态框
        /// </summary>
        /// <param name="recordId">兑换记录Id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _Appraise(long recordId)
        {
            PointGiftExchangeRecord record = pointMallService.GetRecord(recordId);
            if (record.PayerUserId != UserContext.CurrentUser.UserId)
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有评价的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            return View(record);
        }

        /// <summary>
        /// 评价
        /// </summary>
        /// <param name="recordId">兑换记录Id</param>
        /// <param name="appraise">评价内容</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _Appraise(long recordId, string appraise)
        {
            PointGiftExchangeRecord record = pointMallService.GetRecord(recordId);
            if (record.PayerUserId != UserContext.CurrentUser.UserId)
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有评价的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            pointMallService.SetAppraise(recordId, Formatter.FormatMultiLinePlainTextForStorage(appraise, true) ?? string.Empty);
            return Json(new StatusMessageData(StatusMessageType.Success, "评价成功！"));
        }

        /// <summary>
        /// 取消兑换申请
        /// </summary>
        /// <param name="recordId">兑换记录Id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _CancelExchange(long recordId)
        {
            PointGiftExchangeRecord record = pointMallService.GetRecord(recordId);
            if (record.PayerUserId != UserContext.CurrentUser.UserId)
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "没有取消兑换申请的权限",
                    Title = "没有权限",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            pointMallService.CancelRecord(pointMallService.GetRecord(recordId));
            return Json(new StatusMessageData(StatusMessageType.Success, "取消兑换成功！"));
        }


        #endregion

        #region 我的收藏
        /// <summary>
        /// 我的收藏
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MyFavorites(int pageIndex = 1)
        {
            //判断用户是否已登陆
            IUser user = UserContext.CurrentUser;
            if (user == null)
            {
                return Redirect(SiteUrls.Instance().Login(true));
            }
            pageResourceManager.InsertTitlePart("我的收藏");

            //获取收藏对象Id分页数据
            PagingDataSet<long> giftIds = favoriteService.GetPagingObjectIds(user.UserId, pageIndex, 20);
            IEnumerable<PointGift> gifts = pointMallService.GetGifts(giftIds);
            PagingDataSet<PointGift> pagingGifts = new PagingDataSet<PointGift>(gifts)
            {
                PageIndex = giftIds.PageIndex,
                PageSize = giftIds.PageSize,
                TotalRecords = giftIds.TotalRecords
            };
            return View(pagingGifts);
        }

        /// <summary>
        /// 取消收藏
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _CancelFavorite(long objectId)
        {
            if (favoriteService.CancelFavorite(objectId, UserContext.CurrentUser.UserId))
            {
                return Json(new StatusMessageData(StatusMessageType.Success, "取消成功！"));
            }
            return Json(new StatusMessageData(StatusMessageType.Error, "取消失败！"));
        }
        #endregion

    }
}