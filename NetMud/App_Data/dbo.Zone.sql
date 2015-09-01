CREATE TABLE [dbo].[Zone]
(
    [Id]						BIGINT			IDENTITY (1, 1) NOT NULL,
    [Created]					DATETIME		DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]				DATETIME		DEFAULT (getutcdate()) NOT NULL,
    [Name]						VARCHAR(200)	NOT NULL,
    [BaseElevation]				INT				DEFAULT ((0)) NOT NULL,
    [TemperatureCoefficient]	INT				DEFAULT ((0)) NOT NULL,
    [PressureCoefficient]		INT				DEFAULT ((0)) NOT NULL,
    [Owner]						BIGINT			DEFAULT ((-1)) NOT NULL,
	[Claimable]					BIT				DEFAULT ((0)) NOT NULL
)