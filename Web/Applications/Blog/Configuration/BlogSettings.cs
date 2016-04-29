//<TunynetCopyright>
//--------------------------------------------------------------
//<copyright>拓宇网络科技有限公司 2005-2012</copyright>
//<version>V0.5</verion>
//<createdate>2012-10-16</createdate>
//<author>jiangshl</author>
//<email>jiangshl@tunynet.com</email>
//<log date="2012-10-16" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>


using Tunynet;
using System;
using Tunynet.Caching;
namespace Spacebuilder.Blog
{
    /// <summary>
    /// 日志设置
    /// </summary>
    [Serializable]
    [CacheSetting(true)]
    public class BlogSettings:IEntity
    {
        private bool allowSetSiteCategory = true;
        /// <summary>
        /// 是否允许用户设置站点分类
        /// </summary>
        public bool AllowSetSiteCategory
        {
            get { return allowSetSiteCategory; }
            set { allowSetSiteCategory = value; }
        }

        private string recommendPicTypeId = "10020102";
        /// <summary>
        /// 图片日志推荐类型ID
        /// </summary>
        public string RecommendPicTypeId
        {
            get { return recommendPicTypeId; }
            set { recommendPicTypeId = value; }
        }

        private string recommendWordTypeId = "10020101";
        /// <summary>
        /// 文字日志推荐类型ID
        /// </summary>
        public string RecommendWordTypeId
        {
            get { return recommendWordTypeId; }
            set { recommendWordTypeId = value; }
        }

        private string recommendUserTypeId = "00001102";
        /// <summary>
        /// 推荐用户类型ID
        /// </summary>
        public string RecommendUserTypeId
        {
            get { return recommendUserTypeId; }
            set { recommendUserTypeId = value; }
        }

        private bool showSummaryInRss = false;
        /// <summary>
        /// 是否在Rss输出里只显示摘要
        /// </summary>
        public bool ShowSummaryInRss
        {
            get { return showSummaryInRss; }
            set { showSummaryInRss = value; }
        }

        private int rssPageSize = 30;
        /// <summary>
        /// Rss输出条数
        /// </summary>
        public int RssPageSize
        {
            get { return rssPageSize; }
            set { rssPageSize = value; }
        }

        #region IEntity 成员

        object IEntity.EntityId
        {
            get { return typeof(BlogSettings).FullName; }
        }

        bool IEntity.IsDeletedInDatabase { get; set; }

        #endregion
       
    }
}