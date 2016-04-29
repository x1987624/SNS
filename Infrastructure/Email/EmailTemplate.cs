//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-29</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-29" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Mail;
using System.Xml;
using Tunynet.Utilities;

namespace Tunynet.Email
{
    /// <summary>
    /// 邮件模板
    /// </summary>
    public sealed class EmailTemplate
    {
        /// <summary>
        /// EmailTemplate构造器
        /// </summary>
        /// <param name="rootNode">EmailTemplate所属xml文档节点</param>
        public EmailTemplate(XmlNode rootNode)
        {
            XmlNode attrNode = rootNode.Attributes.GetNamedItem("priority");

            if (attrNode != null)
            {
                MailPriority _priority;
                if (Enum.TryParse<MailPriority>(attrNode.InnerText, out _priority))
                    this.priority = _priority;
            }
            attrNode = rootNode.Attributes.GetNamedItem("templateName");
            if (attrNode != null)
                templateName = attrNode.InnerText;

            XmlNode subjectNode = rootNode.SelectSingleNode("subject");
            if (subjectNode != null)
                this.subject = subjectNode.InnerText;

            XmlNode fromNode = rootNode.SelectSingleNode("from");
            if (fromNode != null)
                this.From = fromNode.InnerText;

            XmlNode bodyNode = rootNode.SelectSingleNode("body");
            if (bodyNode != null)
            {
                attrNode = bodyNode.Attributes.GetNamedItem("url");
                //todo:mazq,by zhengw:需要完整的URL
                //
                if (attrNode != null)
                    this.BodyUrl = attrNode.InnerText;
                else
                    this.Body = bodyNode.InnerXml;
            }
        }

        #region 属性

        private string templateName;
        /// <summary>
        /// 模板名称（在Email模板中必须保证唯一）
        /// </summary>
        public string TemplateName
        {
            get { return templateName; }
            set { templateName = value; }
        }

        private string from;
        /// <summary>
        /// 发件人
        /// </summary>
        public String From
        {
            get { return from; }
            set { from = value; }
        }

        private string subject = "";
        /// <summary>
        /// Email主题
        /// </summary>
        public String Subject
        {
            get { return subject; }
            set { subject = value; }
        }

        /// <summary>
        /// 邮件内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 用于获取邮件内容的URL
        /// </summary>
        public string BodyUrl { get; set; }

        private MailPriority priority = MailPriority.Normal;
        /// <summary>
        /// Email优先级
        /// </summary>
        public MailPriority Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        #endregion

    }
}
