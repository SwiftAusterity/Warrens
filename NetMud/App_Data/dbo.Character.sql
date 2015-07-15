CREATE TABLE [dbo].[Character] (
    [Id]            BIGINT        IDENTITY (1, 1) NOT NULL,
    [Created]       DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]   DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [SurName]       VARCHAR (200) NOT NULL,
    [GivenName]     VARCHAR (200) NOT NULL,
    [AccountHandle] VARCHAR (200) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

