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

namespace Tunynet
{
    /// <summary>
    /// 可序列化属性接口
    /// </summary>
    /// <remarks>
    /// 定义可序列化属性的实体建议从<see cref="Tunynet.SerializablePropertiesBase"/>派生
    /// </remarks>
    /// <include file='SerializablePropertiesExample.xml' path='doc/members/member[@name="T:Tunynet.ISerializableProperties"]/example'/>
    public interface ISerializableProperties
    {
        /// <summary>
        /// 获取propertyName指定的属性值
        /// </summary>
        /// <remarks>
        /// 不能显式实现
        /// </remarks>
        /// <param name="propertyName">序列化属性名称字符串</param>
        T GetExtendedProperty<T>(string propertyName);

        /// <summary>
        /// 获取propertyName指定的属性值
        /// </summary>
        /// <remarks>
        /// 不能显式实现
        /// </remarks>
        /// <param name="propertyName">属性名称</param>
        /// <param name="defaultValue">如果未找到则返回该默认值</param>
        T GetExtendedProperty<T>(string propertyName, T defaultValue);

        /// <summary>
        /// 设置扩展属性
        /// </summary>
        /// <remarks>
        /// 不能显式实现
        /// </remarks>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyValue">属性值</param>
        void SetExtendedProperty(string propertyName, object propertyValue);

        /// <summary>
        /// 把扩展属性序列化
        /// </summary>
        /// <remarks>
        /// 需要显式实现
        /// </remarks>
        void Serialize();

    }
}
