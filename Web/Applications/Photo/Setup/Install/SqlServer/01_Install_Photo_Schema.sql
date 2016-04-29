IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_Albums_CoverId]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_Albums] DROP CONSTRAINT [DF_spb_Albums_CoverId]
END
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_Albums_AuditStatus]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_Albums] DROP CONSTRAINT [DF_spb_Albums_AuditStatus]
END
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_Albums]') AND type in (N'U'))
DROP TABLE [dbo].[spb_Albums]
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_PhotoLabels]') AND type in (N'U'))
DROP TABLE [dbo].[spb_PhotoLabels]
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_Photos_AuditStatus]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_Photos] DROP CONSTRAINT [DF_spb_Photos_AuditStatus]
END
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_Photos_IsEssential]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_Photos] DROP CONSTRAINT [DF_spb_Photos_IsEssential]
END
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_Photos_IP]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_Photos] DROP CONSTRAINT [DF_spb_Photos_IP]
END
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND type in (N'U'))
DROP TABLE [dbo].[spb_Photos]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[spb_Photos](
	[PhotoId] [bigint] IDENTITY(1,1) NOT NULL,
	[AlbumId] [bigint] NOT NULL,
	[TenantTypeId] [char](6) NOT NULL,
	[OwnerId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Author] [nvarchar](64) NOT NULL,
	[RelativePath] [nvarchar](128) NOT NULL,
	[OriginalUrl] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](512) NOT NULL,
	[AuditStatus] [smallint] NOT NULL CONSTRAINT [DF_spb_Photos_AuditStatus]  DEFAULT ((40)),
	[IsEssential] [tinyint] NOT NULL CONSTRAINT [DF_spb_Photos_IsEssential]  DEFAULT ((0)),
	[IP] [nvarchar](64) NOT NULL CONSTRAINT [DF_spb_Photos_IP]  DEFAULT (''),
	[PrivacyStatus] [smallint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_Photos] PRIMARY KEY CLUSTERED 
(
	[PhotoId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND name = N'IX_spb_Photos_AlbumId')
CREATE NONCLUSTERED INDEX [IX_spb_Photos_AlbumId] ON [dbo].[spb_Photos] 
(
	[AlbumId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND name = N'IX_spb_Photos_AuditStatus')
CREATE NONCLUSTERED INDEX [IX_spb_Photos_AuditStatus] ON [dbo].[spb_Photos] 
(
	[AuditStatus] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND name = N'IX_spb_Photos_IsEssential')
CREATE NONCLUSTERED INDEX [IX_spb_Photos_IsEssential] ON [dbo].[spb_Photos] 
(
	[IsEssential] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND name = N'IX_spb_Photos_OwnerId')
CREATE NONCLUSTERED INDEX [IX_spb_Photos_OwnerId] ON [dbo].[spb_Photos] 
(
	[OwnerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND name = N'IX_spb_Photos_PrivacyStatus')
CREATE NONCLUSTERED INDEX [IX_spb_Photos_PrivacyStatus] ON [dbo].[spb_Photos] 
(
	[PrivacyStatus] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND name = N'IX_spb_Photos_TenantTypeId')
CREATE NONCLUSTERED INDEX [IX_spb_Photos_TenantTypeId] ON [dbo].[spb_Photos] 
(
	[TenantTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Photos]') AND name = N'IX_spb_Photos_UserId')
CREATE NONCLUSTERED INDEX [IX_spb_Photos_UserId] ON [dbo].[spb_Photos] 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'PhotoId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'照片ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'PhotoId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'AlbumId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'相册ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'AlbumId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'TenantTypeId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'租户类型，同步自相册' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'TenantTypeId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'OwnerId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所有者ID，同步自相册' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'OwnerId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'UserId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'相册作者ID，同步自相册' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'UserId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'Author'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作者名称，同步自相册' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'Author'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'RelativePath'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'照片相对物理地址，用于直连访问' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'RelativePath'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'OriginalUrl'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'采集照片的原始地址' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'OriginalUrl'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'Description'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'照片描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'Description'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'AuditStatus'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'审核状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'AuditStatus'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'IsEssential'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否精华' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'IsEssential'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'IP'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ip地址' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'IP'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'PrivacyStatus'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'隐私状态，同步自相册' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'PrivacyStatus'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'DateCreated'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'DateCreated'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Photos', N'COLUMN',N'LastModified'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'修改时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Photos', @level2type=N'COLUMN',@level2name=N'LastModified'
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_PhotoLabels]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[spb_PhotoLabels](
	[LabelId] [bigint] IDENTITY(1,1) NOT NULL,
	[PhotoId] [bigint] NOT NULL,
	[TenantTypeId] [char](6) NOT NULL,
	[ObjetId] [bigint] NOT NULL,
	[ObjectName] [nvarchar](255) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Description] [nvarchar](512) NOT NULL,
	[AreaX] [int] NOT NULL,
	[AreaY] [int] NOT NULL,
	[AreaWidth] [int] NOT NULL,
	[AreaHeight] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_PhotoLabels] PRIMARY KEY CLUSTERED 
(
	[LabelId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_PhotoLabels]') AND name = N'IX_spb_PhotoLabels_PhotoId')
CREATE NONCLUSTERED INDEX [IX_spb_PhotoLabels_PhotoId] ON [dbo].[spb_PhotoLabels] 
(
	[PhotoId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_PhotoLabels]') AND name = N'IX_spb_PhotoLabels_TenantTypeId')
CREATE NONCLUSTERED INDEX [IX_spb_PhotoLabels_TenantTypeId] ON [dbo].[spb_PhotoLabels] 
(
	[TenantTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'LabelId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'圈人Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'LabelId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'PhotoId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'照片ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'PhotoId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'TenantTypeId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'租户类型ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'TenantTypeId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'ObjetId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'被圈人（对象）ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'ObjetId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'ObjectName'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'被圈人（对象）名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'ObjectName'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'UserId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'UserId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'Description'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'Description'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'AreaX'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'被圈区域X坐标' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'AreaX'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'AreaY'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'被圈区域Y坐标' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'AreaY'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'AreaWidth'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'被圈区域宽度' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'AreaWidth'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'AreaHeight'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'被圈区域高度' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'AreaHeight'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'DateCreated'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'DateCreated'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_PhotoLabels', N'COLUMN',N'LastModified'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'修改时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_PhotoLabels', @level2type=N'COLUMN',@level2name=N'LastModified'
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_Albums]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[spb_Albums](
	[AlbumId] [bigint] IDENTITY(1,1) NOT NULL,
	[AlbumName] [nvarchar](255) NOT NULL,
	[TenantTypeId] [nchar](6) NOT NULL,
	[OwnerId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Author] [nvarchar](64) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[CoverId] [bigint] NOT NULL CONSTRAINT [DF_spb_Albums_CoverId]  DEFAULT ((0)),
	[PhotoCount] [int] NOT NULL,
	[DisplayOrder] [bigint] NOT NULL,
	[AuditStatus] [smallint] NOT NULL CONSTRAINT [DF_spb_Albums_AuditStatus]  DEFAULT ((40)),
	[PrivacyStatus] [smallint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[LastUploadDate] [datetime] NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_Albums] PRIMARY KEY CLUSTERED 
(
	[AlbumId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Albums]') AND name = N'IX_spb_Albums_AuditStatus')
CREATE NONCLUSTERED INDEX [IX_spb_Albums_AuditStatus] ON [dbo].[spb_Albums] 
(
	[AuditStatus] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Albums]') AND name = N'IX_spb_Albums_DisplayOrder')
CREATE NONCLUSTERED INDEX [IX_spb_Albums_DisplayOrder] ON [dbo].[spb_Albums] 
(
	[DisplayOrder] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Albums]') AND name = N'IX_spb_Albums_OwnerId')
CREATE NONCLUSTERED INDEX [IX_spb_Albums_OwnerId] ON [dbo].[spb_Albums] 
(
	[OwnerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Albums]') AND name = N'IX_spb_Albums_PrivacyStatus')
CREATE NONCLUSTERED INDEX [IX_spb_Albums_PrivacyStatus] ON [dbo].[spb_Albums] 
(
	[PrivacyStatus] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Albums]') AND name = N'IX_spb_Albums_TenantTypeId')
CREATE NONCLUSTERED INDEX [IX_spb_Albums_TenantTypeId] ON [dbo].[spb_Albums] 
(
	[TenantTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_Albums]') AND name = N'IX_spb_Albums_UserId')
CREATE NONCLUSTERED INDEX [IX_spb_Albums_UserId] ON [dbo].[spb_Albums] 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'AlbumId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'相册ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'AlbumId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'AlbumName'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'相册名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'AlbumName'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'TenantTypeId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'租户类型' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'TenantTypeId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'OwnerId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'所有者ID，用户的相册，OwnerId=UserId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'OwnerId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'UserId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'相册作者ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'UserId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'Author'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作者名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'Author'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'Description'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'相册描述' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'Description'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'CoverId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'封面照片ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'CoverId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'PhotoCount'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'照片数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'PhotoCount'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'DisplayOrder'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'排序，默认与主键相同' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'DisplayOrder'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'AuditStatus'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'审核状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'AuditStatus'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'PrivacyStatus'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'隐私状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'PrivacyStatus'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'DateCreated'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'DateCreated'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'LastModified'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'修改时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'LastModified'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_Albums', N'COLUMN',N'LastUploadDate'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后上传照片时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_Albums', @level2type=N'COLUMN',@level2name=N'LastUploadDate'
