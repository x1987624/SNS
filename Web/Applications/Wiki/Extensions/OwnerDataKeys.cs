//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using Tunynet.Common;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 用户计数类型扩展类
    /// </summary>
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 创建词条数
        /// </summary>
        public static string PageCount(this OwnerDataKeys ownerDataKeys)
        {
            return WikiConfig.Instance().ApplicationKey + "-PageCount";
        }

        /// <summary>
        /// 编辑词条版本数
        /// </summary>
        public static string EditionCount(this OwnerDataKeys ownerDataKeys)
        {
            return WikiConfig.Instance().ApplicationKey + "-EditionCount";
        }
    }
}