using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Tunynet.UI;
using Tunynet.Common;
using DevTrends.MvcDonutCaching;

namespace Spacebuilder.Common
{
    /// <summary>
    /// Class HomeController.
    /// </summary>
    [Themed(PresentAreaKeysOfBuiltIn.Home, IsApplication = false)]
    [TitleFilter(IsAppendSiteName = true)]
    public class HomeController : Controller
    {
        /// <summary>
        /// 首页.
        /// </summary>
        /// <author version="">肖晏</author>
        /// <modifier version=""></modifier>
        /// <returns>ActionResult.</returns>
        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 模块一幻灯广告.
        /// </summary>
        /// <author version="">肖晏</author>
        /// <modifier version=""></modifier>
        /// <returns>ActionResult.</returns>
        public ActionResult _PartialSlideAd()
        {

            return PartialView();
        }

        /// <summary>
        /// 模块一幻灯广告下tab.
        /// </summary>
        /// <author version="">肖晏</author>
        /// <modifier version=""></modifier>
        /// <returns>ActionResult.</returns>
        public ActionResult _PartialSubjectTab()
        {
            return PartialView();
        }

        /// <summary>
        /// 模块一热门新闻.
        /// </summary>
        /// <author version="">肖晏</author>
        /// <modifier version=""></modifier>
        /// <returns>ActionResult.</returns>
        public ActionResult _PartialHotNews()
        {
            return PartialView();
        }

        /// <summary>
        /// 模块一热门新闻下广告.
        /// </summary>
        /// <author version="">肖晏</author>
        /// <modifier version=""></modifier>
        /// <returns>ActionResult.</returns>
        public ActionResult _PartialHotNewsUnderAd()
        {
            return PartialView();
        }

        public ActionResult _PartialCommunityActivities(){
            return PartialView();
        }

        /// <summary>
        /// 底部.
        /// </summary>
        /// <author version=""></author>
        /// <modifier version=""></modifier>
        /// <returns>ActionResult.</returns>
        //[DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _Header()
        {
            return PartialView();
        }

        /// <summary>
        /// 底部.
        /// </summary>
        /// <author version=""></author>
        /// <modifier version=""></modifier>
        /// <returns>ActionResult.</returns>
        //[DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _Footer()
        {
            return PartialView();
        }
    }
}
