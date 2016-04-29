//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2013</copyright>
//<version>V0.5</verion>
//<createdate>2013-06-22</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2013-06-22" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet;
using Tunynet.Common;
using Tunynet.Events;
using Spacebuilder.Common;
using Tunynet.Common.Configuration;
using Tunynet.FileStore;

using System.Configuration;
using Tunynet.Utilities;

namespace Spacebuilder.Wiki
{
    //todo:zhengw:未处理OwnerData

    /// <summary>
    /// 词条业务逻辑类
    /// </summary>
    public class WikiService 
    {
        private IWikiPageRepository wikiPageRepository;
        private IWikiPageVersionRepository wikiPageVersionRepository;
        private CategoryService categoryService = new CategoryService();
  
        /// <summary>
        /// 构造函数
        /// </summary>
        public WikiService()
            : this(new WikiPageRepository(), new WikiPageVersionRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="wikiPageRepository">词条仓储实现</param>
        /// <param name="wikiPageVersionRepository">词条版本仓储实现</param>
        public WikiService(IWikiPageRepository wikiPageRepository, IWikiPageVersionRepository wikiPageVersionRepository)
        {
            this.wikiPageRepository = wikiPageRepository;
            this.wikiPageVersionRepository = wikiPageVersionRepository;
        }

        #region 维护词条

        /// <summary>
        /// 撰写词条
        /// </summary>
        /// <param name="wikiPage">词条实体</param>
        public bool Create(WikiPage wikiPage, string body)
        {
            EventBus<WikiPage>.Instance().OnBefore(wikiPage, new CommonEventArgs(EventOperationType.Instance().Create()));

            //设置审核状态，草稿的审核状态为待审核
            AuditService auditService = new AuditService();
            auditService.ChangeAuditStatusForCreate(wikiPage.UserId, wikiPage);

            long pageId = 0;
            long.TryParse(wikiPageRepository.Insert(wikiPage).ToString(), out pageId);

            if (pageId > 0)
            {
                WikiPageVersion wikiPageVersion = WikiPageVersion.New();
                wikiPageVersion.PageId = pageId;
                wikiPageVersion.TenantTypeId = wikiPage.TenantTypeId;
                wikiPageVersion.OwnerId = wikiPage.OwnerId;
                wikiPageVersion.VersionNum = 1;
                wikiPageVersion.Title = wikiPage.Title;
                wikiPageVersion.UserId = wikiPage.UserId;
                wikiPageVersion.Author = wikiPage.Author;
                wikiPageVersion.Summary = StringUtility.Trim(Tunynet.Utilities.HtmlUtility.StripHtml(body,true,false).Replace("\r","").Replace("\n",""),200);
                wikiPageVersion.Body = body;
                wikiPageVersion.AuditStatus = AuditStatus.Success;
                wikiPageVersionRepository.Insert(wikiPageVersion);
                //转换临时附件
                AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().WikiPage());
                attachmentService.ToggleTemporaryAttachments(wikiPage.OwnerId, TenantTypeIds.Instance().WikiPage(), wikiPageVersion.PageId);

                OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                ownerDataService.Change(wikiPage.UserId, OwnerDataKeys.Instance().PageCount(), 1);

                EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<WikiPage, AuditEventArgs>.Instance().OnAfter(wikiPage, new AuditEventArgs(null, wikiPage.AuditStatus));

                return true;
            }

            return false;
        }

        /// <summary>
        /// 撰写词条
        /// </summary>
        /// <param name="wikiPage">词条实体</param>
        public bool Create_sp(WikiPage wikiPage, string body)
        {
            EventBus<WikiPage>.Instance().OnBefore(wikiPage, new CommonEventArgs(EventOperationType.Instance().Create()));

            //设置审核状态，草稿的审核状态为待审核
            //AuditService auditService = new AuditService();
            //auditService.ChangeAuditStatusForCreate(wikiPage.UserId, wikiPage);

            long pageId = 0;
            long.TryParse(wikiPageRepository.Insert(wikiPage).ToString(), out pageId);

            if (pageId > 0)
            {
                WikiPageVersion wikiPageVersion = WikiPageVersion.New();
                wikiPageVersion.PageId = pageId;
                wikiPageVersion.TenantTypeId = wikiPage.TenantTypeId;
                wikiPageVersion.OwnerId = wikiPage.OwnerId;
                wikiPageVersion.VersionNum = 1;
                wikiPageVersion.Title = wikiPage.Title;
                wikiPageVersion.UserId = wikiPage.UserId;
                wikiPageVersion.Author = wikiPage.Author;
                wikiPageVersion.Summary = StringUtility.Trim(Tunynet.Utilities.HtmlUtility.StripHtml(body, true, false).Replace("\r", "").Replace("\n", ""), 200);
                wikiPageVersion.Body = body;
                wikiPageVersion.AuditStatus = AuditStatus.Success;
                wikiPageVersionRepository.Insert(wikiPageVersion);
                //转换临时附件
                AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().WikiPage());
                attachmentService.ToggleTemporaryAttachments(wikiPage.OwnerId, TenantTypeIds.Instance().WikiPage(), wikiPageVersion.PageId);

                OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                ownerDataService.Change(wikiPage.UserId, OwnerDataKeys.Instance().PageCount(), 1);

                EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<WikiPage, AuditEventArgs>.Instance().OnAfter(wikiPage, new AuditEventArgs(null, wikiPage.AuditStatus));

                return true;
            }

            return false;
        }

        /// <summary>
        /// 更新词条
        /// </summary>
        public void Update(WikiPage wikiPage)
        {
            wikiPageRepository.Update(wikiPage);

            EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(EventOperationType.Instance().Update()));
        }

        /// <summary>
        /// 加精/取消精华
        /// </summary>
        /// <param name="pageId">词条Id</param>
        /// <param name="isEssential">是否设为精华</param>
        public void SetEssential(long pageId, bool isEssential)
        {
            WikiPage wikiPage = wikiPageRepository.Get(pageId);

            if (wikiPage == null)
                return;

            if (wikiPage.IsEssential == isEssential)
                return;

            wikiPage.IsEssential = isEssential;
            wikiPageRepository.Update(wikiPage);

            string operationType = isEssential ? EventOperationType.Instance().SetEssential() : EventOperationType.Instance().CancelEssential();
            EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(operationType));
        }

        /// <summary>
        /// 加锁/解锁
        /// </summary>
        /// <param name="pageId">词条Id</param>
        /// <param name="isLocked">是否加锁</param>
        public void SetLock(long pageId, bool isLocked)
        {
            WikiPage wikiPage = wikiPageRepository.Get(pageId);

            if (wikiPage == null)
                return;

            if (wikiPage.IsLocked == isLocked)
                return;

            wikiPage.IsLocked = isLocked;
            wikiPageRepository.Update(wikiPage);

            string operationType = isLocked ? EventOperationType.Instance().SetLock() : EventOperationType.Instance().CancelLock();
            EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(operationType));
        }

        /// <summary>
        /// 批准/不批准词条
        /// </summary>
        /// <param name="pageId">待被更新的词条Id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void Approve(long pageId, bool isApproved)
        {
            WikiPage wikiPage = wikiPageRepository.Get(pageId);

            AuditStatus newAuditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            if (wikiPage.AuditStatus == newAuditStatus)
            {
                return;
            }

            AuditStatus oldAuditStatus = wikiPage.AuditStatus;
            wikiPage.AuditStatus = newAuditStatus;
            wikiPageRepository.Update(wikiPage);
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(operationType));
            EventBus<WikiPage, AuditEventArgs>.Instance().OnAfter(wikiPage, new AuditEventArgs(oldAuditStatus, newAuditStatus));
        }

        /// <summary>
        /// 逻辑删除词条
        /// </summary>
        /// <param name="pageId">词条Id</param>
        public void LogicalDelete(long pageId)
        {
            WikiPage wikiPage = wikiPageRepository.Get(pageId);

            if (wikiPage == null)
                return;

            if (wikiPage.IsLogicalDelete == true)
                return;

            wikiPage.IsLogicalDelete = true;
            wikiPageRepository.Update(wikiPage);

            EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(EventOperationType.Instance().LogicalDelete()));
        }

        /// <summary>
        /// 恢复删除的词条
        /// </summary>
        /// <param name="pageId">词条Id</param>
        public void Restore(long pageId)
        {
            WikiPage wikiPage = wikiPageRepository.Get(pageId);

            if (wikiPage == null)
                return;

            if (wikiPage.IsLogicalDelete == true)
                return;

            wikiPage.IsLogicalDelete = true;
            wikiPageRepository.Update(wikiPage);

            EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(EventOperationType.Instance().Restore()));
        }

        /// <summary>
        /// 删除词条
        /// </summary>
        /// <param name="pageId">词条Id</param>
        public void Delete(long pageId)
        {
            WikiPage WikiPage = wikiPageRepository.Get(pageId);
            if (WikiPage == null)
            {
                return;
            }

            Delete(WikiPage);
        }

        /// <summary>
        /// 删除词条
        /// </summary>
        /// <param name="wikiPage">词条实体</param>
        public void Delete(WikiPage wikiPage)
        {
            if (wikiPage == null)
            {
                return;
            }

            //todo:zhengw：需要处理词条版本

            CategoryService categoryService = new CategoryService();

            //删除站点分类关联（投稿到）
            categoryService.ClearCategoriesFromItem(wikiPage.PageId, 0, TenantTypeIds.Instance().WikiPage());

            //删除标签关联
            TagService tagService = new TagService(TenantTypeIds.Instance().WikiPage());
            tagService.ClearTagsFromItem(wikiPage.PageId, wikiPage.OwnerId);

            //删除推荐记录
            RecommendService recommendService = new RecommendService();
            recommendService.Delete(wikiPage.PageId, wikiPage.TenantTypeId);

            //用户创建词条数-1
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
            ownerDataService.Change(wikiPage.UserId, OwnerDataKeys.Instance().PageCount(), -1);

            EventBus<WikiPage>.Instance().OnBefore(wikiPage, new CommonEventArgs(EventOperationType.Instance().Delete()));
            wikiPageRepository.Delete(wikiPage);

            EventBus<WikiPage>.Instance().OnAfter(wikiPage, new CommonEventArgs(EventOperationType.Instance().Delete()));
            EventBus<WikiPage, AuditEventArgs>.Instance().OnAfter(wikiPage, new AuditEventArgs(wikiPage.AuditStatus, null));
        }

        /// <summary>
        /// 删除用户时处理所有词条相关的数据（删除用户时使用）
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="takeOverUserName">用于接管删除用户时不能删除的内容(例如：用户创建的群组)</param>
        /// <param name="takeOverAll">是否接管被删除用户的所有内容</param>
        /// <remarks>接管被删除用户的所有内容</remarks>
        public void DeleteUser(long userId, string takeOverUserName, bool takeOverAll)
        {
            //takeOverAll为true，指定其他用户接管数据
            if (takeOverAll)
            {
                IUserService userService = DIContainer.Resolve<IUserService>();
                User takeOverUser = userService.GetFullUser(takeOverUserName);
                wikiPageRepository.TakeOver(userId, takeOverUser);
            }
            //takeOverAll为false，调用Delete方法逐条删除用户的词条，积分、索引、动态等的删除由EventModule处理，评论、顶踩、附件等相关数据由系统自动删除
            else
            {
                int pageSize = 100;     //批量删除，每次删100条
                int pageIndex = 1;
                int pageCount = 1;
                do
                {
                    PagingDataSet<WikiPage> Wikis = GetOwnerPages(TenantTypeIds.Instance().User(), userId, true, null, pageSize, pageIndex);
                    foreach (WikiPage Wiki in Wikis)
                    {
                        Delete(Wiki);
                    }
                    pageCount = Wikis.PageCount;
                    pageIndex++;
                } while (pageIndex <= pageCount);
            }
        }

        #endregion

        #region 获取词条

        /// <summary>
        /// 获取词条
        /// </summary>
        /// <param name="title">词条名</param>
        public bool IsExists(string title)
        {
            if (string.IsNullOrEmpty(title))
                return false;
            else
                return PageIdToTitleDictionary.GetPageId(title.Trim()) > 0;
        }

        /// <summary>
        /// 获取词条
        /// </summary>
        /// <param name="pageId">词条Id</param>
        /// <returns>词条实体</returns>
        public WikiPage Get(long pageId)
        {
            return wikiPageRepository.Get(pageId);
        }

        /// <summary>
        /// 获取Owner的词条
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码</param>        
        /// <returns>词条分页列表</returns>
        public PagingDataSet<WikiPage> GetOwnerPages(string tenantTypeId, long ownerId, bool ignoreAudit, string tagName, int pageSize = 20, int pageIndex = 1)
        {
            return wikiPageRepository.GetOwnerPages(tenantTypeId, ownerId, ignoreAudit, tagName, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取用户编辑过的词条
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="userId"></param>
        /// <param name="ignoreAudit"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<WikiPage> GetUserEditedPages(string tenantTypeId, long userId, bool ignoreAudit, int pageSize = 20, int pageIndex = 1)
        {
            return wikiPageRepository.GetUserEditedPages(tenantTypeId, userId, ignoreAudit, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取需要用户完善的词条
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="userId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<WikiPage> GetPerfectPages(string tenantTypeId, long userId, int pageSize = 20, int pageIndex = 1)
        {
            return wikiPageRepository.GetPerfectPages(tenantTypeId, userId, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取词条的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">词条排序依据</param>
        /// <returns>词条列表</returns>
        public IEnumerable<WikiPage> GetTops(string tenantTypeId, int topNumber, long? categoryId, bool? isEssential, SortBy_WikiPage sortBy)
        {
            return wikiPageRepository.GetTops(tenantTypeId, topNumber, categoryId, isEssential, sortBy);
        }

        /// <summary>
        /// 获取词条排行分页集合
        /// </summary>
        /// <remarks>rss订阅也使用方法</remarks>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="ignoreAudit">忽略审核状态</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="userIdForTags">根据此UserId来获取用户标签，并作为词条的关联标签</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>词条分页列表</returns>
        public PagingDataSet<WikiPage> Gets(string tenantTypeId, long? ownerId = null, bool ignoreAudit = false, long? categoryId = null, string tagName = null, bool? isEssential = null, SortBy_WikiPage sortBy = SortBy_WikiPage.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return wikiPageRepository.Gets(tenantTypeId, ownerId, ignoreAudit, categoryId, tagName, isEssential, sortBy, pageSize, pageIndex);
        }

        /// 获取词条排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="categoryId">站点类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="titleKeywords">词条关键字</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>词条分页列表</returns>
        public PagingDataSet<WikiPage> GetsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, bool? isEssential, long? ownerId, string titleKeywords, int pageSize = 20, int pageIndex = 1)
        {
            return wikiPageRepository.GetsForAdmin(tenantTypeId, auditStatus, categoryId, isEssential, ownerId, titleKeywords, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据词条Id集合组装词条实体集合
        /// </summary>
        /// <param name="pageIds">词条Id集合</param>
        /// <returns>词条实体集合</returns>
        public IEnumerable<WikiPage> GetWikiPages(IEnumerable<long> pageIds)
        {
            return wikiPageRepository.PopulateEntitiesByEntityIds<long>(pageIds);
        }

        /// <summary>
        /// 获取词条管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>词条管理数据</returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            return wikiPageRepository.GetManageableDatas(tenantTypeId);
        }

        /// <summary>
        /// 获取词条统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>词条统计数据</returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            return wikiPageRepository.GetStatisticDatas();
        }

        #endregion

        #region 维护词条版本

        //todo:zhengw:未处理VersionNum，感觉没用
        /// <summary>
        /// 创建词条版本
        /// </summary>
        /// <param name="wikiPageVersion">词条实体</param>
        public void CreatePageVersion(WikiPageVersion wikiPageVersion)
        {
            WikiPage wikiPage = Get(wikiPageVersion.PageId);
            if (wikiPage == null)
                return;

            EventBus<WikiPageVersion>.Instance().OnBefore(wikiPageVersion, new CommonEventArgs(EventOperationType.Instance().Create()));

            AuditService auditService = new AuditService();
            auditService.ChangeAuditStatusForCreate(wikiPageVersion.UserId, wikiPageVersion);
            
            wikiPageVersionRepository.Insert(wikiPageVersion);
            if (wikiPageVersion.VersionId > 0)
            {
                wikiPage.EditionCount++;
                wikiPage.LastModified = DateTime.UtcNow;
                wikiPageRepository.Update(wikiPage);
                //转换临时附件
                AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().WikiPage());
                attachmentService.ToggleTemporaryAttachments(wikiPage.OwnerId, TenantTypeIds.Instance().WikiPage(), wikiPageVersion.PageId);

                EventBus<WikiPageVersion>.Instance().OnAfter(wikiPageVersion, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<WikiPageVersion, AuditEventArgs>.Instance().OnAfter(wikiPageVersion, new AuditEventArgs(null, wikiPageVersion.AuditStatus));
            }
        }
        //todo:zhengw:未处理VersionNum，感觉没用
        /// <summary>
        /// 创建词条版本
        /// </summary>
        /// <param name="wikiPageVersion">词条实体</param>
        public void CreatePageVersion(WikiPageVersion wikiPageVersion,bool Is)
        {
            WikiPage wikiPage = Get(wikiPageVersion.PageId);
            if (wikiPage == null)
                return;

            EventBus<WikiPageVersion>.Instance().OnBefore(wikiPageVersion, new CommonEventArgs(EventOperationType.Instance().Create()));

            AuditService auditService = new AuditService();
            //auditService.ChangeAuditStatusForCreate(wikiPageVersion.UserId, wikiPageVersion);

            wikiPageVersionRepository.Insert(wikiPageVersion);
            if (wikiPageVersion.VersionId > 0)
            {
                wikiPage.EditionCount++;
                wikiPage.LastModified = DateTime.UtcNow;
                wikiPageRepository.Update(wikiPage);
                //转换临时附件
                AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().WikiPage());
                attachmentService.ToggleTemporaryAttachments(wikiPage.OwnerId, TenantTypeIds.Instance().WikiPage(), wikiPageVersion.PageId);

                EventBus<WikiPageVersion>.Instance().OnAfter(wikiPageVersion, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<WikiPageVersion, AuditEventArgs>.Instance().OnAfter(wikiPageVersion, new AuditEventArgs(null, wikiPageVersion.AuditStatus));
            }
        }
        /// <summary>
        /// 批准/不批准词条版本
        /// </summary>
        /// <param name="versionId">待被更新的词条版本Id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void ApprovePageVersion(long versionId, bool isApproved)
        {
            WikiPageVersion wikiPageVersion = wikiPageVersionRepository.Get(versionId);

            AuditStatus newAuditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            if (wikiPageVersion.AuditStatus == newAuditStatus)
            {
                return;
            }

            AuditStatus oldAuditStatus = wikiPageVersion.AuditStatus;
            wikiPageVersion.AuditStatus = newAuditStatus;
            wikiPageVersionRepository.Update(wikiPageVersion);

            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            EventBus<WikiPageVersion>.Instance().OnAfter(wikiPageVersion, new CommonEventArgs(operationType));
            EventBus<WikiPageVersion, AuditEventArgs>.Instance().OnAfter(wikiPageVersion, new AuditEventArgs(oldAuditStatus, newAuditStatus));
        }

        /// <summary>
        /// 删除词条版本
        /// </summary>
        /// <param name="versionId">词条版本Id</param>
        public void DeletePageVersion(long versionId)
        {
            WikiPageVersion wikiPageVersion = wikiPageVersionRepository.Get(versionId);
            if (wikiPageVersion == null)
            {
                return;
            }
            DeletePageVersion(wikiPageVersion);
        }

        /// <summary>
        /// 删除词条版本
        /// </summary>
        /// <param name="wikiPageVersion">词条版本</param>
        public void DeletePageVersion(WikiPageVersion wikiPageVersion)
        {
            if (wikiPageVersion == null)
            {
                return;
            }
            WikiPage wikiPage = Get(wikiPageVersion.PageId);
            if (wikiPage == null)
                return;

            EventBus<WikiPageVersion>.Instance().OnBefore(wikiPageVersion, new CommonEventArgs(EventOperationType.Instance().Delete()));
            wikiPageVersionRepository.Delete(wikiPageVersion);
            if (wikiPage.EditionCount > 0)
            {
                wikiPage.EditionCount--;
                wikiPageRepository.Update(wikiPage);
            }
            EventBus<WikiPageVersion>.Instance().OnAfter(wikiPageVersion, new CommonEventArgs(EventOperationType.Instance().Delete()));
            EventBus<WikiPageVersion, AuditEventArgs>.Instance().OnAfter(wikiPageVersion, new AuditEventArgs(wikiPageVersion.AuditStatus, null));
        }

        /// <summary>
        /// 回滚版本
        /// </summary>
        /// <param name="versionId">回滚到的版本</param>
        public void RollbackPageVersion(long versionId, int versionNum)
        {
            WikiPageVersion wikiPageVersion = GetPageVersion(versionId);
            if (wikiPageVersion == null)
                return;
            WikiPage wikiPage = Get(wikiPageVersion.PageId);
            if (wikiPage == null)
                return;
            WikiPageVersion newWikiPageVersion = WikiPageVersion.New();
            newWikiPageVersion.PageId = wikiPageVersion.PageId;
            newWikiPageVersion.TenantTypeId = wikiPageVersion.TenantTypeId;
            newWikiPageVersion.OwnerId = wikiPageVersion.OwnerId;
            //todo:zhengw:未处理VersionNum，感觉没用
            newWikiPageVersion.VersionNum = 0;
            newWikiPageVersion.Title = wikiPageVersion.Title;
            newWikiPageVersion.UserId = wikiPageVersion.UserId;
            newWikiPageVersion.Author = wikiPageVersion.Author;
            newWikiPageVersion.Summary = Tunynet.Utilities.HtmlUtility.TrimHtml(wikiPageVersion.GetBody(), 200);
            newWikiPageVersion.Body = wikiPageVersion.GetBody();
            newWikiPageVersion.AuditStatus = AuditStatus.Success;
            string versionNumUrl = string.Format("<a  target='_blank' href={0}>版本{1}</a>", SiteUrls.Instance().PageDetail(wikiPageVersion.PageId, versionId), versionNum);
            newWikiPageVersion.Reason = string.Format("回滚到{0}", versionNumUrl);
            wikiPageVersionRepository.Insert(newWikiPageVersion);
        }


        #endregion

        #region 获取词条版本

        /// <summary>
        /// 获取词条版本
        /// </summary>
        /// <param name="versionId">版本Id</param>
        /// <returns>词条版本</returns>
        public WikiPageVersion GetPageVersion(long versionId)
        {
            return wikiPageVersionRepository.Get(versionId);
        }

        /// <summary>
        /// 根据词条Id集合组装词条实体集合
        /// </summary>
        /// <param name="versionIds">词条Id集合</param>
        /// <returns>词条实体集合</returns>
        public IEnumerable<WikiPageVersion> GetPageVersions(IEnumerable<long> versionIds)
        {
            return wikiPageVersionRepository.PopulateEntitiesByEntityIds<long>(versionIds);
        }

        /// <summary>
        /// 获取词条的历史版本
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<WikiPageVersion> GetPageVersionsOfPage(long pageId, int pageSize = 20, int pageIndex = 1)
        {
            return wikiPageVersionRepository.GetPageVersionsOfPage(pageId, pageSize, pageIndex);
        }

        /// <summary>
        /// 后台管理词条版本
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="auditStatus"></param>
        /// <param name="categoryId"></param>
        /// <param name="ownerId"></param>
        /// <param name="titleKeywords"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<WikiPageVersion> GetPageVersionsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, long? ownerId, string titleKeywords, int pageSize = 20, int pageIndex = 1)
        {
            return wikiPageVersionRepository.GetPageVersionsForAdmin(tenantTypeId, auditStatus, categoryId, ownerId, titleKeywords, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取词条管版本理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>词条版本管理数据</returns>
        public Dictionary<string, long> GetManageableDatasForWikiVersion(string tenantTypeId = null)
        {
            return wikiPageVersionRepository.GetManageableDatasForWikiVersion(tenantTypeId);
        }

        #endregion


        #region 完善词条


        #endregion

        /// <summary>
        /// 获取问答的标题和body
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        public void GetAskTitleAndBody(long questionId, out string title, out string body)
        {
            wikiPageRepository.GetAskTitleAndBody(questionId, out title, out body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantTypeId"></param>
        /// <param name="topNumber"></param>
        /// <param name="categoryId"></param>
        /// <param name="isEssential"></param>
        /// <param name="sortBy"></param>
        /// <returns></returns>
        public IEnumerable<WikiPage> GetTopsForExtractEntry(string tenantTypeId, int topNumber, long? categoryId, bool? isEssential, SortBy_WikiPage sortBy)
        {
            IEnumerable<WikiPage> wikis = wikiPageRepository.GetTops(tenantTypeId, topNumber, categoryId, isEssential, sortBy);
            IEnumerable<WikiPage> iwikis = wikis as IEnumerable<WikiPage>;
            return iwikis;
        }

 
        /// <summary>
        /// 获取词条排行分页集合
        /// </summary>
        /// <returns>词条分页列表</returns>
        public PagingDataSet<WikiPage> Gets(string keyWord, string tenantTypeId, long? ownerId = null, bool ignoreAudit = false, long? categoryId = null, string tagName = null, bool? isEssential = null, SortBy_WikiPage sortBy = SortBy_WikiPage.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            return wikiPageRepository.Gets(keyWord, tenantTypeId, ownerId, ignoreAudit, categoryId, tagName, isEssential, sortBy, pageSize, pageIndex);
        }
    }
}