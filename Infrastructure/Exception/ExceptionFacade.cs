//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.51</verion>
//<createdate>2012-02-28</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-28" version="0.5">创建</log>
//<log date="2012-03-03" version="0.51" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Tunynet.Logging;

namespace Tunynet
{
    /// <summary>
    /// 异常Facade，统一调用自定义的异常
    /// </summary>
    [Serializable]
    public class ExceptionFacade : Exception, ISerializable
    {
        private readonly ExceptionDescriptor exceptionDescriptor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exceptionDescriptor">异常表述器</param>
        /// <param name="innerException">异常</param>
        public ExceptionFacade(ExceptionDescriptor exceptionDescriptor, Exception innerException = null)
            : base(null, innerException) { this.exceptionDescriptor = exceptionDescriptor; }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">异常信息</param>
        /// <param name="innerException">异常</param>
        public ExceptionFacade(String message = null, Exception innerException = null)
            : base(message, innerException)
        {
            exceptionDescriptor = new CommonExceptionDescriptor(message);
        }


        /// <summary>
        /// 异常显示信息
        /// </summary>
        public override String Message
        {
            get { return (exceptionDescriptor == null) ? base.Message : exceptionDescriptor.GetFriendlyMessage(); }
        }

        /// <summary>
        /// 上下文信息
        /// </summary>
        public string OperationContextMessage
        {
            get
            {
                return exceptionDescriptor.GetOperationContextMessage();
            }
        }

        /// <summary>
        /// 记入系统日志
        /// </summary>
        public void Log()
        {
            if (exceptionDescriptor != null && exceptionDescriptor.IsLogEnabled)
            {
                if (base.InnerException != null)
                    LoggerFactory.GetLogger().Log(exceptionDescriptor.LogLevel, exceptionDescriptor.GetLoggingMessage(), base.InnerException);
                else
                    LoggerFactory.GetLogger().Log(exceptionDescriptor.LogLevel, exceptionDescriptor.GetLoggingMessage());
            }
        }

    }
}
