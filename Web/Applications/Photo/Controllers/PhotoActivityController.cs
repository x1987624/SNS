//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet.UI;
using Tunynet.Common;
using Tunynet;
using DevTrends.MvcDonutCaching;

namespace Spacebuilder.Photo.Controllers
{
    /// <summary>
    /// 相册动态控制器
    /// </summary>
    [Themed(PresentAreaKeysOfBuiltIn.Channel, IsApplication = true)]
    public class PhotoActivityController : Controller
    {
        public ActivityService activityService { get; set; }
        public PhotoService photoService { get; set; }
        public CommentService commentService { get; set; }
        public UserService userService { get; set; }
        private AttitudeService attitudeService = new AttitudeService(TenantTypeIds.Instance().Photo());
        public Authorizer authorizer { get; set; }
        /// <summary>
        /// 创建照片动态
        /// </summary>
        /// <param name="ActivityId"></param>
        /// <returns></returns>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _CreatePhoto(long ActivityId)
        {
            //实例化动态
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            //实例化照片，相册
            Album album = photoService.GetAlbum(activity.SourceId);
            if (album == null || album.PhotoCount==0||album.User==null)
            {
                return Content(string.Empty);
            }

            IEnumerable<Photo> photos = photoService.GetPhotosOfAlbum(TenantTypeIds.Instance().User(), activity.SourceId, false, SortBy_Photo.DateCreated_Desc, album.LastUploadDate, 7, 1);
            ViewData["Album"] = album;

            //判断是否具有查看相册图片的权限
            ViewData["isValid"] = authorizer.Album_View(album);           

            return View(photos);
        }

        /// <summary>
        /// 评论照片动态
        /// </summary>
        /// <param name="ActivityId"></param>
        /// <returns></returns>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _CommentPhoto(long ActivityId)
        {
            //实例化动态
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            //实例化评论
            PagingDataSet<Comment> commentPaging = commentService.GetRootComments(TenantTypeIds.Instance().Photo(), activity.ReferenceId, 1, SortBy_Comment.DateCreatedDesc);
            //去掉评论作者相同的评论然后取前3个
            IEnumerable<long> commentUserIds = commentPaging.AsEnumerable().Select(n => n.UserId).Distinct().Take(3);
            List<Comment> commentList = new List<Comment>();
            foreach (var commentUserId in commentUserIds)
            {
                commentList.Add(commentPaging.First(n => n.UserId == commentUserId));
            }
            IEnumerable<Comment> comments = commentList.AsEnumerable();
            if (comments == null)
            {
                return Content(string.Empty);
            }
            ViewData["CommentCount"]=commentPaging.Count();

            //实例化照片
            Photo photo = photoService.GetPhoto(activity.ReferenceId);
            if (photo == null)
            {
                return Content(string.Empty);
            }
            ViewData["Photo"] = photo;

            return View(comments);
        }

        /// <summary>
        /// 创建圈人动态
        /// </summary>
        /// <param name="ActivityId"></param>
        /// <returns></returns>
        //[DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult _CreatePhotoLabel(long ActivityId)
        {
            //实例化动态
            Activity activity = activityService.Get(ActivityId);
            if (activity == null)
            {
                return Content(string.Empty);
            }
            ViewData["Activity"] = activity;

            //实例化圈人
            PhotoLabel photoLabel = photoService.GetLabel(activity.SourceId);
            if (photoLabel == null)
            {
                return Content(string.Empty);
            }

            //实例化评论
            PagingDataSet<Comment> commentPaging = commentService.GetRootComments(TenantTypeIds.Instance().Photo(), photoLabel.PhotoId, 1, SortBy_Comment.DateCreatedDesc);
            int commentCount = 0;
            if (commentPaging == null)
            {
                commentCount = 0;
            }
            else
            {
                commentCount = commentPaging.Count;
            }
            ViewData["CommentCount"] = commentCount;
            ViewData["User"] = DIContainer.Resolve<UserService>().GetUser(photoLabel.UserId);

            return View(photoLabel);
        }
    }
}
