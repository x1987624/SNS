//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-12-23</createdate>
//<author>zhengw</author>
//<email>zhengw@tunynet.com</email>
//<log date="2011-12-23" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet
{
    /// <summary>
    /// 可分页数据接口
    /// </summary>
    public interface IPagingDataSet
    {
        /// <summary>
        /// 当前页数
        /// </summary>
        int PageIndex { get; set; }
        /// <summary>
        /// 每页显示记录数
        /// </summary>
        int PageSize { get; set; }
        /// <summary>
        /// 总记录数
        /// </summary>
        long TotalRecords { get; set; }

    }
}
