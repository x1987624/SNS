//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;
using System;

namespace Spacebuilder.Photo.Controllers
{
    /// <summary>
    /// 相册后台管理控制器
    /// </summary>
    [ManageAuthorize(CheckCookie = false)]
    [TitleFilter(TitlePart = "照片管理", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.ControlPanel, IsApplication = true)]
    public class ControlPanelPhotoController : Controller
    {
        public IPageResourceManager pageResourceManager { get; set; }
        public PhotoService photoService { get; set; }
        public TagService tagService = new TagService(TenantTypeIds.Instance().Photo());

        /// <summary>
        /// 照片管理
        /// </summary>
        /// <param name="descriptionKeyword">名称关键字</param>
        /// <param name="userId">作者Id</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="isEssential">是否精华</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>
        public ActionResult ManagePhotos(string TenantTypeId, string descriptionKeyword, string userId, long? ownerId, AuditStatus? auditStatus, bool? isEssential, int? pageSize, int? pageIndex)
        {
            //处理用户选择器产生的userId
            long? photoUserId = null;
            if (!string.IsNullOrEmpty(userId))
            {
                userId = userId.Trim(',');
                if (!string.IsNullOrEmpty(userId))
                {
                    photoUserId = long.Parse(userId);
                }
            }
            ViewData["userId"] = photoUserId;

            //组装是否加精
            List<SelectListItem> items = new List<SelectListItem> { new SelectListItem { Text = "是", Value = true.ToString() }, new SelectListItem { Text = "否", Value = false.ToString() } };
            ViewData["isEssential"] = new SelectList(items, "Value", "Text", isEssential);

            PagingDataSet<Photo> photos = photoService.GetPhotosForAdmin(TenantTypeId, descriptionKeyword, photoUserId, ownerId, isEssential, auditStatus, pageSize ?? 20, pageIndex ?? 1);
            return View(photos);
        }

        /// <summary>
        /// 更新审核状态
        /// </summary>
        /// <param name="photoIds">被操作项的Id集合</param>
        /// <param name="isApproved">是否通过审核</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _UpdatePhotosAuditStatus(IEnumerable<long> photoIds, bool isApproved)
        {
            foreach (long photoId in photoIds)
            {
                photoService.ApprovePhoto(photoId, isApproved);
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "更新审核状态成功"));
        }

        /// <summary>
        /// 批量加精/取消精华
        /// </summary>
        /// <param name="photoIds">被操作项的Id集合</param>
        /// <param name="isEssential">加精/取消精华</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SetEssential(IEnumerable<long> photoIds, bool isEssential)
        {
            foreach (long photoId in photoIds)
            {
                photoService.SetEssential(photoService.GetPhoto(photoId), isEssential);
            }
            return Json(new StatusMessageData(StatusMessageType.Success, isEssential ? "设置精华成功" : "取消精华成功"));
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="photoIds">被操作项的Id集合</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _DeletePhotos(IEnumerable<long> photoIds)
        {
            foreach (long photoId in photoIds)
            {
                photoService.DeletePhoto(photoService.GetPhoto(photoId));
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "删除成功"));
        }

        /// <summary>
        /// 贴标签
        /// </summary>
        /// <param name="photoIds">被操作项的Id集合</param>
        /// <returns></returns> 
        [HttpGet]
        public ActionResult _SetTags(string photoIds)
        {
            ViewData["photoIds"] = photoIds;
            return View();
        }

        /// <summary>
        /// 贴标签
        /// </summary>
        /// <param name="photoIds">被操作项的Id集合</param>
        /// <param name="tagsName">标签名</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult _SetTags(string photoIds, string tagNames)
        {
            photoIds = photoIds.TrimEnd(',');
            string[] photoIdsArray = photoIds.Split(',');
            tagNames = Request.Form.Get<string>("tagNames", string.Empty);
            for (int i = 0; i < photoIdsArray.Length; i++)
            {
                long photoId = Convert.ToInt64(photoIdsArray[i]);
                tagService.AddTagsToItem(tagNames, photoService.GetPhoto(photoId).UserId, photoId);
            }
            return Json(new StatusMessageData(StatusMessageType.Success, "操作成功"));
        }

        /// <summary>
        /// 相册管理
        /// </summary>
        /// <param name="nameKeyword">名称关键字</param>
        /// <param name="userId">作者ID</param>
        /// <param name="ownerId">所有者ID</param>
        /// <param name="auditStatus">审核状态</param>
        /// <param name="pageSize">分页大小</param>
        /// <param name="pageIndex">页码</param>
        /// <returns></returns>

        public ActionResult ManageAlbums(string tenantId, string nameKeyword, string userId, string ownerId, AuditStatus? auditStatus, int pageSize = 20, int pageIndex = 1)
        {
            pageResourceManager.InsertTitlePart("相册管理");

            long? _userId = null;
            if (!string.IsNullOrEmpty(userId))
            {
                userId = userId.Trim(',');
                if (!string.IsNullOrEmpty(userId))
                {
                    _userId = long.Parse(userId);
                }
            }

            long? _ownerId = null;
            if (!string.IsNullOrEmpty(ownerId))
            {
                ownerId = userId.Trim(',');
                if (!string.IsNullOrEmpty(ownerId))
                {
                    _ownerId = long.Parse(ownerId);
                }
            }

            ViewData["userId"] = _userId;
            ViewData["ownerId"] = _ownerId;

            PagingDataSet<Album> albums = photoService.GetAlbumsForAdmin(tenantId, nameKeyword, _userId, _ownerId, auditStatus, pageSize, pageIndex);
            return View(albums);
        }

        /// <summary>
        /// 更新相册审核状态
        /// </summary>
        /// <param name="albumIds">相册ID集合</param>
        /// <param name="isApproved">是否通过审核</param>
        /// <returns>表示更新成功的JSON</returns>
        [HttpPost]
        public JsonResult _UpdateAlbumAuditStatus(IEnumerable<long> albumIds, bool isApproved)
        {
            foreach (long albumId in albumIds)
            {
                photoService.ApproveAlbum(albumId, isApproved);
            }

            return Json(new StatusMessageData(StatusMessageType.Success, "更新相册审核状态成功！"));
        }

        /// <summary>
        /// 更新相册审核状态
        /// </summary>
        /// <param name="albumId">相册ID</param>
        /// <param name="isApproved">是否通过审核</param>
        /// <returns>表示更新成功的JSON</returns>
        [HttpPost]
        public JsonResult _UpdateAlbumAuditStatu(long albumId, bool isApproved)
        {
            photoService.ApproveAlbum(albumId, isApproved);
            return Json(new StatusMessageData(StatusMessageType.Success, "更新相册审核状态成功！"));
        }

        /// <summary>
        /// 批量删除相册
        /// </summary>
        /// <param name="albumIds">相册ID集合</param>
        /// <returns>表示删除成功的JSON</returns>
        [HttpPost]
        public ActionResult _DeleteAlbums(IEnumerable<long> albumIds)
        {
            foreach (long albumId in albumIds)
            {
                photoService.DeleteAlbum(photoService.GetAlbum(albumId));
            }

            return Json(new StatusMessageData(StatusMessageType.Success, "删除相册成功！"));
        }



    }
}