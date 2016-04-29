//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------


using Tunynet;
using System;
using Tunynet.Caching;
namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册设置
    /// </summary>
    [Serializable]
    [CacheSetting(true)]
    public class PhotoSettings:IEntity
    {

        private string recommendAlbumTypeId = "10030101";
        /// <summary>
        /// 相册推荐类型ID
        /// </summary>
        public string RecommendAlbumTypeId
        {
            get { return recommendAlbumTypeId; }
            set { recommendAlbumTypeId = value; }
        }     

        private string recommendPhotoTypeId = "10030201";
        /// <summary>
        /// 照片推荐类型ID
        /// </summary>
        public string RecommendPhotoTypeId
        {
            get { return recommendPhotoTypeId; }
            set { recommendPhotoTypeId = value; }
        }

        private string recommendTagTypeId = "10030202";
        /// <summary>
        /// 标签推荐类型ID
        /// </summary>
        public string RecommendTagTypeId
        {
            get { return recommendTagTypeId; }
            set { recommendTagTypeId = value; }
        }


        #region IEntity 成员

        object IEntity.EntityId
        {
            get { return typeof(PhotoSettings).FullName; }
        }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
        
    }
}