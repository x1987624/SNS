//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-28</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-28" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Logging
{
    /// <summary>
    /// ILogger扩展
    /// </summary>
    public static class LoggerExtension
    {

        #region 在日志记录Message

        /// <summary>
        /// 记录Debug级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        public static void Debug(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// 记录Info级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        public static void Info(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// 记录Warn级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        public static void Warn(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// 记录Error级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        public static void Error(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Error, message);
        }

        /// <summary>
        /// 记录Fatal级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        public static void Fatal(this ILogger logger, object message)
        {
            logger.Log(LogLevel.Fatal, message);
        }

        #endregion

        #region 在日志记录Message和Exception

        /// <summary>
        /// 记录Debug级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        /// <param name="exception">异常</param>
        public static void Debug(this ILogger logger, Exception exception, object message)
        {
            logger.Log(LogLevel.Debug, exception, message);
        }


        /// <summary>
        /// 记录Info级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        /// <param name="exception">异常</param>
        public static void Info(this ILogger logger, Exception exception, object message)
        {
            logger.Log(LogLevel.Information, exception, message);
        }

        /// <summary>
        /// 记录Warn级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        /// <param name="exception">异常</param>
        public static void Warn(this ILogger logger, Exception exception, object message)
        {
            logger.Log(LogLevel.Warning, exception, message);
        }

        /// <summary>
        /// 记录Error级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        /// <param name="exception">异常</param>
        public static void Error(this ILogger logger, Exception exception, object message)
        {
            logger.Log(LogLevel.Error, exception, message);
        }

        /// <summary>
        /// 记录Fatal级别日志
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="message">需记录的内容</param>
        /// <param name="exception">异常</param>
        public static void Fatal(this ILogger logger, Exception exception, object message)
        {
            logger.Log(LogLevel.Fatal, exception, message);
        }

        #endregion


        #region 以String.Format形式记录日志

        /// <summary>
        /// 记录Debug级别日志(类似<see cref="string.Format(string,object[])"/>)
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="format">需记录的内容格式<see cref="string.Format(string,object[])"/></param>
        /// <param name="args">替换format占位符的参数</param>
        public static void DebugFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Debug, format, args);
        }

        /// <summary>
        /// 记录Info级别日志(类似<see cref="string.Format(string,object[])"/>)
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="format">需记录的内容格式<see cref="string.Format(string,object[])"/></param>
        /// <param name="args">替换format占位符的参数</param>
        public static void InfoFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Information, format, args);
        }

        /// <summary>
        /// 记录Warn级别日志(类似<see cref="string.Format(string,object[])"/>)
        /// </summary>
        /// <param name="format">需记录的内容格式<see cref="string.Format(string,object[])"/></param>
        /// <param name="args">替换format占位符的参数</param>
        public static void WarnFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Warning, format, args);
        }

        /// <summary>
        /// 记录Error级别日志(类似<see cref="string.Format(string,object[])"/>)
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="format">需记录的内容格式<see cref="string.Format(string,object[])"/></param>
        /// <param name="args">替换format占位符的参数</param>
        public static void ErrorFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Error, format, args);
        }

        /// <summary>
        /// 记录Fatal级别日志(类似<see cref="string.Format(string,object[])"/>)
        /// </summary>
        /// <param name="logger"><see cref="Tunynet.Logging.ILogger">ILogger</see></param>
        /// <param name="format">需记录的内容格式<see cref="string.Format(string,object[])"/></param>
        /// <param name="args">替换format占位符的参数</param>
        public static void FatalFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Log(LogLevel.Fatal, format, args);
        }

        #endregion

    }
}
