CREATE TABLE [dbo].[Race]
(
    [Id]					BIGINT        IDENTITY (1, 1) NOT NULL,
    [Created]				DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [LastRevised]			DATETIME      DEFAULT (getutcdate()) NOT NULL,
    [Name]					VARCHAR (200) NOT NULL,
	[Arms]					VARCHAR (MAX) NOT NULL,
	[Legs]					VARCHAR (MAX) NOT NULL,
	[Torso]					BIGINT NOT NULL,
	[Head]					BIGINT NOT NULL,
	[BodyParts]				VARCHAR (MAX) NOT NULL,
	[DietaryNeeds]			SMALLINT NOT NULL,
	[SanguinaryMaterial]	BIGINT NOT NULL,
	[VisionRange]           VARCHAR (MAX) NOT NULL,
	[TemperatureTolerance]  VARCHAR (MAX) NOT NULL,
	[Breathes]				SMALLINT NOT NULL,
	[TeethType]				SMALLINT NOT NULL,
	[StartingLocation]		BIGINT NOT NULL,
	[EmergencyLocation]		BIGINT NOT NULL
)