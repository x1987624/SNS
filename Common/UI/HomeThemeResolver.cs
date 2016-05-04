// ***********************************************************************
// Author           : 肖晏
// Created          : 12-21-2015
// ***********************************************************************
// <copyright file="HomeThemeResolver.cs" company="xyan">
//     Copyright (c) xyan.
// </copyright>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tunynet.UI;
using System.Web.Routing;
using Tunynet.Common;
using Tunynet;
using Spacebuilder.Common;

namespace Spacebuilder.UI
{
    /// <summary>
    /// Class HomeThemeResolver.
    /// </summary>
    public class HomeThemeResolver : IThemeResolver
    {
        #region IThemeSelector 成员

        /// <summary>
        /// 获取请求页面使用的皮肤
        /// </summary>
        /// <param name="controllerContext"><see cref="RequestContext" /></param>
        /// <author version=""></author>
        /// <modifier version=""></modifier>
        /// <returns>ThemeAppearance.</returns>
        public ThemeAppearance GetRequestTheme(RequestContext controllerContext)
        {
            string themeKey = null;
            string appearanceKey = null;
            SiteSettings siteSettings = DIContainer.Resolve<ISettingsManager<SiteSettings>>().Get();
            if (!string.IsNullOrEmpty(siteSettings.SiteTheme) && !string.IsNullOrEmpty(siteSettings.SiteThemeAppearance))
            {
                themeKey = siteSettings.SiteTheme;
                appearanceKey = siteSettings.SiteThemeAppearance;
            }
            else
            {
                PresentArea pa = new PresentAreaService().Get(PresentAreaKeysOfBuiltIn.Home);
                if (pa != null)
                {
                    themeKey = pa.DefaultThemeKey;
                    appearanceKey = pa.DefaultAppearanceKey;
                }
            }
            return new ThemeService().GetThemeAppearance(PresentAreaKeysOfBuiltIn.Home, themeKey, appearanceKey);
        }

        /// <summary>
        /// 加载皮肤的css
        /// </summary>
        /// <param name="controllerContext"><see cref="RequestContext" /></param>
        /// <author version=""></author>
        /// <modifier version=""></modifier>
        public void IncludeStyle(RequestContext controllerContext)
        {
            ThemeAppearance themeAppearance = GetRequestTheme(controllerContext);
            if (themeAppearance == null)
                return;

            PresentArea presentArea = new PresentAreaService().Get(themeAppearance.PresentAreaKey);
            if (presentArea == null)
                return;

            string themeCssPath = string.Format("{0}/{1}/theme.css", presentArea.ThemeLocation, themeAppearance.ThemeKey);
            //string appearanceCssPath = string.Format("{0}/{1}/Appearances/{2}/appearance.css", presentArea.ThemeLocation, themeAppearance.ThemeKey, themeAppearance.AppearanceKey);

            IPageResourceManager resourceManager = DIContainer.ResolvePerHttpRequest<IPageResourceManager>();
            // resourceManager.IncludeStyle(themeCssPath);
            //resourceManager.IncludeStyle(appearanceCssPath);
        }

        /// <summary>
        /// 验证当前用户是否修改皮肤的权限
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        public bool Validate(long ownerId)
        {
            IUser currentUser = UserContext.CurrentUser;
            if (currentUser == null)
                return false;
            if (currentUser.IsSuperAdministrator() || currentUser.IsContentAdministrator())
                return true;
            return false;
        }

        /// <summary>
        /// 获取拥有者当前选中的皮肤
        /// </summary>
        /// <param name="ownerId">拥有者Id（如：用户Id、群组Id）</param>
        /// <returns></returns>
        public string GetThemeAppearance(long ownerId)
        {
            SiteSettings siteSettings = DIContainer.Resolve<ISettingsManager<SiteSettings>>().Get();
            return siteSettings.SiteTheme + "," + siteSettings.SiteThemeAppearance;
        }

        /// <summary>
        /// 更新皮肤
        /// </summary>
        /// <param name="ownerId">拥有者Id（如：用户Id、群组Id）</param>
        /// <param name="isUseCustomStyle">是否使用自定义皮肤</param>
        /// <param name="themeAppearance">themeKey与appearanceKey用逗号关联</param>
        public void ChangeThemeAppearance(long ownerId, bool isUseCustomStyle, string themeAppearance)
        {
            SiteSettings siteSettings = DIContainer.Resolve<ISettingsManager<SiteSettings>>().Get();
            string themeKey = null;
            string appearanceKey = null;
            string[] themeAppearanceArray = themeAppearance.Split(',');
            if (themeAppearanceArray.Count() == 2)
            {
                themeKey = themeAppearanceArray[0];
                appearanceKey = themeAppearanceArray[1];
            }
            else
            {
                PresentArea pa = new PresentAreaService().Get(PresentAreaKeysOfBuiltIn.Home);
                if (pa != null)
                {
                    themeKey = pa.DefaultThemeKey;
                    appearanceKey = pa.DefaultAppearanceKey;
                }
            }
            siteSettings.SiteTheme = themeKey;
            siteSettings.SiteThemeAppearance = appearanceKey;
            DIContainer.Resolve<ISettingsManager<SiteSettings>>().Save(siteSettings);
        }
        #endregion
    }
}
