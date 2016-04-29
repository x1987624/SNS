//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using Tunynet;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// PageId与Title的查询器
    /// </summary>
    public abstract class PageIdToTitleDictionary
    {
        private static ConcurrentDictionary<long, string> dictionaryOfPageIdToTitle = new ConcurrentDictionary<long, string>();
        private static ConcurrentDictionary<string, long> dictionaryOfTitleToPageId = new ConcurrentDictionary<string, long>();

        #region Instance

        private static volatile PageIdToTitleDictionary _defaultInstance = null;
        private static readonly object lockObject = new object();

        /// <summary>
        /// 获取PageIdToTitleDictionary实例
        /// </summary>
        /// <returns></returns>
        private static PageIdToTitleDictionary Instance()
        {
            if (_defaultInstance == null)
            {
                lock (lockObject)
                {
                    if (_defaultInstance == null)
                    {
                        _defaultInstance = DIContainer.Resolve<PageIdToTitleDictionary>();
                        if (_defaultInstance == null)
                            throw new ExceptionFacade("未在DIContainer注册PageIdToTitleDictionary的具体实现类");
                    }
                }
            }
            return _defaultInstance;
        }

        #endregion

        /// <summary>
        /// 根据词条Id获取词条名
        /// </summary>
        /// <returns>
        /// 词条名
        /// </returns>
        protected abstract string GetTitleByPageId(long pageId);

        /// <summary>
        /// 根据词条名获取词条Id
        /// </summary>
        /// <returns>
        /// 词条Id
        /// </returns>
        protected abstract long GetPageIdByTitle(string title);


        /// <summary>
        /// 通过pageId获取title
        /// </summary>
        /// <param name="PageId">PageId</param>
        public static string GetTitle(long pageId)
        {
            if (dictionaryOfPageIdToTitle.ContainsKey(pageId))
                return dictionaryOfPageIdToTitle[pageId];
            string title = Instance().GetTitleByPageId(pageId);
            if (!string.IsNullOrEmpty(title))
            {
                dictionaryOfPageIdToTitle[pageId] = title;
                if (!dictionaryOfTitleToPageId.ContainsKey(title))
                    dictionaryOfTitleToPageId[title] = pageId;
                return title;
            }
            return string.Empty;
        }

        /// <summary>
        /// 通过title获取pageId
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        public static long GetPageId(string title)
        {
            if (dictionaryOfTitleToPageId.ContainsKey(title))
                return dictionaryOfTitleToPageId[title];
            long pageId = Instance().GetPageIdByTitle(title);
            if (pageId > 0)
            {
                dictionaryOfTitleToPageId[title] = pageId;
                if (!dictionaryOfPageIdToTitle.ContainsKey(pageId))
                    dictionaryOfPageIdToTitle[pageId] = title;
            }
            return pageId;
        }

        /// <summary>
        /// 移除PageId
        /// </summary>
        /// <param name="pageId">pageId</param>
        internal static void RemovePageId(long pageId)
        {
            string title;
            dictionaryOfPageIdToTitle.TryRemove(pageId, out title);
        }

        /// <summary>
        /// 移除Title
        /// </summary>
        /// <param name="title">title</param>
        internal static void RemoveTitle(string title)
        {
            long pageId;
            dictionaryOfTitleToPageId.TryRemove(title, out pageId);
        }
    }
}