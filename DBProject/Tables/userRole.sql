CREATE TABLE [dbo].[userRole] (
    [userId] INT NOT NULL,
    [roleId] INT NOT NULL,
    CONSTRAINT [PK_userRole] PRIMARY KEY CLUSTERED ([userId] ASC, [roleId] ASC),
    CONSTRAINT [FK_userRole_roles] FOREIGN KEY ([roleId]) REFERENCES [dbo].[roles] ([id]),
    CONSTRAINT [FK_userRole_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id])
);

