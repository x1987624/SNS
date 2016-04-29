//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-12-02</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-12-02" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace Tunynet.Utilities
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringUtility
    {
        #region String length formatter

        /// <summary>
        /// 对字符串进行截字，截去字的部分用"..."替代
        /// </summary>
        /// <remarks>
        /// 一个字符指双字节字符，单字节字符仅算半个字符
        /// </remarks>
        /// <param name="rawString">待截字的字符串</param>
        /// <param name="charLimit">截字的长度，按双字节计数</param>
        /// <returns>截字后的字符串</returns>
        public static string Trim(string rawString, int charLimit)
        {
            return Trim(rawString, charLimit, "...");
        }

        /// <summary>
        /// 对字符串进行截字(区分单字节及双字节字符)
        /// </summary>
        /// <remarks>
        /// 一个字符指双字节字符，单字节字符仅算半个字符
        /// </remarks>
        /// <param name="rawString">待截字的字符串</param>
        /// <param name="charLimit">截字的长度，按双字节计数</param>
        /// <param name="appendString">截去字的部分用替代字符串</param>
        /// <returns>截字后的字符串</returns>
        public static string Trim(string rawString, int charLimit, string appendString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length <= charLimit)
            {
                return rawString;
            }

            int rawStringLength = Encoding.UTF8.GetBytes(rawString).Length;
            if (rawStringLength <= charLimit * 2)
                return rawString;

            charLimit = charLimit * 2 - Encoding.UTF8.GetBytes(appendString).Length;
            StringBuilder checkedStringBuilder = new StringBuilder();
            int appendedLenth = 0;
            for (int i = 0; i < rawString.Length; i++)
            {
                char _char = rawString[i];
                checkedStringBuilder.Append(_char);

                appendedLenth += _char > 0x80 ? 2 : 1;

                if (appendedLenth >= charLimit)
                    break;
            }

            return checkedStringBuilder.Append(appendString).ToString();
        }

        #endregion

        #region Encode & Decode

        /// <summary>
        /// Unicode转义序列
        /// </summary>
        /// <param name="rawString">待编码的字符串</param>
        public static string UnicodeEncode(string rawString)
        {
            if (rawString == null || rawString == string.Empty)
                return rawString;
            StringBuilder text = new StringBuilder();
            foreach (int c in rawString)
            {
                string t = "";
                if (c > 126)
                {
                    text.Append("\\u");
                    t = c.ToString("x");
                    for (int x = 0; x < 4 - t.Length; x++)
                    {
                        text.Append("0");
                    }
                }
                else
                {
                    t = ((char)c).ToString();
                }
                text.Append(t);
            }

            return text.ToString();
        }

        #endregion

        #region Xml clean

        /// <summary>
        /// 清除xml中的不合法字符
        /// </summary>
        /// <remarks>
        /// <para>无效字符：</para>
        /// <list type="number">
        /// <item>0x00 - 0x08</item>
        /// <item>0x0b - 0x0c</item>
        /// <item>0x0e - 0x1f</item>
        /// </list>        
        /// </remarks>
        /// <param name="rawXml">待清理的xml字符串</param>
        public static string CleanInvalidCharsForXML(string rawXml)
        {
            if (string.IsNullOrEmpty(rawXml))
                return rawXml;

            StringBuilder checkedStringBuilder = new StringBuilder();
            Char[] chars = rawXml.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                int charValue = Convert.ToInt32(chars[i]);

                if ((charValue >= 0x00 && charValue <= 0x08)
                    || (charValue >= 0x0b && charValue <= 0x0c)
                    || (charValue >= 0x0e && charValue <= 0x1f))
                    continue;

                checkedStringBuilder.Append(chars[i]);
            }

            return checkedStringBuilder.ToString();
        }

        #endregion

        #region SQLInjection 预防

        /// <summary>
        /// 清理Sql注入特殊字符
        /// </summary>
        /// <remarks>
        /// 需清理字符：'、--、exec 、' or
        /// </remarks>
        /// <param name="sql">待处理的sql字符串</param>
        /// <returns>清理后的sql字符串</returns>
        public static string StripSQLInjection(string sql)
        {
            if (!string.IsNullOrEmpty(sql))
            {
                //防止执行 ' or
                string pattern1 = @"((\%27)|(\'))\s*((\%6F)|o|(\%4F))((\%72)|r|(\%52))";

                //过滤 ' --
                string pattern2 = @"(\%27)|(\')|(\-\-)";

                //防止执行sql server 内部存储过程或扩展存储过程
                string pattern3 = @"\s+exec(\s|\+)+(s|x)p\w+";

                sql = Regex.Replace(sql, pattern1, string.Empty, RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, pattern2, string.Empty, RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, pattern3, string.Empty, RegexOptions.IgnoreCase);

                sql = sql.Replace("%", "[%]");
            }
            return sql;
        }

        #endregion
    }
}
