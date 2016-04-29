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
    /// 系统日志工厂
    /// </summary>
    /// <remarks>
    /// 用于获取ILogger，以便进行系统日志记录
    /// </remarks>
    public static class LoggerFactory
    {

        /// <summary>
        /// 依据LoggerName获取<see cref="Tunynet.Logging.ILogger"/>
        /// </summary>
        /// <param name="loggerName">日志名称（例如：log4net的logger配置名称）</param>
        /// <returns><see cref="Tunynet.Logging.ILogger"/></returns>
        public static ILogger GetLogger(string loggerName)
        {
            ILoggerFactoryAdapter loggerFactoryAdapter = DIContainer.Resolve<ILoggerFactoryAdapter>();
            return loggerFactoryAdapter.GetLogger(loggerName);
        }

        /// <summary>
        /// 获取logger name为tunynet的 <see cref="Tunynet.Logging.ILogger"/>
        /// </summary>
        /// <returns><see cref="Tunynet.Logging.ILogger"/></returns>
        public static ILogger GetLogger()
        {
            return GetLogger("tunynet");
        }

    }
}
