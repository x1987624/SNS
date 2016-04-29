//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Caching;
using Spacebuilder.CMS;
using Tunynet.Events;
using Tunynet.Common;
using Tunynet.Utilities;
using Tunynet;
using Spacebuilder.Common;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// ContentItem 业务逻辑
    /// </summary>
    public class ContentItemService
    {
        private ContentItemRepository contentItemRepository;

        /// <summary>
        /// 构造器
        /// </summary>
        public ContentItemService()
        {
            contentItemRepository = new ContentItemRepository();
        }


        /// <summary>
        /// 创建ContentItem
        /// </summary>
        /// <param name="contentItem"></param>
        public void Create(ContentItem contentItem)
        {
            //设置审核状态
            if (contentItem.IsContributed && contentItem.ContentFolder != null && contentItem.ContentFolder.NeedAuditing)
            {
                new AuditService().ChangeAuditStatusForCreate(contentItem.UserId, contentItem);
            }
            else
                contentItem.AuditStatus = AuditStatus.Success;

            //执行事件
            EventBus<ContentItem>.Instance().OnBefore(contentItem, new CommonEventArgs(EventOperationType.Instance().Create()));

            contentItemRepository.Insert(contentItem);
            //转换临时附件
            AttachmentService attachmentService = new AttachmentService(TenantTypeIds.Instance().ContentItem());
            attachmentService.ToggleTemporaryAttachments(contentItem.UserId, TenantTypeIds.Instance().ContentItem(), contentItem.ContentItemId);
            if (contentItem.AdditionalProperties.Get<bool>("FirstAsTitleImage", false))
            {
                IEnumerable<Attachment> attachments = new AttachmentService(TenantTypeIds.Instance().ContentItem()).GetsByAssociateId(contentItem.ContentItemId);
                Attachment fristImage = attachments.Where(n => n.MediaType == MediaType.Image).FirstOrDefault();
                if (fristImage != null)
                {
                    contentItem.FeaturedImageAttachmentId = fristImage.AttachmentId;
                    contentItem.FeaturedImage = fristImage.GetRelativePath() + "\\" + fristImage.FileName;
                }
            }
            //用户投稿计数+1
            OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
            ownerDataService.Change(contentItem.UserId, OwnerDataKeys.Instance().ContributeCount(), 1);
            //执行事件
            EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(EventOperationType.Instance().Create(), ApplicationIds.Instance().CMS()));
            EventBus<ContentItem, AuditEventArgs>.Instance().OnAfter(contentItem, new AuditEventArgs(null, contentItem.AuditStatus));
        }

        /// <summary>
        /// 更新ContentItem
        /// </summary>
        /// <param name="contentItem"></param>
        public void Update(ContentItem contentItem)
        {
            //执行事件
            EventBus<ContentItem>.Instance().OnBefore(contentItem, new CommonEventArgs(EventOperationType.Instance().Update()));
            AuditStatus prevAuditStatus = contentItem.AuditStatus;

            //设置审核状态
            AuditService auditService = new AuditService();
            auditService.ChangeAuditStatusForUpdate(contentItem.UserId, contentItem);
            if (contentItem.AdditionalProperties.Get<bool>("FirstAsTitleImage", false))
            {
                IEnumerable<Attachment> attachments = new AttachmentService(TenantTypeIds.Instance().ContentItem()).GetsByAssociateId(contentItem.ContentItemId);
                Attachment fristImage = attachments.Where(n => n.MediaType == MediaType.Image).FirstOrDefault();
                if (fristImage != null)
                {
                    contentItem.FeaturedImageAttachmentId = fristImage.AttachmentId;
                    contentItem.FeaturedImage = fristImage.GetRelativePath() + "\\" + fristImage.FileName;
                }
            }
            contentItemRepository.Update(contentItem);

            //执行事件
            EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(EventOperationType.Instance().Update(), ApplicationIds.Instance().CMS()));
            EventBus<ContentItem, AuditEventArgs>.Instance().OnAfter(contentItem, new AuditEventArgs(prevAuditStatus, contentItem.AuditStatus));
        }

        /// <summary>
        /// 删除ContentItem
        /// </summary>
        /// <param name="contentItemId"></param>
        public void Delete(long contentItemId)
        {
            ContentItem contentItem = contentItemRepository.Get(contentItemId);
            if (contentItem != null)
            {
                Delete(contentItem);
            }
        }

        /// <summary>
        /// 删除ContentItem
        /// </summary>
        /// <param name="contentItem"></param>
        public void Delete(ContentItem contentItem)
        {
            if (contentItem != null)
            {
                //执行事件
                EventBus<ContentItem>.Instance().OnBefore(contentItem, new CommonEventArgs(EventOperationType.Instance().Delete()));

                contentItemRepository.Delete(contentItem);
                //用户投稿计数-1
                OwnerDataService ownerDataService = new OwnerDataService(TenantTypeIds.Instance().User());
                ownerDataService.Change(contentItem.UserId, OwnerDataKeys.Instance().ContributeCount(), -1);

                //删除标签关联
                TagService tagService = new TagService(TenantTypeIds.Instance().ContentItem());
                tagService.ClearTagsFromItem(contentItem.ContentItemId, contentItem.UserId);
                //删除评论
                CommentService commentService = new CommentService();
                commentService.DeleteCommentedObjectComments(contentItem.ContentItemId);
                //执行事件
                EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(EventOperationType.Instance().Delete(), ApplicationIds.Instance().CMS()));
                EventBus<ContentItem, AuditEventArgs>.Instance().OnAfter(contentItem, new AuditEventArgs(contentItem.AuditStatus, null));
            }
        }

        /// <summary>
        /// 删除contentFolderId下的所有ContentItem
        /// </summary>
        /// <param name="contentFolderId"></param>
        public void DeleteByFolder(int contentFolderId)
        {
            ContentItemQuery query = new ContentItemQuery(CacheVersionType.None)
            {
                ContentFolderId = contentFolderId
            };

            PagingDataSet<ContentItem> items = contentItemRepository.GetContentItems(false, query, int.MaxValue, 1);

            foreach (var item in items)
            {
                Delete(item);
            }
        }

        /// <summary>
        /// 批量移动ContentItem
        /// </summary>
        /// <param name="contentItemIds"></param>
        /// <param name="toContentFolderId"></param>
        public void Move(IEnumerable<long> contentItemIds, int toContentFolderId)
        {
            ContentFolderService contentFolderService = new ContentFolderService();

            ContentFolder toContentFolder = contentFolderService.Get(toContentFolderId);
            if (toContentFolder == null)
                return;

            contentItemIds = contentItemIds.Distinct();
            IEnumerable<ContentItem> contentItemsForMove = contentItemRepository.PopulateEntitiesByEntityIds(contentItemIds).Where(c => c.ContentFolderId != toContentFolderId);

            contentItemRepository.Move(contentItemsForMove, toContentFolderId);
            foreach (var contentItem in contentItemsForMove)
            {
                //执行事件
                EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(EventOperationType.Instance().Update(), ApplicationIds.Instance().CMS()));
            }
        }

        /// <summary>
        /// 设置/取消 精华
        /// </summary>
        /// <param name="contentItemId"></param>
        /// <param name="isEssential"></param>
        public void SetEssential(long contentItemId, bool isEssential)
        {
            ContentItem contentItem = contentItemRepository.Get(contentItemId);

            if (contentItem != null && contentItem.IsEssential != isEssential)
            {
                contentItem.IsEssential = isEssential;
                contentItemRepository.Update(contentItem);

                //执行事件
                if (isEssential)
                    EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(EventOperationType.Instance().SetEssential(), ApplicationIds.Instance().CMS()));
                else
                    EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(EventOperationType.Instance().CancelEssential(), ApplicationIds.Instance().CMS()));
            }
        }

        /// <summary>
        /// 设置置顶
        /// </summary>
        /// <param name="contentItemId"></param>
        /// <param name="isGlobalSticky"></param>
        /// <param name="globalStickyDate"></param>
        /// <param name="isFolderSticky"></param>
        /// <param name="folderStickyDate"></param>
        public void SetSticky(long contentItemId, bool isGlobalSticky, DateTime globalStickyDate, bool isFolderSticky, DateTime folderStickyDate)
        {
            ContentItem contentItem = contentItemRepository.Get(contentItemId);
            if (contentItem == null)
                return;
            string eventOperationTypeGlobal = string.Empty;
            if (contentItem.IsGlobalSticky != isGlobalSticky)
            {
                contentItem.IsGlobalSticky = isGlobalSticky;
                contentItem.GlobalStickyDate = ValueUtility.GetSafeSqlDateTime(globalStickyDate);
                eventOperationTypeGlobal = isGlobalSticky ? EventOperationType.Instance().SetGlobalSticky() : EventOperationType.Instance().CancelGlobalSticky();
            }
            string eventOperationTypeFolder = string.Empty;
            if (contentItem.IsFolderSticky != isFolderSticky)
            {
                contentItem.IsFolderSticky = isFolderSticky;
                contentItem.FolderStickyDate = ValueUtility.GetSafeSqlDateTime(folderStickyDate);
                eventOperationTypeFolder = isFolderSticky ? EventOperationType.Instance().SetFolderSticky() : EventOperationType.Instance().CancelFolderSticky();
            }

            contentItemRepository.Update(contentItem);

            if (!string.IsNullOrEmpty(eventOperationTypeGlobal))
            {//执行事件
                EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(eventOperationTypeGlobal, ApplicationIds.Instance().CMS()));
            }
            if (!string.IsNullOrEmpty(eventOperationTypeFolder))
            {//执行事件
                EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(eventOperationTypeFolder, ApplicationIds.Instance().CMS()));
            }
        }

        /// <summary>
        /// 使置顶到期的的资讯恢复至未置顶状态
        /// </summary>
        public void ExpireStickyContentItems()
        {
            contentItemRepository.ExpireStickyContentItems();
        }

        /// <summary>
        /// 设置审核设置
        /// </summary>
        public void UpdateAuditStatus(long contentItemId, bool isApproved)
        {
            AuditStatus auditStatus = isApproved ? AuditStatus.Success : AuditStatus.Fail;

            ContentItem contentItem = contentItemRepository.Get(contentItemId);
            string operationType = isApproved ? EventOperationType.Instance().Approved() : EventOperationType.Instance().Disapproved();
            if (contentItem != null && contentItem.AuditStatus != auditStatus)
            {
                AuditStatus oldAuditStatus = contentItem.AuditStatus;
                contentItem.AuditStatus = auditStatus;

                contentItemRepository.Update(contentItem);

                //触发事件
                EventBus<ContentItem>.Instance().OnAfter(contentItem, new CommonEventArgs(operationType, ApplicationIds.Instance().CMS()));
                EventBus<ContentItem, AuditEventArgs>.Instance().OnAfter(contentItem, new AuditEventArgs(oldAuditStatus, auditStatus));
            }
        }

        /// <summary>
        /// 删除用户时处理所有资讯相关的数据（删除用户时使用）
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
                contentItemRepository.TakeOver(userId, takeOverUser);
            }
            //takeOverAll为false，调用Delete方法逐条删除用户的资讯，积分、索引、动态等的删除由EventModule处理，评论、顶踩、附件等相关数据由系统自动删除
            else
            {
                int pageSize = 100;     //批量删除，每次删100条
                int pageIndex = 1;
                int pageCount = 1;
                do
                {
                    PagingDataSet<ContentItem> contentItems = GetUserContentItems(userId, null, null, pageSize, pageIndex);
                    foreach (ContentItem contentItem in contentItems)
                    {
                        Delete(contentItem);
                    }
                    pageCount = contentItems.PageCount;
                    pageIndex++;
                } while (pageIndex <= pageCount);
            }
        }

        /// <summary>
        /// 获取ContentItem
        /// </summary>
        /// <param name="contentItemId"></param>
        /// <returns></returns>
        public ContentItem Get(long contentItemId)
        {
            return contentItemRepository.Get(contentItemId);
        }

        /// <summary>
        /// 获取浏览过本文的人还看过的资讯列表
        /// </summary>
        /// <param name="contentItemId"></param>
        /// <param name="topNumber"></param>
        /// <returns></returns>
        public IEnumerable<ContentItem> GetVisitorAlsoVisitedItems(long contentItemId, int topNumber)
        {
            var visitService = new VisitService(TenantTypeIds.Instance().ContentItem());
            var itemIds = visitService.GetVisitorAlsoVisitedObjectIds(contentItemId, topNumber);
            return contentItemRepository.PopulateEntitiesByEntityIds(itemIds);
        }

        /// <summary>
        /// 根据资讯Id列表获取实体列表
        /// </summary>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        public IEnumerable<ContentItem> GetContentItems(IEnumerable<long> itemIds)
        {
            return contentItemRepository.PopulateEntitiesByEntityIds(itemIds);
        }


        /// <summary>
        /// 获取非主流排序方式的前topNumber条ContentItem
        /// </summary>
        /// <remarks>
        /// 主要用于频道
        /// </remarks>
        public IEnumerable<ContentItem> GetTops(int topNumber, int? contentFolderId, ContentItemSortBy sortBy)
        {
            return contentItemRepository.GetTops(topNumber, contentFolderId, sortBy);
        }

        /// <summary>
        /// 分页获取非主流排序方式的ContentItem
        /// </summary>
        public PagingDataSet<ContentItem> GetContentItemsSortBy(ContentItemSortBy sortBy, int? contentTypeId = null, int? contentFolderId = null, bool includeFolderDescendants = true, string tagName = null, int pageSize = 15, int pageIndex = 1)
        {
            ContentItemQuery query;
            if (contentFolderId.HasValue)
            {
                query = new ContentItemQuery(CacheVersionType.AreaVersion);
                query.AreaCachePropertyName = "ContentFolderId";
                query.AreaCachePropertyValue = contentFolderId.Value;
                query.ContentFolderId = contentFolderId;
            }
            else
            {
                query = new ContentItemQuery(CacheVersionType.GlobalVersion);
            }
            query.IncludeFolderDescendants = includeFolderDescendants;
            query.SortBy = sortBy;
            query.PubliclyAuditStatus = new AuditService().GetPubliclyAuditStatus(ApplicationIds.Instance().CMS());
            query.TagName = tagName;
            if (contentTypeId.HasValue && contentTypeId.Value > 0)
                query.ContentTypeId = contentTypeId;

            return contentItemRepository.GetContentItems(true, query, pageSize, pageIndex);
        }

        /// <summary>
        /// 分页获取非主流排序方式的ContentItem
        /// </summary>
        public PagingDataSet<ContentItem> GetUserContentItems(long userId, int? contentFolderId = null, PubliclyAuditStatus? publiclyAuditStatus = null, int pageSize = 20, int pageIndex = 1)
        {
            ContentItemQuery query = new ContentItemQuery(CacheVersionType.AreaVersion);
            query.AreaCachePropertyName = "UserId";
            query.AreaCachePropertyValue = userId;
            query.UserId = userId;
            if (contentFolderId.HasValue && contentFolderId.Value > 0)
                query.ContentFolderId = contentFolderId.Value;
            query.IncludeFolderDescendants = true;
            query.PubliclyAuditStatus = publiclyAuditStatus;
            return contentItemRepository.GetContentItems(true, query, pageSize, pageIndex);
        }

        /// <summary>
        /// 依据查询条件获取ContentItem(后台管理员使用)
        /// </summary>
        /// <param name="auditStatus"></param>
        /// <param name="subjectKeyword"></param>
        /// <param name="contentFolderId"></param>
        /// <param name="includeFolderDescendants"></param>
        /// <param name="tagNameKeyword"></param>
        /// <param name="contentTypeId"></param>
        /// <param name="userId"></param>
        /// <param name="isContributed"></param>
        /// <param name="isEssential"></param>
        /// <param name="isSticky"></param>
        /// <param name="minDate"></param>
        /// <param name="maxDate"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public PagingDataSet<ContentItem> GetContentItemsForAdmin(AuditStatus? auditStatus, string subjectKeyword, int? contentFolderId, bool? includeFolderDescendants = true, string tagNameKeyword = null, int? contentTypeId = null, long? userId = null, long? moderatorUserId = null, bool? isContributed = null, bool? isEssential = null, bool? isSticky = null, DateTime? minDate = null, DateTime? maxDate = null, int pageSize = 15, int pageIndex = 1)
        {
            PubliclyAuditStatus? publiclyAuditStatus = null;
            if (auditStatus.HasValue)
                switch (auditStatus.Value)
                {
                    case AuditStatus.Again:
                        publiclyAuditStatus = PubliclyAuditStatus.Again;
                        break;
                    case AuditStatus.Fail:
                        publiclyAuditStatus = PubliclyAuditStatus.Fail;
                        break;
                    case AuditStatus.Success:
                        publiclyAuditStatus = PubliclyAuditStatus.Success;
                        break;
                    case AuditStatus.Pending:
                    default:
                        publiclyAuditStatus = PubliclyAuditStatus.Pending;
                        break;
                }
            ContentItemQuery query = new ContentItemQuery(CacheVersionType.None)
            {
                ContentFolderId = contentFolderId,
                IncludeFolderDescendants = includeFolderDescendants,
                ContentTypeId = contentTypeId,
                UserId = userId,
                ModeratorUserId = moderatorUserId,
                IsContributed = isContributed,
                IsEssential = isEssential,
                IsSticky = isSticky,
                SubjectKeyword = subjectKeyword,
                TagNameKeyword = tagNameKeyword,
                PubliclyAuditStatus = publiclyAuditStatus,
                SortBy = ContentItemSortBy.ReleaseDate_Desc,
                MinDate = minDate,
                MaxDate = maxDate,

            };
            return contentItemRepository.GetContentItems(false, query, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取资讯管理数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>资讯管理数据</returns>
        public Dictionary<string, long> GetManageableDatas(string tenantTypeId = null)
        {
            return contentItemRepository.GetManageableData(tenantTypeId);
        }

        /// <summary>
        /// 获取资讯统计数据
        /// </summary>
        /// <param name="tenantTypeId">租户类型Id（可以获取该应用下针对某种租户类型的统计计数，默认不进行筛选）</param>
        /// <returns>资讯统计数据</returns>
        public Dictionary<string, long> GetStatisticDatas(string tenantTypeId = null)
        {
            return contentItemRepository.GetApplicationStatisticData();
        }
    }

    /// <summary>
    /// ContentItem排序字段
    /// </summary>
    public enum ContentItemSortBy
    {
        /// <summary>
        /// 发布日期
        /// </summary>
        ReleaseDate_Desc,

        /// <summary>
        /// 阶段浏览次数
        /// </summary>
        StageHitTimes,

        /// <summary>
        /// 浏览次数
        /// </summary>
        HitTimes,

        /// <summary>
        /// 阶段评论数
        /// </summary>
        StageCommentCount,

        /// <summary>
        /// 评论总数
        /// </summary>
        CommentCount,

        /// <summary>
        /// 排序序号
        /// </summary>
        DisplayOrder

    }

}
