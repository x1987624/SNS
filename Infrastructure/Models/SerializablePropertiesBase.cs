//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-12-23</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-12-23" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet
{
    /// <summary>
    /// 可序列化属性基类
    /// </summary>
    /// <include file='SerializablePropertiesExample.xml' path='doc/members/member[@name="T:Tunynet.SerializablePropertiesBase"]/example'/>
    [Serializable]
    public abstract class SerializablePropertiesBase : ISerializableProperties
    {

        #region ISerializableProperties 成员

        /// <summary>
        /// 获取propertyName指定的属性值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        public T GetExtendedProperty<T>(string propertyName)
        {
            return PropertySerializer.GetExtendedProperty<T>(propertyName);
        }

        /// <summary>
        /// 获取propertyName指定的属性值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="defaultValue">如果未找到则返回该默认值</param>
        public T GetExtendedProperty<T>(string propertyName, T defaultValue)
        {
            return PropertySerializer.GetExtendedProperty<T>(propertyName, defaultValue);
        }

        /// <summary>
        /// 设置可序列化属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyValue">属性值</param>
        public void SetExtendedProperty(string propertyName, object propertyValue)
        {
            PropertySerializer.SetExtendedProperty(propertyName, propertyValue);
        }

        /// <summary>
        /// 存储前把可序列化属性序列化成PropertyNames和PropertyValues
        /// </summary>
        void ISerializableProperties.Serialize()
        {
            PropertySerializer.Serialize(ref this.propertyNames, ref this.propertyValues);
        }

        private PropertySerializer propertySerializer = null;
        /// <summary>
        /// 可序列化属性序列化器
        /// </summary>
        protected PropertySerializer PropertySerializer
        {
            get
            {
                if (propertySerializer == null)
                    propertySerializer = new PropertySerializer(this.PropertyNames, this.PropertyValues);

                return propertySerializer;
            }
        }

        private string propertyNames;
        /// <summary>
        /// 序列化属性名称字符串
        /// </summary>
        /// <remarks>
        /// 保留该属性的目的是通过orm存取数据库的数据
        /// </remarks>
        public string PropertyNames
        {
            get { return propertyNames; }
            protected set { this.propertyNames = value; }
        }

        private string propertyValues;
        /// <summary>
        /// 序列化属性值字符串
        /// </summary>
        /// <remarks>
        /// 保留该属性的目的是通过orm存取数据库的数据
        /// </remarks>
        public string PropertyValues
        {
            get { return propertyValues; }
            protected set { this.propertyValues = value; }
        }

        #endregion

    }
}
