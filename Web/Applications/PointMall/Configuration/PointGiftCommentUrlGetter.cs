using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tunynet.Common;
using Spacebuilder.Common;
using Tunynet;

namespace Spacebuilder.PointMall
{
    public class PointGiftCommentUrlGetter : ICommentUrlGetter
    {

        /// <summary>
        /// 租户类型Id
        /// </summary>
        public string TenantTypeId
        {
            get { return TenantTypeIds.Instance().PointGift(); }
        }

        /// <summary>
        /// 获取被评论对象名称
        /// </summary>
        /// <param name="commentedObjectId">被评论对象Id</param>
        /// <param name="tenantTypeId">租户类型Id</param>
        /// <returns></returns>
        public string GetCommentedObjectName(long commentedObjectId, string tenantTypeId)
        {
            if (tenantTypeId == TenantTypeIds.Instance().PointGift())
            {
                PointGift pointGift = new PointMallService().GetGift(commentedObjectId);
                if (pointGift != null)
                {
                    return pointGift.Name;
                }
            }
            return string.Empty;
        }

        public string GetCommentDetailUrl(long commentedObjectId, long id, long? userId = null)
        {
            return SiteUrls.Instance().GiftDetail(commentedObjectId);
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
            if (tenantTypeId == TenantTypeIds.Instance().PointGift())
            {
                return SiteUrls.Instance().GiftDetail(commentedObjectId);
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
            PointGift pointGift = new PointMallService().GetGift(commentedObjectId);
            if (pointGift != null)
            {
                IUser user = DIContainer.Resolve<UserService>().GetUser(pointGift.UserId);
                CommentedObject commentedObject = new CommentedObject();
                commentedObject.DetailUrl = SiteUrls.Instance().GiftDetail(commentedObjectId);
                commentedObject.Name = pointGift.Name;
                commentedObject.Author = user.DisplayName;
                commentedObject.UserId = pointGift.UserId;
                return commentedObject;
            }
            return null;
        }
    }
}