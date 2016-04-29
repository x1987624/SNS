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
    /// 相册的实体
    /// </summary>
    [TableName("spb_Albums")]
    [PrimaryKey("AlbumId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "UserId,OwnerId")]
    [Serializable]
    public class Album : SerializablePropertiesBase, IAuditable, IEntity, IPrivacyable
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static Album New()
        {
            Album album = new Album()
            {
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            return album;
        }

        #region 需持久化属性

        /// <summary>
        ///相册ID
        /// </summary>
        public long AlbumId { get; protected set; }

        /// <summary>
        ///相册名称
        /// </summary>
        public string AlbumName { get; set; }

        /// <summary>
        ///租户类型
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///所有者ID，用户的相册，OwnerId=UserId
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        ///相册作者ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///作者名称
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///相册描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///封面照片ID
        /// </summary>
        public long CoverId { get; set; }

        /// <summary>
        ///照片数
        /// </summary>
        public int PhotoCount { get; set; }

        /// <summary>
        ///排序，默认与主键相同
        /// </summary>
        public long DisplayOrder { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///修改时间
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        ///最后上传照片时间
        /// </summary>
        public DateTime LastUploadDate { get; set; }



        #endregion

        #region 扩展属性及方法

        /// <summary>
        /// 标题图
        /// </summary>
        [Ignore]
        public Photo Cover
        {
            get
            {
                PhotoService photoService = new PhotoService(); 
                Photo photo = photoService.GetPhotosOfAlbum(this.TenantTypeId, this.AlbumId, false, SortBy_Photo.DateCreated_Desc, null, 1, 1).FirstOrDefault();                
                Photo cover =photoService.GetPhoto(this.CoverId);
                
                if (cover != null)
                {
                    return cover;
                }
                else
                {
                    return photo;
                }               
            }
        }

        /// <summary>
        /// 作者
        /// </summary>
        [Ignore]
        public User User
        {
            get { return DIContainer.Resolve<UserService>().GetFullUser(this.UserId); }
        }

        /// <summary>
        /// 标题图不存在时取前9张照片
        /// </summary>
        [Ignore]
        public IEnumerable<Photo> Covers
        {
            get
            {
                if (CoverId > 0)
                {
                    return new List<Photo>();
                }
                else
                {
                    bool ignoreAuditAndPrivacy = false;
                    IUser currentUser = UserContext.CurrentUser;
                    if (currentUser == null)
                    {
                        ignoreAuditAndPrivacy = false;
                    }
                    else
                    {
                        if (UserId == currentUser.UserId)
                        {
                            ignoreAuditAndPrivacy = true;
                        }
                        else
                        {
                            ignoreAuditAndPrivacy = false;
                        }
                    }
                    return new PhotoService().GetPhotosOfAlbum(this.TenantTypeId, this.AlbumId, ignoreAuditAndPrivacy, SortBy_Photo.DateCreated_Desc, null, 9, 1);
                }
            }
        }

        /// <summary>
        /// 克隆当前实体（浅拷贝）
        /// </summary>
        /// <returns></returns>
        public Album Clone()
        {
            return (Album)this.MemberwiseClone();
        }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.AlbumId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region IAuditable 实现
        /// <summary>
        /// 审核项Key 
        /// </summary>
        public string AuditItemKey
        {
            get { return AuditItemKeys.Instance().Album(); }
        }

        public AuditStatus AuditStatus { get; set; }
        #endregion

        #region IPrivacyable 实现
        /// <summary>
        /// 内容项Id
        /// </summary>
        long IPrivacyable.ContentId
        {
            get { return AlbumId; }
        }

        string IPrivacyable.TenantTypeId
        {
            get { return TenantTypeIds.Instance().Album(); }
        }

        long IPrivacyable.UserId
        {
            get { return this.UserId; }
        }

        /// <summary>
        ///隐私状态
        /// </summary>
        public PrivacyStatus PrivacyStatus { get; set; }
        #endregion
    }

}