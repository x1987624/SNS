//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册应用动态项
    /// </summary>
    public static class ActivityItemKeysExtension
    {
        /// <summary>
        /// 发布照片
        /// </summary>
        public static string CreatePhoto(this ActivityItemKeys activityItemKeys)
        {
            return "CreatePhoto";
        }

        /// <summary>
        /// 照片圈人
        /// </summary>
        public static string LabelPhoto(this ActivityItemKeys activityItemKeys)
        {
            return "LabelPhoto";
        }

        /// <summary>
        /// 评论照片
        /// </summary>
        public static string CommentPhoto(this ActivityItemKeys activityItemKeys)
        {
            return "CommentPhoto";
        }
    }
}
