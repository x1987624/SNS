-----应用
DELETE FROM [dbo].[tn_Applications] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_Applications] ([ApplicationId], [ApplicationKey], [Description], [IsEnabled], [IsLocked], [DisplayOrder]) VALUES (1003, N'Photo', N'相册应用', 1, 0, 1003)

-----快捷操作
DELETE FROM [dbo].[tn_ApplicationManagementOperations] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_ApplicationManagementOperations] ([OperationId], [ApplicationId], [AssociatedNavigationId], [PresentAreaKey], [OperationType], [OperationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10100301, 1003, 0, N'Channel', 1, N'上传照片', N'', N'', N'UserSpace_Photo_Upload', N'spaceKey', N'Upload', NULL, N'_blank', 10100301, 0, 0, 1)
INSERT [dbo].[tn_ApplicationManagementOperations] ([OperationId], [ApplicationId], [AssociatedNavigationId], [PresentAreaKey], [OperationType], [OperationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11100301, 1003, 0, N'UserSpace', 1, N'上传照片', N'', N'', N'UserSpace_Photo_Upload', NULL, N'Upload', NULL, N'_blank', 11100301, 0, 1, 1)

-----应用在呈现区域的设置
DELETE FROM [dbo].[tn_ApplicationInPresentAreaSettings] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (1003, N'UserSpace', 0, 1, 1)
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (1003, N'Channel', 0, 1, 0)

-----默认安装记录
DELETE FROM [dbo].[tn_ApplicationInPresentAreaInstallations] WHERE [ApplicationId] = 1003 and OwnerId = 0
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) VALUES (0, 1003, 'Channel')

-----租户类型
DELETE FROM [dbo].[tn_TenantTypes] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'100300', 1003, N'相册应用', N'')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'100301', 1003, N'相册', N'Spacebuilder.Photo.Album,Spacebuilder.Photo')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'100302', 1003, N'照片', N'Spacebuilder.Photo.Photo,Spacebuilder.Photo')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'100303', 1003, N'圈人', N'Spacebuilder.Photo.PhotoLabel,Spacebuilder.Photo')

-----租户使用的服务
DELETE FROM [dbo].[tn_TenantTypesInServices] WHERE [TenantTypeId] in(N'100301',N'100302')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'100301', N'Recommend')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'100302', N'Attitude')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'100302', N'Commend')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'100302', N'Comment')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'100302', N'Notice')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'100302', N'Recommend')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'100302', N'Tag')

-----动态
DELETE FROM  [dbo].[tn_ActivityItems] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CreatePhoto', 1003, N'发布照片', 1, N'', 0, 1, 1)
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CommentPhoto', 1003, N'评论照片', 2, N'', 0, 1, 1)
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'LabelPhoto', 1003, N'照片圈人', 3, N'', 0, 1, 0)

-----用户角色
DELETE FROM [dbo].[tn_Roles] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_Roles] ([RoleName], [FriendlyRoleName], [IsBuiltIn], [ConnectToUser], [ApplicationId], [IsPublic], [Description], [IsEnabled], [RoleImage]) VALUES (N'PhotoAdministrator', N'相册管理员', 1, 1, 1003, 1, N'管理相册应用下的内容', 1, N'')

-----权限
DELETE FROM [dbo].[tn_PermissionItems] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_PermissionItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [EnableQuota], [EnableScope]) VALUES (N'Photo_Create', 1003, N'上传照片', 3, 0, 0)
-----角色针对权限的设置
DELETE FROM [dbo].[tn_PermissionItemsInUserRoles] WHERE [ItemKey] = N'Photo_Create' and [RoleName] = N'RegisteredUsers'
INSERT [dbo].[tn_PermissionItemsInUserRoles] ([RoleName], [ItemKey], [PermissionType], [PermissionQuota], [PermissionScope], [IsLocked]) VALUES ( N'RegisteredUsers', N'Photo_Create', 1, 0, 0, 0)

-----审核
DELETE FROM [dbo].[tn_AuditItems] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_AuditItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description]) VALUES (N'Album', 1003, N'创建相册', 9, N'')
INSERT [dbo].[tn_AuditItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description]) VALUES (N'Photo', 1003, N'上传照片', 10, N'')

-----积分
DELETE FROM [dbo].[tn_PointItems] WHERE [ApplicationId]=1003
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Photo_BeLabelled', 1003, N'照片被圈', 142, 0, 2, 0, 0, 0, 0, N'',0)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Photo_BeLabelled_Delete', 1003, N'删除被圈照片', 143, 0, -2, 0, 0, 0, 0, N'',0)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Photo_BeLiked', 1003, N'照片被喜欢', 141, 0, 2, 0, 0, 0, 0, N'',0)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Photo_DeletePhoto', 1003, N'删除照片', 144, 0, -2, 0, 0, 0, 0, N'',1)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Photo_UploadPhoto', 1003, N'上传照片', 145, 0, 2, 0, 0, 0, 0, N'',1)

-----初始化导航菜单
DELETE FROM [dbo].[tn_InitialNavigations] WHERE [ApplicationId] = 1003
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10100301, 0, 0, N'Channel', 1003, 0, N'相册', N' ', N' ', N'Channel_Photo_Home', NULL, N'Album', NULL, N'_self', 10100301, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10100302, 10100301, 1, N'Channel', 1003, 0, N'相册首页', N' ', N' ', N'Channel_Photo_Home', NULL, NULL, NULL, N'_self', 10100302, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10100303, 10100301, 1, N'Channel', 1003, 0, N'照片排行', N'', N'', N'Channel_Photo_New', NULL, NULL, NULL, N'_self', 10100303, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10100304, 10100301, 1, N'Channel', 1003, 0, N'我的相册', N'', N'', N'UserSpace_Photo_Home', N'spaceKey', NULL, NULL, N'_self', 10100304, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11100301, 0, 0, N'UserSpace', 1003, 0, N'相册', N' ', N' ', N'UserSpace_Photo_Home', NULL, N'Album', NULL, N'_self', 11100301, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11100302, 11100301, 1, N'UserSpace', 1003, 0, N'相册首页', N' ', N' ', N'UserSpace_Photo_Home', NULL, NULL, NULL, N'_self', 11100302, 1, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11100303, 11100301, 1, N'UserSpace', 1003, 0, N'最新照片', N' ', N' ', N'UserSpace_Photo_Photos', NULL, NULL, NULL, N'_self', 11100303, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11100304, 11100301, 1, N'UserSpace', 1003, 0, N'相册列表', N' ', N' ', N'UserSpace_Photo_Albums', NULL, NULL, NULL, N'_self', 11100304, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11100305, 11100301, 1, N'UserSpace', 1003, 0, N'我的喜欢', N' ', N' ', N'UserSpace_Photo_Favorites', NULL, NULL, NULL, N'_self', 11100305, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (20100301, 20000011, 2, N'ControlPanel', 1003, 0, N'相册', N' ', N' ', N'ControlPanel_Photo_Home', NULL, NULL, NULL, N'_self', 20100301, 0, 0, 1)

-----推荐
DELETE FROM [dbo].[tn_RecommendItemTypes] WHERE [TenantTypeId] in ('100301','100302')
INSERT [dbo].[tn_RecommendItemTypes] ([TypeId], [TenantTypeId], [Name], [Description], [HasFeaturedImage], [DateCreated]) VALUES (N'10030101', N'100301', N'推荐相册', N'推荐相册', 0, GETDATE())
INSERT [dbo].[tn_RecommendItemTypes] ([TypeId], [TenantTypeId], [Name], [Description], [HasFeaturedImage], [DateCreated]) VALUES (N'10030201', N'100302', N'推荐照片', N'推荐照片', 0, GETDATE())
INSERT [dbo].[tn_RecommendItemTypes] ([TypeId], [TenantTypeId], [Name], [Description], [HasFeaturedImage], [DateCreated]) VALUES (N'10030202', N'100302', N'推荐标签', N'推荐相册下的标签', 0, GETDATE())

-----广告位
DELETE FROM [dbo].[tn_AdvertisingPosition] WHERE [PositionId] like '101003%'
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'10100300001', N'Channel', N'相册频道首页中部广告位(950x170)', N'AdvertisingPosition\00001\01003\00001\10100300001.jpg', 950, 170, 1)

-----自动为已注册用户安装相册应用
-----1.安装导航
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 10100304, 10100301, 1, N'Channel', 1003,UserId, 0, N'我的相册', N'', N'', N'UserSpace_Photo_Home', N'spaceKey', NULL, NULL, N'_self', 10100304, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003)
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11100301, 0, 0, N'UserSpace', 1003,UserId, 0, N'相册', N' ', N' ', N'UserSpace_Photo_Home', NULL, N'Album', NULL, N'_self', 11100301, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003)

INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11100302, 11100301, 1, N'UserSpace', 1003,UserId, 0, N'相册首页', N' ', N' ', N'UserSpace_Photo_Home', NULL, NULL, NULL, N'_self', 11100302, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003)
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11100303, 11100301, 1, N'UserSpace', 1003,UserId, 0, N'最新照片', N' ', N' ', N'UserSpace_Photo_Photos', NULL, NULL, NULL, N'_self', 11100303, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003)
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11100304, 11100301, 1, N'UserSpace', 1003,UserId, 0, N'相册列表', N' ', N' ', N'UserSpace_Photo_Albums', NULL, NULL, NULL, N'_self', 11100304, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003)
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11100305, 11100301, 1, N'UserSpace', 1003,UserId, 0, N'我的喜欢', N' ', N' ', N'UserSpace_Photo_Favorites', NULL, NULL, NULL, N'_self', 11100305, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003)
-----2.创建默认相册
INSERT [dbo].[spb_Albums]([AlbumName],[TenantTypeId],[OwnerId],[UserId],[Author],[Description],[CoverId],[PhotoCount],[DisplayOrder],[AuditStatus],[PrivacyStatus],[DateCreated],[LastModified],[LastUploadDate])
        SELECT N'默认相册','000011',UserId,UserId,UserName, N'默认相册',0,0,0,40,2,GETDATE(),GETDATE(),GETDATE()
        FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003)
-----3.插入安装记录
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) 
        SELECT UserId,1003,N'UserSpace' FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003)
