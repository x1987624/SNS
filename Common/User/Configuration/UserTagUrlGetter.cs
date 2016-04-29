﻿//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Routing;
using Spacebuilder.Common;
using Tunynet.Utilities;
using Tunynet.Common;

namespace Spacebuilder.Common
{
    /// <summary>
    /// 用户Url获取
    /// </summary>
    public class UserTagUrlGetter : ITagUrlGetter
    {
        /// <summary>
        /// 获取链接
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public string GetUrl(string tagName, long ownerId = 0)
        {
            return SiteUrls.Instance().UserSearch(tagName, UserSearchRange.TAG);
        }
    }
}
