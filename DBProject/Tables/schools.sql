CREATE TABLE [dbo].[schools] (
    [id]             INT     IDENTITY (1, 1)      NOT NULL,
    [organizationId] INT           NOT NULL,
    [schoolName]     VARCHAR (50)  NOT NULL,
    [address]        VARCHAR (100) NULL,
    [city]           VARCHAR (100) NULL,
    [province]       VARCHAR (50)  NULL,
    [postalCode]     VARCHAR (6)   NULL,
    [phone]          VARCHAR (10)  NULL,
    [fax]            VARCHAR (10)  NULL,
    [email]          VARCHAR (100) NULL,
    [website]        VARCHAR (100) NULL,
    [sourceId]       VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_schools] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_schools_organizations] FOREIGN KEY ([organizationId]) REFERENCES [dbo].[organizations] ([id])
);

