//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-03-01</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-03-01" version="0.5">创建</log>
//<log date="2012-03-10" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Email
{
    /// <summary>
    /// EmailSettings管理器接口
    /// </summary>
    public interface IEmailSettingsManager
    {
        /// <summary>
        /// 获取EmailSettings
        /// </summary>
        /// <returns></returns>
        EmailSettings Get();

        /// <summary>
        /// 保存EmailSettings
        /// </summary>
        /// <returns></returns>
        void Save(EmailSettings emailSettings);
    }
}
