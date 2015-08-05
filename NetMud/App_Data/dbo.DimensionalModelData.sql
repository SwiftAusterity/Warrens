CREATE TABLE [dbo].[DimensionalModelData]
(
	[Id]                    BIGINT        IDENTITY (1, 1) NOT NULL,
    [Created]               DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]           DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [Name]                  VARCHAR (200) NOT NULL,
	[Model]					VARCHAR (MAX) NOT NULL
)
