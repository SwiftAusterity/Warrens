CREATE TABLE [dbo].[Account] (
    [GlobalIdentityHandle] VARCHAR (200) NOT NULL,
    [LogChannelSubscriptions] VARCHAR (MAX) default('') NOT NULL,
    PRIMARY KEY CLUSTERED ([GlobalIdentityHandle] ASC)
);

