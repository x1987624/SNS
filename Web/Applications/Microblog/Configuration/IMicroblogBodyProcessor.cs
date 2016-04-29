//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-08-23</createdate>
//<author>libsh</author>
//<email>libsh@tunynet.com</email>
//<log date="2012-08-23" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spacebuilder.Microblog
{
    /// <summary>
    /// 微博正文解析器
    /// </summary>
    public interface IMicroblogBodyProcessor
    {
        /// <summary>
        /// 内容处理
        /// </summary>
        /// <param name="body">待处理内容</param>
        /// <param name="ownerTenantTypeId">拥有者租户类型</param>
        /// <param name="associateId">待处理相关项Id</param>
        /// <param name="userId">相关项作者</param>
        /// <param name="ownerId">拥有者Id</param>
        /// <returns></returns>
        string Process(string body,string ownerTenantTypeId, long associateId, long userId, long ownerId);
    }
}