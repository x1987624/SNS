//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-09</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-12-09" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Tunynet.Utilities;
using System.IO;

namespace Tunynet
{
    /// <summary>
    /// 默认运行环境实现
    /// </summary>
    public class DefaultRunningEnvironment : IRunningEnvironment
    {
        private const string WebConfigPath = "~/web.config";
        private const string BinPath = "~/bin";
        private const string RefreshHtmlPath = "~/refresh.html";

        /// <summary>
        /// 是否完全信任运行环境
        /// </summary>
        public bool IsFullTrust
        {
            get { return AppDomain.CurrentDomain.IsHomogenous && AppDomain.CurrentDomain.IsFullyTrusted; }
        }

        /// <summary>
        /// 重新启动AppDomain
        /// </summary>
        public void RestartAppDomain()
        {
            if (IsFullTrust)
            {
                HttpRuntime.UnloadAppDomain();
            }
            else
            {
                bool success = TryWriteBinFolder() || TryWriteWebConfig();

                if (!success)
                {
                    throw new ApplicationException(string.Format("需要启动站点，在非FullTrust环境下必须给\"{0}\"或者\"~/{1}\"写入的权限", BinPath, WebConfigPath));
                }
            }

            //通过http请求使站点启动
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                // Don't redirect posts...
                if (httpContext.Request.RequestType == "GET")
                {
                    httpContext.Response.Redirect(httpContext.Request.RawUrl, true /*endResponse*/);
                }
                else
                {
                    httpContext.Response.ContentType = "text/html";
                    httpContext.Response.WriteFile(RefreshHtmlPath);
                    httpContext.Response.End();
                }
            }
        }

        /// <summary>
        /// 尝试修改web.config最后更新时间
        /// </summary>
        /// <remarks>目的是使应用程序自动重新加载</remarks>
        /// <returns>修改成功返回true，否则返回false</returns>
        private bool TryWriteWebConfig()
        {
            try
            {
                // In medium trust, "UnloadAppDomain" is not supported. Touch web.config
                // to force an AppDomain restart.
                File.SetLastWriteTimeUtc(WebUtility.GetPhysicalFilePath(WebConfigPath), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试引起bin文件夹的改动
        /// </summary>
        /// <remarks>目的是使应用程序自动重新加载</remarks>
        /// <returns>成功写入返回true，否则返回false</returns>
        private bool TryWriteBinFolder()
        {
            try
            {
                var binMarker = WebUtility.GetPhysicalFilePath(BinPath + "HostRestart");
                Directory.CreateDirectory(binMarker);
                using (var stream = File.CreateText(Path.Combine(binMarker, "log.txt")))
                {
                    stream.WriteLine("Restart on '{0}'", DateTime.UtcNow);
                    stream.Flush();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
