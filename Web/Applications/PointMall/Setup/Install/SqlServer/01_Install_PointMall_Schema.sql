IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_MailAddress]') AND type in (N'U'))
DROP TABLE [dbo].[spb_MailAddress]
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_PointGiftExchangeRecords]') AND type in (N'U'))
DROP TABLE [dbo].[spb_PointGiftExchangeRecords]
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_PointGifts]') AND type in (N'U'))
DROP TABLE [dbo].[spb_PointGifts]
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_PointGifts]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[spb_PointGifts](
	[GiftId] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[FeaturedImageAttachmentId] [bigint] NOT NULL,
	[FeaturedImage] [nvarchar](255) NOT NULL,
	[Price] [int] NOT NULL,
	[ExchangedCount] [int] NOT NULL,
	[IsEnabled] [tinyint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[FeaturedImageIds] [nvarchar](255) NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_PointGifts] PRIMARY KEY CLUSTERED 
(
	[GiftId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_PointGiftExchangeRecords]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[spb_PointGiftExchangeRecords](
	[RecordId] [bigint] IDENTITY(1,1) NOT NULL,
	[GiftId] [bigint] NOT NULL,
	[GiftName] [nvarchar](128) NOT NULL,
	[PayerUserId] [bigint] NOT NULL,
	[Payer] [nvarchar](128) NOT NULL,
	[Number] [int] NOT NULL,
	[Price] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[Appraise] [nvarchar](max) NOT NULL,
	[AppraiseDate] [datetime] NULL,
	[TrackInfo] [nvarchar](255) NOT NULL,
	[Status] [int] NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_PointGiftExchangeRecords] PRIMARY KEY CLUSTERED 
(
	[RecordId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spb_MailAddress]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[spb_MailAddress](
	[AddressId] [bigint] IDENTITY(1,1) NOT NULL,
	[Addressee] [nvarchar](128) NOT NULL,
	[Tel] [nvarchar](64) NOT NULL,
	[Address] [nvarchar](512) NOT NULL,
	[PostCode] [nvarchar](32) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[PropertyNames] [nvarchar](max) NULL,
	[PropertyValues] [nvarchar](max) NULL,
 CONSTRAINT [PK_spb_MailAddress] PRIMARY KEY CLUSTERED 
(
	[AddressId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
