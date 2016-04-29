//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-16</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-16" version="0.5">创建</log>
//<log date="2012-02-16" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Tunynet.Events
{
    /// <summary>
    /// 事件总线（用于定义事件、触发事件）
    /// </summary>
    /// <typeparam name="S">触发事件的对象类型</typeparam>    
    public class EventBus<S> : EventBus<S, CommonEventArgs>
    {

        #region 获取实例
        private static volatile EventBus<S> _instance = null;
        private static readonly object lockObject = new object();

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns>返回EventBus实例</returns>
        public static EventBus<S> Instance()
        {
            if (_instance == null)
            {
                lock (lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new EventBus<S>();
                    }
                }
            }
            return _instance;
        }
        #endregion
 

    }
}
