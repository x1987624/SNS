-----添加应用数据
DELETE FROM [dbo].[tn_Applications] WHERE [ApplicationId] = 1016
INSERT [dbo].[tn_Applications] ([ApplicationId], [ApplicationKey], [Description], [IsEnabled], [IsLocked], [DisplayOrder]) VALUES (1016, N'Wiki', N'百科应用', 1, 0, 1016)

-----应用在呈现区域的设置
DELETE FROM [dbo].[tn_ApplicationInPresentAreaSettings] WHERE [ApplicationId] = 1016
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (1016, N'Channel', 0, 1, 1)
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (1016, N'UserSpace', 0, 1, 0)

-----默认安装记录
DELETE FROM [dbo].[tn_ApplicationInPresentAreaInstallations] WHERE [ApplicationId] = 1016 and OwnerId = 0
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) VALUES (0, 1016, 'Channel')
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) VALUES (0, 1016, 'UserSpace')

-----快捷操作
DELETE FROM [dbo].[tn_ApplicationManagementOperations] WHERE [ApplicationId] = 1016
INSERT [dbo].[tn_ApplicationManagementOperations] ([OperationId], [ApplicationId], [AssociatedNavigationId], [PresentAreaKey], [OperationType], [OperationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101601, 1016, 0, N'Channel', 1, N'创建词条', N'', N'', N'Channel_Wiki_EditPage', NULL, N'', NULL, N'_blank', 10101601, 0, 1, 1)

-------动态
--DELETE FROM  [dbo].[tn_ActivityItems] WHERE [ApplicationId] = 1016
--INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CommentWikiAnswer', 1016, N'评论回答', 4, N'', 0, 1, 0)
--INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CommentWikiQuestion', 1016, N'评论问题', 3, N'', 0, 1, 0)
--INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CreateWikiAnswer', 1016, N'发布回答', 2, N'', 0, 1, 0)
--INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CreateWikiQuestion', 1016, N'发布问题', 1, N'', 0, 1, 1)
--INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'SupportWikiAnswer', 1016, N'赞同回答', 5, N'', 0, 1, 0)

-----用户角色
DELETE FROM [dbo].[tn_Roles] WHERE [ApplicationId] = 1016
INSERT [dbo].[tn_Roles] ([RoleName], [FriendlyRoleName], [IsBuiltIn], [ConnectToUser], [ApplicationId], [IsPublic], [Description], [IsEnabled], [RoleImage]) VALUES (N'WikiAdministrator', N'百科管理员', 1, 1, 1016, 1, N'管理百科应用下的内容', 1, N'')

-----权限项
DELETE FROM [dbo].[tn_PermissionItems] WHERE [ApplicationId] = 1016
INSERT [dbo].[tn_PermissionItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [EnableQuota], [EnableScope]) VALUES (N'WikiPage_Create', 1016, N'创建词条', 16, 0, 0)
INSERT [dbo].[tn_PermissionItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [EnableQuota], [EnableScope]) VALUES (N'WikiPageVersion_Create', 1016, N'编辑词条', 17, 0, 0)

-----角色针对权限的设置
DELETE FROM [dbo].[tn_PermissionItemsInUserRoles] WHERE [ItemKey] = N'WikiPage_Create' and [RoleName] = N'RegisteredUsers'
DELETE FROM [dbo].[tn_PermissionItemsInUserRoles] WHERE [ItemKey] = N'WikiPageVersion_Create' and [RoleName] = N'RegisteredUsers'
INSERT [dbo].[tn_PermissionItemsInUserRoles] ([RoleName], [ItemKey], [PermissionType], [PermissionQuota], [PermissionScope], [IsLocked]) VALUES ( N'RegisteredUsers', N'WikiPage_Create', 1, 0, 0, 0)
INSERT [dbo].[tn_PermissionItemsInUserRoles] ([RoleName], [ItemKey], [PermissionType], [PermissionQuota], [PermissionScope], [IsLocked]) VALUES ( N'RegisteredUsers', N'WikiPageVersion_Create', 1, 0, 0, 0)

-----审核项
DELETE FROM [dbo].[tn_AuditItems] WHERE [ApplicationId] = 1016
INSERT [dbo].[tn_AuditItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description]) VALUES (N'Wiki_Page', 1016, N'词条', 1, N'')
INSERT [dbo].[tn_AuditItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description]) VALUES (N'Wiki_PageVersion', 1016, N'词条版本', 2, N'')

--审核规则
DELETE FROM [dbo].[tn_AuditItemsInUserRoles] WHERE [ItemKey] = 'Wiki_Page'
DELETE FROM [dbo].[tn_AuditItemsInUserRoles] WHERE [ItemKey] = 'Wiki_PageVersion'
INSERT [dbo].[tn_AuditItemsInUserRoles]([RoleName],[ItemKey] ,[StrictDegree],[IsLocked])VALUES(N'RegisteredUsers',N'Wiki_Page',2 ,0)
INSERT [dbo].[tn_AuditItemsInUserRoles]([RoleName],[ItemKey] ,[StrictDegree],[IsLocked])VALUES(N'ModeratedUser',N'Wiki_Page',2 ,0)
INSERT [dbo].[tn_AuditItemsInUserRoles]([RoleName],[ItemKey] ,[StrictDegree],[IsLocked])VALUES(N'RegisteredUsers',N'Wiki_PageVersion',2 ,0)
INSERT [dbo].[tn_AuditItemsInUserRoles]([RoleName],[ItemKey] ,[StrictDegree],[IsLocked])VALUES(N'ModeratedUser',N'Wiki_PageVersion',2 ,0)

-----积分项
--DELETE FROM [dbo].[tn_PointItems] WHERE [ApplicationId] = 1016
--INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description]) VALUES (N'Wiki_AcceptedAnswer', 1016, N'采纳回答', 136, 2, 2, 0, 0, 0, 0, N'')

-----租户类型
DELETE FROM [dbo].[tn_TenantTypes] WHERE TenantTypeId in ('101600','101601','101602')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'101600', 1016, N'百科应用', N'')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'101601', 1016, N'词条', N'')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'101602', 1016, N'词条版本', N'')

-----租户使用到的服务
DELETE FROM [dbo].[tn_TenantTypesInServices] WHERE [TenantTypeId] = '101601'
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101601','Attachment')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101601','AtUser')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101601','Tag')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101601','Notice')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId], [ServiceKey]) VALUES ('101601','SiteCategory')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId], [ServiceKey]) VALUES ('101601','Comment')
INSERT INTO [dbo].[tn_TenantTypesInServices] ([TenantTypeId], [ServiceKey]) VALUES (N'101601', N'Recommend')

-----初始化导航
DELETE FROM [dbo].[tn_InitialNavigations] WHERE [ApplicationId] = 1016
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101601, 0, 0, N'Channel', 1016, 0, N'百科', N'', N'', N'Channel_Wiki_Home', '', N'World', NULL, N'_self', 10101601, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101602, 10101601, 1, N'Channel', 1016, 0, N'百科首页', N' ', N' ', N'Channel_Wiki_Home', '', 'World', NULL, N'_self', 10101602, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101605, 10101601, 1, N'Channel', 1016, 0, N'我的百科', N' ', N' ', N'Channel_Wiki_My', NULL, NULL, NULL, N'_self', 10101605, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (20101601, 20000011, 2, N'ControlPanel', 1016, 0, N'百科', N'', N'', N'ControlPanel_Wiki_Home', '', 'World', NULL, N'_self', 20101601, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11101601, 0, 0, N'UserSpace', 1016, 0, N'百科', N' ', N' ', N'Channel_Wiki_My', N'', N'World', NULL, N'_self', 11101601, 0, 0, 1)

-----自动为已注册用户安装问答应用
-----1.安装导航
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11101601, 0, 0, N'UserSpace', 1016, UserId, 0, N'百科', N' ', N' ', N'Channel_Wiki_My', N'', N'Wiki', NULL, N'_self', 11101601, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1016)

-----2.插入安装记录
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) 
        SELECT UserId,1016,N'UserSpace' FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1016)