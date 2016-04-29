//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;
using System;

using System.Linq;
using Tunynet.Utilities;

namespace Spacebuilder.PointMall.Controllers
{
    /// <summary>
    /// 积分商城后台管理控制器
    /// </summary>
    [ManageAuthorize]
    [TitleFilter(TitlePart = "积分商城", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.ControlPanel, IsApplication = true)]
    public class ControlPanelPointMallController : Controller
    {
        public IPageResourceManager pageResourceManager { get; set; }
        public PointMallService pointMallService { get; set; }
        public CategoryService categoryService { get; set; }

        #region 管理商品

        /// <summary>
        /// 管理商品
        /// </summary>
        /// <param name="name">商品名</param>
        /// <param name="categoryId">分类Id</param>
        /// <param name="isEnabled">是否上架</param>
        /// <param name="maxPrice">最大价格</param>
        /// <param name="minPrice">最小价格</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ManagePointGifts(string name, long? categoryId, bool? isEnabled, int? maxPrice, int? minPrice, int pageIndex = 1, int pageSize = 20)
        {
            pageResourceManager.InsertTitlePart("商品管理");
            PagingDataSet<PointGift> pointGifts = pointMallService.GetGiftsForAdmin(name, categoryId, isEnabled, maxPrice ?? 0, minPrice ?? 0, pageSize, pageIndex);
            List<SelectListItem> items = new List<SelectListItem> { new SelectListItem { Text = "已上架", Value = true.ToString() }, new SelectListItem { Text = "已下架", Value = false.ToString() } };
            ViewData["isEnabledList"] = new SelectList(items, "Value", "Text", isEnabled);
            return View(pointGifts);
        }

        /// <summary>
        /// 更改商品状态
        /// </summary>
        /// <param name="giftIds">商品Id集合</param>
        /// <param name="isEnabled">是否上架</param>
        /// <returns>Json</returns>
        [HttpPost]
        public ActionResult _ChangeGiftStatus(IEnumerable<long> giftIds, bool isEnabled)
        {
            if (giftIds == null)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到要操作的商品，操作失败！"));
            }
            RecommendService recommendService = new RecommendService();
            foreach (var item in giftIds)
            {
                pointMallService.SetEnabled(pointMallService.GetGift(item), isEnabled);
                if (!isEnabled)
                {
                    //删除该商品上的推荐
                    recommendService.Delete(item, TenantTypeIds.Instance().PointGift());
                }
            }
            string message;
            if (isEnabled)
            {
                message = "上架操作成功！";
            }
            else
            {
                message = "下架操作成功！";
            }
            return Json(new StatusMessageData(StatusMessageType.Success, message));
        }

        /// <summary>
        /// 修改商品类别的Get方法
        /// </summary>
        /// <param name="giftIds">商品Id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _SetGiftCategories(IEnumerable<long> giftIds)
        {
            ViewData["giftIds"] = giftIds;
            return View();
        }

        /// <summary>
        /// 类别设置的Post方法
        /// </summary>
        /// <param name="giftIds">商品Id集合</param>
        /// <param name="categoryId">要修改的类别Id</param>
        /// <returns>Json</returns>
        [HttpPost]
        public ActionResult _SetGiftCategories(IEnumerable<long> giftIds, long categoryId)
        {
            foreach (var giftId in giftIds)
            {
                categoryService.ClearCategoriesFromItem(giftId, 0, TenantTypeIds.Instance().PointGift());
            }
            categoryService.AddItemsToCategory(giftIds, categoryId);

            return Json(new StatusMessageData(StatusMessageType.Success, "设置类别成功！"));
        }
        #endregion

        #region 编辑商品

        /// <summary>
        /// 添加/编辑商品的Get方法
        /// </summary>
        /// <param name="giftId">商品Id</param>
        /// <returns>View</returns>
        [HttpGet]
        public ActionResult EditGift(long giftId = 0)
        {
            string title = giftId > 0 ? "编辑商品" : "添加商品";
            pageResourceManager.InsertTitlePart(title);

            pageResourceManager.AppendTitleParts(title);

            PointGiftEditModel pointGiftEditModel = new PointGiftEditModel();
            if (giftId > 0)
            {
                pointGiftEditModel = pointMallService.GetGift(giftId).AsPointGiftEditModel();

            }
            return View(pointGiftEditModel);
        }

        /// <summary>
        /// 添加/修改商品的post方法
        /// </summary>
        /// <param name="model">商品EditModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditGift(PointGiftEditModel model)
        {
            PointGift pointGift = model.AsPointGift();

            if (pointGift.GiftId > 0)
            {
                pointMallService.UpdateGift(pointGift);

                categoryService.ClearCategoriesFromItem(pointGift.GiftId, 0, TenantTypeIds.Instance().PointGift());
            }
            else
            {
                if (!pointMallService.CreateGift(pointGift))
                {
                    return Json(new StatusMessageData(StatusMessageType.Error, "添加商品失败！"));
                }
            }

            //添加类别
            categoryService.AddItemsToCategory(new List<long>() { pointGift.GiftId }, model.CategoryId);
            return Json(new StatusMessageData(StatusMessageType.Success, model.GiftId > 0 ? "编辑商品成功！" : "添加商品成功！"));
        }

        #endregion

        #region 管理申请

        /// <summary>
        /// 管理申请
        /// </summary>
        /// <param name="beginDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="userId">用户Id</param>
        /// <param name="approveStatus">审核状态</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ManageRecords(DateTime? beginDate, DateTime? endDate, string userId = null, ApproveStatus? approveStatus = null, int pageIndex = 1, int pageSize = 20)
        {
            pageResourceManager.InsertTitlePart("申请管理");
            
            long _userId = 0;
            if (!string.IsNullOrEmpty(userId))
            {
                userId = userId.Trim(',');
            }
            if (!string.IsNullOrEmpty(userId))
            {
                _userId = long.Parse(userId);
                ViewData["userId"] = _userId;
            }
            PagingDataSet<PointGiftExchangeRecord> pointGiftExchangeRecord = pointMallService.GetRecordsForAdmin(_userId, approveStatus, beginDate, endDate ?? DateTime.UtcNow, pageSize, pageIndex);
            return View(pointGiftExchangeRecord);
        }

        /// <summary>
        /// 修改兑换申请状态
        /// </summary>
        /// <param name="recordIds">要处理的兑换记录Id集合</param>
        /// <param name="isApprove">是否接受</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _ChangeRecordStatus(IEnumerable<long> recordIds, bool isApprove)
        {
            int count=0;
            foreach (var recordId in recordIds)
            {
                PointGiftExchangeRecord record = pointMallService.GetRecord(recordId);
                //待批准的才能操作
                if (record.Status == ApproveStatus.Pending)
                {
                    pointMallService.IsApprove(record, isApprove);
                    count++;
                }
            }
            if (count == recordIds.Count())
            {
                return Json(new StatusMessageData(StatusMessageType.Success, isApprove ? "批准操作成功！" : "拒绝操作成功！"));
            }
            else
            {
                return Json(new StatusMessageData(StatusMessageType.Error, isApprove ? "共有" + count + "个申请批准成功，共" + (recordIds.Count() - count) + "个已处理的申请无法被批准。" : "共有" + count + "个申请拒绝成功，共" + (recordIds.Count() - count) + "个已处理的申请无法被拒绝。"));
            }
            
        }

        /// <summary>
        /// 删除兑换记录
        /// </summary>
        /// <param name="recordIds">要删除的兑换记录Id集合</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _DeleteRecords(IEnumerable<long> recordIds)
        {
            int count = 0;
            for (int i = 0; i < recordIds.Count(); i++)
            {
                var recordId = recordIds.ElementAt(i);
                if (!pointMallService.CancelRecord(pointMallService.GetRecord(recordId)))
                {
                    count++;
                }
            }
            if (count > 0)
            {
                return Json(new StatusMessageData(StatusMessageType.Success, count > 0 ? "共" + (recordIds.Count() - count) + "条待批准的记录被删除！共有" + count + "条已处理的记录无法被删除！" : "共" + (recordIds.Count() - count) + "条待处理记录被删除！"));
            }
            return Json(new StatusMessageData(StatusMessageType.Success,"删除操作成功！"));
        }

        /// <summary>
        /// 设置跟踪信息的Get方法
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _SetTrackInfo(long recordId)
        {
            var record = pointMallService.GetRecord(recordId);
            ViewData["trackInfo"] = record.TrackInfo;
            return View();
        }

        /// <summary>
        /// 设置跟踪信息的post方法
        /// </summary>
        /// <param name="recordId">兑换申请Id</param>
        /// <param name="trackInfo">跟踪信息</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SetTrackInfo(long recordId, string trackInfo)
        {
            pointMallService.SetTrackInfo(recordId, StringUtility.Trim(trackInfo, 250));
            return Json(new StatusMessageData(StatusMessageType.Success, "设置收件地址成功！"));
        }

        /// <summary>
        /// 展示联系方式的Get方法
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _ShowMailAddressInfo(long recordId)
        {
            return View(pointMallService.GetRecord(recordId));
        }

        #endregion
    }

}