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
using System.Net.Mail;
using Tunynet.Repositories;
using Tunynet.Email;
using Tunynet.Events;

namespace Tunynet.Email
{
    /// <summary>
    /// 邮件服务逻辑类
    /// </summary>
    public class EmailService
    {
        private object _lock = new object();

        /// <summary>
        /// 可用的全部Smtp设置（如果此值为空，则采用默认设置）
        /// </summary>
        public static List<SmtpSettings> AllSmtpSettings { get; set; }

        //EmailQueue Repository
        private IEmailQueueRepository emailQueueRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public EmailService() : this(new EmailQueueRepository()) { }

        /// <summary>
        /// 可设置repository的构造函数
        /// </summary>
        public EmailService(IEmailQueueRepository emailQueueRepository)
        {
            this.emailQueueRepository = emailQueueRepository;
        }

        #region 邮件队列

        /// <summary>
        /// 在队列中对发送失败的邮件更新下次发送时间及发送次数，并且删除超过最大发送次数的邮件
        /// </summary>
        /// <param name="ids">发送失败的邮件Id集合</param>
        /// <param name="retryInterval">失败的Email过多少分钟再次重新发送</param>
        /// <param name="maxNumberOfTries">尝试发送的最大次数，超过该次数将设置为失败状态</param>
        public void SendFailed(IEnumerable<int> ids, int retryInterval, int maxNumberOfTries)
        {
            //根据当前的TryTime，若maxNumberOfTries <= TryTime+1 则：设置邮件状态IsFailed为：True
            //                   若maxNumberOfTries >  TryTime+1 则：设置邮件NumberOfTries,根据retryInterval，修改NextTryTime

            //操作步骤：
            //1 根据ids获取所有的实体
            //2 逐个进行判断，然后，编辑修改
            //由于发送失败属于异常情况，暂不考虑性能问题

            if (ids == null)
                return;

            foreach (int id in ids)
            {
                EmailQueueEntry emailQueueEntry = emailQueueRepository.Get(id);
                if (emailQueueEntry == null)
                    continue;
                else
                {
                    //尝试次数大于等于最多尝试次数，设置为发送失败
                    if ((emailQueueEntry.NumberOfTries + 1) >= maxNumberOfTries)
                        emailQueueEntry.IsFailed = true;
                    else//修改尝试时间和下次尝试时间
                    {
                        emailQueueEntry.NumberOfTries = emailQueueEntry.NumberOfTries + 1;
                        emailQueueEntry.NextTryTime = DateTime.UtcNow.AddMinutes(retryInterval);
                    }
                    //更新到数据库
                    emailQueueRepository.Update(emailQueueEntry);
                }
            }

        }

        /// <summary>
        /// 获取队列中的邮件集合
        /// </summary>
        /// <param name="maxNumber">从队列一次获取的最大数量</param>
        /// <returns>当前队列中的EmailTemplate集合</returns>
        public Dictionary<int, MailMessage> Dequeue(int maxNumber)
        {
            //按照优先级从高到低排序，获取达到NextTryTime的邮件
            Dictionary<int, MailMessage> emailDictionary = new Dictionary<int, MailMessage>();

            IEnumerable<EmailQueueEntry> emails = emailQueueRepository.Dequeue(maxNumber);

            //转换为字典类型,同时转换为MailMessage
            if (emails != null)
            {
                foreach (EmailQueueEntry email in emails)
                {
                    if (email != null)
                        emailDictionary.Add(email.Id, email.AsMailMessage(email));
                }
            }

            return emailDictionary;
        }

        /// <summary>
        /// 把待发送MailMessage加入队列
        /// </summary>
        /// <param name="email">待发送的MailMessage</param>
        public int? Enqueue(MailMessage email)
        {
            if (email == null || email.To == null || email.To.Count < 1)
                return null;

            int id = 0;
            int.TryParse(emailQueueRepository.Insert(new EmailQueueEntry(email)).ToString(), out id);

            //将邮件插入数据库队列
            return (int?)id;
        }

        /// <summary>
        /// 单条删除邮件
        /// </summary>
        /// <param name="id">要删除的邮件ID</param>
        public void Delete(int id)
        {
            //若能获取到则删除
            EmailQueueEntry deleteEntry = emailQueueRepository.Get(id);
            if (deleteEntry != null)
            {
                EventBus<EmailQueueEntry>.Instance().OnBefore(deleteEntry, new CommonEventArgs(EventOperationType.Instance().Delete()));
                emailQueueRepository.Delete(deleteEntry);
                EventBus<EmailQueueEntry>.Instance().OnAfter(deleteEntry, new CommonEventArgs(EventOperationType.Instance().Delete()));
            }
        }

        /// <summary>
        /// 批量删除邮件
        /// </summary>
        /// <param name="ids">要删除的邮件ID集合</param>
        public void Delete(IEnumerable<int> ids)
        {
            //后台管理员批量删除邮件
            if (ids == null)
                return;

            //逐个删除
            foreach (int id in ids)
            {
                Delete(id);
            }
        }

        #endregion

        #region 邮件发送

        /// <summary>
        /// 异步发送Email
        /// </summary>
        /// <param name="mail">待发送的邮件</param>
        /// <returns>发送成功返回true，否则返回false</returns>
        public bool SendAsyn(MailMessage mail, bool isRetry = true)
        {
            string errorMessage;
            return SendAsyn(mail, out errorMessage, isRetry);
        }

        /// <summary>
        /// 异步发送Email，不对发送失败或者成功进行处理
        /// </summary>
        /// <param name="mail">待发送的邮件</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>发送成功返回true，否则返回false</returns>
        public bool SendAsyn(MailMessage mail, out string errorMessage, bool isRetry = true)
        {
            //获取SmtpClient
            SmtpClient smtpClient = null;
            SmtpSettings smtpSettings = null;

            bool isSuccess = true;
            errorMessage = string.Empty;
            try
            {
                smtpSettings = NextSmtpSettings();
                smtpClient = GetSmtpClient(smtpSettings);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                isSuccess = false;
            }

            //若SmtpClient获取成功，发送邮件
            if (isSuccess && smtpClient != null)
            {
                try
                {
                    string displayName = mail.From.DisplayName;
                    if (smtpSettings.ForceSmtpUserAsFromAddress)
                    {
                        mail.From = new MailAddress(smtpSettings.UserEmailAddress, displayName);
                        mail.Sender = new MailAddress(smtpSettings.UserEmailAddress, displayName);
                    }
                    smtpClient.SendAsync(mail, string.Empty);
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                    isSuccess = false;
                }
            }

            //若发送失败，并且需要重试，那么，将发送失败的mail存入队列
            if (!isSuccess && isRetry)
            {
                Enqueue(mail);
            }

            return isSuccess;
        }

        /// <summary>
        /// 发送Email
        /// </summary>
        /// <param name="mail">待发送的邮件</param>
        /// <returns>发送成功返回true，否则返回false</returns>
        public bool Send(MailMessage mail)
        {
            string errorMessage;
            return Send(mail, out errorMessage);
        }

        /// <summary>
        /// 发送Email
        /// </summary>
        /// <param name="mail">待发送的邮件</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>发送成功返回true，否则返回false</returns>
        public bool Send(MailMessage mail, out string errorMessage)
        {
            //获取SmtpClient
            SmtpClient smtpClient = null;
            SmtpSettings smtpSettings = null;
            try
            {
                smtpSettings = NextSmtpSettings();
                smtpClient = GetSmtpClient(smtpSettings);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }

            //若SmtpClient获取成功，发送邮件
            if (smtpClient != null)
            {
                try
                {
                    string displayName = mail.From.DisplayName;
                    if (smtpSettings.ForceSmtpUserAsFromAddress)
                    {
                        mail.From = new MailAddress(smtpSettings.UserEmailAddress, displayName);
                        mail.Sender = new MailAddress(smtpSettings.UserEmailAddress, displayName);
                    }

                    smtpClient.Send(mail);
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取StmpClient
        /// </summary>
        private SmtpClient GetSmtpClient(SmtpSettings smtpSettings = null)
        {
            SmtpClient client = null;

            if (smtpSettings == null)
            {
                IEmailSettingsManager emailSettingsManager = DIContainer.Resolve<IEmailSettingsManager>();
                EmailSettings settings = emailSettingsManager.Get();
                smtpSettings = settings.SmtpSettings;
            }

            client = new SmtpClient(smtpSettings.Host, smtpSettings.Port);
            client.EnableSsl = smtpSettings.EnableSsl;

            //for SMTP Authentication
            if (smtpSettings.RequireCredentials)
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(smtpSettings.UserName, smtpSettings.Password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
            }

            return client;
        }

        private static int _index = 0;

        /// <summary>
        /// 获取下一个可以用的Smtp设置
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private SmtpSettings NextSmtpSettings()
        {
            if (AllSmtpSettings == null)
            {
                IEmailSettingsManager emailSettingsManager = DIContainer.Resolve<IEmailSettingsManager>();
                EmailSettings settings = emailSettingsManager.Get();
                return settings.SmtpSettings;
            }

            int index = 0;
            lock (_lock)
            {
                _index = (_index + 1) % AllSmtpSettings.Count();
                index = _index;
            }

            //以下代码是为了维持邮箱的发送邮件的均衡

            for (int i = index; i < AllSmtpSettings.Count(); i++)
            {
                SmtpSettings settings = AllSmtpSettings.ElementAt(i);
                if (settings.DailyLimit > settings.TodaySendCount)
                {
                    settings.TodaySendCount++;
                    return settings;
                }
            }

            for (int i = 0; i < index; i++)
            {
                SmtpSettings settings = AllSmtpSettings.ElementAt(i);
                if (settings.DailyLimit > settings.TodaySendCount)
                {
                    settings.TodaySendCount++;
                    return settings;
                }
            }

            throw new Exception("所有的Smtp设置都超出了使用限制，请尝试添加更多的Smtp设置");
        }

        #endregion

    }
}
