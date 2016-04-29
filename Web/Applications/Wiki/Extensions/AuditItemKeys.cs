//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科审核项
    /// </summary>
    public static class AuditItemKeysExtension
    {
        /// <summary>
        /// 词条审核项
        /// </summary>
        public static string Wiki_Page(this AuditItemKeys auditItemKeys)
        {
            return "Wiki_Page";
        }

        /// <summary>
        /// 词条审核项
        /// </summary>
        public static string Wiki_PageVersion(this AuditItemKeys auditItemKeys)
        {
            return "Wiki_PageVersion";
        }
    }

}
