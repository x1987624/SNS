//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-08-28</createdate>
//<author>bianchx</author>
//<email>bianchx@tunynet.com</email>
//<log date="2012-08-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

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
using System.ComponentModel.DataAnnotations;

namespace Spacebuilder.Bar
{
    /// <summary>
    /// 回帖实体
    /// </summary>
    [Serializable]
    [TableName("spb_BarPosts")]
    [PrimaryKey("PostId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "UserId,ThreadId,ParentId", PropertyNameOfBody = "Body")]
    public class BarPost : IAuditable, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        //todo:需要检查成员初始化的类型是否正确
        public static BarPost New()
        {
            BarPost barPost = new BarPost()
            {
                Author = string.Empty,
                Subject = string.Empty,
                IP = WebUtility.GetIP(),
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };
            return barPost;
        }

        #region 需持久化属性

        /// <summary>
        ///PostId
        /// </summary>
        public long PostId { get; protected set; }

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
        ///所属帖子Id
        /// </summary>
        public long ThreadId { get; set; }

        /// <summary>
        ///父回帖Id
        /// </summary>
        public long ParentId { get; set; }

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
        ///发帖人IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        ///子回复数
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public int ChildPostCount { get; set; }

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

        #region 扩展
        /// <summary>
        /// 主题帖
        /// </summary>
        [Ignore]
        public BarThread Thread
        {
            get
            {
                BarThreadService service = new BarThreadService();
                return service.Get(this.ThreadId);
            }
        }

        /// <summary>
        /// 回帖所属的帖吧
        /// </summary>
        [Ignore]
        public BarSection Section
        {
            get
            {
                BarSectionService service = new BarSectionService();
                return service.Get(this.SectionId);
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
                return DIContainer.Resolve<UserService>().GetFullUser(this.UserId);
            }
        }

        #endregion

        #region IAuditable 实现
        /// <summary>
        /// 审核项Key 
        /// </summary>
        string IAuditable.AuditItemKey
        {
            get { return AuditItemKeys.Instance().Bar_Post(); }
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        #endregion

        #region 方法

        /// <summary>
        /// 获取BarPost的Body
        /// </summary>
        /// <remarks>
        /// 由于使用独立的实体内容缓存，Body属性已经置为null
        /// </remarks>
        /// <returns></returns>
        public string GetBody()
        {
            return new BarPostRepository().GetBody(this.PostId);
        }

        /// <summary>
        /// 获取BarPost的解析过的内容
        /// </summary>
        public string GetResolvedBody()
        {
            return new BarPostRepository().GetResolvedBody(this.PostId);
        }

        #endregion



        #region IEntity
        object IEntity.EntityId
        {
            get { return this.PostId; }
        }

        bool IEntity.IsDeletedInDatabase { get; set; }
        #endregion
    }
}