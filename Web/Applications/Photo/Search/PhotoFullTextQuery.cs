//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 照片全文检索的条件
    /// </summary>
    public class PhotoFullTextQuery
    {
        /// <summary>
        /// 租户类型
        /// </summary>
        public string TenantTypeId { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 筛选
        /// </summary>
        public PhotoSearchRange Filter { get; set; }


        private bool ignoreAuditAndPrivacy = false;
        /// <summary>
        /// 是否忽略审核状态和隐私状态
        /// </summary>
        public bool IgnoreAuditAndPrivacy
        {
            get { return ignoreAuditAndPrivacy; }
            set { ignoreAuditAndPrivacy = value; }
        }

        private long userId = 0;
        /// <summary>
        /// 筛选XXX的相册
        /// </summary>
        public long UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        
        private int pageIndex = 1;
        /// <summary>
        /// 当前显示页面页码
        /// </summary>
        public int PageIndex
        {
            get
            {
                if (pageIndex < 1)
                    return 1;
                else
                    return pageIndex;
            }
            set { pageIndex = value; }
        }

        /// <summary>
        /// 每页显示记录数
        /// </summary>
        public int PageSize = 10;
    }

    /// <summary>
    ///筛选枚举 
    /// </summary>
    public enum PhotoSearchRange
    {
        /// <summary>
        /// 全部
        /// </summary>
        ALL = 0,

        /// <summary>
        /// 描述
        /// </summary>
        DESCRIPTION = 1,

        /// <summary>
        /// 标签
        /// </summary>
        TAG = 2,

        /// <summary>
        /// 作者
        /// </summary>
        AUTHOR = 3
    }
}