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
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 词条版本
    /// </summary>
    [TableName("spb_WikiPageVersions")]
    [PrimaryKey("VersionId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "PageId,OwnerId,UserId", PropertyNameOfBody = "Body")]
    [Serializable]
    public class WikiPageVersion : SerializablePropertiesBase, IAuditable, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static WikiPageVersion New()
        {
            WikiPageVersion wikiPageVersion = new WikiPageVersion()
            {
                Title = string.Empty,
                Author = string.Empty,
                Summary = string.Empty,
                TenantTypeId = TenantTypeIds.Instance().Wiki(),
                Reason = string.Empty,
                DateCreated = DateTime.UtcNow,
                IP = WebUtility.GetIP()
            };
            return wikiPageVersion;
        }

        #region 需持久化属性

        /// <summary>
        ///VersionId
        /// </summary>
        public long VersionId { get; protected set; }

        /// <summary>
        ///PageId
        /// </summary>
        public long PageId { get; set; }

        /// <summary>
        ///租户类型Id
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///拥有者Id（独立百科为0；所属为群组时为群组Id）
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        ///版本号
        /// </summary>
        public int VersionNum { get; set; }

        /// <summary>
        ///词条名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///编辑者UserId
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///编辑者DisplayName
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///摘要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        ///内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        ///修改原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        ///审核状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        /// <summary>
        ///发布日期
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///IP地址
        /// </summary>
        public string IP { get; set; }

        #endregion

        #region 扩展属性
        /// <summary>
        /// 获取WikiPageVersion的Body
        /// </summary>
        /// <remarks>
        /// 由于使用独立的实体内容缓存，Body属性已经置为null
        /// </remarks>
        /// <returns></returns>
        public string GetBody()
        {
            return new WikiPageVersionRepository().GetBody(this.VersionId);
        }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.VersionId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region IAuditable 实现

        /// <summary>
        /// 审核项Key 
        /// </summary>
        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().Wiki_PageVersion(); }
        }

        #endregion

        #region 导航属性

        /// <summary>
        /// 所有者分类id
        /// </summary>
        [Ignore]
        public IEnumerable<Category> Categories
        {
            get
            {
                IEnumerable<Category> categories = new CategoryService().GetCategoriesOfItem(this.VersionId, 0, TenantTypeIds.Instance().Wiki());
                
                return categories;
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
                return new WikiPageVersionRepository().GetResolvedBody(VersionId);
            }
        }

        private IEnumerable<string> tagNames;
        /// <summary>
        /// 标签名列表
        /// </summary>
        [Ignore]
        public IEnumerable<string> TagNames
        {
            get
            {
                if (tagNames == null)
                {
                    TagService service = new TagService(TenantTypeIds.Instance().Wiki());
                    IEnumerable<ItemInTag> tags = service.GetItemInTagsOfItem(this.VersionId);
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
        /// 主题帖作者
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

        #endregion
    }
}
