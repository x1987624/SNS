////------------------------------------------------------------------------------
//// <copyright company="Tunynet">
////     Copyright (c) Tunynet Inc.  All rights reserved.
//// </copyright> 
////------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科全文检索
    /// </summary>
    public class WikiFullTextQuery
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 关键字集合
        /// </summary>
        public IEnumerable<string> Keywords { get; set; }     

        /// <summary>
        /// 站点分类ID
        /// </summary>
        public long SiteCategoryId { get; set; }

        /// <summary>
        /// 当前显示页面页码
        /// </summary>
        private int pageIndex = 1;
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

        /// <summary>
        /// 筛选
        /// </summary>
        public WikiSearchRange Range { get; set; }
    }

    public enum WikiSearchRange
    {
        /// <summary>
        /// 全部
        /// </summary>
        ALL = 0,

        /// <summary>
        /// 标题
        /// </summary>
        Title = 1,

        /// <summary>
        /// 分类
        /// </summary>
        Category = 2,

        /// <summary>
        /// 标签
        /// </summary>
        TAG = 3,

        /// <summary>
        /// 作者
        /// </summary>
        AUTHOR = 4,

        /// <summary>
        /// 内容
        /// </summary>
        Body = 5
    }
}