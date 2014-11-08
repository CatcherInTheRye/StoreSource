CREATE TABLE [dbo].[studentSupportRequestAssignments] (
    [id]                      INT    IDENTITY (1, 1)       NOT NULL,
    [studentSupportRequestId] INT           NOT NULL,
    [specialistId]            INT           NOT NULL,
    [supportRequestTypeId]    INT           NOT NULL,
    [dateAssigned]            SMALLDATETIME NOT NULL,
    CONSTRAINT [PK_supportRequestAssignment] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_supportRequestAssignment_studentSupportRequest] FOREIGN KEY ([studentSupportRequestId]) REFERENCES [dbo].[studentSupportRequests] ([id]),
    CONSTRAINT [FK_supportRequestAssignment_supportRequestTypes] FOREIGN KEY ([supportRequestTypeId]) REFERENCES [dbo].[supportRequestTypes] ([id]),
    CONSTRAINT [FK_supportRequestAssignment_users] FOREIGN KEY ([specialistId]) REFERENCES [dbo].[users] ([id])
);

