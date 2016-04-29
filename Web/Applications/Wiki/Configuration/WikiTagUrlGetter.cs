//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Spacebuilder.Common;
using Tunynet.Common;
using System;

using System.Web.Routing;
using Tunynet.Utilities;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科标签云Url获取
    /// </summary>
    public class WikiTagUrlGetter : ITagUrlGetter
    {
        /// <summary>
        /// 获取链接
        /// </summary>
        /// <param name="tagName">标签名称</param>
        /// <returns>点击标签的链接</returns>
        public string GetUrl(string tagName, long ownerId = 0)
        {
            return SiteUrls.Instance().WikiTagDetail(tagName);
        }
    }
}