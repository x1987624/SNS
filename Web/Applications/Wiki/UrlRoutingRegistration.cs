//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Configuration;
using System.Web.Mvc;
using Spacebuilder.Wiki.Controllers;
using Tunynet.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科路由设置
    /// </summary>
    public class UrlRoutingRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Wiki"; }
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

            //百科频道-百科首页
            context.MapRoute(
              "Channel_Wiki_Home", // Route name
              "Wiki" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "Index", CurrentNavigationId = "10101602" } // Parameter defaults
            );

            //百科频道-问题
            context.MapRoute(
              "Channel_Wiki_Pages", // Route name
              "Wiki/Pages" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "Pages", CurrentNavigationId = "10101601 " } // Parameter defaults
            );

            //百科频道-问题详情页
            context.MapRoute(
              "Channel_Wiki_PageDetail", // Route name
              "Wiki/p/{pageId}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "PageDetail", CurrentNavigationId = "10101601 " } // Parameter defaults
            );

            //百科频道-标签详情页
            context.MapRoute(
              "Channel_Wiki_TagDetail", // Route name
              "Wiki/t/{tagName}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "Pages_tag", CurrentNavigationId = "10101601 " } // Parameter defaults
            );

            //百科频道-我的百科
            context.MapRoute(
              "Channel_Wiki_User", // Route name
              "Wiki/u/{SpaceKey}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "WikiUser", CurrentNavigationId = "10101605" } // Parameter defaults
            );

            //百科频道-我的百科
            context.MapRoute(
              "Channel_Wiki_My", // Route name
              "Wiki/My" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "WikiUser", CurrentNavigationId = "10101605" } // Parameter defaults
            );

            //百科频道-提问
            context.MapRoute(
              "Channel_Wiki_EditPage", // Route name
              "Wiki/EditPage" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "EditPage", CurrentNavigationId = "10101605" } // Parameter defaults
            );


            //演讲模块
            context.MapRoute(
              "Channel_HaierSnsSpeechModule_SpeechModule", // Route name
              "Sp" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "SpeechModule" } // Parameter defaults
            );

            //演讲模块申请表
            context.MapRoute(
              "Channel_HaierSnsSpeechModule_manageapplysforuser", // Route name
              "Sp/Mf/{tenantTypeIdForApply}_{tenantTypeIdForItem}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "manageapplysforuser"} // Parameter defaults
            );
            //演讲模块演讲稿
            context.MapRoute(
              "Channel_HaierSnsSpeechModule_speechmodules", // Route name
              "Sps/c-{categoryid}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "speechmodules"} // Parameter defaults
            );
            //演讲模块演讲稿
            context.MapRoute(
              "Channel_HaierSnsSpeechModule_SpeechModuleDetail", // Route name
              "Sp/P-{pageid}" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelWiki", action = "SpeechModuleDetail" } // Parameter defaults
            );
            

            context.MapRoute(
                "Channel_Wiki_Common", // Route name
                "Wiki/{action}" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelWiki", action = "Index" } // Parameter defaults
            );

            #endregion

            

            #region ControlPanel
            context.MapRoute(
                "ControlPanel_Wiki_Home", // Route name
                "ControlPanel/Content/Wiki/ManagePages" + extensionForOldIIS, // URL with parameters
                new { controller = "ControlPanelWiki", action = "ManagePages", CurrentNavigationId = "20101601" } // Parameter defaults
            );

            context.MapRoute(
                "ControlPanel_Wiki_Common", // Route name
                "ControlPanel/Content/Wiki/{action}" + extensionForOldIIS, // URL with parameters
                new { controller = "ControlPanelWiki", CurrentNavigationId = "20101601" } // Parameter defaults
            );

            #endregion

        }
    }
}