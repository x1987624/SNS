//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// 资讯审核项
    /// </summary>
    public static class AuditItemKeysExtension
    {
        /// <summary>
        /// 内容项
        /// </summary>
        public static string CMS_ContentItem(this AuditItemKeys auditItemKeys)
        {
            return "CMS_ContentItem";
        }

    }

}
