using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;
using Spacebuilder.Common;

namespace Spacebuilder.Photo
{
    public class PhotoCommentUrlGetter : ICommentUrlGetter
    {

        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().Photo(); }
        }

        /// <summary>
        /// 获取被评论对象名称
        /// </summary>
        /// <param name="commentedObjectId">被评论对象Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public string GetCommentedObjectName(long commentedObjectId, string tenantTypeId)
        {
            if (tenantTypeId == TenantTypeIds.Instance().Photo())
            {
                Photo photo = new PhotoService().GetPhoto(commentedObjectId);
                if (photo != null)
                {
                    return SiteUrls.FullUrl(SiteUrls.Instance().PhotoDetail(commentedObjectId));
                }
            }
            return string.Empty;
        }

        public string GetCommentDetailUrl(long commentedObjectId, long id, long? userId = null)
        {
            return SiteUrls.Instance().PhotoDetail(commentedObjectId);
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
            if (tenantTypeId == TenantTypeIds.Instance().Photo())
            {
                return SiteUrls.Instance().PhotoDetail(commentedObjectId);
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
            Photo photo = new PhotoService().GetPhoto(commentedObjectId);
            if (photo != null)
            {
                CommentedObject commentedObject = new CommentedObject();
                commentedObject.DetailUrl = SiteUrls.Instance().PhotoDetail(commentedObjectId);
                commentedObject.Name = SiteUrls.FullUrl(SiteUrls.Instance().PhotoDetail(commentedObjectId));
                commentedObject.Author = photo.Author;
                commentedObject.UserId = photo.UserId;
                return commentedObject;
            }
            return null;
        }
    }
}