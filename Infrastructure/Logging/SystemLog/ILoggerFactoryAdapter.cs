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
    /// 供LoggerFactory使用的适配器接口
    /// </summary>
    public interface ILoggerFactoryAdapter
    {
        /// <summary>
        /// 依据LoggerName获取<see cref="Tunynet.Logging.ILogger"/>
        /// </summary>
        /// <param name="loggerName">日志名称（例如：log4net的logger配置名称）</param>
        /// <returns><see cref="Tunynet.Logging.ILogger"/></returns>
        ILogger GetLogger(string loggerName);
    }
}
