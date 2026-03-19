USE [CapstoneProjectDb]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Achievements]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Achievements](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[RuleSpec] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Achievements] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChatRoomMembers]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatRoomMembers](
	[Id] [uniqueidentifier] NOT NULL,
	[ChatRoomId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[JoinedAt] [datetime2](7) NOT NULL,
	[LeftAt] [datetime2](7) NULL,
	[LastReadAt] [datetime2](7) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_ChatRoomMembers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChatRooms]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChatRooms](
	[Id] [uniqueidentifier] NOT NULL,
	[RoomType] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[IsClosed] [bit] NOT NULL,
	[ClosedAt] [datetime2](7) NULL,
	[ClosedBy] [uniqueidentifier] NULL,
	[LastMessageId] [uniqueidentifier] NULL,
	[LastMessageAt] [datetime2](7) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_ChatRooms] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Concepts]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Concepts](
	[Id] [uniqueidentifier] NOT NULL,
	[LearningGoalId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[ContentKey] [nvarchar](max) NULL,
	[SortOrder] [int] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Concepts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExecutionsResults]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExecutionsResults](
	[Id] [uniqueidentifier] NOT NULL,
	[SubmissionId] [uniqueidentifier] NOT NULL,
	[StartedAt] [datetime2](7) NULL,
	[FinishedAt] [datetime2](7) NULL,
	[IsDeterministic] [bit] NOT NULL,
	[ServerSimVersion] [nvarchar](max) NULL,
	[ResultSpec] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_ExecutionsResults] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Hints]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Hints](
	[Id] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[OrderNo] [int] NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Hints] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LearningGoals]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LearningGoals](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[SortOrder] [int] NOT NULL,
	[IconUrl] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_LearningGoals] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LearningPathItems]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LearningPathItems](
	[Id] [uniqueidentifier] NOT NULL,
	[LearningGoalId] [uniqueidentifier] NOT NULL,
	[ItemType] [int] NOT NULL,
	[ConceptId] [uniqueidentifier] NULL,
	[MapId] [uniqueidentifier] NULL,
	[SortOrder] [int] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_LearningPathItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MapDetails]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MapDetails](
	[Id] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[JsonContent] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_MapDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MapRatings]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MapRatings](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[Rating] [int] NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_MapRatings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MapReports]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MapReports](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[Reason] [nvarchar](max) NOT NULL,
	[Details] [nvarchar](max) NULL,
	[ReportStatus] [int] NOT NULL,
	[ReviewedBy] [uniqueidentifier] NULL,
	[ReviewedAt] [datetime2](7) NULL,
	[ReviewNote] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_MapReports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Maps]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Maps](
	[Id] [uniqueidentifier] NOT NULL,
	[Title] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Difficulty] [int] NOT NULL,
	[TimeLimitMs] [int] NOT NULL,
	[IsPublished] [bit] NOT NULL,
	[MapStatus] [int] NOT NULL,
	[Price] [decimal](18, 2) NULL,
	[EditorialContent] [nvarchar](max) NULL,
	[UnlockEditorialAfterStars] [int] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
	[WinCondition] [int] NOT NULL,
	[Type] [int] NOT NULL,
	[AvatarUrl] [nvarchar](max) NULL,
 CONSTRAINT [PK_Maps] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MapTags]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MapTags](
	[Id] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[TagId] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_MapTags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Matches]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Matches](
	[Id] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[StartedAt] [datetime2](7) NULL,
	[EndedAt] [datetime2](7) NULL,
	[RulesSpec] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Matches] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MessageReads]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MessageReads](
	[Id] [uniqueidentifier] NOT NULL,
	[MessageId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ReadAt] [datetime2](7) NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_MessageReads] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Messages]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Messages](
	[Id] [uniqueidentifier] NOT NULL,
	[ChatRoomId] [uniqueidentifier] NOT NULL,
	[SenderId] [uniqueidentifier] NOT NULL,
	[Content] [nvarchar](max) NOT NULL,
	[MessageType] [int] NOT NULL,
	[FilePath] [nvarchar](max) NULL,
	[FileName] [nvarchar](max) NULL,
	[FileSize] [bigint] NULL,
	[ReplyToMessageId] [uniqueidentifier] NULL,
	[IsEdited] [bit] NOT NULL,
	[EditedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MyMaps]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MyMaps](
	[Id] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[IsAuthor] [bit] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_MyMaps] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrbitCoinTransactions]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrbitCoinTransactions](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Amount] [decimal](18, 4) NOT NULL,
	[TransactionType] [int] NOT NULL,
	[RelatedEntityType] [nvarchar](450) NULL,
	[RelatedEntityId] [uniqueidentifier] NULL,
	[FeeAmount] [decimal](18, 4) NOT NULL,
	[BalanceAfter] [decimal](18, 4) NULL,
	[Note] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_OrbitCoinTransactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Packages]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Packages](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[DurationDays] [int] NOT NULL,
	[Limit] [int] NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[FeaturesSpec] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Packages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentRecords]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentRecords](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[PackageId] [uniqueidentifier] NULL,
	[MapId] [uniqueidentifier] NULL,
	[PaymentId] [uniqueidentifier] NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentStatus] [int] NOT NULL,
	[PaidAt] [datetime2](7) NULL,
	[ExternalId] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_PaymentRecords] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[Id] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoleClaims]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_RoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[Id] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[Status] [int] NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoomParticipants]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoomParticipants](
	[Id] [uniqueidentifier] NOT NULL,
	[RoomId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[JoinedAt] [datetime2](7) NOT NULL,
	[IsReady] [bit] NOT NULL,
	[IsOwner] [bit] NOT NULL,
	[Rank] [int] NULL,
	[FinalScore] [int] NULL,
	[SubmissionId] [uniqueidentifier] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_RoomParticipants] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rooms]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rooms](
	[Id] [uniqueidentifier] NOT NULL,
	[MatchId] [uniqueidentifier] NOT NULL,
	[MaxPlayers] [int] NOT NULL,
	[Code] [nvarchar](450) NULL,
	[RoomStatus] [int] NOT NULL,
	[StartedAt] [datetime2](7) NULL,
	[EndedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Rooms] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Submissions]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Submissions](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[Language] [nvarchar](max) NOT NULL,
	[AstSpec] [nvarchar](max) NULL,
	[BytecodeSpec] [nvarchar](max) NULL,
	[ResultStatus] [int] NOT NULL,
	[Score] [int] NULL,
	[StepsUsed] [int] NULL,
	[BlocksUsed] [int] NULL,
	[MatchId] [uniqueidentifier] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Submissions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserAchievements]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAchievements](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[AchievementId] [uniqueidentifier] NOT NULL,
	[UnlockedAt] [datetime2](7) NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_UserAchievements] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserClaims]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserConceptProgresses]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserConceptProgresses](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[ConceptId] [uniqueidentifier] NOT NULL,
	[IsCompleted] [bit] NOT NULL,
	[CompletedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_UserConceptProgresses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserLearningGoals]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserLearningGoals](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[LearningGoalId] [uniqueidentifier] NOT NULL,
	[SelectedAt] [datetime2](7) NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_UserLearningGoals] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserLogins]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_UserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserMapResults]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserMapResults](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[BestScore] [int] NOT NULL,
	[BestStars] [int] NOT NULL,
	[Attempts] [int] NOT NULL,
	[LastPlayedAt] [datetime2](7) NULL,
	[MasteryDeltaSpec] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_UserMapResults] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserMatchResults]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserMatchResults](
	[Id] [uniqueidentifier] NOT NULL,
	[MatchId] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[SubmissionId] [uniqueidentifier] NULL,
	[Rank] [int] NOT NULL,
	[FinalScore] [int] NOT NULL,
	[SubmittedAt] [datetime2](7) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_UserMatchResults] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserPackages]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserPackages](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[PackageId] [uniqueidentifier] NOT NULL,
	[Remaining] [int] NOT NULL,
	[ExpiresAt] [datetime2](7) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_UserPackages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserRoles]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserRoles](
	[UserId] [uniqueidentifier] NOT NULL,
	[RoleId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](max) NOT NULL,
	[LastName] [nvarchar](max) NOT NULL,
	[LastLoginAt] [datetime2](7) NULL,
	[JoiningAt] [datetime2](7) NOT NULL,
	[RefreshToken] [nvarchar](max) NULL,
	[RefreshTokenExpiryTime] [datetime2](7) NULL,
	[AvatarPath] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[Status] [int] NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[Bio] [nvarchar](max) NULL,
	[DateOfBirth] [datetime2](7) NULL,
	[Gender] [int] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserTokens]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserTokens](
	[UserId] [uniqueidentifier] NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserWallets]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserWallets](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Balance] [decimal](18, 4) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_UserWallets] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[XpTransactions]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[XpTransactions](
	[Id] [uniqueidentifier] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NULL,
	[Delta] [int] NOT NULL,
	[Reason] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedBy] [uniqueidentifier] NULL,
	[DeletedAt] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_XpTransactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[AggregatedCounter]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[AggregatedCounter](
	[Key] [nvarchar](100) NOT NULL,
	[Value] [bigint] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_CounterAggregated] PRIMARY KEY CLUSTERED 
(
	[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Counter]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Counter](
	[Key] [nvarchar](100) NOT NULL,
	[Value] [int] NOT NULL,
	[ExpireAt] [datetime] NULL,
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_HangFire_Counter] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Hash]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Hash](
	[Key] [nvarchar](100) NOT NULL,
	[Field] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ExpireAt] [datetime2](7) NULL,
 CONSTRAINT [PK_HangFire_Hash] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Field] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Job]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Job](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[StateId] [bigint] NULL,
	[StateName] [nvarchar](20) NULL,
	[InvocationData] [nvarchar](max) NOT NULL,
	[Arguments] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Job] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[JobParameter]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[JobParameter](
	[JobId] [bigint] NOT NULL,
	[Name] [nvarchar](40) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_JobParameter] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[JobQueue]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[JobQueue](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[Queue] [nvarchar](50) NOT NULL,
	[FetchedAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_JobQueue] PRIMARY KEY CLUSTERED 
(
	[Queue] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[List]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[List](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Key] [nvarchar](100) NOT NULL,
	[Value] [nvarchar](max) NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_List] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Schema]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Schema](
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_HangFire_Schema] PRIMARY KEY CLUSTERED 
(
	[Version] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Server]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Server](
	[Id] [nvarchar](200) NOT NULL,
	[Data] [nvarchar](max) NULL,
	[LastHeartbeat] [datetime] NOT NULL,
 CONSTRAINT [PK_HangFire_Server] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[Set]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[Set](
	[Key] [nvarchar](100) NOT NULL,
	[Score] [float] NOT NULL,
	[Value] [nvarchar](256) NOT NULL,
	[ExpireAt] [datetime] NULL,
 CONSTRAINT [PK_HangFire_Set] PRIMARY KEY CLUSTERED 
(
	[Key] ASC,
	[Value] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [HangFire].[State]    Script Date: 3/19/2026 11:05:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [HangFire].[State](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
	[Reason] [nvarchar](100) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Data] [nvarchar](max) NULL,
 CONSTRAINT [PK_HangFire_State] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260126142904_initialDTB', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260303022740_AddMoreTableInDTB', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260304102121_addAdminMap', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260305075633_AddMoreFieldInUser', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260305141201_RefactorEntity', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260306072314_addOrbitCoin', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260310030846_addMapTypes', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260311035130_addAvatarURL', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260313161228_addMyMapTable', N'8.0.13')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260315122435_AddLearningPath', N'8.0.13')
GO
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'6352e0d4-d6d3-4505-b84c-13dedf443d6e', N'b8aca0a2-1fab-4ebe-a272-a065f364ce90', N'For loop', N'Vòng lặp với số lần xác định.', N'for-loop', 1, CAST(N'2026-03-17T02:49:04.517' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'047ec58e-fd3c-4525-b5fb-2ca961182936', N'c5f28f09-baa5-4e01-a733-ef754765bea7', N'Thuật toán cơ bản', N'Các bước giải quyết bài toán bằng code.', N'basic-algorithm', 2, CAST(N'2026-03-17T02:49:04.523' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'7eae7fc2-7b51-4670-aac3-8044b518d51b', N'c7237395-1117-48bd-a4bc-6b700de02c4d', N'So sánh', N'So sánh lớn hơn, nhỏ hơn, bằng.', N'comparison', 2, CAST(N'2026-03-17T02:49:04.517' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'094d8b4a-28e1-4d2c-90e9-84f806c19527', N'b8aca0a2-1fab-4ebe-a272-a065f364ce90', N'While loop', N'Vòng lặp khi điều kiện còn đúng.', N'while-loop', 2, CAST(N'2026-03-17T02:49:04.520' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'5413fe04-a3a7-47e7-ad67-96025db4135b', N'c7237395-1117-48bd-a4bc-6b700de02c4d', N'If-else', N'Rẽ nhánh theo điều kiện đúng/sai.', N'if-else', 1, CAST(N'2026-03-17T02:49:04.517' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'14c81509-fb12-451d-b0ed-afa9ee74af0c', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', N'Biến là gì', N'Làm quen với biến và gán giá trị.', N'variables', 1, CAST(N'2026-03-17T02:49:04.480' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'aabcbcc9-13ed-4d28-97cd-b0db3c3250ee', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', N'Phép toán', N'Các phép toán cơ bản: cộng, trừ, nhân, chia.', N'operators', 2, CAST(N'2026-03-17T02:49:04.513' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'b5ccbbee-782f-4166-9703-d88e09a58654', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', N'Thứ tự thực thi', N'Chương trình chạy từ trên xuống dưới, từ trái sang phải.', N'execution-order', 3, CAST(N'2026-03-17T02:49:04.513' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Concepts] ([Id], [LearningGoalId], [Name], [Description], [ContentKey], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'96d393a6-d175-4a17-873b-f338303fb9bf', N'c5f28f09-baa5-4e01-a733-ef754765bea7', N'Phân tích bài toán', N'Đọc đề, tìm input/output, chia bước.', N'problem-analysis', 1, CAST(N'2026-03-17T02:49:04.520' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'78c5044c-f35f-44e0-9597-0ee1f12c2d52', N'e7eaa083-407d-4cd6-9338-4b0513218772', 2, N'Don''t have to count step by your shelf, use mathematical operation', CAST(N'2026-03-18T13:54:48.547' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'5ee19bfd-8451-4190-aa5e-0f74cb84aa31', N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', 2, N'The "fruit collected?" block will return true if collect a fruit, combine with variable block', CAST(N'2026-03-18T14:35:56.140' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a6dc3a08-f9c7-4d06-bca2-14435ad3f63b', N'1c3b4a0d-791b-47d6-be83-e8b74397e48e', 1, N'Use variable block and set a number for the break power to break the box', CAST(N'2026-03-18T05:55:05.913' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'07150847-4912-4c62-a4b3-1a4d049c35cf', N'a4e3b5cc-3691-4a15-bfa1-d42bfce98df3', 1, N'Use turn left block, turn right block for turn character facing', CAST(N'2026-03-18T04:51:29.043' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'fced5cfe-d579-410d-b0c3-360e2a4c7719', N'401d637b-3506-4826-8f05-ad9e90586a8f', 1, N'Variable can be changes', CAST(N'2026-03-18T09:01:45.347' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'ddc00a90-2b9f-46a4-b1b1-405197763d30', N'a4e3b5cc-3691-4a15-bfa1-d42bfce98df3', 2, N'Use move forward block for moving the character', CAST(N'2026-03-18T04:51:29.043' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'b745a2c6-efcd-4751-9bf9-40bac098c669', N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', 1, N'Don''t wase time place a lot move forward and turn block, use while loop', CAST(N'2026-03-18T14:35:56.140' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'2f298654-3bc9-45dd-92ea-42d7dcad9705', N'0d100d4a-b3b7-4505-a22c-5b9a8851898d', 1, N'Use turn left block, turn right block for turn the character facing', CAST(N'2026-03-18T04:52:44.207' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'9181066b-4317-4831-bf64-771f14cf3e2f', N'4f1c3eef-97f2-43e5-a9cc-a417f54f546b', 1, N'Variable can changes their value by using set block', CAST(N'2026-03-18T08:17:55.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'7e3849fa-0182-4629-ae4e-82dd00a0df91', N'0d100d4a-b3b7-4505-a22c-5b9a8851898d', 2, N'Use move forward and jump block for moving the character', CAST(N'2026-03-18T04:52:44.207' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'cdde4d93-037f-494b-8de2-87873b71750a', N'401d637b-3506-4826-8f05-ad9e90586a8f', 2, N'Use box hardness ahead to get hardness of the box ahead', CAST(N'2026-03-18T09:01:45.347' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'7945be97-3db3-475f-90cd-905e09114ae8', N'a4e3b5cc-3691-4a15-bfa1-d42bfce98df3', 3, N'Use break block for break the box and open/close block for open/close the door', CAST(N'2026-03-18T04:51:29.043' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'9eb428b4-6831-49ee-b3be-aa5a97bc0e9a', N'4f1c3eef-97f2-43e5-a9cc-a417f54f546b', 2, N'Box hardness ahead block can get box hardness', CAST(N'2026-03-18T08:17:55.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'd3e59ac2-8ef4-422a-8112-b12a0d67e698', N'e7eaa083-407d-4cd6-9338-4b0513218772', 1, N'Use the repeat block and loop the move', CAST(N'2026-03-18T13:54:48.530' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Hints] ([Id], [MapId], [OrderNo], [Content], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a10aa1ea-83cf-4205-ad84-dd5a738fc261', N'0d100d4a-b3b7-4505-a22c-5b9a8851898d', 3, N'Use break block for break boxes', CAST(N'2026-03-18T04:52:44.207' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[LearningGoals] ([Id], [Name], [Description], [SortOrder], [IconUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'c7237395-1117-48bd-a4bc-6b700de02c4d', N'Điều kiện', N'Học cách dùng if/else, so sánh và rẽ nhánh trong chương trình.', 2, NULL, CAST(N'2026-03-17T02:49:04.460' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningGoals] ([Id], [Name], [Description], [SortOrder], [IconUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', N'Logic cơ bản', N'Làm quen với biến, phép toán, thứ tự thực thi và điều khiển luồng cơ bản.', 1, NULL, CAST(N'2026-03-17T02:49:04.427' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningGoals] ([Id], [Name], [Description], [SortOrder], [IconUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'b8aca0a2-1fab-4ebe-a272-a065f364ce90', N'Vòng lặp', N'Làm chủ for, while và xử lý lặp để giải quyết bài toán.', 3, NULL, CAST(N'2026-03-17T02:49:04.460' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningGoals] ([Id], [Name], [Description], [SortOrder], [IconUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'c5f28f09-baa5-4e01-a733-ef754765bea7', N'Giải quyết vấn đề', N'Kết hợp logic, điều kiện và vòng lặp để phân tích và giải bài toán.', 4, NULL, CAST(N'2026-03-17T02:49:04.463' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'678e3210-c63e-4541-8594-01d2a93b5412', N'c7237395-1117-48bd-a4bc-6b700de02c4d', 0, N'5413fe04-a3a7-47e7-ad67-96025db4135b', NULL, 1, CAST(N'2026-03-17T02:49:04.587' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'48f2b362-d413-4f14-a43d-354c432b2a87', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', 1, NULL, NULL, 2, CAST(N'2026-03-17T02:49:04.580' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'e0b7d167-007f-43c4-9089-3cd47c89725d', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', 1, NULL, NULL, 6, CAST(N'2026-03-17T02:49:04.587' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'133140c6-f6d0-427f-9b10-3f9c9fef2ed3', N'b8aca0a2-1fab-4ebe-a272-a065f364ce90', 1, NULL, NULL, 2, CAST(N'2026-03-17T02:49:04.593' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'668b5357-7b36-48e0-a760-4dff7bfed3ba', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', 0, N'b5ccbbee-782f-4166-9703-d88e09a58654', NULL, 5, CAST(N'2026-03-17T02:49:04.583' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'1ee9c8cb-1049-412e-a975-67939277f994', N'b8aca0a2-1fab-4ebe-a272-a065f364ce90', 0, N'6352e0d4-d6d3-4505-b84c-13dedf443d6e', NULL, 1, CAST(N'2026-03-17T02:49:04.593' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'c1ad634d-5a7d-47fa-ab4e-6f23cb8c1c2e', N'b8aca0a2-1fab-4ebe-a272-a065f364ce90', 0, N'094d8b4a-28e1-4d2c-90e9-84f806c19527', NULL, 3, CAST(N'2026-03-17T02:49:04.597' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'8f1252ca-de10-4503-808e-72a9ed3c4b78', N'c5f28f09-baa5-4e01-a733-ef754765bea7', 0, N'96d393a6-d175-4a17-873b-f338303fb9bf', NULL, 1, CAST(N'2026-03-17T02:49:04.597' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'c869df97-37c6-43d9-af3f-7ae035abe57c', N'c5f28f09-baa5-4e01-a733-ef754765bea7', 1, NULL, NULL, 2, CAST(N'2026-03-17T02:49:04.600' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a14657f0-6e55-4eba-8746-81d9e5b39a82', N'c7237395-1117-48bd-a4bc-6b700de02c4d', 1, NULL, NULL, 2, CAST(N'2026-03-17T02:49:04.590' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a76a77ea-415a-4ba9-9cda-83fac84b2e14', N'c7237395-1117-48bd-a4bc-6b700de02c4d', 0, N'7eae7fc2-7b51-4670-aac3-8044b518d51b', NULL, 3, CAST(N'2026-03-17T02:49:04.590' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'dea2215d-b9f4-4970-87d1-86fc8afa5c85', N'b8aca0a2-1fab-4ebe-a272-a065f364ce90', 1, NULL, NULL, 4, CAST(N'2026-03-17T02:49:04.597' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'c8d5fbbb-5b51-4770-9649-8b04ec57c5f2', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', 1, NULL, NULL, 4, CAST(N'2026-03-17T02:49:04.583' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'5cd644a1-c470-47b4-9405-9643faca30b7', N'c7237395-1117-48bd-a4bc-6b700de02c4d', 1, NULL, NULL, 4, CAST(N'2026-03-17T02:49:04.590' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'29cbc712-17c1-4706-b33b-9fb6ebf61672', N'c5f28f09-baa5-4e01-a733-ef754765bea7', 0, N'047ec58e-fd3c-4525-b5fb-2ca961182936', NULL, 3, CAST(N'2026-03-17T02:49:04.600' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'49b02618-5bb2-4392-bf1b-b793341bb2fe', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', 0, N'aabcbcc9-13ed-4d28-97cd-b0db3c3250ee', NULL, 3, CAST(N'2026-03-17T02:49:04.583' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'1f227b75-9af1-43f3-b530-d7ca6d5a4c81', N'c5f28f09-baa5-4e01-a733-ef754765bea7', 1, NULL, NULL, 4, CAST(N'2026-03-17T02:49:04.600' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[LearningPathItems] ([Id], [LearningGoalId], [ItemType], [ConceptId], [MapId], [SortOrder], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'954bf2be-4d1f-43bc-9a7e-e0ccdfb18afe', N'd6b2b0b8-06a9-459e-bf19-7351bb72acdf', 0, N'14c81509-fb12-451d-b0ed-afa9ee74af0c', NULL, 1, CAST(N'2026-03-17T02:49:04.543' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[MapDetails] ([Id], [MapId], [JsonContent], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'91a3bfb0-f7af-4377-943f-213526a5aad2', N'401d637b-3506-4826-8f05-ad9e90586a8f', N'{
  "id": "level-topdown-1773824501419",
  "name": "More Box",
  "width": 20,
  "height": 15,
  "tileset": "default",
  "layers": {
    "background": [
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ]
    ],
    "ground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        97,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        99,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        117,
        118,
        118,
        118,
        118,
        118,
        118,
        113,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        119,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "foreground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        18,
        0,
        18,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        18,
        0,
        18,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        27,
        28,
        25,
        0,
        18,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        18,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        27,
        28,
        28,
        28,
        25,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "collision": [
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        false,
        true,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        true,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ]
    ]
  },
  "startPosition": {
    "row": 3,
    "col": 3
  },
  "goalPosition": {
    "row": 11,
    "col": 16
  },
  "objects": [
    {
      "id": "deco-1",
      "type": "box1",
      "position": {
        "row": 11,
        "col": 14
      },
      "metadata": {
        "objectId": 6,
        "hardness": 1
      }
    },
    {
      "id": "deco-2",
      "type": "box1",
      "position": {
        "row": 10,
        "col": 16
      },
      "metadata": {
        "objectId": 6,
        "hardness": 1
      }
    },
    {
      "id": "deco-3",
      "type": "box3",
      "position": {
        "row": 11,
        "col": 13
      },
      "metadata": {
        "objectId": 8,
        "hardness": 3
      }
    },
    {
      "id": "deco-4",
      "type": "box3",
      "position": {
        "row": 9,
        "col": 16
      },
      "metadata": {
        "objectId": 8,
        "hardness": 3
      }
    },
    {
      "id": "deco-5",
      "type": "box2",
      "position": {
        "row": 11,
        "col": 15
      },
      "metadata": {
        "objectId": 7,
        "hardness": 2
      }
    },
    {
      "id": "deco-6",
      "type": "box2",
      "position": {
        "row": 8,
        "col": 16
      },
      "metadata": {
        "objectId": 7,
        "hardness": 2
      }
    },
    {
      "id": "fruit-1",
      "type": "fruit",
      "position": {
        "row": 6,
        "col": 16
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-2",
      "type": "fruit",
      "position": {
        "row": 3,
        "col": 15
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-3",
      "type": "fruit",
      "position": {
        "row": 3,
        "col": 8
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-4",
      "type": "fruit",
      "position": {
        "row": 6,
        "col": 3
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-5",
      "type": "fruit",
      "position": {
        "row": 11,
        "col": 7
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-6",
      "type": "fruit",
      "position": {
        "row": 11,
        "col": 3
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-7",
      "type": "fruit",
      "position": {
        "row": 6,
        "col": 7
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-8",
      "type": "fruit",
      "position": {
        "row": 8,
        "col": 13
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "deco-7",
      "type": "box1",
      "position": {
        "row": 3,
        "col": 7
      },
      "metadata": {
        "objectId": 6,
        "hardness": 1
      }
    },
    {
      "id": "deco-8",
      "type": "box1",
      "position": {
        "row": 11,
        "col": 4
      },
      "metadata": {
        "objectId": 6,
        "hardness": 1
      }
    },
    {
      "id": "deco-9",
      "type": "box3",
      "position": {
        "row": 8,
        "col": 3
      },
      "metadata": {
        "objectId": 8,
        "hardness": 3
      }
    },
    {
      "id": "deco-10",
      "type": "box2",
      "position": {
        "row": 4,
        "col": 16
      },
      "metadata": {
        "objectId": 7,
        "hardness": 2
      }
    },
    {
      "id": "deco-11",
      "type": "box2",
      "position": {
        "row": 5,
        "col": 10
      },
      "metadata": {
        "objectId": 7,
        "hardness": 2
      }
    },
    {
      "id": "deco-12",
      "type": "box2",
      "position": {
        "row": 9,
        "col": 8
      },
      "metadata": {
        "objectId": 7,
        "hardness": 2
      }
    }
  ],
  "metadata": {
    "difficulty": "medium",
    "description": "Every type of box have a different hardness",
    "targetAlgorithm": "manual",
    "estimatedSteps": 50
  },
  "blockConstraints": {
    "blockLimit": null,
    "bannedBlocks": [],
    "requiredBlocks": []
  }
}', CAST(N'2026-03-18T09:01:45.347' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapDetails] ([Id], [MapId], [JsonContent], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'df9ebd0f-e681-4192-a329-57017521616d', N'a4e3b5cc-3691-4a15-bfa1-d42bfce98df3', N'{
  "id": "level-topdown-1773809488975",
  "name": "Top down movement tutorial",
  "width": 20,
  "height": 15,
  "tileset": "default",
  "layers": {
    "background": [
      [
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10
      ],
      [
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        10,
        10
      ],
      [
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10
      ],
      [
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10,
        10
      ]
    ],
    "ground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        97,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        99,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        177,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        37,
        178,
        0
      ],
      [
        0,
        46,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        47,
        48,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "foreground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        18,
        0,
        18,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        18,
        0,
        18,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        18,
        0,
        18,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        23,
        24,
        25,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "collision": [
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ]
    ]
  },
  "startPosition": {
    "row": 3,
    "col": 3
  },
  "goalPosition": {
    "row": 9,
    "col": 13
  },
  "objects": [
    {
      "id": "fruit-1",
      "type": "fruit",
      "position": {
        "row": 11,
        "col": 3
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-2",
      "type": "fruit",
      "position": {
        "row": 7,
        "col": 9
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-3",
      "type": "fruit",
      "position": {
        "row": 3,
        "col": 8
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-4",
      "type": "fruit",
      "position": {
        "row": 5,
        "col": 16
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-5",
      "type": "fruit",
      "position": {
        "row": 7,
        "col": 3
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "deco-1",
      "type": "door",
      "position": {
        "row": 7,
        "col": 13
      },
      "metadata": {
        "objectId": 5,
        "isOpen": false
      }
    },
    {
      "id": "deco-2",
      "type": "box1",
      "position": {
        "row": 7,
        "col": 4
      },
      "metadata": {
        "objectId": 6,
        "hardness": 1
      }
    },
    {
      "id": "deco-3",
      "type": "box1",
      "position": {
        "row": 3,
        "col": 7
      },
      "metadata": {
        "objectId": 6,
        "hardness": 1
      }
    }
  ],
  "metadata": {
    "difficulty": "medium",
    "description": "Top down movement tutorial",
    "targetAlgorithm": "manual",
    "estimatedSteps": 50,
    "requiredFruits": 0
  },
  "blockConstraints": {
    "blockLimit": null,
    "bannedBlocks": [],
    "requiredBlocks": []
  }
}', CAST(N'2026-03-17T14:19:33.393' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T04:51:29.043' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1)
INSERT [dbo].[MapDetails] ([Id], [MapId], [JsonContent], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'832f7540-64c8-4f80-a2ab-5b58a7e7d95a', N'e7eaa083-407d-4cd6-9338-4b0513218772', N'{
  "id": "level-topdown-1773842086415",
  "name": "Introduce for loop",
  "width": 15,
  "height": 15,
  "tileset": "default",
  "layers": {
    "background": [
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ]
    ],
    "ground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        97,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        99,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        112,
        118,
        118,
        118,
        118,
        118,
        113,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        109,
        0,
        0,
        0,
        0,
        0,
        107,
        109,
        0
      ],
      [
        0,
        107,
        108,
        112,
        118,
        113,
        108,
        102,
        0,
        0,
        0,
        0,
        107,
        109,
        0
      ],
      [
        0,
        107,
        108,
        109,
        0,
        117,
        113,
        122,
        102,
        0,
        0,
        103,
        108,
        109,
        0
      ],
      [
        0,
        107,
        112,
        119,
        0,
        0,
        107,
        108,
        122,
        98,
        98,
        123,
        108,
        109,
        0
      ],
      [
        0,
        107,
        109,
        0,
        0,
        0,
        117,
        113,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        109,
        0,
        0,
        0,
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        122,
        99,
        0,
        0,
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        122,
        98,
        98,
        98,
        123,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        117,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        119,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "foreground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "collision": [
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        true,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        true,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        true,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        true,
        true,
        false,
        false,
        false,
        true,
        true,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ]
    ]
  },
  "startPosition": {
    "row": 2,
    "col": 2
  },
  "goalPosition": {
    "row": 12,
    "col": 12
  },
  "objects": [
    {
      "id": "fruit-1",
      "type": "fruit",
      "position": {
        "row": 4,
        "col": 4
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-2",
      "type": "fruit",
      "position": {
        "row": 8,
        "col": 8
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-3",
      "type": "fruit",
      "position": {
        "row": 11,
        "col": 11
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-4",
      "type": "fruit",
      "position": {
        "row": 2,
        "col": 8
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-5",
      "type": "fruit",
      "position": {
        "row": 2,
        "col": 12
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-6",
      "type": "fruit",
      "position": {
        "row": 8,
        "col": 12
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-7",
      "type": "fruit",
      "position": {
        "row": 7,
        "col": 2
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-8",
      "type": "fruit",
      "position": {
        "row": 12,
        "col": 2
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-9",
      "type": "fruit",
      "position": {
        "row": 12,
        "col": 7
      },
      "metadata": {
        "points": 10
      }
    }
  ],
  "metadata": {
    "difficulty": "medium",
    "description": "Loop through the map.",
    "targetAlgorithm": "manual",
    "estimatedSteps": 50
  },
  "blockConstraints": {
    "blockLimit": null,
    "bannedBlocks": [
      "custom_while",
      "custom_do_while",
      "repeat_until"
    ],
    "requiredBlocks": [
      {
        "type": "repeat",
        "minCount": 1
      }
    ]
  }
}', CAST(N'2026-03-18T13:54:48.563' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapDetails] ([Id], [MapId], [JsonContent], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'5f8b9003-2c20-4c98-9788-6a4d9595ba00', N'4f1c3eef-97f2-43e5-a9cc-a417f54f546b', N'{
  "id": "level-platform-1773821874998",
  "name": "More Boxes",
  "width": 20,
  "height": 15,
  "tileset": "default",
  "layers": {
    "background": [
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ],
      [
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181,
        181
      ]
    ],
    "ground": [
      [
        28,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        28
      ],
      [
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        31,
        0,
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        31
      ],
      [
        31,
        16,
        17,
        28,
        0,
        0,
        0,
        0,
        31,
        0,
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        15,
        16,
        16,
        16,
        17,
        0,
        15,
        16,
        17,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        15,
        16,
        16,
        16,
        17,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        15,
        16,
        16,
        16,
        17,
        0,
        31
      ],
      [
        31,
        16,
        16,
        16,
        17,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        29,
        30,
        0,
        0,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        28,
        45,
        46,
        0,
        28,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        15,
        16,
        16,
        16,
        16,
        16,
        17,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        0,
        0,
        0,
        18,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        0,
        28,
        0,
        31,
        28,
        0,
        0,
        0,
        0,
        0,
        29,
        30,
        0,
        0,
        0,
        31
      ],
      [
        31,
        0,
        0,
        0,
        28,
        28,
        0,
        47,
        28,
        28,
        0,
        0,
        0,
        0,
        45,
        46,
        0,
        28,
        0,
        31
      ],
      [
        28,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        16,
        28
      ]
    ],
    "foreground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "collision": [
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        true,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        true,
        true,
        true,
        true,
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        true,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        false,
        true,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        true,
        false,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        true,
        true,
        false,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        true,
        true,
        false,
        true,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ]
    ]
  },
  "startPosition": {
    "row": 13,
    "col": 2
  },
  "goalPosition": {
    "row": 1,
    "col": 1
  },
  "objects": [
    {
      "id": "fruit-1",
      "type": "fruit",
      "position": {
        "row": 11,
        "col": 5
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-2",
      "type": "fruit",
      "position": {
        "row": 7,
        "col": 12
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-3",
      "type": "fruit",
      "position": {
        "row": 5,
        "col": 15
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-4",
      "type": "fruit",
      "position": {
        "row": 4,
        "col": 9
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-5",
      "type": "fruit",
      "position": {
        "row": 2,
        "col": 5
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-6",
      "type": "fruit",
      "position": {
        "row": 11,
        "col": 15
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "deco-1",
      "type": "box3",
      "position": {
        "row": 1,
        "col": 2
      },
      "metadata": {
        "objectId": 6,
        "hardness": 3
      }
    },
    {
      "id": "deco-2",
      "type": "box3",
      "position": {
        "row": 4,
        "col": 8
      },
      "metadata": {
        "objectId": 6,
        "hardness": 3
      }
    },
    {
      "id": "deco-3",
      "type": "box2",
      "position": {
        "row": 4,
        "col": 10
      },
      "metadata": {
        "objectId": 5,
        "hardness": 2
      }
    },
    {
      "id": "deco-4",
      "type": "box1",
      "position": {
        "row": 4,
        "col": 7
      },
      "metadata": {
        "objectId": 5,
        "hardness": 1,
        "difficulty": "normal"
      }
    },
    {
      "id": "deco-5",
      "type": "box1",
      "position": {
        "row": 11,
        "col": 14
      },
      "metadata": {
        "objectId": 5,
        "hardness": 1,
        "difficulty": "normal"
      }
    },
    {
      "id": "deco-6",
      "type": "box3",
      "position": {
        "row": 7,
        "col": 13
      },
      "metadata": {
        "objectId": 6,
        "hardness": 3
      }
    },
    {
      "id": "fruit-7",
      "type": "fruit",
      "position": {
        "row": 6,
        "col": 2
      },
      "metadata": {
        "points": 10
      }
    }
  ],
  "metadata": {
    "difficulty": "medium",
    "description": "Not only one kind of box",
    "targetAlgorithm": "manual",
    "estimatedSteps": 50,
    "requiredFruits": 0
  },
  "blockConstraints": {
    "blockLimit": null,
    "bannedBlocks": [],
    "requiredBlocks": []
  }
}', CAST(N'2026-03-18T08:04:55.153' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T08:17:55.113' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1)
INSERT [dbo].[MapDetails] ([Id], [MapId], [JsonContent], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a7aece22-d601-4a99-9325-82546b90dec4', N'0d100d4a-b3b7-4505-a22c-5b9a8851898d', N'{
  "id": "level-platform-1773809564159",
  "name": "Platform movement tutorial",
  "width": 20,
  "height": 15,
  "tileset": "default",
  "layers": {
    "background": [
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ],
      [
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177,
        177
      ]
    ],
    "ground": [
      [
        156,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        141,
        156
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        159
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        159
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        140,
        141,
        141,
        159
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        159
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        159
      ],
      [
        159,
        141,
        141,
        142,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        159
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        159
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        159
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        156,
        159
      ],
      [
        159,
        0,
        0,
        0,
        0,
        0,
        140,
        141,
        142,
        0,
        29,
        30,
        0,
        0,
        0,
        156,
        0,
        157,
        158,
        159
      ],
      [
        159,
        0,
        156,
        0,
        156,
        0,
        0,
        159,
        0,
        0,
        45,
        46,
        0,
        156,
        134,
        136,
        0,
        173,
        174,
        159
      ],
      [
        159,
        156,
        156,
        156,
        134,
        136,
        0,
        175,
        156,
        134,
        135,
        135,
        135,
        135,
        137,
        138,
        135,
        135,
        136,
        159
      ],
      [
        159,
        134,
        135,
        135,
        137,
        138,
        135,
        135,
        135,
        151,
        151,
        151,
        151,
        151,
        153,
        154,
        151,
        151,
        151,
        159
      ],
      [
        156,
        151,
        151,
        151,
        153,
        154,
        151,
        151,
        151,
        151,
        151,
        151,
        151,
        151,
        151,
        151,
        151,
        151,
        151,
        156
      ]
    ],
    "foreground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "collision": [
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        false,
        true,
        true,
        false,
        false,
        false,
        true,
        false,
        true,
        true,
        true
      ],
      [
        true,
        false,
        true,
        false,
        true,
        false,
        false,
        true,
        false,
        false,
        true,
        true,
        false,
        true,
        true,
        true,
        false,
        true,
        true,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        false,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        false,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ]
    ]
  },
  "startPosition": {
    "row": 5,
    "col": 2
  },
  "goalPosition": {
    "row": 9,
    "col": 15
  },
  "objects": [
    {
      "id": "fruit-1",
      "type": "fruit",
      "position": {
        "row": 9,
        "col": 11
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-2",
      "type": "fruit",
      "position": {
        "row": 9,
        "col": 7
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-3",
      "type": "fruit",
      "position": {
        "row": 10,
        "col": 2
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "enemy-1",
      "type": "decorative",
      "position": {
        "row": 9,
        "col": 8
      },
      "metadata": {
        "difficulty": "normal"
      }
    },
    {
      "id": "enemy-2",
      "type": "decorative",
      "position": {
        "row": 10,
        "col": 4
      },
      "metadata": {
        "difficulty": "normal"
      }
    }
  ],
  "metadata": {
    "difficulty": "medium",
    "description": "Platform movement tutorial.",
    "targetAlgorithm": "manual",
    "estimatedSteps": 50,
    "requiredFruits": 0
  },
  "blockConstraints": {
    "blockLimit": null,
    "bannedBlocks": [],
    "requiredBlocks": []
  }
}', CAST(N'2026-03-17T04:32:53.497' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T04:52:44.207' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1)
INSERT [dbo].[MapDetails] ([Id], [MapId], [JsonContent], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'2afe7228-622f-43d6-8b2d-968f02a4bc92', N'1c3b4a0d-791b-47d6-be83-e8b74397e48e', N'{
  "id": "level-platform-1773813302265",
  "name": "Introduce variable",
  "width": 20,
  "height": 15,
  "tileset": "default",
  "layers": {
    "background": [
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ],
      [
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178,
        178
      ]
    ],
    "ground": [
      [
        92,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        77,
        92
      ],
      [
        95,
        0,
        0,
        95,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        95
      ],
      [
        95,
        0,
        0,
        95,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        95
      ],
      [
        95,
        0,
        0,
        111,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        95
      ],
      [
        95,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        95
      ],
      [
        95,
        77,
        77,
        78,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        95
      ],
      [
        95,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        76,
        77,
        77,
        95
      ],
      [
        95,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        95
      ],
      [
        95,
        0,
        76,
        77,
        77,
        78,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        29,
        30,
        0,
        0,
        95
      ],
      [
        95,
        0,
        95,
        0,
        0,
        95,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        45,
        46,
        76,
        77,
        95
      ],
      [
        95,
        0,
        95,
        0,
        0,
        95,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        93,
        94,
        0,
        0,
        0,
        95
      ],
      [
        95,
        0,
        111,
        0,
        0,
        95,
        0,
        0,
        70,
        71,
        72,
        0,
        0,
        0,
        109,
        110,
        0,
        0,
        0,
        95
      ],
      [
        95,
        70,
        71,
        72,
        0,
        111,
        0,
        70,
        87,
        87,
        87,
        70,
        71,
        71,
        71,
        71,
        71,
        72,
        0,
        95
      ],
      [
        95,
        86,
        87,
        87,
        70,
        71,
        72,
        87,
        87,
        87,
        87,
        87,
        87,
        73,
        74,
        87,
        87,
        87,
        72,
        95
      ],
      [
        92,
        86,
        87,
        87,
        87,
        87,
        87,
        87,
        87,
        87,
        87,
        87,
        87,
        89,
        90,
        87,
        87,
        87,
        88,
        92
      ]
    ],
    "foreground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "collision": [
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        false,
        false,
        true
      ],
      [
        true,
        false,
        true,
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        true,
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        true,
        false,
        false,
        true,
        false,
        false,
        true,
        true,
        true,
        false,
        false,
        false,
        true,
        true,
        false,
        false,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        false,
        true,
        false,
        true,
        true,
        false,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        false,
        true
      ],
      [
        true,
        false,
        false,
        true,
        true,
        true,
        true,
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ]
    ]
  },
  "startPosition": {
    "row": 4,
    "col": 2
  },
  "goalPosition": {
    "row": 5,
    "col": 18
  },
  "objects": [
    {
      "id": "fruit-1",
      "type": "fruit",
      "position": {
        "row": 7,
        "col": 4
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-2",
      "type": "fruit",
      "position": {
        "row": 5,
        "col": 16
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-3",
      "type": "fruit",
      "position": {
        "row": 10,
        "col": 9
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-4",
      "type": "fruit",
      "position": {
        "row": 9,
        "col": 14
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-5",
      "type": "fruit",
      "position": {
        "row": 7,
        "col": 15
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-6",
      "type": "fruit",
      "position": {
        "row": 8,
        "col": 18
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "deco-1",
      "type": "box3",
      "position": {
        "row": 4,
        "col": 3
      },
      "metadata": {
        "objectId": 6,
        "hardness": 3
      }
    }
  ],
  "metadata": {
    "difficulty": "medium",
    "description": "Introduce variable block, use variable block and set a number for the break power to break the box",
    "targetAlgorithm": "manual",
    "estimatedSteps": 50
  },
  "blockConstraints": {
    "blockLimit": null,
    "bannedBlocks": [],
    "requiredBlocks": []
  }
}', CAST(N'2026-03-18T05:55:05.947' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapDetails] ([Id], [MapId], [JsonContent], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'8c171ecd-91b0-4b2b-8eb3-9d8b74d5aacc', N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', N'{
  "id": "level-topdown-1773844556096",
  "name": "Introduce while/do while loop",
  "width": 20,
  "height": 15,
  "tileset": "default",
  "layers": {
    "background": [
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ],
      [
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11,
        11
      ]
    ],
    "ground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        97,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        98,
        99,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        107,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        108,
        109,
        0
      ],
      [
        0,
        117,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        118,
        119,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "foreground": [
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ],
      [
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ]
    ],
    "collision": [
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ],
      [
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
      ]
    ]
  },
  "startPosition": {
    "row": 2,
    "col": 2
  },
  "goalPosition": {
    "row": 12,
    "col": 17
  },
  "objects": [
    {
      "id": "fruit-1",
      "type": "fruit",
      "position": {
        "row": 4,
        "col": 4
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-2",
      "type": "fruit",
      "position": {
        "row": 9,
        "col": 5
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-3",
      "type": "fruit",
      "position": {
        "row": 12,
        "col": 7
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-4",
      "type": "fruit",
      "position": {
        "row": 3,
        "col": 9
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-5",
      "type": "fruit",
      "position": {
        "row": 6,
        "col": 12
      },
      "metadata": {
        "points": 10
      }
    },
    {
      "id": "fruit-6",
      "type": "fruit",
      "position": {
        "row": 7,
        "col": 8
      },
      "metadata": {
        "points": 10
      }
    }
  ],
  "metadata": {
    "difficulty": "medium",
    "description": "Loop though the map with while",
    "targetAlgorithm": "manual",
    "estimatedSteps": 50,
    "requiredFruits": 4
  },
  "blockConstraints": {
    "blockLimit": null,
    "bannedBlocks": [
      "repeat_until",
      "repeat"
    ],
    "requiredBlocks": [
      {
        "type": "custom_while",
        "minCount": 1
      }
    ]
  }
}', CAST(N'2026-03-18T14:28:24.120' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T14:35:56.143' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1)
GO
INSERT [dbo].[Maps] ([Id], [Title], [Description], [Difficulty], [TimeLimitMs], [IsPublished], [MapStatus], [Price], [EditorialContent], [UnlockEditorialAfterStars], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status], [WinCondition], [Type], [AvatarUrl]) VALUES (N'e7eaa083-407d-4cd6-9338-4b0513218772', N'Introduce for loop', N'Loop through the map.', 1, 300000, 1, 4, CAST(0.00 AS Decimal(18, 2)), NULL, 3, CAST(N'2026-03-18T13:54:48.490' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1, 1, 0, N'https://res.cloudinary.com/dwanwdz4r/image/upload/v1773842087/uploads/avatars/maps/map_new_639094388864384658_639094388864384971.png')
INSERT [dbo].[Maps] ([Id], [Title], [Description], [Difficulty], [TimeLimitMs], [IsPublished], [MapStatus], [Price], [EditorialContent], [UnlockEditorialAfterStars], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status], [WinCondition], [Type], [AvatarUrl]) VALUES (N'0d100d4a-b3b7-4505-a22c-5b9a8851898d', N'Platform movement tutorial', N'Platform movement tutorial.', 1, 300000, 1, 4, CAST(0.00 AS Decimal(18, 2)), NULL, 3, CAST(N'2026-03-17T04:32:53.457' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T04:52:44.207' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1, 1, 1, N'https://res.cloudinary.com/dwanwdz4r/image/upload/v1773721972/uploads/avatars/maps/map_new_639093187679561434_639093187679571335.png')
INSERT [dbo].[Maps] ([Id], [Title], [Description], [Difficulty], [TimeLimitMs], [IsPublished], [MapStatus], [Price], [EditorialContent], [UnlockEditorialAfterStars], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status], [WinCondition], [Type], [AvatarUrl]) VALUES (N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', N'Introduce while/do while loop', N'Loop though the map with while', 1, 500000, 1, 4, CAST(0.00 AS Decimal(18, 2)), NULL, 3, CAST(N'2026-03-18T14:28:24.120' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T14:35:56.140' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1, 2, 0, N'https://res.cloudinary.com/dwanwdz4r/image/upload/v1773844103/uploads/avatars/maps/map_new_639094409009086983_639094409009087020.png')
INSERT [dbo].[Maps] ([Id], [Title], [Description], [Difficulty], [TimeLimitMs], [IsPublished], [MapStatus], [Price], [EditorialContent], [UnlockEditorialAfterStars], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status], [WinCondition], [Type], [AvatarUrl]) VALUES (N'4f1c3eef-97f2-43e5-a9cc-a417f54f546b', N'More Boxes', N'Not only one kind of box', 2, 600000, 1, 4, CAST(0.00 AS Decimal(18, 2)), NULL, 3, CAST(N'2026-03-18T08:04:55.063' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T08:17:55.107' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1, 1, 1, N'https://res.cloudinary.com/dwanwdz4r/image/upload/v1773821093/uploads/avatars/maps/map_new_639094178918161649_639094178918175046.png')
INSERT [dbo].[Maps] ([Id], [Title], [Description], [Difficulty], [TimeLimitMs], [IsPublished], [MapStatus], [Price], [EditorialContent], [UnlockEditorialAfterStars], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status], [WinCondition], [Type], [AvatarUrl]) VALUES (N'401d637b-3506-4826-8f05-ad9e90586a8f', N'More Box', N'Every type of box have a different hardness', 1, 400000, 1, 4, CAST(0.00 AS Decimal(18, 2)), NULL, 3, CAST(N'2026-03-18T09:01:45.343' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1, 1, 0, N'https://res.cloudinary.com/dwanwdz4r/image/upload/v1773824503/uploads/avatars/maps/map_new_639094213014431355_639094213014431889.png')
INSERT [dbo].[Maps] ([Id], [Title], [Description], [Difficulty], [TimeLimitMs], [IsPublished], [MapStatus], [Price], [EditorialContent], [UnlockEditorialAfterStars], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status], [WinCondition], [Type], [AvatarUrl]) VALUES (N'a4e3b5cc-3691-4a15-bfa1-d42bfce98df3', N'Top down movement tutorial', N'Top down movement tutorial', 1, 300000, 1, 4, CAST(0.00 AS Decimal(18, 2)), NULL, 3, CAST(N'2026-03-17T14:19:33.357' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T04:51:29.037' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1, 1, 0, N'https://res.cloudinary.com/dwanwdz4r/image/upload/v1773757171/uploads/avatars/maps/map_new_639093539695513604_639093539695526684.png')
INSERT [dbo].[Maps] ([Id], [Title], [Description], [Difficulty], [TimeLimitMs], [IsPublished], [MapStatus], [Price], [EditorialContent], [UnlockEditorialAfterStars], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status], [WinCondition], [Type], [AvatarUrl]) VALUES (N'1c3b4a0d-791b-47d6-be83-e8b74397e48e', N'Introduce variable', N'Introduce variable block, use variable block and set a number for the break power to break the box', 1, 300000, 1, 4, CAST(0.00 AS Decimal(18, 2)), NULL, 3, CAST(N'2026-03-18T05:55:05.877' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1, 1, 1, N'https://res.cloudinary.com/dwanwdz4r/image/upload/v1773813304/uploads/avatars/maps/map_new_639094101023159673_639094101023172215.png')
GO
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'57a4b905-4b6f-4538-aca7-05651e0a0a1c', N'4f1c3eef-97f2-43e5-a9cc-a417f54f546b', N'54d24283-2b8a-4acb-bf2a-08de83cfc050', CAST(N'2026-03-18T08:17:55.113' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a3da0b46-2f41-4c55-b31e-244a18df1152', N'1c3b4a0d-791b-47d6-be83-e8b74397e48e', N'86c5a948-c70b-44d9-bf34-08de83cfc050', CAST(N'2026-03-18T05:55:05.947' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'ab1ea00e-b67a-4401-973c-32857c82d527', N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', N'54d24283-2b8a-4acb-bf2a-08de83cfc050', CAST(N'2026-03-18T14:35:56.143' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'9fb55efd-4699-4a28-bc9a-3f9ca0159134', N'1c3b4a0d-791b-47d6-be83-e8b74397e48e', N'54d24283-2b8a-4acb-bf2a-08de83cfc050', CAST(N'2026-03-18T05:55:05.933' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'5d97acec-a325-4fbd-a8c8-4336ce701f66', N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', N'86c5a948-c70b-44d9-bf34-08de83cfc050', CAST(N'2026-03-18T14:35:56.143' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'ba32d505-c3b1-4187-bba8-497c127e449b', N'a4e3b5cc-3691-4a15-bfa1-d42bfce98df3', N'cc392079-c522-4908-bf35-08de83cfc050', CAST(N'2026-03-18T04:51:29.043' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'7a9357d1-f44f-4ca6-8ce6-4ab916cecf6a', N'401d637b-3506-4826-8f05-ad9e90586a8f', N'86c5a948-c70b-44d9-bf34-08de83cfc050', CAST(N'2026-03-18T09:01:45.347' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'88d08747-844a-43e6-a977-637db2be8964', N'e7eaa083-407d-4cd6-9338-4b0513218772', N'cc392079-c522-4908-bf35-08de83cfc050', CAST(N'2026-03-18T13:54:48.550' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'7ba15694-310f-4a69-bae2-803fe317ad76', N'e7eaa083-407d-4cd6-9338-4b0513218772', N'ee2131e4-c2af-43fc-bf2d-08de83cfc050', CAST(N'2026-03-18T13:54:48.563' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'34c2fedc-6939-4f28-a249-96edbbdc7407', N'0d100d4a-b3b7-4505-a22c-5b9a8851898d', N'86c5a948-c70b-44d9-bf34-08de83cfc050', CAST(N'2026-03-18T04:52:44.207' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'0df82fe0-c72f-4f5a-bf4d-9ed72c69a8c9', N'401d637b-3506-4826-8f05-ad9e90586a8f', N'54d24283-2b8a-4acb-bf2a-08de83cfc050', CAST(N'2026-03-18T09:01:45.347' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'b16c28aa-b2c5-47a8-b66b-a4834ba4ae3f', N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', N'cc392079-c522-4908-bf35-08de83cfc050', CAST(N'2026-03-18T14:35:56.143' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'd2d40943-b979-420b-ae1d-ae53282954ee', N'a4e3b5cc-3691-4a15-bfa1-d42bfce98df3', N'86c5a948-c70b-44d9-bf34-08de83cfc050', CAST(N'2026-03-18T04:51:29.043' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'4c1e8d57-f571-4e9b-8edd-bd1f3a2514d4', N'4f1c3eef-97f2-43e5-a9cc-a417f54f546b', N'86c5a948-c70b-44d9-bf34-08de83cfc050', CAST(N'2026-03-18T08:17:55.113' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'd6d321e5-8856-4f6a-8ab8-bf13ad8e0e77', N'0d100d4a-b3b7-4505-a22c-5b9a8851898d', N'cc392079-c522-4908-bf35-08de83cfc050', CAST(N'2026-03-18T04:52:44.207' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'4ad6fb28-89be-408c-b069-c02b9bdfc9d3', N'401d637b-3506-4826-8f05-ad9e90586a8f', N'cc392079-c522-4908-bf35-08de83cfc050', CAST(N'2026-03-18T09:01:45.347' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'10ebde1f-415b-463a-9303-c1840b91f5cd', N'4f1c3eef-97f2-43e5-a9cc-a417f54f546b', N'ca878aa7-7286-4e17-bf36-08de83cfc050', CAST(N'2026-03-18T08:17:55.113' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'5583a347-89ca-4d20-88b4-c849526f4414', N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', N'ee2131e4-c2af-43fc-bf2d-08de83cfc050', CAST(N'2026-03-18T14:35:56.143' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'ac27de4e-5009-40bd-958c-ced734e82c39', N'e7eaa083-407d-4cd6-9338-4b0513218772', N'86c5a948-c70b-44d9-bf34-08de83cfc050', CAST(N'2026-03-18T13:54:48.563' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MapTags] ([Id], [MapId], [TagId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'1a06f368-4260-40d2-99fa-e400c9f31fb4', N'1c3b4a0d-791b-47d6-be83-e8b74397e48e', N'cc392079-c522-4908-bf35-08de83cfc050', CAST(N'2026-03-18T05:55:05.947' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[MyMaps] ([Id], [MapId], [UserId], [IsAuthor], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a31f248b-355f-43e6-98c6-56fb4e2a5644', N'a4e3b5cc-3691-4a15-bfa1-d42bfce98df3', N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 1, CAST(N'2026-03-17T14:19:33.393' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MyMaps] ([Id], [MapId], [UserId], [IsAuthor], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'42ddf932-60a0-4e01-8c06-6702f575874c', N'1c3b4a0d-791b-47d6-be83-e8b74397e48e', N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 1, CAST(N'2026-03-18T05:55:05.963' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MyMaps] ([Id], [MapId], [UserId], [IsAuthor], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'b5616f92-3129-413e-a350-90d03c50e6ae', N'ca20cf05-0fd0-4f8a-9afe-6907ac9a71fa', N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 1, CAST(N'2026-03-18T14:28:24.120' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MyMaps] ([Id], [MapId], [UserId], [IsAuthor], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'58ec7216-8c11-4370-b962-a602b011ba45', N'401d637b-3506-4826-8f05-ad9e90586a8f', N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 1, CAST(N'2026-03-18T09:01:45.347' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MyMaps] ([Id], [MapId], [UserId], [IsAuthor], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'7d696227-4e98-4af2-9387-cca687ec7c3e', N'4f1c3eef-97f2-43e5-a9cc-a417f54f546b', N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 1, CAST(N'2026-03-18T08:04:55.173' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MyMaps] ([Id], [MapId], [UserId], [IsAuthor], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'd0247e00-f245-4620-bb3d-f400de04005e', N'e7eaa083-407d-4cd6-9338-4b0513218772', N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 1, CAST(N'2026-03-18T13:54:48.580' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[MyMaps] ([Id], [MapId], [UserId], [IsAuthor], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'd4dfc24d-67e9-4abd-b26c-ff5a67898256', N'0d100d4a-b3b7-4505-a22c-5b9a8851898d', N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 1, CAST(N'2026-03-17T04:32:53.513' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[Packages] ([Id], [Name], [DurationDays], [Limit], [Price], [FeaturesSpec], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'e0137795-afa1-4025-f7ff-08de83cfc05c', N'Free', 3650, 20, CAST(0.00 AS Decimal(18, 2)), N'Play basic maps; max 20 maps; no hints; cannot create/publish maps; no XP boost.', CAST(N'2026-03-17T02:49:04.197' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T13:28:54.857' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1)
INSERT [dbo].[Packages] ([Id], [Name], [DurationDays], [Limit], [Price], [FeaturesSpec], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'35b1f176-42af-4593-f800-08de83cfc05c', N'Pro', 30, NULL, CAST(149.00 AS Decimal(18, 2)), N'Play basic and advanced maps; hints enabled; cannot create/publish maps; XP boost enabled.', CAST(N'2026-03-17T02:49:04.220' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T13:28:54.857' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1)
INSERT [dbo].[Packages] ([Id], [Name], [DurationDays], [Limit], [Price], [FeaturesSpec], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'81986b87-4943-4c75-f801-08de83cfc05c', N'Creator', 30, NULL, CAST(299.00 AS Decimal(18, 2)), N'Play basic and advanced maps; hints enabled; can create and publish maps; map analytics; XP boost enabled.', CAST(N'2026-03-17T02:49:04.220' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', CAST(N'2026-03-18T13:28:54.857' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 0, NULL, NULL, 1)
GO
INSERT [dbo].[Payments] ([Id], [Code], [Name], [Description], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'4369766f-61ba-44ce-48c8-08de83cfc064', N'OrbitCoin', N'OrbitCoin', N'Virtual currency (in-platform)', CAST(N'2026-03-17T02:49:04.243' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Payments] ([Id], [Code], [Name], [Description], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'81f4d857-dbf7-4b3e-48c9-08de83cfc064', N'PayOS', N'PayOS', N'User top-up via PayOS', CAST(N'2026-03-17T02:49:04.277' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[Roles] ([Id], [Description], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [Status], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'0dec9a4b-27cd-43ec-cd5f-08de83cfc012', N'', CAST(N'2026-03-17T02:49:03.6967828' AS DateTime2), NULL, NULL, NULL, 1, N'Admin', N'ADMIN', NULL)
INSERT [dbo].[Roles] ([Id], [Description], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [Status], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'86ecc453-2646-469b-cd60-08de83cfc012', N'', CAST(N'2026-03-17T02:49:03.8075039' AS DateTime2), NULL, NULL, NULL, 1, N'Learner', N'LEARNER', NULL)
INSERT [dbo].[Roles] ([Id], [Description], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [Status], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'386ffb42-fccf-410b-cd61-08de83cfc012', N'', CAST(N'2026-03-17T02:49:03.8113895' AS DateTime2), NULL, NULL, NULL, 1, N'Moderator', N'MODERATOR', NULL)
GO
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'54d24283-2b8a-4acb-bf2a-08de83cfc050', N'Variables', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'1e85877f-1f7b-4948-bf2b-08de83cfc050', N'Operators', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'c4b7c2d6-cf68-43d5-bf2c-08de83cfc050', N'Conditionals', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'ee2131e4-c2af-43fc-bf2d-08de83cfc050', N'Loops', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'2400c8c8-8a3a-48fc-bf2e-08de83cfc050', N'Functions', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'0980c334-5ff3-4f6e-bf2f-08de83cfc050', N'Arrays', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'3423c886-1102-43b1-bf30-08de83cfc050', N'Objects', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'36c8e59f-6eb9-4b6d-bf31-08de83cfc050', N'Pointers', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'b721f10d-2fc5-4923-bf32-08de83cfc050', N'Recursion', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'654a60b6-cf59-46d8-bf33-08de83cfc050', N'Algorithm Basics', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'86c5a948-c70b-44d9-bf34-08de83cfc050', N'Beginner', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'cc392079-c522-4908-bf35-08de83cfc050', N'Easy', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'ca878aa7-7286-4e17-bf36-08de83cfc050', N'Medium', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'571e169f-7268-4ef0-bf37-08de83cfc050', N'Hard', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'732caeb6-6fe5-485d-bf38-08de83cfc050', N'Expert', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'56fe3ae9-ce51-4323-bf39-08de83cfc050', N'Pathfinding', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a11933a7-af6a-4db3-bf3a-08de83cfc050', N'Resource Collection', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'9c3637a2-68f3-4f86-bf3b-08de83cfc050', N'Obstacle Avoidance', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'ab04602e-c277-410b-bf3c-08de83cfc050', N'Logic Puzzle', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'6df4ba7c-e034-4cb6-bf3d-08de83cfc050', N'Optimization', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'3b63db4e-def3-4b85-bf3e-08de83cfc050', N'Pattern Recognition', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'a431c0b1-2134-4edc-bf3f-08de83cfc050', N'Strategy', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'146766fd-0ee9-4388-bf40-08de83cfc050', N'Logical Thinking', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'264c5ccf-270d-4642-bf41-08de83cfc050', N'Problem Solving', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'4ad455f7-ac39-444e-bf42-08de83cfc050', N'Computational Thinking', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'1814cf57-969c-4391-bf43-08de83cfc050', N'Algorithm Design', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
INSERT [dbo].[Tags] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'24bd5f03-1dfc-4748-bf44-08de83cfc050', N'Debugging', CAST(N'2026-03-17T02:49:04.110' AS DateTime), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[UserConceptProgresses] ([Id], [UserId], [ConceptId], [IsCompleted], [CompletedAt], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'86647c57-b53d-4548-8e11-383a5a932b62', N'330a05a6-3b99-49d9-9a92-f828ba123470', N'14c81509-fb12-451d-b0ed-afa9ee74af0c', 1, CAST(N'2026-03-17T02:54:34.4335510' AS DateTime2), CAST(N'2026-03-17T02:54:34.433' AS DateTime), N'330a05a6-3b99-49d9-9a92-f828ba123470', NULL, NULL, 0, NULL, NULL, 1)
GO
INSERT [dbo].[UserLearningGoals] ([Id], [UserId], [LearningGoalId], [SelectedAt], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedBy], [DeletedAt], [Status]) VALUES (N'66a88cc0-1633-45db-803c-c78c4dc5a44c', N'330a05a6-3b99-49d9-9a92-f828ba123470', N'b8aca0a2-1fab-4ebe-a272-a065f364ce90', CAST(N'2026-03-18T14:14:19.4540746' AS DateTime2), CAST(N'2026-03-17T02:53:06.437' AS DateTime), N'330a05a6-3b99-49d9-9a92-f828ba123470', CAST(N'2026-03-18T14:14:19.453' AS DateTime), N'330a05a6-3b99-49d9-9a92-f828ba123470', 0, NULL, NULL, 1)
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId]) VALUES (N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', N'0dec9a4b-27cd-43ec-cd5f-08de83cfc012')
INSERT [dbo].[UserRoles] ([UserId], [RoleId]) VALUES (N'4d9fb522-64fd-450b-327c-08de83cfc02d', N'86ecc453-2646-469b-cd60-08de83cfc012')
INSERT [dbo].[UserRoles] ([UserId], [RoleId]) VALUES (N'330a05a6-3b99-49d9-9a92-f828ba123470', N'86ecc453-2646-469b-cd60-08de83cfc012')
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [LastLoginAt], [JoiningAt], [RefreshToken], [RefreshTokenExpiryTime], [AvatarPath], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [Status], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Bio], [DateOfBirth], [Gender]) VALUES (N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', N'System', N'Admin', CAST(N'2026-03-18T14:35:36.1114118' AS DateTime2), CAST(N'2026-03-17T02:49:03.8211220' AS DateTime2), N'7yBGFu2l+5ozMhLBQrcBIUsWLKfC7ucW5I2OLtL4ER/wUqsxLJxXlWKRnArikmJiiuzI/De+U/aYT46wGRU7UQ==', CAST(N'2026-03-25T14:35:36.1114053' AS DateTime2), NULL, CAST(N'2026-03-17T02:49:03.8211411' AS DateTime2), NULL, CAST(N'2026-03-18T14:35:36.1114138' AS DateTime2), N'29f8c7e0-11bb-46c1-327b-08de83cfc02d', 1, N'admin@capstoneproject.com', N'ADMIN@CAPSTONEPROJECT.COM', N'admin@capstoneproject.com', N'ADMIN@CAPSTONEPROJECT.COM', 1, N'AQAAAAIAAYagAAAAEK2E8dQQU9+TNVS6xgORnQZUXmvyvto/z/06JZqOgj9jLhQSTZoUAqU0RTmWEy2DKA==', N'JSTEIL6UPC34I6HMT4ZCNXICHKRC6C4H', N'1c939823-d05a-486e-9a68-c3ef1aa69fea', NULL, 0, 0, NULL, 1, 0, NULL, NULL, NULL)
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [LastLoginAt], [JoiningAt], [RefreshToken], [RefreshTokenExpiryTime], [AvatarPath], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [Status], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Bio], [DateOfBirth], [Gender]) VALUES (N'4d9fb522-64fd-450b-327c-08de83cfc02d', N'Demo', N'User', NULL, CAST(N'2026-03-17T02:49:04.0479976' AS DateTime2), NULL, NULL, NULL, CAST(N'2026-03-17T02:49:04.0479977' AS DateTime2), NULL, NULL, NULL, 1, N'demo@capstoneproject.com', N'DEMO@CAPSTONEPROJECT.COM', N'demo@capstoneproject.com', N'DEMO@CAPSTONEPROJECT.COM', 1, N'AQAAAAIAAYagAAAAEDd4gSY7FDb3UEQdlT4yl0iESfqYHzl/zJpNwUjU7AYzuCG7JaWAcoa72W5pT37Y4Q==', N'OSAIXZ4FBGUS4ASNHY2MA7C7L4CJQHG5', N'e62cab19-3926-440e-94a8-70244da8f47d', NULL, 0, 0, NULL, 1, 0, NULL, NULL, NULL)
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [LastLoginAt], [JoiningAt], [RefreshToken], [RefreshTokenExpiryTime], [AvatarPath], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [Status], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [Bio], [DateOfBirth], [Gender]) VALUES (N'330a05a6-3b99-49d9-9a92-f828ba123470', N'NGUYEN NGOC', N'TIEN', CAST(N'2026-03-18T14:36:19.5473879' AS DateTime2), CAST(N'2026-03-17T02:52:33.9350218' AS DateTime2), N'SBoAdelQ77S595lbTN9t2Aq9UhdNl1YtRiR8+UcNZDbnMwCs4/DA6TqZU94m2huBqrT4WDNCiG9Z2qFnUiwc8w==', CAST(N'2026-03-25T14:36:19.5473816' AS DateTime2), NULL, CAST(N'2026-03-17T02:52:33.9357179' AS DateTime2), N'330a05a6-3b99-49d9-9a92-f828ba123470', CAST(N'2026-03-18T14:36:19.5473889' AS DateTime2), N'330a05a6-3b99-49d9-9a92-f828ba123470', 1, N'tiennnse181773@fpt.edu.vn', N'TIENNNSE181773@FPT.EDU.VN', N'tiennnse181773@fpt.edu.vn', N'TIENNNSE181773@FPT.EDU.VN', 1, N'AQAAAAIAAYagAAAAEB9z2a5QccW5qJX9XX2RMa297l3QBYmoDthxM8sVCK3rvzdQN9mrgQWhfdEZyAVO9g==', N'I3JSRRM7HW3YMASVYY37KETURQ7XMNJO', N'ef1b04ee-b543-43bb-855e-c875063830c8', N'0192837465', 0, 0, NULL, 1, 0, NULL, NULL, NULL)
GO
INSERT [HangFire].[AggregatedCounter] ([Key], [Value], [ExpireAt]) VALUES (N'stats:succeeded', 1, NULL)
INSERT [HangFire].[AggregatedCounter] ([Key], [Value], [ExpireAt]) VALUES (N'stats:succeeded:2026-03-18', 1, CAST(N'2026-04-18T02:28:02.503' AS DateTime))
INSERT [HangFire].[AggregatedCounter] ([Key], [Value], [ExpireAt]) VALUES (N'stats:succeeded:2026-03-18-02', 1, CAST(N'2026-03-19T02:28:02.503' AS DateTime))
GO
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'CreatedAt', N'1773715743443', NULL)
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'Cron', N'0 2 * * *', NULL)
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'Job', N'{"t":"CapstoneProject.Infrastructure.Services.QuickLoginCleanupJob, CapstoneProject.Infrastructure","m":"Execute","p":["System.Int32"],"a":["1"]}', NULL)
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'LastExecution', N'1773800881859', NULL)
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'LastJobId', N'1', NULL)
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'NextExecution', N'1773885600000', NULL)
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'Queue', N'default', NULL)
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'TimeZoneId', N'UTC', NULL)
INSERT [HangFire].[Hash] ([Key], [Field], [Value], [ExpireAt]) VALUES (N'recurring-job:quicklogin-cleanup-inactive', N'V', N'2', NULL)
GO
SET IDENTITY_INSERT [HangFire].[Job] ON 

INSERT [HangFire].[Job] ([Id], [StateId], [StateName], [InvocationData], [Arguments], [CreatedAt], [ExpireAt]) VALUES (1, 3, N'Succeeded', N'{"t":"CapstoneProject.Infrastructure.Services.QuickLoginCleanupJob, CapstoneProject.Infrastructure","m":"Execute","p":["System.Int32"]}', N'["1"]', CAST(N'2026-03-18T02:28:01.897' AS DateTime), CAST(N'2026-03-19T02:28:02.503' AS DateTime))
SET IDENTITY_INSERT [HangFire].[Job] OFF
GO
INSERT [HangFire].[JobParameter] ([JobId], [Name], [Value]) VALUES (1, N'CurrentCulture', N'"en-US"')
INSERT [HangFire].[JobParameter] ([JobId], [Name], [Value]) VALUES (1, N'CurrentUICulture', N'"en-US"')
INSERT [HangFire].[JobParameter] ([JobId], [Name], [Value]) VALUES (1, N'RecurringJobId', N'"quicklogin-cleanup-inactive"')
INSERT [HangFire].[JobParameter] ([JobId], [Name], [Value]) VALUES (1, N'Time', N'1773800881')
GO
INSERT [HangFire].[Schema] ([Version]) VALUES (9)
GO
INSERT [HangFire].[Set] ([Key], [Score], [Value], [ExpireAt]) VALUES (N'recurring-jobs', 1773885600, N'quicklogin-cleanup-inactive', NULL)
GO
SET IDENTITY_INSERT [HangFire].[State] ON 

INSERT [HangFire].[State] ([Id], [JobId], [Name], [Reason], [CreatedAt], [Data]) VALUES (1, 1, N'Enqueued', N'Triggered by recurring job scheduler', CAST(N'2026-03-18T02:28:02.167' AS DateTime), N'{"EnqueuedAt":"1773800881906","Queue":"default"}')
INSERT [HangFire].[State] ([Id], [JobId], [Name], [Reason], [CreatedAt], [Data]) VALUES (2, 1, N'Processing', NULL, CAST(N'2026-03-18T02:28:02.450' AS DateTime), N'{"StartedAt":"1773800882373","ServerId":"laptop-ebaqmo4n:26448:dd3b960c-b72c-4a85-b3e5-c6925feda0d0","WorkerId":"c6d2ccb5-2427-461c-8295-70b924bdcec9"}')
INSERT [HangFire].[State] ([Id], [JobId], [Name], [Reason], [CreatedAt], [Data]) VALUES (3, 1, N'Succeeded', NULL, CAST(N'2026-03-18T02:28:02.503' AS DateTime), N'{"SucceededAt":"1773800882498","PerformanceDuration":"44","Latency":"556"}')
SET IDENTITY_INSERT [HangFire].[State] OFF
GO
ALTER TABLE [dbo].[Achievements] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Achievements] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ChatRoomMembers] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ChatRoomMembers] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ChatRooms] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ChatRooms] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Concepts] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Concepts] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ExecutionsResults] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ExecutionsResults] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Hints] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Hints] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LearningGoals] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[LearningGoals] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[LearningPathItems] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[LearningPathItems] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[MapDetails] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MapDetails] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[MapRatings] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MapRatings] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[MapReports] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MapReports] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Maps] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Maps] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Maps] ADD  DEFAULT ((0)) FOR [WinCondition]
GO
ALTER TABLE [dbo].[Maps] ADD  DEFAULT ((0)) FOR [Type]
GO
ALTER TABLE [dbo].[MapTags] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MapTags] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Matches] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Matches] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[MessageReads] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MessageReads] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Messages] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Messages] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[MyMaps] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MyMaps] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Packages] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Packages] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[PaymentRecords] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PaymentRecords] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Payments] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[RoomParticipants] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[RoomParticipants] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Rooms] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Rooms] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Submissions] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Submissions] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserAchievements] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserAchievements] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserConceptProgresses] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserConceptProgresses] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserLearningGoals] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserLearningGoals] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserMapResults] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserMapResults] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserMatchResults] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserMatchResults] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserPackages] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserPackages] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[UserWallets] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserWallets] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[XpTransactions] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[XpTransactions] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[ChatRoomMembers]  WITH CHECK ADD  CONSTRAINT [FK_ChatRoomMembers_ChatRooms_ChatRoomId] FOREIGN KEY([ChatRoomId])
REFERENCES [dbo].[ChatRooms] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ChatRoomMembers] CHECK CONSTRAINT [FK_ChatRoomMembers_ChatRooms_ChatRoomId]
GO
ALTER TABLE [dbo].[ChatRoomMembers]  WITH CHECK ADD  CONSTRAINT [FK_ChatRoomMembers_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[ChatRoomMembers] CHECK CONSTRAINT [FK_ChatRoomMembers_Users_UserId]
GO
ALTER TABLE [dbo].[Concepts]  WITH CHECK ADD  CONSTRAINT [FK_Concepts_LearningGoals_LearningGoalId] FOREIGN KEY([LearningGoalId])
REFERENCES [dbo].[LearningGoals] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Concepts] CHECK CONSTRAINT [FK_Concepts_LearningGoals_LearningGoalId]
GO
ALTER TABLE [dbo].[ExecutionsResults]  WITH CHECK ADD  CONSTRAINT [FK_ExecutionsResults_Submissions_SubmissionId] FOREIGN KEY([SubmissionId])
REFERENCES [dbo].[Submissions] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExecutionsResults] CHECK CONSTRAINT [FK_ExecutionsResults_Submissions_SubmissionId]
GO
ALTER TABLE [dbo].[Hints]  WITH CHECK ADD  CONSTRAINT [FK_Hints_Maps_MapId] FOREIGN KEY([MapId])
REFERENCES [dbo].[Maps] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Hints] CHECK CONSTRAINT [FK_Hints_Maps_MapId]
GO
ALTER TABLE [dbo].[LearningPathItems]  WITH CHECK ADD  CONSTRAINT [FK_LearningPathItems_Concepts_ConceptId] FOREIGN KEY([ConceptId])
REFERENCES [dbo].[Concepts] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[LearningPathItems] CHECK CONSTRAINT [FK_LearningPathItems_Concepts_ConceptId]
GO
ALTER TABLE [dbo].[LearningPathItems]  WITH CHECK ADD  CONSTRAINT [FK_LearningPathItems_LearningGoals_LearningGoalId] FOREIGN KEY([LearningGoalId])
REFERENCES [dbo].[LearningGoals] ([Id])
GO
ALTER TABLE [dbo].[LearningPathItems] CHECK CONSTRAINT [FK_LearningPathItems_LearningGoals_LearningGoalId]
GO
ALTER TABLE [dbo].[LearningPathItems]  WITH CHECK ADD  CONSTRAINT [FK_LearningPathItems_Maps_MapId] FOREIGN KEY([MapId])
REFERENCES [dbo].[Maps] ([Id])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[LearningPathItems] CHECK CONSTRAINT [FK_LearningPathItems_Maps_MapId]
GO
ALTER TABLE [dbo].[MapDetails]  WITH CHECK ADD  CONSTRAINT [FK_MapDetails_Maps_MapId] FOREIGN KEY([MapId])
REFERENCES [dbo].[Maps] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MapDetails] CHECK CONSTRAINT [FK_MapDetails_Maps_MapId]
GO
ALTER TABLE [dbo].[MapReports]  WITH CHECK ADD  CONSTRAINT [FK_MapReports_Maps_MapId] FOREIGN KEY([MapId])
REFERENCES [dbo].[Maps] ([Id])
GO
ALTER TABLE [dbo].[MapReports] CHECK CONSTRAINT [FK_MapReports_Maps_MapId]
GO
ALTER TABLE [dbo].[Maps]  WITH CHECK ADD  CONSTRAINT [FK_Maps_Users_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Maps] CHECK CONSTRAINT [FK_Maps_Users_CreatedBy]
GO
ALTER TABLE [dbo].[MapTags]  WITH CHECK ADD  CONSTRAINT [FK_MapTags_Maps_MapId] FOREIGN KEY([MapId])
REFERENCES [dbo].[Maps] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MapTags] CHECK CONSTRAINT [FK_MapTags_Maps_MapId]
GO
ALTER TABLE [dbo].[MapTags]  WITH CHECK ADD  CONSTRAINT [FK_MapTags_Tags_TagId] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tags] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MapTags] CHECK CONSTRAINT [FK_MapTags_Tags_TagId]
GO
ALTER TABLE [dbo].[MessageReads]  WITH CHECK ADD  CONSTRAINT [FK_MessageReads_Messages_MessageId] FOREIGN KEY([MessageId])
REFERENCES [dbo].[Messages] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MessageReads] CHECK CONSTRAINT [FK_MessageReads_Messages_MessageId]
GO
ALTER TABLE [dbo].[MessageReads]  WITH CHECK ADD  CONSTRAINT [FK_MessageReads_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[MessageReads] CHECK CONSTRAINT [FK_MessageReads_Users_UserId]
GO
ALTER TABLE [dbo].[Messages]  WITH CHECK ADD  CONSTRAINT [FK_Messages_ChatRooms_ChatRoomId] FOREIGN KEY([ChatRoomId])
REFERENCES [dbo].[ChatRooms] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Messages] CHECK CONSTRAINT [FK_Messages_ChatRooms_ChatRoomId]
GO
ALTER TABLE [dbo].[Messages]  WITH CHECK ADD  CONSTRAINT [FK_Messages_Messages_ReplyToMessageId] FOREIGN KEY([ReplyToMessageId])
REFERENCES [dbo].[Messages] ([Id])
GO
ALTER TABLE [dbo].[Messages] CHECK CONSTRAINT [FK_Messages_Messages_ReplyToMessageId]
GO
ALTER TABLE [dbo].[Messages]  WITH CHECK ADD  CONSTRAINT [FK_Messages_Users_SenderId] FOREIGN KEY([SenderId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Messages] CHECK CONSTRAINT [FK_Messages_Users_SenderId]
GO
ALTER TABLE [dbo].[MyMaps]  WITH CHECK ADD  CONSTRAINT [FK_MyMaps_Maps_MapId] FOREIGN KEY([MapId])
REFERENCES [dbo].[Maps] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MyMaps] CHECK CONSTRAINT [FK_MyMaps_Maps_MapId]
GO
ALTER TABLE [dbo].[MyMaps]  WITH CHECK ADD  CONSTRAINT [FK_MyMaps_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MyMaps] CHECK CONSTRAINT [FK_MyMaps_Users_UserId]
GO
ALTER TABLE [dbo].[PaymentRecords]  WITH CHECK ADD  CONSTRAINT [FK_PaymentRecords_Packages_PackageId] FOREIGN KEY([PackageId])
REFERENCES [dbo].[Packages] ([Id])
GO
ALTER TABLE [dbo].[PaymentRecords] CHECK CONSTRAINT [FK_PaymentRecords_Packages_PackageId]
GO
ALTER TABLE [dbo].[PaymentRecords]  WITH CHECK ADD  CONSTRAINT [FK_PaymentRecords_Payments_PaymentId] FOREIGN KEY([PaymentId])
REFERENCES [dbo].[Payments] ([Id])
GO
ALTER TABLE [dbo].[PaymentRecords] CHECK CONSTRAINT [FK_PaymentRecords_Payments_PaymentId]
GO
ALTER TABLE [dbo].[RoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RoleClaims] CHECK CONSTRAINT [FK_RoleClaims_Roles_RoleId]
GO
ALTER TABLE [dbo].[RoomParticipants]  WITH CHECK ADD  CONSTRAINT [FK_RoomParticipants_Rooms_RoomId] FOREIGN KEY([RoomId])
REFERENCES [dbo].[Rooms] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RoomParticipants] CHECK CONSTRAINT [FK_RoomParticipants_Rooms_RoomId]
GO
ALTER TABLE [dbo].[RoomParticipants]  WITH CHECK ADD  CONSTRAINT [FK_RoomParticipants_Submissions_SubmissionId] FOREIGN KEY([SubmissionId])
REFERENCES [dbo].[Submissions] ([Id])
GO
ALTER TABLE [dbo].[RoomParticipants] CHECK CONSTRAINT [FK_RoomParticipants_Submissions_SubmissionId]
GO
ALTER TABLE [dbo].[Rooms]  WITH CHECK ADD  CONSTRAINT [FK_Rooms_Matches_MatchId] FOREIGN KEY([MatchId])
REFERENCES [dbo].[Matches] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Rooms] CHECK CONSTRAINT [FK_Rooms_Matches_MatchId]
GO
ALTER TABLE [dbo].[UserAchievements]  WITH CHECK ADD  CONSTRAINT [FK_UserAchievements_Achievements_AchievementId] FOREIGN KEY([AchievementId])
REFERENCES [dbo].[Achievements] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserAchievements] CHECK CONSTRAINT [FK_UserAchievements_Achievements_AchievementId]
GO
ALTER TABLE [dbo].[UserClaims]  WITH CHECK ADD  CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserClaims] CHECK CONSTRAINT [FK_UserClaims_Users_UserId]
GO
ALTER TABLE [dbo].[UserConceptProgresses]  WITH CHECK ADD  CONSTRAINT [FK_UserConceptProgresses_Concepts_ConceptId] FOREIGN KEY([ConceptId])
REFERENCES [dbo].[Concepts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserConceptProgresses] CHECK CONSTRAINT [FK_UserConceptProgresses_Concepts_ConceptId]
GO
ALTER TABLE [dbo].[UserConceptProgresses]  WITH CHECK ADD  CONSTRAINT [FK_UserConceptProgresses_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserConceptProgresses] CHECK CONSTRAINT [FK_UserConceptProgresses_Users_UserId]
GO
ALTER TABLE [dbo].[UserLearningGoals]  WITH CHECK ADD  CONSTRAINT [FK_UserLearningGoals_LearningGoals_LearningGoalId] FOREIGN KEY([LearningGoalId])
REFERENCES [dbo].[LearningGoals] ([Id])
GO
ALTER TABLE [dbo].[UserLearningGoals] CHECK CONSTRAINT [FK_UserLearningGoals_LearningGoals_LearningGoalId]
GO
ALTER TABLE [dbo].[UserLearningGoals]  WITH CHECK ADD  CONSTRAINT [FK_UserLearningGoals_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserLearningGoals] CHECK CONSTRAINT [FK_UserLearningGoals_Users_UserId]
GO
ALTER TABLE [dbo].[UserLogins]  WITH CHECK ADD  CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserLogins] CHECK CONSTRAINT [FK_UserLogins_Users_UserId]
GO
ALTER TABLE [dbo].[UserMatchResults]  WITH CHECK ADD  CONSTRAINT [FK_UserMatchResults_Matches_MatchId] FOREIGN KEY([MatchId])
REFERENCES [dbo].[Matches] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserMatchResults] CHECK CONSTRAINT [FK_UserMatchResults_Matches_MatchId]
GO
ALTER TABLE [dbo].[UserMatchResults]  WITH CHECK ADD  CONSTRAINT [FK_UserMatchResults_Submissions_SubmissionId] FOREIGN KEY([SubmissionId])
REFERENCES [dbo].[Submissions] ([Id])
GO
ALTER TABLE [dbo].[UserMatchResults] CHECK CONSTRAINT [FK_UserMatchResults_Submissions_SubmissionId]
GO
ALTER TABLE [dbo].[UserPackages]  WITH CHECK ADD  CONSTRAINT [FK_UserPackages_Packages_PackageId] FOREIGN KEY([PackageId])
REFERENCES [dbo].[Packages] ([Id])
GO
ALTER TABLE [dbo].[UserPackages] CHECK CONSTRAINT [FK_UserPackages_Packages_PackageId]
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Roles_RoleId]
GO
ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Users_UserId]
GO
ALTER TABLE [dbo].[UserTokens]  WITH CHECK ADD  CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserTokens] CHECK CONSTRAINT [FK_UserTokens_Users_UserId]
GO
ALTER TABLE [HangFire].[JobParameter]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_JobParameter_Job] FOREIGN KEY([JobId])
REFERENCES [HangFire].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [HangFire].[JobParameter] CHECK CONSTRAINT [FK_HangFire_JobParameter_Job]
GO
ALTER TABLE [HangFire].[State]  WITH CHECK ADD  CONSTRAINT [FK_HangFire_State_Job] FOREIGN KEY([JobId])
REFERENCES [HangFire].[Job] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [HangFire].[State] CHECK CONSTRAINT [FK_HangFire_State_Job]
GO
