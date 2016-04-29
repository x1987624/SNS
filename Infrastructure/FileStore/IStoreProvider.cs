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
using System.Drawing;
using Tunynet.Imaging;

namespace Tunynet.FileStore
{
    /// <summary>
    /// 文件存储提供者
    /// </summary>
    public interface IStoreProvider
    {
        #region 属性

        /// <summary>
        /// 该Providate文件存储根路径
        /// </summary>
        string StoreRootPath { get; }

        /// <summary>
        ///直连URL根路径
        /// </summary>
        string DirectlyRootUrl { get; }

        #endregion


        #region File

        /// <summary>
        /// 通过相对文件路径及文件名称获取文件.
        /// </summary>
        /// <param name="relativePath">相对文件路径(例如：000\000\000\002）</param>
        /// <param name="fileName">文件名称</param>
        IStoreFile GetFile(string relativePath, string fileName);

        /// <summary>
        /// 通过相对文件名称获取文件.
        /// </summary>
        /// <param name="relativeFileName">相对文件名称(例如：000\000\000\002\2.jpg）</param>
        IStoreFile GetFile(string relativeFileName);

        /// <summary>
        /// 获取文件路径中的所有文件
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        /// <param name="isOnlyCurrentFolder">是否只获取当前层次的文件</param>
        IEnumerable<IStoreFile> GetFiles(string relativePath, bool isOnlyCurrentFolder);

        /// <summary>
        /// 创建或更新一个文件
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentStream">The stream containing the content of the file.</param>
        IStoreFile AddOrUpdateFile(string relativePath, string fileName, Stream contentStream);

        /// <summary>
        /// 通过文件路径及文件名称删除文件.
        /// </summary>
        /// <param name="relativePath">文件路径</param>
        /// <param name="fileName">文件名称</param>
        void DeleteFile(string relativePath, string fileName);

        /// <summary>
        /// 删除文件路径中以fileNamePrefix开头的文件
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        /// <param name="fileNamePrefix">文件名称前缀</param>
        void DeleteFiles(string relativePath, string fileNamePrefix);

        /// <summary>
        /// 删除文件路径中的所有文件
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        void DeleteFolder(string relativePath);

        /// <summary>
        /// 获取文件直连URL
        /// </summary>
        /// <param name="relativePath">文件相对路径</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        string GetDirectlyUrl(string relativePath, string fileName);

        /// <summary>
        /// 获取文件直连URL
        /// </summary>
        /// <param name="relativePath">文件相对路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="lastModified">最后修改时间</param>
        /// <returns></returns>
        string GetDirectlyUrl(string relativePath, string fileName, DateTime lastModified);

        /// <summary>
        /// 获取文件直连URL
        /// </summary>
        /// <param name="relativeFileName">文件相对路径（包含文件名）</param>
        /// <returns></returns>
        string GetDirectlyUrl(string relativeFileName);

        /// <summary>
        /// 获取文件直连URL
        /// </summary>
        /// <param name="relativeFileName">文件相对路径（包含文件名）</param>
        /// <param name="lastModified">最后修改时间</param>
        /// <returns></returns>
        string GetDirectlyUrl(string relativeFileName, DateTime lastModified);

        /// <summary>
        /// 从全路径获取相对路径
        /// </summary>
        /// <param name="fullLocalPath"></param>
        /// <param name="pathIncludesFilename"></param>
        /// <returns></returns>
        string GetRelativePath(string fullLocalPath, bool pathIncludesFilename);

        /// <summary>
        /// 获取完整的本地物理路径
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        /// <param name="fileName">文件名称</param>
        string GetFullLocalPath(string relativePath, string fileName);

        /// <summary>
        /// 获取完整的本地物理路径
        /// </summary>
        /// <param name="relativeFileName">相对文件路径及名称</param>
        string GetFullLocalPath(string relativeFileName);

        #endregion

        /// <summary>
        /// 把多个目录组成部分连接成完整目录
        /// </summary>
        /// <param name="directoryParts">目录组成部分</param>
        /// <returns>返回合并到一起的目录</returns>
        string JoinDirectory(params string[] directoryParts);


        #region 获取不同尺寸的图片

        /// <summary>
        /// 获取不同尺寸大小的图片
        /// </summary>
        /// <param name="fileRelativePath">文件的相对路径</param>
        /// <param name="filename">文件名称</param>
        /// <param name="size">图片尺寸</param>
        /// <param name="resizeMethod">图像缩放方式</param>
        IStoreFile GetResizedImage(string fileRelativePath, string filename, Size size, ResizeMethod resizeMethod);

        /// <summary>
        /// 获取各种尺寸图片的名称
        /// </summary>
        /// <param name="filename">文件名称</param>
        /// <param name="size">图片尺寸</param>
        string GetSizeImageName(string filename, Size size, ResizeMethod resizeMethod);

        #endregion

    }
}