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
using System.Linq;
using System;


namespace Spacebuilder.Wiki.Controllers
{


    /// <summary>
    /// 百科管理控制器
    /// </summary>
    [ManageAuthorize(CheckCookie = false)]
    [TitleFilter(TitlePart = "百科管理", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.ControlPanel, IsApplication = true)]
    public class ControlPanelWikiController : Controller
    {
        private IPageResourceManager pageResourceManager = DIContainer.ResolvePerHttpRequest<IPageResourceManager>();
        private WikiService wikiService = new WikiService();
        private CategoryService categoryService = new CategoryService();
        private UserService userService = new UserService();

        #region 词条管理

        /// <summary>
        /// wiki管理页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ManagePages(AuditStatus? auditStatus = null, long? categoryId = null, bool? isEssential = null, long? userId = null, string titleKeywords = null, int pageSize = 20, int pageIndex = 1)
        {
            pageResourceManager.InsertTitlePart("百科管理");

            PagingDataSet<WikiPage> wikiPageList = wikiService.GetsForAdmin(TenantTypeIds.Instance().Wiki(), auditStatus, categoryId, isEssential, userId, titleKeywords, pageSize, pageIndex);

            //获取类别
            IEnumerable<Category> categorys = categoryService.GetOwnerCategories(0, TenantTypeIds.Instance().WikiPage());
            SelectList categoryList = new SelectList(categorys.Select(n => new { text = n.CategoryName, value = n.CategoryId }), "value", "text", categoryId);
            ViewData["categoryList"] = categoryList;

            ViewData["userId"] = userId;
            List<SelectListItem> selectListIsEssential = new List<SelectListItem> { new SelectListItem { Text = "是", Value = true.ToString() }, new SelectListItem { Text = "否", Value = false.ToString() } };
            ViewData["isEssential"] = new SelectList(selectListIsEssential, "Value", "Text", isEssential);
            return View(wikiPageList);
        }

        /// <summary>
        /// 删除词条
        /// </summary>
        /// <param name="wikiIds"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _DeleteWikiPage(IEnumerable<long> pageIds)
        {
            if (pageIds != null && pageIds.Count() > 0)
            {
                foreach (long item in pageIds)
                {
                    wikiService.Delete(item);
                }
                return Json(new StatusMessageData(StatusMessageType.Success, "词条删除成功"));
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "请选择词条"));
        }

        /// <summary>
        /// 更新词条审核状态
        /// </summary>
        /// <param name="pageIds"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _ApproveWikiPage(IEnumerable<long> pageIds, bool isApproved)
        {
            if (pageIds != null && pageIds.Count() > 0)
            {
                foreach (long item in pageIds)
                {
                    wikiService.Approve(item, isApproved);
                }
                return Json(new StatusMessageData(StatusMessageType.Success, "更新词条成功"));
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "请选择词条"));
        }


        /// <summary>
        /// 更新词条锁定状态 
        /// </summary>
        /// <param name="pageIds"></param>
        /// <param name="isLocked"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _WikiPageSetLock(IEnumerable<long> pageIds, bool isLocked)
        {
            if (pageIds != null && pageIds.Count() > 0)
            {
                foreach (long item in pageIds)
                {
                    wikiService.SetLock(item, isLocked);
                }
                return Json(new StatusMessageData(StatusMessageType.Success, "更新词条成功"));
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "请选择词条"));
        }
        #endregion
        /// <summary>
        /// 设置分类
        /// </summary>
        /// <param name="pageIds"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _SetCategory(string pageIds)
        {

            IEnumerable<Category> categorys = categoryService.GetOwnerCategories(0, TenantTypeIds.Instance().WikiPage());
            SelectList categoryList = new SelectList(categorys.Select(n => new { text = n.CategoryName, value = n.CategoryId }), "value", "text", categorys.First().CategoryId);

            ViewData["categoryList"] = categoryList;
            return View();
        }

        /// <summary>
        /// 设置分类
        /// </summary>
        /// <param name="pageIds"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _SetCategory(string pageIds, long categoryId)
        {

            IUser user = UserContext.CurrentUser;
            if (!string.IsNullOrEmpty(pageIds))
            {
                string[] pageIdsArray = pageIds.TrimEnd(',').Split(',');
                foreach (string item in pageIdsArray)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;

                    categoryService.ClearCategoriesFromItem(long.Parse(item), 0, TenantTypeIds.Instance().WikiPage());
                    categoryService.AddCategoriesToItem(new List<long> { categoryId }, long.Parse(item));

                }
                return Json(new StatusMessageData(StatusMessageType.Success, "成功设置分类"));
            }
            else
                return Json(new StatusMessageData(StatusMessageType.Success, "请选择词条"));
        }


        #region 词条版本管理

        /// <summary>
        /// 词条版本管理
        /// </summary>
        /// <param name="auditStatus"></param>
        /// <param name="categoryId"></param>
        /// <param name="userId"></param>
        /// <param name="titleKeywords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult ManageVersions(AuditStatus? auditStatus = null, long? categoryId = null, long? userId = null, string titleKeywords = null, int pageSize = 20, int pageIndex = 1)
        {
            pageResourceManager.InsertTitlePart("词条版本管理");

            PagingDataSet<WikiPageVersion> wikiPageList = wikiService.GetPageVersionsForAdmin(TenantTypeIds.Instance().Wiki(), auditStatus, categoryId, userId, titleKeywords, pageSize, pageIndex);


            //获取类别
            IEnumerable<Category> categorys = categoryService.GetOwnerCategories(0, TenantTypeIds.Instance().WikiPage());
            SelectList categoryList = new SelectList(categorys.Select(n => new { text = n.CategoryName, value = n.CategoryId }), "value", "text", categoryId);
            ViewData["categoryList"] = categoryList;

            return View(wikiPageList);
        }

        /// <summary>
        /// 删除词条版本
        /// </summary>
        /// <param name="versionIds"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _DeleteWikiPageVersions(IEnumerable<long> versionIds)
        {
            if (versionIds != null && versionIds.Count() > 0)
            {
                foreach (long item in versionIds)
                {
                    wikiService.DeletePageVersion(item);
                }
                return Json(new StatusMessageData(StatusMessageType.Success, "词条版本删除成功"));
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "请选择词条版本"));
        }

        /// <summary>
        /// 更新词条版本审核状态
        /// </summary>
        /// <param name="pageIds"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _ApproveWikiPageVersion(IEnumerable<long> versionIds, bool isApproved)
        {
            if (versionIds != null && versionIds.Count() > 0)
            {
                foreach (long item in versionIds)
                {
                    wikiService.ApprovePageVersion(item, isApproved);
                }
                return Json(new StatusMessageData(StatusMessageType.Success, "更新词条版本成功"));
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "请选择词条版本"));
        }
        #endregion

    }
}