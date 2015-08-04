CREATE TABLE [dbo].[InanimateData] (
    [Id]                    BIGINT        IDENTITY (1, 1) NOT NULL,
    [Created]               DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]           DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [Name]					VARCHAR (200) NOT NULL,
	[MobileContainers]		VARCHAR (MAX) DEFAULT ('{}') NOT NULL,
	[InanimateContainers]	VARCHAR (MAX) DEFAULT ('{}') NOT NULL,
    [LastKnownLocation]     BIGINT        DEFAULT ((-1)) NOT NULL,
    [LastKnownLocationType] VARCHAR (MAX) DEFAULT ('') NOT NULL,
);
