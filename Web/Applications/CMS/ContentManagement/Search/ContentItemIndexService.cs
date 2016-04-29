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
//using System.IO;
//using Lucene.Net.Analysis;
//using Common.Logging;
//using Lucene.Net.Index;
//using Lucene.Net.Store;
//using Lucene.Net.Documents;
//using Spacebuilder.CMS.Metadata;
//using Lucene.Net.Search;
//using Spacebuilder.CMS.Repositories;
//using Spacebuilder.CMS;

//namespace Spacebuilder.CMS.Search
//{
//    public class ContentItemIndexService
//    {
//        //索引文件夹
//        private readonly string IndexDirectory = "~/IndexFiles/ContentItems/";
//        public readonly string PhysicalIndexDirectory = null;

//        private static volatile ContentItemIndexService _self = null;
//        private static readonly object lockObject = new object();
//        public static ContentItemIndexService Instance()
//        {
//            if (_self == null)
//            {
//                lock (lockObject)
//                {
//                    if (_self == null)
//                    {
//                        _self = new ContentItemIndexService();
//                    }
//                }
//            }
//            return _self;
//        }

//        private ContentItemIndexService()
//        {
//            PhysicalIndexDirectory = WebUtils.GetPhysicalFilePath(IndexDirectory);
//            var directory = new DirectoryInfo(PhysicalIndexDirectory);
//            if (!directory.Exists)
//                directory.Create();

//            if (!IndexReader.IndexExists(FSDirectory.Open(directory)))
//            {
//                var writer = new IndexWriter(FSDirectory.Open(directory), GetChineseAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);
//                writer.Close();
//                Logger.InfoFormat("Index [{0}] created", IndexDirectory);
//            }
//        }


//        /// <summary>
//        /// 保存到索引文件
//        /// </summary>
//        /// <param name="contentItems"></param>
//        public void Store(ContentItem contentItem)
//        {
//            Store(new List<ContentItem> { contentItem });
//        }

//        /// <summary>
//        /// 保存到索引文件
//        /// </summary>
//        /// <param name="contentItems"></param>
//        public void Store(IEnumerable<ContentItem> contentItems)
//        {
//            Store(contentItems, true);
//        }

//        /// <summary>
//        /// 保存到索引文件
//        /// </summary>
//        /// <param name="contentItems"></param>
//        private void Store(IEnumerable<ContentItem> contentItems, bool deleteRepeated)
//        {
//            if (contentItems.Count() == 0)
//            {
//                return;
//            }
//            if (deleteRepeated)
//            {
//                // 首先清除可能存在的索引
//                Delete(contentItems.Select(i => i.ContentItemId));
//            }

//            var writer = new IndexWriter(GetLuceneDirectory(), GetChineseAnalyzer(), false, IndexWriter.MaxFieldLength.UNLIMITED);
//            try
//            {
//                foreach (var contentItem in contentItems)
//                {
//                    var doc = ConvertContentItemToDocument(contentItem);
//                    writer.AddDocument(doc);
//                    Logger.InfoFormat("Document [{0}] indexed", contentItem.ContentItemId);
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Error("An unexpected error occured while add the document from the index.", ex);
//            }
//            finally
//            {
//                writer.Optimize();
//                writer.Close();
//            }
//        }


//        /// <summary>
//        /// 删除索引
//        /// </summary>
//        /// <param name="contentItemIds"></param>
//        public void Delete(int contentItemId)
//        {
//            Delete(new List<int> { contentItemId });
//        }

//        /// <summary>
//        /// 删除索引
//        /// </summary>
//        /// <param name="contentItemIds"></param>
//        public void Delete(IEnumerable<int> contentItemIds)
//        {
//            if (!contentItemIds.Any())
//            {
//                return;
//            }
//            var writer = new IndexWriter(GetLuceneDirectory(), GetChineseAnalyzer(), false, IndexWriter.MaxFieldLength.UNLIMITED);
//            try
//            {
//                var query = new BooleanQuery();
//                try
//                {
//                    foreach (var id in contentItemIds)
//                    {
//                        query.Add(new BooleanClause(new TermQuery(new Term(ContentItemIndexFields.ContentItemId, id.ToString())), BooleanClause.Occur.SHOULD));
//                    }
//                    writer.DeleteDocuments(query);
//                }
//                catch (Exception ex)
//                {
//                    Logger.ErrorFormat("An unexpected error occured while removing the documents [{0}] from the index.", ex, String.Join(", ", contentItemIds));
//                }
//            }
//            finally
//            {
//                writer.Close();
//            }
//        }

//        /// <summary>
//        /// 重建索引
//        /// </summary>
//        public void RebuildIndex()
//        {
//            DirectoryInfo directoryInfo = new DirectoryInfo(PhysicalIndexDirectory);
//            if (directoryInfo.Exists)
//                directoryInfo.Delete(true);

//            directoryInfo.Create();

//            if (!IndexReader.IndexExists(Lucene.Net.Store.FSDirectory.Open(new System.IO.DirectoryInfo(IndexDirectory))))
//            {
//                var writer = new IndexWriter(GetLuceneDirectory(), GetChineseAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);
//                writer.Close();
//            }

//            IEnumerable<ContentItem> contentItems = ContentItemService.GetContentItemsForAdmin(int.MaxValue, 1, null, null, null, null, null, null, null, null);
            
//            int batchIndex = 0;
//            List<ContentItem> batchContentItem = new List<ContentItem>();
//            foreach (var contentItem in contentItems)
//            {
//                batchIndex++;
//                batchContentItem.Add(contentItem);

//                if (batchIndex >= 1000)
//                {
//                    Store(batchContentItem, false);
//                    batchContentItem.Clear();
//                    batchIndex = 0;
//                }
//            }
//            if (batchContentItem.Count > 0)
//            {
//                Store(batchContentItem, false);
//            }
//        }



//        #region Help Methods

//        /// <summary>
//        /// 将对象转换为Document
//        /// </summary>
//        /// <param name="thread"></param>
//        /// <returns></returns>
//        protected Document ConvertContentItemToDocument(ContentItem contentItem)
//        {
//            if (contentItem == null)
//                return null;

//            Document doc = new Document();
//            Field field;

//            field = new Field(ContentItemIndexFields.ContentItemId, contentItem.ContentItemId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.ContentFolderId, contentItem.ContentFolderId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.ContentTypeId, contentItem.ContentTypeId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.Title, contentItem.Title, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.UserId, contentItem.UserId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.Author, contentItem.Author, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.IsEssential, contentItem.IsEssential.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.IsSticky, contentItem.IsSticky.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.DateCreated, DateTools.DateToString(contentItem.DateCreated, DateTools.Resolution.DAY), Field.Store.YES, Field.Index.NOT_ANALYZED);
//            doc.Add(field);

//            field = new Field(ContentItemIndexFields.LastModified, DateTools.DateToString(contentItem.LastModified, DateTools.Resolution.DAY), Field.Store.YES, Field.Index.NOT_ANALYZED);
//            doc.Add(field);

//            ContentTypeDefinition contentType = contentItem.ContentType;
//            if (contentType != null)
//            {
//                foreach (var column in contentType.Columns)
//                {
//                    if (column.DataType.Equals(UsableDatabaseTypes.Boolean, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        field = new Field(column.ColumnName, contentItem.AdditionalProperties.Get<bool>(column.ColumnName).ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//                    }
//                    else if (column.DataType.Equals(UsableDatabaseTypes.DateTime, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        field = new Field(column.ColumnName, DateTools.DateToString(contentItem.AdditionalProperties.Get<DateTime>(column.ColumnName), DateTools.Resolution.DAY), Field.Store.YES, Field.Index.NOT_ANALYZED);
//                    }
//                    else if (column.DataType.Equals(UsableDatabaseTypes.Decimal, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        field = new Field(column.ColumnName, contentItem.AdditionalProperties.Get<Decimal>(column.ColumnName).ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//                    }
//                    else if (column.DataType.Equals(UsableDatabaseTypes.Int32, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        field = new Field(column.ColumnName, contentItem.AdditionalProperties.Get<Int32>(column.ColumnName).ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//                    }
//                    else if (column.DataType.Equals(UsableDatabaseTypes.Int64, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        field = new Field(column.ColumnName, contentItem.AdditionalProperties.Get<Int64>(column.ColumnName).ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED);
//                    }
//                    else if (column.DataType.Equals(UsableDatabaseTypes.String, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        field = new Field(column.ColumnName, contentItem.AdditionalProperties.Get<string>(column.ColumnName).ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);
//                    }
//                    else
//                    {
//                        field = null;
//                    }
//                    if (field != null)
//                        doc.Add(field);
//                }
//            }


//            return doc;


//        }

//        /// <summary>
//        /// 获取Lucene索引文件目录
//        /// </summary>
//        /// <returns></returns>
//        protected Lucene.Net.Store.Directory GetLuceneDirectory()
//        {
//            var directoryInfo = new DirectoryInfo(PhysicalIndexDirectory);
//            return FSDirectory.Open(directoryInfo);
//        }

//        /// <summary>
//        /// 获取用于中文分词的Analyzer
//        /// </summary>
//        /// <returns>Analyzer</returns>
//        protected Analyzer GetChineseAnalyzer()
//        {
//            return new Lucene.Net.Analysis.PanGu.PanGuAnalyzer();
//        }

//        private ILog logger = null;
//        protected ILog Logger
//        {
//            get
//            {
//                if (logger == null)
//                    logger = Logging.LoggerFactory.GetLogger();

//                return logger;
//            }
//        }

//        #endregion

//    }
//}
