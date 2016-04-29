//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-01-10</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-01-10" version="0.5">新建</log>
//<log date="2012-01-16" version="0.51" reviewer="zhengw">对代码进行走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Tunynet.Globalization
{
    /// <summary>
    /// 资源获取器
    /// </summary>
    public static class ResourceAccessor
    {
        private static ResourceManager _commonResourceManager;
        private static ConcurrentDictionary<int, ResourceManager> _applicationResourceManagers = new ConcurrentDictionary<int, ResourceManager>();

        /// <summary>
        /// 从公共资源文件获取资源项
        /// </summary>
        /// <param name="resourcesKey">资源名称(忽略大小写)</param>
        /// <returns>如果找到相应资源项则返回资源项字符串,否则返回缺少资源项的提示</returns>
        public static string GetString(string resourcesKey)
        {
            string result;
            try
            {
                result = _commonResourceManager.GetString(resourcesKey);

                if (result != null)
                    return result;
            }
            catch { }

            return GetMissingResourcePrompt(resourcesKey);
        }

        /// <summary>
        /// 获取应用的资源项(如果在应用中找不到，则从公共资源文件获取)
        /// </summary>
        /// <param name="resourcesKey">资源名称(忽略大小写)</param>
        /// <param name="applicationId">应用Id</param>
        /// <returns>如果找到相应资源项则返回资源项字符串,否则返回缺少资源项的提示</returns>
        public static string GetString(string resourcesKey, int applicationId)
        {
            string result;

            ResourceManager applicationResourceManager;
            if (_applicationResourceManagers.TryGetValue(applicationId, out applicationResourceManager))
            {
                try
                {
                    result = applicationResourceManager.GetString(resourcesKey);

                    if (result != null)
                        return result;
                }
                catch { }
            }
            try
            {
                result = _commonResourceManager.GetString(resourcesKey);

                if (result != null)
                    return result;
            }
            catch { }

            return GetMissingResourcePrompt(resourcesKey);
        }

        /// <summary>
        /// 获取未找到资源项时的提示信息
        /// </summary>
        /// <param name="resourcesKey">资源名称</param>
        /// <returns>提示信息</returns>
        private static string GetMissingResourcePrompt(string resourcesKey)
        {
            return string.Format("<span style=\"color:#ff0000; font-weight:bold\">missing resource: {0}</span>", resourcesKey);
        }


        #region 初始化

        /// <summary>
        /// 初始化（注册commonResourceManager以及currentCultureAccessor）
        /// </summary>        
        /// <param name="commonResourceFileBaseName">资源文件BaseName</param>
        /// <param name="commonResourceAssembly">资源文件所在程序集</param>
        public static void Initialize(string commonResourceFileBaseName, Assembly commonResourceAssembly)
        {
            _commonResourceManager = new ResourceManager(commonResourceFileBaseName, commonResourceAssembly);
            _commonResourceManager.IgnoreCase = true;
        }

        /// <summary>
        /// 注册应用的ResourceManager
        /// </summary>
        /// <param name="applicationId">应用Id</param>
        /// <param name="resourceFileBaseName">资源文件BaseName</param>
        /// <param name="assembly">资源文件所在程序集</param>
        public static void RegisterApplicationResourceManager(int applicationId, string resourceFileBaseName, Assembly assembly)
        {
            ResourceManager rm = null;
            try
            {
                rm = new ResourceManager(resourceFileBaseName, assembly);
            }
            catch { }

            if (rm != null)
            {
                rm.IgnoreCase = true;
                _applicationResourceManagers[applicationId] = rm;
            }
        }

        #endregion
    }
}
