//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Lucene.Net.Documents;
using Spacebuilder.Search;
using Tunynet;
using Tunynet.Common;
using Tunynet.Utilities;

namespace Spacebuilder.Ask
{
    /// <summary>
    /// 问答索引文档
    /// </summary>
    public class AskIndexDocument
    {
        private static AskService askService = new AskService();

        #region 索引字段

        public static readonly string QuestionId = "QuestionId";
        public static readonly string Author = "Author";
        public static readonly string Subject = "Subject";
        public static readonly string Body = "Body";
        public static readonly string Answer = "Answer";
        public static readonly string Tag = "Tag";
        public static readonly string AuditStatus = "AuditStatus";
        public static readonly string DateCreated = "DateCreated";

        #endregion


        /// <summary>
        /// AskQuestion转换成Document
        /// </summary>
        /// <param name="question">问题对象</param>
        /// <returns>问题对应的document对象</returns>
        public static Document Convert(AskQuestion question)
        {
            //文档初始权重
            Document doc = new Document();

            //不索引问题审核状态为不通过的、已取消的
            if (question.AuditStatus != Tunynet.Common.AuditStatus.Fail && question.Status != QuestionStatus.Canceled)
            {
                //索引基本信息
                doc.Add(new Field(AskIndexDocument.QuestionId, question.QuestionId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field(AskIndexDocument.Author, question.Author.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field(AskIndexDocument.Subject, question.Subject.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                if (!string.IsNullOrWhiteSpace(question.Body))
                {
                    doc.Add(new Field(AskIndexDocument.Body, HtmlUtility.StripHtml(question.Body, true, false).ToLower(), Field.Store.NO, Field.Index.ANALYZED));
                }
                doc.Add(new Field(AskIndexDocument.AuditStatus, ((int)question.AuditStatus).ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field(AskIndexDocument.DateCreated, DateTools.DateToString(question.DateCreated, DateTools.Resolution.MILLISECOND), Field.Store.YES, Field.Index.NOT_ANALYZED));

                //索引问题标签
                foreach (var tag in question.Tags)
                {
                    doc.Add(new Field(AskIndexDocument.Tag, tag.TagName.ToLower(), Field.Store.YES, Field.Index.ANALYZED));
                }

                //循环加入问题的回答
                int pageSize = 100;
                int pageIndex = 1;
                int pageCount = 1;
                do
                {
                    PagingDataSet<AskAnswer> answers = askService.GetAnswersByQuestionId(question.QuestionId, SortBy_AskAnswer.DateCreated_Desc, pageSize, pageIndex);
                    foreach (AskAnswer answer in answers)
                    {

                        Field field = new Field(AskIndexDocument.Answer, HtmlUtility.TrimHtml(answer.Body, 0).ToLower(), Field.Store.NO, Field.Index.ANALYZED);
                        if (answer.IsBest)
                        {
                            field.SetBoost((float)BoostLevel.Hight);
                        }
                        doc.Add(field);
                    }
                    pageCount = answers.PageCount;
                    pageIndex++;
                } while (pageIndex <= pageCount);
            }

            //回答数和悬赏值给文档加权重
            float boost = question.AnswerCount + question.Reward + (float)BoostLevel.Low;

            //已解决问题给文档加权重
            if (question.Status == QuestionStatus.Resolved)
            {
                boost += (float)BoostLevel.Medium;
            }

            //精华问题给文档加权重
            if (question.IsEssential)
            {
                boost += (float)BoostLevel.Hight;
            }
            doc.SetBoost(boost);

            return doc;
        }

        /// <summary>
        /// 从Question到Document的批量转换
        /// </summary>
        /// <param name="questions">问题集合</param>
        /// <returns>Document集合</returns>
        public static IEnumerable<Document> Convert(IEnumerable<AskQuestion> questions)
        {
            List<Document> docs = new List<Document>();
            foreach (var question in questions)
            {
                Document doc = Convert(question);
                if (doc != null)
                {
                    docs.Add(doc);
                }
            }
            return docs;
        }
    }
}