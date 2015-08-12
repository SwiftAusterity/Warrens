CREATE TABLE [dbo].[Character] (
    [Id]					BIGINT			IDENTITY (1, 1) NOT NULL,
    [Created]				DATETIME		DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]			DATETIME		DEFAULT (getutcdate()) NOT NULL,
    [SurName]				VARCHAR (200)	NOT NULL,
    [Name]					VARCHAR (200)	NOT NULL,
	[Gender]				VARCHAR (200)	NOT NULL,
	[GamePermissionsRank]	SMALLINT		DEFAULT(0),
    [AccountHandle]			VARCHAR (200)	NOT NULL,
	[DimensionalModelID]	BIGINT			NOT NULL,
	[LastKnownLocation]		VARCHAR(MAX)	DEFAULT('') NOT NULL,
	[LastKnownLocationType]	VARCHAR(MAX)	DEFAULT('') NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

