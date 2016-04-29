//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-03-15</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-03-15" version="0.5">创建</log>
//<log date="2012-03-16" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tunynet.FileStore
{
    /// <summary>
    /// 存储中的文件
    /// </summary>
    public interface IStoreFile
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// 文件大小
        /// </summary>
        long Size { get; }

        /// <summary>
        /// 相对StoragePath的路径
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        DateTime LastModified { get; }


        /// <summary>
        /// 获取用于读取文件的Stream
        /// </summary>
        Stream OpenReadStream();
    }
}
