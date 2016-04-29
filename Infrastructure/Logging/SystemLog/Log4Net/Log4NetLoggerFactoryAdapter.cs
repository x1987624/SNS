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
using log4net.Config;
using Tunynet.Utilities;
using System.IO;
using log4net;

namespace Tunynet.Logging.Log4Net
{
    /// <summary>
    /// 用log4net实现的LoggerFactoryAdapter
    /// </summary>
    public class Log4NetLoggerFactoryAdapter : ILoggerFactoryAdapter
    {
        //log4net配置文件是否已经加载
        private static bool _isConfigLoaded = false;

        /// <summary>
        /// 构造函数（默认加载"~/Config/log4net.config"作为log4net配置文件）
        /// </summary>
        public Log4NetLoggerFactoryAdapter()
            : this("~/Config/log4net.config") { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configFilename">
        ///     <remarks>
        ///         <para>log4net配置文件路径，支持以下格式：</para>
        ///         <list type="bullet">
        ///             <item>~/config/log4net.config</item>
        ///             <item>~/web.config</item>
        ///             <item>c:\abc\log4net.config</item>
        ///         </list>
        ///     </remarks>
        /// </param>
        public Log4NetLoggerFactoryAdapter(string configFilename)
        {
            if (!_isConfigLoaded)
            {
                IRunningEnvironment runningEnvironment = DIContainer.Resolve<IRunningEnvironment>();

                if (string.IsNullOrEmpty(configFilename))
                    configFilename = "~/Config/log4net.config";

                FileInfo configFileInfo = new FileInfo(WebUtility.GetPhysicalFilePath(configFilename));
                if (!configFileInfo.Exists)
                    throw new ApplicationException(string.Format("log4net配置文件 {0} 未找到", configFileInfo.FullName));

                if (runningEnvironment.IsFullTrust)
                    XmlConfigurator.ConfigureAndWatch(configFileInfo);
                else
                    XmlConfigurator.Configure(configFileInfo);

                _isConfigLoaded = true;
            }
        }

        /// <summary>
        /// 依据LoggerName获取<see cref="Tunynet.Logging.ILogger"/>
        /// </summary>
        /// <param name="loggerName">日志名称（例如：log4net的logger配置名称）</param>
        /// <returns><see cref="Tunynet.Logging.ILogger"/></returns>
        public ILogger GetLogger(string loggerName)
        {
            return new Log4NetLogger(LogManager.GetLogger(loggerName));
        }
    }
}
