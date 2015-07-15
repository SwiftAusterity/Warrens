CREATE TABLE [dbo].[Account] (
    [GlobalIdentityHandle] VARCHAR (200) NOT NULL,
    [CurrentlySelectedCharacter] BIGINT NULL, 
    PRIMARY KEY CLUSTERED ([GlobalIdentityHandle] ASC)
);

