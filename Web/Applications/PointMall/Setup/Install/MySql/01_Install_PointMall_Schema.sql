SET FOREIGN_KEY_CHECKS = 0;


DROP TABLE IF EXISTS `spb_MailAddress`;
CREATE TABLE `spb_MailAddress` (
AddressId BigInt NOT NULL AUTO_INCREMENT,
Addressee Varchar(128) NOT NULL,
Tel Varchar(64) NOT NULL,
Address Varchar(512) NOT NULL,
PostCode Varchar(32) NOT NULL,
UserId BigInt NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
PRIMARY KEY (`AddressId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;
SET FOREIGN_KEY_CHECKS = 0;


DROP TABLE IF EXISTS `spb_PointGiftExchangeRecords`;
CREATE TABLE `spb_PointGiftExchangeRecords` (
RecordId BigInt NOT NULL AUTO_INCREMENT,
GiftId BigInt NOT NULL,
GiftName Varchar(128) NOT NULL,
PayerUserId BigInt NOT NULL,
Payer Varchar(128) NOT NULL,
Number Int(11) NOT NULL,
Price Int(11) NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
Appraise mediumtext NOT NULL,
AppraiseDate DateTime DEFAULT NULL,
TrackInfo Varchar(256) NOT NULL,
Status Int(11) NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
PRIMARY KEY (`RecordId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;
SET FOREIGN_KEY_CHECKS = 0;


DROP TABLE IF EXISTS `spb_PointGifts`;
CREATE TABLE `spb_PointGifts` (
GiftId BigInt NOT NULL AUTO_INCREMENT,
Name Varchar(128) NOT NULL,
UserId BigInt NOT NULL,
Description mediumtext NOT NULL,
FeaturedImageAttachmentId BigInt NOT NULL,
FeaturedImage Varchar(255) NOT NULL,
Price Int(11) NOT NULL,
ExchangedCount Int(11) NOT NULL,
IsEnabled TinyInt NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
FeaturedImageIds Varchar(255) NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
PRIMARY KEY (`GiftId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;
