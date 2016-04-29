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
    /// 资源相关异常描述
    /// </summary>
    public class ResourceExceptionDescriptor : ExceptionDescriptor
    {
        /// <summary>
        /// 构造器
        /// </summary>
        public ResourceExceptionDescriptor()
            : this(null)
        {

        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="message">异常信息</param>
        public ResourceExceptionDescriptor(string message)
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
        public ResourceExceptionDescriptor(string messageFormat, params object[] args)
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
        /// 内容不存在
        /// </summary>
        /// <param name="title">要访问内容的名称或标题，例如“群组-zone”</param>
        /// <param name="contentId">要访问内容的Id</param>
        /// <returns></returns>
        public ResourceExceptionDescriptor WithContentNotFound(string title, object contentId = null)
        {
            string resourceKey = "Exception_ContentNotFound";
            contentId = contentId == null ? string.Empty : contentId.ToString();
            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { title, contentId });
            return this;
        }

        /// <summary>
        /// 用户不存在
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="userId">UserId</param>
        /// <returns></returns>
        public ResourceExceptionDescriptor WithUserNotFound(string userName, int userId)
        {
            string resourceKey = "Exception_UserNotFound";
            this.MessageDescriptor = new ExceptionMessageDescriptor(resourceKey, new { userName, userId });
            return this;
        }
    }
}
