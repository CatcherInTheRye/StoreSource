CREATE TABLE [dbo].[studentSupportRequestNotification] (
    [id]                      INT IDENTITY (1, 1) NOT NULL,
    [studentSupportRequestId] INT NOT NULL,
    [userId]                  INT NOT NULL,
    CONSTRAINT [PK_supportRequestNotification] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_supportRequestNotification_supportRequestNotification] FOREIGN KEY ([studentSupportRequestId]) REFERENCES [dbo].[studentSupportRequests] ([id]),
    CONSTRAINT [FK_supportRequestNotification_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id])
);

