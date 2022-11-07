USE [Observer_new]
GO

/****** Object:  Table [dbo].[subscriber]    Script Date: 27/10/2022 7:57:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[subscriber_roles](
	[roleID] [int] IDENTITY(9001,1) NOT NULL,
	[roleDescription] [varchar](50) NOT NULL,
	[createdAt] [datetime] NOT NULL,
 CONSTRAINT [PK_subscriber_roles] PRIMARY KEY CLUSTERED 
(
	[roleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


INSERT INTO [dbo].[subscriber_roles]
           ([roleDescription]
           ,[createdAt])
     VALUES
            ('Admin', GETDATE())
           ,('User', GETDATE())
           ,('Staff', GETDATE())
GO


CREATE TABLE [dbo].[subscriber](
	[SubscriberID] [int] IDENTITY(1,1) NOT NULL,
	[emailAddress] [varchar](50) NOT NULL,
	[firstName] [varchar](50) NOT NULL,
	[lastName] [varchar](50) NOT NULL,
	[dateOfBirth] [datetime] NULL,
	[passwordHash] [varchar](300) NOT NULL,
	[secretquestion] [varchar](100) NULL,
	[secretans] [varchar](50) NULL,
	[ipAddress] [varchar](50) NULL,
	[isActive] [bit] NOT NULL,
	[addressID] [int] NULL,
	[phoneNumber] [varchar](50) NULL,
	[newsletter] [bit] NULL,
	[createdAt] [datetime] NOT NULL,
	[roleID] [int] NULL,
	[token] [varchar](50) NULL,
	[ccHashID] [int] NULL,
	[lastLogin] [datetime] NULL,
 CONSTRAINT [PK_subscriber] PRIMARY KEY CLUSTERED 
(
	[SubscriberID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[subscriber_address]    Script Date: 27/10/2022 7:57:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[subscriber_address](
	[addressID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriberID] [int],
	[addressType] [varchar](2) NULL,
	[emailAddress] [varchar](50) NOT NULL,
	[addressLine1] [varchar](100) NOT NULL,
	[addressLine2] [varchar](100) NULL,
	[cityTown] [varchar](50) NULL,
	[stateParish] [varchar](50) NULL,
	[zipCode] [varchar](15) NULL,
	[country] [varchar](50) NOT NULL,
	[createdAt] [datetime] NOT NULL,
	[lastLogin] [datetime] NULL,
 CONSTRAINT [PK_subscriber_address] PRIMARY KEY CLUSTERED 
(
	[addressID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[subscriber_epaper]    Script Date: 27/10/2022 7:57:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[subscriber_epaper](
	[epaperID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriberID] [int] NOT NULL,
	[emailAddress] [varchar](50) NOT NULL,
	[token] [varchar](50) NULL,
	[rateID] [int] NOT NULL,
	[startDate] [datetime] NOT NULL,
	[endDate] [datetime] NOT NULL,
	[isActive] [bit] NOT NULL,
	[subType] [varchar](50) NULL,
	[createdAt] [datetime] NOT NULL,
	[notificationEmail] [varchar](50) NULL,
	
 CONSTRAINT [PK_subscriber_epaper] PRIMARY KEY CLUSTERED 
(
	[epaperID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[subscriber_print]    Script Date: 27/10/2022 7:57:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[subscriber_print](
	[printID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriberID] [int] NOT NULL,
	[emailAddress] [varchar](50) NOT NULL,
	[rateID] [int] NOT NULL,
	[addressID] [int] NOT NULL,
	[startDate] [datetime] NOT NULL,
	[endDate] [datetime] NOT NULL,
	[isActive] [bit] NOT NULL,
	[deliveryInstructions] [varchar](200) NULL,
	[circprosubid] [varchar](50) NULL,
	[createdAt] [datetime] NOT NULL,
 CONSTRAINT [PK_subscriber_print] PRIMARY KEY CLUSTERED 
(
	[printID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[subscriber_tranx]    Script Date: 27/10/2022 7:57:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[subscriber_tranx](
	[tranxID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriberID] [int] NULL,
	[emailAddress] [varchar](50) NULL,
	[cardOwner] [varchar](50) NULL,
	[cardType] [varchar](20) NULL,
	[cardExp] [varchar](10) NULL,
	[cardLastFour] [varchar](10) NULL,
	[tranxDate] [datetime] NULL,
	[rateID] [int] NULL,
	[tranxType] [varchar](50) NULL,
	[orderID] [varchar](50) NULL,
	[tranxAmount] [float] NULL,
	[tranxNotes] [varchar](50) NULL,
	[ipAddress] [varchar](50) NULL,
 CONSTRAINT [PK_subscriber_tranx] PRIMARY KEY CLUSTERED 
(
	[tranxID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[subscriber]  WITH CHECK ADD  CONSTRAINT [address] FOREIGN KEY([addressID])
REFERENCES [dbo].[subscriber_address] ([addressID])
GO

ALTER TABLE [dbo].[subscriber] CHECK CONSTRAINT [address]
GO

ALTER TABLE [dbo].[subscriber]  WITH NOCHECK ADD  CONSTRAINT [roles] FOREIGN KEY([roleID])
REFERENCES [dbo].[subscriber_roles] ([roleID])
GO

ALTER TABLE [dbo].[subscriber] CHECK CONSTRAINT [roles]
GO

ALTER TABLE [dbo].[subscriber_epaper]  WITH NOCHECK ADD  CONSTRAINT [epaper] FOREIGN KEY([SubscriberID])
REFERENCES [dbo].[subscriber] ([SubscriberID])
GO

ALTER TABLE [dbo].[subscriber_epaper] CHECK CONSTRAINT [epaper]
GO

ALTER TABLE [dbo].[subscriber_print]  WITH NOCHECK ADD  CONSTRAINT [print] FOREIGN KEY([SubscriberID])
REFERENCES [dbo].[subscriber] ([SubscriberID])
GO

ALTER TABLE [dbo].[subscriber_print] CHECK CONSTRAINT [print]
GO

ALTER TABLE [dbo].[subscriber_tranx]  WITH NOCHECK ADD  CONSTRAINT [tranx] FOREIGN KEY([SubscriberID])
REFERENCES [dbo].[subscriber] ([SubscriberID])
GO

ALTER TABLE [dbo].[subscriber_tranx] CHECK CONSTRAINT [tranx]
GO


