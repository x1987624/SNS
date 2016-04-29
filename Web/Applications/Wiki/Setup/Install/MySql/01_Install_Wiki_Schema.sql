SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS `spb_WikiPages`;
CREATE TABLE `spb_WikiPages` (
PageId BigInt NOT NULL AUTO_INCREMENT,
TenantTypeId Char(6) NOT NULL,
OwnerId BigInt NOT NULL,
Title Varchar(128) NOT NULL,
UserId BigInt NOT NULL,
Author Varchar(64) NOT NULL,
AuditStatus Int(11) NOT NULL,
EditionCount Int(11) NOT NULL,
IsEssential TinyInt NOT NULL,
IsLocked TinyInt NOT NULL,
IsLogicalDelete TinyInt NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
FeaturedImageAttachmentId BigInt NOT NULL DEFAULT 0,
FeaturedImage Varchar(255) NOT NULL DEFAULT '',
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
PRIMARY KEY (`PageId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;SET FOREIGN_KEY_CHECKS = 0;DROP TABLE IF EXISTS `spb_WikiPageVersions`;
CREATE TABLE `spb_WikiPageVersions` (
VersionId BigInt NOT NULL AUTO_INCREMENT,
PageId BigInt NOT NULL,
TenantTypeId Char(6) NOT NULL DEFAULT '',
OwnerId BigInt NOT NULL DEFAULT 0,
VersionNum Int(11) NOT NULL,
Title Varchar(128) NOT NULL,
UserId BigInt NOT NULL,
Author Varchar(64) NOT NULL,
Summary Varchar(512) NOT NULL,
Body mediumtext NOT NULL,
Reason Varchar(512) NOT NULL,
AuditStatus Int(11) NOT NULL,
DateCreated DateTime NOT NULL,
IP Varchar(64) NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
PRIMARY KEY (`VersionId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;