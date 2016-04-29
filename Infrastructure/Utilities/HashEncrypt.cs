//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2006-02-23</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2006-02-23"  version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Security.Cryptography;

namespace Tunynet.Utilities
{
    /// <summary>
    /// 非对称加密算法
    /// </summary>
    public class HashEncrypt
    {
        private HashEncryptType _mbytHashType;
        private string _mstrOriginalString;
        private string _mstrHashString;
        private HashAlgorithm _mhash;
        bool mboolUseSalt;
        string mstrSaltValue = String.Empty;
        short msrtSaltLength = 8;

        #region "Public Properties"

        /// <summary>
        /// 非对称加密类型
        /// </summary>
        public HashEncryptType HashType
        {
            get { return _mbytHashType; }
            set
            {
                if (_mbytHashType != value)
                {
                    _mbytHashType = value;
                    _mstrOriginalString = String.Empty;
                    _mstrHashString = String.Empty;

                    this.SetEncryptor();
                }
            }
        }

        /// <summary>
        /// SaltValue
        /// </summary>
        public string SaltValue
        {
            get { return mstrSaltValue; }
            set { mstrSaltValue = value; }
        }

        /// <summary>
        /// UseSalt
        /// </summary>
        public bool UseSalt
        {
            get { return mboolUseSalt; }
            set { mboolUseSalt = value; }
        }

        /// <summary>
        /// SaltLength
        /// </summary>
        public short SaltLength
        {
            get { return msrtSaltLength; }
            set { msrtSaltLength = value; }
        }

        /// <summary>
        /// 原始字符串
        /// </summary>
        public string OriginalString
        {
            get { return _mstrOriginalString; }
            set { _mstrOriginalString = value; }
        }

        /// <summary>
        /// 加密后的字符串
        /// </summary>
        public string HashString
        {
            get { return _mstrHashString; }
            set { _mstrHashString = value; }
        }

        #endregion

        #region "Constructors"

        /// <summary>
        /// 构造函数
        /// </summary>
        public HashEncrypt()
        {
            _mbytHashType = HashEncryptType.MD5;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hashType">加密类型</param>
        public HashEncrypt(HashEncryptType hashType)
        {
            _mbytHashType = hashType;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="originalString">原始字符串</param>
        /// <param name="hashType">加密类型</param>
        public HashEncrypt(HashEncryptType hashType, string originalString)
        {
            _mbytHashType = hashType;
            _mstrOriginalString = originalString;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hashType">加密类型</param>
        /// <param name="originalString">原始字符串</param>
        /// <param name="useSalt">是否使用散列</param>
        /// <param name="saltValue">散列值（如果启用散列但没有传入散列值则使用随机生的散列值）</param>
        public HashEncrypt(HashEncryptType hashType, string originalString, bool useSalt, string saltValue)
        {
            _mbytHashType = hashType;
            _mstrOriginalString = originalString;
            mboolUseSalt = useSalt;
            mstrSaltValue = saltValue;
        }

        #endregion

        #region "SetEncryptor() Method"

        /// <summary>
        /// 设置加密算法
        /// </summary>
        private void SetEncryptor()
        {
            switch (_mbytHashType)
            {
                case HashEncryptType.MD5:
                    _mhash = new MD5CryptoServiceProvider();
                    break;
                case HashEncryptType.SHA1:
                    _mhash = new SHA1CryptoServiceProvider();
                    break;
                case HashEncryptType.SHA256:
                    _mhash = new SHA256Managed();
                    break;
                case HashEncryptType.SHA384:
                    _mhash = new SHA384Managed();
                    break;
                case HashEncryptType.SHA512:
                    _mhash = new SHA512Managed();
                    break;
            }
        }

        #endregion

        #region "Encrypt() Methods"

        /// <summary>
        /// 进行非对称加密
        /// </summary>
        public string Encrypt()
        {
            byte[] bytValue;
            byte[] bytHash;

            // Create New Crypto Service Provider Object
            this.SetEncryptor();

            // Check to see if we will Salt the value
            if (mboolUseSalt)
                if (mstrSaltValue.Length == 0)
                    mstrSaltValue = this.CreateSalt();

            // Convert the original string to array of Bytes
            bytValue =
              System.Text.Encoding.UTF8.GetBytes(
              mstrSaltValue + _mstrOriginalString);

            // Compute the Hash, returns an array of Bytes  
            bytHash = _mhash.ComputeHash(bytValue);

            // Return a base 64 encoded string of the Hash value
            return Convert.ToBase64String(bytHash);
        }

        /// <summary>
        /// 进行非对称加密
        /// </summary>
        /// <param name="originalString">原始字符串</param>
        public string Encrypt(string originalString)
        {
            _mstrOriginalString = originalString;

            return this.Encrypt();
        }

        /// <summary>
        /// 进行非对称加密
        /// </summary>
        /// <param name="originalString">原始字符串</param>
        /// <param name="hashType">加密类型</param>
        public string Encrypt(string originalString, HashEncryptType hashType)
        {
            _mstrOriginalString = originalString;
            _mbytHashType = hashType;

            return this.Encrypt();
        }

        /// <summary>
        /// 进行非对称加密
        /// </summary>
        /// <param name="originalString">原始字符串</param>
        /// <param name="useSalt">是否启用散列</param>
        public string Encrypt(string originalString, bool useSalt)
        {
            _mstrOriginalString = originalString;
            mboolUseSalt = useSalt;

            return this.Encrypt();
        }

        /// <summary>
        /// 进行非对称加密
        /// </summary>
        /// <param name="originalString">原始字符串</param>
        /// <param name="hashType">加密类型</param>
        /// <param name="saltValue">散列值</param>
        public string Encrypt(string originalString, HashEncryptType hashType, string saltValue)
        {
            _mstrOriginalString = originalString;
            _mbytHashType = hashType;
            mstrSaltValue = saltValue;

            return this.Encrypt();
        }

        /// <summary>
        /// 进行非对称加密
        /// </summary>
        /// <param name="originalString">原始字符串</param>
        /// <param name="saltValue">散列值</param>
        public string Encrypt(string originalString, string saltValue)
        {
            _mstrOriginalString = originalString;
            mstrSaltValue = saltValue;

            return this.Encrypt();
        }

        #endregion

        #region "Misc. Routines"

        /// <summary>
        /// 重置加密设置
        /// </summary>
        public void Reset()
        {
            mstrSaltValue = String.Empty;
            _mstrOriginalString = String.Empty;
            _mstrHashString = String.Empty;
            mboolUseSalt = false;
            _mbytHashType = HashEncryptType.MD5;

            _mhash = null;
        }

        /// <summary>
        /// 创建散列
        /// </summary>
        public string CreateSalt()
        {
            byte[] bytSalt = new byte[8];
            RNGCryptoServiceProvider rng;

            rng = new RNGCryptoServiceProvider();

            rng.GetBytes(bytSalt);

            return Convert.ToBase64String(bytSalt);
        }

        #endregion

    }

    /// <summary>
    /// 非对称加密类型
    /// </summary>
    public enum HashEncryptType : byte
    {
        /// <summary>
        /// MD5算法
        /// </summary>
        MD5,
        /// <summary>
        /// SHA1算法
        /// </summary>
        SHA1,
        /// <summary>
        /// SHA256算法
        /// </summary>
        SHA256,
        /// <summary>
        /// SHA384算法
        /// </summary>
        SHA384,
        /// <summary>
        /// SHA512算法
        /// </summary>
        SHA512
    }
}
