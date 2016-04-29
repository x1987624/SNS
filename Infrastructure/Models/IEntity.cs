//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2011-10-11</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2011-10-11" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunynet
{
    /// <summary>
    /// Entity接口（所有实体都应该实现该接口）
    /// </summary>
    /// <example>
    /// <para>应该显示实现该接口，例如：</para>
    ///     <![CDATA[
    ///     [TableName("test_SampleEntities")]
    ///     [PrimaryKey("Id", autoIncrement = true)]
    ///     [CacheSetting(true, PropertyNamesOfArea = "UserId,AuditStatus", PropertyNameOfBody = "Body")]
    ///     [Serializable]
    ///     public class SampleEntity : IEntity
    ///     {
    ///         ......
    ///         #region IEntity 成员
    ///         object IEntity.EntityId { get { return this.Id; } }
    ///         bool IEntity.IsDeletedInDatabase { get; set; }
    ///         #endregion
    ///     }
    /// ]]>
    /// </example>
    public interface IEntity
    {
        /// <summary>
        /// 实体ID
        /// </summary>
        object EntityId { get; }

        /// <summary>
        /// 该实体是否已经在数据库中删除(分布式部署时使用)
        /// </summary>
        bool IsDeletedInDatabase { get; set; }
    }

}
