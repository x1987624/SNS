//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spacebuilder.Search;
using Tunynet.Common;
using Tunynet.Search;
using Spacebuilder.Common;
using Tunynet;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Tunynet.Utilities;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答搜索器
    /// </summary>
    public class AskSearcher : ISearcher
    {
        private AskService askService = new AskService();
        private AuditService auditService = new AuditService();
        private ISearchEngine searchEngine;
        private PubliclyAuditStatus publiclyAuditStatus;
        private SearchedTermService searchedTermService = new SearchedTermService();
        public static string CODE = "AskSearcher";
        public static string WATERMARK = "搜索问题、回答";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">Searcher名称</param>
        /// <param name="indexPath">索引文件所在路径（支持"~/"及unc路径）</param>
        /// <param name="asQuickSearch">是否作为快捷搜索</param>
        /// <param name="displayOrder">显示顺序</param>
        public AskSearcher(string name, string indexPath, bool asQuickSearch, int displayOrder)
        {
            this.Name = name;
            this.IndexPath = WebUtility.GetPhysicalFilePath(indexPath);
            this.AsQuickSearch = asQuickSearch;
            this.DisplayOrder = displayOrder;
            searchEngine = SearcherFactory.GetSearchEngine(indexPath);
            publiclyAuditStatus = auditService.GetPubliclyAuditStatus(AskConfig.Instance().ApplicationId);
        }

        #region 搜索器属性

        /// <summary>
        /// 是否作为快捷搜索
        /// </summary>
        public bool AsQuickSearch { get; private set; }

        /// <summary>
        /// 搜索器水印
        /// </summary>
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
        /// <returns>全局搜索URL</returns>
        public string GlobalSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance()._AskGlobalSearch() + "?keyword=" + keyword;
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
        /// <returns>快捷搜索URL</returns>
        public string QuickSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance()._AskQuickSearch() + "?keyword=" + keyword;
        }

        /// <summary>
        /// 处理当前应用搜索的URL
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns>URL</returns>
        public string PageSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance().AskPageSearch(keyword);
        }

        #endregion

        #region 索引内容维护

        /// <summary>
        /// 重建索引
        /// </summary>  
        public void RebuildIndex()
        {
            int pageSize = 1000;
            int pageIndex = 1;
            long totalRecords = 0;
            bool isBeginning = true;
            bool isEndding = false;
            do
            {
                PagingDataSet<AskQuestion> askQuestions = askService.GetQuestionsForAdmin(null,null,null,null,null,null,null,pageSize,pageIndex);
                totalRecords = askQuestions.TotalRecords;
                isEndding = (pageSize * pageIndex < totalRecords) ? false : true;

                //重建索引
                List<AskQuestion> askQuestionList = askQuestions.ToList<AskQuestion>();
                IEnumerable<Document> docs = AskIndexDocument.Convert(askQuestionList);
                searchEngine.RebuildIndex(docs, isBeginning, isEndding);
                isBeginning = false;
                pageIndex++;
            } while (!isEndding);
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="question">待添加的问答</param>
        public void Insert(AskQuestion question)
        {
            Insert(new AskQuestion[] { question });

        }

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="askQuestions">待添加的问答</param>
        public void Insert(IEnumerable<AskQuestion> askQuestions)
        {
            IEnumerable<Document> docs = AskIndexDocument.Convert(askQuestions);
            searchEngine.Insert(docs);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="questionId">待删除的问题Id</param>
        public void Delete(long questionId)
        {
            searchEngine.Delete(questionId.ToString(), AskIndexDocument.QuestionId);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="questionIds">待删除的问答Id列表</param>
        public void Delete(IEnumerable<long> questionIds)
        {
            foreach (var questionId in questionIds)
            {
                searchEngine.Delete(questionId.ToString(), AskIndexDocument.QuestionId);
            }
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="question">待更新的问答</param>
        public void Update(AskQuestion question)
        {

            Update(new AskQuestion[] { question });
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="questions">待更新的日志集合</param>
        public void Update(IEnumerable<AskQuestion> questions)
        {
            IEnumerable<Document> docs = AskIndexDocument.Convert(questions);
            IEnumerable<string> questionIds = questions.Select(n => n.QuestionId.ToString());
            searchEngine.Update(docs, questionIds, AskIndexDocument.QuestionId);
        }

        #endregion

        #region 搜索

        /// <summary>
        /// 问答分页搜索
        /// </summary>
        /// <param name="askQuery">搜索条件</param>
        /// <returns>符合条件的分页集合</returns>
        public PagingDataSet<AskQuestion> Search(AskFullTextQuery askQuery)
        {
            if (string.IsNullOrWhiteSpace(askQuery.Keyword) && askQuery.IsRelation == false && askQuery.IsAlike==false)
            {
                return new PagingDataSet<AskQuestion>(new List<AskQuestion>());
            }
            LuceneSearchBuilder searchBuilder = BuilderLuceneSearchBuilder(askQuery);

            Query query = null;
            Filter filter = null;
            Sort sort = null;
            searchBuilder.BuildQuery(out query, out filter, out sort);

            //调用SearchService.Search(),执行搜索
            PagingDataSet<Document> searchResult = searchEngine.Search(query, filter, sort, askQuery.PageIndex, askQuery.PageSize);
            IEnumerable<Document> docs = searchResult.ToList<Document>();

            //问题ids
            List<long> questionIds = new List<long>();
            foreach (Document doc in docs)
            {
                long questionId = long.Parse(doc.Get(AskIndexDocument.QuestionId));
                questionIds.Add(questionId);
            }

            //根据ids批量获取问题实体
            IEnumerable<AskQuestion> questionList = askService.GetQuestions(questionIds);

            //组装分页对象
            PagingDataSet<AskQuestion> questions = new PagingDataSet<AskQuestion>(questionList)
            {
                TotalRecords = searchResult.TotalRecords,
                PageIndex = searchResult.PageIndex,
                PageSize = searchResult.PageSize,
                QueryDuration = searchResult.QueryDuration
            };
            return questions;
        }

        /// <summary>
        /// 获取匹配的前topNumber条问题热词
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="topNumber">前N条</param>
        /// <returns>符合条件的集合</returns>
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
        /// 构建lucene查询条件
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private LuceneSearchBuilder BuilderLuceneSearchBuilder(AskFullTextQuery query)
        {
            LuceneSearchBuilder searchBuilder = new LuceneSearchBuilder();
            Dictionary<string, BoostLevel> fieldNameAndBoosts = new Dictionary<string, BoostLevel>();

            if (query.IsAlike)
            {
                searchBuilder.WithPhrases(AskIndexDocument.Subject, query.Keywords, BoostLevel.Hight, false);
            }else if (query.IsRelation)
            {
                fieldNameAndBoosts.Add(AskIndexDocument.Subject, BoostLevel.Hight);
                fieldNameAndBoosts.Add(AskIndexDocument.Tag, BoostLevel.Hight);
                searchBuilder.WithPhrases(fieldNameAndBoosts, query.Keywords, BooleanClause.Occur.SHOULD, false);
            }
            else
            {
                //筛选
                switch (query.Range)
                {
                    case AskSearchRange.SUBJECT:
                        fieldNameAndBoosts.Add(AskIndexDocument.Subject,BoostLevel.Hight);
                        fieldNameAndBoosts.Add(AskIndexDocument.Body,BoostLevel.Medium);
                        searchBuilder.WithPhrases(fieldNameAndBoosts, query.Keyword, BooleanClause.Occur.SHOULD, false);
                        
                        break;
                    case AskSearchRange.TAG:
                        searchBuilder.WithPhrase(AskIndexDocument.Tag, query.Keyword, BoostLevel.Hight, false);
                        break;
                    case AskSearchRange.AUTHOR:
                        searchBuilder.WithPhrase(AskIndexDocument.Author, query.Keyword, BoostLevel.Hight, false);
                        break;
                    case AskSearchRange.ANSWER:
                        searchBuilder.WithPhrase(AskIndexDocument.Answer, query.Keyword, BoostLevel.Hight, false);
                        break;
                    default:
                        fieldNameAndBoosts.Add(AskIndexDocument.Subject, BoostLevel.Hight);
                        fieldNameAndBoosts.Add(AskIndexDocument.Tag, BoostLevel.Hight);
                        fieldNameAndBoosts.Add(AskIndexDocument.Body, BoostLevel.Medium);
                        fieldNameAndBoosts.Add(AskIndexDocument.Answer, BoostLevel.Medium);
                        fieldNameAndBoosts.Add(AskIndexDocument.Author, BoostLevel.Low);
                        searchBuilder.WithPhrases(fieldNameAndBoosts, query.Keyword, BooleanClause.Occur.SHOULD, false);
                        break;
                }
            }

            //过滤可以显示的问题
            switch (publiclyAuditStatus)
            {
                case PubliclyAuditStatus.Again:
                case PubliclyAuditStatus.Fail:
                case PubliclyAuditStatus.Pending:
                case PubliclyAuditStatus.Success:
                    searchBuilder.WithField(AskIndexDocument.AuditStatus, ((int)publiclyAuditStatus).ToString(), true, BoostLevel.Hight, true);
                    break;
                case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                    searchBuilder.WithinRange(AskIndexDocument.AuditStatus, ((int)publiclyAuditStatus).ToString(), ((int)PubliclyAuditStatus.Success).ToString(), true);
                    break;
            }
            if (query.sortBy == SortBy_AskQuestion.DateCreated_Desc)
            {
                searchBuilder.SortByString(AskIndexDocument.DateCreated, true);
            }
            
            return searchBuilder;
        }

        #endregion
    }
}