/****** Object:  Table [dbo].[spb_AskAnswers]    Script Date: 03/28/2013 17:51:00 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_AskAnswers_AuditStatus]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_AskAnswers] DROP CONSTRAINT [DF_spb_AskAnswers_AuditStatus]
END
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_AskAnswers_IP]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_AskAnswers] DROP CONSTRAINT [DF_spb_AskAnswers_IP]
END
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskAnswers]') AND type in (N'U'))
DROP TABLE [dbo].[spb_AskAnswers]
/****** Object:  Table [dbo].[spb_AskQuestions]    Script Date: 03/28/2013 17:51:00 ******/
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_AskQuestions_OwnerId]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_AskQuestions] DROP CONSTRAINT [DF_spb_AskQuestions_OwnerId]
END
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_AskQuestions_AuditStatus]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_AskQuestions] DROP CONSTRAINT [DF_spb_AskQuestions_AuditStatus]
END
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_AskQuestions_IsEssential]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_AskQuestions] DROP CONSTRAINT [DF_spb_AskQuestions_IsEssential]
END
IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_spb_AskQuestions_IP]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[spb_AskQuestions] DROP CONSTRAINT [DF_spb_AskQuestions_IP]
END
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND type in (N'U'))
DROP TABLE [dbo].[spb_AskQuestions]
/****** Object:  Table [dbo].[spb_AskQuestions]    Script Date: 03/28/2013 17:51:00 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[spb_AskQuestions](
	[QuestionId] [bigint] IDENTITY(1,1) NOT NULL,
	[TenantTypeId] [char](6) NOT NULL,
	[OwnerId] [bigint] NOT NULL CONSTRAINT [DF_spb_AskQuestions_OwnerId]  DEFAULT ((0)),
	[UserId] [bigint] NOT NULL,
	[Author] [nvarchar](64) NOT NULL,
	[Subject] [nvarchar](255) NOT NULL,
	[Body] [nvarchar](max) NULL,
	[Reward] [int] NOT NULL,
	[AnswerCount] [int] NOT NULL,
	[LastAnswerUserId] [bigint] NULL,
	[LastAnswerAuthor] [nvarchar](64) NULL,
	[LastAnswerDate] [datetime] NULL,
	[Status] [smallint] NOT NULL,
	[AuditStatus] [smallint] NOT NULL CONSTRAINT [DF_spb_AskQuestions_AuditStatus]  DEFAULT ((40)),
	[IsEssential] [tinyint] NOT NULL CONSTRAINT [DF_spb_AskQuestions_IsEssential]  DEFAULT ((0)),
	[IP] [nvarchar](64) NOT NULL CONSTRAINT [DF_spb_AskQuestions_IP]  DEFAULT (''),
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[LastModifier] [nvarchar](64) NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
	[LastModifiedUserId] [bigint] NOT NULL,
 CONSTRAINT [PK_spb_AskQuestions] PRIMARY KEY CLUSTERED 
(
	[QuestionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND name = N'IX_spb_AskQuestions_AnswerCount')
CREATE NONCLUSTERED INDEX [IX_spb_AskQuestions_AnswerCount] ON [dbo].[spb_AskQuestions] 
(
	[AnswerCount] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND name = N'IX_spb_AskQuestions_AuditStatus')
CREATE NONCLUSTERED INDEX [IX_spb_AskQuestions_AuditStatus] ON [dbo].[spb_AskQuestions] 
(
	[AuditStatus] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND name = N'IX_spb_AskQuestions_OwnerId')
CREATE NONCLUSTERED INDEX [IX_spb_AskQuestions_OwnerId] ON [dbo].[spb_AskQuestions] 
(
	[OwnerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND name = N'IX_spb_AskQuestions_Reward')
CREATE NONCLUSTERED INDEX [IX_spb_AskQuestions_Reward] ON [dbo].[spb_AskQuestions] 
(
	[Reward] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND name = N'IX_spb_AskQuestions_Status')
CREATE NONCLUSTERED INDEX [IX_spb_AskQuestions_Status] ON [dbo].[spb_AskQuestions] 
(
	[Status] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND name = N'IX_spb_AskQuestions_TenantTypeId')
CREATE NONCLUSTERED INDEX [IX_spb_AskQuestions_TenantTypeId] ON [dbo].[spb_AskQuestions] 
(
	[TenantTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskQuestions]') AND name = N'IX_spb_AskQuestions_UserId')
CREATE NONCLUSTERED INDEX [IX_spb_AskQuestions_UserId] ON [dbo].[spb_AskQuestions] 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'QuestionId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'问题Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'QuestionId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'TenantTypeId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'租户类型Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'TenantTypeId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'OwnerId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'拥有者Id（独立问答为0；所属为群组时为群组Id）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'OwnerId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'UserId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'提问者用户Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'UserId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'Author'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'提问者DisplayName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'Author'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'Subject'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'标题' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'Subject'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'Body'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'补充说明' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'Body'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'Reward'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'悬赏分值' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'Reward'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'AnswerCount'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回答数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'AnswerCount'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'LastAnswerUserId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后回答用户Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'LastAnswerUserId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'LastAnswerAuthor'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后回答用户DisplayName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'LastAnswerAuthor'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'LastAnswerDate'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后回答时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'LastAnswerDate'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'Status'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'状态（待解决、已解决、取消）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'Status'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'AuditStatus'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'审核状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'AuditStatus'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'IsEssential'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否精华，由管理员设置' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'IsEssential'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'IP'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'提问者IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'IP'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'DateCreated'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'DateCreated'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'LastModified'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后更新日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'LastModified'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskQuestions', N'COLUMN',N'LastModifier'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后更新人的DisplayName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskQuestions', @level2type=N'COLUMN',@level2name=N'LastModifier'
/****** Object:  Table [dbo].[spb_AskAnswers]    Script Date: 03/28/2013 17:51:00 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskAnswers]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[spb_AskAnswers](
	[AnswerId] [bigint] IDENTITY(1,1) NOT NULL,
	[QuestionId] [bigint] NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Author] [nvarchar](64) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[IsBest] [tinyint] NOT NULL,
	[AuditStatus] [smallint] NOT NULL CONSTRAINT [DF_spb_AskAnswers_AuditStatus]  DEFAULT ((40)),
	[IP] [nvarchar](64) NOT NULL CONSTRAINT [DF_spb_AskAnswers_IP]  DEFAULT (''),
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_AskAnswers] PRIMARY KEY CLUSTERED 
(
	[AnswerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskAnswers]') AND name = N'IX_spb_AskAnswers_AuditStatus')
CREATE NONCLUSTERED INDEX [IX_spb_AskAnswers_AuditStatus] ON [dbo].[spb_AskAnswers] 
(
	[AuditStatus] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskAnswers]') AND name = N'IX_spb_AskAnswers_IsBest')
CREATE NONCLUSTERED INDEX [IX_spb_AskAnswers_IsBest] ON [dbo].[spb_AskAnswers] 
(
	[IsBest] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskAnswers]') AND name = N'IX_spb_AskAnswers_QuestionId')
CREATE NONCLUSTERED INDEX [IX_spb_AskAnswers_QuestionId] ON [dbo].[spb_AskAnswers] 
(
	[QuestionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[spb_AskAnswers]') AND name = N'IX_spb_AskAnswers_UserId')
CREATE NONCLUSTERED INDEX [IX_spb_AskAnswers_UserId] ON [dbo].[spb_AskAnswers] 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'AnswerId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回答Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'AnswerId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'QuestionId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'问题Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'QuestionId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'UserId'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回答者用户Id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'UserId'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'Author'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回答者DisplayName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'Author'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'Body'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'补充说明' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'Body'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'IsBest'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'是否最佳回答' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'IsBest'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'AuditStatus'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'审核状态' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'AuditStatus'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'IP'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'回答者IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'IP'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'DateCreated'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'创建时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'DateCreated'
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'spb_AskAnswers', N'COLUMN',N'LastModified'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最后更新日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'spb_AskAnswers', @level2type=N'COLUMN',@level2name=N'LastModified'
