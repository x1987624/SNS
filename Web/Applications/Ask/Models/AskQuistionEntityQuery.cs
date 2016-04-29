using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;

namespace Spacebuilder.Ask.Question
{
    public class AskQuistionEntityQuery
    {
        /// <summary>
        ///标签关键字
        ///</summary>
        string TagsKeywords{get;set;}
        ///<summary>
        ///作者ID
        /// </summary>
        public long? UserId { get; set; }
        ///<summary>
        ///审核状态
        /// </summary>
        public AuditStatus? AuditStatus { get; set; }
        /// <summary>
        /// 标题关键字
        /// </summary>
        public string TitleKeywords{get;set;}
    }
}