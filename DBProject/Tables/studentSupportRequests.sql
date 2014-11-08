CREATE TABLE [dbo].[studentSupportRequests] (
    [id]            INT           IDENTITY (1, 1) NOT NULL,
    [studentId]     INT           NOT NULL,
    [requestedBy]   INT           NOT NULL,
    [dateRequested] SMALLDATETIME NOT NULL,
    [reason]        NTEXT         NULL,
    [approvalNote]  NTEXT         NULL,
    [approved]      BIT           NOT NULL,
    [denied]        BIT           NOT NULL,
    [whoApproved]   INT           NULL,
    [dateApproved]  SMALLDATETIME NULL,
    [submitted]     BIT           CONSTRAINT [submittedContraint] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_studentSupportRequest] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_studentSupportRequest_students] FOREIGN KEY ([studentId]) REFERENCES [dbo].[students] ([id]),
    CONSTRAINT [FK_studentSupportRequest_users] FOREIGN KEY ([requestedBy]) REFERENCES [dbo].[users] ([id]),
    CONSTRAINT [FK_studentSupportRequest_users1] FOREIGN KEY ([whoApproved]) REFERENCES [dbo].[users] ([id])
);







