CREATE TABLE [dbo].[studentSupportRequestTypes] (
    [id]                      INT IDENTITY (1, 1) NOT NULL,
    [studentSupportRequestId] INT NOT NULL,
    [supportRequestTypeId]    INT NOT NULL,
    [approved]                BIT NOT NULL,
    CONSTRAINT [PK_supportRequestType] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_supportRequestType_studentSupportRequest] FOREIGN KEY ([studentSupportRequestId]) REFERENCES [dbo].[studentSupportRequests] ([id]),
    CONSTRAINT [FK_supportRequestType_supportRequestTypes] FOREIGN KEY ([supportRequestTypeId]) REFERENCES [dbo].[supportRequestTypes] ([id])
);

