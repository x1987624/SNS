//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2010-11-01</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2010-11-01" version="0.5">创建</log>
//<log date="2012-02-16" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Events
{
    /// <summary>
    /// 事件处理程序模块接口
    /// </summary>
    public interface IEventMoudle
    {
        /// <summary>
        /// 注册事件处理程序
        /// </summary>
        void RegisterEventHandler();

    }
}
