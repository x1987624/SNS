//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-12-15</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-12-15" version="0.5">新建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Tunynet.Utilities
{
    /// <summary>
    /// IEnumerable&lt;T&gt; 只读集合扩展方法
    /// </summary>
    public static class ReadOnlyCollectionExtension
    {
        /// <summary>
        /// 转换成只读集合
        /// </summary>
        /// <typeparam name="T">类型参数</typeparam>
        /// <param name="enumerable">可枚举的集合</param>
        /// <returns>返回只读集合</returns>
        public static IList<T> ToReadOnly<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable.ToList());
        }
    }
}
