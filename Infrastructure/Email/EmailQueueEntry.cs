//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>20120302</createdate>
//<author>杨明杰</author>
//<email>yangmj@tunynet.com</email>
//<log date="2012-03-02" version="0.5">新建</log>
//<log date="2012-03-10" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Tunynet;
using System.Net.Mail;

namespace Tunynet.Email
{
    [TableName("tn_EmailQueue")]
    [PrimaryKey("Id", autoIncrement = true)]
    [Serializable]
    public class EmailQueueEntry : IEntity
    {
        /// <summary>
        /// 新建实体时使用
        /// </summary>
        public static EmailQueueEntry New()
        {
            EmailQueueEntry emailQueueEntry = new EmailQueueEntry()
            {
                MailFrom = string.Empty,
                MailTo = string.Empty,
                MailBcc = string.Empty,
                MailCc = string.Empty,
                Body = string.Empty,
                Subject = string.Empty,
                NextTryTime = DateTime.UtcNow
            };
            return emailQueueEntry;
        }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public EmailQueueEntry()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public EmailQueueEntry(MailMessage mail)
        {
            this.Priority = (int)mail.Priority;
            this.IsBodyHtml = mail.IsBodyHtml;
            this.MailTo = mail.To.ToString();
            this.MailCc = mail.CC.ToString();
            this.MailBcc = mail.Bcc.ToString();
            this.MailFrom = mail.From.ToString();
            this.Subject = mail.Subject;
            this.Body = mail.Body;
            this.NextTryTime = DateTime.UtcNow;
            this.NumberOfTries = 0;
            this.IsFailed = false;
        }

        #region 需持久化属性

        /// <summary>
        ///邮件在队列中的标识
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        ///邮件优先级（对应System.Net.Mail.MailPriority的整型值）
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        ///邮件内容是不是Html格式
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        ///收件人(多个收件人用逗号分隔)
        /// </summary>
        public string MailTo { get; set; }

        /// <summary>
        ///抄送地址(多个地址用逗号分隔)
        /// </summary>
        public string MailCc { get; set; }

        /// <summary>
        ///密送地址(多个地址用逗号分隔)
        /// </summary>
        public string MailBcc { get; set; }

        /// <summary>
        ///发件人
        /// </summary>
        public string MailFrom { get; set; }

        /// <summary>
        ///邮件标题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        ///邮件内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        ///下次尝试发送时间
        /// </summary>
        public DateTime NextTryTime { get; set; }

        /// <summary>
        ///尝试发送次数
        /// </summary>
        public int NumberOfTries { get; set; }

        /// <summary>
        ///IsFailed
        /// </summary>
        public bool IsFailed { get; set; }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.Id; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

        #region 辅助方法
        
        /// <summary>
        /// 将EmailQueueEntry 转换为 MailMessage
        /// </summary>
        public MailMessage AsMailMessage(EmailQueueEntry emailEntry)
        {
            MailMessage currentMessage = new MailMessage();

            currentMessage.Priority = (MailPriority)emailEntry.Priority;
            currentMessage.IsBodyHtml = emailEntry.IsBodyHtml;
            String2MailAddressCollection(currentMessage.To, emailEntry.MailTo);
            String2MailAddressCollection(currentMessage.CC, emailEntry.MailCc);
            String2MailAddressCollection(currentMessage.Bcc, emailEntry.MailBcc);
            currentMessage.From = new MailAddress(emailEntry.MailFrom);
            currentMessage.Subject = emailEntry.Subject;
            currentMessage.Body = emailEntry.Body;
            return currentMessage;
        }

        /// <summary>
        /// 将String（使用英文,隔开）类型转换为MailAddressCollection
        /// </summary>
        private void String2MailAddressCollection(MailAddressCollection collection, string emails)
        {
            string[] emailStrings = emails.Split(',');
            if (emailStrings != null && emailStrings.Length > 0)
            {
                foreach (string email in emailStrings)
                {
                    if (!string.IsNullOrEmpty(email.Trim()))
                        collection.Add(new MailAddress(email));
                }
            }
        }

        #endregion
    }
}
