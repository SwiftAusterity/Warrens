CREATE TABLE [dbo].[Character]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[Created]     DATETIME DEFAULT (getutcdate()) NOT NULL,
    [LastRevised] DATETIME DEFAULT (getutcdate()) NOT NULL,
    [Name]        VARCHAR (200) NOT NULL,
	[SurName]     VARCHAR (200) NOT NULL,
	[GivenName]   VARCHAR (200) NOT NULL
)
