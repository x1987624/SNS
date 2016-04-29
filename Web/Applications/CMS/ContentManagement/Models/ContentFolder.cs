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
using Tunynet.Caching;
using Tunynet.Common;
using Spacebuilder.CMS.Metadata;
using Tunynet;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// 栏目
    /// </summary>
    [TableName("spb_cms_ContentFolders")]
    [PrimaryKey("ContentFolderId", autoIncrement = true)]
    [Serializable]
    [CacheSetting(true, ExpirationPolicy = EntityCacheExpirationPolicies.Stable)]
    public class ContentFolder : SerializablePropertiesBase, IEntity
    {
        /// <summary>
        /// ContentFolderId
        /// </summary>
        public int ContentFolderId { get; set; }

        private string folderName = string.Empty;
        /// <summary>
        /// 栏目名称
        /// </summary>
        public string FolderName
        {
            get { return folderName; }
            set { folderName = value; }
        }

        private string description = string.Empty;
        /// <summary>
        /// 栏目描述
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private bool isAllowDownload = true;
        /// <summary>
        /// 是否允许下载
        /// </summary>
        [Ignore]
        public bool IsAllowDownload
        {
            get { return isAllowDownload; }
            set { isAllowDownload = value; }
        }

        private int parentId;
        /// <summary>
        /// ParentId
        /// </summary>
        public int ParentId
        {
            get { return parentId; }
            set { parentId = value; }
        }

        private string parentIdList = string.Empty;
        /// <summary>
        /// 所有父级SectionId
        /// </summary>
        public string ParentIdList
        {
            get { return parentIdList; }
            set { parentIdList = value; }
        }

        private int childCount;
        /// <summary>
        /// 子栏目数目
        /// </summary>
        public int ChildCount
        {
            get { return childCount; }
            set { childCount = value; }
        }

        private int depth;
        /// <summary>
        /// 深度
        /// </summary>
        public int Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        private bool isEnabled = true;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        private int contentItemCount;
        /// <summary>
        /// 内容计数
        /// </summary>
        public int ContentItemCount
        {
            get { return contentItemCount; }
            set { contentItemCount = value; }
        }

        private DateTime dateCreated = DateTime.UtcNow;
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime DateCreated
        {
            get { return dateCreated; }
            set { dateCreated = value; }
        }

        private string contentTypeKeys = string.Empty;
        /// <summary>
        /// 内容模型Key集合(多个用英文逗号隔开)
        /// </summary>
        public string ContentTypeKeys
        {
            get { return contentTypeKeys; }
            set { contentTypeKeys = value; }
        }

        private int displayOrder;
        /// <summary>
        /// 排列顺序
        /// </summary>
        public int DisplayOrder
        {
            get { return displayOrder; }
            set { displayOrder = value; }
        }

        private bool enableContribute = true;
        /// <summary>
        /// 是否允许用户投稿
        /// </summary>
        public bool EnableContribute
        {
            get { return enableContribute; }
            set { enableContribute = value; }
        }

        private bool needAuditing;
        /// <summary>
        /// 栏目中的内容是否需要审核
        /// </summary>
        public bool NeedAuditing
        {
            get { return needAuditing; }
            set { needAuditing = value; }
        }


        private bool isAsNavigation = true;
        /// <summary>
        /// 是否作为导航显示
        /// </summary>
        public bool IsAsNavigation
        {
            get { return isAsNavigation; }
            set { isAsNavigation = value; }
        }

        private bool isLink;
        /// <summary>
        /// 是否外部链接
        /// </summary>
        public bool IsLink
        {
            get { return isLink; }
            set { isLink = value; }
        }

        private string linkUrl = string.Empty;
        /// <summary>
        /// 链接地址(支持 ~/)
        /// </summary>
        public string LinkUrl
        {
            get { return linkUrl; }
            set { linkUrl = value; }
        }

        private bool isLinkToNewWindow;
        /// <summary>
        /// 是否在新窗口打开链接
        /// </summary>
        public bool IsLinkToNewWindow
        {
            get { return isLinkToNewWindow; }
            set { isLinkToNewWindow = value; }
        }

        private string page_List = string.Empty;
        /// <summary>
        /// 默认内容列表页面
        /// </summary>
        public string Page_List
        {
            get { return page_List; }
            set { page_List = value; }
        }

        private string page_Detail = string.Empty;
        /// <summary>
        /// 默认详细显示页面
        /// </summary>
        public string Page_Detail
        {
            get { return page_Detail; }
            set { page_Detail = value; }
        }

        #region 扩展属性

        /// <summary>
        /// 编辑
        /// </summary>
        [Ignore]
        public string Editor
        {
            get
            {
                return GetExtendedProperty<string>("Editor");
            }

            set
            {
                SetExtendedProperty("Editor", value);
            }
        }


        /// <summary>
        /// 图标
        /// </summary>
        [Ignore]
        public string Icon
        {
            get
            {
                return GetExtendedProperty<string>("Icon");
            }

            set
            {
                SetExtendedProperty("Icon", value);
            }
        }

        /// <summary>
        /// meta标题
        /// </summary>
        [Ignore]
        public string METATitle
        {
            get
            {
                return GetExtendedProperty<string>("METATitle");
            }

            set
            {
                SetExtendedProperty("METATitle", value);
            }
        }
        /// <summary>
        /// meta关键词
        /// </summary>
        [Ignore]
        public string METAKeywords
        {
            get
            {
                return GetExtendedProperty<string>("METAKeywords");
            }

            set
            {
                SetExtendedProperty("METAKeywords", value);
            }
        }
        /// <summary>
        /// meta描述
        /// </summary>
        [Ignore]
        public string METADescription
        {
            get
            {
                return GetExtendedProperty<string>("METADescription");
            }

            set
            {
                SetExtendedProperty("METADescription", value);
            }
        }

        #endregion


        #region 导航属性

        /// <summary>
        /// 父栏目
        /// </summary>
        [Ignore]
        public ContentFolder Parent
        {
            get
            {
                if (this.ParentId > 0)
                    return new ContentFolderService().Get(this.ParentId);
                else
                    return new ContentFolder();
            }
        }

        /// <summary>
        /// 获取所有子栏目(非即时更新)
        /// </summary>
        [Ignore]
        public IEnumerable<ContentFolder> Children
        {
            get
            {
                if (ChildCount > 0)
                    return new ContentFolderService().GetChildren(this.ContentFolderId);
                else
                    return new List<ContentFolder>();
            }
        }

        /// <summary>
        /// 获取所有父级栏目(非即时更新)
        /// </summary>
        [Ignore]
        public IEnumerable<ContentFolder> Parents
        {
            get
            {
                List<ContentFolder> allParentContentFolders = new List<ContentFolder>();
                if (!string.IsNullOrEmpty(this.ParentIdList))
                {
                    foreach (var parentIdString in this.ParentIdList.Split(','))
                    {
                        int parentId = 0;
                        int.TryParse(parentIdString, out parentId);
                        if (parentId > 0)
                        {
                            ContentFolder parentFolder = new ContentFolderService().Get(parentId);
                            if (parentFolder != null)
                                allParentContentFolders.Add(parentFolder);
                        }
                    }
                }
                return allParentContentFolders;
            }
        }



        /// <summary>
        /// 获取当前栏目的所有管理员名称集合
        /// </summary>
        [Ignore]
        public IEnumerable<IUser> Moderators
        {
            get
            {
                return new ContentFolderModeratorService().GetModerators(ContentFolderId);
            }
        }

        /// <summary>
        /// 获取内容模型集合
        /// </summary>
        [Ignore]
        public IEnumerable<ContentTypeDefinition> ContentTypes
        {
            get
            {
                var keys = this.ContentTypeKeys.Split(',');
                return new MetadataService().GetContentTypes(true).Where(n => keys.Contains(n.ContentTypeKey));
            }
        }

        private int cumulateItemCount = 0;
        /// <summary>
        /// 累积内容项数量(包含所有后代ItemCount)
        /// </summary>
        [Ignore]
        public int CumulateItemCount
        {
            get
            {
                if (ChildCount > 0)
                {
                    cumulateItemCount = new ContentFolderService().GetCumulateItemCount(ContentFolderId);
                    return cumulateItemCount;
                }
                else
                {
                    return ContentItemCount;
                }
            }
        }

        #endregion


        #region IEntity 成员

        object IEntity.EntityId { get { return this.ContentFolderId; } }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion


    }
}
