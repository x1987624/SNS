//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-02-16</createdate>
//<author>mazq</author>
//<email>mazq@tunynet.com</email>
//<log date="2012-02-16" version="0.5">创建</log>
//<log date="2012-02-18" version="0.6" author="zhengw">走查</log>
//--------------------------------------------------------------
//</TunynetCopyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Caching;
using PetaPoco;

namespace Tunynet.Logging
{
    /// <summary>
    /// 操作日志实体
    /// </summary>
    [TableName("tn_OperationLogs")]
    [PrimaryKey("Id", autoIncrement = true)]
    [CacheSetting(false)]
    [Serializable]
    public class OperationLogEntry : IEntity, IOperationLogSpecificPart
    {

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public OperationLogEntry()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public OperationLogEntry(OperatorInfo operatorInfo)
        {
            this.OperatorUserId = operatorInfo.OperatorUserId;
            this.OperatorIP = operatorInfo.OperatorIP;
            this.Operator = operatorInfo.Operator;
            this.AccessUrl = operatorInfo.AccessUrl;
            this.DateCreated = DateTime.UtcNow;
        }

        #region 需持久化属性

        /// <summary>
        ///Id
        /// </summary>
        public long Id { get; protected set; }

        /// <summary>
        ///应用Id
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        ///日志来源，一般为应用模块名称
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///操作类型标识
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        ///操作对象名称
        /// </summary>
        public string OperationObjectName { get; set; }

        /// <summary>
        ///OperationObjectId
        /// </summary>
        public long OperationObjectId { get; set; }

        /// <summary>
        ///操作描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///操作者UserId
        /// </summary>
        public long OperatorUserId { get; set; }

        /// <summary>
        ///操作者名称
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        ///操作者IP
        /// </summary>
        public string OperatorIP { get; set; }

        /// <summary>
        ///操作访问的url
        /// </summary>
        public string AccessUrl { get; set; }

        /// <summary>
        ///创建日期
        /// </summary>
        public DateTime DateCreated { get; set; }

        #endregion

        #region IEntity 成员

        object IEntity.EntityId { get { return this.Id; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
    }

}

