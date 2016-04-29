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
using Tunynet.Common;

namespace Spacebuilder.CMS
{
    /// <summary>
    /// ContentItem查询条件封装
    /// </summary>
    public class ContentItemQuery : IListCacheSetting
    {
        public ContentItemQuery(CacheVersionType cacheVersionType)
        {
            this.cacheVersionType = cacheVersionType;
            this.SortBy = ContentItemSortBy.ReleaseDate_Desc;
        }

        /// <summary>
        /// ContentFolderId
        /// </summary>
        public int? ContentFolderId { get; set; }

        /// <summary>
        /// 是否包含ContentFolderId的后代
        /// </summary>
        public bool? IncludeFolderDescendants { get; set; }

        /// <summary>
        /// ContentTypeId
        /// </summary>
        public int? ContentTypeId { get; set; }

        /// <summary>
        /// UserId
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// 管理员用户Id
        /// </summary>
        public long? ModeratorUserId { get; set; }

        /// <summary>
        /// 最小时间
        /// </summary>
        public DateTime? MinDate { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public DateTime? MaxDate { get; set; }

        /// <summary>
        /// 是否用户投稿
        /// </summary>
        public bool? IsContributed { get; set; }

        /// <summary>
        /// 是否精华
        /// </summary>
        public bool? IsEssential { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool? IsSticky { get; set; }

        /// <summary>
        /// 标题关键词
        /// </summary>
        public string SubjectKeyword { get; set; }

        /// <summary>
        /// 标签关键词
        /// </summary>
        public string TagNameKeyword { get; set; }

        /// <summary>
        /// 标签关键词
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public PubliclyAuditStatus? PubliclyAuditStatus { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public ContentItemSortBy SortBy { get; set; }


        #region IListCacheSetting 成员

        private CacheVersionType cacheVersionType = CacheVersionType.None;
        /// <summary>
        /// 列表缓存版本设置
        /// </summary>
        CacheVersionType IListCacheSetting.CacheVersionType
        {
            get { return cacheVersionType; }
        }

        private string areaCachePropertyName = null;
        /// <summary>
        /// 缓存分区字段名称
        /// </summary>
        public string AreaCachePropertyName
        {
            get { return areaCachePropertyName; }
            set { areaCachePropertyName = value; }
        }

        private object areaCachePropertyValue = null;
        /// <summary>
        /// 缓存分区字段值
        /// </summary>
        public object AreaCachePropertyValue
        {
            get { return areaCachePropertyValue; }
            set { areaCachePropertyValue = value; }
        }

        #endregion
    }
}
