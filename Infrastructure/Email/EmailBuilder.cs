//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-03-01</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2012-03-01" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using Tunynet.Caching;
using System.Web;
using System.IO;
using Tunynet.Utilities;
using System.Xml;
using RazorEngine;
using System.Dynamic;

namespace Tunynet.Email
{
    /// <summary>
    /// 邮件构建器
    /// </summary>
    public class EmailBuilder
    {

        #region Instance

        private EmailBuilder()
        { }

        private static volatile EmailBuilder _defaultInstance = null;
        private static readonly object lockObject = new object();
        private static bool isInitialized;
        private static Dictionary<string, EmailTemplate> emailTemplates = null;
        /// <summary>
        /// 获取EmailBuilder实例
        /// </summary>
        /// <returns></returns>
        public static EmailBuilder Instance()
        {
            if (_defaultInstance == null)
            {
                lock (lockObject)
                {
                    if (_defaultInstance == null)
                    {
                        _defaultInstance = new EmailBuilder();
                    }
                }
            }
            EnsureLoadTemplates();
            return _defaultInstance;
        }

        #endregion

        /// <summary>
        /// 加载所有邮件模板，并预编译
        /// </summary>
        /// <remarks>在Starter中调用</remarks>
        private static void EnsureLoadTemplates()
        {
            if (!isInitialized)
            {
                lock (lockObject)
                {
                    if (!isInitialized)
                    {
                        //从 \Languages\zh-CN\emails\ 及  \Applications\[ApplicationKey]\Languages\zh-CN\emails\ 加载邮件模板 
                        emailTemplates = LoadEmailTemplates();
                        isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// 生成MailMessage
        /// </summary>
        /// <param name="templateName">邮件模板名称</param>
        /// <param name="model">解析模板使用的数据</param>
        /// <param name="to">收件人</param>
        /// <param name="from">发件人</param>
        /// <returns>返回生成的MailMessage</returns>
        public MailMessage Resolve(string templateName, ExpandoObject model, string to, string from = null)
        {
            return Resolve(templateName, model, new string[] { to }, from);
        }

        /// <summary>
        /// 生成MailMessage
        /// </summary>
        /// <param name="templateName">邮件模板名称</param>
        /// <param name="model">解析模板使用的数据</param>
        /// <param name="from">发件人</param>
        /// <param name="to">收件人</param>
        /// <param name="cc">抄送地址</param>
        /// <param name="bcc">密送地址</param>
        /// <exception cref="CommonExceptionDescriptor">发件人不能为null</exception>
        /// <exception cref="CommonExceptionDescriptor">编译邮件模板标题时报错</exception>
        /// <exception cref="CommonExceptionDescriptor">编译邮件模板内容时报错</exception>
        /// <exception cref="CommonExceptionDescriptor">邮件模板中Body、BodyUrl必须填一个</exception>
        /// <returns>返回生成的MailMessage</returns>
        public MailMessage Resolve(string templateName, dynamic model, IEnumerable<string> to, string from = null, IEnumerable<string> cc = null, IEnumerable<string> bcc = null)
        {
            if (to == null)
                return null;
            if (model == null)
                model = new ExpandoObject();
            IEmailSettingsManager emailSettingsManager = DIContainer.Resolve<IEmailSettingsManager>();
            EmailSettings settings = emailSettingsManager.Get();
            //if (settings.SmtpSettings.ForceSmtpUserAsFromAddress)
            //    from = settings.SmtpSettings.UserEmailAddress;

            EmailTemplate emailTemplate = GetEmailTemplate(templateName);

            if (string.IsNullOrEmpty(from))
            {
                if (string.Equals(emailTemplate.From, "NoReplyAddress", StringComparison.CurrentCultureIgnoreCase))
                    from = settings.NoReplyAddress;
                else if (string.Equals(emailTemplate.From, "AdminAddress", StringComparison.CurrentCultureIgnoreCase))
                    from = settings.AdminEmailAddress;
                else
                    throw new ExceptionFacade(new CommonExceptionDescriptor("发件人不能为null"));
            }


            MailMessage email = new MailMessage();
            email.IsBodyHtml = true;



            try
            {
                //todo all by bianchx:为了保证发送出去的邮件发送人名称显示都是相同的，所以需要保证Model中的SiteName都是有效的
                email.From = new MailAddress(from, model.SiteName);
            }
            catch { }

            foreach (var toAddress in to)
            {
                try
                {
                    email.To.Add(toAddress);
                }
                catch { }
            }

            if (cc != null)
            {
                foreach (var ccAddress in cc)
                {
                    try
                    {
                        email.CC.Add(ccAddress);
                    }
                    catch { }
                }
            }
            if (bcc != null)
            {
                foreach (var bccAddress in bcc)
                {
                    try
                    {
                        email.Bcc.Add(bccAddress);
                    }
                    catch { }
                }
            }

            //使用RazorEngine解析 EmailTemplate.Subject
            try
            {
                email.Subject = Razor.Parse(emailTemplate.Subject, model, emailTemplate.TemplateName);
            }
            catch (Exception e)
            {
                throw new ExceptionFacade(new CommonExceptionDescriptor("编译邮件模板标题时报错"), e);
            }
            email.Priority = emailTemplate.Priority;
            if (!string.IsNullOrEmpty(emailTemplate.Body))
            {
                try
                {
                    email.Body = Razor.Parse(emailTemplate.Body, model, emailTemplate.Body);
                }
                catch (Exception e)
                {
                    throw new ExceptionFacade("编译邮件模板内容时报错", e);
                }

            }
            else if (!string.IsNullOrEmpty(emailTemplate.BodyUrl))
                email.Body = HttpCollects.GetHTMLContent(emailTemplate.BodyUrl);
            else
                throw new ExceptionFacade("邮件模板中Body、BodyUrl必须填一个");
            return email;
        }

        /// <summary>
        /// 加载Email模板
        /// </summary>
        private static Dictionary<string, EmailTemplate> LoadEmailTemplates()
        {
            Dictionary<string, EmailTemplate> emailTemplates;
            //todo:zhengw,by libsh:需要确定如果获取当前语言
            //回复：暂不修改
            string language = "zh-CN";

            string cacheKey = "EmailTemplates::" + language;
            ICacheService cacheService = DIContainer.Resolve<ICacheService>();

            emailTemplates = cacheService.Get<Dictionary<string, EmailTemplate>>(cacheKey);

            if (emailTemplates == null)
            {
                emailTemplates = new Dictionary<string, EmailTemplate>();

                // Read in the file
                string searchPattern = "*.xml";
                //平台级邮件模板
                string commonDirectoryPath = WebUtility.GetPhysicalFilePath(string.Format("~/Languages/" + language + "/emails/"));
                string[] fileNames = Directory.GetFiles(commonDirectoryPath, searchPattern);

                //应用级邮件模板
                string applicationsRootDirectory = WebUtility.GetPhysicalFilePath("~/Applications/");
                IEnumerable<string> applicationEmailTemplateFileNames = new List<string>();
                if (Directory.Exists(applicationsRootDirectory))
                {
                    foreach (var applicationPath in Directory.GetDirectories(applicationsRootDirectory))
                    {
                        string applicationEmailTemplateDirectory = Path.Combine(applicationPath, "Languages\\" + language + "\\emails\\");
                        if (!Directory.Exists(applicationEmailTemplateDirectory))
                            continue;
                        applicationEmailTemplateFileNames = applicationEmailTemplateFileNames.Union(Directory.GetFiles(applicationEmailTemplateDirectory, searchPattern));
                    }
                }

                //为提升Linq执行效率，尽量避免在循环体中使用立即执行方法，比如：.ToArray()
                fileNames = fileNames.Union(applicationEmailTemplateFileNames).ToArray();

                dynamic dModel = new ExpandoObject();

                Type modelType = ((object)dModel).GetType();

                foreach (string fileName in fileNames)
                {
                    if (!File.Exists(fileName))
                        continue;

                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);

                    string templateName;
                    foreach (XmlNode node in doc.GetElementsByTagName("email"))
                    {
                        XmlNode attrNode = node.Attributes.GetNamedItem("templateName");
                        if (attrNode == null)
                            continue;
                        templateName = attrNode.InnerText;
                        EmailTemplate emailTemplate = new EmailTemplate(node);
                        emailTemplates[templateName] = emailTemplate;

                        //编译模板
                        if (!string.IsNullOrEmpty(emailTemplate.Body))
                            Razor.Compile(emailTemplate.Body, modelType, templateName);
                    }
                }
                cacheService.Add(cacheKey, emailTemplates, CachingExpirationType.Stable);
            }

            return emailTemplates;
        }

        /// <summary>
        /// 获取单个邮件模板
        /// </summary>
        /// <param name="templateName">模板名称</param>
        /// <exception cref="ResourceExceptionDescriptor">邮件模板不存在</exception>
        /// <returns>邮件模板</returns>
        private static EmailTemplate GetEmailTemplate(string templateName)
        {
            if (emailTemplates != null && emailTemplates.ContainsKey(templateName))
                return emailTemplates[templateName];
            throw new ExceptionFacade(new ResourceExceptionDescriptor().WithContentNotFound("邮件模板", templateName));
        }


        /// <summary>
        /// 加载的Email模板列表
        /// </summary>
        /// <remarks>仅用于测试</remarks>
        public IList<EmailTemplate> EmailTemplates
        {
            get
            {
                return emailTemplates.Values.ToReadOnly();
            }
        }
    }
}
