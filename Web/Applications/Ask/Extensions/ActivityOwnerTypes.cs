//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Routing;
using Tunynet.Common;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 动态所有者类型扩展
    /// </summary>
    public static class ActivityOwnerTypesExtension
    {
        /// <summary>
        /// 问答
        /// </summary>
        public static int AskQuestion(this ActivityOwnerTypes ActivityOwnerTypes)
        {
            return 1301;
        }

        /// <summary>
        /// 问答(标签)
        /// </summary>
        public static int AskTag(this ActivityOwnerTypes ActivityOwnerTypes)
        {
            return 1303;
        }
    }
}