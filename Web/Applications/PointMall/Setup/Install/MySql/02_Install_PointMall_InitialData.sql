-- 应用
DELETE FROM `tn_Applications` where `ApplicationId` = 2001;
INSERT `tn_Applications` (`ApplicationId`, `ApplicationKey`, `Description`, `IsEnabled`, `IsLocked`, `DisplayOrder`) VALUES (2001, 'PointMall', '积分商城', 1, 0, 2001);

-- 应用在呈现区域的设置
DELETE FROM `tn_ApplicationInPresentAreaSettings` where `ApplicationId` = 2001;
INSERT `tn_ApplicationInPresentAreaSettings` (`ApplicationId`, `PresentAreaKey`, `IsBuiltIn`, `IsAutoInstall`, `IsGenerateData`) VALUES (2001, 'Channel', 0, 1, 0);
INSERT `tn_ApplicationInPresentAreaSettings` (`ApplicationId`, `PresentAreaKey`, `IsBuiltIn`, `IsAutoInstall`, `IsGenerateData`) VALUES (2001, 'UserSpace', 0, 1, 0);

-- 默认安装记录
DELETE FROM `tn_ApplicationInPresentAreaInstallations` WHERE `ApplicationId` = 2001 and OwnerId = 0;
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) VALUES (0, 2001, 'Channel');

-- 快捷操作
DELETE FROM `tn_ApplicationManagementOperations` where `ApplicationId` = 2001;
INSERT `tn_ApplicationManagementOperations` (`OperationId`, `ApplicationId`, `AssociatedNavigationId`, `PresentAreaKey`, `OperationType`, `OperationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11200101, 2001, 11200104, 'UserSpace', 1, '如何获取积分？', ' ', ' ', 'UserSpace_Honour_PointRule', NULL, NULL, NULL, '_blank', 11200101, 1, 0, 1);

-- 租户类型
DELETE FROM `tn_TenantTypes` where `ApplicationId` = 2001;
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('200101', 2001, '商品', '');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('200102', 2001, '兑换申请','');

-- 用户角色
DELETE FROM `tn_Roles` WHERE `ApplicationId` = 2001;
INSERT `tn_Roles` (`RoleName`, `FriendlyRoleName`, `IsBuiltIn`, `ConnectToUser`, `ApplicationId`, `IsPublic`, `Description`, `IsEnabled`, `RoleImage`) VALUES ('PointMallAdministrator', '积分商城管理员', 1, 1, 2001, 1, '管理积分商城应用下的内容', 1, '');

-- 租户相关服务
DELETE FROM `tn_TenantTypesInServices` where `TenantTypeId` in ('200101', '200102');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('200101', 'Comment');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('200101', 'Recommend');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('200101', 'SiteCategory');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('200102', 'Notice');

-- 推荐类别
DELETE FROM `tn_RecommendItemTypes` where `TenantTypeId` = '200101';
INSERT `tn_RecommendItemTypes` (`TypeId`, `TenantTypeId`, `Name`, `Description`, `HasFeaturedImage`, `DateCreated`) VALUES ('20010101', '200101', '推荐商品', '推荐商品', 1, now());
INSERT `tn_RecommendItemTypes` (`TypeId`, `TenantTypeId`, `Name`, `Description`, `HasFeaturedImage`, `DateCreated`) VALUES ('20010102', '200101', '推荐商品幻灯片', '商品首页推荐幻灯片', 1, now());

-- 初始导航
DELETE FROM `tn_InitialNavigations` where `ApplicationId` = 2001;
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10200101, 0, 0, 'Channel', 2001, 0, '积分商城', '', '', 'Channel_PointMall_Home', NULL, 'PointMall', NULL, '_self', 10200101, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11200101, 0, 0, 'UserSpace', 2001, 1, '商城', '', '', 'UserSpace_PointMall_Home', NULL, 'PointMall', NULL, '_self', 11200101, 1, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11200102, 11200101, 1, 'UserSpace', 2001, 1, '商品申请', '', '', 'UserSpace_PointMall_Home', NULL, NULL, NULL, '_self', 11200102, 1, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11200103, 11200101, 1, 'UserSpace', 2001, 1, '我的收藏', '', '', 'UserSpace_PointMall_Favorite', NULL, NULL, NULL, '_self', 11200103, 1, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11200104, 11200101, 1, 'UserSpace', 2001, 0, '积分记录', ' ', ' ', 'UserSpace_Honour_PointRecords', NULL, NULL, NULL, '_self', 11200104, 1, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (20200101, 20000011, 2, 'ControlPanel', 2001, 0, '积分商城', '', '', 'ControlPanel_PointMall_Home', NULL, NULL, NULL, '_self', 20200101, 0, 0, 1);


-- 动态
DELETE FROM `tn_ActivityItems` where `ApplicationId` = 2001;
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('ExchangeGift', 2001, '兑换商品', 1, '', 1, 1, 1);

-- 自动为已注册用户安装积分商城应用
-- 1.安装导航
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11200101, 0, 0, 'UserSpace', 2001,UserId, 1, '商城', '', '', 'UserSpace_PointMall_Home', NULL, 'PointMall', NULL, '_self', 11200101, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=2001);
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11200102, 11200101, 1, 'UserSpace', 2001,UserId, 1, '商品申请', '', '', 'UserSpace_PointMall_Home', NULL, NULL, NULL, '_self', 11200102, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=2001);

INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11200103, 11200101, 1, 'UserSpace', 2001,UserId, 1, '我的收藏', '', '', 'UserSpace_PointMall_Favorite', NULL, NULL, NULL, '_self', 11200103, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=2001);
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11200104, 11200101, 1, 'UserSpace', 2001,UserId, 0, '积分记录', ' ', ' ', 'UserSpace_Honour_PointRecords', NULL, NULL, NULL, '_self', 11200104, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=2001);

-- 2.插入安装记录
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) 
        SELECT UserId,2001,'UserSpace' FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=2001);