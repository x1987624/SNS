//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------


using Tunynet;
using System;
using Tunynet.Caching;
namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 积分商城设置
    /// </summary>
    [Serializable]
    [CacheSetting(true)]
    public class PointMallSettings:IEntity
    {
        private string recommendGiftTypeId = "20010101";
        private string recommendGiftTypeIdHome = "20010102";

        /// <summary>
        /// 商品推荐类型ID
        /// </summary>
        public string RecommendGiftTypeId
        {
            get { return recommendGiftTypeId; }
            set { recommendGiftTypeId = value; }
        }

        /// <summary>
        /// 商品推荐类型ID-商品首页
        /// </summary>
        public string RecommendGiftTypeIdHome
        {
            get { return recommendGiftTypeIdHome; }
            set { recommendGiftTypeIdHome = value; }
        }

        #region IEntity 成员

        object IEntity.EntityId
        {
            get { return typeof(PointMallSettings).FullName; }
        }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
        
    }
}