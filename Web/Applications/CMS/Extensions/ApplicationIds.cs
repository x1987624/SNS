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
using Spacebuilder.Common;

namespace Spacebuilder.CMS
{

    /// <summary>
    /// 应用Id
    /// </summary>
    public static class ApplicationIdsExtension
    {
        /// <summary>
        /// 资讯应用Id
        /// </summary>
        public static int CMS(this ApplicationIds applicationIds)
        {
            return 1015;
        }
    }

}
