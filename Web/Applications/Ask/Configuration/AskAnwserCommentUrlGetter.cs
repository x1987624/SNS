using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Ask
{
    public class AskAnwserCommentUrlGetter: ICommentUrlGetter
    {

        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().AskAnswer(); }
        }

        /// <summary>
        /// 获取被评论对象名称
        /// </summary>
        /// <param name="commentedObjectId">被评论对象Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public string GetCommentedObjectName(long commentedObjectId, string tenantTypeId)
        {
            if (tenantTypeId == TenantTypeIds.Instance().AskAnswer())
            {
                AskAnswer askAnswer = new AskService().GetAnswer(commentedObjectId);
                if (askAnswer != null)
                {
                    return askAnswer.Body;
                }
            }
            return string.Empty;
        }

        public string GetCommentDetailUrl(long commentedObjectId, long id, long? userId = null)
        {
            AskAnswer answer = new AskService().GetAnswer(commentedObjectId);
            if (answer != null)
            {
                return SiteUrls.Instance().AskQuestionDetail(answer.QuestionId);
            }
            return null;
        }

        public string GetCommentedObjectUrl(long commentedObjectId, long? userId = null)
        {
            return null;
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
            if (tenantTypeId == TenantTypeIds.Instance().AskAnswer())
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
            AskAnswer answer = new AskService().GetAnswer(commentedObjectId);
            if (answer != null)
            {
                CommentedObject commentedObject = new CommentedObject();
                commentedObject.DetailUrl = SiteUrls.Instance().AskQuestionDetail(answer.QuestionId);
                commentedObject.Name = answer.Body;
                commentedObject.Author = answer.Author;
                commentedObject.UserId = answer.UserId;
                return commentedObject;
            }
            return null;
        }
    }
}