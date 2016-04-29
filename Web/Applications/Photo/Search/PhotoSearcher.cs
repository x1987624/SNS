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
using Tunynet.Utilities;
using Spacebuilder.Common;
using Tunynet;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 照片搜索器
    /// </summary>
    public class PhotoSearcher : ISearcher
    {
        private PhotoService photoService = new PhotoService();
        private ISearchEngine searchEngine;
        private SearchedTermService searchedTermService = new SearchedTermService();
        private AuditService auditService = new AuditService();
        private PubliclyAuditStatus publiclyAuditStatus;

        /// <summary>
        /// 照片搜索code属性
        /// </summary>
        public static string CODE = "PhotoSearcher";

        /// <summary>
        /// 水印
        /// </summary>
        public static string WATERMARK = "搜索照片";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">Searcher名称</param>
        /// <param name="indexPath">索引文件所在路径（支持"~/"及unc路径）</param>
        /// <param name="asQuickSearch">是否作为快捷搜索</param>
        /// <param name="displayOrder">显示顺序</param>
        public PhotoSearcher(string name, string indexPath, bool asQuickSearch, int displayOrder)
        {
            this.Name = name;
            this.IndexPath = WebUtility.GetPhysicalFilePath(indexPath);
            this.AsQuickSearch = asQuickSearch;
            this.DisplayOrder = displayOrder;
            searchEngine = SearcherFactory.GetSearchEngine(indexPath);
            publiclyAuditStatus = auditService.GetPubliclyAuditStatus(PhotoConfig.Instance().ApplicationId);
        }

        #region 搜索器属性

        /// <summary>
        /// 搜索器的唯一标识
        /// </summary>
        public string Code
        {
            get { return CODE; }
        }

        /// <summary>
        /// 搜索器水印
        /// </summary>
        public string WaterMark
        {
            get { return WATERMARK; }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

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
        public int DisplayOrder
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否基于Lucene实现
        /// </summary>
        public bool IsBaseOnLucene
        {
            get { return true; }
        }

        /// <summary>
        /// Lucene索引路径（完整物理路径，支持unc）
        /// </summary>
        public string IndexPath
        {
            get;
            private set;
        }

        /// <summary>
        /// 是否作为快捷搜索
        /// </summary>
        public bool AsQuickSearch
        {
            get;
            private set;
        }

        /// <summary>
        /// 处理快捷搜索的URL
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns></returns>
        public string QuickSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance()._PhotoQuickSearch() + "?keyword=" + keyword;
        }

        /// <summary>
        /// 处理当前应用搜索的URL
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns></returns>
        public string PageSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance().PhotoPageSearch(keyword);
        }

        /// <summary>
        /// 处理全局搜索的URL
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns></returns>
        public string GlobalSearchActionUrl(string keyword)
        {
            return SiteUrls.Instance()._PhotoGlobalSearch() + "?keyword=" + keyword;
        }

        /// <summary>
        /// 关联的搜索引擎实例
        /// </summary>
        public ISearchEngine SearchEngine
        {
            get { return searchEngine; }
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
                PagingDataSet<Photo> photos = photoService.GetPhotosForAdmin(null,null,null,null,null,null, pageSize,pageIndex);
                totalRecords = photos.TotalRecords;
                isEndding = (pageSize * pageIndex < totalRecords) ? false : true;

                List<Photo> photoList = photos.ToList<Photo>();
                IEnumerable<Document> docs = PhotoIndexDocument.Convert(photoList);
                searchEngine.RebuildIndex(docs, isBeginning, isEndding);

                isBeginning = false;
                pageIndex++;
            } while (!isEndding);

        }

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="photo">待添加的索引</param>
        public void Insert(Photo photo)
        {
            Insert(new Photo[] { photo });
        }

        /// <summary>
        /// 批量添加索引
        /// </summary>
        /// <param name="photos">Photo集合</param>
        public void Insert(IEnumerable<Photo> photos)
        {
            IEnumerable<Document> docs = PhotoIndexDocument.Convert(photos);
            searchEngine.Insert(docs);
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="photo">要更新的Photo</param>
        public void Update(Photo photo)
        {
            Document doc = PhotoIndexDocument.Convert(photo);
            searchEngine.Update(doc, photo.PhotoId.ToString(), PhotoIndexDocument.PhotoId);
        }

        /// <summary>
        /// 批量更新索引
        /// </summary>
        /// <param name="photos">Photo集合</param>
        public void Update(IEnumerable<Photo> photos)
        {
            IEnumerable<Document> docs = PhotoIndexDocument.Convert(photos);
            IEnumerable<string> photoIds = photos.Select(n => n.PhotoId.ToString());
            searchEngine.Update(docs, photoIds, PhotoIndexDocument.PhotoId);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="photo">待删除的Photo</param>
        public void Delete(Photo photo)
        {
            searchEngine.Delete(photo.PhotoId.ToString(), PhotoIndexDocument.PhotoId);
        }

        /// <summary>
        /// 批量删除索引
        /// </summary>
        /// <param name="photos">Photo集合</param>
        public void Delete(IEnumerable<Photo> photos)
        {
            IEnumerable<string> photoIds = photos.Select(n => n.PhotoId.ToString());
            searchEngine.Delete(photoIds, PhotoIndexDocument.PhotoId);
        }

        #endregion

        #region 搜索

        /// <summary>
        /// 照片搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public PagingDataSet<Photo> Search(PhotoFullTextQuery query)
        {
            LuceneSearchBuilder searchBuilder = BuildLuceneSearchBuilder(query);

            //使用LuceneSearchBuilder构建Lucene需要Query、Filter、Sort
            Query photoQuery = null;
            Filter filter = null;
            Sort sort = null;
            searchBuilder.BuildQuery(out photoQuery, out filter, out sort);

            //调用SearchEngine.Search(),执行搜索
            PagingDataSet<Document> searchResult = searchEngine.Search(photoQuery, filter, sort, query.PageIndex, query.PageSize);
            IEnumerable<Document> docs = searchResult.ToList<Document>();

            //解析出PhotoId,组装Photo实体
            List<long> photoIds = new List<long>();
            foreach (var doc in docs)
            {
                long photoId = long.Parse(doc.Get(PhotoIndexDocument.PhotoId));
                photoIds.Add(photoId);
            }
            IEnumerable<Photo> photoList = photoService.GetPhotos(photoIds);

            //组装分页集合
            PagingDataSet<Photo> photos = new PagingDataSet<Photo>(photoList)
            {
                TotalRecords = searchResult.TotalRecords,
                PageIndex = searchResult.PageIndex,
                PageSize = searchResult.PageSize,
                QueryDuration = searchResult.QueryDuration
            };

            return photos;

        }


        /// <summary>
        /// 获取匹配的前几条热词
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
        /// 根据照片搜索条件构建lucene查询条件
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns>lucene条件</returns>
        private LuceneSearchBuilder BuildLuceneSearchBuilder(PhotoFullTextQuery query)
        {
            LuceneSearchBuilder searchBuilder = new LuceneSearchBuilder();
            Dictionary<string, BoostLevel> fieldNameAndBoosts = new Dictionary<string, BoostLevel>();

            if (query.UserId>0)
            {
                 searchBuilder.WithField(PhotoIndexDocument.UserId, query.UserId.ToString(), true, BoostLevel.Hight,false);
            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                switch (query.Filter)
                {
                    case PhotoSearchRange.DESCRIPTION:
                        searchBuilder.WithPhrase(PhotoIndexDocument.Description, query.Keyword, BoostLevel.Hight, false);
                        break;
                    case PhotoSearchRange.AUTHOR:
                        searchBuilder.WithPhrase(PhotoIndexDocument.Author, query.Keyword, BoostLevel.Hight, false);
                        break;
                    case PhotoSearchRange.TAG:
                        searchBuilder.WithPhrase(PhotoIndexDocument.Tag, query.Keyword, BoostLevel.Hight, false);
                        break;
                    default:
                        fieldNameAndBoosts.Add(PhotoIndexDocument.Tag, BoostLevel.Hight);
                        fieldNameAndBoosts.Add(PhotoIndexDocument.Description, BoostLevel.Hight);
                        fieldNameAndBoosts.Add(PhotoIndexDocument.Author, BoostLevel.Medium);
                        searchBuilder.WithPhrases(fieldNameAndBoosts, query.Keyword, BooleanClause.Occur.SHOULD, false);
                        break;
                }   
            }

            //筛选 全部、某人的照片
            if (query.UserId > 0)
            {
                searchBuilder.WithField(PhotoIndexDocument.UserId, query.UserId.ToString(), true, BoostLevel.Hight, true);
            }

            //过滤审核状态和隐私状态
            if (!query.IgnoreAuditAndPrivacy)
            {
                searchBuilder.NotWithField(PhotoIndexDocument.PrivacyStatus, ((int)PrivacyStatus.Private).ToString());
                switch (publiclyAuditStatus)
                {
                    case PubliclyAuditStatus.Again:
                    case PubliclyAuditStatus.Fail:
                    case PubliclyAuditStatus.Pending:
                    case PubliclyAuditStatus.Success:
                        searchBuilder.WithField(PhotoIndexDocument.AuditStatus, ((int)publiclyAuditStatus).ToString(), true, BoostLevel.Hight, true);
                        break;
                    case PubliclyAuditStatus.Again_GreaterThanOrEqual:
                    case PubliclyAuditStatus.Pending_GreaterThanOrEqual:
                        searchBuilder.WithinRange(PhotoIndexDocument.AuditStatus, ((int)publiclyAuditStatus).ToString(), ((int)PubliclyAuditStatus.Success).ToString(), true);
                        break;
                }
            }

            return searchBuilder;
        }

        #endregion
    }
}