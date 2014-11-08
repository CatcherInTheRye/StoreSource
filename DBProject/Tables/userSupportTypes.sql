CREATE TABLE [dbo].[userSupportTypes] (
    [id]                   INT IDENTITY (1, 1) NOT NULL,
    [userId]               INT NOT NULL,
    [supportRequestTypeId] INT NOT NULL,
    CONSTRAINT [PK_userSupportTypes] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_userSupportTypes_supportRequestTypes] FOREIGN KEY ([supportRequestTypeId]) REFERENCES [dbo].[supportRequestTypes] ([id]),
    CONSTRAINT [FK_userSupportTypes_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id]),
    CONSTRAINT [IX_userSupportTypes] UNIQUE NONCLUSTERED ([id] ASC, [userId] ASC, [supportRequestTypeId] ASC)
);



