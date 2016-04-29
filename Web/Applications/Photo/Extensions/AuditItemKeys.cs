//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册审核项
    /// </summary>
    public static class AuditItemKeysExtension
    {
        /// <summary>
        /// 相册审核项
        /// </summary>
        public static string Album(this AuditItemKeys auditItemKeys)
        {
            return "Album";
        }

        /// <summary>
        /// 图片审核项
        /// </summary>
        public static string Photo(this AuditItemKeys auditItemKeys)
        {
            return "Photo";
        }

    }

}
