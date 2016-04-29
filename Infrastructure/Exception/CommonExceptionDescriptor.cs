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

namespace Tunynet
{
    /// <summary>
    /// 通用异常描述
    /// </summary>
    public class CommonExceptionDescriptor : ExceptionDescriptor
    {
        /// <summary>
        /// 构造器
        /// </summary>
        public CommonExceptionDescriptor()
            : this(null)
        {
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="message">异常信息</param>
        public CommonExceptionDescriptor(string message)
        {
            this.IsLogEnabled = true;
            this.LogLevel = Logging.LogLevel.Warning;
            this.Message = message;
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="messageFormat">格式化异常信息</param>
        /// <param name="args">格式化异常信息参数</param>
        public CommonExceptionDescriptor(string messageFormat, params object[] args)
            : this(string.Format(messageFormat, args))
        {
        }

        /// <summary>
        /// 获取记入日志的内容
        /// </summary>
        /// <returns>返回记入日志的内容</returns>
        public override string GetLoggingMessage()
        {
            return GetFriendlyMessage() + "--" + GetOperationContextMessage();
        }

        /// <summary>
        /// 禁止注册
        /// </summary>
        public CommonExceptionDescriptor WithRegisterDenied()
        {
            string resourceKey = "Exception_RegisterDenied";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { });
            return this;
        }

        /// <summary>
        /// 发送邮件时产生异常
        /// </summary>
        /// <param name="message">追加的异常信息</param>
        public CommonExceptionDescriptor WithEmailUnableToSend(string message)
        {
            string resourceKey = "Exception_EmailUnableToSend";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { message });
            return this;
        }

        /// <summary>
        /// 不允许频繁发帖
        /// </summary>
        public CommonExceptionDescriptor WithFloodDenied()
        {
            string resourceKey = "Exception_FloodDenied";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { });
            return this;
        }

        /// <summary>
        /// I/O或无访问权限引发的异常
        /// </summary>
        public CommonExceptionDescriptor WithUnauthorizedAccessException()
        {
            string resourceKey = "Exception_UnauthorizedAccessException";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { });
            return this;
        }

        /// <summary>
        /// 未知错误
        /// </summary>
        public CommonExceptionDescriptor WithUnknownError()
        {
            string resourceKey = "Exception_UnknownError";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { });
            return this;
        }

    }
}
