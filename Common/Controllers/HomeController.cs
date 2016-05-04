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
