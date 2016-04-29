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
using log4net;

namespace Tunynet.Logging.Log4Net
{
    /// <summary>
    /// 用Log4Net实现的ILogger
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        ILog log;

        internal Log4NetLogger(ILog log)
        {
            this.log = log;
        }


        /// <summary>
        /// 检查level级别的日志是否启用
        /// </summary>
        /// <param name="level">日志级别<seealso cref="Tunynet.Logging.LogLevel"/></param>
        /// <returns>如果启用返回true，否则返回false</returns>
        public bool IsEnabled(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return log.IsDebugEnabled;
                case LogLevel.Information:
                    return log.IsInfoEnabled;
                case LogLevel.Warning:
                    return log.IsWarnEnabled;
                case LogLevel.Error:
                    return log.IsErrorEnabled;
                case LogLevel.Fatal:
                    return log.IsFatalEnabled;
            }
            return false;
        }

        /// <summary>
        /// 记录level级别的日志
        /// </summary>
        /// <param name="level">日志级别<seealso cref="Tunynet.Logging.LogLevel"/></param>
        /// <param name="message">需记录的内容</param>
        public void Log(LogLevel level, object message)
        {
            if (!IsEnabled(level))
                return;

            switch (level)
            {
                case LogLevel.Debug:
                    log.Debug(message);
                    break;
                case LogLevel.Information:
                    log.Info(message);
                    break;
                case LogLevel.Warning:
                    log.Warn(message);
                    break;
                case LogLevel.Error:
                    log.Error(message);
                    break;
                case LogLevel.Fatal:
                    log.Fatal(message);
                    break;
            }
        }

        /// <summary>
        /// 记录level级别的日志
        /// </summary>
        /// <param name="level">日志级别<seealso cref="Tunynet.Logging.LogLevel"/></param>
        /// <param name="message">需记录的内容</param>
        /// <param name="exception">异常</param>
        public void Log(LogLevel level, Exception exception, object message)
        {
            if (!IsEnabled(level))
                return;

            switch (level)
            {
                case LogLevel.Debug:
                    log.Debug(message, exception);
                    break;
                case LogLevel.Information:
                    log.Info(message, exception);
                    break;
                case LogLevel.Warning:
                    log.Warn(message, exception);
                    break;
                case LogLevel.Error:
                    log.Error(message, exception);
                    break;
                case LogLevel.Fatal:
                    log.Fatal(message, exception);
                    break;
            }
        }

        /// <summary>
        /// 记录level级别的日志
        /// </summary>
        /// <param name="level">日志级别<seealso cref="Tunynet.Logging.LogLevel"/></param>
        /// <param name="format">需记录的内容格式<see cref="string.Format(string,object[])"/></param>
        /// <param name="args">替换format占位符的参数</param>
        public void Log(LogLevel level, string format, params object[] args)
        {
            if (!IsEnabled(level))
                return;

            switch (level)
            {
                case LogLevel.Debug:
                    log.DebugFormat(format, args);
                    break;
                case LogLevel.Information:
                    log.InfoFormat(format, args);
                    break;
                case LogLevel.Warning:
                    log.WarnFormat(format, args);
                    break;
                case LogLevel.Error:
                    log.ErrorFormat(format, args);
                    break;
                case LogLevel.Fatal:
                    log.FatalFormat(format, args);
                    break;
            }
        }

    }
}
