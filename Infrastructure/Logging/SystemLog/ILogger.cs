//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-09</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-12-09" version="0.5">创建</log>
//<log date="2012-02-11" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Logging
{
    /// <summary>
    /// 系统日志接口(日志记录的API)
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 检查level级别的日志是否启用
        /// </summary>
        /// <param name="level">日志级别<seealso cref="Tunynet.Logging.LogLevel"/></param>
        /// <returns>如果启用返回true，否则返回false</returns>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// 记录level级别的日志
        /// </summary>
        /// <param name="level">日志级别<seealso cref="Tunynet.Logging.LogLevel"/></param>
        /// <param name="message">需记录的内容</param>
        void Log(LogLevel level, object message);

        /// <summary>
        /// 记录level级别的日志
        /// </summary>
        /// <param name="level">日志级别<seealso cref="Tunynet.Logging.LogLevel"/></param>
        /// <param name="message">需记录的内容</param>
        /// <param name="exception">异常</param>
        void Log(LogLevel level, Exception exception, object message);

        /// <summary>
        /// 记录level级别的日志
        /// </summary>
        /// <param name="level">日志级别<seealso cref="Tunynet.Logging.LogLevel"/></param>
        /// <param name="format">需记录的内容格式<see cref="string.Format(string,object[])"/></param>
        /// <param name="args">替换format占位符的参数</param>
        void Log(LogLevel level, string format, params object[] args);


    }
}
