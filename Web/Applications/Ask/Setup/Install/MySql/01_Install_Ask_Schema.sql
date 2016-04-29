SET FOREIGN_KEY_CHECKS = 0;


DROP TABLE IF EXISTS `spb_AskAnswers`;
CREATE TABLE `spb_AskAnswers` (
AnswerId BigInt NOT NULL AUTO_INCREMENT,
QuestionId BigInt NOT NULL,
UserId BigInt NOT NULL,
Author Varchar(64) NOT NULL,
Body mediumtext NOT NULL,
IsBest TinyInt NOT NULL,
AuditStatus SmallInt NOT NULL,
IP Varchar(64) NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
KEY `IX_AuditStatus` (`AuditStatus`),
KEY `IX_IsBest` (`IsBest`),
KEY `IX_QuestionId` (`QuestionId`),
KEY `IX_UserId` (`UserId`),
PRIMARY KEY (`AnswerId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;
SET FOREIGN_KEY_CHECKS = 0;


DROP TABLE IF EXISTS `spb_AskQuestions`;
CREATE TABLE `spb_AskQuestions` (
QuestionId BigInt NOT NULL AUTO_INCREMENT,
TenantTypeId Char(6) NOT NULL,
OwnerId BigInt NOT NULL,
UserId BigInt NOT NULL,
Author Varchar(64) NOT NULL,
`Subject` Varchar(256) NOT NULL,
Body mediumtext DEFAULT NULL,
Reward Int(11) NOT NULL,
AnswerCount Int(11) NOT NULL,
LastAnswerUserId BigInt DEFAULT NULL,
LastAnswerAuthor Varchar(64) DEFAULT NULL,
LastAnswerDate DateTime DEFAULT NULL,
`Status` SmallInt NOT NULL,
AuditStatus SmallInt NOT NULL,
IsEssential TinyInt NOT NULL,
IP Varchar(64) NOT NULL,
DateCreated DateTime NOT NULL,
LastModified DateTime NOT NULL,
LastModifier Varchar(64) NOT NULL,
PropertyNames mediumtext DEFAULT NULL,
PropertyValues mediumtext DEFAULT NULL,
LastModifiedUserId BigInt NOT NULL,
KEY `IX_AnswerCount` (`AnswerCount`),
KEY `IX_AuditStatus` (`AuditStatus`),
KEY `IX_OwnerId` (`OwnerId`),
KEY `IX_Reward` (`Reward`),
KEY `IX_Status` (`Status`),
KEY `IX_TenantTypeId` (`TenantTypeId`),
KEY `IX_UserId` (`UserId`),
PRIMARY KEY (`QuestionId`)
)ENGINE=innodb DEFAULT CHARSET=utf8;
