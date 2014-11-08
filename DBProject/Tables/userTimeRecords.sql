CREATE TABLE [dbo].[userTimeRecords] (
    [id]        INT           IDENTITY (1, 1) NOT NULL,
    [userId]    INT           NOT NULL,
    [studentId] INT           NULL,
    [date]      SMALLDATETIME NOT NULL,
    [code]      VARCHAR (10)  NULL,
    [time]      INT           NOT NULL,
    [notes]     NTEXT         NULL,
    CONSTRAINT [PK_userTimeRecords] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_userTimeRecords_students] FOREIGN KEY ([studentId]) REFERENCES [dbo].[students] ([id]),
    CONSTRAINT [FK_userTimeRecords_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id])
);



