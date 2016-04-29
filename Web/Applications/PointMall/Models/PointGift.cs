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

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 商品的实体
    /// </summary>
    [TableName("spb_PointGifts")]
    [PrimaryKey("GiftId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "GiftId")]
    [Serializable]
    public class PointGift : SerializablePropertiesBase, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static PointGift New()
        {
            PointGift gifts = new PointGift()
            {
                DateCreated = DateTime.UtcNow,
                LastModified=DateTime.UtcNow
            };

            return gifts;
        }

        #region 需持久化属性
        /// <summary>
        ///商品ID
        /// </summary>
        public long GiftId { get; protected set; }

        /// <summary>
        ///商品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///商品描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///添加人UserId
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 附件ID
        /// </summary>
        public long FeaturedImageAttachmentId { get; set; }

        /// <summary>
        ///商品展示IMG
        /// </summary>
        public string FeaturedImage { get; set; }

        /// <summary>
        ///单价
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// 兑换总数
        /// </summary>
        public int ExchangedCount { get; set; }

        /// <summary>
        ///是否上架
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 商品图片ID
        /// </summary>
        public string FeaturedImageIds { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; protected set; }

        /// <summary>
        ///修改时间
        /// </summary>
        public DateTime LastModified { get; set; }
        #endregion

        #region 扩展属性及方法

        /// <summary>
        /// 获取商品的展示图片地址
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> FeaturedImageUrls()
        {
            List<string> imageUrls = new List<string>();

            string[] ids = null;

            if (!string.IsNullOrEmpty(this.FeaturedImageIds))
            {
                ids = this.FeaturedImageIds.Split(',');

                Attachment attachemnt = null;
                var attachmentService = new AttachmentService(TenantTypeIds.Instance().PointGift());
                foreach (string id in ids)
                {
                    attachemnt = attachmentService.Get(Convert.ToInt64(id));
                    imageUrls.Add(attachemnt.GetRelativePath());
                    attachemnt = null;
                }
            }

            return imageUrls;
        }

        /// <summary>
        /// 兑换数量
        /// </summary>
        public int ExchangeNumber
        {
            get { return new PointMallService().GetGiftExchangeNumber(this.GiftId); }
        }

        /// <summary>
        /// 类别
        /// </summary>
        public Category Category
        {
            get { return new CategoryService().GetCategoriesOfItem(this.GiftId, 0, TenantTypeIds.Instance().PointGift()).FirstOrDefault(); }
        }

        /// <summary>
        /// 浏览计数
        /// </summary>
        public int HitCount
        {
            get
            {
                CountService countService = new CountService(TenantTypeIds.Instance().PointGift());
                return countService.GetStageCount(CountTypes.Instance().HitTimes(),7,this.GiftId);
            }
        }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.GiftId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
    }

}