CREATE TABLE [dbo].[organizationSettings] (
    [id]             INT           IDENTITY (1, 1) NOT NULL,
    [organizationId] INT           NOT NULL,
    [sync]           BIT           CONSTRAINT [DF_organizationSettings_sync] DEFAULT ((0)) NOT NULL,
    [powerSchoolUrl] VARCHAR (100) NULL,
    [clientId]       VARCHAR (100) NULL,
    [clientSecret]   VARCHAR (100) NULL,
    CONSTRAINT [PK_organizationSettings] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_organizationSettings_organizations] FOREIGN KEY ([organizationId]) REFERENCES [dbo].[organizations] ([id])
);



