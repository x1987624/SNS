//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-9</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-9" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Tunynet.Utilities
{
    /// <summary>
    /// 加密工具类
    /// </summary>
    public static class EncryptionUtility
    {

        #region 对称/非对称

        /// <summary>
        /// 对称加密
        /// </summary>
        /// <param name="encryptType">加密类型</param>
        /// <param name="str">需要加密的字符串</param>
        /// <param name="ivString">初始化向量</param>
        /// <param name="keyString">加密密钥</param>
        /// <returns>加密后的字符串</returns>
        public static string SymmetricEncrypt(SymmetricEncryptType encryptType, string str, string ivString, string keyString)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(ivString) || string.IsNullOrEmpty(keyString))
                return str;

            SymmetricEncrypt encrypt = new SymmetricEncrypt(encryptType);
            encrypt.IVString = ivString;
            encrypt.KeyString = keyString;
            return encrypt.Encrypt(str);
        }

        /// <summary>
        /// 对称解密
        /// </summary>
        /// <param name="encryptType">加密类型</param>
        /// <param name="str">需要加密的字符串</param>
        /// <param name="ivString">初始化向量</param>
        /// <param name="keyString">加密密钥</param>
        /// <returns>解密后的字符串</returns>
        public static string SymmetricDncrypt(SymmetricEncryptType encryptType, string str, string ivString, string keyString)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            SymmetricEncrypt encrypt = new SymmetricEncrypt(encryptType);
            encrypt.IVString = ivString;
            encrypt.KeyString = keyString;
            return encrypt.Decrypt(str);
        }

        #endregion

        #region MD5

        /// <summary>
        /// 标准MD5加密
        /// </summary>
        /// <param name="str">待加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string MD5(string str)
        {
            byte[] b = Encoding.UTF8.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("x").PadLeft(2, '0');
            return ret;
        }

        /// <summary>
        /// 16位的MD5加密
        /// </summary>
        /// <param name="str">待加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string MD5_16(string str)
        {
            return MD5(str).Substring(8, 16);
        }

        #endregion

        #region base64编码/解码

        /// <summary>
        /// base64编码
        /// </summary>
        /// <param name="str">待编码的字符串</param>
        /// <returns>编码后的字符串</returns>
        public static string Base64_Encode(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        /// <summary>
        /// base64解码
        /// </summary>
        /// <param name="str">待解码的字符串</param>
        /// <returns>解码后的字符串</returns>
        public static string Base64_Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }

        #endregion
    }
}
