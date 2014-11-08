﻿CREATE TABLE [dbo].[studentHistory] (
    [id]                INT      IDENTITY (1, 1)     NOT NULL,
    [studentId]         INT           NOT NULL,
    [schoolId]          INT           NOT NULL,
    [studentNumber]     VARCHAR (20)  NOT NULL,
    [lastName]          VARCHAR (100) NOT NULL,
    [firstName]         VARCHAR (100) NOT NULL,
    [middleName]        VARCHAR (100) NULL,
    [gender]            CHAR (1)      NOT NULL,
    [dob]               SMALLDATETIME NOT NULL,
    [grade]             VARCHAR (25)  NOT NULL,
    [code]              VARCHAR (25)  NULL,
    [specialPrograms]   VARCHAR (250) NULL,
    [homePhone]         VARCHAR (10)  NULL,
    [mailingAddress]    VARCHAR (100) NULL,
    [mailingCity]       VARCHAR (50)  NULL,
    [mailingProvince]   VARCHAR (50)  NULL,
    [mailingPostalCode] VARCHAR (6)   NULL,
    [address]           VARCHAR (100) NULL,
    [city]              VARCHAR (50)  NULL,
    [province]          VARCHAR (50)  NULL,
    [postalCode]        VARCHAR (6)   NULL,
    [motherName]        VARCHAR (100) NULL,
    [motherPhone]       VARCHAR (10)  NULL,
    [motherEmail]       VARCHAR (100) NULL,
    [fatherName]        VARCHAR (100) NULL,
    [fatherPhone]       VARCHAR (10)  NULL,
    [fatherEmail]       VARCHAR (100) NULL,
    [guardianName]      VARCHAR (100) NULL,
    [guardianPhone]     VARCHAR (10)  NULL,
    [guardianEmail]     VARCHAR (100) NULL,
    [sourceId]          VARCHAR (100) NOT NULL,
    [from]              SMALLDATETIME NOT NULL,
    [to]                SMALLDATETIME NOT NULL,
    [updatedBy]         VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_studentHistory] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_studentHistory_schools] FOREIGN KEY ([schoolId]) REFERENCES [dbo].[schools] ([id]),
    CONSTRAINT [FK_studentHistory_students] FOREIGN KEY ([studentId]) REFERENCES [dbo].[students] ([id])
);
