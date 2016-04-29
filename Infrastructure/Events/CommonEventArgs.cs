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
using Tunynet.Utilities;
using System.Web;
using Tunynet.Logging;

namespace Tunynet.Events
{
    /// <summary>
    /// 通用事件参数
    /// </summary>
    public class CommonEventArgs : EventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventOperationType">事件操作类型
        /// <remarks>
        /// 建议使用<see cref="Tunynet.Events.EventOperationType"/>协助输入，例如：<br/>
        /// EventOperationType.Instance().Create()
        /// </remarks>
        /// </param>
        public CommonEventArgs(string eventOperationType)
            : this(eventOperationType, 0)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventOperationType">事件操作类型
        /// <remarks>
        /// 建议使用<see cref="Tunynet.Events.EventOperationType"/>协助输入，例如：<br/>
        /// EventOperationType.Instance().Create()
        /// </remarks>
        /// </param>
        /// <param name="applicationId">应用Id</param>
        public CommonEventArgs(string eventOperationType, int applicationId)
        {
            _eventOperationType = eventOperationType;
            _applicationId = applicationId;
            IOperatorInfoGetter operatorInfoGetter = DIContainer.Resolve<IOperatorInfoGetter>();
            if (operatorInfoGetter == null)
                throw new ApplicationException("IOperatorInfoGetter not registered to DIContainer");
            operatorInfo = operatorInfoGetter.GetOperatorInfo();
        }

        private string _eventOperationType;
        /// <summary>
        /// 事件操作类型 
        /// </summary>
        /// <remarks>
        /// 建议使用<see cref="Tunynet.Events.EventOperationType"/>协助输入，例如：<br/>
        /// EventOperationType.Instance().Create()
        /// </remarks>
        public string EventOperationType
        {
            get { return _eventOperationType; }
        }

        private int _applicationId;
        /// <summary>
        /// 应用Id
        /// </summary>
        public int ApplicationId
        {
            get { return _applicationId; }
        }

        private OperatorInfo operatorInfo;
        /// <summary>
        /// 操作者信息
        /// </summary>
        public OperatorInfo OperatorInfo
        {
            get { return operatorInfo; }
        }
    }
}
