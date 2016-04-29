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
using Spacebuilder.Common;
using Tunynet.Common;
using System.Drawing;
using System.IO;
using Tunynet.Events;
using Tunynet.FileStore;
using Tunynet.Imaging;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖吧业务逻辑类
    /// </summary>
    public class BarSectionService
    {
        private IBarSectionRepository barSectionRepository = null;
        private AuditService auditService = new AuditService();

        /// <summary>
        /// 构造函数
        /// </summary>
        public BarSectionService()
            : this(new BarSectionRepository())
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="barSectionRepository"></param>
        public BarSectionService(IBarSectionRepository barSectionRepository)
        {
            this.barSectionRepository = barSectionRepository;
        }

        #region 维护帖吧

        /// <summary>
        /// 创建帖吧
        /// </summary>
        /// <param name="section">帖吧</param>
        /// <param name="userId">当前操作人</param>
        /// <param name="managerIds">管理员用户Id</param>
        /// <param name="logoFile">帖吧标识图</param>
        /// <returns>是否创建成功</returns>
        public bool Create(BarSection section, long userId, IEnumerable<long> managerIds, Stream logoFile)
        {
            EventBus<BarSection>.Instance().OnBefore(section, new CommonEventArgs(EventOperationType.Instance().Create()));
            //设置审核状态
            auditService.ChangeAuditStatusForCreate(userId, section);

            if (!(section.SectionId > 0))
                section.SectionId = IdGenerator.Next();

            long id = 0;
            long.TryParse(barSectionRepository.Insert(section).ToString(), out id);

            if (id > 0)
            {
                if (managerIds != null && managerIds.Count() > 0)
                {
                    List<long> mangagerIds_list = managerIds.ToList();
                    mangagerIds_list.Remove(section.UserId);
                    managerIds = mangagerIds_list;
                    barSectionRepository.UpdateManagerIds(id, managerIds);
                }
                if (section.TenantTypeId == TenantTypeIds.Instance().Bar())
                {
                    //帖吧主、吧管理员自动关注本帖吧
                    SubscribeService subscribeService = new SubscribeService(TenantTypeIds.Instance().BarSection());
                    int followedCount = 0;
                    bool result = subscribeService.Subscribe(section.SectionId, section.UserId);
                    if (result)
                        followedCount++;
                    if (managerIds != null && managerIds.Count() > 0)
                        foreach (var managerId in managerIds)
                        {
                            result = subscribeService.Subscribe(section.SectionId, managerId);
                            if (result)
                                followedCount++;
                        }
                    //增加帖吧的被关注数
                    CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                    countService.ChangeCount(CountTypes.Instance().FollowedCount(), section.SectionId, section.UserId, followedCount, true);
                }



                //上传Logo
                if (logoFile != null)
                {
                    LogoService logoService = new LogoService(TenantTypeIds.Instance().BarSection());
                    section.LogoImage = logoService.UploadLogo(section.SectionId, logoFile);
                    barSectionRepository.Update(section);
                }
                EventBus<BarSection>.Instance().OnAfter(section, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<BarSection, AuditEventArgs>.Instance().OnAfter(section, new AuditEventArgs(section.AuditStatus, null));
            }
            return id > 0;
        }

        /// <summary>
        /// 更新帖吧
        /// </summary>
        /// <param name="section">帖吧</param>
        /// <param name="userId">当前操作人</param>
        /// <param name="managerIds">管理员用户Id</param>
        /// <param name="sectionedFile">帖吧标识图</param>
        public void Update(BarSection section, long userId, IEnumerable<long> managerIds, Stream sectionedFile)
        {
            EventBus<BarSection>.Instance().OnBefore(section, new CommonEventArgs(EventOperationType.Instance().Update()));



            //上传Logo
            if (sectionedFile != null)
            {
                LogoService logoService = new LogoService(TenantTypeIds.Instance().BarSection());
                section.LogoImage = logoService.UploadLogo(section.SectionId, sectionedFile);

            }

            auditService.ChangeAuditStatusForUpdate(userId, section);
            barSectionRepository.Update(section);

            if (managerIds != null && managerIds.Count() > 0)
            {
                List<long> mangagerIds_list = managerIds.ToList();
                mangagerIds_list.Remove(section.UserId);
                managerIds = mangagerIds_list;
            }
            barSectionRepository.UpdateManagerIds(section.SectionId, managerIds);

            if (section.TenantTypeId == TenantTypeIds.Instance().Bar())
            {
                SubscribeService subscribeService = new SubscribeService(TenantTypeIds.Instance().BarSection());

                //帖吧主、吧管理员自动关注本帖吧
                int followedCount = 0;
                bool result = subscribeService.Subscribe(section.SectionId, section.UserId);
                if (result)
                    followedCount++;
                if (managerIds != null && managerIds.Count() > 0)
                {
                    foreach (var managerId in managerIds)
                    {
                        result = subscribeService.Subscribe(section.SectionId, managerId);
                        if (result)
                            followedCount++;
                    }
                }

                //增加帖吧的被关注数
                CountService countService = new CountService(TenantTypeIds.Instance().BarSection());
                countService.ChangeCount(CountTypes.Instance().FollowedCount(), section.SectionId, section.UserId, followedCount, true);
            }
            EventBus<BarSection>.Instance().OnAfter(section, new CommonEventArgs(EventOperationType.Instance().Update()));
        }

        /// <summary>
        /// 更新帖吧的审核状态
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void UpdateAuditStatus(long sectionId, bool isApproved)
        {
            BarSection section = barSectionRepository.Get(sectionId);
            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            if (section.AuditStatus == auditStatus)
                return;
            AuditStatus oldAuditStatus = section.AuditStatus;
            section.AuditStatus = auditStatus;
            barSectionRepository.Update(section);
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            EventBus<BarSection>.Instance().OnAfter(section, new CommonEventArgs(operationType));
            EventBus<BarSection, AuditEventArgs>.Instance().OnAfter(section, new AuditEventArgs(oldAuditStatus, section.AuditStatus));
        }


        /// <summary>
        /// 批量更新审核状态
        /// </summary>
        /// <param name="sectionIds">待被更新的帖吧Id集合</param>
        /// <param name="isApproved">是否通过审核</param>
        public void BatchUpdateAuditStatus(IEnumerable<long> sectionIds, bool isApproved)
        {
            IEnumerable<BarSection> sections = barSectionRepository.PopulateEntitiesByEntityIds(sectionIds);
            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            foreach (var section in sections)
            {
                if (section.AuditStatus == auditStatus)
                    continue;
                AuditStatus oldAuditStatus = section.AuditStatus;
                section.AuditStatus = auditStatus;
                barSectionRepository.Update(section);
                EventBus<BarSection>.Instance().OnAfter(section, new CommonEventArgs(operationType));
                EventBus<BarSection, AuditEventArgs>.Instance().OnAfter(section, new AuditEventArgs(oldAuditStatus, section.AuditStatus));
            }
        }


        /// <summary>
        /// 删除帖吧
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        public void Delete(long sectionId)
        {
            BarSection section = barSectionRepository.Get(sectionId);
            if (section == null)
                return;
            EventBus<BarSection>.Instance().OnBefore(section, new CommonEventArgs(EventOperationType.Instance().Delete()));

            //帖子
            BarThreadService barThreadService = new BarThreadService();
            barThreadService.DeletesBySectionId(sectionId);

            CategoryService categoryService = new CategoryService();

            //帖吧分类
            categoryService.ClearCategoriesFromItem(sectionId, null, TenantTypeIds.Instance().BarSection());

            //帖子分类
            var categories = categoryService.GetRootCategories(TenantTypeIds.Instance().BarThread(), sectionId);
            foreach (var category in categories)
            {
                categoryService.Delete(category.CategoryId);
            }
            //帖吧标签
            TagService tagService = new TagService(TenantTypeIds.Instance().BarThread());
            tagService.ClearTagsFromOwner(sectionId);

            //删除Logo             
            DeleteLogo(sectionId);

            //删除推荐记录
            RecommendService recommendService = new RecommendService();
            recommendService.Delete(sectionId, TenantTypeIds.Instance().BarSection());

            int affectCount = barSectionRepository.Delete(section);
            if (affectCount > 0)
            {
                EventBus<BarSection>.Instance().OnAfter(section, new CommonEventArgs(EventOperationType.Instance().Delete()));
                EventBus<BarSection, AuditEventArgs>.Instance().OnAfter(section, new AuditEventArgs(null, section.AuditStatus));
            }
        }

        /// <summary>
        /// 删除Logo
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        public void DeleteLogo(long sectionId)
        {
            LogoService logoService = new LogoService(TenantTypeIds.Instance().BarSection());
            logoService.DeleteLogo(sectionId);
            BarSection section = barSectionRepository.Get(sectionId);
            if (section == null)
                return;
            section.LogoImage = string.Empty;
            barSectionRepository.Update(section);
        }

        /// <summary>
        /// 删除帖吧管理员
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="userId"></param>
        public void DeleteManager(long sectionId, long userId)
        {
            barSectionRepository.DeleteManager(sectionId, userId);
        }

        /// <summary>
        /// 删除某个用户的所有申请过的帖吧
        /// </summary>
        /// <param name="userId"></param>
        public void DeletesByUserId(long userId)
        {
            IEnumerable<BarSection> barSections = barSectionRepository.GetsByUserId(userId);
            foreach (var barSection in barSections)
            {
                Delete(barSection.SectionId);
            }
        }

        #endregion

        #region 获取帖吧
        /// <summary>
        /// 获取单个帖吧实体
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <returns>帖吧</returns>
        public BarSection Get(long sectionId)
        {
            return barSectionRepository.Get(sectionId);
        }


        //1、版主应该改成管理员，需求、原型图、本方法命名都应该修改；
        //2、帖吧管理员与帖吧的关注者是什么关系，帖吧管理员允许取消对帖吧的关注吗？

        /// <summary>
        /// 是否为吧主
        /// </summary>
        /// <param name="userId">被验证用户Id</param>
        /// <param name="sectionId">帖吧Id</param>
        /// <returns></returns>
        public bool IsSectionOwner(long userId, long sectionId)
        {
            BarSection barSection = Get(sectionId);
            if (barSection == null)
                return false;
            return barSection.UserId == userId;
        }


        /// <summary>
        /// 是否为吧管理员
        /// </summary>
        /// <param name="userId">被验证用户Id</param>
        /// <param name="sectionId">帖吧Id</param>
        /// <returns></returns>
        public bool IsSectionManager(long userId, long sectionId)
        {
            IEnumerable<long> managerIds = barSectionRepository.GetSectionManagerIds(sectionId);
            return managerIds.Contains(userId);
        }


        /// <summary>
        /// 获取帖吧的管理员列表
        /// </summary>
        /// <param name="sectionId">帖吧Id</param>
        /// <returns>帖吧</returns>
        public IEnumerable<User> GetSectionManagers(long sectionId)
        {
            IUserService userService = DIContainer.Resolve<IUserService>();
            return userService.GetFullUsers(barSectionRepository.GetSectionManagerIds(sectionId));
        }

        /// <summary>
        /// 依据OwnerId获取单个帖吧（用于OwnerId与帖吧一对一关系）
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <returns>帖吧</returns>
        public BarSection GetByOwnerId(string tenantTypeId, long ownerId)
        {
            return barSectionRepository.GetByOwnerId(tenantTypeId, ownerId);
        }




        /// <summary>
        /// 获取用户申请过的帖吧列表（仅适用于用户创建独立帖吧的情况）
        /// </summary>
        /// <param name="userId">创建者Id</param>
        /// <returns>帖吧列表</returns>
        public IEnumerable<BarSection> GetsByUserId(long userId)
        {
            return barSectionRepository.GetsByUserId(userId);
        }

        /// <summary>
        /// 获取帖吧的排行数据（仅显示审核通过的帖吧）
        /// </summary>
        /// <param name="topNumber">前多少条</param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="sortBy">帖吧排序依据</param>
        /// <returns></returns>
        public IEnumerable<BarSection> GetTops(int topNumber, long? categoryId, SortBy_BarSection sortBy = SortBy_BarSection.DateCreated_Desc)
        {
            //只获取启用状态的帖吧
            //缓存周期：相对稳定，不用维护即时性
            return barSectionRepository.GetTops(TenantTypeIds.Instance().Bar(), topNumber, categoryId, sortBy);
        }

        /// <summary>
        /// 获取帖吧列表
        /// </summary>
        /// <remarks>在频道帖吧分类页使用</remarks>
        /// <param name="nameKeyword"></param>
        /// <param name="sortBy">帖吧排序依据</param>
        /// <param name="categoryId">帖吧分类Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖吧列表</returns>
        public PagingDataSet<BarSection> Gets(string nameKeyword, long? categoryId, SortBy_BarSection sortBy, int pageIndex)
        {



            //需要获取categoryId所有后代的类别下的BarSection
            //按排序序号、创建时间倒序排序
            //缓存周期：相对稳定，需维护即时性
            return barSectionRepository.Gets(TenantTypeIds.Instance().Bar(), nameKeyword, categoryId, sortBy, pageIndex);
        }

        /// <summary>
        /// 帖吧管理时查询帖吧分页集合
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <param name="query">帖吧查询条件</param>
        /// <param name="pageSize">每页多少条数据</param>
        /// <param name="pageIndex">页码</param>
        /// <returns>帖吧分页集合</returns>
        public PagingDataSet<BarSection> Gets(string tenantTypeId, BarSectionQuery query, int pageSize, int pageIndex)
        {
            //缓存周期：对象集合，需要维护即时性
            //使用用户选择器设置query.UserId参数
            return barSectionRepository.Gets(tenantTypeId, query, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据帖吧Id集合组装帖吧实体集合
        /// </summary>
        /// <param name="sectionIds">帖吧Id集合</param>
        /// <returns>帖吧实体集合</returns>
        public IEnumerable<BarSection> GetBarsections(IEnumerable<long> sectionIds)
        {
            return barSectionRepository.PopulateEntitiesByEntityIds<long>(sectionIds);
        }
        #endregion


        /// <summary>
        /// 获取帖吧管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            return barSectionRepository.GetManageableDatas(tenantTypeId);
        }

        /// <summary>
        /// 获取帖吧统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            return barSectionRepository.GetStatisticDatas();
        }

        /// <summary>
        /// 获取前N个贴吧标签ID（TagInOwner类型）
        /// </summary>
        /// <param name="topNumber">前N个ID数量</param>
        /// <param name="sortBy">排序依据字段</param>
        /// <returns></returns>
        public IEnumerable<long> GetTopTagsForBar(int topNumber, SortBy_Tag? sortBy)
        {
            return barSectionRepository.GetTopTagsForBar(topNumber, sortBy);
        }
    }
}