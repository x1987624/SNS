//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 扩展NoticeTemplateNames
    /// </summary>
    public static class NoticeTemplateNamesExtension
    {
        
        /// <summary>
        /// 照片有新评论
        /// </summary>
        public static string NewPhotoComment(this NoticeTemplateNames noticeTemplateNames)
        {
            return "NewPhotoComment";
        }

    }
}