/****** Object:  Table [dbo].[Accounts]    Script Date: 08/05/2019 10:52:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[Name] [nvarchar](500) NOT NULL,
	[sfaProviderUserType] [nvarchar](100) NULL,
	[A1LifecycleState] [nvarchar](50) NULL,
	[UPIN] [nvarchar](200) NULL,
	[createtimestamp] [date] NOT NULL,
	[mail] [nvarchar](200) NOT NULL,
	[modifytimestamp] [date] NULL,
	[uid] [nvarchar](200) NULL,
 CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED 
(
	[mail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Audit]    Script Date: 08/05/2019 15:16:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Audit](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](200) NOT NULL,
	[Status] [varchar](50) NOT NULL,
	[Notes] [varchar](5000) NULL,
	[TimeStamp] [datetime] NULL,
 CONSTRAINT [PK_Audit] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Audit] ADD  CONSTRAINT [DF_Audit_TimeStamp]  DEFAULT (getdate()) FOR [TimeStamp]
GO
/****** Object:  Table [dbo].[CircuitBreaker]    Script Date: 08/05/2019 10:52:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CircuitBreaker](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LastCircuitOpenDate] [datetime] NOT NULL,
	[CircuitBreakerStatus] [nvarchar](50) NOT NULL,
	[HalfOpenRetryCount] [int] NOT NULL,
 CONSTRAINT [PK_CircuitBreaker] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX idx_audit_email ON [Audit] ([Email]);
GO

