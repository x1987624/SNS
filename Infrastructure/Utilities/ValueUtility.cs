﻿//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-01-04</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-01-04" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet.Utilities
{
    /// <summary>
    /// 用于类型转换的工具类
    /// </summary>
    public static class ValueUtility
    {
        #region DB values

        /// <summary>
        /// 获取安全的SQL Server DateTime
        /// </summary>
        public static System.DateTime GetSafeSqlDateTime(System.DateTime date)
        {
            if (date < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue)
                return (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue.Value.AddYears(1);
            else if (date > (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue)
                return (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue;
            return date;
        }

        /// <summary>
        /// 获取安全的SQL Server int
        /// </summary>
        public static int GetSafeSqlInt(int i)
        {
            if (i <= (int)System.Data.SqlTypes.SqlInt32.MinValue)
                return (int)System.Data.SqlTypes.SqlInt32.MinValue + 1;
            else if (i >= (int)System.Data.SqlTypes.SqlInt32.MaxValue)
                return (int)System.Data.SqlTypes.SqlInt32.MaxValue - 1;
            return i;
        }

        /// <summary>
        /// 获取在SQL Server中可以使用的整型最大值
        /// </summary>
        /// <returns></returns>
        public static int GetSqlMaxInt()
        {
            return (int)System.Data.SqlTypes.SqlInt32.MaxValue - 1;
        }

        #endregion

        /// <summary>
        /// 把字符串数组转换成整型列表
        /// </summary>
        /// <param name="strArray">需要转换的字符串数组</param>
        /// <returns>根据字符串数据转换后的数值集合</returns>
        public static List<int> ParseInt(string[] strArray)
        {
            List<int> result = new List<int>();

            if (strArray == null || strArray.Length == 0)
                return result;

            foreach (string str in strArray)
            {
                int tempInt = 0;
                if (int.TryParse(str, out tempInt))
                    result.Add(tempInt);
            }
            return result;
        }

        /// <summary>
        /// 把value转换成类型为T的数据，无法进行转换时返回defaultValue
        /// </summary>
        /// <typeparam name="T">需转换的类型</typeparam>
        /// <param name="value">待转换的数据</param>
        /// <returns>转换后的数据</returns>
        public static T ChangeType<T>(object value)
        {
            return ChangeType<T>(value, default(T));
        }

        /// <summary>
        /// 把value转换成类型为T的数据，无法进行转换时返回defaultValue
        /// </summary>
        /// <typeparam name="T">需转换的类型参数</typeparam>
        /// <param name="value">待转换的数据</param>
        /// <param name="defalutValue">无法转换时需返回的默认值</param>
        /// <returns>转换后的数据</returns>
        public static T ChangeType<T>(object value, T defalutValue)
        {
            if (value != null)
            {
                Type tType = typeof(T);
                if (tType.IsInterface || (tType.IsClass && tType != typeof(string)))
                {
                    if (value is T)
                        return (T)value;
                }
                else if (tType.IsGenericType && tType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(tType));
                }
                else if (tType.IsEnum)
                {
                    return (T)Enum.Parse(tType, value.ToString());
                }
                else
                {
                    return (T)Convert.ChangeType(value, tType);
                }
            }
            return defalutValue;
        }

    }
}
