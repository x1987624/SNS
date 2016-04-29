//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 扩展NoticeTemplateNames
    /// </summary>
    public static class NoticeTemplateNamesExtension
    {
        /// <summary>
        /// 兑换申请被批准
        /// </summary>
        public static string ApplyRecord(this NoticeTemplateNames noticeTemplateNames)
        {
            return "ApplyRecord";
        }

        /// <summary>
        /// 兑换申请被拒绝
        /// </summary>
        public static string CancelRecord(this NoticeTemplateNames noticeTemplateNames)
        {
            return "CancelRecord";
        }        
    }
}