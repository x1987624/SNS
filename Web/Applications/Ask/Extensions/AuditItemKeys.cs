//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答审核项
    /// </summary>
    public static class AuditItemKeysExtension
    {
        /// <summary>
        /// 问题审核项
        /// </summary>
        public static string Ask_Question(this AuditItemKeys auditItemKeys)
        {
            return "Ask_Question";
        }

        /// <summary>
        /// 回答审核项
        /// </summary>
        public static string Ask_Answer(this AuditItemKeys auditItemKeys)
        {
            return "Ask_Answer";
        }

    }

}
