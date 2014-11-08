CREATE TABLE [dbo].[roleAreas] (
    [ID]    INT          IDENTITY (1, 1) NOT NULL,
    [Text]  VARCHAR (50) NULL,
    [href]  VARCHAR (50) NOT NULL,
    [image] VARCHAR (50) NOT NULL,
    [role]  INT          NOT NULL,
    CONSTRAINT [PK_roleArea] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_roleArea_roles] FOREIGN KEY ([role]) REFERENCES [dbo].[roles] ([id])
);

