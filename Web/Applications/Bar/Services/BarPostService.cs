//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-28</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-08-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Events;

namespace Spacebuilder.Bar
{

    /// <summary>
    /// 回帖业务逻辑类
    /// </summary>
    public class BarPostService
    {
        private AuditService auditService = new AuditService();
        private IBarPostRepository barPostRepository = null;



        /// <summary>
        /// 构造函数
        /// </summary>
        public BarPostService()
            : this(new BarPostRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="barPostRepository"></param>
        public BarPostService(IBarPostRepository barPostRepository)
        {
            this.barPostRepository = barPostRepository;
        }


        #region 维护回复贴

        /// <summary>
        /// 创建回复贴
        /// </summary>
        /// <param name="post">回复贴</param>
        public bool Create(BarPost post)
        {


            BarSectionService barSectionService = new BarSectionService();
            BarSection barSection = barSectionService.Get(post.SectionId);
            if (barSection == null)
                return false;

            EventBus<BarPost>.Instance().OnBefore(post, new CommonEventArgs(EventOperationType.Instance().Create()));
            //设置审核状态
            auditService.ChangeAuditStatusForCreate(post.UserId, post);
            long id = 0;
            long.TryParse(barPostRepository.Insert(post).ToString(), out id);

            if (id > 0)
            {
                new AttachmentService(TenantTypeIds.Instance().BarPost()).ToggleTemporaryAttachments(post.UserId, TenantTypeIds.Instance().BarPost(), id);

                //计数
                CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                countService.ChangeCount(CountTypes.Instance().ThreadAndPostCount(), barSection.SectionId, barSection.UserId, 1, true);
                if (post.TenantTypeId == TenantTypeIds.Instance().Group())
                {
                    //群组内容计数+1
                    OwnerDataService groupOwnerDataService = new OwnerDataService(TenantTypeIds.Instance().Group());
                    groupOwnerDataService.Change(post.SectionId, OwnerDataKeys.Instance().PostCount(), 1);
                }
                else if (post.TenantTypeId == TenantTypeIds.Instance().Bar())
                {
                    //用户内容计数+1
                    OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                    ownerDataService.Change(post.UserId, OwnerDataKeys.Instance().PostCount(), 1);
                }

                //更新帖子主题计数缓存
                RealTimeCacheHelper realTimeCacheHelper = EntityData.ForType(typeof(BarThread)).RealTimeCacheHelper;
                string cacheKey = realTimeCacheHelper.GetCacheKeyOfEntity(post.ThreadId);
                ICacheService cacheService = DIContainer.Resolve<ICacheService>();
                BarThread barThread = cacheService.Get<BarThread>(cacheKey);

                if (barThread != null && barThread.ThreadId > 0)
                {
                    barThread.PostCount++;
                    cacheService.Set(cacheKey, barThread, CachingExpirationType.SingleObject);
                }

                new AtUserService(TenantTypeIds.Instance().BarPost()).ResolveBodyForEdit(post.GetBody(), post.UserId, post.PostId);

                EventBus<BarPost>.Instance().OnAfter(post, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<BarPost, AuditEventArgs>.Instance().OnAfter(post, new AuditEventArgs(null, post.AuditStatus));
            }
            return id > 0;
        }

        /// <summary>
        /// 更新回复贴
        /// </summary>
        /// <param name="post">回复贴</param>
        public void Update(BarPost post, long userId)
        {
            EventBus<BarPost>.Instance().OnBefore(post, new CommonEventArgs(EventOperationType.Instance().Update()));
            auditService.ChangeAuditStatusForUpdate(userId, post);
            barPostRepository.Update(post);
            new AtUserService(TenantTypeIds.Instance().BarPost()).ResolveBodyForEdit(post.GetBody(), post.UserId, post.PostId);
            EventBus<BarPost>.Instance().OnAfter(post, new CommonEventArgs(EventOperationType.Instance().Update()));
        }

        /// <summary>
        /// 更新审核状态
        /// </summary>
        /// <param name="postId">待被更新的回帖Id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void UpdateAuditStatus(long postId, bool isApproved)
        {
            BarPost post = barPostRepository.Get(postId);
            AuditStatus oldAuditStatus = post.AuditStatus;
            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            if (post.AuditStatus == auditStatus)
                return;
            post.AuditStatus = auditStatus;
            barPostRepository.Update(post);
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();



            EventBus<BarPost>.Instance().OnAfter(post, new CommonEventArgs(operationType));
            EventBus<BarPost, AuditEventArgs>.Instance().OnAfter(post, new AuditEventArgs(oldAuditStatus, post.AuditStatus));
        }

        /// <summary>
        /// 批量更新审核状态
        /// </summary>
        /// <param name="postIds">待被更新的回复贴Id集合</param>
        /// <param name="isApproved">是否通过审核</param>
        public void BatchUpdateAuditStatus(IEnumerable<long> postIds, bool isApproved)
        {
            IEnumerable<BarPost> posts = GetBarPosts(postIds);

            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            foreach (var post in posts)
            {
                if (post.AuditStatus == auditStatus)
                    continue;
                AuditStatus oldAuditStatus = post.AuditStatus;

                post.AuditStatus = auditStatus;
                barPostRepository.Update(post);



                EventBus<BarPost>.Instance().OnAfter(post, new CommonEventArgs(operationType));
                EventBus<BarPost, AuditEventArgs>.Instance().OnAfter(post, new AuditEventArgs(oldAuditStatus, post.AuditStatus));
            }
            //更新主题帖最新回复的缓存
        }

        /// <summary>
        /// 删除回复贴
        /// </summary>
        /// <param name="postId">回复贴Id</param>
        public void Delete(long postId)
        {
            BarPost post = barPostRepository.Get(postId);
            if (post == null)
                return;
            BarSectionService barSectionService = new BarSectionService();
            BarSection barSection = barSectionService.Get(post.SectionId);
            if (barSection == null)
                return;
            EventBus<BarPost>.Instance().OnBefore(post, new CommonEventArgs(EventOperationType.Instance().Delete()));
            int affectCount = barPostRepository.Delete(post);
            if (affectCount > 0)
            {
                //更新帖吧的计数
                CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                countService.ChangeCount(CountTypes.Instance().ThreadAndPostCount(), barSection.SectionId, barSection.UserId, -1 - post.ChildPostCount, true);
                if (post.TenantTypeId == TenantTypeIds.Instance().Group())
                {
                    //群组内容计数-1
                    OwnerDataService groupOwnerDataService = new OwnerDataService(TenantTypeIds.Instance().Group());
                    groupOwnerDataService.Change(post.SectionId, OwnerDataKeys.Instance().PostCount(), -1);
                }
                else if (post.TenantTypeId == TenantTypeIds.Instance().Bar())
                {
                    //用户内容计数-1
                    OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                    ownerDataService.Change(post.UserId, OwnerDataKeys.Instance().PostCount(), -1);
                }

                EventBus<BarPost>.Instance().OnAfter(post, new CommonEventArgs(EventOperationType.Instance().Delete()));
                EventBus<BarPost, AuditEventArgs>.Instance().OnAfter(post, new AuditEventArgs(post.AuditStatus, null));
            }
        }

        /// <summary>
        /// 删除帖子下的所有回帖
        /// </summary>
        /// <param name="threadId"></param>
        public void DeletesByThreadId(long threadId)
        {
            IEnumerable<BarPost> barPosts = barPostRepository.GetAllPostsOfThread(threadId);
            foreach (var barPost in barPosts)
            {
                Delete(barPost.PostId);
            }
        }

        /// <summary>
        /// 删除某个用户的所有回帖
        /// </summary>
        /// <param name="userId"></param>
        public void DeletesByUserId(long userId)
        {
            IEnumerable<BarPost> barPosts = barPostRepository.GetAllPostsOfUser(userId);
            foreach (var barPost in barPosts)
            {
                Delete(barPost.PostId);
            }
        }

        #endregion

        #region 获取回复贴

        /// <summary>
        /// 判断用户某一段时间内是否对某个帖子回过帖
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="threadId"></param>
        /// <param name="beforeDays"></param>
        /// <returns></returns>
        public bool IsPosted(long userId, long threadId, int beforeDays = 30)
        {
            IEnumerable<long> threadIds = barPostRepository.GetThreadIdsByUser(userId, beforeDays);

            if (threadIds != null)
            {
                return threadIds.Contains(threadId);
            }

            return false;

        }

        /// <summary>
        /// 获取单个回复贴实体
        /// </summary>
        /// <param name="postId">回复贴Id</param>
        /// <returns>回复贴</returns>
        public BarPost Get(long postId)
        {
            return barPostRepository.Get(postId);

        }

        /// <summary>
        /// 获取主题帖最新的一条回复
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns>回复贴（若没有，则返回null）</returns>
        public BarPost GetNewestPost(long threadId)
        {
            //筛选：审核状态必须可对外显示
            //排序：



            return barPostRepository.GetNewestPost(threadId);
        }

        /// <summary>
        /// 获取我的回复贴分页集合
        /// </summary>
        /// <param name="userId">回复贴作者Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回复贴列表</returns>
        public PagingDataSet<BarPost> GetMyPosts(long userId, string tenantTypeId = null, int pageIndex = 1)
        {
            return barPostRepository.GetMyPosts(userId, tenantTypeId, pageIndex);
        }

        /// <summary>
        /// 获取主题帖的回帖排行分页集合
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <param name="onlyStarter">仅看楼主</param>
        /// <param name="sortBy">回帖排序依据（默认为按创建时间正序排序）</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>回复贴列表</returns>
        public PagingDataSet<BarPost> Gets(long threadId, bool onlyStarter = false, SortBy_BarPost sortBy = SortBy_BarPost.DateCreated, int pageIndex = 1)
        {





            BarThread thread = new BarThreadService().Get(threadId);
            long starterId = 0;
            if (thread != null)
                starterId = thread.UserId;

            return barPostRepository.Gets(threadId, onlyStarter, starterId, sortBy, pageIndex);
        }

        /// <summary>
        /// 获取子级回帖列表
        /// </summary>
        /// <param name="parentId">父回帖Id</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="sortBy">排序字段</param> 
        /// <returns></returns>
        public PagingDataSet<BarPost> GetChildren(long parentId, SortBy_BarPost sortBy = SortBy_BarPost.DateCreated, int pageIndex = 1)
        {
            //排序：Id正序
            //缓存分区：ParentId
            //仅显示可公开对外显示的 PubliclyAuditStatus 



            return barPostRepository.GetChildren(parentId, pageIndex, sortBy);
        }

        /// <summary>
        /// 获取回帖的管理列表
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="query">帖子查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖子分页集合</returns>
        public PagingDataSet<BarPost> Gets(string tenantTypeId, BarPostQuery query, int pageSize, int pageIndex)
        {
            return barPostRepository.Gets(tenantTypeId, query, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据回帖Id集合组装回帖实体集合
        /// </summary>
        /// <param name="postIds">回帖Id集合</param>
        /// <returns>回帖实体集合</returns>
        public IEnumerable<BarPost> GetBarPosts(IEnumerable<long> postIds)
        {
            return barPostRepository.PopulateEntitiesByEntityIds<long>(postIds);
        }

        /// <summary>
        /// 获取帖子所在的页数
        /// </summary>
        public int GetPageIndexForPostInThread(long threadId, long postId)
        {
            return barPostRepository.GetPageIndexForPostInThread(threadId, postId);
        }

        /// <summary>
        /// 获取二级回复在二级回复列表的页码数
        /// </summary>
        /// <param name="parentId">父回帖的id</param>
        /// <param name="postId">子回复的id</param>
        /// <returns>二级回复在二级回复列表中的页码数</returns>
        public int GetPageIndexForChildrenPost(long parentId, long postId)
        {
            return barPostRepository.GetPageIndexForChildrenPost(parentId, postId);
        }

        #endregion


        /// <summary>
        /// 获取统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型</param>
        /// <returns></returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId)
        {
            return barPostRepository.GetManageableDatas(tenantTypeId);
        }
    }
}