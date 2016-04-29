//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spacebuilder.CMS.Metadata;
using Tunynet.Caching;
using PetaPoco;
using Tunynet.Common;
using Tunynet;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// 内容项
    /// </summary>
    [TableName("spb_cms_ContentItems")]
    [PrimaryKey("ContentItemId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "ContentFolderId,UserId")]
    [Serializable]
    public class ContentItem : SerializablePropertiesBase, IAuditable, IEntity
    {

        #region 持久化属性

        /// <summary>
        /// ContentItemId
        /// </summary>
        public long ContentItemId { get; set; }

        private int contentFolderId;
        /// <summary>
        /// 栏目Id
        /// </summary>
        public int ContentFolderId
        {
            get { return contentFolderId; }
            set { contentFolderId = value; }
        }

        private int contentTypeId;
        /// <summary>
        /// 内容模型Id
        /// </summary>
        public int ContentTypeId
        {
            get { return contentTypeId; }
            set { contentTypeId = value; }
        }

        private string title = string.Empty;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private string featuredImage = string.Empty;
        /// <summary>
        /// 标题图（相对路径）
        /// </summary>
        public string FeaturedImage
        {
            get { return featuredImage; }
            set { featuredImage = value; }
        }

        private long featuredImageAttachmentId = 0;
        /// <summary>
        /// 标题图对应的附件Id
        /// </summary>
        public long FeaturedImageAttachmentId
        {
            get { return featuredImageAttachmentId; }
            set { featuredImageAttachmentId = value; }
        }

        private long userId;
        /// <summary>
        /// 发布者的UserId
        /// </summary>
        public long UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        private string author = string.Empty;
        /// <summary>
        /// 发布者DisplayName
        /// </summary>
        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        private string summary = string.Empty;
        /// <summary>
        /// 摘要信息
        /// </summary>
        public string Summary
        {
            get { return summary; }
            set { summary = value; }
        }

        private bool isContributed = false;
        /// <summary>
        /// 是否用户投稿
        /// </summary>
        public bool IsContributed
        {
            get { return isContributed; }
            set { isContributed = value; }
        }

        private bool isEssential = false;
        /// <summary>
        /// 是否精华
        /// </summary>
        public bool IsEssential
        {
            get { return isEssential; }
            set { isEssential = value; }
        }

        private bool isGlobalSticky = false;
        /// <summary>
        /// 是否全局置顶
        /// </summary>
        public bool IsGlobalSticky
        {
            get { return isGlobalSticky; }
            set { isGlobalSticky = value; }
        }

        private DateTime globalStickyDate = ValueUtility.GetSafeSqlDateTime(DateTime.MinValue);
        /// <summary>
        /// 全局置顶截止日期
        /// </summary>
        public DateTime GlobalStickyDate
        {
            get { return globalStickyDate; }
            set { globalStickyDate = value; }
        }

        private bool isFolderSticky = false;
        /// <summary>
        /// 是否栏目置顶
        /// </summary>
        public bool IsFolderSticky
        {
            get { return isFolderSticky; }
            set { isFolderSticky = value; }
        }

        private DateTime folderStickyDate = ValueUtility.GetSafeSqlDateTime(DateTime.MinValue);
        /// <summary>
        /// 栏目置顶截止日期
        /// </summary>
        public DateTime FolderStickyDate
        {
            get { return folderStickyDate; }
            set { folderStickyDate = value; }
        }

        private bool isLocked;
        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsLocked
        {
            get { return isLocked; }
            set { isLocked = value; }
        }

        private string ip = WebUtility.GetIP();
        /// <summary>
        ///ip地址 
        /// </summary>
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        private DateTime releaseDate = DateTime.UtcNow;
        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime ReleaseDate
        {
            get { return releaseDate; }
            set { releaseDate = value; }
        }

        private DateTime dateCreated = DateTime.UtcNow;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime DateCreated
        {
            get { return dateCreated; }
            set { dateCreated = value; }
        }

        private DateTime lastModified = DateTime.UtcNow;
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastModified
        {
            get { return lastModified; }
            set { lastModified = value; }
        }

        private long displayOrder = 0;
        /// <summary>
        /// 排序序号
        /// </summary>
        public long DisplayOrder
        {
            get { return displayOrder; }
            set { displayOrder = value; }
        }


        #region 审核状态

        private AuditStatus auditingStatus = AuditStatus.Success;
        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditStatus AuditStatus
        {
            get { return auditingStatus; }
            set { auditingStatus = value; }
        }
        #endregion


        #endregion

        #region 扩展

        private IDictionary<string, object> additionalProperties = null;
        /// <summary>
        /// 附表中的字段
        /// </summary>
        [Ignore]
        public IDictionary<string, object> AdditionalProperties
        {
            get
            {
                if (additionalProperties == null)
                {
                    additionalProperties = new ContentItemRepository().GetContentItemAdditionalProperties(this.ContentTypeId, this.ContentItemId);
                    if (additionalProperties == null)
                        additionalProperties = new Dictionary<string, object>();
                }
                return additionalProperties;
            }
            set { additionalProperties = value; }
        }

        /// <summary>
        /// 所属栏目
        /// </summary>
        [Ignore]
        public ContentFolder ContentFolder
        {
            get
            {
                ContentFolder contentFolder = new ContentFolderService().Get(ContentFolderId);
                if (contentFolder != null)
                    return contentFolder;
                return new ContentFolder();
            }
        }

        /// <summary>
        /// 内容模型
        /// </summary>
        [Ignore]
        public ContentTypeDefinition ContentType
        {
            get { return new MetadataService().GetContentType(this.ContentTypeId); }
        }


        /// <summary>
        /// 下一资讯ContentItemId
        /// </summary>
        [Ignore]
        public long NextContentItemId
        {
            get { return new ContentItemRepository().GetNextContentItemId(this); }
        }

        /// <summary>
        /// 上一资讯ContentItemId
        /// </summary>
        [Ignore]
        public long PrevContentItemId
        {
            get { return new ContentItemRepository().GetPrevContentItemId(this); }
        }

        /// <summary>
        /// 下一篇资讯
        /// </summary>
        [Ignore]
        public ContentItem NextContentItem
        {
            get
            {
                return new ContentItemService().Get(NextContentItemId);
            }
        }

        /// <summary>
        /// 上一篇资讯
        /// </summary>
        [Ignore]
        public ContentItem PrevContentItem
        {
            get
            {
                return new ContentItemService().Get(PrevContentItemId);
            }
        }

        /// <summary>
        /// 获取ContentItem的解析过的内容
        /// </summary>
        public string GetResolvedBody()
        {
            return new ContentItemRepository().GetResolvedBody(this.ContentItemId);
        }


        /// <summary>
        /// 资讯作者
        /// </summary>
        [Ignore]
        public User User
        {
            get
            {
                IUserService service = DIContainer.Resolve<IUserService>();
                return service.GetFullUser(this.UserId);
            }
        }

        private IEnumerable<string> tagNames;
        /// <summary>
        /// 资讯标签名列表
        /// </summary>
        [Ignore]
        public IEnumerable<string> TagNames
        {
            get
            {
                TagService service = new TagService(TenantTypeIds.Instance().ContentItem());
                IEnumerable<ItemInTag> tags = service.GetItemInTagsOfItem(this.ContentItemId);
                if (tags == null)
                    return new List<string>();
                return tags.Select(n => n.TagName);
            }
            set { }
        }

        /// <summary>
        /// 所属本资讯的附件
        /// </summary>
        [Ignore]
        public IEnumerable<Attachment> Attachments
        {
            get
            {
                IEnumerable<Attachment> attachments = new AttachmentService(TenantTypeIds.Instance().ContentItem()).GetsByAssociateId(this.ContentItemId);
                if (attachments != null)
                    return attachments;
                return new List<Attachment>();
            }
        }
        /// <summary>
        /// 浏览数
        /// </summary>
        [Ignore]
        public int HitTimes
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().ContentItem());
                return countService.Get(CountTypes.Instance().HitTimes(), this.ContentItemId);
            }
        }

        /// <summary>
        /// 阶段浏览数
        /// </summary>
        [Ignore]
        public int StageHitTimes
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().ContentItem());
                return countService.GetStageCount(CountTypes.Instance().HitTimes(), 7, this.ContentItemId);
            }
        }

        /// <summary>
        /// 评论数
        /// </summary>
        [Ignore]
        public int CommentCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().ContentItem());
                return countService.Get(CountTypes.Instance().CommentCount(), this.ContentItemId);
            }
        }

        /// <summary>
        /// 阶段评论数
        /// </summary>
        [Ignore]
        public int StageCommentCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().ContentItem());
                return countService.GetStageCount(CountTypes.Instance().CommentCount(), 7, this.ContentItemId);
            }
        }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.ContentItemId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region IAuditable 实现

        /// <summary>
        /// 审核项Key 
        /// </summary>
        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().CMS_ContentItem(); }
        }

        #endregion

    }
}
