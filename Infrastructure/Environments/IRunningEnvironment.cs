//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-09</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-12-09" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet
{
    /// <summary>
    /// 运行环境接口
    /// </summary>
    public interface IRunningEnvironment
    {
        /// <summary>
        /// 是否完全信任运行环境
        /// </summary>
        bool IsFullTrust { get; }

        /// <summary>
        /// 重新启动AppDomain
        /// </summary>
        void RestartAppDomain();

    }
}
