CREATE TABLE [dbo].[RoomData]
(
    [Id]						BIGINT			IDENTITY (1, 1) NOT NULL,
    [Created]					DATETIME		DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]				DATETIME		DEFAULT (getutcdate()) NOT NULL,
    [Name]						VARCHAR (200)	NOT NULL,
	[Borders]					VARCHAR (MAX)	NOT NULL DEFAULT('{}'),
	[Medium]					BIGINT			NOT NULL DEFAULT((-1)),
    [Zone]						BIGINT			DEFAULT ((-1)) NOT NULL,
	[DimensionalModelLength]	INT				NOT NULL DEFAULT (-1),
	[DimensionalModelHeight]	INT				NOT NULL DEFAULT (-1),
	[DimensionalModelWidth]		INT				NOT NULL DEFAULT (-1)
)
