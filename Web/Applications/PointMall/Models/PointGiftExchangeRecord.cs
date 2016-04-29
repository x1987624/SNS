//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using Tunynet;
using PetaPoco;
using Tunynet.Caching;
using Spacebuilder.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 兑换记录的实体
    /// </summary>
    [TableName("spb_PointGiftExchangeRecords")]
    [PrimaryKey("RecordId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "PayerUserId,GiftId")]
    [Serializable]
    public class PointGiftExchangeRecord : SerializablePropertiesBase, IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static PointGiftExchangeRecord New()
        {
            PointGiftExchangeRecord record = new PointGiftExchangeRecord()
            {
                DateCreated = DateTime.UtcNow,
                LastModified =DateTime.UtcNow,
                Status = ApproveStatus.Pending
            };

            return record;
        }

        #region 需持久化属性
        /// <summary>
        ///记录ID
        /// </summary>
        public long RecordId { get; protected set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        public long GiftId { get; set; }

        /// <summary>
        ///商品名称
        /// </summary>
        public string GiftName { get; set; }

        /// <summary>
        ///兑换人ID
        /// </summary>
        public long PayerUserId { get; set; }

        /// <summary>
        ///兑换人姓名
        /// </summary>
        public string Payer { get; set; }

        /// <summary>
        ///数量
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        ///花费的总积分
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        [SqlBehavior(~SqlBehaviorFlags.Update)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///最后更新时间
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// 评价
        /// </summary>
        public string Appraise { get; set; }

        /// <summary>
        /// 评价时间
        /// </summary>
        public DateTime? AppraiseDate { get; set; }

        /// <summary>
        /// 跟踪信息
        /// </summary>
        public string TrackInfo { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public ApproveStatus Status { get; set; }
        #endregion

        #region 序列化属性（邮寄地址）

        /// <summary>
        ///收件人
        /// </summary>
        [Ignore]
        public string Addressee 
        {
            get { return GetExtendedProperty<string>("Addressee"); }
            set { SetExtendedProperty("Addressee", value); } 
        }

        /// <summary>
        ///联系电话
        /// </summary>
        [Ignore]
        public string Tel 
        {
            get { return GetExtendedProperty<string>("Tel"); }
            set { SetExtendedProperty("Tel", value); } 
        }

        /// <summary>
        ///邮寄地址
        /// </summary>
        [Ignore]
        public string Address 
        {
            get { return GetExtendedProperty<string>("Address"); }
            set { SetExtendedProperty("Address", value); } 
        }

        /// <summary>
        ///邮编
        /// </summary>
        [Ignore]
        public string PostCode 
        {
            get { return GetExtendedProperty<string>("PostCode"); }
            set { SetExtendedProperty("PostCode", value); } 
        }

        #endregion

        #region 扩展属性及方法
        /// <summary>
        /// 兑换人
        /// </summary>
        [Ignore]
        public User PayerUser
        {
            get { return DIContainer.Resolve<UserService>().GetFullUser(this.PayerUserId); }
        }

        /// <summary>
        /// 商品
        /// </summary>
        [Ignore]
        public PointGift PointGift
        {
            get { return new PointMallService().GetGift(this.GiftId); }
        }
        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.RecordId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion     
    }    
}