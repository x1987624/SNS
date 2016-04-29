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
using Tunynet.Common;
using Tunynet.Events;
using Spacebuilder.Common;

namespace Spacebuilder.Bar
{

    /// <summary>
    /// 主题帖业务逻辑类
    /// </summary>
    public class BarThreadService
    {

        private IBarThreadRepository barThreadRepository = null;
        private AuditService auditService = new AuditService();
        /// <summary>
        /// 构造函数
        /// </summary>
        public BarThreadService()
            : this(new BarThreadRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="barThreadRepository"></param>
        public BarThreadService(IBarThreadRepository barThreadRepository)
        {
            this.barThreadRepository = barThreadRepository;
        }

        #region 维护主题帖

        /// <summary>
        /// 创建主题帖
        /// </summary>
        /// <param name="thread">主题帖</param>
        public bool Create(BarThread thread)
        {
            BarSectionService barSectionService = new BarSectionService();
            EventBus<BarThread>.Instance().OnBefore(thread, new CommonEventArgs(EventOperationType.Instance().Create()));
            //设置审核状态
            auditService.ChangeAuditStatusForCreate(thread.UserId, thread);
            long id = 0;
            long.TryParse(barThreadRepository.Insert(thread).ToString(), out id);

            if (id > 0)
            {
                new AttachmentService(TenantTypeIds.Instance().BarThread()).ToggleTemporaryAttachments(thread.UserId, TenantTypeIds.Instance().BarThread(), id);
                BarSection barSection = barSectionService.Get(thread.SectionId);
                if (barSection != null)
                {
                    //计数
                    CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                    countService.ChangeCount(CountTypes.Instance().ThreadCount(), barSection.SectionId, barSection.UserId, 1, true);
                    countService.ChangeCount(CountTypes.Instance().ThreadAndPostCount(), barSection.SectionId, barSection.UserId, 1, true);
                    if (thread.TenantTypeId == TenantTypeIds.Instance().Group())
                    {
                        //群组内容计数+1
                        OwnerDataService groupOwnerDataService = new OwnerDataService(TenantTypeIds.Instance().Group());
                        groupOwnerDataService.Change(thread.SectionId, OwnerDataKeys.Instance().ThreadCount(), 1);
                    }
                }
                if (thread.TenantTypeId == TenantTypeIds.Instance().Bar())
                {
                    //用户内容计数+1
                    OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                    ownerDataService.Change(thread.UserId, OwnerDataKeys.Instance().ThreadCount(), 1);
                }
                AtUserService atUserService = new AtUserService(TenantTypeIds.Instance().BarThread());
                atUserService.ResolveBodyForEdit(thread.GetBody(), thread.UserId, thread.ThreadId);

                EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<BarThread, AuditEventArgs>.Instance().OnAfter(thread, new AuditEventArgs(null, thread.AuditStatus));
            }
            return id > 0;
        }

        /// <summary>
        /// 更新主题帖
        /// </summary>
        /// <param name="thread">主题帖</param>
        /// <param name="userId">当前操作人</param>
        public void Update(BarThread thread, long userId)
        {
            EventBus<BarThread>.Instance().OnBefore(thread, new CommonEventArgs(EventOperationType.Instance().Update()));
            AuditStatus oldAuditStatus = thread.AuditStatus;

            auditService.ChangeAuditStatusForUpdate(userId, thread);
            barThreadRepository.Update(thread);

            EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(EventOperationType.Instance().Update()));
            EventBus<BarThread, AuditEventArgs>.Instance().OnAfter(thread, new AuditEventArgs(oldAuditStatus, thread.AuditStatus));
        }

        /// <summary>
        /// 移动帖子
        /// </summary>
        /// <param name="threadId">要移动帖子的ThreadId</param>
        /// <param name="moveToSectionId">转移到帖吧的SectionId</param>
        public void MoveThread(long threadId, long moveToSectionId)
        {
            BarThread thread = barThreadRepository.Get(threadId);
            if (thread.SectionId == moveToSectionId)
                return;
            long oldSectionId = thread.SectionId;
            var barSectionService = new BarSectionService();
            var oldSection = barSectionService.Get(oldSectionId);
            if (oldSection == null)
                return;
            var newSection = barSectionService.Get(moveToSectionId);
            if (newSection == null)
                return;
            barThreadRepository.MoveThread(threadId, moveToSectionId);

            CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
            countService.ChangeCount(CountTypes.Instance().ThreadCount(), oldSection.SectionId, oldSection.UserId, -1, true);
            countService.ChangeCount(CountTypes.Instance().ThreadAndPostCount(), oldSection.SectionId, oldSection.UserId, -1, true);

            countService.ChangeCount(CountTypes.Instance().ThreadCount(), newSection.SectionId, newSection.UserId, 1, true);
            countService.ChangeCount(CountTypes.Instance().ThreadAndPostCount(), newSection.SectionId, newSection.UserId, 1, true);
        }

        /// <summary>
        /// 设置精华
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <param name="isEssential">是否设为精华</param>
        public void SetEssential(long threadId, bool isEssential)
        {
            BarThread thread = barThreadRepository.Get(threadId);
            if (thread.IsEssential == isEssential)
                return;
            thread.IsEssential = isEssential;
            barThreadRepository.Update(thread);



            string operationType = isEssential ? EventOperationType.Instance().SetEssential() : EventOperationType.Instance().CancelEssential();
            EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(operationType));
        }

        /// <summary>
        /// 设置置顶
        /// </summary>
        /// <param name="threadId">待操作的主题帖Id</param>
        /// <param name="isSticky">待更新至的置顶状态</param>
        /// <param name="stickyDate">设为置顶时，需设置置顶时间；取消置顶时，不需要设置此参数</param>
        public void SetSticky(long threadId, bool isSticky, DateTime? stickyDate = null)
        {
            BarThread thread = barThreadRepository.Get(threadId);
            if (thread.IsSticky == isSticky)
                return;
            thread.IsSticky = isSticky;
            //设为置顶时，设置置顶时间
            if (isSticky)
                thread.StickyDate = stickyDate ?? DateTime.UtcNow.AddDays(7);
            barThreadRepository.Update(thread);



            string operationType = isSticky ? EventOperationType.Instance().SetSticky() : EventOperationType.Instance().CancelSticky();
            EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(operationType));
        }

        /// <summary>
        /// 更新审核状态
        /// </summary>
        /// <param name="threadId">待被更新的主题帖Id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void UpdateAuditStatus(long threadId, bool isApproved)
        {
            BarThread thread = barThreadRepository.Get(threadId);
            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            if (thread.AuditStatus == auditStatus)
                return;
            AuditStatus oldAuditStatus = thread.AuditStatus;
            thread.AuditStatus = auditStatus;
            barThreadRepository.Update(thread);
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            
            EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(operationType));
            EventBus<BarThread, AuditEventArgs>.Instance().OnAfter(thread, new AuditEventArgs(oldAuditStatus, thread.AuditStatus));
        }




        /// <summary>
        /// 使置顶到期的的帖子恢复至未置顶状态
        /// </summary>
        public void ExpireStickyThreads()
        {
            barThreadRepository.ExpireStickyThreads();
        }

        /// <summary>
        /// 批量更新审核状态
        /// </summary>
        /// <param name="threadIds">待被更新的主题帖Id集合</param>
        /// <param name="isApproved">是否通过审核</param>
        public void BatchUpdateAuditStatus(IEnumerable<long> threadIds, bool isApproved)
        {
            IEnumerable<BarThread> threads = GetBarThreads(threadIds);
            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            foreach (var thread in threads)
            {
                if (thread.AuditStatus == auditStatus)
                    continue;
                AuditStatus oldAuditStatus = thread.AuditStatus;
                thread.AuditStatus = auditStatus;
                barThreadRepository.Update(thread);



                EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(operationType));
                EventBus<BarThread, AuditEventArgs>.Instance().OnAfter(thread, new AuditEventArgs(oldAuditStatus, thread.AuditStatus));
            }
        }

        /// <summary>
        /// 批量设置精华
        /// </summary>
        /// <param name="threadIds">待被更新的主题帖Id集合</param>
        /// <param name="isEssential">是否设为精华</param>
        public void BatchSetEssential(IEnumerable<long> threadIds, bool isEssential)
        {
            IEnumerable<BarThread> threads = GetBarThreads(threadIds);
            string operationType = isEssential ? EventOperationType.Instance().SetEssential() : EventOperationType.Instance().CancelEssential();
            foreach (var thread in threads)
            {
                if (thread.IsEssential == isEssential)
                    continue;
                thread.IsEssential = isEssential;
                barThreadRepository.Update(thread);


                EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(operationType));
            }
        }

        /// <summary>
        /// 批量设置置顶
        /// </summary>
        /// <param name="threadIds">待被更新的主题帖Id集合</param>
        /// <param name="isSticky">是否置顶</param>
        /// <param name="stickyDate">置顶时间</param>
        public void BatchSetSticky(IEnumerable<long> threadIds, bool isSticky, DateTime stickyDate)
        {
            IEnumerable<BarThread> threads = GetBarThreads(threadIds);
            string operationType = isSticky ? EventOperationType.Instance().SetSticky() : EventOperationType.Instance().CancelSticky();
            foreach (var thread in threads)
            {
                if (thread.IsSticky == isSticky)
                    continue;

                thread.IsSticky = isSticky;
                //设为置顶时，设置置顶时间
                if (isSticky)
                    thread.StickyDate = stickyDate;
                barThreadRepository.Update(thread);


                EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(operationType));
            }
        }

        /// <summary>
        /// 删除主题帖
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        public void Delete(long threadId)
        {
            BarThread thread = barThreadRepository.Get(threadId);
            if (thread == null)
                return;

            EventBus<BarThread>.Instance().OnBefore(thread, new CommonEventArgs(EventOperationType.Instance().Delete()));

            BarSectionService barSectionService = new BarSectionService();
            BarSection barSection = barSectionService.Get(thread.SectionId);
            if (barSection != null)
            {
                //帖子标签
                TagService tagService = new TagService(TenantTypeIds.Instance().BarThread());
                tagService.ClearTagsFromItem(threadId, barSection.SectionId);

                //帖子分类
                CategoryService categoryService = new CategoryService();
                categoryService.ClearCategoriesFromItem(threadId, barSection.SectionId, TenantTypeIds.Instance().BarThread());
            }

            //删除回帖
            BarPostService barPostService = new BarPostService();
            barPostService.DeletesByThreadId(threadId);

            //删除推荐记录
            RecommendService recommendService = new RecommendService();
            recommendService.Delete(threadId, TenantTypeIds.Instance().BarThread());

            int affectCount = barThreadRepository.Delete(thread);

            if (affectCount > 0)
            {
                //更新帖吧的计数
                CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                countService.ChangeCount(CountTypes.Instance().ThreadCount(), barSection.SectionId, barSection.UserId, -1, true);
                countService.ChangeCount(CountTypes.Instance().ThreadAndPostCount(), barSection.SectionId, barSection.UserId, -1, true);

                if (thread.TenantTypeId == TenantTypeIds.Instance().Group())
                {
                    //群组内容计数-1
                    OwnerDataService groupOwnerDataService = new OwnerDataService(TenantTypeIds.Instance().Group());
                    groupOwnerDataService.Change(thread.SectionId, OwnerDataKeys.Instance().ThreadCount(), -1);
                }
                else if (thread.TenantTypeId == TenantTypeIds.Instance().Bar())
                {
                    //用户内容计数-1
                    OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                    ownerDataService.Change(thread.UserId, OwnerDataKeys.Instance().ThreadCount(), -1);
                }
                EventBus<BarThread>.Instance().OnAfter(thread, new CommonEventArgs(EventOperationType.Instance().Delete()));
                EventBus<BarThread, AuditEventArgs>.Instance().OnAfter(thread, new AuditEventArgs(thread.AuditStatus, null));
            }


            //BarThread删除可能影响的：
            //1、附件 （看到了）
            //2、BarPost（看到了）
            //3、BarRating（看到了）
            //4、相关计数对象（看到了）
            //5、用户在应用中的数据（看到了）
            //6、@用户（看到了）


        }

        /// <summary>
        /// 删除帖吧下的所有帖子
        /// </summary>
        /// <param name="sectionId"></param>
        public void DeletesBySectionId(long sectionId)
        {
            IEnumerable<BarThread> barThreads = barThreadRepository.GetAllThreadsOfSection(sectionId);
            foreach (var barThread in barThreads)
            {
                Delete(barThread.ThreadId);
            }
        }

        /// <summary>
        /// 删除用户下的所有帖子
        /// </summary>
        /// <param name="sectionId"></param>
        private void DeletesByUserId(long userId)
        {
            IEnumerable<BarThread> barThreads = barThreadRepository.GetAllThreadsOfUser(userId);
            foreach (var barThread in barThreads)
            {
                Delete(barThread.ThreadId);
            }
        }

        /// <summary>
        /// 删除用户记录（删除用户时使用）
        /// </summary>
        /// <param name="userId">被删除用户</param>
        /// <param name="takeOverUserName">接管用户名</param>
        /// <param name="takeOverAll">是否接管被删除用户的所有内容</param>
        public void DeleteUser(long userId, string takeOverUserName, bool takeOverAll)
        {
            long takeOverUserId = UserIdToUserNameDictionary.GetUserId(takeOverUserName);
            IUserService userService = DIContainer.Resolve<IUserService>();
            User takeOver = userService.GetFullUser(takeOverUserId);
            BarSectionService barSectionService = new BarSectionService();
            BarThreadService barThreadService = new BarThreadService();
            BarPostService barPostService = new BarPostService();

            //删除用户时，不删除贴吧，把贴吧转让，如果没有指定转让人，那就转给网站初始管理员
            if (takeOver == null)
            {
                takeOverUserId = new SystemDataService().GetLong("Founder");
                takeOver = userService.GetFullUser(takeOverUserId);
            }

            barThreadRepository.DeleteUser(userId, takeOver, takeOverAll);
            if (takeOver != null)
            {
                if (!takeOverAll)
                {
                    barThreadService.DeletesByUserId(userId);
                    barPostService.DeletesByUserId(userId);
                }
            }
            //else
            //{
            //    barSectionService.DeletesByUserId(userId);
            //    barThreadService.DeletesByUserId(userId);
            //    barPostService.DeletesByUserId(userId);
            //}
        }

        #endregion

        #region 获取主题帖

        /// <summary>
        /// 获取单个主题帖实体
        /// </summary>
        /// <param name="threadId">主题帖Id</param>
        /// <returns>主题帖</returns>
        public BarThread Get(long threadId)
        {



            return barThreadRepository.Get(threadId);
        }

        /// <summary>
        /// 获取用户的主题帖分页集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="ignoreAudit">是否忽略审核状态（作者或管理员查看时忽略审核状态）</param>
        /// <param name="isPosted">是否是取我回复过的</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns>主题帖列表</returns>
        public PagingDataSet<BarThread> GetUserThreads(string tenantTypeId, long userId, bool ignoreAudit, bool isPosted = false, int pageIndex = 1, long? sectionId = null)
        {
            //不必筛选审核状态
            //缓存周期：对象集合，需要维护即时性
            //排序：发布时间（倒序）
            return barThreadRepository.GetUserThreads(tenantTypeId, userId, ignoreAudit, isPosted, pageIndex, sectionId);
        }

        /// <summary>
        /// 获取主题帖的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="isEssential">是否为精华帖</param>
        /// <param name="sortBy">主题帖排序依据</param>
        /// <returns></returns>
        public IEnumerable<BarThread> GetTops(string tenantTypeId, int topNumber, bool? isEssential = null, SortBy_BarThread sortBy = SortBy_BarThread.StageHitTimes)
        {






            //只获取可对外显示审核状态的主题帖
            //缓存周期：常用对象集合，不用维护即时性
            return barThreadRepository.GetTops(tenantTypeId, topNumber, isEssential, sortBy);
        }

        /// <summary>
        /// 获取群组贴吧的排行数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="topNumber">前多少条</param>
        /// <param name="sortBy">主题帖排序依据</param>
        /// <returns></returns>
        public IEnumerable<BarThread> GetTopsThreadOfGroup(string tenantTypeId, int topNumber, bool? isEssential = null, SortBy_BarThread sortBy = SortBy_BarThread.StageHitTimes)
        {
            return barThreadRepository.GetTopsThreadOfGroup(tenantTypeId, topNumber, isEssential, sortBy);
        }


        /// <summary>
        /// 根据标签名获取主题帖排行分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="tageName">标签名</param>
        /// <param name="isEssential">是否为精华帖</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>主题帖列表</returns>
        public PagingDataSet<BarThread> Gets(string tenantTypeId, string tageName, bool? isEssential = null, SortBy_BarThread sortBy = SortBy_BarThread.StageHitTimes, int pageIndex = 1)
        {



            //只获取可对外显示审核状态的主题帖
            //缓存周期：对象集合，不用维护即时性
            return barThreadRepository.Gets(tenantTypeId, tageName, isEssential, sortBy, pageIndex);
        }

        /// <summary>
        /// 根据帖吧获取主题帖分页集合
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="isEssential">是否为精华帖</param>
        /// <param name="sortBy">帖子排序依据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>主题帖列表</returns>
        public PagingDataSet<BarThread> Gets(long sectionId, long? categoryId = null, bool? isEssential = null, SortBy_BarThread sortBy = SortBy_BarThread.LastModified_Desc, int pageIndex = 1)
        {
            //只获取可对外显示审核状态的主题帖
            //缓存周期：对象集合，需要维护即时性



            return barThreadRepository.Gets(sectionId, categoryId, isEssential, sortBy, pageIndex);
        }

        /// <summary>
        /// 帖子管理时查询帖子分页集合
        /// </summary>
        /// <param name="query">帖子查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖子分页集合</returns>
        public PagingDataSet<BarThread> Gets(string tenantTypeId, BarThreadQuery query, int pageSize, int pageIndex)
        {
            //当SubjectKeyword、StartDate、EndDate为null时，进行缓存
            //当SectionId不为null时，使用分区版本，分区名为：SectionId，否则使用全局版本
            //缓存周期：对象集合，需要维护即时性
            //使用用户选择器设置query.UserId参数
            return barThreadRepository.Gets(tenantTypeId, query, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据主题帖Id集合组装主题帖实体集合
        /// </summary>
        /// <param name="threadIds">主题帖Id集合</param>
        /// <returns>主题帖实体集合</returns>
        public IEnumerable<BarThread> GetBarThreads(IEnumerable<long> threadIds)
        {
            return barThreadRepository.PopulateEntitiesByEntityIds<long>(threadIds);
        }

        #endregion


        /// <summary>
        /// 获取帖子管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            return barThreadRepository.GetManageableDatas(tenantTypeId);
        }

        /// <summary>
        /// 获取帖子统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            return barThreadRepository.GetStatisticDatas();
        }
    }
}