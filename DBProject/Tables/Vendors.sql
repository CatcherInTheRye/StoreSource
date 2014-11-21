CREATE TABLE [dbo].[Vendors] (
    [Id]          INT            IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Title]       NVARCHAR (100) NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    [AddressId]   INT            NULL,
    CONSTRAINT [PK_Vendor] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Vendor_Address] FOREIGN KEY ([AddressId]) REFERENCES [dbo].[Addresses] ([Id])
);

