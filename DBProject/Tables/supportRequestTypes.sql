CREATE TABLE [dbo].[supportRequestTypes] (
    [id]   INT     IDENTITY (1, 1)     NOT NULL,
    [name] VARCHAR (50) NOT NULL,
    [code] VARCHAR (5)  NOT NULL,
    CONSTRAINT [PK_supportRequestTypes] PRIMARY KEY CLUSTERED ([id] ASC)
);

