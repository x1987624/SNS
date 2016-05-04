DELETE FROM tn_themes WHERE Id='Home,Default';
INSERT INTO tn_themes(Id,PresentAreaKey,ThemeKey,Parent,Version)
VALUES('Home,Default','Home','Default','Default','2.0');

DELETE FROM tn_PresentAreas WHERE PresentAreaKey='Home';
INSERT INTO tn_PresentAreas(PresentAreaKey,AllowMultipleInstances,EnableThemes,DefaultAppearanceId,ThemeLocation)
VALUES('Home',1,0,'Home,Default,Default','~/Themes/Home/');

DELETE FROM tn_ThemeAppearances WHERE Id='Home,Default,Default';
INSERT INTO tn_ThemeAppearances(Id,PresentAreaKey,ThemeKey,AppearanceKey,Name,PreviewImage,PreviewLargeImage,
LogoFileName,Description,Tags,Author,Copyright,LastModified,Version,ForProductVersion,DateCreated,IsEnabled,
DisplayOrder,UserCount,Roles,RequiredRank)
VALUES('Home,Default,Default','Home','Default','Default','Default','Preview.png','Preview.png',
'','','','admin','heren','2015-12-21','2.0','2.0','2016-05-03',1,
22,0,'',0);

-- 1、执行脚本 2、在Common的UI下新增HomeThemeResolver 3、在Starter里新增ThemeService.RegisterThemeResolver("Home", new HomeThemeResolver());
