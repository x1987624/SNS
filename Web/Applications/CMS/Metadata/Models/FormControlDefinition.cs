//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Tunynet;

namespace Spacebuilder.CMS.Metadata
{
    /// <summary>
    /// 用于处理字段的表单控件
    /// </summary>
    [TableName("spb_cms_FormControlDefinitions")]
    [PrimaryKey("ControlCode", autoIncrement = false)]
    public class FormControlDefinition : IEntity
    {
        /// <summary>
        /// 控件编码
        /// </summary>
        public string ControlCode { get; set; }

        /// <summary>
        /// 控件名称
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }

        #region IEntity 成员

        object IEntity.EntityId { get { return this.ControlCode; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

    }
}
