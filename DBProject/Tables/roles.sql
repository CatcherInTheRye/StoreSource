CREATE TABLE [dbo].[roles] (
    [id]              INT      IDENTITY (1, 1)     NOT NULL,
    [roleName]        VARCHAR (20)  NOT NULL,
    [roleDescription] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_roles] PRIMARY KEY CLUSTERED ([id] ASC)
);

