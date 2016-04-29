//<TunynetCopyright>
//--------------------------------------------------------------
//<version>V0.5</verion>
//<createdate>2012-10-29</createdate>
//<author>wanf</author>
//<email>wanf@tunynet.com</email>
//<log date="2012-10-29" version="0.5">创建</log>
//--------------------------------------------------------------
//</TunynetCopyright>
using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Spacebuilder.Common;
using Spacebuilder.Search;
using Tunynet;
using Tunynet.Common;
using Tunynet.Search;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 日志搜索器
    /// </summary>
    public class WikiSearcher : ISearcher
    {
        private WikiService wikiService = new WikiService();
        private TagService tagService = new TagService(TenantTypeIds.Instance().Wiki());
        private CategoryService categoryService = new CategoryService();
        private AuditService auditService = new AuditService();
        private ISearchEngine searchEngine;
        private PubliclyAuditStatus publiclyAuditStatus;
        private SearchedTermService searchedTermService = new SearchedTermService();
        public static string CODE = "WikiSearcher";
        public static string WATERMARK = "搜索百科";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">Searcher名称</param>
        /// <param name="indexPath">索引文件所在路径（支持"~/"及unc路径）</param>
        /// <param name="asQuickSearch">是否作为快捷搜索</param>
        /// <param name="displayOrder">显示顺序</param>
        public WikiSearcher(string name, string indexPath, bool asQuickSearch, int displayOrder)
        {
            this.Name = name;
            this.IndexPath = Tunynet.Utilities.WebUtility.GetPhysicalFilePath(indexPath);
            this.AsQuickSearch = asQuickSearch;
            this.DisplayOrder = displayOrder;
            searchEngine = SearcherFactory.GetSearchEngine(indexPath);
            publiclyAuditStatus = auditService.GetPubliclyAuditStatus(WikiConfig.Instance().ApplicationId);
        }

        #region 搜索器属性

        /// <summary>
        /// 是否作为快捷搜索
        /// </summary>
        public bool AsQuickSearch { get; private set; }

        public string WaterMark { get { return WATERMARK; } }

        /// <summary>
        /// 关联的搜索引擎实例
        /// </summary>
        public ISearchEngine SearchEngine
        {
            get
            {
                return searchEngine;
            }
        }

        /// <summary>
        /// 搜索器的唯一标识
        /// </summary>
        public string Code { get { return CODE; } }

        /// <summary>
        /// 是否前台显示
        /// </summary>
        public bool IsDisplay
        {
            get { return true; }
        }

        /// <summary>
        /// 显示顺序
        /// </summary>
        public int DisplayOrder { get; private set; }

        /// <summary>
        /// 处理全局搜索的URL
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns></returns>
        public string GlobalSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance().WikiGlobalSearch() + "?keyword=" + keyword;
        }

        /// <summary>
        /// Lucene索引路径（完整物理路径，支持unc）
        /// </summary>
        public string IndexPath { get; private set; }

        /// <summary>
        /// 是否基于Lucene实现
        /// </summary>
        public bool IsBaseOnLucene
        {
            get { return true; }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 处理快捷搜索的URL
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns></returns>
        public string QuickSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance().WikiQuickSearch() + "?keyword=" + keyword;
        }

        /// <summary>
        /// 处理当前应用搜索的URL
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns></returns>
        public string PageSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance().WikiPageSearch(keyword);
        }

        #endregion

        #region 索引内容维护


        /// <summary>
        /// 重建索引
        /// </summary>  
        public void RebuildIndex()
        {
            //pageSize参数决定了每次批量取多少条数据进行索引。要注意的是，如果是分布式搜索，客户端会将这部分数据通过WCF传递给服务器端，而WCF默认的最大传输数据量是65535B，pageSize较大时这个设置显然是不够用的，WCF会报400错误；系统现在将最大传输量放宽了，但仍要注意一次不要传输过多，如遇异常，可适当调小pageSize的值
            int pageSize = 1000;
            int pageIndex = 1;
            long totalRecords = 0;
            bool isBeginning = true;
            bool isEndding = false;
            do
            {
                //分页获取帖子列表
                PagingDataSet<WikiPage> WikiPages = wikiService.GetsForAdmin(null, null, null, null, null, null, pageSize, pageIndex);
                totalRecords = WikiPages.TotalRecords;

                isEndding = (pageSize * pageIndex < totalRecords) ? false : true;

                //重建索引
                List<WikiPage> WikiPageList = WikiPages.ToList<WikiPage>();

                IEnumerable<Document> docs = WikiIndexDocument.Convert(WikiPageList);

                searchEngine.RebuildIndex(docs, isBeginning, isEndding);

                isBeginning = false;
                pageIndex++;
            }
            while (!isEndding);
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="WikiPage">待添加的日志</param>
        public void Insert(WikiPage WikiPage)
        {
            Insert(new WikiPage[] { WikiPage });
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="WikiPages">待添加的日志</param>
        public void Insert(IEnumerable<WikiPage> WikiPages)
        {
            IEnumerable<Document> docs = WikiIndexDocument.Convert(WikiPages);
            searchEngine.Insert(docs);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="WikiPageId">待删除的日志Id</param>
        public void Delete(long WikiPageId)
        {
            searchEngine.Delete(WikiPageId.ToString(), WikiIndexDocument.PageId);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="WikiPageIds">待删除的日志Id列表</param>
        public void Delete(IEnumerable<long> WikiPageIds)
        {
            foreach (var WikiPageId in WikiPageIds)
            {
                searchEngine.Delete(WikiPageId.ToString(), WikiIndexDocument.PageId);
            }
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="WikiPage">待更新的日志</param>
        public void Update(WikiPage WikiPage)
        {
            Document doc = WikiIndexDocument.Convert(WikiPage);
            searchEngine.Update(doc, WikiPage.PageId.ToString(), WikiIndexDocument.PageId);
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="WikiPages">待更新的日志集合</param>
        public void Update(IEnumerable<WikiPage> WikiPages)
        {
            IEnumerable<Document> docs = WikiIndexDocument.Convert(WikiPages);
            IEnumerable<string> WikiPageIds = WikiPages.Select(n => n.PageId.ToString());
            searchEngine.Update(docs, WikiPageIds, WikiIndexDocument.PageId);
        }

        #endregion

        #region 搜索
        /// <summary>
        /// 日志分页搜索
        /// </summary>
        /// <param name="WikiQuery">搜索条件</param>
        /// <returns>符合搜索条件的分页集合</returns>
        public PagingDataSet<WikiPage> Search(WikiFullTextQuery WikiQuery)
        {
            if (WikiQuery.SiteCategoryId == 0)
            {
                if (string.IsNullOrWhiteSpace(WikiQuery.Keyword))
                {
                    return new PagingDataSet<WikiPage>(new List<WikiPage>());
                }
            }

            LuceneSearchBuilder searchBuilder = BuildLuceneSearchBuilder(WikiQuery);

            //使用LuceneSearchBuilder构建Lucene需要Query、Filter、Sort
            Query query = null;
            Filter filter = null;
            Sort sort = null;
            searchBuilder.BuildQuery(out query, out filter, out sort);

            //调用SearchService.Search(),执行搜索
            PagingDataSet<Document> searchResult = searchEngine.Search(query, filter, sort, WikiQuery.PageIndex, WikiQuery.PageSize);
            IEnumerable<Document> docs = searchResult.ToList<Document>();

            //解析出搜索结果中的日志ID
            List<long> WikiPageIds = new List<long>();
            //获取索引中百科的标签
            Dictionary<long, IEnumerable<string>> WikiTags = new Dictionary<long, IEnumerable<string>>();

            foreach (Document doc in docs)
            {
                long WikiPageId = long.Parse(doc.Get(WikiIndexDocument.PageId));
                WikiPageIds.Add(WikiPageId);
                WikiTags[WikiPageId] = doc.GetValues(WikiIndexDocument.Tag).ToList<string>();
            }

            //根据日志ID列表批量查询日志实例
            IEnumerable<WikiPage> WikiPageList = wikiService.GetWikiPages(WikiPageIds);

            foreach (var WikiPage in WikiPageList)
            {
                if (WikiTags.ContainsKey(WikiPage.PageId))
                {
                    WikiPage.TagNames = WikiTags[WikiPage.PageId];
                }
            }

            //组装分页对象
            PagingDataSet<WikiPage> WikiPages = new PagingDataSet<WikiPage>(WikiPageList)
            {
                TotalRecords = searchResult.TotalRecords,
                PageSize = searchResult.PageSize,
                PageIndex = searchResult.PageIndex,
                QueryDuration = searchResult.QueryDuration
            };

            return WikiPages;
        }

        /// <summary>
        /// 获取匹配的前几条日志热词
        /// </summary>
        /// <param name="keyword">要匹配的关键字</param>
        /// <param name="topNumber">前N条</param>
        /// <returns>符合搜索条件的分页集合</returns>
        public IEnumerable<string> AutoCompleteSearch(string keyword, int topNumber)
        {
            IEnumerable<SearchedTerm> searchedAdminTerms = searchedTermService.GetManuals(keyword, CODE);
            IEnumerable<SearchedTerm> searchedUserTerms = searchedTermService.GetTops(keyword, topNumber, CODE);
            IEnumerable<SearchedTerm> listSearchAdminUserTerms = searchedAdminTerms.Union(searchedUserTerms);
            if (listSearchAdminUserTerms.Count() > topNumber)
            {
                listSearchAdminUserTerms.Take(topNumber);
            }
            return listSearchAdminUserTerms.Select(t => t.Term);
        }

        /// <summary>
        /// 根据帖吧搜索查询条件构建Lucene查询条件
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        private LuceneSearchBuilder BuildLuceneSearchBuilder(WikiFullTextQuery WikiQuery)
        {
            LuceneSearchBuilder searchBuilder = new LuceneSearchBuilder();
            //范围
            Dictionary<string, BoostLevel> fieldNameAndBoosts = new Dictionary<string, BoostLevel>();

            if (!string.IsNullOrEmpty(WikiQuery.Keyword))
            {
                switch (WikiQuery.Range)
                {
                    case WikiSearchRange.Title:
                        searchBuilder.WithPhrase(WikiIndexDocument.Title, WikiQuery.Keyword, BoostLevel.Hight, false);
                        break;
                    case WikiSearchRange.Category:
                        searchBuilder.WithPhrase(WikiIndexDocument.CategoryId, WikiQuery.Keyword, BoostLevel.Hight, false);
                        break;
                    case WikiSearchRange.AUTHOR:
                        searchBuilder.WithPhrase(WikiIndexDocument.Author, WikiQuery.Keyword, BoostLevel.Hight, false);
                        break;
                    case WikiSearchRange.TAG:
                        searchBuilder.WithPhrase(WikiIndexDocument.Tag, WikiQuery.Keyword, BoostLevel.Hight, false);
                        break;
                    default:
                        fieldNameAndBoosts.Add(WikiIndexDocument.Title, BoostLevel.Hight);
                        fieldNameAndBoosts.Add(WikiIndexDocument.Tag, BoostLevel.Medium);
                        fieldNameAndBoosts.Add(WikiIndexDocument.Body, BoostLevel.Medium);
                        fieldNameAndBoosts.Add(WikiIndexDocument.Author, BoostLevel.Low);
                        fieldNameAndBoosts.Add(WikiIndexDocument.CategoryId, BoostLevel.Hight);
                        searchBuilder.WithPhrases(fieldNameAndBoosts, WikiQuery.Keyword, BooleanClause.Occur.SHOULD, false);
                        break;
                }
            }

            //某个站点分类
            if (WikiQuery.SiteCategoryId != 0)
            {
                searchBuilder.WithField(WikiIndexDocument.CategoryId, WikiQuery.SiteCategoryId.ToString(), true, BoostLevel.Hight, true);
            }

            return searchBuilder;
        }
        #endregion
    }
}