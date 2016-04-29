-- 添加应用数据
DELETE FROM `tn_Applications` WHERE `ApplicationId` = 1013;
INSERT `tn_Applications` (`ApplicationId`, `ApplicationKey`, `Description`, `IsEnabled`, `IsLocked`, `DisplayOrder`) VALUES (1013, 'Ask', '问答应用', 1, 0, 1013);

-- 应用在呈现区域的设置
DELETE FROM `tn_ApplicationInPresentAreaSettings` WHERE `ApplicationId` = 1013;
INSERT `tn_ApplicationInPresentAreaSettings` (`ApplicationId`, `PresentAreaKey`, `IsBuiltIn`, `IsAutoInstall`, `IsGenerateData`) VALUES (1013, 'Channel', 0, 1, 1);
INSERT `tn_ApplicationInPresentAreaSettings` (`ApplicationId`, `PresentAreaKey`, `IsBuiltIn`, `IsAutoInstall`, `IsGenerateData`) VALUES (1013, 'UserSpace', 0, 1, 0);

-- 默认安装记录
DELETE FROM `tn_ApplicationInPresentAreaInstallations` WHERE `ApplicationId` = 1013 and OwnerId = 0;
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) VALUES (0, 1013, 'Channel');

-- 快捷操作
DELETE FROM `tn_ApplicationManagementOperations` WHERE `ApplicationId` = 1013;
INSERT `tn_ApplicationManagementOperations` (`OperationId`, `ApplicationId`, `AssociatedNavigationId`, `PresentAreaKey`, `OperationType`, `OperationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101301, 1013, 0, 'Channel', 1, '提问', '', '', 'Channel_Ask_EditQuestion', NULL, 'Question', NULL, '_blank', 10101301, 0, 1, 1);

-- 动态
DELETE FROM  `tn_ActivityItems` WHERE `ApplicationId` = 1013;
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CommentAskAnswer', 1013, '评论回答', 4, '', 0, 1, 0);
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CommentAskQuestion', 1013, '评论问题', 3, '', 0, 1, 0);
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CreateAskAnswer', 1013, '发布回答', 2, '', 0, 1, 0);
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CreateAskQuestion', 1013, '发布问题', 1, '', 0, 1, 1);
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('SupportAskAnswer', 1013, '赞同回答', 5, '', 0, 1, 0);

-- 用户角色
DELETE FROM `tn_Roles` WHERE `ApplicationId` = 1013;
INSERT `tn_Roles` (`RoleName`, `FriendlyRoleName`, `IsBuiltIn`, `ConnectToUser`, `ApplicationId`, `IsPublic`, `Description`, `IsEnabled`, `RoleImage`) VALUES ('AskAdministrator', '问答管理员', 1, 1, 1013, 1, '管理问答应用下的内容', 1, '');
-- 权限项
DELETE FROM `tn_PermissionItems` WHERE `ApplicationId` = 1013;
INSERT `tn_PermissionItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `EnableQuota`, `EnableScope`) VALUES ('Ask_Create', 1013, '创建问题', 13, 0, 0);
INSERT `tn_PermissionItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `EnableQuota`, `EnableScope`) VALUES ('Ask_CreateAnswer', 1013, '回答问题', 14, 0, 0);
-- 角色针对权限的设置
DELETE FROM `tn_PermissionItemsInUserRoles` WHERE `ItemKey` = 'Ask_Create' and `RoleName` = 'RegisteredUsers';
DELETE FROM `tn_PermissionItemsInUserRoles` WHERE `ItemKey` = 'Ask_CreateAnswer' and `RoleName` = 'RegisteredUsers';
INSERT `tn_PermissionItemsInUserRoles` (`RoleName`, `ItemKey`, `PermissionType`, `PermissionQuota`, `PermissionScope`, `IsLocked`) VALUES ( 'RegisteredUsers', 'Ask_Create', 1, 0, 0, 0);
INSERT `tn_PermissionItemsInUserRoles` (`RoleName`, `ItemKey`, `PermissionType`, `PermissionQuota`, `PermissionScope`, `IsLocked`) VALUES ( 'RegisteredUsers', 'Ask_CreateAnswer', 1, 0, 0, 0);
-- 审核项
DELETE FROM `tn_AuditItems` WHERE `ApplicationId` = 1013;
INSERT `tn_AuditItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`) VALUES ('Ask_Answer', 1013, '回答', 2, '');
INSERT `tn_AuditItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`) VALUES ('Ask_Question', 1013, '提问', 1, '');

-- 积分项
DELETE FROM `tn_PointItems` WHERE `ApplicationId` = 1013;
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Ask_AcceptedAnswer', 1013, '采纳回答', 136, 2, 2, 0, 0, 0, 0, '',0);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Ask_AnswerWereAccepted', 1013, '回答被采纳', 135, 0, 10, 0, 0, 0, 0, '',0);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Ask_CreateAnswer', 1013, '回答问题', 133, 2, 1, 2, 0, 0, 0, '',1);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Ask_DeleteAnswer', 1013, '删除回答', 132, -2, -1, -2, 0, 0, 0, '',1);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Ask_CreateQuestion', 1013, '创建问题', 138, 2, 1, 2, 0, 0, 0, '',1);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Ask_DeleteQuestion', 1013, '删除问题', 137, -2, -1, -2, 0, 0, 0, '',0);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Ask_BeOpposed', 1013, '回答收到反对', 134, -3, -2, -1, 0, 0, 0, '',0);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Ask_BeSupported', 1013, '回答收到赞同', 131, 3, 2, 1, 0, 0, 0, '',0);

-- 租户类型
DELETE FROM `tn_TenantTypes` WHERE TenantTypeId in ('101300','101301','101302');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('101300', 1013, '问答应用', '');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('101301', 1013, '问题', '');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('101302', 1013, '回答', '');

-- 租户使用到的服务
DELETE FROM `tn_TenantTypesInServices` WHERE `TenantTypeId` in ('101301','101302');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101301','Comment');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101302','Comment');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101301','Attachment');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101302','Attachment');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101301','AtUser');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101301','Tag');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101301','Subscribe');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101302','Attitude');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101301','Notice');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101302','Notice');

-- 初始化导航
DELETE FROM `tn_InitialNavigations` WHERE `ApplicationId` = 1013;
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101301, 0, 0, 'Channel', 1013, 0, '问答', '', '', 'Channel_Ask_Home', NULL, 'Ask', NULL, '_self', 10101301, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101302, 10101301, 1, 'Channel', 1013, 0, '问答首页', ' ', ' ', 'Channel_Ask_Home', NULL, NULL, NULL, '_self', 10101302, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101303, 10101301, 1, 'Channel', 1013, 0, '问题', ' ', ' ', 'Channel_Ask_Questions', NULL, NULL, NULL, '_self', 10101303, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101304, 10101301, 1, 'Channel', 1013, 0, '标签', ' ', ' ', 'Channel_Ask_Tags', NULL, NULL, NULL, '_self', 10101304, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101305, 10101301, 1, 'Channel', 1013, 0, '用户排行', '', '', 'Channel_Ask_Rank', NULL, NULL, NULL, '_self', 10101305, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101306, 10101301, 1, 'Channel', 1013, 0, '我的问答', ' ', ' ', 'Channel_Ask_My', NULL, NULL, NULL, '_self', 10101306, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11101301, 0, 0, 'UserSpace', 1013, 0, '问答', ' ', ' ', 'Channel_Ask_My', 'spaceKey', 'Ask', NULL, '_self', 11101301, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (20101301, 20000011, 2, 'ControlPanel', 1013, 0, '问答', '', '', 'ControlPanel_Ask_Home', NULL, NULL, NULL, '_self', 20101301, 0, 0, 1);

-- 广告位
DELETE FROM `tn_AdvertisingPosition` WHERE `PositionId` like '101013%';
INSERT `tn_AdvertisingPosition` (`PositionId`, `PresentAreaKey`, `Description`, `FeaturedImage`, `Width`, `Height`, `IsEnable`) VALUES ('10101300001', 'Channel', '问答频道首页左中部广告位(190x270)', 'AdvertisingPosition\\00001\\01013\\00001\\10101300001.jpg', 190, 270, 1);
INSERT `tn_AdvertisingPosition` (`PositionId`, `PresentAreaKey`, `Description`, `FeaturedImage`, `Width`, `Height`, `IsEnable`) VALUES ('10101300002', 'Channel', '问答频道首页中上部广告位(550x190)', 'AdvertisingPosition\\00001\\01013\\00002\\10101300002.jpg', 550, 190, 1);
INSERT `tn_AdvertisingPosition` (`PositionId`, `PresentAreaKey`, `Description`, `FeaturedImage`, `Width`, `Height`, `IsEnable`) VALUES ('10101300003', 'Channel', '问答详细显示页左下部广告位(230x260)', 'AdvertisingPosition\\00001\\01013\\00003\\10101300003.jpg', 230, 260, 1);
INSERT `tn_AdvertisingPosition` (`PositionId`, `PresentAreaKey`, `Description`, `FeaturedImage`, `Width`, `Height`, `IsEnable`) VALUES ('10101300004', 'Channel', '问答详细显示页中部广告位(710x120)', 'AdvertisingPosition\\00001\\01013\\00004\\10101300004.jpg', 710, 120, 1);

-- 自动为已注册用户安装问答应用
-- 1.安装导航
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`)
SELECT 11101301, 0, 0, 'UserSpace', 1013, UserId, 0, '问答', ' ', ' ', 'Channel_Ask_My', 'spaceKey', 'Ask', NULL, '_self', 11101301, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1013);

-- 2.插入安装记录
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) 
        SELECT UserId,1013,'UserSpace' FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1013);