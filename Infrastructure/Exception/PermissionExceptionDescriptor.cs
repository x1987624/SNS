//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.52</verion>
//<createdate>2012-02-29</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-29" version="0.5">创建</log>
//<log date="2012-02-29" version="0.51" author="libsh">增加具体实现</log>
//<log date="2012-03-03" version="0.52" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet
{
    /// <summary>
    /// 权限相关异常描述
    /// </summary>
    public class PermissionExceptionDescriptor : ExceptionDescriptor
    {

        /// <summary>
        /// 构造器
        /// </summary>
        public PermissionExceptionDescriptor()
            : this(null)
        {
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="message">异常信息</param>
        public PermissionExceptionDescriptor(string message)
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
        public PermissionExceptionDescriptor(string messageFormat, params object[] args)
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
        /// 没有管理权限
        /// </summary>
        /// <param name="content">管理的内容，例如“博客文章”</param>
        /// <returns></returns>
        public PermissionExceptionDescriptor WithBasicManagementAccessDenied(string content)
        {
            string resourceKey = "Exception_BasicManagementAccessDenied";
            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { content });
            return this;
        }

        /// <summary>
        /// 没有足够的访问权限
        /// </summary>
        /// <returns></returns>
        public PermissionExceptionDescriptor WithAccessDenied()
        {
            string resourceKey = "Exception_AccessDenied";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { });
            return this;
        }

        /// <summary>
        /// 没有发布内容的权限
        /// </summary>
        /// <param name="content">发布的内容，例如“博客文章”</param>
        /// <returns></returns>
        public PermissionExceptionDescriptor WithPostAccessDenied(string content)
        {
            string resourceKey = "Exception_AccessDenied";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { content });
            return this;
        }

        /// <summary>
        /// 没有回复的权限
        /// </summary>
        /// <returns></returns>
        public PermissionExceptionDescriptor WithPostReplyAccessDenied()
        {
            string resourceKey = "Exception_PostReplyAccessDenied";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { });
            return this;
        }

        /// <summary>
        /// 找不到合法的授权文件
        /// </summary>
        /// <returns></returns>
        public PermissionExceptionDescriptor WithLicenseAuthorizeDenied()
        {
            string resourceKey = "Exception_LicenseAuthorizeDenied";

            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { });
            return this;
        }

    }
}
