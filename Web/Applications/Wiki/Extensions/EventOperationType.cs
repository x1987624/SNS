//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tunynet.Common;
using Tunynet.Events;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public static class EventOperationTypeExtension
    {
        /// <summary>
        /// 加锁
        /// </summary>
        /// <param name="eventOperationType"></param>      
        /// <returns></returns>
        public static string SetLock(this EventOperationType eventOperationType)
        {
            return "SetLock";
        }

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="eventOperationType"></param>      
        /// <returns></returns>
        public static string CancelLock(this EventOperationType eventOperationType)
        {
            return "CancelLock";
        }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        /// <param name="eventOperationType"></param>      
        /// <returns></returns>
        public static string LogicalDelete(this EventOperationType eventOperationType)
        {
            return "LogicalDelete";
        }
        /// <summary>
        /// 恢复删除的词条
        /// </summary>
        /// <param name="eventOperationType"></param>      
        /// <returns></returns>
        public static string Restore(this EventOperationType eventOperationType)
        {
            return "Restore";
        }
    }
}
