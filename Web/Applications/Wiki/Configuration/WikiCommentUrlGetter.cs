using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;
using Spacebuilder.Common;
using Spacebuilder.Wiki;

namespace Spacebuilder.Wiki
{
    public class WikiCommentUrlGetter : ICommentUrlGetter
    {
        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().WikiPage(); }
        }

        /// <summary>
        /// 获取被评论对象名称
        /// </summary>
        /// <param name="commentedObjectId">被评论对象Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public string GetCommentedObjectName(long commentedObjectId, string tenantTypeId)
        {
            if (tenantTypeId == TenantTypeIds.Instance().WikiPage())
            {
                WikiPage page = new WikiService().Get(commentedObjectId);
                if (page != null)
                {
                    return page.Title;
                }
            }
            return string.Empty;
        }

        public string GetCommentDetailUrl(long commentedObjectId, long id, long? userId = null)
        {
            return SiteUrls.Instance().PageDetail(commentedObjectId);
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
            if (tenantTypeId == "101501")
            {
                return SiteUrls.Instance().PageDetail(commentedObjectId);
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
            WikiPage page = new WikiService().Get(commentedObjectId);
            if (page != null)
            {
                CommentedObject commentedObject = new CommentedObject();
                commentedObject.DetailUrl = SiteUrls.Instance().PageDetail(commentedObjectId);
                commentedObject.Name = page.Title;
                commentedObject.Author = page.Author;
                commentedObject.UserId = page.UserId;
                return commentedObject;
            }
            return null;
        }
    }
}