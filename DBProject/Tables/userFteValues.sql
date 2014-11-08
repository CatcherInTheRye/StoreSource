CREATE TABLE [dbo].[userFteValues] (
    [id]       INT  IDENTITY (1, 1)         NOT NULL,
    [userId]   INT           NOT NULL,
    [fteValue] FLOAT (53)    NOT NULL,
    [fteFrom]  SMALLDATETIME NULL,
    [fteTo]    SMALLDATETIME NULL,
    CONSTRAINT [PK_userFteValues] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_userFteValues_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id])
);

