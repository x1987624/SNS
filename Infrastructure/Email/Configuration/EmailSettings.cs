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
using Tunynet.Caching;

namespace Tunynet.Email
{
    /// <summary>
    /// 站点Email设置
    /// </summary>
    [Serializable]
    [CacheSetting(true)]
    public class EmailSettings : IEntity
    {
        private int batchSendLimit = 100;
        /// <summary>
        /// 每次从队列批量发送邮件的最大数量
        /// </summary>
        /// <remarks>-1表示没有限制</remarks>
        public int BatchSendLimit
        {
            get { return batchSendLimit; }
            set { batchSendLimit = value; }
        }

        private string adminEmailAddress = "admin@yourdomain.com";
        /// <summary>
        /// 管理员Email地址
        /// </summary>
        public string AdminEmailAddress
        {
            get { return adminEmailAddress; }
            set { adminEmailAddress = value; }
        }

        private string noReplyAddress = "noreply@yourdomain.com";
        /// <summary>
        /// NoReply邮件地址
        /// </summary>
        public string NoReplyAddress
        {
            get { return noReplyAddress; }
            set { noReplyAddress = value; }
        }

        private int numberOfTries = 6;
        /// <summary>
        /// 尝试发送次数
        /// </summary>
        public int NumberOfTries
        {
            get { return numberOfTries; }
            set { numberOfTries = value; }
        }

        private int sendTimeInterval = 10;
        /// <summary>
        /// 邮件发送间隔(以分钟为单位)
        /// </summary>
        public int SendTimeInterval
        {
            get { return sendTimeInterval; }
            set { sendTimeInterval = value; }
        }

        /// <summary>
        /// STMP服务器设置
        /// </summary>
        public SmtpSettings SmtpSettings { get; set; }

        #region IEntity 成员

        object IEntity.EntityId
        {
            get { return typeof(EmailSettings).FullName; }
        }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion


    }
}
