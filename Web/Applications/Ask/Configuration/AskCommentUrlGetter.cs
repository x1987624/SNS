using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Ask
{
    public class AskCommentUrlGetter : ICommentUrlGetter
    {
        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().AskQuestion(); }
        }
        
        /// <summary>
        /// 获取被评论对象名称
        /// </summary>
        /// <param name="commentedObjectId">被评论对象Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public string GetCommentedObjectName(long commentedObjectId, string tenantTypeId)
        {
            if (tenantTypeId == TenantTypeIds.Instance().AskQuestion())
            {
                AskQuestion askQuestion = new AskService().GetQuestion(commentedObjectId);
                if (askQuestion != null)
                {
                    return askQuestion.Subject;
                }
            }
            return string.Empty;
        }

        public string GetCommentDetailUrl(long commentedObjectId, long id, long? userId = null)
        {
            return SiteUrls.Instance().AskQuestionDetail(commentedObjectId);
        }

        /// <summary>
        /// 获取被评论对象url
        /// </summary>
        /// <param name="commentedObjectId">被评论对象Id</param>
        /// <param name="userId">被评论对象作者Id</param>
        /// <returns></returns>
        public string GetCommentedObjectUrl(long commentedObjectId, long? userId = null, string tenantTypeId = null)
        {
            if (!userId.HasValue || userId <= 0) return string.Empty;
            if (tenantTypeId == TenantTypeIds.Instance().AskQuestion())
            {
                return SiteUrls.Instance().AskQuestionDetail(commentedObjectId);
            }
            return string.Empty;
        }



        /// <summary>
        /// 获取被评论对象(部分)
        /// </summary>
        /// <param name="commentedObjectId"></param>
        /// <returns></returns>
        public CommentedObject GetCommentedObject(long commentedObjectId)
        {
            AskQuestion question = new AskService().GetQuestion(commentedObjectId);
            if (question != null)
            {
                CommentedObject commentedObject = new CommentedObject();
                commentedObject.DetailUrl = SiteUrls.Instance().AskQuestionDetail(commentedObjectId);
                commentedObject.Name = question.Subject;
                commentedObject.Author = question.Author;
                commentedObject.UserId = question.UserId;
                return commentedObject;
            }
            return null;
        }
    }
}