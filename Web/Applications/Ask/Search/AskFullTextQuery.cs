//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答全文检索
    /// </summary>
    public class AskFullTextQuery
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 关键字集合
        /// </summary>
        public IEnumerable<string> Keywords { get; set; }

        private bool isAlike = false;
        /// <summary>
        /// 是否相似问题
        /// </summary>
        public bool IsAlike
        {
            get { return isAlike;}
            set{ isAlike=value;}
        }

        private bool isRelation = false;
        /// <summary>
        /// 是否查相关问答
        /// </summary>
        public bool IsRelation
        {
            get { return isRelation; }
            set { isRelation = value; }
        }

        /// <summary>
        /// 排序
        /// </summary>
        public SortBy_AskQuestion? sortBy{get;set;}

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
        public AskSearchRange Range { get; set; }
    }

    public enum AskSearchRange
    {
        /// <summary>
        /// 全部
        /// </summary>
        ALL = 0,

        /// <summary>
        /// 标题
        /// </summary>
        SUBJECT = 1,

       
        /// <summary>
        /// 作者
        /// </summary>
        AUTHOR = 2,

        /// <summary>
        /// 标签
        /// </summary>
        TAG = 3,

        /// <summary>
        /// 回答
        /// </summary>
        ANSWER = 4
    }

}