//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Events;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// 扩展用户业务逻辑
    /// </summary>
    public static class EventOperationTypeExtension
    {
        /// <summary>
        /// 设置全局置顶
        /// </summary>
        /// <param name="eventOperationType"></param>
        /// <returns></returns>
        public static string SetGlobalSticky(this EventOperationType eventOperationType)
        {
            return "SetGlobalSticky";
        }

        /// <summary>
        /// 取消全局置顶
        /// </summary>
        /// <param name="eventOperationType"></param>
        /// <returns></returns>
        public static string CancelGlobalSticky(this EventOperationType eventOperationType)
        {
            return "CancelGlobalSticky";
        }

        /// <summary>
        /// 设置栏目置顶
        /// </summary>
        /// <param name="eventOperationType"></param>
        /// <returns></returns>
        public static string SetFolderSticky(this EventOperationType eventOperationType)
        {
            return "SetFolderSticky";
        }

        /// <summary>
        /// 取消栏目置顶
        /// </summary>
        /// <param name="eventOperationType"></param>
        /// <returns></returns>
        public static string CancelFolderSticky(this EventOperationType eventOperationType)
        {
            return "CancelFolderSticky";
        }
    }
}
