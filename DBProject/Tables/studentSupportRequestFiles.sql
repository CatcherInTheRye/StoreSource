CREATE TABLE [dbo].[studentSupportRequestFiles] (
    [id]                      INT     IDENTITY (1, 1)      NOT NULL,
    [studentSupportRequestId] INT           NOT NULL,
    [fileName]                VARCHAR (100) NOT NULL,
    [fileContent]             IMAGE         NOT NULL,
    [dateUploaded]            SMALLDATETIME NOT NULL,
    CONSTRAINT [PK_supportRequestFiles] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_supportRequestFile_studentSupportRequest] FOREIGN KEY ([studentSupportRequestId]) REFERENCES [dbo].[studentSupportRequests] ([id])
);



