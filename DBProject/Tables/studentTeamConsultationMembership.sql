CREATE TABLE [dbo].[studentTeamConsultationMembership] (
    [id]                        INT    IDENTITY (1, 1)       NOT NULL,
    [studentTeamConsultationId] INT           NOT NULL,
    [userId]                    INT           NOT NULL,
    [finalized]                 BIT           CONSTRAINT [DF_studentTeamConsultationMembership_finalized] DEFAULT ((0)) NOT NULL,
    [dateFinalized]             SMALLDATETIME NULL,
    CONSTRAINT [PK_studentTeamConsultationMembership] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_studentTeamConsultationMembership_studentTeamConsultations] FOREIGN KEY ([studentTeamConsultationId]) REFERENCES [dbo].[studentTeamConsultations] ([id]),
    CONSTRAINT [FK_studentTeamConsultationMembership_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id])
);

