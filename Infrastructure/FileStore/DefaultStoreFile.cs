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
    /// 存储文件默认实现
    /// </summary>
    public class DefaultStoreFile: IStoreFile
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileInfo"></param>
        public DefaultStoreFile(string relativePath, FileInfo fileInfo)
        {
            this.relativePath = relativePath;
            this.fileInfo = fileInfo;
            this.fullLocalPath = fileInfo.FullName;
        }

        private readonly FileInfo fileInfo;

        #region IStoreFile 成员

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name
        {
            get { return fileInfo.Name; }
        }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extension
        {
            get { return fileInfo.Extension; }
        }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size
        {
            get { return fileInfo.Length; }
        }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastModified
        {
            get { return fileInfo.LastWriteTime; }
        }

        private string relativePath;
        /// <summary>
        /// 文件相对路径
        /// </summary>
        public string RelativePath
        {
            get { return relativePath; }
        }

        /// <summary>
        /// 获取用于读取文件的Stream
        /// </summary>
        public Stream OpenReadStream()
        {
            return new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
            //return File.OpenRead(fileInfo.FullName);
        }

        #endregion


        private string fullLocalPath;
        /// <summary>
        /// 完整文件物理路径(带fileName)
        /// </summary>
        public string FullLocalPath
        {
            get { return fullLocalPath; }
        }
    }

}
