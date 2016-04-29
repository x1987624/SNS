//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-10-16</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2012-10-16" version="0.5">创建</log>
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

namespace Spacebuilder.Blog
{
    /// <summary>
    /// 日志业务逻辑类
    /// </summary>
    public class BlogService
    {
        private IBlogThreadRepository blogThreadRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BlogService()
            : this(new BlogThreadRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="blogThreadRepository">日志仓储实现</param>
        public BlogService(IBlogThreadRepository blogThreadRepository)
        {
            this.blogThreadRepository = blogThreadRepository;
        }

        #region 维护日志



        /// <summary>
        /// 撰写日志/转载日志
        /// </summary>
        /// <param name="blogThread">日志实体</param>
        public bool Create(BlogThread blogThread, string privacyStatus1 = null, string privacyStatus2 = null)
        {
            //设计要点
            //1、使用AuditService设置审核状态；
            //2、注意调用AttachmentService转换临时附件；
            //3、需要触发的事件参见《设计说明书-日志》
            //4、草稿的审核状态为待审核；
            //5、转载的日志还需要为原日志转载数+1（调用计数服务）；

            EventBus<BlogThread>.Instance().OnBefore(blogThread, new CommonEventArgs(EventOperationType.Instance().Create()));

            //设置审核状态，草稿的审核状态为待审核
            if (blogThread.IsDraft)
            {
                blogThread.AuditStatus = AuditStatus.Pending;
            }
            else
            {
                AuditService auditService = new AuditService();
                auditService.ChangeAuditStatusForCreate(blogThread.UserId, blogThread);
            }

            long threadId = 0;
            long.TryParse(blogThreadRepository.Insert(blogThread).ToString(), out threadId);

            if (threadId > 0)
            {
                //转换临时附件
                AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().BlogThread());
                attachmentService.ToggleTemporaryAttachments(blogThread.OwnerId, TenantTypeIds.Instance().BlogThread(), blogThread.ThreadId);

                //原日志转载数+1
                if (blogThread.IsReproduced)
                {
                    CountService countService = new CountService(TenantTypeIds.Instance().BlogThread());
                    countService.ChangeCount(CountTypes.Instance().ReproduceCount(), blogThread.OriginalThreadId, blogThread.OwnerId, 1, true);
                }

                //用户内容计数+1
                if (!blogThread.IsDraft)
                {
                    OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                    ownerDataService.Change(blogThread.UserId, OwnerDataKeys.Instance().ThreadCount(), 1);
                }

                AtUserService atUserService = new AtUserService(TenantTypeIds.Instance().BlogThread());
                atUserService.ResolveBodyForEdit(blogThread.GetBody(), blogThread.UserId, blogThread.ThreadId);

                //设置隐私状态
                UpdatePrivacySettings(blogThread, privacyStatus1, privacyStatus2);

                EventBus<BlogThread>.Instance().OnAfter(blogThread, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<BlogThread, AuditEventArgs>.Instance().OnAfter(blogThread, new AuditEventArgs(null, blogThread.AuditStatus));

                return true;
            }

            return false;
        }

        #region 私有方法

        /// <summary>
        /// 设置隐私状态
        /// </summary>
        private void UpdatePrivacySettings(BlogThread blogThread, string privacyStatus1, string privacyStatus2)
        {
            Dictionary<int, IEnumerable<ContentPrivacySpecifyObject>> privacySpecifyObjects = Utility.GetContentPrivacySpecifyObjects(privacyStatus1, privacyStatus2, ((IPrivacyable)blogThread).TenantTypeId, blogThread.ThreadId);

            if (privacySpecifyObjects.Count > 0)
            {
                new ContentPrivacyService().UpdatePrivacySettings(blogThread, privacySpecifyObjects);
            }

        }

        #endregion

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <param name="blogThread">日志实体</param>
        /// <param name="changeAuditStatusForUpdate">是否改变审核状态</param>
        public void Update(BlogThread blogThread, bool changeAuditStatusForUpdate = true, string privacyStatus1 = null, string privacyStatus2 = null)
        {
            //done:mazq,by zhengw:更新时，审核状态也不需要更新吧

            //设计要点
            //1、需要触发的事件参见《设计说明书-日志》；
            //2、Repository更新时不处理：IsEssential；

            EventBus<BlogThread>.Instance().OnBefore(blogThread, new CommonEventArgs(EventOperationType.Instance().Update()));
            AuditStatus prevAuditStatus = blogThread.AuditStatus;

            //设置审核状态
            if (changeAuditStatusForUpdate)
            {
                AuditService auditService = new AuditService();
                auditService.ChangeAuditStatusForUpdate(blogThread.UserId, blogThread);
            }

            blogThreadRepository.Update(blogThread);

            //设置隐私状态
            UpdatePrivacySettings(blogThread, privacyStatus1, privacyStatus2);

            EventBus<BlogThread>.Instance().OnAfter(blogThread, new CommonEventArgs(EventOperationType.Instance().Update()));
            EventBus<BlogThread, AuditEventArgs>.Instance().OnAfter(blogThread, new AuditEventArgs(prevAuditStatus, blogThread.AuditStatus));

        }

        /// <summary>
        /// 加精/取消精华
        /// </summary>
        /// <param name="threadId">日志Id</param>
        /// <param name="isEssential">是否设为精华</param>
        public void SetEssential(long threadId, bool isEssential)
        {
            //设计要点
            //1、精华状态未变化不用进行任何操作；
            //2、需要触发的事件参见《设计说明书-日志》；

            BlogThread blogThread = blogThreadRepository.Get(threadId);
            //done:jiangshl,by zhengw:需要考虑blogThread为null的情况

            if (blogThread == null)
            {
                return;
            }

            if (blogThread.IsEssential == isEssential)
            {
                return;
            }

            blogThreadRepository.SetEssential(threadId, isEssential);

            string operationType = isEssential ? EventOperationType.Instance().SetEssential() : EventOperationType.Instance().CancelEssential();
            EventBus<BlogThread>.Instance().OnAfter(blogThread, new CommonEventArgs(operationType));

        }

        /// <summary>
        /// 批量加精/取消精华
        /// </summary>
        /// <param name="threadIds">待处理的日志Id列表</param>
        /// <param name="isEssential">是否设为精华</param>
        public void SetEssential(IEnumerable<long> threadIds, bool isEssential)
        {
            //设计要点
            //1、精华状态未变化不用进行任何操作；
            //2、需要触发的事件参见《设计说明书-日志》；
            foreach (var threadId in threadIds)
            {
                SetEssential(threadId, isEssential);
            }
        }

        /// <summary>
        /// 置顶/取消置顶
        /// </summary>
        /// <param name="threadId">日志Id</param>
        /// <param name="isSticky">是否置顶</param>
        public void SetSticky(long threadId, bool isSticky)
        {
            //设计要点
            //1、置顶状态未变化不用进行任何操作；
            //2、需要触发的事件参见《设计说明书-日志》；

            BlogThread blogThread = blogThreadRepository.Get(threadId);
            if (blogThread.IsSticky == isSticky)
            {
                return;
            }

            blogThread.IsSticky = isSticky;
            this.Update(blogThread, false);

            string operationType = isSticky ? EventOperationType.Instance().SetSticky() : EventOperationType.Instance().CancelSticky();
            EventBus<BlogThread>.Instance().OnAfter(blogThread, new CommonEventArgs(operationType));
        }

        /// <summary>
        /// 批准/不批准日志
        /// </summary>
        /// <param name="threadId">待被更新的主题帖Id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void Approve(long threadId, bool isApproved)
        {
            //设计要点
            //1、审核状态未变化不用进行任何操作；
            //2、需要触发的事件参见《设计说明书-日志》；

            BlogThread blogThread = blogThreadRepository.Get(threadId);

            AuditStatus newAuditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            if (blogThread.AuditStatus == newAuditStatus)
            {
                return;
            }

            AuditStatus oldAuditStatus = blogThread.AuditStatus;
            blogThread.AuditStatus = newAuditStatus;
            this.Update(blogThread, false);

            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            EventBus<BlogThread>.Instance().OnAfter(blogThread, new CommonEventArgs(operationType));
            EventBus<BlogThread, AuditEventArgs>.Instance().OnAfter(blogThread, new AuditEventArgs(oldAuditStatus, newAuditStatus));
        }

        /// <summary>
        /// 批量批准/不批准日志
        /// </summary>
        /// <param name="threadIds">待处理的日志Id列表</param>
        /// <param name="isApproved">是否通过审核</param>
        public void Approve(IEnumerable<long> threadIds, bool isApproved)
        {
            //设计要点
            //1、审核状态未变化不用进行任何操作；
            //2、需要触发的事件参见《设计说明书-日志》；

            foreach (var threadId in threadIds)
            {
                Approve(threadId, isApproved);
            }
        }

        /// <summary>
        /// 删除日志
        /// </summary>
        /// <param name="threadId">日志Id</param>
        public void Delete(long threadId)
        {
            BlogThread blogThread = blogThreadRepository.Get(threadId);
            if (blogThread == null)
            {
                return;
            }
            Delete(blogThread);
        }

        /// <summary>
        /// 删除日志
        /// </summary>
        /// <param name="blogThread">日志实体</param>
        public void Delete(BlogThread blogThread)
        {
            //删除日志可能会影响：
            //1、站点分类，在EventModule中处理
            //2、拥有者分类，在EventModule中处理
            //3、日志对应的动态（可在EventModule中处理，可参考贴吧）
            //4、其它数据的删除由各模块自动处理
            //需要触发的事件参见《设计说明书-日志》；

            if (blogThread == null)
            {
                return;
            }

            CategoryService categoryService = new CategoryService();

            var sender = new CommentService().GetCommentedObjectComments(blogThread.ThreadId);
            //删除用户分类关联
            categoryService.ClearCategoriesFromItem(blogThread.ThreadId, blogThread.OwnerId, TenantTypeIds.Instance().BlogThread());

            //删除站点分类关联（投稿到）
            categoryService.ClearCategoriesFromItem(blogThread.ThreadId, 0, TenantTypeIds.Instance().BlogThread());

            //删除标签关联
            TagService tagService = new TagService(TenantTypeIds.Instance().BlogThread());
            tagService.ClearTagsFromItem(blogThread.ThreadId, blogThread.OwnerId);

            //删除推荐记录
            RecommendService recommendService = new RecommendService();
            recommendService.Delete(blogThread.ThreadId, blogThread.TenantTypeId);

            //删除订阅记录todo:libsh
            //SubscribeService subscribeService = new SubscribeService(TenantTypeIds.Instance().BlogThread());


            //用户内容计数-1
            if (!blogThread.IsDraft)
            {
                OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                ownerDataService.Change(blogThread.UserId, OwnerDataKeys.Instance().ThreadCount(), -1);
            }

            EventBus<BlogThread>.Instance().OnBefore(blogThread, new CommonEventArgs(EventOperationType.Instance().Delete()));

            blogThreadRepository.Delete(blogThread);

            EventBus<BlogThread>.Instance().OnAfter(blogThread, new CommonEventArgs(EventOperationType.Instance().Delete()));
            EventBus<BlogThread, AuditEventArgs>.Instance().OnAfter(blogThread, new AuditEventArgs(blogThread.AuditStatus, null));
            if (sender != null)
                EventBus<Comment>.Instance().OnBatchAfter(sender, new CommonEventArgs(EventOperationType.Instance().Delete()));
        }

        /// <summary>
        /// 删除用户时处理所有日志相关的数据（删除用户时使用）
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="takeOverUserName">用于接管删除用户时不能删除的内容(例如：用户创建的群组)</param>
        /// <param name="takeOverAll">是否接管被删除用户的所有内容</param>
        /// <remarks>接管被删除用户的所有内容</remarks>
        public void DeleteUser(long userId, string takeOverUserName, bool takeOverAll)
        {
            //设计要点
            //1、参见《设计说明书-日志》的“删除用户时的业务逻辑”；

            //takeOverAll为true，指定其他用户接管数据
            if (takeOverAll)
            {
                IUserService userService = DIContainer.Resolve<IUserService>();
                User takeOverUser = userService.GetFullUser(takeOverUserName);
                blogThreadRepository.TakeOver(userId, takeOverUser);
            }
            //takeOverAll为false，调用Delete方法逐条删除用户的日志，积分、索引、动态等的删除由EventModule处理，评论、顶踩、附件等相关数据由系统自动删除
            else
            {
                //done:jiangshl,by zhengw:不用这么麻烦吧？直接全部取出来遍历删除掉不就行了。

                int pageSize = 100;     //批量删除，每次删100条
                int pageIndex = 1;
                int pageCount = 1;
                do
                {
                    PagingDataSet<BlogThread> blogs = GetOwnerThreads(TenantTypeIds.Instance().User(), userId, true, false, null, null, false, pageSize, pageIndex);
                    foreach (BlogThread blog in blogs)
                    {
                        Delete(blog);
                    }
                    pageCount = blogs.PageCount;
                    pageIndex++;
                } while (pageIndex <= pageCount);
            }

        }

        #endregion


        #region 获取日志

        /// <summary>
        /// 获取日志
        /// </summary>
        /// <param name="threadId">日志Id</param>
        /// <returns>日志实体</returns>
        public BlogThread Get(long threadId)
        {
            //设计要点 
            //1、BlogThread需要有上一篇、下一篇 的ThreadId（参考贴吧）
            //2、用于显示的；
            //3、需要触发的事件参见《设计说明书-日志》
            //4、草稿的审核状态为待审核；

            return blogThreadRepository.Get(threadId);
        }

        /// <summary>
        /// 上一日志ThreadId
        /// </summary>
        /// <param name="blogThread">当前日志实体</param>
        /// <returns>上一日志ThreadId</returns>
        public long GetPrevThreadId(BlogThread blogThread)
        {
            return blogThreadRepository.GetPrevThreadId(blogThread);
        }

        /// <summary>
        /// 下一日志ThreadId
        /// </summary>
        /// <param name="blogThread">当前日志实体</param>
        /// <returns>下一日志ThreadId</returns>
        public long GetNextThreadId(BlogThread blogThread)
        {
            return blogThreadRepository.GetNextThreadId(blogThread);
        }

        /// <summary>
        /// 获取Owner的日志
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="ignorePrivate">是否忽略隐私状态为不公开的（作者或管理员查看时忽略隐私状态）</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isSticky">是否置顶的排在前面</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码</param>        
        /// <returns>日志分页列表</returns>
        public PagingDataSet<BlogThread> GetOwnerThreads(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate, long? categoryId, string tagName, bool isSticky = false, int pageSize = 20, int pageIndex = 1)
        {
            //设计要点：
            //1、ignoreAudit = false时，需要根据应用设置过滤审核状态，可能用到的代码 new AuditService().GetPubliclyAuditStatus(BlogConfig.Instance().ApplicationId); 可参考BarThreadRepository.Gets()中的PubliclyAuditStatus处理
            //2、置顶日志靠前显示
            //3、排除草稿

            return blogThreadRepository.GetOwnerThreads(tenantTypeId, ownerId, ignoreAudit, ignorePrivate, categoryId, tagName, isSticky, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取Owner的草稿
        /// </summary>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <param name="ownerId">所有者id</param>
        /// <returns>日志列表</returns>
        public IEnumerable<BlogThread> GetDraftThreads(string tenantTypeId, long ownerId)
        {
            //获取草稿
            //无需缓存

            return blogThreadRepository.GetDraftThreads(tenantTypeId, ownerId);
        }

        /// <summary>
        /// 获取归档项目
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="ignoreAudit">忽略审核状态</param>
        /// <param name="ignorePrivate">忽略隐私状态为私有的记录</param>
        /// <returns>归档项目列表</returns>
        public IEnumerable<ArchiveItem> GetArchiveItems(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate)
        {
            //设计要点：
            //1、缓存周期：常用对象集合，不用维护即时性

            return blogThreadRepository.GetArchiveItems(tenantTypeId, ownerId, ignoreAudit, ignorePrivate);
        }

        /// <summary>
        /// 获取存档的日志分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">ownerId</param>
        /// <param name="ignoreAudit">忽略审核状态</param>
        /// <param name="ignorePrivate">忽略隐私状态为私有的记录</param>
        /// <param name="archivePeriod">归档阶段</param>
        /// <param name="archiveItem">归档统计项目</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        public PagingDataSet<BlogThread> GetsForArchive(string tenantTypeId, long ownerId, bool ignoreAudit, bool ignorePrivate, ArchivePeriod archivePeriod, ArchiveItem archiveItem, int pageSize = 20, int pageIndex = 1)
        {
            //只获取可对外显示审核状态的主题帖
            //排除草稿
            //缓存周期：对象集合，不用维护即时性

            return blogThreadRepository.GetsForArchive(tenantTypeId, ownerId, ignoreAudit, ignorePrivate, archivePeriod, archiveItem, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取日志的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">日志排序依据</param>
        /// <returns>日志列表</returns>
        public IEnumerable<BlogThread> GetTops(string tenantTypeId, int topNumber, long? categoryId, bool? isEssential, SortBy_BlogThread sortBy)
        {
            //只获取可对外显示审核状态的主题帖
            //缓存周期：常用对象集合，不用维护即时性
            //排除草稿
            var some = blogThreadRepository.GetTops(tenantTypeId, topNumber, categoryId, isEssential, sortBy);
            return blogThreadRepository.GetTops(tenantTypeId, topNumber, categoryId, isEssential, sortBy);
        }

        /// <summary>
        /// 获取日志排行分页集合
        /// </summary>
        /// <remarks>rss订阅也使用方法</remarks>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="ignoreAudit">忽略审核状态</param>
        /// <param name="ignorePrivate">忽略隐私状态为私有的记录</param>
        /// <param name="categoryId">类别Id</param>
        /// <param name="tagName">标签名称</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        public PagingDataSet<BlogThread> Gets(string tenantTypeId, long? ownerId, bool ignoreAudit, bool ignorePrivate, long? categoryId, string tagName, bool? isEssential, SortBy_BlogThread sortBy = SortBy_BlogThread.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            //只获取可对外显示审核状态的主题帖
            //排除草稿
            //缓存周期：常用对象集合，不用维护即时性
            //最多获取SecondaryMaxRecords条记录

            return blogThreadRepository.Gets(tenantTypeId, ownerId, ignoreAudit, ignorePrivate, categoryId, tagName, isEssential, sortBy, pageSize, pageIndex);
        }

        /// 获取日志排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="categoryId">站点类别Id</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="ownerId">所有者id</param>
        /// <param name="subjectKeywords">标题关键字</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>日志分页列表</returns>
        public PagingDataSet<BlogThread> GetsForAdmin(string tenantTypeId, AuditStatus? auditStatus, long? categoryId, bool? isEssential, long? ownerId, string subjectKeywords, int pageSize = 20, int pageIndex = 1)
        {
            //无需缓存
            //最多获取SecondaryMaxRecords条记录
            return blogThreadRepository.GetsForAdmin(tenantTypeId, auditStatus, categoryId, isEssential, ownerId, subjectKeywords, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据日志Id集合组装日志实体集合
        /// </summary>
        /// <param name="threadIds">日志Id集合</param>
        /// <returns>日志实体集合</returns>
        public IEnumerable<BlogThread> GetBlogThreads(IEnumerable<long> threadIds)
        {
            return blogThreadRepository.PopulateEntitiesByEntityIds<long>(threadIds);
        }

        /// <summary>
        /// 获取日志管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>日志管理数据</returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            return blogThreadRepository.GetManageableDatas(tenantTypeId);
        }

        /// <summary>
        /// 获取日志统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>日志统计数据</returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            return blogThreadRepository.GetStatisticDatas();
        }

        #endregion

    }
}