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

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册积分项扩展
    /// </summary>
    public static class PointItemKeysExtension
    {
        /// <summary>
        /// 上传照片
        /// </summary>
        /// <param name="pointItemKeys"></param>
        /// <returns></returns>
        public static string Photo_UploadPhoto(this PointItemKeys pointItemKeys)
        {
            return "Photo_UploadPhoto";
        }

        /// <summary>
        /// 删除照片
        /// </summary>
        /// <param name="pointItemKeys"></param>
        /// <returns></returns>
        public static string Photo_DeletePhoto(this PointItemKeys pointItemKeys)
        {
            return "Photo_DeletePhoto";
        }

        /// <summary>
        /// 照片被喜欢
        /// </summary>
        /// <param name="pointItemKeys"></param>
        /// <returns></returns>
        public static string Photo_BeLiked(this PointItemKeys pointItemKeys)
        {
            return "Photo_BeLiked";
        }

        /// <summary>
        /// 照片被圈
        /// </summary>
        /// <param name="pointItemKeys"></param>
        /// <returns></returns>
        public static string Photo_BeLabelled(this PointItemKeys pointItemKeys)
        {
            return "Photo_BeLabelled";
        }
        
        /// <summary>
        /// 圈人被删
        /// </summary>
        /// <param name="pointItemKeys"></param>
        /// <returns></returns>
        public static string Photo_BeLabelled_Delete(this PointItemKeys pointItemKeys)
        {
            return "Photo_BeLabelled_Delete";
        }
    }
}
