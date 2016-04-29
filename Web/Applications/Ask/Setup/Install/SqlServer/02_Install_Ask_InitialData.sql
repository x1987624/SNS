-----添加应用数据
DELETE FROM [dbo].[tn_Applications] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_Applications] ([ApplicationId], [ApplicationKey], [Description], [IsEnabled], [IsLocked], [DisplayOrder]) VALUES (1013, N'Ask', N'问答应用', 1, 0, 1013)

-----应用在呈现区域的设置
DELETE FROM [dbo].[tn_ApplicationInPresentAreaSettings] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (1013, N'Channel', 0, 1, 1)
INSERT [dbo].[tn_ApplicationInPresentAreaSettings] ([ApplicationId], [PresentAreaKey], [IsBuiltIn], [IsAutoInstall], [IsGenerateData]) VALUES (1013, N'UserSpace', 0, 1, 0)

-----默认安装记录
DELETE FROM [dbo].[tn_ApplicationInPresentAreaInstallations] WHERE [ApplicationId] = 1013 and OwnerId = 0
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) VALUES (0, 1013, 'Channel')

-----快捷操作
DELETE FROM [dbo].[tn_ApplicationManagementOperations] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_ApplicationManagementOperations] ([OperationId], [ApplicationId], [AssociatedNavigationId], [PresentAreaKey], [OperationType], [OperationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101301, 1013, 0, N'Channel', 1, N'提问', N'', N'', N'Channel_Ask_EditQuestion', NULL, N'Question', NULL, N'_blank', 10101301, 0, 1, 1)

-----动态
DELETE FROM  [dbo].[tn_ActivityItems] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CommentAskAnswer', 1013, N'评论回答', 4, N'', 0, 1, 0)
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CommentAskQuestion', 1013, N'评论问题', 3, N'', 0, 1, 0)
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CreateAskAnswer', 1013, N'发布回答', 2, N'', 0, 1, 0)
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'CreateAskQuestion', 1013, N'发布问题', 1, N'', 0, 1, 1)
INSERT [dbo].[tn_ActivityItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description], [IsOnlyOnce], [IsUserReceived], [IsSiteReceived]) VALUES (N'SupportAskAnswer', 1013, N'赞同回答', 5, N'', 0, 1, 0)

-----用户角色
DELETE FROM [dbo].[tn_Roles] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_Roles] ([RoleName], [FriendlyRoleName], [IsBuiltIn], [ConnectToUser], [ApplicationId], [IsPublic], [Description], [IsEnabled], [RoleImage]) VALUES (N'AskAdministrator', N'问答管理员', 1, 1, 1013, 1, N'管理问答应用下的内容', 1, N'')

-----权限项
DELETE FROM [dbo].[tn_PermissionItems] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_PermissionItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [EnableQuota], [EnableScope]) VALUES (N'Ask_Create', 1013, N'创建问题', 13, 0, 0)
INSERT [dbo].[tn_PermissionItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [EnableQuota], [EnableScope]) VALUES (N'Ask_CreateAnswer', 1013, N'回答问题', 14, 0, 0)
-----角色针对权限的设置
DELETE FROM [dbo].[tn_PermissionItemsInUserRoles] WHERE [ItemKey] = N'Ask_Create' and [RoleName] = N'RegisteredUsers'
DELETE FROM [dbo].[tn_PermissionItemsInUserRoles] WHERE [ItemKey] = N'Ask_CreateAnswer' and [RoleName] = N'RegisteredUsers'
INSERT [dbo].[tn_PermissionItemsInUserRoles] ([RoleName], [ItemKey], [PermissionType], [PermissionQuota], [PermissionScope], [IsLocked]) VALUES ( N'RegisteredUsers', N'Ask_Create', 1, 0, 0, 0)
INSERT [dbo].[tn_PermissionItemsInUserRoles] ([RoleName], [ItemKey], [PermissionType], [PermissionQuota], [PermissionScope], [IsLocked]) VALUES ( N'RegisteredUsers', N'Ask_CreateAnswer', 1, 0, 0, 0)
-----审核项
DELETE FROM [dbo].[tn_AuditItems] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_AuditItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description]) VALUES (N'Ask_Answer', 1013, N'回答', 2, N'')
INSERT [dbo].[tn_AuditItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [Description]) VALUES (N'Ask_Question', 1013, N'提问', 1, N'')

-----积分项
DELETE FROM [dbo].[tn_PointItems] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Ask_AcceptedAnswer', 1013, N'采纳回答', 136, 2, 2, 0, 0, 0, 0, N'',0)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Ask_AnswerWereAccepted', 1013, N'回答被采纳', 135, 0, 10, 0, 0, 0, 0, N'',0)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Ask_CreateAnswer', 1013, N'回答问题', 133, 2, 1, 2, 0, 0, 0, N'',1)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Ask_DeleteAnswer', 1013, N'删除回答', 132, -2, -1, -2, 0, 0, 0, N'',1)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Ask_CreateQuestion', 1013, N'创建问题', 138, 2, 1, 2, 0, 0, 0, N'',1)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Ask_DeleteQuestion', 1013, N'删除问题', 137, -2, -1, -2, 0, 0, 0, N'',0)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Ask_BeOpposed', 1013, N'回答收到反对', 134, -3, -2, -1, 0, 0, 0, N'',0)
INSERT [dbo].[tn_PointItems] ([ItemKey], [ApplicationId], [ItemName], [DisplayOrder], [ExperiencePoints], [ReputationPoints], [TradePoints], [TradePoints2], [TradePoints3], [TradePoints4], [Description],[NeedPointMessage]) VALUES (N'Ask_BeSupported', 1013, N'回答收到赞同', 131, 3, 2, 1, 0, 0, 0, N'',0)

-----租户类型
DELETE FROM [dbo].[tn_TenantTypes] WHERE TenantTypeId in ('101300','101301','101302')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'101300', 1013, N'问答应用', N'')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'101301', 1013, N'问题', N'Spacebuilder.Ask.AskQuestion,Spacebuilder.Ask')
INSERT [dbo].[tn_TenantTypes] ([TenantTypeId], [ApplicationId], [Name], [ClassType]) VALUES (N'101302', 1013, N'回答', N'Spacebuilder.Ask.AskAnswer,Spacebuilder.Ask')

-----租户使用到的服务
DELETE FROM [dbo].[tn_TenantTypesInServices] WHERE [TenantTypeId] in ('101301','101302')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101301','Comment')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101302','Comment')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101301','Attachment')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101302','Attachment')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101301','AtUser')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101301','Tag')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101301','Subscribe')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101302','Attitude')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101301','Notice')
INSERT INTO [dbo].[tn_TenantTypesInServices]([TenantTypeId],[ServiceKey]) VALUES('101302','Notice')

-----初始化导航
DELETE FROM [dbo].[tn_InitialNavigations] WHERE [ApplicationId] = 1013
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101301, 0, 0, N'Channel', 1013, 0, N'问答', N'', N'', N'Channel_Ask_Home', NULL, N'Ask', NULL, N'_self', 10101301, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101302, 10101301, 1, N'Channel', 1013, 0, N'问答首页', N' ', N' ', N'Channel_Ask_Home', NULL, NULL, NULL, N'_self', 10101302, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101303, 10101301, 1, N'Channel', 1013, 0, N'问题', N' ', N' ', N'Channel_Ask_Questions', NULL, NULL, NULL, N'_self', 10101303, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101304, 10101301, 1, N'Channel', 1013, 0, N'标签', N' ', N' ', N'Channel_Ask_Tags', NULL, NULL, NULL, N'_self', 10101304, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101305, 10101301, 1, N'Channel', 1013, 0, N'用户排行', N'', N'', N'Channel_Ask_Rank', NULL, NULL, NULL, N'_self', 10101305, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (10101306, 10101301, 1, N'Channel', 1013, 0, N'我的问答', N' ', N' ', N'Channel_Ask_My', NULL, NULL, NULL, N'_self', 10101306, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (11101301, 0, 0, N'UserSpace', 1013, 0, N'问答', N' ', N' ', N'Channel_Ask_User', N'', N'Ask', NULL, N'_self', 11101301, 0, 0, 1)
INSERT [dbo].[tn_InitialNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) VALUES (20101301, 20000011, 2, N'ControlPanel', 1013, 0, N'问答', N'', N'', N'ControlPanel_Ask_Home', NULL, NULL, NULL, N'_self', 20101301, 0, 0, 1)

-----广告位
DELETE FROM [dbo].[tn_AdvertisingPosition] WHERE [PositionId] like '101013%'
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'10101300001', N'Channel', N'问答频道首页左中部广告位(190x270)', N'AdvertisingPosition\00001\01013\00001\10101300001.jpg', 190, 270, 1)
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'10101300002', N'Channel', N'问答频道首页中上部广告位(550x190)', N'AdvertisingPosition\00001\01013\00002\10101300002.jpg', 550, 190, 1)
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'10101300003', N'Channel', N'问答详细显示页左下部广告位(230x260)', N'AdvertisingPosition\00001\01013\00003\10101300003.jpg', 230, 260, 1)
INSERT [dbo].[tn_AdvertisingPosition] ([PositionId], [PresentAreaKey], [Description], [FeaturedImage], [Width], [Height], [IsEnable]) VALUES (N'10101300004', N'Channel', N'问答详细显示页中部广告位(710x120)', N'AdvertisingPosition\00001\01013\00004\10101300004.jpg', 710, 120, 1)

-----自动为已注册用户安装问答应用
-----1.安装导航
INSERT [dbo].[tn_PresentAreaNavigations] ([NavigationId], [ParentNavigationId], [Depth], [PresentAreaKey], [ApplicationId],[OwnerId], [NavigationType], [NavigationText], [ResourceName], [NavigationUrl], [UrlRouteName], [RouteDataName], [IconName], [ImageUrl], [NavigationTarget], [DisplayOrder], [OnlyOwnerVisible], [IsLocked], [IsEnabled]) 
SELECT 11101301, 0, 0, N'UserSpace', 1013, UserId, 0, N'问答', N' ', N' ', N'Channel_Ask_My', N'spaceKey', N'Ask', NULL, N'_self', 11101301, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1013)

-----2.插入安装记录
INSERT [dbo].[tn_ApplicationInPresentAreaInstallations] ([OwnerId], [ApplicationId], [PresentAreaKey]) 
        SELECT UserId,1013,N'UserSpace' FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1013)