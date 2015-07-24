CREATE TABLE [dbo].[Account] (
    [GlobalIdentityHandle] VARCHAR (200) NOT NULL,
    [LogChannelSubscriptions] VARCHAR (MAX) default('') NOT NULL,
    [CurrentlySelectedCharacter] BIGINT NULL, 
    PRIMARY KEY CLUSTERED ([GlobalIdentityHandle] ASC)
);

