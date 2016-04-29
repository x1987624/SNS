//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-08-09</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-08-09" version="0.5">创建</log>
//<log date="2012-08-10" version="0.6" author="yangmj">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;
using Tunynet.Events;
using System.Linq;

namespace Spacebuilder.Microblog
{

    //reply:已解决
    /// <summary>
    /// 微博Service
    /// </summary>
    public class MicroblogService
    {

        //1、各方法逻辑规则应该有注释；
        //2、转发、评论应该有单独的方法；
        #region Service

        private AuditService auditService = new AuditService();
        private CommentService commentService = new CommentService();
        private ActivityService activityService = new ActivityService();
        private ParsedMediaService parsedMediaService = new ParsedMediaService();
        private AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().Microblog());
        private TagService tagService = new TagService(TenantTypeIds.Instance().Microblog());


        IMicroblogRepository microblogRepository = null;

        #endregion Service

        //应用Id
        private int applicationId = 1001;

        /// <summary>
        /// 构造器
        /// </summary>
        public MicroblogService()
            : this(new MicroblogRepository())
        {

        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="microblogRepository">微博数据仓储</param>
        public MicroblogService(IMicroblogRepository microblogRepository)
        {
            this.microblogRepository = microblogRepository;
        }

        #region 维护

        /// <summary>
        /// 创建微博
        /// </summary>
        /// <param name="microblog">待创建微博实体</param>
        /// <returns></returns>
        public long Create(MicroblogEntity microblog)
        {
            EventBus<MicroblogEntity>.Instance().OnBefore(microblog, new CommonEventArgs(EventOperationType.Instance().Create()));

            //设置审核状态
            auditService.ChangeAuditStatusForCreate(microblog.UserId, microblog);

            string videoAlias = string.Empty, audioAlias = string.Empty;

            microblog.Body = parsedMediaService.ResolveBodyForEdit(microblog.Body, out videoAlias, out audioAlias);
            microblog.HasVideo = !string.IsNullOrEmpty(videoAlias);
            microblog.HasMusic = !string.IsNullOrEmpty(audioAlias);
            microblog.VideoAlias = videoAlias;
            microblog.AudioAlias = audioAlias;

            long id = 0;
            long.TryParse(microblogRepository.Insert(microblog).ToString(), out id);

            if (id > 0)
            {
                string tenantTypeId = TenantTypeIds.Instance().Microblog();

                OwnerDataService ownerDataService = new OwnerDataService(microblog.TenantTypeId);
                ownerDataService.Change(microblog.OwnerId, OwnerDataKeys.Instance().ThreadCount(), 1);


                //将临时附件转换为正式附件
                attachmentService.ToggleTemporaryAttachments(microblog.UserId, tenantTypeId, id);

                AtUserService atUserService = new AtUserService(tenantTypeId);
                atUserService.ResolveBodyForEdit(microblog.Body, microblog.UserId, microblog.MicroblogId);

                TagService tagService = new TagService(tenantTypeId);
                tagService.ResolveBodyForEdit(microblog.Body, microblog.OwnerId, microblog.MicroblogId, tenantTypeId);

                EventBus<MicroblogEntity>.Instance().OnAfter(microblog, new CommonEventArgs(EventOperationType.Instance().Create()));
                EventBus<MicroblogEntity, AuditEventArgs>.Instance().OnAfter(microblog, new AuditEventArgs(null, microblog.AuditStatus));
            }

            return id;
        }

        /// <summary>
        /// 转发微博
        /// </summary>
        /// <param name="microblog">微博实体</param>
        /// <param name="isCommnet">是否对被转发微博进行评论</param>
        /// <param name="isCommentOriginal">是否对微博原文进行评论</param>
        /// <param name="toUserId">评论拥有者Id</param>
        /// <param name="toOriginalUserId">源引微博评论拥有者Id</param>
        /// <param name="microblogId">返回新的microblogId</param>
        /// <returns>是否创建成功，成功为-true</returns>
        public bool Forward(MicroblogEntity microblog, bool isCommnet, bool isCommentOriginal, long toUserId, long toOriginalUserId, out long microblogId)
        {
            microblogId = Create(microblog);
            bool isSuccess = microblogId > 0;

            if (!isSuccess)
                return isSuccess;

            if (isCommnet)
            {
                Comment comment = Comment.New();
                comment.UserId = microblog.UserId;
                comment.TenantTypeId = TenantTypeIds.Instance().Microblog();
                comment.CommentedObjectId = microblog.ForwardedMicroblogId;
                comment.OwnerId = toUserId;
                comment.Author = microblog.Author;
                comment.Body = microblog.Body;
                commentService.Create(comment);
            }

            if (isCommentOriginal)
            {
                Comment comment = Comment.New();
                comment.UserId = microblog.UserId;
                comment.TenantTypeId = TenantTypeIds.Instance().Microblog();
                comment.CommentedObjectId = microblog.OriginalMicroblogId;
                comment.OwnerId = toOriginalUserId;
                comment.Author = microblog.Author;
                comment.Body = microblog.Body;
                commentService.Create(comment);
            }

            return isSuccess;
        }

        /// <summary>
        /// 删除微博
        /// </summary>
        /// <param name="microblogId">微博Id</param>
        public void Delete(long microblogId)
        {
            MicroblogEntity entity = Get(microblogId);
            if (entity == null)
                return;

            var sender = new CommentService().GetCommentedObjectComments(microblogId);



            EventBus<MicroblogEntity>.Instance().OnBefore(entity, new CommonEventArgs(EventOperationType.Instance().Delete()));
            int affect = microblogRepository.Delete(entity);
            if (affect > 0)
            {
                //删除微博时评论的积分处理
                if (sender != null)
                    EventBus<Comment>.Instance().OnBatchAfter(sender, new CommonEventArgs(EventOperationType.Instance().Delete()));
                //更新用户数据
                OwnerDataService ownerDataService = new OwnerDataService(entity.TenantTypeId);
                ownerDataService.Change(entity.OwnerId, OwnerDataKeys.Instance().ThreadCount(), -1);

                EventBus<MicroblogEntity>.Instance().OnAfter(entity, new CommonEventArgs(EventOperationType.Instance().Delete()));
                EventBus<MicroblogEntity, AuditEventArgs>.Instance().OnAfter(entity, new AuditEventArgs(entity.AuditStatus, null));
            }
        }

        /// <summary>
        /// 删除用户的所有微博
        /// </summary>
        /// <param name="userId"></param>
        private void DeletesByUserId(long userId)
        {
            IEnumerable<MicroblogEntity> microblogs = microblogRepository.GetAllMicroblogsOfUser(userId);

            foreach (var microblog in microblogs)
            {
                Delete(microblog.MicroblogId);
            }
        }

        /// <summary>
        /// 删除用户记录（删除用户时使用）
        /// </summary>
        /// <param name="userId">被删除用户</param>
        /// <param name="takeOverUserName">接管用户UserName</param>
        /// <param name="takeOverAll">是否接管被删除用户的所有内容</param>
        public void DeleteUser(long userId, string takeOverUserName, bool takeOverAll)
        {

            IUserService userService = DIContainer.Resolve<IUserService>();
            if (takeOverAll)
            {
                User takeOver = userService.GetFullUser(takeOverUserName);
                if (takeOver != null && takeOver.UserId > 0)
                {
                    microblogRepository.DeleteUser(userId, takeOver, takeOverAll);
                }
            }
            else if (userId > 0)
            {
                DeletesByUserId(userId);
            }
        }

        /// <summary>
        /// 更新审核状态
        /// </summary>
        /// <param name="microblogId">待被更新的微博Id</param>
        /// <param name="isApproved">是否通过审核</param>
        public void UpdateAuditStatus(long microblogId, bool isApproved)
        {
            MicroblogEntity microblog = microblogRepository.Get(microblogId);
            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            if (microblog.AuditStatus == auditStatus)
                return;
            AuditStatus oldAuditStatus = microblog.AuditStatus;
            microblog.AuditStatus = auditStatus;
            microblogRepository.Update(microblog);
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            EventBus<MicroblogEntity>.Instance().OnAfter(microblog, new CommonEventArgs(operationType));
            EventBus<MicroblogEntity, AuditEventArgs>.Instance().OnAfter(microblog, new AuditEventArgs(oldAuditStatus, microblog.AuditStatus));
        }

        /// <summary>
        /// 批量更新审核状态
        /// </summary>
        /// <param name="microblogIds">待被更新的微博Id集合</param>
        /// <param name="isApproved">是否通过审核</param>
        public void BatchUpdateAuditStatus(IEnumerable<long> microblogIds, bool isApproved)
        {
            IEnumerable<MicroblogEntity> microblogs = GetMicroblogs(microblogIds);
            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            foreach (var microblog in microblogs)
            {
                if (microblog.AuditStatus == auditStatus)
                    continue;
                AuditStatus oldAuditStatus = microblog.AuditStatus;
                microblog.AuditStatus = auditStatus;
                microblogRepository.Update(microblog);

                EventBus<MicroblogEntity>.Instance().OnAfter(microblog, new CommonEventArgs(operationType));
                EventBus<MicroblogEntity, AuditEventArgs>.Instance().OnAfter(microblog, new AuditEventArgs(oldAuditStatus, microblog.AuditStatus));
            }
        }

        #endregion

        #region 获取数据

        /// <summary>
        /// 获取最新微博数
        /// </summary>
        /// <param name="lastMicroblogId">用户浏览的最新一条微博Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        public int GetNewerCount(long lastMicroblogId, string tenantTypeId = "")
        {
            return microblogRepository.GetNewerCount(lastMicroblogId, tenantTypeId);
        }

        /// <summary>
        /// 获取最新微博
        /// </summary>
        /// <param name="lastMicroblogId">用户浏览的最新一条微博Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        public IEnumerable<MicroblogEntity> GetNewerMicroblogs(long lastMicroblogId, string tenantTypeId = "")
        {
            return microblogRepository.GetNewerMicroblogs(lastMicroblogId, tenantTypeId);
        }

        /// <summary>
        /// 获取单条微博
        /// </summary>
        /// <param name="microblogId">微博Id</param>
        public MicroblogEntity Get(long microblogId)
        {
            if (microblogId <= 0)
                return null;
            return microblogRepository.Get(microblogId);
        }


        //reply:已修改

        /// <summary>
        /// 获取微博分页数据
        /// </summary>
        ///<param name="pageIndex">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetPagings(int pageIndex, string tenantTypeId = "", MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            return microblogRepository.GetPagings(pageIndex, tenantTypeId, mediaType, isOriginal, sortBy);
        }

        /// <summary>
        /// 根据用户获取可排序的微博Id集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public PagingDataSet<long> GetPaingIds(long userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "")
        {
            return microblogRepository.GetPagingIds(userId, mediaType, isOriginal, pageIndex, tenantTypeId);
        }

        /// <summary>
        /// 根据用户获取可排序的微博集合
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetPagings(long userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "")
        {


            return microblogRepository.GetPagings(userId, mediaType, isOriginal, pageIndex, tenantTypeId);
        }

        /// <summary>
        /// 根据拥有者获取微博分页集合
        /// </summary>
        /// <param name="ownerId">用户Id</param>
        /// <param name="userId">微博作者Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        /// <param name="pageIndex">页码</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetPagings(long ownerId, long? userId, MediaType? mediaType, bool? isOriginal, int pageIndex, string tenantTypeId = "")
        {
            return microblogRepository.GetPagings(ownerId, userId, mediaType, isOriginal, pageIndex, tenantTypeId);
        }

        /// <summary>
        /// 获取指定条数的微博
        /// </summary>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetTops(int topNumber, string tenantTypeId = "", MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            return microblogRepository.GetTops(topNumber, tenantTypeId, mediaType, isOriginal, sortBy);
        }

        /// <summary>
        /// 根据用户获取指定条数的微博
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="mediaType"><see cref="MediaType"/></param>
        /// <param name="isOriginal">是否为原创</param>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetTops(long userId, MediaType? mediaType, bool? isOriginal, int topNumber, string tenantTypeId = "", SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            if (string.IsNullOrEmpty(tenantTypeId))
            {
                tenantTypeId = TenantTypeIds.Instance().Microblog();
            }

            return microblogRepository.GetTops(userId, mediaType, isOriginal, topNumber, tenantTypeId, sortBy);
        }

        /// <summary>
        /// 根据拥有者获取指定条数的微博
        /// </summary>
        ///<param name="ownerId">微博拥有者Id</param>
        ///<param name="topNumber">待获取条数</param>
        ///<param name="tenantTypeId">租户类型Id</param>
        ///<param name="mediaType"><see cref="MediaType"/></param>
        ///<param name="isOriginal">是否为原创</param>
        ///<param name="sortBy">排序</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetTops(long ownerId, int topNumber, string tenantTypeId = "", MediaType? mediaType = null, bool? isOriginal = null, SortBy_Microblog sortBy = SortBy_Microblog.DateCreated)
        {
            if (string.IsNullOrEmpty(tenantTypeId))
            {
                tenantTypeId = TenantTypeIds.Instance().Microblog();
            }

            return microblogRepository.GetTops(ownerId, topNumber, tenantTypeId, mediaType, isOriginal, sortBy);
        }

        /// <summary>
        /// 根据微博主键集合组装微博实体集合
        /// </summary>
        /// <param name="microblogIds">微博主键集合</param>
        /// <returns></returns>
        public IEnumerable<MicroblogEntity> GetMicroblogs(IEnumerable<long> microblogIds)
        {
            return microblogRepository.PopulateEntitiesByEntityIds(microblogIds);
        }

        /// <summary>
        /// 管理员后台获取列表方法
        /// </summary>
        /// <param name="query">查询条件</param>
        ///<param name="pageSize">每页显示内容数</param>
        ///<param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetMicroblogs(MicroblogQuery query, int pageSize, int pageIndex)
        {
            return microblogRepository.GetMicroblogs(query, pageSize, pageIndex);
        }


        /// <summary>
        /// 获取提到用户的微博分页集合
        /// </summary>
        /// <param name="userName">被提到的用户Id</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetMicroblogsByReferredUser(long userId, int pageIndex)
        {
            AtUserService atUserService = new AtUserService(TenantTypeIds.Instance().Microblog());

            PagingDataSet<long> pagingIds = atUserService.GetPagingAssociateIds(userId, pageIndex);

            if (pagingIds != null)
            {

                PagingDataSet<MicroblogEntity> pds = new PagingDataSet<MicroblogEntity>(microblogRepository.PopulateEntitiesByEntityIds(pagingIds));
                pds.PageIndex = pagingIds.PageIndex;
                pds.PageSize = pagingIds.PageSize;
                pds.TotalRecords = pagingIds.TotalRecords;

                return pds;
            }

            return null;
        }

        /// <summary>
        /// 根据话题名称（标签）分页查询微博列表
        /// </summary>
        /// <param name="tagName">话题名称</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetMicroblogsByTagName(string tagName, int pageSize, int pageIndex)
        {

            //reply:江哥他们用的可以不管排序
            PagingEntityIdCollection pec = tagService.GetItemIds(tagName, null, pageSize, pageIndex);

            IEnumerable<MicroblogEntity> microblogEntitysList = microblogRepository.PopulateEntitiesByEntityIds(pec.GetPagingEntityIds(pageSize, pageIndex));

            //组装分页对象
            PagingDataSet<MicroblogEntity> microblogEntitysPage = new PagingDataSet<MicroblogEntity>(microblogEntitysList)
            {
                TotalRecords = pec.TotalRecords,
                PageSize = pageSize,
                PageIndex = pageIndex
            };

            return microblogEntitysPage;
        }

        /// <summary>
        /// 根据多个话题名称（标签）分页查询微博列表
        /// </summary>
        /// <param name="tagNames">话题名称列表</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public PagingDataSet<MicroblogEntity> GetMicroblogsByTagNames(IEnumerable<string> tagNames, int pageSize, int pageIndex)
        {
            PagingEntityIdCollection pec = tagService.GetItemIds(tagNames, null, pageSize, pageIndex);

            IEnumerable<MicroblogEntity> microblogEntitysList = microblogRepository.PopulateEntitiesByEntityIds(pec.GetPagingEntityIds(pageSize, pageIndex));

            //组装分页对象
            PagingDataSet<MicroblogEntity> microblogEntitysPage = new PagingDataSet<MicroblogEntity>(microblogEntitysList)
            {
                TotalRecords = pec.TotalRecords,
                PageSize = pageSize,
                PageIndex = pageIndex
            };

            return microblogEntitysPage;
        }

        /// <summary>
        /// 获取微博管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            return microblogRepository.GetManageableDatas(tenantTypeId);
        }

        /// <summary>
        /// 获取群组统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns></returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            return microblogRepository.GetStatisticDatas();
        }

        #endregion
    }
}
