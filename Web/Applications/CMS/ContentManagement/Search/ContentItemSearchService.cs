////<TunynetCopyright>
////--------------------------------------------------------------
////<copyright>tunynet inc. 2005-2013</copyright>
////<version>V0.5</verion>
////<createdate>2010-10-17</createdate>
////<author>mazq</author>
////<email>mazq@tunynet.com</email>
////<log date="2010-10-17" version="0.5">创建</log>
////--------------------------------------------------------------
////</TunynetCopyright>

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Spacebuilder.CMS.Repositories;
//using Spacebuilder.CMS.Repositories;
//using Lucene.Net.Search;
//using Lucene.Net.Index;
//using Lucene.Net.QueryParsers;
//using Lucene.Net.Analysis;
//using PanGu;
//using Spacebuilder.CMS;
//using Lucene.Net.Store;
//using System.IO;

//namespace Spacebuilder.CMS.Search
//{
//    public static class ContentItemSearchService
//    {
//        /// <summary>
//        /// Lucene索引文件版本
//        /// </summary>
//        public readonly static Lucene.Net.Util.Version LuceneCurrentVersion = Lucene.Net.Util.Version.LUCENE_29;

//        //索引文件夹
//        private readonly static string IndexDirectory = "~/IndexFiles/ContentItems/";


//        public static PagingDataSet<ISearchHit> Search(int pageSize, int pageIndex, int? contentFolderId, int? contentTypeId, string[] searchFields, string keyword)
//        {
//            var query = new BooleanQuery();

//            List<Query> filters = new List<Query>();
//            if (contentFolderId.HasValue && contentFolderId.Value > 0)
//            {
//                var contentFolderFilter = new BooleanQuery();
//                Term contentFolderIdTerm = new Term(ContentItemIndexFields.ContentFolderId, contentFolderId.Value.ToString());
//                contentFolderFilter.Add(new TermQuery(contentFolderIdTerm), BooleanClause.Occur.SHOULD);

//                //所有后代栏目
//                IEnumerable<ContentFolder> descendantContentFolders = ContentFolderService.GetDescendants(contentFolderId.Value);
//                if (descendantContentFolders != null)
//                {
//                    foreach (var folderId in descendantContentFolders.Select(x => x.ContentFolderId))
//                    {
//                        contentFolderIdTerm = new Term(ContentItemIndexFields.ContentFolderId, folderId.ToString());
//                        contentFolderFilter.Add(new TermQuery(contentFolderIdTerm), BooleanClause.Occur.SHOULD);
//                    }
//                }
//                filters.Add(contentFolderFilter);
//            }
//            if (contentTypeId.HasValue)
//            {
//                Term contentTypeIdTerm = new Term(ContentItemIndexFields.ContentTypeId, contentTypeId.Value.ToString());
//                filters.Add(new TermQuery(contentTypeIdTerm));
//            }

//            Query postKeywordQuery = null;
//            keyword = LuceneKeywordsScrubber(keyword);
//            if (!string.IsNullOrEmpty(keyword))
//            {
//                MultiFieldQueryParser keywordQueryParser = new MultiFieldQueryParser(LuceneCurrentVersion, searchFields, GetChineseAnalyzerOfUnTokenized());
//                keywordQueryParser.SetLowercaseExpandedTerms(true);
//                keywordQueryParser.SetDefaultOperator(QueryParser.OR_OPERATOR);

//                string keyWordsOfSplit = SplitKeywordsBySpace(keyword);
//                postKeywordQuery = keywordQueryParser.Parse(keyWordsOfSplit);

//                query.Add(postKeywordQuery, BooleanClause.Occur.MUST);
//            }
//            else
//            {
//                //query.Add(new TermQuery(new Term(ContentItemIndexFields.ContentItemId, "0")), BooleanClause.Occur.MUST_NOT);
//                query.Add(new TermRangeQuery(ContentItemIndexFields.ContentItemId, "0", "9", true, true), BooleanClause.Occur.SHOULD);
//            }

//            Query finalQuery = query;
//            if (filters.Count > 0)
//            {
//                var filterQuery = new BooleanQuery();
//                foreach (var clause in filters)
//                    filterQuery.Add(clause, BooleanClause.Occur.MUST);

//                var filterQueryWrapper = new QueryWrapperFilter(filterQuery);

//                finalQuery = new FilteredQuery(query, filterQueryWrapper);
//            }

//            IEnumerable<ISearchHit> hitList;
//            int totalRecords = 0;
//            DateTime searchStartTime = DateTime.Now;
//            IndexSearcher searcher = null;
//            try
//            {
//                searcher = new IndexSearcher(GetLuceneDirectory(), true);
//                TopScoreDocCollector collector = TopScoreDocCollector.create(searcher.MaxDoc(), false);
//                searcher.Search(finalQuery, collector);
//                ScoreDoc[] hits = collector.TopDocs((pageIndex - 1) * pageSize, pageSize).scoreDocs;
//                hitList = hits.Select(hit => new LuceneSearchHit(searcher.Doc(hit.doc), hit.score)).ToList();
//                totalRecords = collector.GetTotalHits();
//            }
//            catch
//            {
//                hitList = Enumerable.Empty<ISearchHit>();
//            }
//            finally
//            {
//                if (searcher != null)
//                    searcher.Close();
//            }

//            return new PagingDataSet<ISearchHit>(hitList)
//            {
//                PageSize = pageSize,
//                PageIndex = pageIndex,
//                TotalRecords = totalRecords,
//                QueryDuration = (DateTime.Now.Ticks - searchStartTime.Ticks) / 1E7f
//            };
//        }

//        #region Help Methods


//        /// <summary>
//        /// 获取用于中文分词的Analyzer(且返回原始字符串)
//        /// </summary>
//        /// <returns>Analyzer</returns>
//        private static Analyzer GetChineseAnalyzerOfUnTokenized()
//        {
//            return new Lucene.Net.Analysis.PanGu.PanGuAnalyzer(true);
//        }

//        /// <summary>
//        /// 切分关键词(用空格分隔)
//        /// </summary>
//        /// <param name="keywords"></param>
//        /// <returns></returns>
//        private static string SplitKeywordsBySpace(string keywords)
//        {
//            StringBuilder result = new StringBuilder();
//            ICollection<WordInfo> words = new Lucene.Net.Analysis.PanGu.PanGuTokenizer().SegmentToWordInfos(keywords);
//            foreach (WordInfo word in words)
//            {
//                if (word == null)
//                    continue;

//                result.AppendFormat("{0}^{1}.0 ", word.Word, (int)Math.Pow(3, word.Rank));
//            }
//            string keyWords = result.ToString().Trim();
//            if (string.IsNullOrEmpty(keyWords))
//                return keywords;
//            else
//                return keyWords;
//        }

//        /// <summary>
//        /// 去除Lucene关键字
//        /// </summary>
//        public static string LuceneKeywordsScrubber(string str)
//        {
//            if (string.IsNullOrEmpty(str))
//                return str;

//            string[] LuceneKeywords = new string[] { @"\", "(", ")", ":", "^", "[", "]", "{", "}", "~", "*", "?", "!", "\"", "'" };
//            string[] arraySplit = str.Split(LuceneKeywords, StringSplitOptions.RemoveEmptyEntries);
//            return string.Join("", arraySplit);
//        }


//        /// <summary>
//        /// 获取Lucene索引文件目录
//        /// </summary>
//        /// <returns></returns>
//        private static Lucene.Net.Store.Directory GetLuceneDirectory()
//        {
//            var directoryInfo = new DirectoryInfo(WebUtils.GetPhysicalFilePath(IndexDirectory));
//            return FSDirectory.Open(directoryInfo);
//        }

//        #endregion


//    }
//}
