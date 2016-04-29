//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Web.Mvc;
using Tunynet.UI;
using Tunynet;
using Tunynet.Common;
using System.Collections.Generic;
using Tunynet.Utilities;
using System.Linq;
using Spacebuilder.Common;

using Spacebuilder.Group;
using Spacebuilder.Bar.Search;
using Spacebuilder.Search;
using Tunynet.Search;
using DevTrends.MvcDonutCaching;

namespace Spacebuilder.Bar.Controllers
{
    /// <summary>
    /// Ⱥ�����ɹ���
    /// </summary>
    [TitleFilter(IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.GroupSpace, IsApplication = true)]
    [AnonymousBrowseCheck]
    [GroupSpaceAuthorize]
    public class GroupSpaceBarController : Controller
    {
        public Authorizer authorizer { get; set; }
        public IPageResourceManager pageResourceManager { get; set; }
        public BarThreadService barThreadService { get; set; }
        public BarSectionService barSectionService { get; set; }
        public CategoryService categoryService { get; set; }
        public BarPostService barPostService { get; set; }
        public IUserService userService { get; set; }
        public GroupService groupService { get; set; }
        private TagService tagService = new TagService(TenantTypeIds.Instance().Group());
        private BarSettings barSettings = DIContainer.Resolve<ISettingsManager<BarSettings>>().Get();


        #region ��ϸ��ʾ
        /// <summary>
        /// ������ϸ��ʾҳ��
        /// </summary>
        /// <param name="threadId">����id</param>
        /// <param name="pageIndex">����ҳ��</param>
        /// <param name="onlyLandlord">ֻ��¥��</param>
        /// <param name="sortBy">����ʽ</param>
        /// <returns>������ϸ��ʾҳ��</returns>
        [HttpGet]
        public ActionResult Detail(string spaceKey, long threadId, int pageIndex = 1, bool onlyLandlord = false, SortBy_BarPost sortBy = SortBy_BarPost.DateCreated, long? postId = null, long? childPostIndex = null)
        {
            BarThread barThread = barThreadService.Get(threadId);
            if (barThread == null)
                return HttpNotFound();

            GroupEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            BarSection section = barSectionService.Get(barThread.SectionId);
            if (section == null || section.TenantTypeId != TenantTypeIds.Instance().Group())
                return HttpNotFound();

            //��֤�Ƿ�ͨ�����
            long currentSpaceUserId = UserIdToUserNameDictionary.GetUserId(spaceKey);
            if (!authorizer.IsAdministrator(BarConfig.Instance().ApplicationId) && barThread.UserId != currentSpaceUserId
                && (int)barThread.AuditStatus < (int)(new AuditService().GetPubliclyAuditStatus(BarConfig.Instance().ApplicationId)))
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Title = "��δͨ�����",
                    Body = "���ڵ�ǰ������δͨ����ˣ����޷������ǰ���ݡ�",
                    StatusMessageType = StatusMessageType.Hint
                }));


            pageResourceManager.InsertTitlePart(section.Name);
            pageResourceManager.InsertTitlePart(barThread.Subject);

            Category category = categoryService.Get(barThread.CategoryId ?? 0);
            string keyWords = string.Join(",", barThread.TagNames);

            pageResourceManager.SetMetaOfKeywords(category != null ? category.CategoryName + "," + keyWords : keyWords);//����Keyords���͵�Meta
            pageResourceManager.SetMetaOfDescription(HtmlUtility.TrimHtml(barThread.GetResolvedBody(), 120));//����Description���͵�Meta

            ViewData["EnableRating"] = barSettings.EnableRating;

            //�����������
            CountService countService = new CountService(TenantTypeIds.Instance().BarThread());
            countService.ChangeCount(CountTypes.Instance().HitTimes(), barThread.ThreadId, barThread.UserId, 1, false);

            PagingDataSet<BarPost> barPosts = barPostService.Gets(threadId, onlyLandlord, sortBy, pageIndex);
            if (pageIndex > barPosts.PageCount && pageIndex > 1)
                return Detail(spaceKey, threadId, barPosts.PageCount);
            if (Request.IsAjaxRequest())
                return PartialView("~/Applications/Bar/Views/Bar/_ListPost.cshtml", barPosts);

            ViewData["barThread"] = barThread;
            ViewData["group"] = group;
            return View(barPosts);
        }

        /// <summary>
        /// ������ϸ��ʾҳ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SectionDetail(string spaceKey, long? categoryId = null, bool? isEssential = null, SortBy_BarThread? sortBy = null, bool? isPosted = null, int pageIndex = 1)
        {
            IUser currentUser = UserContext.CurrentUser;
            long sectionId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);
            BarSection barSection = barSectionService.Get(sectionId);
            if (barSection == null)
                return HttpNotFound();

            if (barSection.AuditStatus != AuditStatus.Success && !authorizer.BarSection_Manage(barSection))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Title = "û��Ȩ��",
                    Body = "��Ⱥ�黹δͨ����ˣ����ܲ鿴",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }

            PagingDataSet<BarThread> pds = barThreadService.Gets(sectionId, categoryId, isEssential, sortBy ?? SortBy_BarThread.LastModified_Desc, pageIndex);
            if (Request.IsAjaxRequest())
                return PartialView("~/Applications/Bar/Views/Bar/_ListInGroup.cshtml", pds);
            pageResourceManager.InsertTitlePart(barSection.Name);
            Category currentThreadCategory = null;
            if (categoryId.HasValue && categoryId.Value > 0)
                currentThreadCategory = categoryService.Get(categoryId.Value);
            if (currentThreadCategory != null)
            {
                ViewData["currentThreadCategory"] = currentThreadCategory;
            }

            //����ǰ�û���������Լ��������б������ǹ���Ա���������ˣ�
            bool ignoreAudit = currentUser != null && UserContext.CurrentUser.UserId == currentUser.UserId || authorizer.IsAdministrator(BarConfig.Instance().ApplicationId);
            if (isPosted.HasValue)
            {
                pds = barThreadService.GetUserThreads(TenantTypeIds.Instance().Group(), currentUser.UserId, ignoreAudit, isPosted.Value, pageIndex, sectionId);
            }

            ViewData["section"] = barSection;
            ViewData["threadCategories"] = categoryService.GetOwnerCategories(sectionId, TenantTypeIds.Instance().BarThread());
            ViewData["sortBy"] = sortBy;
            return View(pds);
        }
        #endregion

        #region �༭�ķ���
        /// <summary>
        /// �༭����
        /// </summary>
        /// <param name="spaceKey">Ⱥ����</param>
        /// <param name="threadId">����id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(string spaceKey, long? threadId)
        {
            long sectionId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);
            BarSection section = barSectionService.Get(sectionId);
            if (UserContext.CurrentUser == null)
                return Redirect(SiteUrls.Instance().Login(true));

            if (section == null)
                return HttpNotFound();

            GroupEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();

            pageResourceManager.InsertTitlePart(section.Name);

            BarThread barThread = threadId.HasValue ? barThreadService.Get(threadId ?? 0) : null;
            if (threadId.HasValue)
            {
                if (!authorizer.BarThread_Edit(barThread))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "û��Ȩ�ޱ༭" + barThread.Subject + "��",
                        Title = "û��Ȩ��",
                        StatusMessageType = StatusMessageType.Hint
                    }));
                }
            }
            else
            {
                string errorMessage = string.Empty;
                if (!authorizer.BarThread_Create(sectionId, out errorMessage))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = errorMessage,
                        Title = "û��Ȩ��",
                        StatusMessageType = StatusMessageType.Hint
                    }, Request.RawUrl));
                }
            }
            pageResourceManager.InsertTitlePart(threadId.HasValue ? "�༭����" : "����");
            if (threadId.HasValue && barThread == null)
                return HttpNotFound();

            ViewData["barSettings"] = barSettings;

            ViewData["group"] = group;
            ViewData["BarSection"] = section;
            return View("Edit", barThread == null ? new BarThreadEditModel { SectionId = sectionId } : barThread.AsEditModel());
        }

        /// <summary>
        /// �༭����ҳ��
        /// </summary>
        /// <param name="spaceKey">Ⱥ����</param>
        /// <param name="postId">����id</param>
        /// <returns>�༭����ҳ��</returns>
        [HttpGet]
        public ActionResult EditPost(long threadId, long? postId)
        {
            BarThread thread = barThreadService.Get(threadId);
            if (thread == null)
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "û���ҵ���Ҫ�༭�Ļ���",
                    Title = "û���ҵ�����",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            pageResourceManager.InsertTitlePart(thread.BarSection.Name);

            BarPost post = null;
            if (postId.HasValue && postId.Value > 0)
            {
                post = barPostService.Get(postId ?? 0);
                if (!authorizer.BarPost_Edit(post))
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "��û��Ȩ�ޱ༭�˻���",
                        Title = "û��Ȩ��"
                    }));
                }
                if (post == null)
                {
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "û���ҵ���Ҫ�༭�Ļ���",
                        Title = "û���ҵ�����"
                    }));
                }
                pageResourceManager.InsertTitlePart("�༭����");
            }
            else
            {
                string errorMessage = string.Empty;
                if (!authorizer.BarPost_Create(thread.SectionId, out errorMessage))
                {
                    if (UserContext.CurrentUser == null)
                        return Redirect(SiteUrls.Instance().Login(true));

                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = errorMessage,
                        Title = "û��Ȩ��"
                    }));
                }
                pageResourceManager.InsertTitlePart("�������");
            }

            BarPostEditModel postModel = null;
            if (post != null)
                postModel = post.AsEditModel();
            else
            {
                postModel = new BarPostEditModel
                  {
                      ThreadId = threadId,
                      PostId = postId,
                      Subject = thread.Subject
                  };
                string body = Request.QueryString.Get<string>("MultilineBody", null);
                if (!string.IsNullOrEmpty(body))
                    postModel.Body = body = new EmotionService().EmoticonTransforms(body);
            }
            ViewData["PostBodyMaxLength"] = barSettings.PostBodyMaxLength;

            postModel.SectionId = thread.SectionId;
            return View(postModel);
        }
        #endregion

        #region �б�ҳ��
        /// <summary>
        /// �û������б�
        /// </summary>
        /// <param name="userName">�û���</param>
        /// <param name="isPosted">�Ƿ��ǻ���</param>
        /// <param name="pageIndex">ҳ��</param>
        /// <param name="spaceKey">Ⱥ����</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UserThreads(string spaceKey, bool isPosted = false, int pageIndex = 1)
        {
            IUser currentUser = UserContext.CurrentUser;
            long sectionId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);

            //����ǰ�û���������Լ��������б������ǹ���Ա���������ˣ�
            bool ignoreAudit = currentUser != null && UserContext.CurrentUser.UserId == currentUser.UserId || authorizer.IsAdministrator(BarConfig.Instance().ApplicationId);
            PagingDataSet<BarThread> pds = barThreadService.GetUserThreads(TenantTypeIds.Instance().Group(), currentUser.UserId, ignoreAudit, isPosted, pageIndex, sectionId);
            if (Request.IsAjaxRequest())
                return PartialView("~/Applications/Bar/Views/Bar/_ListInGroup.cshtml", pds);
            var group = groupService.Get(spaceKey);
            pageResourceManager.InsertTitlePart(group.GroupName);
            string title = isPosted ? "�ҵĻ���" : "�ҵ�����";
            if (UserContext.CurrentUser != null && UserContext.CurrentUser.UserId != currentUser.UserId)
            {
                title = isPosted ? currentUser.DisplayName + "�Ļ���" : currentUser.DisplayName + "������";
                ViewData["isOwner"] = false;
            }
            pageResourceManager.InsertTitlePart(title);
            ViewData["userId"] = currentUser != null ? currentUser.UserId : 0;
            return View(pds);
        }

        /// <summary>
        /// ��ǩ��ʾ�����б�
        /// </summary>
        /// <returns></returns>
        public ActionResult ListByTag(string spaceKey, string tagName, SortBy_BarThread? sortBy, bool? isEssential, int pageIndex = 1)
        {
            tagName = WebUtility.UrlDecode(tagName);
            PagingDataSet<BarThread> pds = barThreadService.Gets(TenantTypeIds.Instance().Group(), tagName, isEssential, sortBy ?? SortBy_BarThread.StageHitTimes, pageIndex);
            if (Request.IsAjaxRequest())
                return PartialView("~/Applications/Bar/Views/Bar/_ListInGroup.cshtml", pds);
            var group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            pageResourceManager.InsertTitlePart(group.GroupName);
            pageResourceManager.InsertTitlePart(tagName);
            ViewData["sortBy"] = sortBy;
            var tag = new TagService(TenantTypeIds.Instance().BarThread()).Get(tagName);

            if (tag == null)
            {
                return HttpNotFound();
            }
            ViewData["tag"] = tag;

            ViewData["SectionId"] = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);
            ViewData["group"] = group;
            return View(pds);
        }
        #endregion

        #region ����ҳ��
        /// <summary>
        /// ǰ̨��������ҳ�棨�������ӣ�
        /// </summary>
        /// <param name="model">�û�����ʵ��</param>
        /// <param name="pageIndex">��ǰҳ��</param>
        /// <returns>��̨��������ҳ��</returns>
        [HttpGet]
        public ActionResult ManageThreads(string spaceKey, ManageThreadEditModel model, int pageIndex = 1)
        {
            long groupId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);
            BarSection section = barSectionService.Get(groupId);
            if (!authorizer.BarSection_Manage(section))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = string.Format("��û��Ȩ�޹��� {0} ��", section == null ? "" : section.Name),
                    Title = "û��Ȩ��",
                    StatusMessageType = StatusMessageType.Hint
                }));
            }
            var group = groupService.Get(spaceKey);
            pageResourceManager.InsertTitlePart(group.GroupName);

            pageResourceManager.InsertTitlePart("���ɹ���");

            List<SelectListItem> SelectListItem_TrueAndFlase = new List<SelectListItem> { new SelectListItem { Text = "��", Value = true.ToString() }, new SelectListItem { Text = "��", Value = false.ToString() } };

            ViewData["IsEssential"] = new SelectList(SelectListItem_TrueAndFlase, "Value", "Text", model.IsEssential);
            ViewData["IsSticky"] = new SelectList(SelectListItem_TrueAndFlase, "Value", "Text", model.IsSticky);

            IEnumerable<Category> categories = categoryService.GetOwnerCategories(section.SectionId, TenantTypeIds.Instance().BarThread());
            ViewData["CategoryId"] = new SelectList(categories.Select(n => new { text = StringUtility.Trim(n.CategoryName, 20), value = n.CategoryId }), "value", "text", model.CategoryId);

            BarThreadQuery query = model.GetBarThreadQuery();
            query.SectionId = section.SectionId;
            ViewData["BarThreads"] = barThreadService.Gets(TenantTypeIds.Instance().Group(), query, model.PageSize ?? 20, pageIndex);

            model.SectionId = section.SectionId;

            ViewData["TenantType"] = new TenantTypeService().Get(TenantTypeIds.Instance().Group());

            return View(model);
        }

        /// <summary>
        /// �������ҳ��
        /// </summary>
        /// <param name="pageIndex">��ǰҳ��</param>
        /// <param name="model">���������model</param>
        /// <returns>�������</returns>
        [HttpGet]
        public ActionResult ManagePosts(string spaceKey, ManagePostsEditModel model, int pageIndex = 1)
        {
            long sectionId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);
            BarSection section = barSectionService.Get(sectionId);

            if (!authorizer.BarSection_Manage(section))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Title = "û��Ȩ��",
                    Body = "������û��Ȩ�ޱ༭������"
                }));
            }

            var group = groupService.Get(spaceKey);
            pageResourceManager.InsertTitlePart(group.GroupName);

            pageResourceManager.InsertTitlePart("��������");

            BarPostQuery query = model.AsBarPostQuery();
            query.SectionId = section.SectionId;

            model.SectionId = section.SectionId;

            ViewData["BarPosts"] = barPostService.Gets(TenantTypeIds.Instance().Group(), query, model.PageSize ?? 20, pageIndex);
            return View(model);
        }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <param name="pageIndex">��ǰҳ��</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ManageThreadCategories(string spaceKey, int pageIndex = 1)
        {
            long sectionId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);

            if (!authorizer.BarSection_Manage(sectionId))
            {
                return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                {
                    Body = "��û��Ȩ�ޱ༭�����ɵķ���",
                    Title = "û��Ȩ��"
                }));
            }
            var group = groupService.Get(spaceKey);
            pageResourceManager.InsertTitlePart(group.GroupName);

            pageResourceManager.InsertTitlePart("������");
            ViewData["SectionId"] = sectionId;
            return View(categoryService.GetOwnerCategories(sectionId, TenantTypeIds.Instance().BarThread()));
        }

        #endregion

        #region ��������
        /// <summary>
        /// �ݹ��ȡ���еĸ�����ļ��ϣ�������ǰ���ࣩ
        /// </summary>
        /// <param name="category">��ǰ���</param>
        /// <param name="allParentCategories">���еĸ����</param>
        private void RecursiveGetAllParentCategories(Category category, ref List<Category> allParentCategories)
        {
            if (category == null)
                return;
            allParentCategories.Insert(0, category);
            Category parent = category.Parent;
            if (parent != null)
                RecursiveGetAllParentCategories(parent, ref allParentCategories);
        }
        #endregion

        #region �ֲ�ҳ��

        /// <summary>
        /// ��ǩ��
        /// </summary>
        /// <param name="spaceKey">Ⱥ����</param>
        /// <param name="topNum">ǰN������</param>
        /// <returns>��ǩ��</returns>
        [HttpGet]
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _TagCloud(string spaceKey, int topNum = 20)
        {
            long sectionId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);
            TagService tagService = new TagService(TenantTypeIds.Instance().BarThread());
            Dictionary<TagInOwner, int> ownerTags = tagService.GetOwnerTopTags(topNum, sectionId);
            return View(ownerTags);
        }

        /// <summary>
        /// ��ǩ��ͼ
        /// </summary>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        public ActionResult Tags(string spaceKey)
        {
            GroupEntity group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();
            ViewData["group"] = group;
            return View();
        }

        /// <summary>
        /// ��������ҳ��
        /// </summary>
        /// <param name="spaceKey">��������</param>
        /// <returns>��������ҳ��</returns>
        [HttpGet]
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult _BarThreadSearch(string spaceKey)
        {
            long sectionId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);
            return View(sectionId);
        }
        #endregion

        #region ����ҳ��

        /// <summary>
        /// Ⱥ������������
        /// </summary>
        /// <param name="spaceKey">Ⱥ����</param>
        /// <param name="keyword">�ؼ���</param>
        /// <param name="pageIndex">ҳ��</param>
        /// <returns>Ⱥ������������</returns>
        public ActionResult Search(string spaceKey, string keyword, BarSearchRange term = BarSearchRange.ALL, int pageIndex = 1)
        {
            long barSectionId = GroupIdToGroupKeyDictionary.GetGroupId(spaceKey);

            var group = groupService.Get(spaceKey);
            if (group == null)
                return HttpNotFound();

            ViewData["group"] = group;
            BarSection section = barSectionService.Get(barSectionId);
            if (section == null)
                return HttpNotFound();

            ViewData["section"] = section;

            BarFullTextQuery query = new BarFullTextQuery();

            query.Term = term;

            query.PageIndex = pageIndex;
            query.PageSize = 20;//ÿҳ��¼��
            query.Keyword = keyword;
            query.Range = section.SectionId.ToString();

            //��ȡȺ�����ɵ�����
            query.TenantTypeId = TenantTypeIds.Instance().Group();

            //��������id��ѯ��������
            query.TenantTypeId = section.TenantTypeId;
            ViewData["barname"] = section.Name;
            ViewData["TenantTypeId"] = section.TenantTypeId;

            //������������������
            BarSearcher BarSearcher = (BarSearcher)SearcherFactory.GetSearcher(BarSearcher.CODE);
            PagingDataSet<BarEntity> BarEntities = BarSearcher.Search(query);

            if (Request.IsAjaxRequest())
                return View("~/Applications/Bar/Views/Bar/_ListSearchThread.cshtml", BarEntities);

            //����ҳ��Meta
            if (string.IsNullOrWhiteSpace(query.Keyword))
            {
                pageResourceManager.InsertTitlePart("Ⱥ����������");//����ҳ��Title
            }
            else
            {
                pageResourceManager.InsertTitlePart('��' + query.Keyword + '��' + "���������");//����ҳ��Title
            }

            pageResourceManager.SetMetaOfKeywords("��������");//����Keyords���͵�Meta
            pageResourceManager.SetMetaOfDescription("��������");//����Description���͵�Meta

            return View(BarEntities);
        }

        #endregion

    }

    /// <summary>
    /// Ⱥ�����ɹ���˵�
    /// </summary>
    public enum ManageSubMenu
    {
        /// <summary>
        /// ��������
        /// </summary>
        ManageThread,

        /// <summary>
        /// �������
        /// </summary>
        ManagePost,

        /// <summary>
        /// �������
        /// </summary>
        ManageCategroy
    }
}