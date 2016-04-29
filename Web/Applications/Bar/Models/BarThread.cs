using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PetaPoco;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 帖子
    /// </summary>
    [Serializable]
    [TableName("spb_BarThreads")]
    [PrimaryKey("ThreadId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "UserId,SectionId", PropertyNameOfBody = "Body")]
    public class BarThread : SerializablePropertiesBase, IAuditable, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>        
        public static BarThread New()
        {
            BarThread barThread = new BarThread()
            {
                Author = string.Empty,
                Subject = string.Empty,
                StickyDate = DateTime.UtcNow,
                HighlightStyle = string.Empty,
                HighlightDate = DateTime.UtcNow,
                IP = WebUtility.GetIP(),
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
            return barThread;
        }

        #region 需持久化属性

        /// <summary>
        ///ThreadId
        /// </summary>
        public long ThreadId { get; protected set; }

        /// <summary>
        ///所属帖吧Id
        /// </summary>
        public long SectionId { get; set; }

        /// <summary>
        ///所属帖吧租户类型Id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///所属帖吧拥有者Id（例如：群组Id）
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        ///主题作者用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///主题作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///帖子标题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        ///帖子内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        ///是否锁定
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        ///是否精华
        /// </summary>
        // [SqlBehavior(~SqlBehaviorFlags.Update)]
        public bool IsEssential { get; set; }

        /// <summary>
        ///是否置顶
        /// </summary>
        // [SqlBehavior(~SqlBehaviorFlags.Update)]
        public bool IsSticky { get; set; }

        /// <summary>
        ///置顶期限
        /// </summary>
        // [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime StickyDate { get; set; }

        /// <summary>
        ///是否仅回复可见
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        ///高亮显示的样式代码
        /// </summary>
        //[SqlBehavior(~SqlBehaviorFlags.Update)]
        public string HighlightStyle { get; set; }

        /// <summary>
        ///高亮显示期限
        /// </summary>
        //[SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime HighlightDate { get; set; }

        /// <summary>
        ///售价（交易积分）
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// 回帖数
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public int PostCount { get; set; }

        /// <summary>
        ///发帖人IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///最后更新日期（被回复时也需要更新时间）
        /// </summary>
        public DateTime LastModified { get; set; }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.ThreadId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region 扩展属性
        /// <summary>
        /// 下一主题ThreadId
        /// </summary>
        [Ignore]
        public long NextThreadId
        {
            get { return new BarThreadRepository().GetNextThreadId(this); }

        }

        /// <summary>
        /// 上一主题ThreadId
        /// </summary>
        [Ignore]
        public long PrevThreadId
        {
            get { return new BarThreadRepository().GetPrevThreadId(this); }
        }

        /// <summary>
        /// 下一篇帖子
        /// </summary>
        [Ignore]
        public BarThread NextThread
        {
            get
            {
                return new BarThreadService().Get(NextThreadId);
            }
        }

        /// <summary>
        /// 上一篇帖子
        /// </summary>
        [Ignore]
        public BarThread PrevThread
        {
            get
            {
                return new BarThreadService().Get(PrevThreadId);
            }
        }

        /// <summary>
        /// 主题作者
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

        /// <summary>
        /// 所属帖吧
        /// </summary>
        [Ignore]
        public BarSection BarSection
        {
            get
            {
                BarSectionService barSectionService = new BarSectionService();
                return barSectionService.Get(this.SectionId);
            }
        }

        /// <summary>
        /// 帖子的分类id
        /// </summary>
        [Ignore]
        public long? CategoryId
        {
            get
            {
                IEnumerable<Category> selectedCategories = new CategoryService().GetCategoriesOfItem(this.ThreadId, this.SectionId, TenantTypeIds.Instance().BarThread());
                long? selectedCategoryId = null;
                if (selectedCategories != null && selectedCategories.Count() > 0)
                    selectedCategoryId = selectedCategories.First().CategoryId;
                return selectedCategoryId;
            }
        }

        /// <summary>
        /// 帖子的分类
        /// </summary>
        [Ignore]
        public Category Category
        {
            get
            {
                IEnumerable<Category> selectedCategories = new CategoryService().GetCategoriesOfItem(this.ThreadId, this.SectionId, TenantTypeIds.Instance().BarThread());
                Category category = null;
                if (selectedCategories != null && selectedCategories.Count() > 0)
                    category = selectedCategories.First();
                return category;
            }
        }


        /// <summary>
        /// 主题标签名列表
        /// </summary>
        [Ignore]
        public IEnumerable<string> TagNames
        {
            get
            {
                TagService service = new TagService(TenantTypeIds.Instance().BarThread());
                IEnumerable<ItemInTag> tags = service.GetItemInTagsOfItem(this.ThreadId);
                if (tags == null)
                    return new List<string>();
                return tags.Select(n => n.TagName);
            }
        }

        /// <summary>
        /// 所属本帖子的
        /// </summary>
        [Ignore]
        public IEnumerable<Attachment> Attachments
        {
            get
            {
                IEnumerable<Attachment> attachments = new AttachmentService(TenantTypeIds.Instance().BarThread()).GetsByAssociateId(this.ThreadId);
                if (attachments != null)
                    return attachments;
                return new List<Attachment>();
            }
        }

        #endregion

        #region IAuditable 实现
        /// <summary>
        /// 审核项Key 
        /// </summary>
        string IAuditable.AuditItemKey
        {
            get { return AuditItemKeys.Instance().Bar_Thread(); }
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        //[SqlBehavior(~SqlBehaviorFlags.Update)]
        public AuditStatus AuditStatus { get; set; }

        #endregion

        #region 计数

        /// <summary>
        /// 浏览数
        /// </summary>
        [Ignore]
        public int HitTimes
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().BarThread());
                return countService.Get(CountTypes.Instance().HitTimes(), this.ThreadId);
            }
        }

        /// <summary>
        /// 今日浏览数
        /// </summary>
        [Ignore]
        public int ToDayHitTimes
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().BarThread());
                return countService.GetStageCount(CountTypes.Instance().HitTimes(), 1, this.ThreadId);
            }
        }

        /// <summary>
        /// 最近7天浏览数
        /// </summary>
        [Ignore]
        public int Last7DaysHitTimes
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().BarThread());
                return countService.GetStageCount(CountTypes.Instance().HitTimes(), 7, this.ThreadId);
            }
        }

        #endregion


        #region 方法

        /// <summary>
        /// 获取BarThread的Body
        /// </summary>
        /// <remarks>
        /// 由于使用独立的实体内容缓存，Body属性已经置为null
        /// </remarks>
        /// <returns></returns>
        public string GetBody()
        {
            return new BarThreadRepository().GetBody(this.ThreadId);
        }

        /// <summary>
        /// 获取BarThread的解析过的内容
        /// </summary>
        public string GetResolvedBody()
        {
            return new BarThreadRepository().GetResolvedBody(this.ThreadId);
        }

        /// <summary>
        /// 获取帖子的最新一条回复贴
        /// </summary>
        /// <returns></returns>
        public BarPost GetNewestPost()
        {
            return new BarPostRepository().GetNewestPost(this.ThreadId);
        }

        #endregion
    }
}