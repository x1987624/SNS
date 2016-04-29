//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.51</verion>
//<createdate>2012-02-29</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-02-29" version="0.5">创建</log>
//<log date="2012-03-03" version="0.51" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Logging;
using Tunynet.Globalization;
using System.Web;

namespace Tunynet
{
    /// <summary>
    /// 异常表述器
    /// </summary>
    public abstract class ExceptionDescriptor
    {

        /// <summary>
        /// 是否允许记入系统日志
        /// </summary>
        public bool IsLogEnabled { get; protected set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel LogLevel { get; protected set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常信息描述
        /// </summary>
        public ExceptionMessageDescriptor MessageDescriptor { get; set; }

        /// <summary>
        /// 获取记入日志的内容
        /// </summary>
        /// <returns>返回记入日志的内容</returns>
        public abstract string GetLoggingMessage();

        /// <summary>
        /// 获取友好的异常提示信息
        /// </summary>
        /// <returns></returns>
        public virtual string GetFriendlyMessage()
        {
            if (!string.IsNullOrEmpty(Message))
                return Message;
            else if (MessageDescriptor != null)
                return MessageDescriptor.GetExceptionMeassage();
            else
                return string.Empty;
        }

        /// <summary>
        /// 返回操作的上下文信息
        /// </summary>
        /// <returns></returns>
        public virtual string GetOperationContextMessage()
        {
            StringBuilder sb = new StringBuilder();
            HttpContext httpContext = HttpContext.Current;

            if (httpContext != null && httpContext.Request != null)
            {
                if (httpContext.Request.Url != null)
                    sb.AppendLine(string.Format(ResourceAccessor.GetString("Common_ExceptionUrl"), httpContext.Request.Url.AbsoluteUri));

                if (httpContext.Request.RequestType != null)
                    sb.AppendLine(string.Format(ResourceAccessor.GetString("Common_HttpMethod"), httpContext.Request.RequestType));

                if (httpContext.Request.UserHostAddress != null)
                    sb.AppendLine(string.Format(ResourceAccessor.GetString("Common_UserIP"), httpContext.Request.UserHostAddress));

                if (httpContext.Request.UserAgent != null)
                    sb.AppendLine(string.Format(ResourceAccessor.GetString("Common_UserAgent"), httpContext.Request.UserAgent));
            }

            return sb.ToString();
        }
    }


    /// <summary>
    /// 异常信息描述
    /// </summary>
    public class ExceptionMessageDescriptor
    {
        /// <summary>
        /// 构造器
        /// </summary>
        public ExceptionMessageDescriptor()
        {
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="messageFormatResourceKey">格式化异常信息ResourceKey</param>
        /// <param name="args">格式化异常信息参数</param>
        public ExceptionMessageDescriptor(string messageFormatResourceKey, params object[] args)
            : this(messageFormatResourceKey, 0, args)
        {
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="messageFormatResourceKey">格式化异常信息ResourceKey</param>
        /// <param name="applicationId">应用Id</param>
        /// <param name="args">格式化异常信息参数</param>
        public ExceptionMessageDescriptor(string messageFormatResourceKey, int applicationId = 0, params object[] args)
        {
            this.MessageFormatResourceKey = messageFormatResourceKey;
            if (applicationId > 0)
                this.ApplicationId = applicationId;

            this.Arguments = args;
        }

        /// <summary>
        /// 格式化异常信息
        /// </summary>
        public string MessageFormat { get; set; }

        /// <summary>
        /// 格式化异常信息ResourceKey
        /// </summary>
        public string MessageFormatResourceKey { get; set; }

        /// <summary>
        /// 应用Id
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        /// 格式化异常信息参数
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// 获取异常信息
        /// </summary>
        /// <returns></returns>
        internal string GetExceptionMeassage()
        {
            string stringFormat = null;

            if (!string.IsNullOrEmpty(MessageFormatResourceKey))
            {
                if (ApplicationId > 0)
                    stringFormat = ResourceAccessor.GetString(MessageFormatResourceKey, ApplicationId);
                else
                    stringFormat = ResourceAccessor.GetString(MessageFormatResourceKey);
            }
            else if (!string.IsNullOrEmpty(MessageFormat))
            {
                stringFormat = MessageFormat;
            }

            if (stringFormat != null)
                return string.Format(stringFormat, Arguments);
            else
                return string.Empty;
        }

    }

}
