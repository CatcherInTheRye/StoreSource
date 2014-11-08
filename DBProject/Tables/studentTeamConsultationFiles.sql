CREATE TABLE [dbo].[studentTeamConsultationFiles] (
    [id]                        INT   IDENTITY (1, 1)        NOT NULL,
    [studentTeamConsultationId] INT           NOT NULL,
    [fileName]                  VARCHAR (100) NOT NULL,
    [fileContent]               IMAGE         NOT NULL,
    CONSTRAINT [PK_studentTeamConsultationFiles] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_studentTeamConsultationFiles_studentTeamConsultation] FOREIGN KEY ([studentTeamConsultationId]) REFERENCES [dbo].[studentTeamConsultations] ([id])
);

