//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.Caching;
using PetaPoco;
using Tunynet;

namespace Spacebuilder.CMS.Metadata
{
    /// <summary>
    /// 内容模型字段
    /// </summary>
    [TableName("spb_cms_ContentTypeColumnDefinitions")]
    [PrimaryKey("ColumnId", autoIncrement = true)]
    [CacheSetting(true, PropertyNamesOfArea = "ContentTypeId")]
    public class ContentTypeColumnDefinition : IEntity, ICloneable
    {
        /// <summary>
        /// ColumnId
        /// </summary>
        public int ColumnId { get; set; }

        /// <summary>
        /// ContentTypeId
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string ColumnName { get; set; }

        private string columnLabel = string.Empty;
        /// <summary>
        /// 字段标签（字段的解释说明）
        /// </summary>
        public string ColumnLabel
        {
            get { return columnLabel; }
            set { columnLabel = value; }
        }

        private bool isBuiltIn = false;
        /// <summary>
        /// 字段名称
        /// </summary>
        public bool IsBuiltIn
        {
            get { return isBuiltIn; }
            set { isBuiltIn = value; }
        }

        /// <summary>
        /// DataType
        /// </summary>
        public string DataType { get; set; }

        private int length = 0;
        /// <summary>
        /// 字段长度(仅适用于String)
        /// </summary>
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        private string precision = string.Empty;
        /// <summary>
        /// 精度及小数位数(仅适用于Decimal，例如 18,4)
        /// </summary>
        public string Precision
        {
            get { return precision; }
            set { precision = value; }
        }

        private bool isNotNull = true;
        /// <summary>
        /// 是否非空
        /// </summary>
        public bool IsNotNull
        {
            get { return isNotNull; }
            set { isNotNull = value; }
        }

        private string defaultValue = string.Empty;
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        private bool isIndex = false;
        /// <summary>
        /// 是否索引
        /// </summary>
        public bool IsIndex
        {
            get { return isIndex; }
            set { isIndex = value; }
        }

        private bool isUnique = false;
        /// <summary>
        /// 是否唯一
        /// </summary>
        public bool IsUnique
        {
            get { return isUnique; }
            set { isUnique = value; }
        }

        private string keyOrIndexName = string.Empty;
        /// <summary>
        /// 主键或索引名称
        /// </summary>
        public string KeyOrIndexName
        {
            get { return keyOrIndexName; }
            set { keyOrIndexName = value; }
        }

        private string keyOrIndexColumns = string.Empty;
        /// <summary>
        /// 主键或索引字段名称(1、如果为空，则使用ColumnName 2、如果多个ColumnName，则用逗号分隔)
        /// </summary>
        public string KeyOrIndexColumns
        {
            get { return keyOrIndexColumns; }
            set { keyOrIndexColumns = value; }
        }

        /// <summary>
        /// 表单类型编码
        /// </summary>
        public string ControlCode { get; set; }

        private string initialValue = string.Empty;
        /// <summary>
        /// 初始值
        ///对于select、Checkbox：多个项目用逗号分隔，默认项目末尾加 :default
        ///对于联动类型，填写CodeSetCode
        /// </summary>
        public string InitialValue
        {
            get { return initialValue; }
            set { initialValue = value; }
        }

        private bool enableInput = true;
        /// <summary>
        /// 录入项(是否允许录入)
        /// </summary>
        public bool EnableInput
        {
            get { return enableInput; }
            set { enableInput = value; }
        }

        private bool enableEdit = true;
        /// <summary>
        /// 修改项(是否允许修改)
        /// </summary>
        public bool EnableEdit
        {
            get { return enableEdit; }
            set { enableEdit = value; }
        }

        private string validateRole = string.Empty;
        /// <summary>
        /// 验证规则（例如：Email、URL）
        /// </summary>
        public string ValidateRole
        {
            get { return validateRole; }
            set { validateRole = value; }
        }


        #region 导航属性

        /// <summary>
        /// 内容模型
        /// </summary>
        public ContentTypeDefinition ContentType
        {
            get { return new MetadataService().GetContentType(this.ContentTypeId); }
        }

        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion


        #region IEntity 成员

        object IEntity.EntityId { get { return this.ColumnId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

    }
}
