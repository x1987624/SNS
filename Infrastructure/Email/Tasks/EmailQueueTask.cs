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
using Tunynet.Tasks;
using System.Collections.Generic;
using System.Net.Mail;
using Tunynet.Email;
using System.Threading;

namespace Tunynet.Email.Tasks
{
    /// <summary>
    /// 邮件发送自运行任务
    /// </summary>
    public class EmailTask : ITask
    {
        private static ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();
        /// <summary>
        /// 任务执行的内容
        /// </summary>
        /// <param name="taskDetail">任务配置状态信息</param>
        public void Execute(TaskDetail taskDetail = null)
        {
            EmailService emailService = new EmailService();
            List<int> successedIds = new List<int>();
            List<int> failedIds = new List<int>();

            RWLock.EnterWriteLock();
            //从配置文件读取配置
            IEmailSettingsManager emailSettingsManager = DIContainer.Resolve<IEmailSettingsManager>();
            EmailSettings settings = emailSettingsManager.Get();

            //1 首先获取待发送的邮件列表
            Dictionary<int, MailMessage> emailQueue = emailService.Dequeue(settings.BatchSendLimit);

            //2 逐个邮件进行发送（非异步发送）
            //3 记录记录发送成功的和发送失败的ID
            foreach (var item in emailQueue)
            {
                if (emailService.Send(item.Value))
                    successedIds.Add(item.Key);
                else
                    failedIds.Add(item.Key);
            }

            //4 发送成功的记录删除
            emailService.SendFailed(failedIds, settings.SendTimeInterval, settings.NumberOfTries);//从配置文件读取
            //5 发送失败的记录更新
            emailService.Delete(successedIds);//从配置文件读取
            RWLock.ExitWriteLock();

        }
    }
}
