//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 创建商品的EditModel
    /// </summary>
    public class PointGiftEditModel
    {

        #region 属性

        /// <summary>
        ///商品ID
        /// </summary>
        public long GiftId { get; set; }

        /// <summary>
        ///商品名称
        /// </summary>
        [Display(Name = "名称")]
        [Required(ErrorMessage = "必须输入商品名！")]
        [StringLength(128, ErrorMessage = "不能超过128个字符！")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        ///商品描述
        /// </summary>
        [AllowHtml]
        [Display(Name = "描述")]
        [DataType(DataType.Html)]
        [Required(ErrorMessage = "请输入内容")]
        [StringLength(10000, ErrorMessage = "不能超过10000个字符！")]
        public string Description { get; set; }

        /// <summary>
        ///商品展示IMG
        /// </summary>
        [Display(Name = "展示图片")]
        public string FeaturedImage { get; set; }

        /// <summary>
        ///单价
        /// </summary>
        [Display(Name = "单价")]
        [Required(ErrorMessage = "必须输入单价！")]
        [RegularExpression(@"[0-9]{0,9}", ErrorMessage = "只能输入小于9位的正数！")]
        public int Price { get; set; }

        /// <summary>
        ///是否上架
        /// </summary>
        [Display(Name = "上架")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 商品图片ID
        /// </summary>
        public string FeaturedImageIds { get; set; }

        /// <summary>
        /// 类别Id
        /// </summary>
        [Display(Name = "类别")]
        [Required(ErrorMessage = "必须选择类别！")]
        public long CategoryId { get; set; }



        #endregion

        #region 方法

        /// <summary>
        /// 转化为PointGift
        /// </summary>
        /// <returns>PointGift</returns>
        public PointGift AsPointGift()
        {
            PointGift gift = null;

            if (this.GiftId > 0)
            {
                gift = new PointMallService().GetGift(this.GiftId);
            }
            else
            {
                gift = PointGift.New();
                gift.UserId = UserContext.CurrentUser.UserId;
            }

            gift.Description = this.Description;
            gift.FeaturedImage = this.FeaturedImage ?? string.Empty;
            gift.IsEnabled = this.IsEnabled;
            gift.LastModified = DateTime.UtcNow;
            gift.Name = this.Name;
            gift.Price = this.Price;
            gift.FeaturedImageIds = this.FeaturedImageIds ?? string.Empty;
            if (!string.IsNullOrEmpty(this.FeaturedImageIds))
            {
                gift.FeaturedImageAttachmentId = long.Parse(FeaturedImageIds.Split(',').First());
            }
            else
            {
                gift.FeaturedImageAttachmentId = 0;
            }

            var attachment = new AttachmentService(TenantTypeIds.Instance().PointGift()).Get(gift.FeaturedImageAttachmentId);
            gift.FeaturedImage = attachment != null ? attachment.GetRelativePath() + "\\" + attachment.FileName : string.Empty;
            gift.LastModified = DateTime.UtcNow;

            return gift;
        }

        #endregion
    }

    /// <summary>
    /// PointGift的扩展类
    /// </summary>
    public static class PointGiftExtensions
    {
        /// <summary>
        /// 转化为PointGiftEditModel
        /// </summary>
        /// <param name="pointGift">数据库实体</param>
        /// <returns>对应的EditModel</returns>
        public static PointGiftEditModel AsPointGiftEditModel(this PointGift pointGift)
        {
            PointGiftEditModel pointGiftEditModel = new PointGiftEditModel();
            pointGiftEditModel.Description = pointGift.Description ?? string.Empty;
            pointGiftEditModel.FeaturedImage = pointGift.FeaturedImage ?? string.Empty;
            pointGiftEditModel.FeaturedImageIds = pointGift.FeaturedImageIds ?? string.Empty;
            pointGiftEditModel.IsEnabled = pointGift.IsEnabled;
            pointGiftEditModel.Name = pointGift.Name;
            pointGiftEditModel.Price = pointGift.Price;
            pointGiftEditModel.GiftId = pointGift.GiftId;
            pointGiftEditModel.CategoryId = pointGift.Category == null ? 0 : pointGift.Category.CategoryId;

            return pointGiftEditModel;
        }
    }
}