CREATE TABLE [dbo].[organizations] (
    [id]             INT           IDENTITY (1, 1) NOT NULL,
    [districtName]   VARCHAR (100) NOT NULL,
    [address]        VARCHAR (100) NULL,
    [city]           VARCHAR (50)  NULL,
    [province]       VARCHAR (50)  NULL,
    [postalCode]     VARCHAR (15)  NULL,
    [contactPhone]   VARCHAR (20)  NULL,
    [contactFax]     VARCHAR (20)  NULL,
    [contactEmail]   VARCHAR (100) NOT NULL,
    [contactName]    VARCHAR (100) NOT NULL,
    [systemUserName] VARCHAR (50)  NOT NULL,
    [systemPassword] VARCHAR (50)  NOT NULL,
    [active]         BIT           NULL,
    CONSTRAINT [PK_organizations] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [IX_organizations] UNIQUE NONCLUSTERED ([districtName] ASC)
);





