CREATE TABLE [dbo].[Products] (
    [Id]           INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Title]        NVARCHAR (100)  NOT NULL,
    [Description]  NVARCHAR (MAX)  NULL,
    [Model]        NVARCHAR (100)  NOT NULL,
    [Cost]         DECIMAL (20, 4) NULL,
    [Price]        DECIMAL (20, 4) NULL,
    [MarginRuleId] INT             NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Product_MarginRule] FOREIGN KEY ([MarginRuleId]) REFERENCES [dbo].[MarginRules] ([Id])
);

