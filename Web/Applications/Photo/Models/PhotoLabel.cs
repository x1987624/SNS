//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using Tunynet;
using Tunynet.Common;
using PetaPoco;
using Tunynet.Caching;
using Spacebuilder.Common;

namespace Spacebuilder.Photo
{

    /// <summary>
    /// 圈人实体
    /// </summary>
    [TableName("spb_PhotoLabels")]
    [PrimaryKey("LabelId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "PhotoId")]
    [Serializable]
    public class PhotoLabel : SerializablePropertiesBase, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static PhotoLabel New()
        {
            PhotoLabel label = new PhotoLabel()
            {
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            return label;
        }

        #region 需持久化属性

        /// <summary>
        ///圈人Id
        /// </summary>
        public long LabelId { get; protected set; }

        /// <summary>
        ///照片ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        ///租户类型ID
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        ///被圈人（对象）ID
        /// </summary>
        public long ObjetId { get; set; }

        /// <summary>
        ///被圈人（对象）名称
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// 操作者Id
        /// </summary>
        public long UserId { get; set; }


        /// <summary>
        ///描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///被圈区域X坐标
        /// </summary>
        public int AreaX { get; set; }

        /// <summary>
        ///被圈区域Y坐标
        /// </summary>
        public int AreaY { get; set; }

        /// <summary>
        ///被圈区域宽度
        /// </summary>
        public int AreaWidth { get; set; }

        /// <summary>
        ///被圈区域高度
        /// </summary>
        public int AreaHeight { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///修改时间
        /// </summary>
        public DateTime LastModified { get; set; }



        #endregion

        #region 扩展属性

        /// <summary>
        /// 图片
        /// </summary>
        [Ignore]
        public Photo Photo
        {
            get { return new PhotoService().GetPhoto(this.PhotoId); }
        }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.LabelId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
    }
}
