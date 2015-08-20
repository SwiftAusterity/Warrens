CREATE TABLE [dbo].[Race]
(
    [Id]					BIGINT        IDENTITY (1, 1) NOT NULL,
    [Created]				DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]			DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [Name]					VARCHAR (200) NOT NULL,
	[ArmsId]				BIGINT NOT NULL,
	[ArmsAmount]			SMALLINT NOT NULL,
	[LegsId]				BIGINT NOT NULL,
	[LegsAmount]			SMALLINT NOT NULL,
	[Torso]					BIGINT NOT NULL,
	[Head]					BIGINT NOT NULL,
	[BodyParts]				VARCHAR (MAX) NOT NULL,
	[DietaryNeeds]			SMALLINT NOT NULL,
	[SanguinaryMaterial]	BIGINT NOT NULL,
	[VisionRangeLow]        SMALLINT NOT NULL,
	[VisionRangeHigh]       SMALLINT NOT NULL,
	[TemperatureToleranceLow]  SMALLINT NOT NULL,
	[TemperatureToleranceHigh]  SMALLINT NOT NULL,
	[Breathes]				SMALLINT NOT NULL,
	[TeethType]				SMALLINT NOT NULL,
	[StartingLocation]		BIGINT NOT NULL,
	[EmergencyLocation]		BIGINT NOT NULL
)