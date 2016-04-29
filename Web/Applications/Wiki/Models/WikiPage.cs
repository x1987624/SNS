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
using System.Text;
using PetaPoco;
using Tunynet;
using Tunynet.Caching;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 词条
    /// </summary>
    [TableName("spb_WikiPages")]
    [PrimaryKey("PageId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "OwnerId,UserId")]
    [Serializable]
    public class WikiPage : SerializablePropertiesBase, IAuditable, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static WikiPage New()
        {
            WikiPage wikiPage = new WikiPage()
            {
                Title = string.Empty,
                Author = string.Empty,
                TenantTypeId = TenantTypeIds.Instance().Wiki(),
                AuditStatus = AuditStatus.Success,
                FeaturedImageAttachmentId = 0,
                FeaturedImage = string.Empty,
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
            return wikiPage;
        }

        #region 需持久化属性

        /// <summary>
        ///词条Id
        /// </summary>
        public long PageId { get; protected set; }

        /// <summary>
        ///租户类型Id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///拥有者Id（独立百科为0；所属为群组时为群组Id）
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        ///词条名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 标题图对应的附件Id
        /// </summary>
        public long FeaturedImageAttachmentId { get; set; }

        /// <summary>
        /// 标题图文件（带部分路径）
        /// </summary>
        public string FeaturedImage { get; set; }

        /// <summary>
        ///创建者UserId
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///创建者DisplayName
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///审核状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        /// <summary>
        ///编辑次数
        /// </summary>
        public int EditionCount { get; set; }

        /// <summary>
        ///是否是精华
        /// </summary>
        public bool IsEssential { get; set; }

        /// <summary>
        ///是否锁定
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        ///是否被逻辑删除
        /// </summary>
        public bool IsLogicalDelete { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///最后更新日期
        /// </summary>
        public DateTime LastModified { get; set; }

        #endregion

        #region 扩展属性

        /// <summary>
        /// WikiPage最新版本
        /// </summary>
        [Ignore]
        public WikiPageVersion LastestVersion
        {
            get
            {
                return new WikiPageVersionRepository().GetLastestVersion(this.PageId);
            }
        }

        /// <summary>
        /// WikiPage最新版本的解析过的内容
        /// </summary>
        [Ignore]
        public string ResolvedBody
        {
            get
            {
                if (LastestVersion == null)
                    return string.Empty;
                return new WikiPageVersionRepository().GetResolvedBody(LastestVersion.VersionId);
            }
        }

        /// <summary>
        /// WikiPage最新版本的内容
        /// </summary>
        [Ignore]
        public string Body
        {
            get
            {
                if (LastestVersion == null)
                    return string.Empty;
                return new WikiPageVersionRepository().GetBody(LastestVersion.VersionId);
            }
        }

        private IEnumerable<string> tagNames;
        /// <summary>
        /// 词条标签名列表
        /// </summary>
        [Ignore]
        public IEnumerable<string> TagNames
        {
            get
            {
                if (tagNames == null)
                {
                    TagService service = new TagService(TenantTypeIds.Instance().WikiPage());
                    IEnumerable<ItemInTag> tags = service.GetItemInTagsOfItem(this.PageId);
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
                CountService countService = new CountService(TenantTypeIds.Instance().WikiPage());
                return countService.Get(CountTypes.Instance().HitTimes(), this.PageId);
            }
        }
        /// <summary>
        /// Wiki 分类
        /// </summary>
        [Ignore]
        public Category SiteCategory
        {
            get
            {
                IEnumerable<Category> categories = new CategoryService().GetCategoriesOfItem(this.PageId, 0, TenantTypeIds.Instance().WikiPage());
                Category category = Category.New(); 
                if (categories != null && categories.Count() > 0)
                {
                    category = categories.First();
                }
                return category;
            }
        }
        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.PageId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region IAuditable 实现

        /// <summary>
        /// 审核项Key 
        /// </summary>
        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().Wiki_Page(); }
        }

        #endregion

    }
}
