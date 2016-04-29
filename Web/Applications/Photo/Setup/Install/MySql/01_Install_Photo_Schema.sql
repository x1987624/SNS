SET FOREIGN_KEY_CHECKS = 0;


DROP TABLE IF EXISTS `spb_Albums`;
CREATE TABLE `spb_Albums` (
AlbumId BigInt NOT NULL AUTO_INCREMENT,
AlbumName Varchar(256) NOT NULL,
TenantTypeId Varchar(6) NOT NULL,
OwnerId BigInt NOT NULL,
UserId BigInt NOT NULL,
Author Varchar(64) NOT NULL,
`Description` Varchar(255) NOT NULL,
CoverId BigInt NOT NULL DEFAULT 0,
PhotoCount Int(11) NOT NULL,
DisplayOrder BigInt NOT NULL,
AuditStatus SmallInt NOT NULL DEFAULT 40,
PrivacyStatus SmallInt NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
LastUploadDate DateTime NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
KEY `IX_AuditStatus` (`AuditStatus`),
KEY `IX_DisplayOrder` (`DisplayOrder`),
KEY `IX_OwnerId` (`OwnerId`),
KEY `IX_PrivacyStatus` (`PrivacyStatus`),
KEY `IX_TenantTypeId` (`TenantTypeId`),
KEY `IX_UserId` (`UserId`),
PRIMARY KEY (`AlbumId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;
SET FOREIGN_KEY_CHECKS = 0;


DROP TABLE IF EXISTS `spb_PhotoLabels`;
CREATE TABLE `spb_PhotoLabels` (
LabelId BigInt NOT NULL AUTO_INCREMENT,
PhotoId BigInt NOT NULL,
TenantTypeId Char(6) NOT NULL,
ObjetId BigInt NOT NULL,
ObjectName Varchar(256) NOT NULL,
UserId BigInt NOT NULL,
`Description` Varchar(512) NOT NULL,
AreaX Int(11) NOT NULL,
AreaY Int(11) NOT NULL,
AreaWidth Int(11) NOT NULL,
AreaHeight Int(11) NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
KEY `IX_PhotoId` (`PhotoId`),
KEY `IX_TenantTypeId` (`TenantTypeId`),
PRIMARY KEY (`LabelId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;
SET FOREIGN_KEY_CHECKS = 0;


DROP TABLE IF EXISTS `spb_Photos`;
CREATE TABLE `spb_Photos` (
PhotoId BigInt NOT NULL AUTO_INCREMENT,
AlbumId BigInt NOT NULL,
TenantTypeId Char(6) NOT NULL,
OwnerId BigInt NOT NULL,
UserId BigInt NOT NULL,
Author Varchar(64) NOT NULL,
RelativePath Varchar(128) NOT NULL,
OriginalUrl Varchar(256) NOT NULL,
`Description` Varchar(512) NOT NULL,
AuditStatus SmallInt NOT NULL DEFAULT 40,
IsEssential TinyInt NOT NULL DEFAULT 0,
IP Varchar(64) NOT NULL DEFAULT '',
PrivacyStatus SmallInt NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
KEY `IX_AlbumId` (`AlbumId`),
KEY `IX_AuditStatus` (`AuditStatus`),
KEY `IX_IsEssential` (`IsEssential`),
KEY `IX_OwnerId` (`OwnerId`),
KEY `IX_PrivacyStatus` (`PrivacyStatus`),
KEY `IX_TenantTypeId` (`TenantTypeId`),
KEY `IX_UserId` (`UserId`),
PRIMARY KEY (`PhotoId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;
