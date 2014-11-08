CREATE TABLE [dbo].[settings] (
    [Id]    INT IDENTITY (1, 1) NOT NULL,
    [Month] INT NOT NULL,
    [Year]  INT NOT NULL,
    [Hours] INT NOT NULL,
    CONSTRAINT [Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);

