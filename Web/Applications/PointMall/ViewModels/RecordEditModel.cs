using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Spacebuilder.Common;
using Tunynet.Utilities;

namespace Spacebuilder.PointMall
{
    public class RecordEditModel : Controller
    {
        /// <summary>
        /// 构造器，初始化数量为1
        /// </summary>
        public RecordEditModel()
        {
            this.Number = 1;
        }

        #region 兑换申请的属性
        /// <summary>
        ///记录ID
        /// </summary>
        public long RecordId { get; set; }

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
        [Display(Name = "数量")]
        public int Number { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///最后更新时间
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// 跟踪信息
        /// </summary>
        public string TrackInfo { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public ApproveStatus Status { get; set; }

        #endregion

        #region 邮寄地址的属性

        /// <summary>
        ///收件人
        /// </summary>
        [Display(Name = "收件人")]
        [Required(ErrorMessage = "必须输入联系人！")]
        [StringLength(128, ErrorMessage = "不能超过30个字符！")]
        public string Addressee { get; set; }

        /// <summary>
        ///联系电话
        /// </summary>
        [Display(Name = "联系电话")]
        [Required(ErrorMessage = "必须输入联系电话！")]
        [StringLength(64,ErrorMessage="长度不能超过64！")]
        public string Tel { get; set; }

        /// <summary>
        ///邮寄地址
        /// </summary>
        [Display(Name = "邮寄地址")]
        [StringLength(512, ErrorMessage = "不能超过512个字符！")]
        [Required(ErrorMessage = "必须输入邮寄地址！")]
        public string Address { get; set; }

        /// <summary>
        ///邮编
        /// </summary>
        [Display(Name = "邮编")]
        [StringLength(30, ErrorMessage = "不能超过30个字符")]
        [Required(ErrorMessage = "必须输入邮编！")]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "只能输入字母或数字")]
        public string PostCode { get; set; }

        #endregion

        /// <summary>
        /// 转化为数据库实体
        /// </summary>
        /// <returns></returns>
        public PointGiftExchangeRecord AsPointGiftExchangeRecord()
        {

            PointGiftExchangeRecord record = PointGiftExchangeRecord.New();
            record.Address = this.Address ?? string.Empty;
            record.Addressee = this.Addressee ?? string.Empty;
            record.Appraise = string.Empty;
            record.AppraiseDate = null;
            record.DateCreated = DateTime.UtcNow;
            record.GiftId = this.GiftId;
            record.GiftName = record.PointGift.Name;
            record.LastModified = DateTime.UtcNow;
            record.Number = this.Number;
            record.Payer = UserContext.CurrentUser.DisplayName;
            record.PayerUserId = UserContext.CurrentUser.UserId;
            record.PostCode = this.PostCode ?? string.Empty;
            record.Status = this.Status;
            record.TrackInfo = string.Empty;
            record.Tel = this.Tel;
            return record;
        }

    }

    /// <summary>
    /// 数据库实体扩展类
    /// </summary>
    public static class PointGiftExchangeRecordExtensions
    {
        /// <summary>
        /// 转化为ViewModel
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static RecordEditModel AsRecordEditModel(this PointGiftExchangeRecord record)
        {
            return new RecordEditModel
            {

                Address = record.Address,
                GiftName = record.GiftName,
                Addressee = record.Addressee,
                PostCode = record.PostCode,
                Tel = record.Tel,
                DateCreated = record.DateCreated,
                GiftId = record.GiftId,
                LastModified = record.LastModified,
                Number = record.Number,
                Payer = record.Payer,
                PayerUserId = record.PayerUserId,
                RecordId = record.RecordId,
                Status = record.Status,
                TrackInfo = record.TrackInfo,
            };
        }
    }
}
