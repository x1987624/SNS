//------------------------------------------------------------------------------
// <copyright company="Tunynet">
//     Copyright (c) Tunynet Inc.  All rights reserved.
// </copyright> 
//------------------------------------------------------------------------------

using System.Configuration;
using System.Web.Mvc;
using Spacebuilder.Photo.Controllers;
using Tunynet.Common;

namespace Spacebuilder.Photo
{
    /// <summary>
    /// 问答路由设置
    /// </summary>
    public class UrlRoutingRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Photo"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //对于IIS6.0默认配置不支持无扩展名的url
            string extensionForOldIIS = ".html";
            int iisVersion = 0;

            if (!int.TryParse(ConfigurationManager.AppSettings["IISVersion"], out iisVersion))
                iisVersion = 7;
            if (iisVersion >= 7)
                extensionForOldIIS = string.Empty;

            #region 动态
            context.MapRoute(
               string.Format("ActivityDetail_{0}_CreatePhoto", TenantTypeIds.Instance().Album()), // Route name
                "PhotoActivity/CreatePhoto/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "PhotoActivity", action = "_CreatePhoto" } // Parameter defaults
            );

            context.MapRoute(
               string.Format("ActivityDetail_{0}_CommentPhoto", TenantTypeIds.Instance().Comment()), // Route name
                "PhotoActivity/CommentPhoto/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "PhotoActivity", action = "_CommentPhoto" } // Parameter defaults
            );

            context.MapRoute(
               string.Format("ActivityDetail_{0}_LabelPhoto", TenantTypeIds.Instance().PhotoLabel()), // Route name
                "PhotoActivity/LabelPhoto/{ActivityId}" + extensionForOldIIS, // URL with parameters
                new { controller = "PhotoActivity", action = "_CreatePhotoLabel" } // Parameter defaults
            );
            #endregion

            #region Channel

            //相册首页
            context.MapRoute(
              "Channel_Photo_Home", // Route name
              "Photo" + extensionForOldIIS, // URL with parameters
              new { controller = "ChannelPhoto", action = "Home", CurrentNavigationId = "10100302" } // Parameter defaults
            );

            //照片排行-最新
            context.MapRoute(
                "Channel_Photo_New", // Route name
                "Photo/New" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "New", CurrentNavigationId = "10100303" } // Parameter defaults
            );

            //照片排行-精选
            context.MapRoute(
                "Channel_Photo_Essential", // Route name
                "Photo/Essential" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "Essential", CurrentNavigationId = "10100303" } // Parameter defaults
            );

            //照片排行-热点
            context.MapRoute(
                "Channel_Photo_Hot", // Route name
                "Photo/Hot" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "Hot", CurrentNavigationId = "10100303" } // Parameter defaults
            );

            //照片排行-热评
            context.MapRoute(
                "Channel_Photo_HotComment", // Route name
                "Photo/HotComment" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "HotComment", CurrentNavigationId = "10100303" } // Parameter defaults
            );

            //照片排行-喜欢
            context.MapRoute(
                "Channel_Photo_Favorite", // Route name
                "Photo/Favorite" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "Favorite", CurrentNavigationId = "10100303" } // Parameter defaults
            );

            //标签下的照片
            context.MapRoute(
                "Channel_Photo_Tag", // Route name
                "Photo/t-{tagName}" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "Tag", CurrentNavigationId = "10100302" } // Parameter defaults
            );

            //照片详细显示
            context.MapRoute(
                "Channel_Photo_Detail", // Route name
                "Photo/p-{photoId}" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "Detail", CurrentNavigationId = "10100302" }, // Parameter defaults
                new { photoId = @"(\d+)|(\{\d+\})" }
            );

            //标签下的最新照片
            context.MapRoute(
                "Channel_Photo_TagNew", // Route name
                "Photo/tn-{tagName}" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "TagNew", CurrentNavigationId = "10100301" }, // Parameter defaults
                new { tagName = @"(\S+)|(\{\S+\})" }
            );

            //标签下的热门照片
            context.MapRoute(
                "Channel_Photo_TagHot", // Route name
                "Photo/th-{tagName}" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "TagHot", CurrentNavigationId = "10100301" }, // Parameter defaults
                new { tagName = @"(\S+)|(\{\S+\})" }
            );

            //标签下的精华照片
            context.MapRoute(
                "Channel_Photo_TagEssential", // Route name
                "Photo/te-{tagName}" + extensionForOldIIS, // URL with parameters
                new { controller = "ChannelPhoto", action = "TagEssential", CurrentNavigationId = "10100301" }, // Parameter defaults
                new { tagName = @"(\S+)|(\{\S+\})" }
            );

            //相册在频道下的其它页面
            context.MapRoute(
                "Channel_Photo_Common", // Route name
                "Photo/{action}" + extensionForOldIIS, // URL with parame ters
                new { controller = "ChannelPhoto" } // Parameter defaults
            );

            #endregion

            #region UserSpace

            //相册首页
            context.MapRoute(
                "UserSpace_Photo_Home", // Route name
                "u/{SpaceKey}/PhotoHome" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePhoto", action = "Home", CurrentNavigationId = "11100302" } // Parameter defaults
            );

            //相册首页
            context.MapRoute(
                "ApplicationCount_Photo", // Route name
                "u/{SpaceKey}/PhotoAlbums" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePhoto", action = "Albums", CurrentNavigationId = "11100304" } // Parameter defaults
            );

            //最新照片
            context.MapRoute(
                "UserSpace_Photo_Photos", // Route name
                "u/{SpaceKey}/Photos" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePhoto", action = "Photos", CurrentNavigationId = "11100303" } // Parameter defaults
            );

            //相册列表
            context.MapRoute(
                "UserSpace_Photo_Albums", // Route name
                "u/{SpaceKey}/PhotoAlbums" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePhoto", action = "Albums", CurrentNavigationId = "11100304" } // Parameter defaults
            );

            //我的喜欢
            context.MapRoute(
                "UserSpace_Photo_Favorites", // Route name
                "u/{SpaceKey}/PhotoFavorites" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePhoto", action = "Favorites", CurrentNavigationId = "11100305" } // Parameter defaults
            );

            //上传照片按钮
            context.MapRoute(
                "UserSpace_Photo_Upload", // Route name
                "u/{SpaceKey}/PhotoUpload" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePhoto", action = "Upload", CurrentNavigationId = "11100301" } // Parameter defaults
            );

            //用户空间-标签详情页
            context.MapRoute(
              "UserSpace_Photo_Tag", // Route name
              "u/{SpaceKey}/PhotoTag-{tagName}" + extensionForOldIIS, // URL with parameters
              new { controller = "UserSpacePhoto", action = "Tag", CurrentNavigationId = "11100301" } // Parameter defaults
            );

            //用户空间-相册详情页-列表模式
            context.MapRoute(
              "UserSpace_Photo_AlbumDetailList", // Route name
              "u/{SpaceKey}/PhotoAlbum-{albumId}" + extensionForOldIIS, // URL with parameters
              new { controller = "UserSpacePhoto", action = "AlbumDetailList", CurrentNavigationId = "11100301" }, // Parameter defaults
              new { albumId = @"(\d+)|(\{\d+\})" }
            );

            //用户空间-相册详情页-阅读模式
            context.MapRoute(
              "UserSpace_Photo_AlbumDetailView", // Route name
              "u/{SpaceKey}/PhotoAlbumView-{albumId}" + extensionForOldIIS, // URL with parameters
              new { controller = "UserSpacePhoto", action = "AlbumDetailView", CurrentNavigationId = "11100301" }, // Parameter defaults
             new { albumId = @"(\d+)|(\{\d+\})" }
             );

            //用户空间-照片管理
            context.MapRoute(
              "UserSpace_Photo_PhotoManage", // Route name
              "u/{SpaceKey}/PhotoManage/{albumId}" + extensionForOldIIS, // URL with parameters
              new { controller = "UserSpacePhoto", action = "PhotoManage", CurrentNavigationId = "11100301" }, // Parameter defaults
              new { albumId = @"(\d+)|(\{\d+\})" }
            );

            //用户空间-他的相册
            context.MapRoute(
                "UserSpace_Photo_Ta", // Route name
                "u/{SpaceKey}/Photo" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePhoto", action = "Ta", CurrentNavigationId = "11100301" } // Parameter defaults
            );

            //相册在用户空间下的其它页面
            context.MapRoute(
                "UserSpace_Photo_Common", // Route name
                "u/{SpaceKey}/Photo/{action}" + extensionForOldIIS, // URL with parameters
                new { controller = "UserSpacePhoto" } // Parameter defaults
            );

            #endregion

            #region ControlPanel

            context.MapRoute(
                "ControlPanel_Photo_Home", // Route name
                "ControlPanel/Content/Photo/ManagePhotos" + extensionForOldIIS, // URL with parameters
                new { controller = "ControlPanelPhoto", action = "ManagePhotos", CurrentNavigationId = "20100301" } // Parameter defaults
            );

            context.MapRoute(
                "ControlPanel_Photo_Common", // Route name
                "ControlPanel/Content/Photo/{action}" + extensionForOldIIS, // URL with parameters
                new { controller = "ControlPanelPhoto", CurrentNavigationId = "20100301" } // Parameter defaults
            );

            #endregion

        }
    }
}