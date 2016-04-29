//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Spacebuilder.Common;
using Tunynet.Common;


namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答标签云Url获取
    /// </summary>
    public class AskTagUrlGetter : ITagUrlGetter
    {

        /// <summary>
        /// 获取链接
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public string GetUrl(string tagName, long ownerId = 0)
        {
            return SiteUrls.Instance().AskTagDetail(tagName);
        }
    }
}