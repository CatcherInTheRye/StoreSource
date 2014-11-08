CREATE TABLE [dbo].[studentTeamConsultations] (
    [id]              INT    IDENTITY (1, 1)       NOT NULL,
    [studentId]       INT           NOT NULL,
    [dateStarted]     SMALLDATETIME NOT NULL,
    [dateFinalized]   SMALLDATETIME NOT NULL,
    [background]      NTEXT         NULL,
    [observations]    NTEXT         NULL,
    [interpretations] NTEXT         NULL,
    [goals]           NTEXT         NULL,
    [progress]        NTEXT         NULL,
    [recommendations] NTEXT         NULL,
    [summary]         NTEXT         NULL,
    [status]          NTEXT         NULL,
    CONSTRAINT [PK_studentTeamConsultations] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_studentTeamConsultations_students] FOREIGN KEY ([studentId]) REFERENCES [dbo].[students] ([id])
);

