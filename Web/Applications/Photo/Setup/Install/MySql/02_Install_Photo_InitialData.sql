-- 应用
DELETE FROM `tn_Applications` WHERE `ApplicationId` = 1003;
INSERT `tn_Applications` (`ApplicationId`, `ApplicationKey`, `Description`, `IsEnabled`, `IsLocked`, `DisplayOrder`) VALUES (1003, 'Photo', '相册应用', 1, 0, 1003);

-- 快捷操作
DELETE FROM `tn_ApplicationManagementOperations` WHERE `ApplicationId` = 1003;
INSERT `tn_ApplicationManagementOperations` (`OperationId`, `ApplicationId`, `AssociatedNavigationId`, `PresentAreaKey`, `OperationType`, `OperationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10100301, 1003, 0, 'Channel', 1, '上传照片', '', '', 'UserSpace_Photo_Upload', 'spaceKey', 'Upload', NULL, '_blank', 10100301, 0, 0, 1);
INSERT `tn_ApplicationManagementOperations` (`OperationId`, `ApplicationId`, `AssociatedNavigationId`, `PresentAreaKey`, `OperationType`, `OperationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11100301, 1003, 0, 'UserSpace', 1, '上传照片', '','', 'UserSpace_Photo_Upload', NULL, 'Upload', NULL, '_blank', 11100301, 0, 1, 1);

-- 应用在呈现区域的设置
DELETE FROM `tn_ApplicationInPresentAreaSettings` WHERE `ApplicationId` = 1003;
INSERT `tn_ApplicationInPresentAreaSettings` (`ApplicationId`, `PresentAreaKey`, `IsBuiltIn`, `IsAutoInstall`, `IsGenerateData`) VALUES (1003, 'UserSpace', 0, 1, 1);
INSERT `tn_ApplicationInPresentAreaSettings` (`ApplicationId`, `PresentAreaKey`, `IsBuiltIn`, `IsAutoInstall`, `IsGenerateData`) VALUES (1003, 'Channel', 0, 1, 0);

-- 默认安装记录
DELETE FROM `tn_ApplicationInPresentAreaInstallations` WHERE `ApplicationId` = 1003 and OwnerId = 0;
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) VALUES (0, 1003, 'Channel');

-- 租户类型
DELETE FROM `tn_TenantTypes` WHERE `ApplicationId` = 1003;
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('100300', 1003, '相册应用', '');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('100301', 1003, '相册', '');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('100302', 1003, '照片', '');
INSERT `tn_TenantTypes` (`TenantTypeId`, `ApplicationId`, `Name`, `ClassType`) VALUES ('100303', 1003, '圈人', '');

-- 租户使用的服务
DELETE FROM `tn_TenantTypesInServices` WHERE `TenantTypeId` in('100301','100302');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('100301', 'Recommend');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('100302', 'Attitude');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('100302', 'Commend');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('100302', 'Comment');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('100302', 'Notice');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('100302', 'Recommend');
INSERT `tn_TenantTypesInServices` (`TenantTypeId`, `ServiceKey`) VALUES ('100302', 'Tag');

-- 动态
DELETE FROM  `tn_ActivityItems` WHERE `ApplicationId` = 1003;
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CreatePhoto', 1003, '发布照片', 1, '', 0, 1, 1);
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('CommentPhoto', 1003, '评论照片', 2, '', 0, 1, 1);
INSERT `tn_ActivityItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`, `IsOnlyOnce`, `IsUserReceived`, `IsSiteReceived`) VALUES ('LabelPhoto', 1003, '照片圈人', 3, '', 0, 1, 0);

-- 用户角色
DELETE FROM `tn_Roles` WHERE `ApplicationId` = 1003;
INSERT `tn_Roles` (`RoleName`, `FriendlyRoleName`, `IsBuiltIn`, `ConnectToUser`, `ApplicationId`, `IsPublic`, `Description`, `IsEnabled`, `RoleImage`) VALUES ('PhotoAdministrator', '相册管理员', 1, 1, 1003, 1, '管理相册应用下的内容', 1, '');

-- 权限
DELETE FROM `tn_PermissionItems` WHERE `ApplicationId` = 1003;
INSERT `tn_PermissionItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `EnableQuota`, `EnableScope`) VALUES ('Photo_Create', 1003, '上传照片', 3, 0, 0);
-- 角色针对权限的设置
DELETE FROM `tn_PermissionItemsInUserRoles` WHERE `ItemKey` = 'Photo_Create' and `RoleName` = 'RegisteredUsers';
INSERT `tn_PermissionItemsInUserRoles` (`RoleName`, `ItemKey`, `PermissionType`, `PermissionQuota`, `PermissionScope`, `IsLocked`) VALUES ( 'RegisteredUsers', 'Photo_Create', 1, 0, 0, 0);

-- 审核
DELETE FROM `tn_AuditItems` WHERE `ApplicationId` = 1003;
INSERT `tn_AuditItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`) VALUES ('Album', 1003, '创建相册', 9, '');
INSERT `tn_AuditItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `Description`) VALUES ('Photo', 1003, '上传照片', 10, '');

-- 积分
DELETE FROM `tn_PointItems` WHERE `ApplicationId`=1003;
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Photo_BeLabelled', 1003, '照片被圈', 142, 0, 2, 0, 0, 0, 0, '',0);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Photo_BeLabelled_Delete', 1003, '删除被圈照片', 143, 0, -2, 0, 0, 0, 0, '',0);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Photo_BeLiked', 1003, '照片被喜欢', 141, 0, 2, 0, 0, 0, 0, '',0);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Photo_DeletePhoto', 1003, '删除照片', 144, 0, -2, 0, 0, 0, 0, '',1);
INSERT `tn_PointItems` (`ItemKey`, `ApplicationId`, `ItemName`, `DisplayOrder`, `ExperiencePoints`, `ReputationPoints`, `TradePoints`, `TradePoints2`, `TradePoints3`, `TradePoints4`, `Description`,`NeedPointMessage`) VALUES ('Photo_UploadPhoto', 1003, '上传照片', 145, 0, 2, 0, 0, 0, 0, '',1);

-- 初始化导航菜单
DELETE FROM `tn_InitialNavigations` WHERE `ApplicationId` = 1003;
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10100301, 0, 0, 'Channel', 1003, 0, '相册', ' ', ' ', 'Channel_Photo_Home', NULL, 'Album', NULL, '_self', 10100301, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10100302, 10100301, 1, 'Channel', 1003, 0, '相册首页', ' ', ' ', 'Channel_Photo_Home', NULL, NULL, NULL, '_self', 10100302, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10100303, 10100301, 1, 'Channel', 1003, 0, '照片排行', '', '', 'Channel_Photo_New', NULL, NULL, NULL, '_self', 10100303, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (10100304, 10100301, 1, 'Channel', 1003, 0, '我的相册', '', '', 'UserSpace_Photo_Home', 'spaceKey', NULL, NULL, '_self', 10100304, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11100301, 0, 0, 'UserSpace', 1003, 0, '相册', ' ', ' ', 'UserSpace_Photo_Home', NULL, 'Album', NULL, '_self', 11100301, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11100302, 11100301, 1, 'UserSpace', 1003, 0, '相册首页', ' ', ' ', 'UserSpace_Photo_Home', NULL, NULL, NULL, '_self', 11100302, 1, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11100303, 11100301, 1, 'UserSpace', 1003, 0, '最新照片', ' ', ' ', 'UserSpace_Photo_Photos', NULL, NULL, NULL, '_self', 11100303, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11100304, 11100301, 1, 'UserSpace', 1003, 0, '相册列表', ' ', ' ', 'UserSpace_Photo_Albums', NULL, NULL, NULL, '_self', 11100304, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (11100305, 11100301, 1, 'UserSpace', 1003, 0, '我的喜欢', ' ', ' ', 'UserSpace_Photo_Favorites', NULL, NULL, NULL, '_self', 11100305, 0, 0, 1);
INSERT `tn_InitialNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) VALUES (20100301, 20000011, 2, 'ControlPanel', 1003, 0, '相册', ' ', ' ', 'ControlPanel_Photo_Home', NULL, NULL, NULL, '_self', 20100301, 0, 0, 1);

-- 推荐
DELETE FROM `tn_RecommendItemTypes` WHERE `TenantTypeId` in ('100301','100302');
INSERT `tn_RecommendItemTypes` (`TypeId`, `TenantTypeId`, `Name`, `Description`, `HasFeaturedImage`, `DateCreated`) VALUES ('10030101', '100301', '推荐相册', '推荐相册', 0, now());
INSERT `tn_RecommendItemTypes` (`TypeId`, `TenantTypeId`, `Name`, `Description`, `HasFeaturedImage`, `DateCreated`) VALUES ('10030201', '100302', '推荐照片', '推荐照片', 0, now());
INSERT `tn_RecommendItemTypes` (`TypeId`, `TenantTypeId`, `Name`, `Description`, `HasFeaturedImage`, `DateCreated`) VALUES ('10030202', '100302', '推荐标签', '推荐相册下的标签', 0, now());

-- 广告位
DELETE FROM `tn_AdvertisingPosition` WHERE `PositionId` like '101003%';
INSERT `tn_AdvertisingPosition` (`PositionId`, `PresentAreaKey`, `Description`, `FeaturedImage`, `Width`, `Height`, `IsEnable`) VALUES ('10100300001', 'Channel', '相册频道首页中部广告位(950x170)', 'AdvertisingPosition\\00001\\01003\\00001\\10100300001.jpg', 950, 170, 1);

-- 自动为已注册用户安装相册应用
-- 1.安装导航
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 10100304, 10100301, 1, 'Channel', 1003,UserId, 0, '我的相册', '', '', 'UserSpace_Photo_Home', 'spaceKey', NULL, NULL, '_self', 10100304, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1003);
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11100301, 0, 0, 'UserSpace', 1003,UserId, 0, '相册', ' ', ' ','UserSpace_Photo_Home', NULL, 'Album', NULL, '_self', 11100301, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1003);

INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11100302, 11100301, 1, 'UserSpace', 1003,UserId, 0, '相册首页', ' ', ' ', 'UserSpace_Photo_Home', NULL, NULL, NULL, '_self', 11100302, 1, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey=N'UserSpace' AND ApplicationId=1003);
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11100303, 11100301, 1, 'UserSpace', 1003,UserId, 0, '最新照片', ' ', ' ', 'UserSpace_Photo_Photos', NULL, NULL, NULL, '_self', 11100303, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1003);
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11100304, 11100301, 1, 'UserSpace', 1003,UserId, 0, '相册列表', ' ', ' ', 'UserSpace_Photo_Albums', NULL, NULL, NULL, '_self', 11100304, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1003);
INSERT `tn_PresentAreaNavigations` (`NavigationId`, `ParentNavigationId`, `Depth`, `PresentAreaKey`, `ApplicationId`,`OwnerId`, `NavigationType`, `NavigationText`, `ResourceName`, `NavigationUrl`, `UrlRouteName`, `RouteDataName`, `IconName`, `ImageUrl`, `NavigationTarget`, `DisplayOrder`, `OnlyOwnerVisible`, `IsLocked`, `IsEnabled`) 
SELECT 11100305, 11100301, 1, 'UserSpace', 1003,UserId, 0, '我的喜欢', ' ', ' ', 'UserSpace_Photo_Favorites', NULL, NULL, NULL, '_self', 11100305, 0, 0, 1
        FROM tn_Users
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1003);
-- 2.创建默认相册
INSERT `spb_Albums`(`AlbumName`,`TenantTypeId`,`OwnerId`,`UserId`,`Author`,`Description`,`CoverId`,`PhotoCount`,`DisplayOrder`,`AuditStatus`,`PrivacyStatus`,`DateCreated`,`LastModified`,`LastUploadDate`)
        SELECT '默认相册','000011',UserId,UserId,UserName, '默认相册',0,0,0,40,2,now(),now(),now()
        FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1003);
-- 3.插入安装记录
INSERT `tn_ApplicationInPresentAreaInstallations` (`OwnerId`, `ApplicationId`, `PresentAreaKey`) 
        SELECT UserId,1003,'UserSpace' FROM tn_Users 
        WHERE  UserId NOT IN(SELECT OwnerId FROM tn_ApplicationInPresentAreaInstallations WHERE PresentAreaKey='UserSpace' AND ApplicationId=1003);
