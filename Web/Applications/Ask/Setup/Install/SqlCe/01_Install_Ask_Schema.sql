
CREATE TABLE [spb_AskQuestions] (
  [QuestionId] bigint NOT NULL  IDENTITY (1,1)
, [TenantTypeId] nchar(6) NOT NULL
, [OwnerId] bigint NOT NULL DEFAULT ((0))
, [UserId] bigint NOT NULL
, [Author] nvarchar(64) NOT NULL
, [Subject] nvarchar(256) NOT NULL
, [Body] ntext NULL
, [Reward] int NOT NULL
, [AnswerCount] int NOT NULL
, [LastAnswerUserId] bigint NULL
, [LastAnswerAuthor] nvarchar(64) NULL
, [LastAnswerDate] datetime NULL
, [Status] smallint NOT NULL
, [AuditStatus] smallint NOT NULL DEFAULT ((40))
, [IsEssential] tinyint NOT NULL DEFAULT ((0))
, [IP] nvarchar(64) NOT NULL DEFAULT ('')
, [DateCreated] datetime NOT NULL
, [LastModified] datetime NOT NULL
, [LastModifier] nvarchar(64) NOT NULL
, [PropertyNames] ntext NULL
, [PropertyValues] ntext NULL
, [LastModifiedUserId] bigint NOT NULL
);
GO
CREATE TABLE [spb_AskAnswers] (
  [AnswerId] bigint NOT NULL  IDENTITY (1,1)
, [QuestionId] bigint NOT NULL
, [UserId] bigint NOT NULL
, [Author] nvarchar(64) NOT NULL
, [Body] ntext NOT NULL
, [IsBest] tinyint NOT NULL
, [AuditStatus] smallint NOT NULL DEFAULT ((40))
, [IP] nvarchar(64) NOT NULL DEFAULT ('')
, [DateCreated] datetime NOT NULL
, [LastModified] datetime NOT NULL
, [PropertyNames] ntext NULL
, [PropertyValues] ntext NULL
);
GO
ALTER TABLE [spb_AskQuestions] ADD CONSTRAINT [PK_spb_AskQuestions] PRIMARY KEY ([QuestionId]);
GO
ALTER TABLE [spb_AskAnswers] ADD CONSTRAINT [PK_spb_AskAnswers] PRIMARY KEY ([AnswerId]);
GO
CREATE INDEX [IX_spb_AskQuestions_AnswerCount] ON [spb_AskQuestions] ([AnswerCount] ASC);
GO
CREATE INDEX [IX_spb_AskQuestions_AuditStatus] ON [spb_AskQuestions] ([AuditStatus] ASC);
GO
CREATE INDEX [IX_spb_AskQuestions_OwnerId] ON [spb_AskQuestions] ([OwnerId] ASC);
GO
CREATE INDEX [IX_spb_AskQuestions_Reward] ON [spb_AskQuestions] ([Reward] ASC);
GO
CREATE INDEX [IX_spb_AskQuestions_Status] ON [spb_AskQuestions] ([Status] ASC);
GO
CREATE INDEX [IX_spb_AskQuestions_TenantTypeId] ON [spb_AskQuestions] ([TenantTypeId] ASC);
GO
CREATE INDEX [IX_spb_AskQuestions_UserId] ON [spb_AskQuestions] ([UserId] ASC);
GO
CREATE INDEX [IX_spb_AskAnswers_AuditStatus] ON [spb_AskAnswers] ([AuditStatus] ASC);
GO
CREATE INDEX [IX_spb_AskAnswers_IsBest] ON [spb_AskAnswers] ([IsBest] ASC);
GO
CREATE INDEX [IX_spb_AskAnswers_QuestionId] ON [spb_AskAnswers] ([QuestionId] ASC);
GO
CREATE INDEX [IX_spb_AskAnswers_UserId] ON [spb_AskAnswers] ([UserId] ASC);
GO

