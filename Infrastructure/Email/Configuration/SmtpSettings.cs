//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-03-01</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-03-01" version="0.5">创建</log>
//<log date="2012-03-10" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Tunynet.Caching;

namespace Tunynet.Email
{
    /// <summary>
    /// smtp服务器设置
    /// </summary>
    [TableName("tn_SmtpSettings")]
    [PrimaryKey("Id", autoIncrement = true)]
    [CacheSetting(false)]
    [Serializable]
    public class SmtpSettings : IEntity
    {
        /// <summary>
        /// 当前设置的id
        /// </summary>
        public long Id { get; protected set; }

        /// <summary>
        /// smtp服务器的域名或IP
        /// </summary>
        public virtual string Host { get; set; }

        /// <summary>
        /// smtp服务器端口号
        /// </summary>
        public virtual int Port { get; set; }

        /// <summary>
        /// smtp服务器是否启用ssl
        /// </summary>
        public virtual bool EnableSsl { get; set; }

        /// <summary>
        /// smtp服务器是否需要验证身份
        /// </summary>
        public virtual bool RequireCredentials { get; set; }

        /// <summary>
        /// 登录smtp服务器的用户名,可以不带 @后的域名部分
        /// </summary>
        public virtual string UserName { get; set; }

        /// <summary>
        /// 登录smtp服务器的用户邮件地址（可能与UserName，也可能不同）
        /// </summary>
        public virtual string UserEmailAddress { get; set; }

        /// <summary>
        /// 登录smtp服务器的密码
        /// </summary>
        public virtual string Password { get; set; }

        private bool forceSmtpUserAsFromAddress;

        /// <summary>
        /// 强制smtp登录用户作为发件人
        /// </summary>
        public bool ForceSmtpUserAsFromAddress
        {
            get { return forceSmtpUserAsFromAddress; }
            set { forceSmtpUserAsFromAddress = value; }
        }

        /// <summary>
        /// 每日发送邮件上限（如果超过此上限，则将不再尝试使用此邮箱发送邮件）
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 今天发送的数目
        /// </summary>
        [Ignore]
        public virtual int TodaySendCount { get; set; }

        /// <summary>
        /// 实体id
        /// </summary>
        [Ignore]
        public object EntityId
        {
            get { return this.Id; }
        }

        /// <summary>
        /// 是否在数据库中删除
        /// </summary>
        [Ignore]
        public bool IsDeletedInDatabase { get; set; }
    }


    ///// <summary>
    ///// 邮件发件人设置
    ///// </summary>
    //public enum SenderAddressType
    //{
    //    /// <summary>
    //    /// 使用期望的发件人地址
    //    /// </summary>
    //    DesiredAddress = 1,

    //    /// <summary>
    //    /// 强制使用站点设置的NoReply的邮件地址(提示用户不要回复该邮件)
    //    /// </summary>
    //    NoReplyAddress = 2,

    //    /// <summary>
    //    /// 强制使用站点设置的管理员邮件地址
    //    /// </summary>
    //    AdminAddress = 3,

    //    /// <summary>
    //    /// 强制使用SMTP登录用户的邮件地址（有些smtp服务器要求发件人与smtp登录人必须一致）
    //    /// </summary>
    //    SmtpUserAddress = 4
    //}

}
