using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;

namespace Spacebuilder.PointMall
{
    /// <summary>
    /// 计数类型扩展类
    /// </summary>
    public static class OwnerDataKeysExtension
    {
        /// <summary>
        /// 申请总数
        /// </summary>
        public static string PostCount(this OwnerDataKeys ownerDataKeys)
        {
            return PointMallConfig.Instance().ApplicationKey + "-ThreadCount";
        }
    }
}