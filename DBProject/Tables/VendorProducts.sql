CREATE TABLE [dbo].[VendorProducts] (
    [Id]        INT IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [VendorId]  INT NOT NULL,
    [ProductId] INT NOT NULL,
    CONSTRAINT [PK_VendorProduct] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_VendorProduct_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]),
    CONSTRAINT [FK_VendorProduct_Vendor] FOREIGN KEY ([VendorId]) REFERENCES [dbo].[Vendors] ([Id])
);

