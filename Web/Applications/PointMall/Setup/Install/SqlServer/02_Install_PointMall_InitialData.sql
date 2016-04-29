-----应用
DELETE FROM [dbo].[tn_Applications] where [ApplicationId] = 2001
INSERT [dbo].[tn_Applications] ([ApplicationId], [ApplicationKey], [Description], [IsEnabled], [IsLocked], [DisplayOrder]) VALUES (2001, N'PointMall', N'积分商城', 1, 0, 2001)

-----应用在呈现区域的设置
DELETE FROM [dbo].[tn_ApplicationInPresentAreaSettings] where [ApplicationId] = 2001
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (2001, N'Channel', 0, 1, 0)
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (2001, N'UserSpace', 0, 1, 0)

-----默认安装记录
DELETE FROM [dbo].[tn_ApplicationInPresentAreaInstallations] WHERE [ApplicationId] = 2001 and OwnerId = 0
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) VALUES (0, 2001, 'Channel')

-----快捷操作
DELETE FROM [dbo].[tn_ApplicationManagementOperations] where [ApplicationId] = 2001
INSERT [dbo].[tn_ApplicationManagementOperations] ([OperationId], [ApplicationId], [AssociatedNavigationId], [PresentAreaKey], [OperationType], [OperationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11200101, 2001, 11200104, N'UserSpace', 1, N'如何获取积分？', N' ', N' ', N'UserSpace_Honour_PointRule', NULL, NULL, NULL, N'_blank', 11200101, 1, 0, 1)

-----租户类型
DELETE FROM [dbo].[tn_TenantTypes] where [ApplicationId] = 2001
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'200101', 2001, N'商品', N'Spacebuilder.PointMall.PointGift,Spacebuilder.PointMall')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'200102', 2001, N'兑换申请', N'Spacebuilder.PointMall.PointGiftExchangeRecord,Spacebuilder.PointMall')

-----用户角色
DELETE FROM [dbo].[tn_Roles] WHERE [ApplicationId] = 2001
INSERT [dbo].[tn_Roles] ([RoleName], [FriendlyRoleName], [IsBuiltIn], [ConnectToUser], [ApplicationId], [IsPublic], [Description], [IsEnabled], [RoleImage]) VALUES (N'PointMallAdministrator', N'积分商城管理员', 1, 1, 2001, 1, N'管理积分商城应用下的内容', 1, N'')

-----租户相关服务
DELETE FROM [dbo].[tn_TenantTypesInServices] where [TenantTypeId] in ('200101', '200102')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'200101', N'Comment')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'200101', N'Recommend')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'200101', N'SiteCategory')
INSERT [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'200102', N'Notice')

-----推荐类别
DELETE FROM [dbo].[tn_RecommendItemTypes] where [TenantTypeId] = '200101'
INSERT [dbo].[tn_RecommendItemTypes] ([TypeId], [TenantTypeId], [Name], [Description], [HasFeaturedImage], [DateCreated]) VALUES (N'20010101', N'200101', N'推荐商品', N'推荐商品', 1, CAST(0x0000A10400000000 AS DateTime))
INSERT [dbo].[tn_RecommendItemTypes] ([TypeId], [TenantTypeId], [Name], [Description], [HasFeaturedImage], [DateCreated]) VALUES (N'20010102', N'200101', N'推荐商品幻灯片', N'商品首页推荐幻灯片', 1, CAST(0x0000A10400000000 AS DateTime))

-----初始导航
DELETE FROM [dbo].[tn_InitialNavigations] where [ApplicationId] = 2001
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10200101, 0, 0, N'Channel', 2001, 0, N'积分商城', N'', N'', N'Channel_PointMall_Home', NULL, N'PointMall', NULL, N'_self', 10200101, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11200101, 0, 0, N'UserSpace', 2001, 1, N'商城', N'', N'', N'UserSpace_PointMall_Home', NULL, N'PointMall', NULL, N'_self', 11200101, 1, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11200102, 11200101, 1, N'UserSpace', 2001, 1, N'商品申请', N'', N'', N'UserSpace_PointMall_Home', NULL, NULL, NULL, N'_self', 11200102, 1, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11200103, 11200101, 1, N'UserSpace', 2001, 1, N'我的收藏', N'', N'', N'UserSpace_PointMall_Favorite', NULL, NULL, NULL, N'_self', 11200103, 1, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11200104, 11200101, 1, N'UserSpace', 2001, 0, N'积分记录', N' ', N' ', N'UserSpace_Honour_PointRecords', NULL, NULL, NULL, N'_self', 11200104, 1, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (20200101, 20000011, 2, N'ControlPanel', 2001, 0, N'积分商城', N'', N'', N'ControlPanel_PointMall_Home', NULL, NULL, NULL, N'_self', 20200101, 0, 0, 1)


-----动态
DELETE FROM [dbo].[tn_ActivityItems] where [ApplicationId] = 2001
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'ExchangeGift', 2001, N'兑换商品', 1, N'', 1, 1, 1)

-----自动为已注册用户安装积分商城应用
-----1.安装导航
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11200101, 0, 0, N'UserSpace', 2001,UserId, 1, N'商城', N'', N'', N'UserSpace_PointMall_Home', NULL, N'PointMall', NULL, N'_self', 11200101, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=2001)
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11200102, 11200101, 1, N'UserSpace', 2001,UserId, 1, N'商品申请', N'', N'', N'UserSpace_PointMall_Home', NULL, NULL, NULL, N'_self', 11200102, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=2001)

INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11200103, 11200101, 1, N'UserSpace', 2001,UserId, 1, N'我的收藏', N'', N'', N'UserSpace_PointMall_Favorite', NULL, NULL, NULL, N'_self', 11200103, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=2001)
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11200104, 11200101, 1, N'UserSpace', 2001,UserId, 0, N'积分记录', N' ', N' ', N'UserSpace_Honour_PointRecords', NULL, NULL, NULL, N'_self', 11200104, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=2001)

-----2.插入安装记录
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) 
        SELECT UserId,2001,N'UserSpace' FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=2001)