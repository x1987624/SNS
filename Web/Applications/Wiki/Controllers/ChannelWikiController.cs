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
using System;
using System.Configuration;
using System.IO;
using System.Text;
using Tunynet.FileStore;

namespace Spacebuilder.Wiki.Controllers
{
    /// <summary>
    /// 百科控制器
    /// </summary>
    [TitleFilter(IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.Channel, IsApplication = true)]
    [AnonymousBrowseCheck]
    public partial class ChannelWikiController : Controller
    {
        public IPageResourceManager pageResourceManager { get; set; }
        public IUserService userService { get; set; }
        public WikiService wikiService { get; set; }
        public CategoryService categoryService { get; set; }
        public ISettingsManager<SiteSettings> siteSettingsManager { get; set; }
        private OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
        private TagService tagService = new TagService(TenantTypeIds.Instance().WikiPage());
        private AttachmentService<Attachment> attachmentService = new AttachmentService<Attachment>(TenantTypeIds.Instance().WikiPage());


        #region 创建、更新、浏览、删除

        /// <summary>
        /// 创建、编辑词条 - 页面
        /// </summary>
        /// <param name="pageId">百科Id</param>
        /// <param name="ownerId">拥有者Id</param>
        [HttpGet]
        public ActionResult EditPage(long? pageId, long? ownerId, long? questionId)
        {
            if (UserContext.CurrentUser == null)
                return Redirect(SiteUrls.Instance().Login(true));

            if (!pageId.HasValue)
                pageId = 0;

            //声明必要的对象
            WikiPage page = null;
            WikiPageEditModel pageEditModel = null;

            //输入的wikiId是否有效
            if (pageId.HasValue && pageId.Value > 0)//编辑词条
            {
                page = wikiService.Get(pageId.Value);
                if (page == null)
                    return HttpNotFound();
            }
            int ModuleSpeechCategoryId = Convert.ToInt32(ConfigurationManager.AppSettings["HaierSpeechModelFolderId"]);
            //百科分类
            IEnumerable<Category> siteCategories = categoryService.GetOwnerCategories(0, TenantTypeIds.Instance().WikiPage());

            ViewData["ownerCategories"] = siteCategories;

            if (pageId.HasValue && pageId.Value > 0)//编辑词条
            {
                pageResourceManager.InsertTitlePart("编辑词条");
                page = wikiService.Get(pageId.Value);
                pageEditModel = page.AsEditModel();
            }
            else//创建词条
            {
                pageResourceManager.InsertTitlePart("新建词条");
                pageEditModel = new WikiPageEditModel();
                if (ownerId.HasValue)
                    pageEditModel.OwnerId = ownerId.Value;
                if (questionId.HasValue && questionId.Value > 0)
                {
                    string title = string.Empty;
                    string body = string.Empty;
                    wikiService.GetAskTitleAndBody(questionId.Value, out title, out body);
                    pageEditModel.Title = title;
                    pageEditModel.Body = body;
                }


            }

            return View(pageEditModel);
        }

        /// <summary>
        /// 创建、编辑词条 - 处理页面
        /// </summary>
        /// <param name="editModel">要发布的词条实体</param>
        [HttpPost]
        public ActionResult EditPage(WikiPageEditModel editModel)
        {
            if (UserContext.CurrentUser == null)
                return Redirect(SiteUrls.Instance().Login(true));

            //声明必要的变量
            WikiPage page = null;
            WikiPageVersion pageVersion = null;

            #region 敏感词过滤

            string errorMessage = string.Empty;
            if (ModelState.HasBannedWord(out errorMessage))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Title = "发布失败",
                    Body = errorMessage,
                    StatusMessageType = StatusMessageType.Error
                }));
            }

            #endregion

            if (editModel.PageId < 1)//新建词条
            {
                page = editModel.AsWikiPage();
                bool valid = wikiService.IsExists(page.Title);
                if (valid)
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "发布同名词条失败",
                        Body = "发布失败，请稍后再试！",
                        StatusMessageType = StatusMessageType.Error
                    }));
                }

                bool isCreated = wikiService.Create(page, editModel.Body);

                if (page.PageId < 1)
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "发布失败",
                        Body = "发布失败，请稍后再试！",
                        StatusMessageType = StatusMessageType.Error
                    }));
                }

                pageVersion = page.LastestVersion;
            }
            else//编辑词条
            {
                //userFocusCategoryService.del
                page = wikiService.Get(editModel.PageId);
                if (page == null)
                    return HttpNotFound();

                //判断一下是否可以编辑
                bool IsCanEdit = !page.IsLocked;
                IUser user = UserContext.CurrentUser;
                if (page.OwnerId == user.UserId || DIContainer.Resolve<Authorizer>().IsAdministrator(ApplicationIds.Instance().Wiki()))
                {
                    IsCanEdit = true;
                }
                if (!IsCanEdit)
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "编辑失败",
                        Body = "编辑失败，该词条已经被锁定，请稍后再试！",
                        StatusMessageType = StatusMessageType.Error
                    }));
                }


                categoryService.DeleteItemInCategory(page.SiteCategory.CategoryId, page.PageId);

                page = editModel.AsWikiPage();

                wikiService.Update(page);

                pageVersion = editModel.AsWikiPageVersion();
                pageVersion.PageId = page.PageId;
                wikiService.CreatePageVersion(pageVersion);

                if (pageVersion.VersionId < 1)
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Title = "编辑失败",
                        Body = "编辑失败，请稍后再试！",
                        StatusMessageType = StatusMessageType.Error
                    }));
                }
            }

            #region 分类、标签

            if (editModel.SiteCategoryId > 0)
            {
                categoryService.AddCategoriesToItem(new List<long> { editModel.SiteCategoryId }, page.PageId);
            }
            string tags = string.Join(",", editModel.TagNames);
            if (!string.IsNullOrEmpty(tags))
            {
                tagService.ClearTagsFromItem(page.PageId, pageVersion.UserId);
                tagService.AddTagsToItem(tags, pageVersion.UserId, page.PageId);
            }

            #endregion


            return Redirect(SiteUrls.Instance().PageDetail(page.PageId));

        }

        /// <summary>
        /// 词条详细页
        /// </summary>
        /// <returns></returns>
        public ActionResult PageDetail(long pageId, long? versionId = null)
        {
            SiteSettings siteSettings = siteSettingsManager.Get();
            if (!siteSettings.EnableAnonymousBrowse)
            {
                //匿名用户访问
                if (UserContext.CurrentUser == null)
                {
                    string EncryptKey = System.Configuration.ConfigurationManager.AppSettings["EncryptKey"].ToString();
                    if (EncryptKey != "false")
                    {
                        string anonymous = Request.QueryString.Get("anonymous");
                        if (string.IsNullOrEmpty(anonymous))
                            return Redirect(SiteUrls.Instance().Login(true));
                    }
                }
            }
            WikiPage currentPage = wikiService.Get(pageId);

            //待审核的词条只有作者和管理员查看
            if (currentPage.AuditStatus != AuditStatus.Success) 
            {
                if (UserContext.CurrentUser == null)
                {
                    return Redirect(SiteUrls.Instance().Login(true));
                }
                else if (!DIContainer.Resolve<Authorizer>().Page_Manage(currentPage)&&UserContext.CurrentUser.UserId!=currentPage.UserId)
                {
                    return Redirect(SiteUrls.Instance().Index());
                }
            }

            //判断输入参数的有效性
            if (currentPage == null)
                return HttpNotFound();

            if (versionId.HasValue && versionId.Value > 0)
            {
                //设置标题
                pageResourceManager.InsertTitlePart("查看词条历史版本");
                ViewData["LastestVersion"] = wikiService.GetPageVersion(versionId.Value);
            }
            else
            {
                //设置标题
                pageResourceManager.InsertTitlePart("词条详细页");
                //浏览量
                CountService countService = new CountService(TenantTypeIds.Instance().WikiPage());
                countService.ChangeCount(CountTypes.Instance().HitTimes(), pageId, currentPage.UserId, 1, false);

                //获取数据在View展示
                //A 词条当前版本
                ViewData["LastestVersion"] = currentPage.LastestVersion;
            }
            //B 分类-最新版本的属性中有
            //C 标签-最新版本的属性中有
            //D 附件
            IEnumerable<Attachment> attachments = attachmentService.GetsByAssociateId(pageId);
            if (attachments != null && attachments.Count() > 0)
                ViewData["attachmentCount"] = attachments.Where(n => n.MediaType == MediaType.Other).Count();

            IEnumerable<WikiPage> hotPages = wikiService.GetTops(TenantTypeIds.Instance().Wiki(), 10, null, null, SortBy_WikiPage.StageHitTimes);
            ViewData["hotPages"] = hotPages;


            //增加计数
            CountService usercountService = new CountService(TenantTypeIds.Instance().User());
            return View(currentPage);
        }

        /// <summary>
        /// 获取词条编辑用户类表
        /// </summary>
        /// <param name="pageId">词条标识</param>
        /// <returns></returns>
        public ActionResult _GetPageEditor(long pageId, int topNumber = 6)
        {
            return View(wikiService.GetPageVersionsOfPage(pageId, topNumber));
        }

        /// <summary>
        /// 验证是否有
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult ValidateTitle(string title, long pageId)
        {
            string errorMessage = "已有同名词条";
            if (PageIdToTitleDictionary.GetPageId(title) == pageId)
                return Json(true, JsonRequestBehavior.AllowGet);
            bool valid = wikiService.IsExists(title);
            if (!valid)
                return Json(true, JsonRequestBehavior.AllowGet);
            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetPageCount(int type = 0)
        {
            PagingDataSet<WikiPage> pages = null;
            if (type == 0)//我创建的
            {
                pages = wikiService.GetOwnerPages(TenantTypeIds.Instance().Wiki(), UserContext.CurrentUser.UserId, true, string.Empty, 10, 1);
            }
            if (type == 1)//我编辑的
            {
                pages = wikiService.GetUserEditedPages(TenantTypeIds.Instance().Wiki(), UserContext.CurrentUser.UserId, true, 10, 1);
            }
            if (pages == null)
                return Json(0, JsonRequestBehavior.AllowGet);

            return Json(pages.TotalRecords, JsonRequestBehavior.AllowGet);
        }

        #endregion



        /// <summary>
        /// 百科首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Home()
        {
            ViewData["allCategories"] = categoryService.GetRootCategories(TenantTypeIds.Instance().WikiPage());
            pageResourceManager.InsertTitlePart("百科首页");
            return View();
        }

        #region _wikiInpublic

        /// <summary>
        /// 在首页的百科词条
        /// </summary>
        /// <returns></returns>
        public ActionResult _WikiInPublic()
        {
            IEnumerable<WikiPage> pages = wikiService.GetTops(TenantTypeIds.Instance().Wiki(), 10, null, null, SortBy_WikiPage.DateCreated_Desc);
            WikiPage wikipage = null;
            if (pages != null && pages.Count() > 0)
            {
                int i = 0;
                foreach (var item in pages)
                {
                    if (i == 0)
                        wikipage = item;
                    i++;
                    if (item.FeaturedImageAttachmentId > 0)
                    {
                        wikipage = item;
                        break;
                    }
                }
            }
            IEnumerable<Category> categories = categoryService.GetRootCategories(TenantTypeIds.Instance().WikiPage());
            if (categories != null && categories.Count() > 0)
            {
                if (categories.Count() > 6)
                {
                    ViewData["categories"] = categories.Take(6);
                }
                else
                {
                    ViewData["categories"] = categories;
                }
            }
            return View(wikipage);
        }
        #endregion

        /// <summary>
        /// 词条列表
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult _ListWikiPages(submenu menu = submenu.all, int pageSize = 10, int pageIndex = 1)
        {
            PagingDataSet<WikiPage> wikiPages = null;
            if (UserContext.CurrentUser != null)
            {
                if (menu == submenu.perfect)
                    wikiPages = wikiService.GetPerfectPages(TenantTypeIds.Instance().Wiki(), UserContext.CurrentUser.UserId, pageSize: pageSize, pageIndex: pageIndex);
                else if (menu == submenu.mycreated)
                    wikiPages = wikiService.GetOwnerPages(TenantTypeIds.Instance().Wiki(), UserContext.CurrentUser.UserId, true, string.Empty, pageSize: pageSize, pageIndex: pageIndex);
                else if (menu == submenu.myperfected)
                    wikiPages = wikiService.GetUserEditedPages(TenantTypeIds.Instance().Wiki(), UserContext.CurrentUser.UserId, true, pageSize: pageSize, pageIndex: pageIndex);
            }
            if (wikiPages == null)
                wikiPages = wikiService.Gets(TenantTypeIds.Instance().Wiki(), null, false, null, string.Empty, null, SortBy_WikiPage.DateCreated_Desc, pageSize, pageIndex);
            return View(wikiPages);
        }

        /// <summary>
        /// 词条列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Pages(long categoryId, int pageSize, int pageIndex)
        {
            pageResourceManager.InsertTitlePart("词条列表");
            Category category = categoryService.Get(categoryId);
            List<Category> allParentCategories = new List<Category>();
            if (category.Parent != null)
                RecursiveGetAllParentCategories(category.Parent, ref allParentCategories);
            ViewData["category"] = category;
            ViewData["allParentCategories"] = allParentCategories;
            PagingDataSet<WikiPage> wikiPages = wikiService.Gets(TenantTypeIds.Instance().Wiki(), null, false, categoryId, string.Empty, null, SortBy_WikiPage.DateCreated_Desc, pageSize, pageIndex);
            if (category != null)
            {
                TempData["wikiCategoryId"] = category.CategoryId;
            }

            return View(wikiPages);
        }

        /// <summary>
        /// 递归获取所有的父分类的集合（包含当前分类）
        /// </summary>
        /// <param name="category">当前类别</param>
        /// <param name="allParentCategories">所有的父类别</param>
        private void RecursiveGetAllParentCategories(Category category, ref List<Category> allParentCategories)
        {
            if (category == null)
                return;
            allParentCategories.Insert(0, category);
            Category parent = category.Parent;
            if (parent != null)
                RecursiveGetAllParentCategories(parent, ref allParentCategories);
        }

        /// <summary>
        /// 子类别
        /// </summary>
        /// <returns></returns>
        public ActionResult _ChildCategory(long categoryId, bool? sameLevel)
        {

            IEnumerable<Category> categorys = new List<Category>();
            if (sameLevel.HasValue && sameLevel.Value)
            {
                Category category = categoryService.Get(categoryId);
                if (category.Depth == 0)
                {
                    categorys = categoryService.GetRootCategories(TenantTypeIds.Instance().WikiPage());
                }
                else if (category.Parent != null && category.Parent.Children != null)
                {
                    categorys = category.Parent.Children;
                }
            }
            else
            {
                categorys = categoryService.GetChildren(categoryId);
            }

            ViewData["sameLevel"] = sameLevel;
            return View(categorys);
        }

        /// <summary>
        /// 根据标签显示词条列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Pages_tag(string tagName, int? pageSize, int? pageIndex)
        {
            pageResourceManager.InsertTitlePart("词条列表");
            PagingDataSet<WikiPage> wikiPages = wikiService.Gets(TenantTypeIds.Instance().Wiki(), null, true, null, tagName, null, SortBy_WikiPage.DateCreated_Desc, pageSize ?? 10, pageIndex ?? 1);
            ViewData["tagName"] = tagName;
            TempData["wikiTagName"] = tagName;
            return View(wikiPages);
        }


        /// <summary>
        /// 热门词条
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public ActionResult _HotPages(long categoryId)
        {
            IEnumerable<WikiPage> wikiPages = wikiService.Gets(TenantTypeIds.Instance().Wiki(), null, true, categoryId, string.Empty, null, SortBy_WikiPage.StageHitTimes);
            return View(wikiPages);
        }
        /// <summary>
        /// 根据标签获取热门词条
        /// </summary>
        public ActionResult _HotPages_tag(string tagName)
        {
            IEnumerable<WikiPage> wikiPages = wikiService.Gets(TenantTypeIds.Instance().Wiki(), null, true, null, tagName, null, SortBy_WikiPage.StageHitTimes);
            return View("_HotPages", wikiPages.Distinct());
        }
        /// <summary>
        /// 标签详细页
        /// </summary>
        /// <returns></returns>
        public ActionResult TagDetail(string tagName)
        {
            var tag = new TagService(TenantTypeIds.Instance().WikiPage()).Get(tagName);

            if (tag == null)
            {
                return HttpNotFound();
            }

            //设置title和meta
            pageResourceManager.InsertTitlePart(tag.TagName);
            if (!string.IsNullOrEmpty(tag.Description))
            {
                pageResourceManager.SetMetaOfDescription(HtmlUtility.TrimHtml(tag.Description, 100));
            }
            return View(tag);
        }

        /// <summary>
        /// 用户的百科
        /// </summary>
        /// <returns></returns>
        public ActionResult WikiUser(string spaceKey)
        {
            IUser user = null;
            if (string.IsNullOrEmpty(spaceKey))
            {
                user = UserContext.CurrentUser;
                if (user == null)
                {
                    return Redirect(SiteUrls.Instance().Login(true));
                }
                pageResourceManager.InsertTitlePart("我的百科");
            }
            else
            {
                user = userService.GetFullUser(spaceKey);
                if (user == null)
                {
                    return HttpNotFound();
                }

                if (!new PrivacyService().Validate(user.UserId, UserContext.CurrentUser != null ? UserContext.CurrentUser.UserId : 0, PrivacyItemKeys.Instance().VisitUserSpace()))
                {
                    if (UserContext.CurrentUser == null)
                        return Redirect(SiteUrls.Instance().Login(true));
                    else
                        return Redirect(SiteUrls.Instance().PrivacyHome(user.UserName));
                }

                if (UserContext.CurrentUser != null && user.UserId == UserContext.CurrentUser.UserId)
                {
                    pageResourceManager.InsertTitlePart("我的百科");
                }
                else
                {
                    pageResourceManager.InsertTitlePart(user.DisplayName + "的百科");
                }
            }
            return View(user);
        }

        /// <summary>
        /// 用户首页显示的贡献词条数
        /// </summary>
        /// <returns></returns>
        public ActionResult _AllWikisCount()
        {
            long allWikiCount = new WikiService().Gets(TenantTypeIds.Instance().Wiki(), null, true, null, string.Empty, null).TotalRecords;

            if (UserContext.CurrentUser != null)
            {
                ViewData["myEditCount"] = wikiService.GetOwnerPages(TenantTypeIds.Instance().Wiki(), UserContext.CurrentUser.UserId, true, string.Empty, 10, 1).TotalRecords;
            }
            return View(allWikiCount);
        }


        /// <summary>
        /// 词条版本比较
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ShowPageVersionDifferent(IEnumerable<long> VersionId)
        {
            bool IsSpeech = Request.Form.Get<bool>("IsSpeech", false);
            ViewData["IsSpeech"] = IsSpeech;
            if (VersionId != null && VersionId.Count() == 2)
            {
                WikiPageVersion v1 = wikiService.GetPageVersion(VersionId.ToList()[1]);
                WikiPageVersion v2 = wikiService.GetPageVersion(VersionId.ToList()[0]);
                string str1 = v1.ResolvedBody;
                string str2 = v2.ResolvedBody;
                //IEnumerable<int> versionnums = Request.Form.Get<IEnumerable<int>>("versionnum");
                string nums = Request.Form.Get<string>("saveNums");
                List<string> _nums = nums.Split(',').ToList();
                if (_nums != null && _nums.Count() >= 2)
                {
                    v1.VersionNum = int.Parse(_nums[1]);
                    v2.VersionNum = int.Parse(_nums[0]);
                }
                HtmlDiff diffHelper = new HtmlDiff(v1.ResolvedBody, v2.ResolvedBody);
                ViewData["diffString"] = diffHelper.Build();
                ViewData["WikiPageVersion1"] = v1;
                ViewData["WikiPageVersion2"] = v2;
                pageResourceManager.InsertTitlePart("词条历史版本比较");
                return View();
            }
            return View();
        }

        #region 百科全文检索
        /// <summary>
        /// 百科搜索
        /// </summary>
        public ActionResult Search(WikiFullTextQuery query)
        {
            query.Keyword = WebUtility.UrlDecode(query.Keyword);
            query.PageSize = 20;//每页记录数

            //调用搜索器进行搜索
            WikiSearcher wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
            PagingDataSet<WikiPage> wikiPages = wikiSearcher.Search(query);

            //添加到用户搜索历史 
            IUser CurrentUser = UserContext.CurrentUser;
            if (CurrentUser != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Keyword))
                {
                    SearchHistoryService searchHistoryService = new SearchHistoryService();
                    searchHistoryService.SearchTerm(CurrentUser.UserId, WikiSearcher.CODE, query.Keyword);
                }
            }

            //添加到热词
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                SearchedTermService searchedTermService = new SearchedTermService();
                searchedTermService.SearchTerm(WikiSearcher.CODE, query.Keyword);
            }

            //获取站点分类，并设置站点分类的选中项
            IEnumerable<Category> siteCategories = categoryService.GetRootCategories(TenantTypeIds.Instance().WikiPage());// categoryService.GetOwnerCategories(0, TenantTypeIds.Instance().Wiki());
            SelectList siteCategoryList = new SelectList(siteCategories.Select(n => new { text = n.CategoryName, value = n.CategoryId }), "value", "text", query.SiteCategoryId);
            ViewData["siteCategoryList"] = siteCategoryList;

            //设置页面Meta
            if (string.IsNullOrWhiteSpace(query.Keyword))
            {
                pageResourceManager.InsertTitlePart("百科搜索");//设置页面Title
            }
            else
            {
                pageResourceManager.InsertTitlePart(query.Keyword + "的相关百科");//设置页面Title
            }

            return View(wikiPages);
        }

        /// <summary>
        /// 百科全局搜索
        /// </summary>
        public ActionResult _GlobalSearch(WikiFullTextQuery query, int topNumber)
        {
            query.PageSize = topNumber;//每页记录数
            query.PageIndex = 1;

            //调用搜索器进行搜索
            WikiSearcher wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
            PagingDataSet<WikiPage> wikiPages = wikiSearcher.Search(query);

            return PartialView(wikiPages);
        }




        /// <summary>
        /// 百科快捷搜索
        /// </summary>
        public ActionResult _QuickSearch(WikiFullTextQuery query, int topNumber)
        {
            query.PageSize = topNumber;//每页记录数
            query.PageIndex = 1;
            query.Range = WikiSearchRange.Title;
            query.Keyword = Server.UrlDecode(query.Keyword);

            //调用搜索器进行搜索
            WikiSearcher wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
            PagingDataSet<WikiPage> blogThreads = wikiSearcher.Search(query);

            return PartialView(blogThreads);
        }

        /// <summary>
        /// 百科搜索自动完成
        /// </summary>
        public JsonResult SearchAutoComplete(string keyword, int topNumber)
        {
            //调用搜索器进行搜索
            WikiSearcher wikiSearcher = (WikiSearcher)SearcherFactory.GetSearcher(WikiSearcher.CODE);
            IEnumerable<string> terms = wikiSearcher.AutoCompleteSearch(keyword, topNumber);

            var jsonResult = Json(terms.Select(t => new { tagName = t, tagNameWithHighlight = SearchEngine.Highlight(keyword, string.Join("", t.Take(34)), 100) }), JsonRequestBehavior.AllowGet);
            return jsonResult;
        }
        #endregion

        #region 历史版本

        /// <summary>
        /// 历史版本
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult HistoryVersion(long pageId, bool IsSpeech = false, int pageIndex = 1)
        {


            WikiPage wikiPage = wikiService.Get(pageId);
            if (wikiPage == null)
                return HttpNotFound();
            ViewData["WikiPage"] = wikiPage;
            PagingDataSet<WikiPageVersion> pds = wikiService.GetPageVersionsOfPage(pageId, 20, pageIndex);
            pageResourceManager.InsertTitlePart("词条历史版本");
            ViewData["IsSpeech"] = IsSpeech;
            return View(pds);
        }

        /// <summary>
        /// 回滚到历史版本
        /// </summary>
        public ActionResult RollbackPageVersion(long versionId, int versionNum)
        {

            WikiPageVersion wikiPageVersion = wikiService.GetPageVersion(versionId);
            if (wikiPageVersion == null)
                return HttpNotFound();
            WikiPage wikiPage = wikiService.Get(wikiPageVersion.PageId);
            if (wikiPage == null)
                return HttpNotFound();
            if (!DIContainer.Resolve<Authorizer>().Page_Manage(wikiPage))
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    StatusMessageType = StatusMessageType.Hint,
                    Title = "没有权限",
                    Body = "没有回滚词条的权限"
                }));
            wikiService.RollbackPageVersion(versionId, versionNum);
            return Redirect(SiteUrls.Instance().HistoryVersion(wikiPage.PageId));
        }

        /// <summary>
        /// 词条幻灯片
        /// </summary>
        /// <returns></returns>
        public ActionResult _WikiSlide(string recommendTypeId = null, int topNumber = 6)
        {
            IEnumerable<RecommendItem> recommendItems = new RecommendService().GetTops(topNumber, recommendTypeId);
            ViewData["recommendTypeId"] = recommendTypeId;
            return View(recommendItems);
        }

        /// <summary>
        /// 增加词条列表
        /// </summary>
        /// <returns></returns>
        public ActionResult _WikiInIndex()
        {

            return View();
        }

        /// <summary>
        /// 增加用于首页的词条列表
        /// </summary>
        /// <returns></returns>
        public ActionResult _WikiIndex(int pageSize, int pageIndex, SortBy_WikiPage sortBy_WikiPage)
        {
            PagingDataSet<WikiPage> wikiPages = wikiService.Gets(TenantTypeIds.Instance().Wiki(), null, false, null, string.Empty, null, sortBy_WikiPage, pageSize, pageIndex);

            return View(wikiPages);
        }
        #endregion


        /// <summary>
        /// 增加词条
        /// </summary>
        /// <returns></returns>
        public ActionResult _WikiTitle()
        {
            IEnumerable<WikiPage> wikiPages = wikiService.Gets(TenantTypeIds.Instance().Wiki(), null, true, null, string.Empty, null, SortBy_WikiPage.StageHitTimes);
            return View(wikiPages);
        }

        /// <summary>
        /// 词条分类
        /// </summary>
        /// <returns></returns>
        public ActionResult _WikiClassify()
        {
            int speechModuleId = Convert.ToInt32(ConfigurationManager.AppSettings["HaierSpeechModelFolderId"]);
            List<Category> returnWikiCategories = new List<Category>();
            List<Category> wikiCategories = categoryService.GetRootCategories(TenantTypeIds.Instance().WikiPage()).ToList();
            foreach (var item in wikiCategories)
            {
                if (item.CategoryId == speechModuleId)
                {
                    continue;
                }
                returnWikiCategories.Add(item);
            }
            ViewData["allCategories"] = returnWikiCategories;
            return View();
        }

        /// <summary>
        /// 根据百科分类Id
        /// </summary>
        /// <param name="categoryId">百科分类Id</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult _CategoryDetail(long categoryId, int pageSize = 10, int pageIndex = 1)
        {
            IUser user = null;

            user = UserContext.CurrentUser;
            if (user == null)
            {
                return Redirect(SiteUrls.Instance().Login(true));
            }
            else
            {
                PagingDataSet<WikiPage> wikiPages = wikiService.Gets(TenantTypeIds.Instance().Wiki(), null, false, categoryId, string.Empty, null, SortBy_WikiPage.DateCreated_Desc, pageSize, pageIndex);

                return View(wikiPages);
            }

        }


        /// <summary>
        /// 百科首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            SiteSettings siteSettings = siteSettingsManager.Get();
            pageResourceManager.InsertTitlePart(siteSettings.SiteDescription);
            pageResourceManager.InsertTitlePart("百科");
            pageResourceManager.SetMetaOfDescription(siteSettings.SearchMetaDescription);
            pageResourceManager.SetMetaOfKeywords(siteSettings.SearchMetaKeyWords);


            return View();
        }

        public ActionResult _SideNavigation()
        {
            return View();
        }
    }

    /// <summary>
    /// 词条菜单
    /// </summary>
    public enum submenu
    {
        /// <summary>
        /// 需要我来完善的词条
        /// </summary>
        perfect,
        /// <summary>
        /// 全部词条
        /// </summary>
        all,
        /// <summary>
        /// 我创建的词条
        /// </summary>
        mycreated,
        /// <summary>
        /// 我完善过的词条
        /// </summary>
        myperfected
    }
}