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
    /// 内容模型定义
    /// </summary>
    [TableName("spb_cms_ContentTypeDefinitions")]
    [PrimaryKey("ContentTypeId", autoIncrement = true)]
    public class ContentTypeDefinition : IEntity
    {
        /// <summary>
        /// ContentTypeId
        /// </summary>
        public int ContentTypeId { get; set; }

        /// <summary>
        /// 模型名称
        /// </summary>
        public string ContentTypeName { get; set; }

        /// <summary>
        /// 模型英文标识
        /// </summary>
        public string ContentTypeKey { get; set; }

        /// <summary>
        /// 是不是内建模型
        /// </summary>
        public bool IsBuiltIn { get; set; }

        private int displayOrder = 100;
        /// <summary>
        /// 排序序号
        /// </summary>
        public int DisplayOrder
        {
            get { return displayOrder; }
            set { displayOrder = value; }
        }

        /// <summary>
        /// 数据库表名
        /// </summary>
        public string TableName { get; set; }

        private string foreignKey = "ContentItemId";
        /// <summary>
        /// 附表与主表关联的字段名称
        /// </summary>
        public string ForeignKey
        {
            get { return foreignKey; }
            set { foreignKey = value; }
        }

        /// <summary>
        /// 发布页面
        /// </summary>
        public string Page_New { get; set; }

        /// <summary>
        /// 修改页面
        /// </summary>
        public string Page_Edit { get; set; }

        /// <summary>
        /// 列表管理页面
        /// </summary>
        public string Page_Manage { get; set; }

        /// <summary>
        /// 默认列表页面
        /// </summary>
        public string Page_Default_List { get; set; }

        /// <summary>
        /// 默认详细显示页面
        /// </summary>
        public string Page_Default_Detail { get; set; }

        private bool isEnabled = false;
        /// <summary>
        ///  是否启用
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        private bool enableContribute = false;
        /// <summary>
        /// 是否允许用户投稿
        /// </summary>
        public bool EnableContribute
        {
            get { return enableContribute; }
            set { enableContribute = value; }
        }

        private bool enableComment = false;
        /// <summary>
        /// 是否启用评论
        /// </summary>
        public bool EnableComment
        {
            get { return enableComment; }
            set { enableComment = value; }
        }

        private bool enableAttachment = false;
        /// <summary>
        /// 是否启用附件
        /// </summary>
        public bool EnableAttachment
        {
            get { return enableAttachment; }
            set { enableAttachment = value; }
        }

        private string allowContributeRoleNames = string.Empty;
        /// <summary>
        /// 允许投稿的角色名集合，多个角色名用英文逗号隔开
        /// </summary>
        public string AllowContributeRoleNames
        {
            get { return allowContributeRoleNames; }
            set { allowContributeRoleNames = value; }
        }



        #region 导航属性

        /// <summary>
        /// 所有字段
        /// </summary>
        public IEnumerable<ContentTypeColumnDefinition> Columns
        {
            get { return new MetadataService().GetColumnsByContentTypeId(this.ContentTypeId); }
        }

        #endregion


        #region IEntity 成员

        object IEntity.EntityId { get { return this.ContentTypeId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion

    }
}
