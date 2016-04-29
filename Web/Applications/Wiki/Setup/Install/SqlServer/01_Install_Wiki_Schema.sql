

/****** Object:  Table [dbo].[spb_WikiPages]    Script Date: 12/03/2013 10:07:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[spb_WikiPages](
	[PageId] [bigint] IDENTITY(1,1) NOT NULL,
	[TenantTypeId] [char](6) NOT NULL,
	[OwnerId] [bigint] NOT NULL,
	[Title] [nvarchar](128) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Author] [nvarchar](64) NOT NULL,
	[AuditStatus] [int] NOT NULL,
	[EditionCount] [int] NOT NULL,
	[IsEssential] [tinyint] NOT NULL,
	[IsLocked] [tinyint] NOT NULL,
	[IsLogicalDelete] [tinyint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[FeaturedImageAttachmentId] [bigint] NOT NULL,
	[FeaturedImage] [nvarchar](255) NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_WikiPages] PRIMARY KEY CLUSTERED 
(
	[PageId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'词条Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'PageId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'租户类型Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'TenantTypeId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'拥有者Id（独立百科为0；所属为群组时为群组Id）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'OwnerId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'词条名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'Title'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建者UserId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'UserId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建者DisplayName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'Author'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'审核状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'AuditStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'编辑次数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'EditionCount'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否是精华' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'IsEssential'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否锁定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'IsLocked'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否被逻辑删除' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'IsLogicalDelete'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'DateCreated'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后更新日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'LastModified'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标题图对应的附件Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'FeaturedImageAttachmentId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标题图文件（带部分路径）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'FeaturedImage'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPages', @level2type=N'COLUMN',@level2name=N'PropertyNames'
GO

ALTER TABLE [dbo].[spb_WikiPages] ADD  CONSTRAINT [DF_spb_WikiPages_FeaturedImageAttachmentId]  DEFAULT ((0)) FOR [FeaturedImageAttachmentId]
GO

ALTER TABLE [dbo].[spb_WikiPages] ADD  CONSTRAINT [DF_spb_WikiPages_FeaturedImage]  DEFAULT ('') FOR [FeaturedImage]
GO



/****** Object:  Table [dbo].[spb_WikiPageVersions]    Script Date: 12/03/2013 10:08:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[spb_WikiPageVersions](
	[VersionId] [bigint] IDENTITY(1,1) NOT NULL,
	[PageId] [bigint] NOT NULL,
	[TenantTypeId] [char](6) NOT NULL,
	[OwnerId] [bigint] NOT NULL,
	[VersionNum] [int] NOT NULL,
	[Title] [nvarchar](128) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Author] [nvarchar](64) NOT NULL,
	[Summary] [nvarchar](255) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[Reason] [nvarchar](512) NOT NULL,
	[AuditStatus] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[IP] [nvarchar](64) NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_WikiPageVersions] PRIMARY KEY CLUSTERED 
(
	[VersionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'租户类型Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'TenantTypeId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'拥有者Id（独立百科为0；所属为群组时为群组Id）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'OwnerId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'版本号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'VersionNum'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'词条名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'Title'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'编辑者UserId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'UserId'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'编辑者DisplayName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'Author'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'摘要' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'Summary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'Body'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'修改原因' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'Reason'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'审核状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'AuditStatus'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'发布日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'DateCreated'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'IP地址' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_WikiPageVersions', @level2type=N'COLUMN',@level2name=N'IP'
GO

ALTER TABLE [dbo].[spb_WikiPageVersions] ADD  CONSTRAINT [DF_spb_WikiPageVersions_TenantTypeId]  DEFAULT ('') FOR [TenantTypeId]
GO

ALTER TABLE [dbo].[spb_WikiPageVersions] ADD  CONSTRAINT [DF_spb_WikiPageVersions_OwnerId]  DEFAULT ((0)) FOR [OwnerId]
GO


