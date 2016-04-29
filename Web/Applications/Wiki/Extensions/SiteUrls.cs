//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Web.Routing;
using Spacebuilder.Wiki;
using Tunynet.Common;

using Tunynet.Utilities;
using System.Collections.Generic;
using Spacebuilder.Common;
using Spacebuilder.Wiki.Controllers;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 问答链接管理
    /// </summary>
    public static class SiteUrlsExtension
    {
        private static readonly string WikiAreaName = WikiConfig.Instance().ApplicationKey;

        #region 频道百科

        /// <summary>
        /// 根据标签显示词条列表
        /// </summary>
        public static string WikiTagDetail(this SiteUrls siteUrls, string tagName)
        {
            return CachedUrlHelper.Action("Pages_tag", "ChannelWiki", WikiAreaName, new RouteValueDictionary { { "tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')) } });
        }

        /// <summary>
        /// 词条编辑
        /// </summary>
        public static string PageEdit(this SiteUrls siteUrls, long? pageId)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (pageId.HasValue && pageId.Value > 0)
                routeValueDictionary.Add("pageId", pageId);
            return CachedUrlHelper.Action("EditPage", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }


        /// <summary>
        /// 词条列表
        /// </summary>
        public static string _ListWikiPages(this SiteUrls siteUrls, submenu menu = submenu.all, int pageSize = 20, int pageIndex = 1)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("menu", menu);
            routeValueDictionary.Add("pageSize", pageSize);
            routeValueDictionary.Add("pageIndex", pageIndex);
            return CachedUrlHelper.Action("_ListWikiPages", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 词条详细显示页
        /// </summary>
        public static string PageDetail(this SiteUrls siteUrls, long pageId, long? versionId = null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageId", pageId);
            if (versionId.HasValue && versionId.Value > 0)
                routeValueDictionary.Add("versionId", versionId);
            return CachedUrlHelper.Action("PageDetail", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 词条历史版本显示页
        /// </summary>
        public static string HistoryVersion(this SiteUrls siteUrls, long pageId, bool IsSpeech = false)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageId", pageId);
            routeValueDictionary.Add("IsSpeech", IsSpeech);
            return CachedUrlHelper.Action("HistoryVersion", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 词条历史版本显示页
        /// </summary>
        public static string RollbackPageVersion(this SiteUrls siteUrls, long versionId, int versionNum)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("versionId", versionId);
            routeValueDictionary.Add("versionNum", versionNum);
            return CachedUrlHelper.Action("RollbackPageVersion", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }


        /// <summary>
        /// 我的百科
        /// </summary>
        public static string WikiUser(this SiteUrls siteUrls, string spaceKey)
        {
            return CachedUrlHelper.Action("WikiUser", "ChannelWiki", WikiAreaName, new RouteValueDictionary { { "spaceKey", spaceKey } });
        }

        /// <summary>
        /// 词条列表
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="categoryId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static string Pages(this SiteUrls siteUrls, long categoryId, int pageSize = 10, int pageIndex = 1)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("categoryId", categoryId);
            routeValueDictionary.Add("pageSize", pageSize);
            routeValueDictionary.Add("pageIndex", pageIndex);
            return CachedUrlHelper.Action("Pages", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }

        public static string _WikiIndex(this SiteUrls siteUrls, int pageSize, int pageIndex, SortBy_WikiPage sortBy_WikiPage)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            routeValueDictionary.Add("pageSize", pageSize);
            routeValueDictionary.Add("pageIndex", pageIndex);
            routeValueDictionary.Add("sortBy_WikiPage", sortBy_WikiPage);
            return CachedUrlHelper.Action("_WikiIndex", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }

        #endregion

        #region 全文检索
        /// <summary>
        /// 百科全局搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string WikiGlobalSearch(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_GlobalSearch", "ChannelWiki", WikiAreaName);
        }

        /// <summary>
        /// 日志快捷搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string WikiQuickSearch(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_QuickSearch", "ChannelWiki", WikiAreaName);
        }

        /// <summary>
        /// 百科搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string WikiPageSearch(this SiteUrls siteUrls, string keyword = "")
        {
            keyword = WebUtility.UrlEncode(keyword);
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(keyword))
            {
                dic.Add("keyword", keyword);
            }
            return CachedUrlHelper.Action("Search", "ChannelWiki", WikiAreaName, dic);
        }

        /// <summary>
        /// 百科搜索自动完成
        /// </summary>
        public static string WikiSearchAutoComplete(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("SearchAutoComplete", "ChannelWiki", WikiAreaName);
        }
        #endregion

        #region 后台百科
        /// <summary>
        /// 词条后台管理
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="auditStatus">审核状态</param>
        public static string WikiPageControlPanelManage(this SiteUrls siteUrls, AuditStatus? auditStatus = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (auditStatus.HasValue)
            {
                dic.Add("auditStatus", auditStatus);
            }
            return CachedUrlHelper.Action("ManagePages", "ControlPanelWiki", WikiAreaName, dic);
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public static string _ApproveWikiPage(this SiteUrls siteUrls, bool isApproved)
        {
            return CachedUrlHelper.Action("_ApproveWikiPage", "ControlPanelWiki", WikiAreaName, new RouteValueDictionary { { "isApproved", isApproved } });
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public static string _ApproveWikiPage_ForCategory(this SiteUrls siteUrls, bool isApproved)
        {
            return CachedUrlHelper.Action("_ApproveWikiPage", "ChannelWiki", WikiAreaName, new RouteValueDictionary { { "isApproved", isApproved } });
        }
        #region haiersns-2-liucg-20130716-锁定词条
        /// <summary>
        /// 锁定词条
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public static string _WikiPageSetLock(this SiteUrls siteUrls, bool isLocked)
        {
            return CachedUrlHelper.Action("_WikiPageSetLock", "ControlPanelWiki", WikiAreaName, new RouteValueDictionary { { "isLocked", isLocked } });
        }

        /// <summary>
        /// 锁定词条
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public static string ShowPageVersionDifferent(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("ShowPageVersionDifferent", "ChannelWiki", WikiAreaName, new RouteValueDictionary { });
        }

        #endregion

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _DeleteWikiPage(this SiteUrls siteUrls, long? pageIds = null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (pageIds.HasValue)
            {
                routeValueDictionary.Add("pageIds", pageIds);
            }
            return CachedUrlHelper.Action("_DeleteWikiPage", "ControlPanelWiki", WikiAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _DeleteWikiPage_ForCategory(this SiteUrls siteUrls, long? pageIds = null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (pageIds.HasValue)
            {
                routeValueDictionary.Add("pageIds", pageIds);
            }
            return CachedUrlHelper.Action("_DeleteWikiPage", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 设置分类
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _SetCategoryWiki(this SiteUrls siteUrls, string pageIds = null)
        {
            if (string.IsNullOrEmpty(pageIds))
                return CachedUrlHelper.Action("_SetCategory", "ControlPanelWiki", WikiAreaName);
            else
                return CachedUrlHelper.Action("_SetCategory", "ControlPanelWiki", WikiAreaName, new RouteValueDictionary { { "pageIds", pageIds } });
        }

        /// <summary>
        /// 管理词条版本
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string ManageVersion(this SiteUrls siteUrls,AuditStatus? auditStatus=null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (auditStatus != null)
                routeValueDictionary.Add("auditStatus", auditStatus);

            return CachedUrlHelper.Action("ManageVersions", "ControlPanelWiki", WikiAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 删除词条版本
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public static string _DeleteWikiPageVersions(this SiteUrls siteUrls, long? versionIds = null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (versionIds.HasValue)
            {
                routeValueDictionary.Add("versionIds", versionIds);
            }
            return CachedUrlHelper.Action("_DeleteWikiPageVersions", "ControlPanelWiki", WikiAreaName, routeValueDictionary);
        }

        /// <summary>
        /// 删除词条版本
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public static string _DeleteWikiPageVersions_ForCategory(this SiteUrls siteUrls, long? versionIds = null)
        {
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary();
            if (versionIds.HasValue)
            {
                routeValueDictionary.Add("versionIds", versionIds);
            }
            return CachedUrlHelper.Action("_DeleteWikiPageVersions", "ChannelWiki", WikiAreaName, routeValueDictionary);
        }


        /// <summary>
        /// 审核词条版本
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public static string _ApproveWikiPageVersion(this SiteUrls siteUrls, bool isApproved)
        {
            return CachedUrlHelper.Action("_ApproveWikiPageVersion", "ControlPanelWiki", WikiAreaName, new RouteValueDictionary { { "isApproved", isApproved } });
        }
        /// <summary>
        /// 审核词条版本
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public static string _ApproveWikiPageVersion_ForCategory(this SiteUrls siteUrls, bool isApproved)
        {
            return CachedUrlHelper.Action("_ApproveWikiPageVersion", "ChannelWiki", WikiAreaName, new RouteValueDictionary { { "isApproved", isApproved } });
        }

        #endregion

        /// <summary>
        /// 根据百科分类显示百科列表
        /// </summary>
        /// <returns></returns>
        public static string _CategoryDetail(this SiteUrls siteUrls, long categoryId, int pageSize = 20, int pageIndex = 1)
        {
            return CachedUrlHelper.Action("_CategoryDetail", "ChannelWiki", WikiAreaName, new RouteValueDictionary { { "categoryId", categoryId }, { "pageSize", pageSize }, { "pageIndex", pageIndex } });
        }


        /// <summary>
        /// 百科首页
        /// </summary>
        /// <returns></returns>
        public static string Index(this SiteUrls siteUrls, int pageSize = 20, int pageIndex = 1)
        {
            return CachedUrlHelper.Action("Index", "ChannelWiki", WikiAreaName);
        }
    }
}
