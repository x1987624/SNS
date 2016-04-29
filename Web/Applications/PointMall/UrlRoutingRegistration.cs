//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Configuration;
using System.Web.Mvc;
using Spacebuilder.PointMall;
using Tunynet.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城路由设置
    /// </summary>
    public class UrlRoutingRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "PointMall"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //对于IIS6.0默认配置不支持无扩展名的url
            string extensionForOldIIS = ".html";
            int iisVersion = 0;

            if (!int.TryParse(ConfigurationManager.AppSettings["IISVersion"], out iisVersion))
                iisVersion = 7;
            if (iisVersion >= 7)
                extensionForOldIIS = string.Empty;

            #region Channel
            //商城首页
            context.MapRoute(
              "Channel_PointMall_Home", // Route name
              "PointMall" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelPointMall", action = "Home", CurrentNavigationId = "10200101" } // Parameter defaults
            );

            //商品详细显示页
            context.MapRoute(
                "Channel_PointMall_Detail", // Route name
                "PointMall/g-{giftId}" + extensionForOldIIS, // URL with parame ters
                new { controller = "ChannelPointMall", action = "GiftDetail", CurrentNavigationId = "10200101" }, // Parameter defaults
                new { giftId = @"(\d+)|(\{\d+\})" }
            );

            //商城在频道下的其它页面
            context.MapRoute(
                "Channel_PointMall_Common", // Route name
                "PointMall/{action}" + extensionForOldIIS, // URL with parame ters
                new { controller = "ChannelPointMall" } // Parameter defaults
            );
            #endregion


            #region 动态
            context.MapRoute(
               string.Format("ActivityDetail_{0}_ExchangeGift", TenantTypeIds.Instance().PointGiftExchangeRecord()), // Route name
                "PointMallActivity/CreatePointMall/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "PointMallActivity", action = "ExchangeGift" } // Parameter defaults
            );
            #endregion

            #region UserSpace
            //商城首页
            context.MapRoute(
                "UserSpace_PointMall_Home", // Route name
                "u/{SpaceKey}/PointMallHome" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePointMall", action = "Home", CurrentNavigationId = "11200102" } // Parameter defaults
            );

            //商城首页
            context.MapRoute(
                "ApplicationCount_PointMall", // Route name
                "u/{SpaceKey}/PointMallHome" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePointMall", action = "Home", CurrentNavigationId = "11200102" } // Parameter defaults
            );

            //我的收藏
            context.MapRoute(
                "UserSpace_PointMall_Favorite", // Route name
                "u/{SpaceKey}/PointMallFavorites" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePointMall", action = "MyFavorites", CurrentNavigationId = "11200103" } // Parameter defaults
            );

            //商城在用户空间下的其它页面
            context.MapRoute(
                "UserSpace_PointMall_Common", // Route name
                "u/{SpaceKey}/PointMall/{action}" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePointMall", CurrentNavigationId = "11200104" } // Parameter defaults
            );


            #endregion

            #region ControlPanel
            context.MapRoute(
                "ControlPanel_PointMall_Home", // Route name
                "ControlPanel/Content/PointMall/ManagePointGifts" + extensionForOldIIS, // URL with parameters
                new { controller = "ControlPanelPointMall", action = "ManagePointGifts", CurrentNavigationId = "20200101" } // Parameter defaults
            );

            context.MapRoute(
                "ControlPanel_PointMall_Common", // Route name
                "ControlPanel/Content/PointMall/{action}" + extensionForOldIIS, // URL with parameters
                new { controller = "ControlPanelPointMall", CurrentNavigationId = "20200101" } // Parameter defaults
            );
            #endregion
        }
    }
}