CREATE TABLE [dbo].[Help] (
    [Id]          INT           NOT NULL IDENTITY,
    [Created]     DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [LastRevised] DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [Name]        VARCHAR (200) NOT NULL,
    [HelpText]    VARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

