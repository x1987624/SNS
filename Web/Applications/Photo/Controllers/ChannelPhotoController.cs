//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Spacebuilder.Common;
using Tunynet;
using Tunynet.Common;

using Tunynet.UI;
using Tunynet.Utilities;
using Spacebuilder.Search;
using Tunynet.Search;
using System;
using DevTrends.MvcDonutCaching;

namespace Spacebuilder.Photo.Controllers
{
    /// <summary>
    /// 相册频道控制器
    /// </summary>
    [TitleFilter(TitlePart = "相册", IsAppendSiteName = true)]
    [Themed(PresentAreaKeysOfBuiltIn.Channel, IsApplication = true)]
    [AnonymousBrowseCheck]
    public class ChannelPhotoController : Controller
    {
        public IPageResourceManager pageResourceManager { get; set; }
        public PhotoService photoService { get; set; }
        public RecommendService recommendService { get; set; }
        public SearchedTermService searchedTermService { get; set; }
        public SearchHistoryService searchHistoryService { get; set; }
        public Authorizer authorizer { get; set; }
        private TagService tagService = new TagService(TenantTypeIds.Instance().Photo());

        #region 照片频道列表

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Home()
        {
            pageResourceManager.InsertTitlePart("相册首页");

            //热门标签
            ViewData["tags"] = HotTags(8);
            PhotoSettings photoSettings = DIContainer.Resolve<ISettingsManager<PhotoSettings>>().Get();
            //获取照片推荐（幻灯片）
            IEnumerable<RecommendItem> recommendPhotoItems = recommendService.GetTops(8, photoSettings.RecommendPhotoTypeId);
            ViewData["recommendPhotoItems"] = recommendPhotoItems;

            //获取推荐相册
            IEnumerable<RecommendItem> recommendAlbumItems = recommendService.GetTops(9, photoSettings.RecommendAlbumTypeId);
            ViewData["recommendAlbumItems"] = recommendAlbumItems;

            return View();
        }

        /// <summary>
        /// 照片排行瀑布流
        /// </summary>
        public ActionResult _Photos(string tagName = null, bool? isEssential = null, SortBy_Photo sortBy = SortBy_Photo.DateCreated_Desc, int pageSize = 20, int pageIndex = 1)
        {
            PagingDataSet<Photo> photos = photoService.GetPhotos(TenantTypeIds.Instance().User(), tagName, isEssential, sortBy, pageSize, pageIndex);
            return View(photos);
        }

        /// <summary>
        /// 照片排行-最新
        /// </summary>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult New()
        {
            pageResourceManager.InsertTitlePart("最新照片");
            return View();
        }

        /// <summary>
        /// 照片排行-精选
        /// </summary>
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Frequently")]
        public ActionResult Essential()
        {
            pageResourceManager.InsertTitlePart("精选照片");
            return View();
        }

        /// <summary>
        /// 照片排行-热点
        /// </summary>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult Hot()
        {
            pageResourceManager.InsertTitlePart("热点照片");
            return View();
        }

        /// <summary>
        /// 照片排行-热评
        /// </summary>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult HotComment()
        {
            pageResourceManager.InsertTitlePart("热评照片");
            return View();
        }

        /// <summary>
        /// 照片排行-喜欢
        /// </summary>  
        /// <returns></returns>
        [DonutOutputCache(CacheProfile = "Stable")]
        public ActionResult Favorite()
        {
            pageResourceManager.InsertTitlePart("大家喜欢的照片");
            return View();
        }

        /// <summary>
        /// 标签下的最新照片
        /// </summary>
        /// <returns></returns>
        public ActionResult TagNew(string tagName)
        {
            //热门标签
            ViewData["tags"] = HotTags(8);

            Tag currentTag = tagService.Get(tagName);

            pageResourceManager.InsertTitlePart("标签[" + tagName + "]的最新照片");
            if (!string.IsNullOrEmpty(currentTag.Description))
            {
                pageResourceManager.SetMetaOfDescription(currentTag.Description);
            }

            return View(currentTag);
        }

        /// <summary>
        /// 标签下的热门照片
        /// </summary>
        /// <returns></returns>
        public ActionResult TagHot(string tagName)
        {
            //热门标签
            ViewData["tags"] = HotTags(8);

            Tag currentTag = tagService.Get(tagName);

            pageResourceManager.InsertTitlePart("标签[" + tagName + "]的热门照片");
            if (!string.IsNullOrEmpty(currentTag.Description))
            {
                pageResourceManager.SetMetaOfDescription(currentTag.Description);
            }

            return View(currentTag);
        }

        /// <summary>
        /// 标签下的精华照片
        /// </summary>
        /// <returns></returns>
        public ActionResult TagEssential(string tagName)
        {
            //热门标签
            ViewData["tags"] = HotTags(8);

            Tag currentTag = tagService.Get(tagName);

            pageResourceManager.InsertTitlePart("标签[" + tagName + "]的精华照片");
            if (!string.IsNullOrEmpty(currentTag.Description))
            {
                pageResourceManager.SetMetaOfDescription(currentTag.Description);
            }

            return View(currentTag);
        }

        /// <summary>
        /// 标签地图
        /// </summary>
        /// <returns></returns>
        public ActionResult Tags()
        {
            pageResourceManager.InsertTitlePart("标签地图");
            return View();
        }

        /// <summary>
        /// 照片详细显示
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail(long photoId)
        {

            Photo photo = photoService.GetPhoto(photoId);

            //更新计数
            CountService countService = new CountService(TenantTypeIds.Instance().Photo());
            countService.ChangeCount(CountTypes.Instance().HitTimes(), photoId, photo.UserId, 1, true);

            Album album = photo.Album;
            if (photo == null || !authorizer.Album_View(album))
            {
                if (Request.IsAjaxRequest())
                    return Json(new StatusMessageData(StatusMessageType.Hint, "没有浏览照片的权限"));
                else
                    return Redirect(SiteUrls.Instance().SystemMessage(TempData, new SystemMessageViewModel
                    {
                        Body = "没有浏览照片的权限",
                        Title = "没有权限",
                        StatusMessageType = StatusMessageType.Hint
                    }));
            }

            IUser currentUser = UserContext.CurrentUser;
            PagingDataSet<Photo> photos = photoService.GetPhotosOfAlbum(null, photo.AlbumId,
                currentUser != null && currentUser.UserId == album.UserId, pageSize: 1000, pageIndex: 1);

            ViewData["album"] = album;
            ViewData["photos"] = photos.ToList();
            if (Request.IsAjaxRequest())
                return PartialView("_Details", photo);
            return View(photo);
        }

        /// <summary>
        /// 照片详细页面侧边
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _DetailPhotoSide(long photoId)
        {
            Photo photo = photoService.GetPhoto(photoId);
            photo.Description = Formatter.FormatMultiLinePlainTextForEdit(photo.Description, false);
            //如果没有读到对象则返回空内容
            if (photo == null)
                return Content(string.Empty);

            Dictionary<int, string> enabledEXIF = new Dictionary<int, string>();
            enabledEXIF.Add(0x010F, "设备制造厂商");
            enabledEXIF.Add(0x0110, "相机型号");
            enabledEXIF.Add(0x9003, "拍摄日期");
            enabledEXIF.Add(0xa002, "宽度");
            enabledEXIF.Add(0xa003, "高度");
            enabledEXIF.Add(0x9205, "最大光圈");
            enabledEXIF.Add(0x920a, "焦距");
            enabledEXIF.Add(0x829A, "曝光时间");
            enabledEXIF.Add(0x8824, "感光度");
            enabledEXIF.Add(0x9209, "闪光灯");

            ViewData["EnabledEXIF"] = enabledEXIF;

            ViewData["EXIFMetaData"] = photoService.GetPhotoEXIFMetaData(photoId);

            ViewData["PhotoLabels"] = photoService.GetLabelsOfPhoto(TenantTypeIds.Instance().User(), photoId);

            CountService countService = new CountService(TenantTypeIds.Instance().Photo());
            ViewData["CommentCount"] = countService.Get(CountTypes.Instance().CommentCount(), photoId);

            return View(photo);
        }

        /// <summary>
        /// 设置照片描述信息
        /// </summary>
        /// <param name="photoId">被设置的id</param>
        /// <param name="description">设置的描述</param>
        /// <returns>是否设置成功</returns>
        [HttpPost]
        public ActionResult _SetPhotoDescription(long photoId, string description)
        {
            Photo photo = photoService.GetPhoto(photoId);

            if (!authorizer.Photo_Edit(photo))
                return Json(new StatusMessageData(StatusMessageType.Error, "您可能没有权限编辑此照片"));

            photo.Description = Formatter.FormatMultiLinePlainTextForStorage(description, false);
            photoService.UpdatePhoto(photo, UserContext.CurrentUser.UserId);
            return Json(new StatusMessageData(StatusMessageType.Success, description));
        }

        #endregion

        #region 频道局部页

        /// <summary>
        /// 标签云图的右边栏的最新照片
        /// </summary>
        public ActionResult _LatestPhotos(int topNumber = 8)
        {
            int pageIndex = 1;
            List<Photo> photoList = new List<Photo>();
            do
            {
                PagingDataSet<Photo> photos = photoService.GetPhotos(TenantTypeIds.Instance().User(), null, null, SortBy_Photo.DateCreated_Desc, pageIndex: pageIndex);
                foreach (var photo in photos)
                {
                    if (authorizer.Photo_Channel(photo.Album))
                    {
                        photoList.Add(photo);
                    }
                }
                pageIndex++;
            } while (photoList.Count() < topNumber);
            return View(photoList.Take(topNumber));
        }

        /// <summary>
        /// 相册瀑布流的分享，喜欢，评论按钮
        /// </summary>
        public ActionResult _ButtonView(long photoId)
        {
            Photo photo = photoService.GetPhoto(photoId);
            ViewData["photo"] = photo;
            ViewData["currentUser"] = UserContext.CurrentUser;
            return View();
        }

        /// <summary>
        /// 相册搜索瀑布流的分享，喜欢，评论按钮
        /// </summary>
        public ActionResult _ButtonViewForSearch(long photoId)
        {
            Photo photo = photoService.GetPhoto(photoId);
            ViewData["photo"] = photo;
            return View();
        }

        #endregion

        #region 照片全文检索

        /// <summary>
        /// 全文检索瀑布流
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult _SearchWaterFall(PhotoFullTextQuery query, int pageIndex = 1, int pageSize = 20)
        {
            query.TenantTypeId = TenantTypeIds.Instance().User();
            query.PageSize = pageSize;
            query.PageIndex = pageIndex;
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null && currentUser.UserId == query.UserId)
            {
                query.IgnoreAuditAndPrivacy = true;

            }

            //调用搜索器进行搜索
            PhotoSearcher photoSearcher = (PhotoSearcher)SearcherFactory.GetSearcher(PhotoSearcher.CODE);
            PagingDataSet<Photo> photos = photoSearcher.Search(query);

            return View(photos);
        }


        /// <summary>
        /// 照片搜索
        /// </summary>
        /// <param name="query">条件</param>
        /// <returns></returns>
        public ActionResult Search(PhotoFullTextQuery query)
        {
            query.Keyword = WebUtility.UrlDecode(query.Keyword);
            query.TenantTypeId = TenantTypeIds.Instance().User();
            query.PageSize = 20;
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser != null)
            {
                if (currentUser.UserId == query.UserId)
                {
                    query.IgnoreAuditAndPrivacy = true;
                }
            }


            ViewData["keyword"] = query.Keyword;
            ViewData["filter"] = query.Filter;
            ViewData["userId"] = query.UserId;

            //调用搜索器进行搜索
            PhotoSearcher photoSearcher = (PhotoSearcher)SearcherFactory.GetSearcher(PhotoSearcher.CODE);
            PagingDataSet<Photo> photos = photoSearcher.Search(query);

            int minus = 0;
            foreach (var photo in photos)
            {
                bool isCurrentUser = true;
                if (currentUser != null)
                {
                    isCurrentUser = query.UserId == currentUser.UserId ? true : photo.AuditStatus == AuditStatus.Success;
                }
                if (!(authorizer.Photo_Search(photo.Album) && isCurrentUser))
                {
                    minus++;
                }
            }
            ViewData["minus"] = minus;

            //添加到用户搜索历史记录

            if (currentUser != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Keyword))
                {
                    searchHistoryService.SearchTerm(currentUser.UserId, PhotoSearcher.CODE, query.Keyword);
                }
            }

            //添加到热词
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                searchedTermService.SearchTerm(PhotoSearcher.CODE, query.Keyword);
            }

            //设置页面Meta
            if (string.IsNullOrWhiteSpace(query.Keyword))
            {
                pageResourceManager.InsertTitlePart("照片搜索");
            }
            else
            {
                pageResourceManager.InsertTitlePart(query.Keyword + "的相关照片");
            }

            return View(photos);
        }

        /// <summary>
        /// 照片全局搜索
        /// </summary>
        /// <param name="query">搜索条件</param>
        /// <param name="topNumber">显示数目</param>
        /// <returns></returns>
        public ActionResult _GlobalSearch(PhotoFullTextQuery query, int topNumber)
        {
            query.TenantTypeId = TenantTypeIds.Instance().User();
            query.PageIndex = 1;
            query.PageSize = 20;
            PhotoSearcher photoSearcher = (PhotoSearcher)SearcherFactory.GetSearcher(PhotoSearcher.CODE);
            PagingDataSet<Photo> photos = photos = photoSearcher.Search(query);

            List<Photo> list = new List<Photo>();
            foreach (var item in photos)
            {
                if (authorizer.Photo_Search(item.Album))
                {
                    list.Add(item);
                }
            }
            ViewData["photoSearcherUrl"] = photoSearcher.PageSearchActionUrl(query.Keyword);
            ViewData["total"] = photos.TotalRecords;
            ViewData["queryDuration"] = photos.QueryDuration;
            return View(list.Take(topNumber));
        }

        /// <summary>
        /// 快捷搜索
        /// </summary>
        /// <param name="query"></param>
        /// <param name="topNumber"></param>
        /// <returns></returns>
        public ActionResult _QuickSearch(PhotoFullTextQuery query, int topNumber)
        {
            query.TenantTypeId = TenantTypeIds.Instance().User();
            query.PageSize = 20;
            query.Keyword = WebUtility.HtmlDecode(query.Keyword);
            query.Filter = PhotoSearchRange.DESCRIPTION;

            PhotoSearcher photoSearcher = (PhotoSearcher)SearcherFactory.GetSearcher(PhotoSearcher.CODE);
            PagingDataSet<Photo> photos = photoSearcher.Search(query);

            List<Photo> list = new List<Photo>();
            foreach (var item in photos)
            {
                if (authorizer.Photo_Search(item.Album))
                {
                    list.Add(item);
                }
            }
            ViewData["total"] = photos.TotalRecords;
            ViewData["photoSearcherUrl"] = photoSearcher.PageSearchActionUrl(query.Keyword);

            return View(list.Take(topNumber));
        }

        /// <summary>
        /// 搜索自动完成
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <param name="topNumber">数目</param>
        /// <returns></returns>
        public JsonResult SearchAutoComplete(string keyword, int topNumber)
        {
            PhotoSearcher photoSearcher = (PhotoSearcher)SearcherFactory.GetSearcher(PhotoSearcher.CODE);
            IEnumerable<string> terms = photoSearcher.AutoCompleteSearch(keyword, topNumber);
            return Json(terms.Select(t => new { tagName = t, tagNameWithHighlight = SearchEngine.Highlight(keyword, string.Join("", t.Take(34)), 100) }), JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region 相册中的圈人

        /// <summary>
        /// 创建圈人
        /// </summary>
        /// <param name="userId">被圈的用户</param>
        /// <param name="height">圈的高度</param>
        /// <param name="width">圈的宽度</param>
        /// <param name="left">左边距</param>
        /// <param name="top">上边距</param>
        /// <param name="photoId">照片的id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreatPhotoLabel(string userId, double height, double width, double left, double top, long photoId)
        {
            Photo photo = photoService.GetPhoto(photoId);

            if (!authorizer.PhotoLabel_Creat(photo))
                return Json(new StatusMessageData(StatusMessageType.Error, "您可能没有权限在这张照片上圈人"));

            long userIdLong = 0;
            string[] listUserId = userId.Split(new char[] { ',', '，' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (listUserId == null || listUserId.Length == 0)
            {
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到用户"));
            }

            foreach (var item in listUserId)
                if (long.TryParse(item, out userIdLong))
                    break;

            IUser user = DIContainer.Resolve<UserService>().GetUser(userIdLong);
            if (user == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "找不到用户"));

            PhotoLabel label = PhotoLabel.New();
            label.AreaHeight = (int)height;
            label.AreaWidth = (int)width;
            label.AreaX = (int)left;
            label.AreaY = (int)top;
            label.ObjetId = userIdLong;
            label.ObjectName = user.DisplayName;
            label.TenantTypeId = TenantTypeIds.Instance().User();
            label.PhotoId = photoId;
            label.Description = string.Empty;
            label.UserId = UserContext.CurrentUser.UserId;

            bool isCreat = photoService.CreateLabel(label);
            if (isCreat)
                return Json(new { messageType = StatusMessageType.Success, id = label.LabelId });
            return Json(new StatusMessageData(StatusMessageType.Error, "创建失败"));
        }

        /// <summary>
        /// 获取照片圈人
        /// </summary>
        /// <param name="labelId">圈人</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetPhotoLabel(long labelId)
        {
            PhotoLabel label = photoService.GetLabel(labelId);
            if (label == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "没有找到圈人信息"), JsonRequestBehavior.AllowGet);

            return Json(label.AsPhotoEditModel(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取照片下的圈人信息
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <param name="tenantTypeId">租户类型id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetLabelsOfPhoto(long photoId, string tenantTypeId = null)
        {
            if (string.IsNullOrEmpty(tenantTypeId))
                tenantTypeId = TenantTypeIds.Instance().User();
            return Json(photoService.GetLabelsOfPhoto(tenantTypeId, photoId).Select(n => n.AsPhotoEditModel()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除照片圈人信息
        /// </summary>
        /// <param name="labelId">圈人的id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeletePhotoLabel(long labelId)
        {
            PhotoLabel label = photoService.GetLabel(labelId);
            if (!authorizer.PhotoLabel_Delete(label))
                return Json(new StatusMessageData(StatusMessageType.Error, "您可能没有权限删除此圈人"));

            photoService.DeleteLabel(label);

            return Json(new StatusMessageData(StatusMessageType.Success, "删除成功"));
        }

        #endregion

        #region 标签


        /// <summary>
        /// 贴标签
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <param name="tags">标签</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _SetPhotoTag(long photoId)
        {
            string tags = Request.Form.Get<string>("tags", null);
            ISettingsManager<TagSettings> tagSettingsManager = DIContainer.Resolve<ISettingsManager<TagSettings>>();
            TagSettings tagSettings = tagSettingsManager.Get();
            if (tags != null && tags.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries).Count() > tagSettings.MaxTagsCount)
                return Json(new StatusMessageData(StatusMessageType.Error, "由于标签超过设置，所以贴标签失败"));

            Photo photo = photoService.GetPhoto(photoId);

            tagService.ClearTagsFromItem(photoId, photo.UserId);
            tagService.AddTagsToItem(tags, photo.UserId, photo.PhotoId);

            return Json(new StatusMessageData(StatusMessageType.Success, "贴标签成功"));
        }

        /// <summary>
        /// 照片详细显示页面侧边的标签
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _DetailPhotoSideTag(long photoId)
        {
            Photo photo = photoService.GetPhoto(photoId);

            IEnumerable<Tag> tags = new TagService(TenantTypeIds.Instance().Photo()).GetTopTagsOfItem(photoId, 20);
            if (tags == null || tags.Count() == 0 || photo == null)
                return Json(new List<string>(), JsonRequestBehavior.AllowGet);

            return Json(tags.Select(n =>
            {
                return new
                {
                    TagName = n.TagName,
                    link = SiteUrls.Instance().PhotosInTag(photo.User.UserName, n.TagName)
                };
            }), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 热门标签
        /// </summary>
        private List<Tag> HotTags(int topNumber)
        {
            List<Tag> tags = new List<Tag>();
            PhotoSettings photoSettings = DIContainer.Resolve<ISettingsManager<PhotoSettings>>().Get();
            //获取推荐标签
            IEnumerable<RecommendItem> recommendTagItems = recommendService.GetTops(topNumber, photoSettings.RecommendTagTypeId);
            tags.AddRange(recommendTagItems.Select(n => n.GetTag()));

            if (recommendTagItems.Count() < topNumber)
            {
                //获取特色标签
                IEnumerable<Tag> isFeaturedTags = tagService.GetTopTags(topNumber, true, SortBy_Tag.ItemCountDesc).Except(tags);
                tags.AddRange(isFeaturedTags);

                //获取普通标签
                if ((recommendTagItems.Count() + isFeaturedTags.Count()) < topNumber)
                {
                    IEnumerable<Tag> commonTags = tagService.GetTopTags(topNumber, false, SortBy_Tag.ItemCountDesc).Except(tags);
                    tags.AddRange(commonTags);
                }
            }
            return tags.Take(topNumber).ToList();
        }

        #endregion

        #region 异步操作

        /// <summary>
        /// 设置精华
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <param name="isEssential">是否精华</param>
        /// <returns>设置精华</returns>
        [HttpPost]
        public ActionResult _SetEssential(long photoId, bool isEssential = true)
        {
            Photo photo = photoService.GetPhoto(photoId);
            if (photo == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "没有找到照片"));

            if (!authorizer.IsAdministrator(PhotoConfig.Instance().ApplicationId))
                return Json(new StatusMessageData(StatusMessageType.Error, "您可能没有权限给此照片加精"));

            photoService.SetEssential(photo, isEssential);
            return Json(new StatusMessageData(StatusMessageType.Success, isEssential ? "加精成功" : "取消精华成功"));
        }

        /// <summary>
        /// 设置标题图
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <param name="isCover">是否是标题图</param>
        /// <returns>设置标题图</returns>
        [HttpPost]
        public ActionResult _SetAlbumCover(long photoId, bool isCover = true)
        {
            Photo photo = photoService.GetPhoto(photoId);

            if (photo == null)
                return Json(new StatusMessageData(StatusMessageType.Error, "设置标题图失败"));

            if (!authorizer.Album_Edit(photo.Album))
                return Json(new StatusMessageData(StatusMessageType.Error, "您可能没有权限设置标题图"));

            photoService.SetCover(photo, isCover);

            return Json(new StatusMessageData(StatusMessageType.Success, isCover ? "设置标题图成功" : "取消标题图成功"));
        }

        /// <summary>
        /// 获取一张照片
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _GetOnePhoto(long photoId)
        {
            Photo photo = photoService.GetPhoto(photoId);
            if (!authorizer.Photo_View(photo))
                return Json(null, JsonRequestBehavior.AllowGet);

            if (photo == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                photoId = photo.PhotoId.ToString(),
                photoUrl = SiteUrls.Instance().ImageUrl(photo.RelativePath, TenantTypeIds.Instance().Photo(), ImageSizeTypeKeys.Instance().Original()),
                dateCreated = photo.DateCreated.ToFriendlyDate(),
                isEssential = photo.IsEssential,
                isCover = photo.Album.CoverId == photo.PhotoId
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 照片详细显示页面

        /// <summary>
        /// 照片详细显示页面的删除
        /// </summary>
        /// <param name="photoId">照片id</param>
        /// <returns>照片详细显示页面的删除</returns>
        [HttpPost]
        public ActionResult _DeleteInDetail(long photoId)
        {
            Photo photo = photoService.GetPhoto(photoId);
            if (!authorizer.Photo_Edit(photo))
                return Json(new StatusMessageData(StatusMessageType.Error, "您可能没有权限删除此照片"));

            string url = SiteUrls.Instance().AlbumDetailList(photo.User.UserName, photo.AlbumId);

            photoService.DeletePhoto(photo);
            return Json(new StatusMessageData(StatusMessageType.Success, url));
        }

        #endregion

        #region 站外采集

        /// <summary>
        /// 采集站外照片页
        /// </summary>
        /// <param name="imgUrls">站外照片链接</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult _CaptureOutSitePhoto(string imgUrls)
        {
            IUser user = UserContext.CurrentUser;
            if (user == null)
            {
                return Redirect(SiteUrls.Instance().Login(true));
            }
            pageResourceManager.InsertTitlePart("采集照片");
            IEnumerable<Album> albums = photoService.GetUserAlbums(TenantTypeIds.Instance().User(), user.UserId, true, SortBy_Album.DisplayOrder, 10000, 1).AsEnumerable<Album>();
            SelectList albumList = new SelectList(albums.Select(n => new { text = n.AlbumName, value = n.AlbumId }), "value", "text");
            ViewData["albumList"] = albumList;
            ViewData["imgUrls"] = imgUrls;
            return View();
        }

        /// <summary>
        /// 采集站外照片
        /// </summary>
        /// <param name="imgUrls">站外照片链接</param>
        /// <param name="albumId">选择的相册</param>
        /// <param name="description">照片描述</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _CaptureOutSitePhoto(string imgUrls, long albumId, string description)
        {
            Album album = photoService.GetAlbum(albumId);
            Photo photo = Photo.New();
            string[] urls = imgUrls.Split('^');
            foreach (var url in urls)
            {
                photo.AlbumId = albumId;
                photo.TenantTypeId = album.TenantTypeId;
                photo.OwnerId = album.OwnerId;
                photo.UserId = album.UserId;
                photo.Author = album.Author;
                photo.OriginalUrl = url;
                photo.RelativePath = string.Empty;
                photo.Description = description;
                photo.AuditStatus = AuditStatus.Pending;
                photo.PrivacyStatus = album.PrivacyStatus;
                photo.IsEssential = false;
                photoService.CreatePhoto(photo, null);
            }
            return Json("1");
        }

        #endregion
    }
}