//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册用户计数类型扩展类
    /// </summary>
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 照片数
        /// </summary>
        public static string PhotoCount(this OwnerDataKeys ownerDataKeys)
        {
            return PhotoConfig.Instance().ApplicationKey+"-ThreadCount";
        }
    }
}