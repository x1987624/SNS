//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Spacebuilder.Common;
using Tunynet.Common;


namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册标签云Url获取
    /// </summary>
    public class PhotoTagUrlGetter : ITagUrlGetter
    {

        /// <summary>
        /// 获取链接
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        public string GetUrl(string tagName, long ownerId = 0)
        {
            return SiteUrls.Instance().TagNew(tagName);
        }
    }
}