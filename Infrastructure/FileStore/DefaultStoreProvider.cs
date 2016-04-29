//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.6</verion>
//<createdate>2012-03-15</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-03-15" version="0.5">创建</log>
//<log date="2012-03-16" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Tunynet.Imaging;
using Tunynet.Utilities;

namespace Tunynet.FileStore
{
    /// <summary>
    /// 默认文件存储提供者
    /// </summary>
    public class DefaultStoreProvider : IStoreProvider
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="storeRootPath"></param>
        public DefaultStoreProvider(string storeRootPath)
            : this(storeRootPath, WebUtility.ResolveUrl(storeRootPath))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="storeRootPath"></param>
        /// <param name="directlyRootUrl"></param>
        public DefaultStoreProvider(string storeRootPath, string directlyRootUrl)
        {
            this.storeRootPath = WebUtility.GetPhysicalFilePath(storeRootPath);
            if (!string.IsNullOrEmpty(this.StoreRootPath))
                this.storeRootPath = this.StoreRootPath.TrimEnd('/', '\\');

            this.directlyRootUrl = directlyRootUrl;
            if (!string.IsNullOrEmpty(this.directlyRootUrl))
                this.directlyRootUrl = WebUtility.ResolveUrl(this.directlyRootUrl.TrimEnd('/', '\\'));
        }

        /// <summary>
        /// 构造函数（适用于访问UNC地址）
        /// </summary>
        /// <param name="storeRootPath">UNC共享目录</param>
        /// <param name="directlyRootUrl">直连地址根路径</param>
        /// <param name="username">访问UNC地址的用户名</param>
        /// <param name="password">访问UNC地址的密码</param>
        public DefaultStoreProvider(string storeRootPath, string directlyRootUrl, string username, string password)
            : this(storeRootPath, directlyRootUrl)
        {
            //连接网络共享目录，需要先释放之前的连接（如果有的话）
            NetworkShareAccesser acesser = new NetworkShareAccesser(storeRootPath, username, password);
            acesser.Disconnect();
            acesser.Connect();
        }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static DefaultStoreProvider()
        {
            StringBuilder regexText = new StringBuilder();
            regexText.Append("^[^");
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                regexText.Append(Regex.Escape(new String(invalidChar, 1)));
            }
            regexText.Append("]{1,255}$");
            ValidFileNamePattern = new Regex(regexText.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

            regexText = new StringBuilder();
            regexText.Append("^[^");
            foreach (char invalidChar in Path.GetInvalidPathChars())
            {
                regexText.Append(Regex.Escape(new String(invalidChar, 1)));
            }
            ValidPathPattern = new Regex(regexText.ToString() + "]{0,769}$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        }


        #region 属性
        private string storeRootPath;
        /// <summary>
        /// 该Providate文件存储根路径(含\)
        /// </summary>
        /// <remarks>
        /// 支持：虚拟目录、应用程序根（~/）、UNC路径、文件系统物理路径
        /// </remarks>
        public string StoreRootPath
        {
            get { return storeRootPath; }
        }

        private string directlyRootUrl;
        /// <summary>
        ///直连URL根路径
        /// </summary>
        public string DirectlyRootUrl
        {
            get { return directlyRootUrl; }
        }

        #endregion


        #region IStoreProvider 成员

        /// <summary>
        /// 通过相对文件路径及文件名称获取文件.
        /// </summary>
        /// <param name="relativePath">相对文件路径(例如：2012\03\16）</param>
        /// <param name="fileName">文件名称</param>
        public IStoreFile GetFile(string relativePath, string fileName)
        {
            //暂时去掉检测
            //if (!IsValid(relativePath, fileName))
            //    throw new InvalidOperationException("The provided path and/or file name is invalid");

            string fullPath = GetFullLocalPath(relativePath, fileName);
            if (File.Exists(fullPath))
                return new DefaultStoreFile(relativePath, new FileInfo(fullPath));
            else
                return null;
        }

        /// <summary>
        /// 通过相对文件名称获取文件.
        /// </summary>
        /// <param name="relativeFileName">相对文件名称(例如：000\000\000\002\2.jpg）</param>
        public IStoreFile GetFile(string relativeFileName)
        {

            string fullPath = GetFullLocalPath(relativeFileName);
            string relativePath = GetRelativePath(fullPath, true);
            if (File.Exists(fullPath))
                return new DefaultStoreFile(relativePath, new FileInfo(fullPath));
            else
                return null;
        }

        /// <summary>
        /// 获取文件路径中的所有文件
        /// </summary>
        /// <exception cref="ArgumentException">文件路径不正确时抛出异常</exception>
        /// <param name="relativePath">相对文件路径</param>
        /// <param name="isOnlyCurrentFolder">是否只获取当前层次的文件</param>
        public IEnumerable<IStoreFile> GetFiles(string relativePath, bool isOnlyCurrentFolder)
        {
            if (!DefaultStoreProvider.IsValidPath(relativePath))
                throw new ArgumentException("The provided path is invalid", "relativePath");

            List<IStoreFile> files = new List<IStoreFile>();
            string localPath = GetFullLocalPath(relativePath, string.Empty);
            if (Directory.Exists(localPath))
            {
                SearchOption searchOption = SearchOption.TopDirectoryOnly;
                if (!isOnlyCurrentFolder)
                    searchOption = SearchOption.AllDirectories;

                foreach (FileInfo file in (new DirectoryInfo(localPath)).GetFiles("*.*", searchOption))
                {
                    if ((file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        DefaultStoreFile fsFile;
                        if (isOnlyCurrentFolder)
                            fsFile = new DefaultStoreFile(relativePath, file);
                        else
                            fsFile = new DefaultStoreFile(GetRelativePath(file.FullName, true), file);

                        files.Add(fsFile);
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// 创建或更新一个文件
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentStream">The stream containing the content of the file.</param>
        public IStoreFile AddOrUpdateFile(string relativePath, string fileName, System.IO.Stream contentStream)
        {
            if (contentStream == null || !contentStream.CanRead)
                return null;

            if (!IsValidPathAndFileName(relativePath, fileName))
                throw new InvalidOperationException("The provided path and/or file name is invalid.");

            string fullPath = GetFullLocalPath(relativePath, fileName);

            EnsurePathExists(fullPath, true);

            contentStream.Position = 0;
            using (FileStream outStream = File.OpenWrite(fullPath))
            {
                byte[] buffer = new byte[contentStream.Length > 65536 ? 65536 : contentStream.Length];

                int readedSize;
                while ((readedSize = contentStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, readedSize);
                }

                outStream.Flush();
                outStream.Close();
            }

            DefaultStoreFile file = new DefaultStoreFile(relativePath, new FileInfo(fullPath));
            return file;
        }

        /// <summary>
        /// 通过文件路径及文件名称删除文件.
        /// </summary>
        /// <param name="relativePath">文件路径</param>
        /// <param name="fileName">文件名称</param>
        public void DeleteFile(string relativePath, string fileName)
        {
            if (!IsValidPathAndFileName(relativePath, fileName))
                throw new InvalidOperationException("The provided path and/or file name is invalid");

            string fullPath = GetFullLocalPath(relativePath, fileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            //DeleteEmptyFolders(GetFullLocalPath(relativePath, string.Empty));
        }

        /// <summary>
        /// 删除文件路径中以fileNamePrefix开头的文件
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        /// <param name="fileNamePrefix">文件名称前缀</param>
        public void DeleteFiles(string relativePath, string fileNamePrefix)
        {
            if (!IsValidPath(relativePath))
                throw new InvalidOperationException("The provided path is invalid");

            string fullPath = GetFullLocalPath(relativePath, string.Empty);
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo directory = new DirectoryInfo(fullPath);
                foreach (FileInfo file in directory.GetFiles(fileNamePrefix + "*"))
                {
                    file.Delete();
                }
            }

            //DeleteEmptyFolders(GetFullLocalPath(relativePath, string.Empty));
        }

        /// <summary>
        /// 删除文件目录
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        public void DeleteFolder(string relativePath)
        {
            if (!IsValidPath(relativePath))
                return;

            string localPath = GetFullLocalPath(relativePath, string.Empty);
            if (Directory.Exists(localPath))
                Directory.Delete(localPath, true);
        }

        /// <summary>
        /// 获取文件直连URL
        /// </summary>
        /// <param name="relativePath">文件相对路径</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public string GetDirectlyUrl(string relativePath, string fileName)
        {
            return GetDirectlyUrl(relativePath, fileName, DateTime.MinValue);
        }

        /// <summary>
        /// 获取文件直连URL
        /// </summary>
        /// <param name="relativePath">文件相对路径</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="lastModified">最后修改时间</param>
        /// <returns></returns>
        public string GetDirectlyUrl(string relativePath, string fileName, DateTime lastModified)
        {
            string url = string.Empty;

            if (string.IsNullOrEmpty(relativePath))
            {
                return GetDirectlyUrl(fileName, lastModified);
            }
            else
            {
                if (relativePath.EndsWith("\\") || relativePath.EndsWith("/"))
                {
                    return GetDirectlyUrl(relativePath + fileName, lastModified);
                }
                else
                {
                    return GetDirectlyUrl(relativePath + "/" + fileName, lastModified);
                }
            }
        }

        /// <summary>
        /// 获取文件直连URL
        /// </summary>
        /// <param name="relativeFileName">文件相对路径（包含文件名）</param>
        /// <returns></returns>
        public string GetDirectlyUrl(string relativeFileName)
        {
            return GetDirectlyUrl(relativeFileName, DateTime.MinValue);
        }

        /// <summary>
        /// 获取文件直连URL
        /// </summary>
        /// <param name="relativeFileName">文件相对路径（包含文件名）</param>
        /// <param name="lastModified">最后修改时间</param>
        /// <returns></returns>
        public string GetDirectlyUrl(string relativeFileName, DateTime lastModified)
        {
            if (string.IsNullOrEmpty(this.DirectlyRootUrl))
            {
                return string.Empty;
            }

            StringBuilder url = new StringBuilder(DirectlyRootUrl);

            relativeFileName = relativeFileName.Replace('\\', '/');
            if (!relativeFileName.StartsWith("/"))
            {
                url.Append("/");
            }

            url.Append(relativeFileName);

            if (lastModified > DateTime.MinValue)
            {
                url.Append("?lm=");
                url.Append(lastModified.Ticks);
            }

            return url.ToString();
        }

        /// <summary>
        /// 连接目录
        /// </summary>
        /// <param name="directoryParts"></param>
        /// <returns></returns>
        public string JoinDirectory(params string[] directoryParts)
        {
            return string.Join(new string(Path.DirectorySeparatorChar, 1), directoryParts);
        }

        #endregion

        #region 获取不同尺寸大小

        /// <summary>
        /// 获取不同尺寸大小的图片
        /// </summary>
        /// <param name="fileRelativePath">文件的相对路径</param>
        /// <param name="filename">文件名称</param>
        /// <param name="size">图片尺寸</param>
        /// <param name="resizeMethod">图像缩放方式</param>
        /// <returns>若原图不存在，则会返回null，否则会返回缩放后的图片</returns>
        public IStoreFile GetResizedImage(string fileRelativePath, string filename, Size size, ResizeMethod resizeMethod)
        {
            string relativePath = fileRelativePath;

            if (filename.ToLower().EndsWith(".gif"))
            {
                return GetFile(relativePath, filename); ;
            }

            string sizedFileName = GetSizeImageName(filename, size, resizeMethod);
            IStoreFile file = GetFile(relativePath, sizedFileName);

            if (file == null)
            {
                IStoreFile originalFile = GetFile(relativePath, filename);
                if (originalFile == null)
                {
                    return null;
                }

                using (Stream originalStream = originalFile.OpenReadStream())
                {
                    if (originalStream != null)
                    {
                        using (Stream resizedStream = ImageProcessor.Resize(originalStream, size.Width, size.Height, resizeMethod))
                        {
                            file = AddOrUpdateFile(relativePath, sizedFileName, resizedStream);
                        }
                    }
                }
            }
            return file;
        }

        /// <summary>
        /// 获取各种尺寸图片的名称
        /// </summary>
        /// <param name="filename">文件名称</param>
        /// <param name="size">图片尺寸</param>
        /// <param name="resizeMethod">图片缩放方式</param>
        public string GetSizeImageName(string filename, Size size, ResizeMethod resizeMethod)
        {
            string resizedFileName = string.Format("{0}-{1}-{2}x{3}{4}", filename, resizeMethod != ResizeMethod.KeepAspectRatio ? resizeMethod.ToString() : string.Empty, size.Width, size.Height, Path.GetExtension(filename));

            return resizedFileName;
        }

        #endregion

        #region Help Methods

        private static readonly Regex ValidPathPattern;
        private static readonly Regex ValidFileNamePattern;

        /// <summary>
        /// 验证文件路径是否合法
        /// </summary>
        private static bool IsValidPath(string path)
        {
            return ValidPathPattern.IsMatch(path);
        }

        /// <summary>
        /// 验证文件名称路径是否合法
        /// </summary>
        private static bool IsValidFileName(string fileName)
        {
            return ValidFileNamePattern == null || ValidFileNamePattern.IsMatch(fileName);
        }

        /// <summary>
        /// 验证文件路径以及文件名称是否合法
        /// </summary>
        private static bool IsValidPathAndFileName(string path, string fileName)
        {
            return IsValidPath(path) && IsValidFileName(fileName) && System.Text.Encoding.UTF8.GetBytes(path + "." + fileName).Length <= 1024;
        }

        /// <summary>
        /// 获取完整的本地物理路径
        /// </summary>
        /// <param name="relativePath">相对文件路径</param>
        /// <param name="fileName">文件名称</param>
        public string GetFullLocalPath(string relativePath, string fileName)
        {
            string fullPath = this.StoreRootPath;

            //如果是c:或d:之类的磁盘根目录，Path.Combine不会自动加上目录分隔符，所以需要先加上
            if (fullPath.EndsWith(":"))
            {
                fullPath = fullPath + "\\";
            }

            if (!string.IsNullOrEmpty(relativePath))
            {
                relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar);
                fullPath = Path.Combine(fullPath, relativePath);
            }

            if (!string.IsNullOrEmpty(fileName))
                fullPath = Path.Combine(fullPath, fileName);

            return fullPath;
        }

        /// <summary>
        /// 获取完整的本地物理路径
        /// </summary>
        /// <param name="relativeFileName">相对文件路径及名称</param>
        public string GetFullLocalPath(string relativeFileName)
        {
            string fullPath = this.StoreRootPath;

            //如果是c:或d:之类的磁盘根目录，Path.Combine不会自动加上目录分隔符，所以需要先加上
            if (fullPath.EndsWith(":"))
            {
                fullPath = fullPath + "\\";
            }

            if (!string.IsNullOrEmpty(relativeFileName))
                fullPath = Path.Combine(fullPath, relativeFileName);

            return fullPath;
        }

        /// <summary>
        /// 从全路径获取相对路径
        /// </summary>
        /// <param name="fullLocalPath"></param>
        /// <param name="pathIncludesFilename"></param>
        /// <returns></returns>
        public string GetRelativePath(string fullLocalPath, bool pathIncludesFilename)
        {
            string relativePath = pathIncludesFilename ? fullLocalPath.Substring(0, fullLocalPath.LastIndexOf(Path.DirectorySeparatorChar)) : fullLocalPath;
            relativePath = relativePath.Replace(this.StoreRootPath, string.Empty);
            relativePath = relativePath.Trim(Path.DirectorySeparatorChar);
            return relativePath;
        }

        /// <summary>
        /// 确保建立文件目录
        /// </summary>
        /// <param name="fullLocalPath">文件完整路径</param>
        /// <param name="pathIncludesFilename">文件路径是否包含文件名称</param>
        private static void EnsurePathExists(string fullLocalPath, bool pathIncludesFilename)
        {
            string path = pathIncludesFilename ? fullLocalPath.Substring(0, fullLocalPath.LastIndexOf(Path.DirectorySeparatorChar)) : fullLocalPath;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

        }

        #endregion

    }
}
