USE [master]
GO

/****** Object:  Database [RafyUnitTest]    Script Date: 11/11/2011 13:50:54 ******/
IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'RafyUnitTest')
DROP DATABASE [RafyUnitTest]
GO

USE [master]
GO

/****** Object:  Database [RafyUnitTest]    Script Date: 11/11/2011 13:50:54 ******/
CREATE DATABASE [RafyUnitTest]
GO

USE [RafyUnitTest]
GO

/****** Object:  Table [dbo].[User]    Script Date: 11/11/2011 13:49:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[User](
	[Id] [uniqueidentifier] NOT NULL,
	[UserName] [nvarchar](200) NOT NULL,
	[Age] [int] NOT NULL,
	[UserCode] [nvarchar](200) NOT NULL,
	[TasksTime] [int] NOT NULL,
	[Level] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO





CREATE TABLE [dbo].[Role](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Role]  WITH CHECK ADD  CONSTRAINT [FK_Role_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Role] CHECK CONSTRAINT [FK_Role_User]
GO






CREATE TABLE [dbo].[Task](
	[TestUserId] [uniqueidentifier] NOT NULL,
	[AllTime] [int] NOT NULL,
	[OrderNo] [int] NOT NULL,
	[PId] [uniqueidentifier] NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Task]  WITH CHECK ADD  CONSTRAINT [FK_Task_User] FOREIGN KEY([TestUserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Task] CHECK CONSTRAINT [FK_Task_User]
GO


