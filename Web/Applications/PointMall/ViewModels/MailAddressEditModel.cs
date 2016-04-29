using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Spacebuilder.Common;
using Tunynet.Utilities;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 邮寄地址的EditModel
    /// </summary>
    public class MailAddressEditModel
    {
        #region 属性

        /// <summary>
        ///邮寄地址ID
        /// </summary>
        public long AddressId { get; set; }

        /// <summary>
        ///收件人
        /// </summary>
        [Display(Name = "收件人")]
        [Required(ErrorMessage = "必须输入联系人！")]
        [StringLength(20,ErrorMessage="不能超过20个字符！")]
        [DataType(DataType.Text)]
        public string Addressee { get; set; }

        /// <summary>
        ///联系电话
        /// </summary>
        [Display(Name = "联系电话")]
        [RegularExpression("[0-9]{11}", ErrorMessage = "不合法的电话号码！")]
        [Required(ErrorMessage = "必须输入联系电话！")]
        [DataType(DataType.PhoneNumber)]
        public string Tel { get; set; }

        /// <summary>
        ///邮寄地址
        /// </summary>
        [Display(Name = "邮寄地址")]
        [StringLength(56,ErrorMessage="不能超过56个字符！")]
        [Required(ErrorMessage = "必须输入邮寄地址！")]
        [DataType(DataType.Text)]
        public string Address { get; set; }

        /// <summary>
        ///邮编
        /// </summary>
        [Display(Name = "邮编")]
        [StringLength(10, ErrorMessage = "不能超过10个字符")]
        [Required(ErrorMessage = "必须输入邮编！")]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "只能输入字母或数字")]
        public string PostCode { get; set; }

        #endregion


        /// <summary>上
        /// 转换为MailAddress
        /// </summary>
        /// <returns></returns>
        public MailAddress AsMailAddress()
        {
            MailAddress mailAddress = null;
            if (this.AddressId <= 0) //创建
            {
                mailAddress = MailAddress.New();
            }
            else       //编辑
            {
                mailAddress = new MailAddressService().Get(this.AddressId);
                if (mailAddress == null)        //找不到实体
                {
                    return null;
                }
                mailAddress.LastModified = DateTime.UtcNow;
            }
            mailAddress.UserId = UserContext.CurrentUser.UserId;
            mailAddress.Address =this.Address;
            mailAddress.Addressee = this.Addressee;
            mailAddress.PostCode = this.PostCode;
            mailAddress.Tel = this.Tel;

            return mailAddress;
        }

    }

    /// <summary>
    /// MailAddress的扩展方法
    /// </summary>
    public static class MailAddressExtensions
    {
        /// <summary>
        /// 转化为编辑实体
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <returns>邮寄地址的EditModel</returns>
        public static MailAddressEditModel AsEditModel(this MailAddress mailAddress)
        {
            MailAddressEditModel editModel = new MailAddressEditModel();

            editModel.Address = mailAddress.Address;
            editModel.Addressee = mailAddress.Addressee;
            editModel.AddressId = mailAddress.AddressId;
            editModel.PostCode = mailAddress.PostCode;
            editModel.Tel = mailAddress.Tel;

            return editModel;
        }
    }
}