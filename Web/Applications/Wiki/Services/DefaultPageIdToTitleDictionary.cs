//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 通过词条数据仓储实现查询
    /// </summary>
    public class DefaultPageIdToTitleDictionary : PageIdToTitleDictionary
    {
        private IWikiPageRepository wikiPageRepository;
        /// <summary>
        /// 构造器
        /// </summary>
        public DefaultPageIdToTitleDictionary()
            : this(new WikiPageRepository())
        {
        }

        /// <summary>
        /// 构造器
        /// </summary>
        public DefaultPageIdToTitleDictionary(IWikiPageRepository wikiPageRepository)
        {
            this.wikiPageRepository = wikiPageRepository;
        }

        /// <summary>
        /// 根据词条Id获取词条名
        /// </summary>
        /// <returns>
        /// 词条Id
        /// </returns>
        protected override string GetTitleByPageId(long pageId)
        {
            WikiPage wikiPage = wikiPageRepository.Get(pageId);
            if (wikiPage != null)
                return wikiPage.Title;
            return null;
        }

        /// <summary>
        /// 根据词条名获取词条Id
        /// </summary>
        /// <param name="title">词条</param>
        /// <returns>
        /// 词条Id
        /// </returns>
        protected override long GetPageIdByTitle(string title)
        {
            return wikiPageRepository.GetPageIdByTitle(title);
        }
    }
}
