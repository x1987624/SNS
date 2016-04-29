//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet.UI;
using Tunynet.Common;
using Tunynet;
using DevTrends.MvcDonutCaching;

namespace Spacebuilder.PointMall.Controllers
{
    /// <summary>
    /// 积分商城动态控制器
    /// </summary>
    [Themed(PresentAreaKeysOfBuiltIn.Channel, IsApplication = true)]
    [AnonymousBrowseCheck]
    public class PointMallActivityController : Controller
    {
        public ActivityService activityService { get; set; }
        public PointMallService pointMallService { get; set; }
        public CommentService commentService { get; set; }
        public UserService userService { get; set; }
        private AttitudeService attitudeService = new AttitudeService(TenantTypeIds.Instance().PointGift());

        /// <summary>
        /// 商品兑换动态
        /// </summary>
        /// <param name="ActivityId"></param>
        /// <returns></returns>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult ExchangeGift(long ActivityId)
        {
            //实例化动态
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            IEnumerable<PointGiftExchangeRecord> records = pointMallService.GetRecordsOfUser(activity.OwnerId, DateTime.Now.AddYears(-1),DateTime.Now, ApproveStatus.Approved, 2, 1);

            return View(records);
        }
    }
}
