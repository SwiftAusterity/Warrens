CREATE TABLE [dbo].[NonPlayerCharacter] (
    [Id]                    BIGINT        IDENTITY (1, 1) NOT NULL,
    [Created]               DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]           DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [SurName]               VARCHAR (200) NOT NULL,
    [Name]                  VARCHAR (200) NOT NULL,
    [Gender]                VARCHAR (200) DEFAULT ('') NOT NULL,
	[Race]					BIGINT NOT NULL DEFAULT (0),
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
