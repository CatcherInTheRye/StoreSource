CREATE TABLE [dbo].[studentSupportReports] (
    [id]                      INT      IDENTITY (1, 1)     NOT NULL,
    [studentId]               INT           NOT NULL,
    [specialistId]            INT           NOT NULL,
    [studentSupportRequestId] INT           NOT NULL,
    [dateStarted]             SMALLDATETIME NOT NULL,
    [dateFinalized]           SMALLDATETIME NULL,
    [lastUpdated]             SMALLDATETIME NULL,
    [background]              NTEXT         NULL,
    [observations]            NTEXT         NULL,
    [interpretations]         NTEXT         NULL,
    [goals]                   NTEXT         NULL,
    [progress]                NTEXT         NULL,
    [recommendations]         NTEXT         NULL,
    [summary]                 NTEXT         NULL,
    [status]                  NTEXT         NULL,
    CONSTRAINT [PK_studentSupportReport] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_studentSupportReport_students] FOREIGN KEY ([studentId]) REFERENCES [dbo].[students] ([id]),
    CONSTRAINT [FK_studentSupportReport_studentSupportRequest] FOREIGN KEY ([studentSupportRequestId]) REFERENCES [dbo].[studentSupportRequests] ([id]),
    CONSTRAINT [FK_studentSupportReport_users] FOREIGN KEY ([specialistId]) REFERENCES [dbo].[users] ([id])
);

