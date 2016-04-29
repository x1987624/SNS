////------------------------------------------------------------------------------
//// <copyright company="Tunynet">
////     Copyright (c) Tunynet Inc.  All rights reserved.
//// </copyright> 
////------------------------------------------------------------------------------

using System.Collections.Generic;
using Lucene.Net.Documents;
using Spacebuilder.Search;
using Tunynet;
using Tunynet.Common;
using Tunynet.Utilities;
using NPinyin;

namespace Spacebuilder.Wiki
{
    /// <summary>
    /// 百科索引文档
    /// </summary>
    public class WikiIndexDocument
    {
        private static WikiService askService = new WikiService();

        #region 索引字段

        public static readonly string PageId = "PageId";
        public static readonly string Author = "Author";
        public static readonly string Title = "Title";
        public static readonly string Body = "Body";
        public static readonly string PinYin = "PinYin";
        public static readonly string Tag = "Tag";
        public static readonly string CategoryId = "CategoryId";
        public static readonly string DateCreated = "DateCreated";
        #endregion


        /// <summary>
        /// WikiPage转换成Document
        /// </summary>
        /// <param name="question">WikiPage</param>
        /// <returns>WikiPage对应的document对象</returns>
        public static Document Convert(WikiPage wikiPage)
        {
            //文档初始权重
            Document doc = new Document();

            //不索引问题审核状态为不通过的、已取消的
            if (wikiPage.AuditStatus != AuditStatus.Fail && wikiPage.IsLogicalDelete != true)
            {
                //索引基本信息
                doc.Add(new Field(WikiIndexDocument.PageId, wikiPage.PageId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field(WikiIndexDocument.Author, wikiPage.Author.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field(WikiIndexDocument.Title, wikiPage.Title.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                if (!string.IsNullOrWhiteSpace(wikiPage.Body))
                {
                    string body=wikiPage.Body;
                    if (wikiPage.LastestVersion != null && !string.IsNullOrEmpty(wikiPage.LastestVersion.Body))
                        body = wikiPage.LastestVersion.Body;
                    doc.Add(new Field(WikiIndexDocument.Body, HtmlUtility.TrimHtml(body, 0).ToLower(), Field.Store.NO, Field.Index.ANALYZED));
                }
                doc.Add(new Field(WikiIndexDocument.PinYin, Pinyin.GetPinyin(wikiPage.Title.ToLower()), Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field(WikiIndexDocument.DateCreated, DateTools.DateToString(wikiPage.DateCreated, DateTools.Resolution.MILLISECOND), Field.Store.YES, Field.Index.NOT_ANALYZED));

                //索引问题标签
                foreach (var tag in wikiPage.TagNames)
                {
                    doc.Add(new Field(WikiIndexDocument.Tag, tag.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                }
                doc.Add(new Field(WikiIndexDocument.CategoryId, wikiPage.SiteCategory.CategoryId.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field(WikiIndexDocument.DateCreated, DateTools.DateToString(wikiPage.DateCreated, DateTools.Resolution.DAY), Field.Store.YES, Field.Index.ANALYZED));               
            }            

            return doc;
        }

        /// <summary>
        /// 从Question到Document的批量转换
        /// </summary>
        /// <param name="questions">问题集合</param>
        /// <returns>Document集合</returns>
        public static IEnumerable<Document> Convert(IEnumerable<WikiPage> wikiPages)
        {
            List<Document> docs = new List<Document>();
            foreach (var wikiPage in wikiPages)
            {
                Document doc = Convert(wikiPage);
                if (doc != null)
                {
                    docs.Add(doc);
                }
            }
            return docs;
        }
    }
}