//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-10-15</createdate>
//<author>wanf</author>
//<email>wanf@tunynet.com</email>
//<log date="2012-10-15" version="0.5">新建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Blog
{
    /// <summary>
    /// 日志实体
    /// </summary>
    [TableName("spb_BlogThreads")]
    [PrimaryKey("ThreadId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "UserId,OwnerId", PropertyNameOfBody = "Body")]
    [Serializable]
    public class BlogThread : SerializablePropertiesBase, IAuditable, IPrivacyable, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static BlogThread New()
        {
            BlogThread blogThread = new BlogThread()
            {
                IsEssential = false,
                IsReproduced = false,
                IP = WebUtility.GetIP(),
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            return blogThread;
        }

        #region 需持久化属性

        /// <summary>
        ///ThreadId
        /// </summary>
        public long ThreadId { get; protected set; }

        /// <summary>
        ///租户类型Id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///拥有者Id（例如：用户Id、群组Id）
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        /// 标题图对应的附件Id
        /// </summary>
        public long FeaturedImageAttachmentId { get; set; }

        /// <summary>
        /// 标题图文件（带部分路径）
        /// </summary>
        public string FeaturedImage { get; set; }

        /// <summary>
        /// 最后更新日期
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        ///日志作者UserId
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///标题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        ///内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        ///摘要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        ///关键字
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        ///是否草稿
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        ///是否锁定（锁定的日志不允许评论）
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        ///是否精华
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public bool IsEssential { get; set; }

        /// <summary>
        ///是否置顶
        /// </summary>
        public bool IsSticky { get; set; }

        /// <summary>
        ///是否转载
        /// </summary>
        public bool IsReproduced { get; set; }

        /// <summary>
        ///被转载用户Id
        /// </summary>
        public long OriginalAuthorId { get; set; }

        /// <summary>
        ///发布人IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///审核状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        /// <summary>
        ///隐私状态
        /// </summary>
        public PrivacyStatus PrivacyStatus { get; set; }

        #endregion

        #region 扩展属性和方法

        /// <summary>
        /// 原作者名称
        /// </summary>
        [Ignore]
        public string OriginalAuthorName
        {
            get { return this.OriginalAuthorId > 0 ? DIContainer.Resolve<UserService>().GetUser(this.OriginalAuthorId).DisplayName : string.Empty; }
        }

        /// <summary>
        /// 被转载的日志id
        /// </summary>
        [Ignore]
        public long OriginalThreadId { get; set; }


        /// <summary>
        /// 上一日志ThreadId
        /// </summary>
        [Ignore]
        public long PrevThreadId
        {
            get { return new BlogService().GetPrevThreadId(this); }
        }

        /// <summary>
        /// 上一日志
        /// </summary>
        public BlogThread PrevThread
        {
            get
            {
                return PrevThreadId > 0 ? new BlogService().Get(PrevThreadId) : null;
            }
        }

        /// <summary>
        /// 下一日志ThreadId
        /// </summary>
        [Ignore]
        public long NextThreadId
        {
            get { return new BlogService().GetNextThreadId(this); }
        }

        /// <summary>
        /// 下一日志
        /// </summary>
        public BlogThread NextThread
        {
            get
            {
                return NextThreadId > 0 ? new BlogService().Get(NextThreadId) : null;
            }
        }

        /// <summary>
        /// 获取BlogThread的Body
        /// </summary>
        /// <remarks>
        /// 由于使用独立的实体内容缓存，Body属性已经置为null
        /// </remarks>
        /// <returns></returns>
        public string GetBody()
        {
            return new BlogThreadRepository().GetBody(this.ThreadId);
        }

        /// <summary>
        /// 获取BlogThread的解析过的内容
        /// </summary>
        public string GetResolvedBody()
        {
            return new BlogThreadRepository().GetResolvedBody(this.ThreadId);
        }

        /// <summary>
        /// 根据是否转载返回带前缀的标题
        /// </summary>
        [Ignore]
        public string ResolvedSubject
        {
            get
            {
                return this.IsReproduced ? Tunynet.Globalization.ResourceAccessor.GetString("Reproduced_Subject_Prefix", ApplicationIds.Instance().Blog()) + this.Subject : this.Subject;
            }
        }

        /// <summary>
        /// 所有者分类id
        /// </summary>
        [Ignore]
        public IEnumerable<Category> OwnerCategories
        {
            get
            {
                IEnumerable<Category> categories = new CategoryService().GetCategoriesOfItem(this.ThreadId, this.OwnerId, TenantTypeIds.Instance().BlogThread());

                return categories;
            }
        }

        private IEnumerable<string> ownerCategoryNames;
        /// <summary>
        /// 所有者分类名称
        /// </summary>
        [Ignore]
        public IEnumerable<string> OwnerCategoryNames
        {
            get
            {
                if (ownerCategoryNames == null || ownerCategoryNames.Count() == 0)
                {
                    IEnumerable<Category> categories = new CategoryService().GetCategoriesOfItem(this.ThreadId, this.OwnerId, TenantTypeIds.Instance().BlogThread());

                    return categories.Select(n => n.CategoryName);
                }
                else
                {
                    return ownerCategoryNames;
                }
            }
            set
            {
                ownerCategoryNames = value;
            }
        }

        /// <summary>
        /// 站点分类id
        /// </summary>
        [Ignore]
        public long? SiteCategoryId
        {
            get
            {
                IEnumerable<Category> categories = new CategoryService().GetCategoriesOfItem(this.ThreadId, 0, TenantTypeIds.Instance().BlogThread());
                long? categoryId = null;
                if (categories != null && categories.Count() > 0)
                {
                    categoryId = categories.First().CategoryId;
                }
                return categoryId;
            }
        }

        /// <summary>
        /// 站点分类名称
        /// </summary>
        [Ignore]
        public string SiteCategoryName
        {
            get
            {
                IEnumerable<Category> categories = new CategoryService().GetCategoriesOfItem(this.ThreadId, 0, TenantTypeIds.Instance().BlogThread());
                string categoryName = string.Empty;
                if (categories != null && categories.Count() > 0)
                {
                    categoryName = categories.First().CategoryName;
                }
                return categoryName;
            }
        }

        private IEnumerable<string> tagNames;
        /// <summary>
        /// 主题标签名列表
        /// </summary>
        [Ignore]
        public IEnumerable<string> TagNames
        {
            get
            {
                if (tagNames == null)
                {
                    TagService service = new TagService(TenantTypeIds.Instance().BlogThread());
                    IEnumerable<ItemInTag> tags = service.GetItemInTagsOfItem(this.ThreadId);
                    if (tags == null)
                    {
                        return new List<string>();
                    }
                    return tags.Select(n => n.TagName);
                }
                else
                {
                    return tagNames;
                }
            }
            set
            {
                tagNames = value;
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
                CountService countService = new CountService(TenantTypeIds.Instance().BlogThread());
                return countService.Get(CountTypes.Instance().HitTimes(), this.ThreadId);
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
                CountService countService = new CountService(TenantTypeIds.Instance().BlogThread());
                return countService.Get(CountTypes.Instance().CommentCount(), this.ThreadId);
            }
        }

        /// <summary>
        /// 被转载数
        /// </summary>
        [Ignore]
        public int ReproduceCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().BlogThread());
                return countService.Get(CountTypes.Instance().ReproduceCount(), this.ThreadId);
            }
        }

        /// <summary>
        /// 日志作者
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
        /// 克隆当前实体（浅拷贝）
        /// </summary>
        /// <returns></returns>
        public BlogThread Clone()
        {
            return (BlogThread)this.MemberwiseClone();
        }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.ThreadId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region IAuditable 实现

        /// <summary>
        /// 审核项Key 
        /// </summary>
        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().Blog_Thread(); }
        }

        #endregion

        #region IPrivacyable 实现

        /// <summary>
        /// 内容项Id
        /// </summary>
        long IPrivacyable.ContentId
        {
            get { return ThreadId; }
        }

        string IPrivacyable.TenantTypeId
        {
            get { return TenantTypeIds.Instance().BlogThread(); }
        }

        long IPrivacyable.UserId
        {
            get { return this.UserId; }
        }
        #endregion





    }
}