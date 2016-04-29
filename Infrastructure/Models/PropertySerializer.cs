//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-12-22</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-12-22" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Globalization;

namespace Tunynet
{
    /// <summary>
    /// 属性序列化器
    /// </summary>
    [Serializable]
    public class PropertySerializer
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyNames">propertyNames</param>
        /// <param name="propertyValues">propertyValues</param>
        public PropertySerializer(string propertyNames, string propertyValues)
        {
            if (!string.IsNullOrEmpty(propertyNames) && !string.IsNullOrEmpty(propertyValues))
                this.extendedAttributes = ConvertToNameValueCollection(propertyNames, propertyValues);
            else
                this.extendedAttributes = new NameValueCollection();
        }

        /// <summary>
        /// 存储前把扩展属性序列化成PropertyNames和PropertyValues
        /// </summary>
        public void Serialize(ref string propertyNames, ref string propertyValues)
        {
            ConvertFromNameValueCollection(this.extendedAttributes, ref propertyNames, ref propertyValues);
        }

        /// <summary>
        /// 获取propertyName指定的属性值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        public T GetExtendedProperty<T>(string propertyName)
        {
            if (typeof(T) == typeof(string))
            {
                TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                return GetExtendedProperty<T>(propertyName, (T)conv.ConvertFrom(string.Empty));
            }
            else
            {
                return GetExtendedProperty<T>(propertyName, default(T));
            }
        }

        /// <summary>
        /// 获取propertyName指定的属性值
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <param name="defaultValue">如果未找到则返回该默认值</param>
        public T GetExtendedProperty<T>(string propertyName, T defaultValue)
        {
            string returnValue = extendedAttributes[propertyName];
            if (returnValue == null)
                return defaultValue;
            else
                return (T)Convert.ChangeType(returnValue, typeof(T));
        }

        /// <summary>
        /// 设置扩展属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyValue">属性值</param>
        public void SetExtendedProperty(string propertyName, object propertyValue)
        {
            if (propertyValue == null)
                extendedAttributes.Remove(propertyName);

            string propertyValue_String = propertyValue.ToString().Trim();
            if (string.IsNullOrEmpty(propertyValue_String))
                extendedAttributes.Remove(propertyName);
            else
                extendedAttributes[propertyName] = propertyValue_String;
        }

        NameValueCollection extendedAttributes = new NameValueCollection();

        #region NameValueCollection格式  序列化&反序列化

        /// <summary>
        /// 从序列化字符串(propertyNames、propertyValues)生成NameValueCollection
        /// </summary>
        /// <param name="propertyNames">用于生成NameValueCollection的Names</param>
        /// <param name="propertyValues">用于生成NameValueCollection的Vaules</param>        
        /// <example>
        /// string keys = "key1:S:0:3:key2:S:3:4:";
        /// string values = "1234567";
        /// 将返回一个NameValueCollection，包括2个Keys(Key1、Key2) 和对应的两个values（123、4567）
        /// </example>
        /// <returns>
        /// 属性名称、值构成的字典集合
        /// </returns>
        private static NameValueCollection ConvertToNameValueCollection(string propertyNames, string propertyValues)
        {
            NameValueCollection nvc = new NameValueCollection();

            if (propertyNames != null && propertyValues != null && propertyNames.Length > 0 && propertyValues.Length > 0)
            {
                char[] splitter = new char[1] { ':' };
                string[] keyNames = propertyNames.Split(splitter);

                for (int i = 0; i < (keyNames.Length / 4); i++)
                {
                    int start = int.Parse(keyNames[(i * 4) + 2], CultureInfo.InvariantCulture);
                    int len = int.Parse(keyNames[(i * 4) + 3], CultureInfo.InvariantCulture);
                    string key = keyNames[i * 4];

                    //Future version will support more complex types	
                    if (((keyNames[(i * 4) + 1] == "S") && (start >= 0)) && (len > 0) && (propertyValues.Length >= (start + len)))
                    {
                        nvc[key] = propertyValues.Substring(start, len);
                    }
                }
            }

            return nvc;
        }

        /// <summary>
        /// 从NameValueCollection生成序列化属性的名称字符串和值字符串
        /// </summary>
        /// <param name="nvc">要转换的NameValueCollection</param>
        /// <param name="propertyNames">NameValueCollection生成的序列化属性名称字符串</param>
        /// <param name="propertyValues">NameValueCollection生成的序列化属性值字符串</param>
        /// <exception cref="System.ArgumentException">序列化属性的名称不能包括":"，否则抛出<see cref="System.ArgumentException"/>异常</exception>
        private static void ConvertFromNameValueCollection(NameValueCollection nvc, ref string propertyNames, ref string propertyValues)
        {
            if (nvc == null || nvc.Count == 0)
                return;

            StringBuilder sbKey = new StringBuilder();
            StringBuilder sbValue = new StringBuilder();

            int index = 0;
            foreach (string key in nvc.AllKeys)
            {
                if (key.IndexOf(':') != -1)
                    throw new ArgumentException("SerializableProperties Name can not contain the character \":\"");

                string v = nvc[key];
                if (!string.IsNullOrEmpty(v))
                {
                    sbKey.AppendFormat("{0}:S:{1}:{2}:", key, index, v.Length);
                    sbValue.Append(v);
                    index += v.Length;
                }
            }
            propertyNames = sbKey.ToString();
            propertyValues = sbValue.ToString();
        }

        #endregion

    }
}