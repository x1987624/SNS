//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet;
using Tunynet.Common;
using PetaPoco;
using Tunynet.Caching;
using Tunynet.Utilities;
using Spacebuilder.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 照片的实体
    /// </summary>
    [TableName("spb_Photos")]
    [PrimaryKey("PhotoId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "AlbumId,UserId,OwnerId")]
    [Serializable]
    public class Photo : SerializablePropertiesBase, IAuditable, IEntity, IPrivacyable
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static Photo New()
        {
            Photo photo = new Photo()
            {
                IP = WebUtility.GetIP(),
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            return photo;
        }

        #region 需持久化属性

        /// <summary>
        ///照片ID
        /// </summary>
        public long PhotoId { get; protected set; }

        /// <summary>
        ///相册ID
        /// </summary>
        public long AlbumId { get; set; }

        /// <summary>
        ///租户类型，同步自相册
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///所有者ID，同步自相册
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        ///相册作者ID，同步自相册
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///作者名称，同步自相册
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///照片相对物理地址，用于直连访问
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        ///采集照片的原始地址
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public string OriginalUrl { get; set; }

        /// <summary>
        ///照片描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///是否精华
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public bool IsEssential { get; set; }

        /// <summary>
        ///ip地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        ///隐私状态，同步自相册
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public PrivacyStatus PrivacyStatus { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///修改时间
        /// </summary>
        public DateTime LastModified { get; set; }



        #endregion

        #region 扩展属性及方法

        /// <summary>
        /// 相册
        /// </summary>
        [Ignore]
        public Album Album
        {
            get { return new PhotoService().GetAlbum(this.AlbumId); }
        }

        /// <summary>
        /// 作者
        /// </summary>
        [Ignore]
        public User User
        {
            get { return DIContainer.Resolve<IUserService>().GetFullUser(this.UserId); }
        }

        /// <summary>
        /// 标签
        /// </summary>
        [Ignore]
        public IEnumerable<Tag> Tags
        {
            get
            {
                return new TagService(TenantTypeIds.Instance().Photo()).GetTopTagsOfItem(this.PhotoId, 20);
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
                CountService countService = new CountService(TenantTypeIds.Instance().Photo());
                return countService.Get(CountTypes.Instance().CommentCount(), this.PhotoId);
            }
        }

        /// <summary>
        /// 照片7天内计数
        /// </summary>
        [Ignore]
        public int RecentHitCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().Photo());
                return countService.GetStageCount(CountTypes.Instance().HitTimes(),7,this.PhotoId);
            }
        }
        #endregion


        #region IEntity 成员

        object IEntity.EntityId { get { return this.PhotoId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region IAuditable 实现

        /// <summary>
        /// 审核项Key 
        /// </summary>
        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().Photo(); }
        }

        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        #endregion

        #region IPrivacyable 实现

        /// <summary>
        /// 内容项Id
        /// </summary>
        long IPrivacyable.ContentId
        {
            get { return PhotoId; }
        }

        string IPrivacyable.TenantTypeId
        {
            get { return TenantTypeIds.Instance().Photo(); }
        }

        long IPrivacyable.UserId
        {
            get { return this.UserId; }
        }
        #endregion
    }

}