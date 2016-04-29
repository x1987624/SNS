//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Web.Routing;
using Spacebuilder.Photo;
using Tunynet.Common;

using Tunynet.Utilities;
using System.Collections.Generic;
using System;

namespace Spacebuilder.Common
{
    /// <summary>
    /// 相册链接管理扩展
    /// </summary>
    public static class SiteUrlsExtension
    {
        private static readonly string PhotoAreaName = PhotoConfig.Instance().ApplicationKey;

        #region 后台管理
        #region 相册管理
        /// <summary>
        /// 相册管理
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="auditStatus">审核状态</param>
        public static string AlbumControlPanelManage(this SiteUrls siteUrls, AuditStatus? auditStatus = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (auditStatus.HasValue)
            {
                dic.Add("auditStatus", auditStatus);
            }

            return CachedUrlHelper.Action("ManageAlbums", "ControlPanelPhoto", PhotoAreaName, dic);

        }

        /// <summary>
        /// 更新评论的审核状态
        /// </summary>
        public static string _UpdateAlbumAuditStatus(this SiteUrls siteUrls, bool isApproved)
        {
            return CachedUrlHelper.Action("_UpdateAlbumAuditStatus", "ControlPanelPhoto", PhotoAreaName, new RouteValueDictionary() { { "isApproved", isApproved } });
        }

        /// <summary>
        /// 更新评论的审核状态
        /// </summary>
        public static string _UpdateAlbumAuditStatu(this SiteUrls siteUrls, long albumId, bool isApproved)
        {
            return CachedUrlHelper.Action("_UpdateAlbumAuditStatu", "ControlPanelPhoto", PhotoAreaName, new RouteValueDictionary() { { "isApproved", isApproved }, { "albumId", albumId } });
        }

        /// <summary>
        /// 单个/批量删除相册
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="albumId"></param>
        /// <returns></returns>
        public static string _PhotoControlPanelDeleteAlbum(this SiteUrls siteUrls, long? albumId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (albumId != null)
            {
                dic.Add("albumIds", albumId);
            }
            return CachedUrlHelper.Action("_DeleteAlbums", "ControlPanelPhoto", PhotoAreaName, dic);
        }
        #endregion

        #region 照片管理
        /// <summary>
        /// 照片管理
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="auditStatus">审核状态</param>
        public static string PhotoControlPanelManage(this SiteUrls siteUrls, AuditStatus? auditStatus = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (auditStatus.HasValue)
            {
                dic.Add("auditStatus", auditStatus);
            }
            return CachedUrlHelper.Action("ManagePhotos", "ControlPanelPhoto", PhotoAreaName, dic);

        }

        /// <summary>
        /// 批量/单个删除照片
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="photoId">单个删除时的照片Id</param>
        /// <returns></returns>
        public static string _PhotoControlPanelDeletePhoto(this SiteUrls siteUrls, long? photoId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != null)
            {
                dic.Add("photoIds", photoId);
            }
            return CachedUrlHelper.Action("_DeletePhotos", "ControlPanelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 批量通过/不通过审核照片
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isApproved">是否通过审核</param>
        /// <returns></returns>
        public static string _PhotoControlPanelUpdateAuditStatus(this SiteUrls siteUrls, bool isApproved)
        {
            return CachedUrlHelper.Action("_UpdatePhotosAuditStatus", "ControlPanelPhoto", PhotoAreaName, new RouteValueDictionary { { "isApproved", isApproved } });
        }

        /// <summary>
        /// 批量/单个设置精华
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="isEssential">是否精华</param>
        /// <returns>批量设置精华</returns>
        public static string _PhotoControlPanelSetEssential(this SiteUrls siteUrls, bool isEssential)
        {
            return CachedUrlHelper.Action("_SetEssential", "ControlPanelPhoto", PhotoAreaName, new RouteValueDictionary { { "isEssential", isEssential } });
        }

        /// <summary>
        /// 照片管理-贴标签
        /// </summary>
        /// <returns></returns>
        public static string _SetTags(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_SetTags", "ControlPanelPhoto", PhotoAreaName);
        }
        #endregion
        #endregion

        #region 用户空间

        /// <summary>
        /// 创建编辑相册页
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="spaceKey">用户标识</param>
        /// <param name="albumId">相册Id</param>
        /// <returns></returns>
        public static string _EditAlbum(this SiteUrls siteUrls, string spaceKey, long albumId = 0, string callBack = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (albumId != 0)
            {
                dic.Add("albumId", albumId);
            }
            dic.Add("spaceKey", spaceKey);
            if (!string.IsNullOrEmpty(callBack))
            {
                dic.Add("callBack", callBack);
            }
            return CachedUrlHelper.Action("_EditAlbum", "UserSpacePhoto", PhotoAreaName, dic);
        }

        #region 上传图片

        /// <summary>
        /// 上传图片页面
        /// </summary>
        public static string Upload(this SiteUrls siteUrls, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("Upload", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 上传照片
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="spaceKey"></param>
        /// <returns></returns>
        public static string UploadPhoto(this SiteUrls siteUrls, string spaceKey, long AlbumId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", WebUtility.UrlEncode(spaceKey));
            dic.Add("AlbumId", AlbumId);
            return CachedUrlHelper.Action("UploadPhoto", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 完成上传
        /// </summary>
        public static string CompleteUpload(this SiteUrls siteUrls, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("CompleteUpload", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 相片局部页
        /// </summary>
        public static string _PhotoItem(this SiteUrls siteUrls, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("_PhotoItem", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 普通上传页面
        /// </summary>
        public static string UploadNormal(this SiteUrls siteUrls, long albumId, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            dic.Add("albumId", albumId);
            return CachedUrlHelper.Action("UploadNormal", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 普通上传
        /// </summary>
        public static string UploadNormal(this SiteUrls siteUrls, string spaceKey, long albumId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            dic.Add("albumId", albumId);
            return CachedUrlHelper.Action("UploadNormal", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 上传照片-完善照片描述(页面)
        /// </summary>
        public static string UploadEdit(this SiteUrls siteUrls, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("UploadEdit", "UserSpacePhoto", PhotoAreaName, dic);
        }

        #endregion

        /// <summary>
        /// 最新照片
        /// </summary>
        public static string Photos(this SiteUrls siteUrls, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("Photos", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 删除相册
        /// </summary>
        public static string _DeleteAlbum(this SiteUrls siteUrls, string spaceKey, long albumId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (albumId != 0)
            {
                dic.Add("albumId", albumId);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("_DeleteAlbum", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 我的相册列表
        /// </summary>
        /// <param name="spaceKey">用户标识</param>
        /// <param name="sortBy">排序</param>
        /// <returns></returns>
        public static string Albums(this SiteUrls siteUrls, string spaceKey, SortBy_Album sortBy = SortBy_Album.DateCreated_Desc)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (sortBy != SortBy_Album.DateCreated_Desc)
            {
                dic.Add("sortBy", sortBy);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("Albums", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 他的相册列表
        /// </summary>
        /// <param name="spaceKey">用户标识</param>
        /// <returns></returns>
        public static string Ta(this SiteUrls siteUrls, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("Ta", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 我的相册首页（空间）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public static string Home(this SiteUrls siteUrls, string spaceKey)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("Home", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 相册详细显示-列表模式
        /// </summary>
        public static string AlbumDetailList(this SiteUrls siteUrls, string spaceKey, long albumId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (albumId != 0)
            {
                dic.Add("albumId", albumId);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("AlbumDetailList", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 相册详细显示-阅读模式
        /// </summary>
        public static string AlbumDetailView(this SiteUrls siteUrls, string spaceKey, long albumId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (albumId != 0)
            {
                dic.Add("albumId", albumId);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("AlbumDetailView", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 照片列表（用于阅读模式下点击更多的显示）
        /// </summary>
        public static string _PhotoList(this SiteUrls siteUrls, string spaceKey, long albumId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (albumId != 0)
            {
                dic.Add("albumId", albumId);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("_PhotoList", "UserSpacePhoto", PhotoAreaName, dic);
        }


        /// <summary>
        /// 设置封面
        /// </summary>
        public static string _SetAlbumCover(this SiteUrls siteUrls, string spaceKey, long photoId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != 0)
            {
                dic.Add("photoId", photoId);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("_SetAlbumCover", "UserSpacePhoto", PhotoAreaName, dic);

        }

        /// <summary>
        /// 编辑照片
        /// </summary>
        public static string _EditPhoto(this SiteUrls siteUrls, string spaceKey, long photoId = 0, string callBack = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != 0)
            {
                dic.Add("photoId", photoId);
            }
            if (!string.IsNullOrEmpty(callBack))
            {
                dic.Add("callBack", callBack);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("_EditPhoto", "UserSpacePhoto", PhotoAreaName, dic);

        }

        /// <summary>
        /// 删除照片
        /// </summary>
        public static string _DeletePhoto(this SiteUrls siteUrls, string spaceKey, long photoId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != 0)
            {
                dic.Add("photoIds", photoId);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("_DeletePhoto", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 移动照片
        /// </summary>
        public static string _MovePhoto(this SiteUrls siteUrls, string spaceKey, long photoId = 0, string callBack = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != 0)
            {
                dic.Add("photoIds", photoId);
            }
            if (!string.IsNullOrEmpty(callBack))
            {
                dic.Add("callBack", callBack);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("_MovePhoto", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 给照片贴标签
        /// </summary>
        public static string _SetPhotoTag(this SiteUrls siteUrls, string spaceKey, long photoId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != 0)
            {
                dic.Add("photoIds", photoId);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("_SetPhotoTag", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 照片管理
        /// </summary>
        public static string PhotoManage(this SiteUrls siteUrls, string spaceKey, long albumId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (albumId != 0)
            {
                dic.Add("albumId", albumId);
            }
            dic.Add("spaceKey", spaceKey);
            return CachedUrlHelper.Action("PhotoManage", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 标签下的照片(用户空间)
        /// </summary>
        public static string PhotosInTag(this SiteUrls siteUrls, string spaceKey, string tagName = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            if (!string.IsNullOrEmpty(tagName))
            {
                dic.Add("tagName", WebUtility.UrlEncode(tagName.Replace(":", "：").TrimEnd('.')));
            }
            return CachedUrlHelper.Action("Tag", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 瀑布流（用于最新照片、我的喜欢等）
        /// </summary>
        public static string _NewWaterFall(this SiteUrls siteUrls, string spaceKey, string tagName = null, bool isFavorite = false, bool showMore = false, int pageIndex = 2)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            dic.Add("pageIndex", pageIndex);
            if (!string.IsNullOrEmpty(tagName))
            {
                dic.Add("tagName", WebUtility.UrlEncode(tagName.Replace(":", "：").TrimEnd('.')));
            }
            if (isFavorite)
            {
                dic.Add("isFavorite", isFavorite);
            }
            if (showMore)
            {
                dic.Add("showMore", showMore);
            }
            return CachedUrlHelper.Action("_NewWaterFall", "UserSpacePhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 瀑布流（用于照片首页）
        /// </summary>
        public static string _HomeWaterFall(this SiteUrls siteUrls, string spaceKey, int pageIndex = 2)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("spaceKey", spaceKey);
            dic.Add("pageIndex", pageIndex);
            return CachedUrlHelper.Action("_HomeWaterFall", "UserSpacePhoto", PhotoAreaName, dic);
        }

        #endregion

        #region 频道列表

        /// <summary>
        /// 频道列表瀑布流
        /// </summary>
        public static string _Photos(this SiteUrls siteUrls, string tagName = null, bool? isEssential = null, SortBy_Photo? sortBy = null, int pageIndex = 1)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(tagName))
            {
                dic.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            if (sortBy.HasValue)
            {
                dic.Add("sortBy", sortBy);
            }
            if (isEssential.HasValue)
            {
                dic.Add("isEssential", isEssential);
            }
            dic.Add("pageIndex", pageIndex);
            return CachedUrlHelper.Action("_Photos", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 照片排行-最新
        /// </summary>
        public static string New(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("New", "ChannelPhoto", PhotoAreaName);
        }

        /// <summary>
        /// 照片排行-精华
        /// </summary>
        public static string Essential(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("Essential", "ChannelPhoto", PhotoAreaName);
        }

        /// <summary>
        /// 照片排行-热点
        /// </summary>
        public static string Hot(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("Hot", "ChannelPhoto", PhotoAreaName);
        }

        /// <summary>
        /// 照片排行-热评
        /// </summary>
        public static string HotComment(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("HotComment", "ChannelPhoto", PhotoAreaName);
        }

        /// <summary>
        /// 照片排行-喜欢
        /// </summary>
        public static string Favorite(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("Favorite", "ChannelPhoto", PhotoAreaName);
        }

        /// <summary>
        /// 标签云图
        /// </summary>
        public static string Tags(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("Tags", "ChannelPhoto", PhotoAreaName);
        }

        /// <summary>
        /// 标签下的最新照片(频道)
        /// </summary>
        public static string TagNew(this SiteUrls siteUrls, string tagName = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(tagName))
            {
                dic.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("TagNew", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 标签下的热点照片(频道)
        /// </summary>
        public static string TagHot(this SiteUrls siteUrls, string tagName = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(tagName))
            {
                dic.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("TagHot", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 标签下的精华照片(频道)
        /// </summary>
        public static string TagEssential(this SiteUrls siteUrls, string tagName = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(tagName))
            {
                dic.Add("tagName", WebUtility.UrlEncode(tagName.TrimEnd('.')));
            }
            return CachedUrlHelper.Action("TagEssential", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 相册频道瀑布流分享，喜欢，评论按钮
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="photoId">照片ID</param>
        /// <returns></returns>
        public static string _ButtonView(this SiteUrls siteUrls, long photoId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != 0)
            {
                dic.Add("photoId", photoId);
            }
            return CachedUrlHelper.Action("_ButtonView", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 照片搜索瀑布流分享，喜欢，评论按钮
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="photoId">照片ID</param>
        /// <returns></returns>
        public static string _ButtonViewForSearch(this SiteUrls siteUrls, long photoId = 0)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != 0)
            {
                dic.Add("photoId", photoId);
            }
            return CachedUrlHelper.Action("_ButtonViewForSearch", "ChannelPhoto", PhotoAreaName, dic);
        }

        #endregion

        #region 照片详细显示

        /// <summary>
        /// 照片详细显示页
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string PhotoDetail(this SiteUrls siteUrls, long photoId)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            dic.Add("photoId", photoId);
            return CachedUrlHelper.Action("Detail", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 设置描述的连接
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="photoId"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static string _SetPhotoDescription(this SiteUrls urls, long? photoId = null, string description = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != null)
                dic.Add("photoId", photoId);
            if (!string.IsNullOrEmpty(description))
                dic.Add("description", description);
            return CachedUrlHelper.Action("_SetPhotoDescription", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 照片详细页面侧边栏
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="photoId">照片的id</param>
        /// <returns>照片详细页面侧边栏</returns>
        public static string _DetailPhotoSide(this SiteUrls urls, long? photoId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId != null)
                dic.Add("photoId", photoId);
            return CachedUrlHelper.Action("_DetailPhotoSide", "ChannelPhoto", PhotoAreaName, dic);
        }

        #endregion

        #region 全文检索

        /// <summary>
        /// 全文搜索瀑布流
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <param name="keyword"></param>
        /// <param name="userId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string _SearchWaterFall(this SiteUrls siteUrls, string keyword = null, long? userId = null, int? filter = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(keyword))
                dic.Add("keyword", WebUtility.UrlEncode(keyword));
            if (userId.HasValue)
                dic.Add("userId", userId);
            if (filter.HasValue)
                dic.Add("filter", filter);

            return CachedUrlHelper.Action("_SearchWaterFall", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 照片全局搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _PhotoGlobalSearch(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_GlobalSearch", "ChannelPhoto", PhotoAreaName);
        }

        /// <summary>
        /// 照片快捷搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string _PhotoQuickSearch(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("_QuickSearch", "ChannelPhoto", PhotoAreaName);
        }

        /// <summary>
        /// 照片搜索
        /// </summary>
        /// <param name="siteUrls"></param>
        /// <returns></returns>
        public static string PhotoPageSearch(this SiteUrls siteUrls, string keyword = "")
        {
            keyword = WebUtility.UrlEncode(keyword);
            RouteValueDictionary dic = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(keyword))
            {
                dic.Add("keyword", keyword);
            }
            return CachedUrlHelper.Action("Search", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 问答搜索自动完成
        /// </summary>
        public static string PhotoSearchAutoComplete(this SiteUrls siteUrls)
        {
            return CachedUrlHelper.Action("SearchAutoComplete", "ChannelPhoto", PhotoAreaName);
        }

        #endregion

        #region 圈人

        /// <summary>
        /// 获取圈人的信息
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="labelId">圈人的id</param>
        /// <returns></returns>
        public static string GetPhotoLabel(this SiteUrls urls, long? labelId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (labelId.HasValue)
                dic.Add("labelId", labelId);
            return CachedUrlHelper.Action("GetPhotoLabel", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 获取相册下的图片
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="photoId">相片id</param>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <returns></returns>
        public static string GetLabelsOfPhoto(this SiteUrls urls, long? photoId = null, string tenantTypeId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId.HasValue)
                dic.Add("photoId", photoId);
            if (!string.IsNullOrEmpty(tenantTypeId))
                dic.Add("tenantTypeId", tenantTypeId);
            return CachedUrlHelper.Action("GetLabelsOfPhoto", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 删除圈人
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="labelId">被删除的圈人id</param>
        /// <returns>删除圈人</returns>
        public static string DeletePhotoLabel(this SiteUrls urls, long? labelId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (labelId != null)
                dic.Add("labelId", labelId);
            return CachedUrlHelper.Action("DeletePhotoLabel", "ChannelPhoto", PhotoAreaName, dic);
        }

        #endregion

        #region 标签

        /// <summary>
        /// 照片详细显示页面侧边的标签
        /// </summary>
        /// <param name="PhotoId">照片id</param>
        /// <param name="urls"></param>
        /// <returns></returns>
        public static string _DetailPhotoSideTag(this SiteUrls urls, long? PhotoId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (PhotoId != null)
                dic.Add("PhotoId", PhotoId);
            return CachedUrlHelper.Action("_DetailPhotoSideTag", "ChannelPhoto", PhotoAreaName, dic);
        }


        /// <summary>
        /// 给照片设置标签
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="photoId">照片id</param>
        /// <param name="tags">标签</param>
        /// <returns></returns>
        public static string _SetPhotoTagInDetail(this SiteUrls urls, long? photoId = null, string tags = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId.HasValue)
                dic.Add("photoId", photoId.Value);
            if (!string.IsNullOrEmpty(tags))
                dic.Add("tags", tags);

            return CachedUrlHelper.Action("_SetPhotoTag", "ChannelPhoto", PhotoAreaName, dic);
        }

        #endregion

        #region 前台异步操作

        /// <summary>
        /// 设置照片精华
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="photoId">照片id</param>
        /// <param name="isEssential">是否精华</param>
        /// <returns></returns>
        public static string _SetPhotoEssential(this SiteUrls urls, long? photoId = null, bool isEssential = true)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId.HasValue)
                dic.Add("photoId", photoId.Value);

            if (!isEssential)
                dic.Add("isEssential", isEssential);

            return CachedUrlHelper.Action("_SetEssential", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 在照片详细显示页面中删除照片
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="photoId">照片id</param>
        /// <returns></returns>
        public static string _DeletePhotoInDetail(this SiteUrls urls, long? photoId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId.HasValue)
                dic.Add("photoId", photoId);

            return CachedUrlHelper.Action("_DeleteInDetail", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 照片详细显示页面设置标题图
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="photoId">照片id</param>
        /// <param name="isCover">是否是标题图</param>
        /// <returns>照片详细显示页面设置标题图</returns>
        public static string _DetailSetAlbumCover(this SiteUrls urls, long? photoId = null, bool isCover = true)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId.HasValue)
                dic.Add("photoId", photoId);

            if (!isCover)
                dic.Add("isCover", isCover);

            return CachedUrlHelper.Action("_SetAlbumCover", "ChannelPhoto", PhotoAreaName, dic);
        }

        /// <summary>
        /// 获取照片的一条数据
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="photoId">照片的id</param>
        /// <returns></returns>
        public static string _GetOnePhoto(this SiteUrls urls, long? photoId = null)
        {
            RouteValueDictionary dic = new RouteValueDictionary();
            if (photoId.HasValue)
                dic.Add("photoId", photoId);
            return CachedUrlHelper.Action("_GetOnePhoto", "ChannelPhoto", PhotoAreaName, dic);
        }

        #endregion
    }
}
