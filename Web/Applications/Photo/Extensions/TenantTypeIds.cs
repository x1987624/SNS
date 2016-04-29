//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 相册定义的租户类型
    /// </summary>
    public static class TenantTypeIdsExtension
    {

        /// <summary>
        /// 相册应用
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string PhotoApplication(this TenantTypeIds TenantTypeIds)
        {
            return "100300";
        }

        /// <summary>
        /// 相册
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string Album(this TenantTypeIds TenantTypeIds)
        {
            return "100301";
        }

        /// <summary>
        /// 照片
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string Photo(this TenantTypeIds TenantTypeIds)
        {
            return "100302";
        }

        /// <summary>
        /// 照片圈人
        /// </summary>
        /// <param name="TenantTypeIds">被扩展对象</param>
        public static string PhotoLabel(this TenantTypeIds TenantTypeIds)
        {
            return "100303";
        }
    }
}