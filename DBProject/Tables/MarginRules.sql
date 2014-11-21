CREATE TABLE [dbo].[MarginRules] (
    [Id]        INT            IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Margin]    DECIMAL (9, 2) NOT NULL,
    [IsPercent] BIT            NOT NULL,
    CONSTRAINT [PK_MarginRules] PRIMARY KEY CLUSTERED ([Id] ASC)
);

