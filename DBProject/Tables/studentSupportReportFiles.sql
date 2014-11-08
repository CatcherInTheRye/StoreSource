CREATE TABLE [dbo].[studentSupportReportFiles] (
    [id]                     INT            IDENTITY (1, 1) NOT NULL,
    [studentSupportReportId] INT            NOT NULL,
    [fileName]               VARCHAR (100)  NOT NULL,
    [fileContent]            IMAGE          NOT NULL,
    [dateUploaded]           SMALLDATETIME  NOT NULL,
    [uploadedBy]             INT            NOT NULL,
    [description]            NVARCHAR (200) NULL,
    CONSTRAINT [PK_supportReportFile] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_supportReportFile_studentSupportReport] FOREIGN KEY ([studentSupportReportId]) REFERENCES [dbo].[studentSupportReports] ([id]),
    CONSTRAINT [FK_supportReportFile_user] FOREIGN KEY ([uploadedBy]) REFERENCES [dbo].[users] ([id])
);





