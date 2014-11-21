CREATE TABLE [dbo].[Addresses] (
    [Id]         INT            IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Phone]      VARCHAR (30)   NULL,
    [HomePhone]  VARCHAR (30)   NULL,
    [City]       NVARCHAR (100) NULL,
    [Region]     NVARCHAR (100) NULL,
    [Street]     NVARCHAR (100) NULL,
    [PostalCode] NVARCHAR (20)  NULL,
    [Email]      NVARCHAR (100) NULL,
    CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED ([Id] ASC)
);

