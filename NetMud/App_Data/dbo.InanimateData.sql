CREATE TABLE [dbo].[InanimateData] (
    [Id]						BIGINT			IDENTITY (1, 1) NOT NULL,
    [Created]					DATETIME		DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]				DATETIME		DEFAULT (getutcdate()) NOT NULL,
    [Name]						VARCHAR (200)	NOT NULL,
	[DimensionalModelLength]	INT				NOT NULL DEFAULT (-1),
	[DimensionalModelHeight]	INT				NOT NULL DEFAULT (-1),
	[DimensionalModelWidth]		INT				NOT NULL DEFAULT (-1),
	[DimensionalModelMaterialCompositions]		VARCHAR (MAX)	DEFAULT ('{}') NOT NULL,
	[DimensionalModelID]		BIGINT			NOT NULL,
	[MobileContainers]			VARCHAR (MAX)	DEFAULT ('{}') NOT NULL,
	[InanimateContainers]		VARCHAR (MAX)	DEFAULT ('{}') NOT NULL
);
