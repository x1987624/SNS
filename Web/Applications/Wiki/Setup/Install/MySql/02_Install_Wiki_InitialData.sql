-- 添加应用数据
DELETE FROM `tn_Applications` WHERE `ApplicationId` = 1016;
INSERT `tn_Applications` (`ApplicationId`, `ApplicationKey`, `Description`, `IsEnabled`, `IsLocked`, `DisplayOrder`) VALUES (1016, 'Wiki', '百科应用', 1, 0, 1016);

-- 应用在呈现区域的设置
DELETE FROM `tn_ApplicationInPresentAreaSettings` WHERE `ApplicationId` = 1016;
INSERT `tn_ApplicationInPresentAreaSettings` (`ApplicationId`, `PresentAreaKey`, `IsBuiltIn`, `IsAutoInstall`, `IsGenerateData`) VALUES (1016, 'Channel', 0, 1, 1);
INSERT `tn_ApplicationInPresentAreaSettings` (`ApplicationId`, `PresentAreaKey`, `IsBuiltIn`, `IsAutoInstall`, `IsGenerateData`) VALUES (1016, 'UserSpace', 0, 1, 0);

-- 默认安装记录
DELETE FROM `tn_ApplicationInPresentAreaInstallations` WHERE `ApplicationId` = 1016 and OwnerId = 0;
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) VALUES (0, 1016, 'Channel');
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) VALUES (0, 1016, 'UserSpace');

-- 快捷操作
DELETE FROM `tn_ApplicationManagementOperations` WHERE `ApplicationId` = 1016;
INSERT `tn_ApplicationManagementOperations` (`OperationId`, `ApplicationId`, `AssociatedNavigationId`, `PresentAreaKey`, `OperationType`, `OperationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101601, 1016, 0, 'Channel', 1, '创建词条', '', '', 'Channel_Wiki_EditPage', NULL, '', NULL, '_blank', 10101601, 0, 1, 1);

-- 动态
-- DELETE FROM  `tn_ActivityItems` WHERE `ApplicationId` = 1016
-- INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CommentWikiAnswer', 1016, '评论回答', 4, '', 0, 1, 0);
-- INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CommentWikiQuestion', 1016, '评论问题', 3, '', 0, 1, 0);
-- INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CreateWikiAnswer', 1016, '发布回答', 2, '', 0, 1, 0);
-- INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CreateWikiQuestion', 1016, '发布问题', 1, '', 0, 1, 1);
-- INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('SupportWikiAnswer', 1016, '赞同回答', 5, '', 0, 1, 0);

-- 用户角色
DELETE FROM `tn_Roles` WHERE `ApplicationId` = 1016;
INSERT `tn_Roles` (`RoleName`, `FriendlyRoleName`, `IsBuiltIn`, `ConnectToUser`, `ApplicationId`, `IsPublic`, `Description`, `IsEnabled`, `RoleImage`) VALUES ('WikiAdministrator', '百科管理员', 1, 1, 1016, 1, '管理百科应用下的内容', 1, '');

-- 权限项
DELETE FROM `tn_PermissionItems` WHERE `ApplicationId` = 1016;
INSERT `tn_PermissionItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `EnableQuota`, `EnableScope`) VALUES ('WikiPage_Create', 1016, '创建词条', 16, 0, 0);
INSERT `tn_PermissionItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `EnableQuota`, `EnableScope`) VALUES ('WikiPageVersion_Create', 1016, '编辑词条', 17, 0, 0);

-- 角色针对权限的设置
DELETE FROM `tn_PermissionItemsInUserRoles` WHERE `ItemKey` = 'WikiPage_Create' and `RoleName` = 'RegisteredUsers';
DELETE FROM `tn_PermissionItemsInUserRoles` WHERE `ItemKey` = 'WikiPageVersion_Create' and `RoleName` = 'RegisteredUsers';
INSERT `tn_PermissionItemsInUserRoles` (`RoleName`, `ItemKey`, `PermissionType`, `PermissionQuota`, `PermissionScope`, `IsLocked`) VALUES ( 'RegisteredUsers', 'WikiPage_Create', 1, 0, 0, 0);
INSERT `tn_PermissionItemsInUserRoles` (`RoleName`, `ItemKey`, `PermissionType`, `PermissionQuota`, `PermissionScope`, `IsLocked`) VALUES ( 'RegisteredUsers', 'WikiPageVersion_Create', 1, 0, 0, 0);

-- 审核项
DELETE FROM `tn_AuditItems` WHERE `ApplicationId` = 1016;
INSERT `tn_AuditItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`) VALUES ('Wiki_Page', 1016, '词条', 1, '');
INSERT `tn_AuditItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`) VALUES ('Wiki_PageVersion', 1016, '词条版本', 2, '');

-- 审核规则
DELETE FROM `tn_AuditItemsInUserRoles` WHERE `ItemKey` = 'Wiki_Page';
DELETE FROM `tn_AuditItemsInUserRoles` WHERE `ItemKey` = 'Wiki_PageVersion';
INSERT `tn_AuditItemsInUserRoles`(`RoleName`,`ItemKey` ,`StrictDegree`,`IsLocked`)VALUES('RegisteredUsers','Wiki_Page',2 ,0);
INSERT `tn_AuditItemsInUserRoles`(`RoleName`,`ItemKey` ,`StrictDegree`,`IsLocked`)VALUES('ModeratedUser','Wiki_Page',2 ,0);
INSERT `tn_AuditItemsInUserRoles`(`RoleName`,`ItemKey` ,`StrictDegree`,`IsLocked`)VALUES('RegisteredUsers','Wiki_PageVersion',2 ,0);
INSERT `tn_AuditItemsInUserRoles`(`RoleName`,`ItemKey` ,`StrictDegree`,`IsLocked`)VALUES('ModeratedUser','Wiki_PageVersion',2 ,0);

-- 积分项
-- DELETE FROM `tn_PointItems` WHERE `ApplicationId` = 1016
-- INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`) VALUES ('Wiki_AcceptedAnswer', 1016, '采纳回答', 136, 2, 2, 0, 0, 0, 0, '')

-- 租户类型
DELETE FROM `tn_TenantTypes` WHERE TenantTypeId in ('101600','101601','101602');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('101600', 1016, '百科应用', '');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('101601', 1016, '词条', '');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('101602', 1016, '词条版本', '');

-- 租户使用到的服务
DELETE FROM `tn_TenantTypesInServices` WHERE `TenantTypeId` = '101601';
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101601','Attachment');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101601','AtUser');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101601','Tag');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`,`ServiceKey`) VALUES('101601','Notice');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`, `ServiceKey`) VALUES ('101601','SiteCategory');
INSERT INTO `tn_TenantTypesInServices`(`TenantTypeId`, `ServiceKey`) VALUES ('101601','Comment');
INSERT INTO `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('101601', 'Recommend');

-- 初始化导航
DELETE FROM `tn_InitialNavigations` WHERE `ApplicationId` = 1016;
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101601, 0, 0, 'Channel', 1016, 0, '百科', '', '', 'Channel_Wiki_Home', '', 'World', NULL, '_self', 10101601, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101602, 10101601, 1, 'Channel', 1016, 0, '百科首页', ' ', ' ', 'Channel_Wiki_Home', '', 'World', NULL, '_self', 10101602, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10101605, 10101601, 1, 'Channel', 1016, 0, '我的百科', ' ', ' ', 'Channel_Wiki_My', NULL, NULL, NULL, '_self', 10101605, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (20101601, 20000011, 2, 'ControlPanel', 1016, 0, '百科', '', '', 'ControlPanel_Wiki_Home', '', 'World', NULL, '_self', 20101601, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11101601, 0, 0, 'UserSpace', 1016, 0, '百科', ' ', ' ', 'Channel_Wiki_My', '', 'World', NULL, '_self', 11101601, 0, 0, 1);

-- 自动为已注册用户安装问答应用
-- 1.安装导航
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`)
SELECT 11101601, 0, 0, 'UserSpace', 1016, UserId, 0, '百科', ' ', ' ', 'Channel_Wiki_My', '', 'Wiki', NULL, '_self', 11101601, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1016);

-- 2.插入安装记录
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) 
        SELECT UserId,1016,'UserSpace' FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1016);