CREATE TABLE [dbo].[Users] (
    [Id]                  INT            IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Email]               NVARCHAR (255) NOT NULL,
    [SecondaryEmail]      NVARCHAR (255) NULL,
    [Login]               NVARCHAR (100) NULL,
    [Password]            NVARCHAR (50)  NULL,
    [PasswordChangedDate] DATETIME       NULL,
    [PasswordChangedBy]   INT            NULL,
    [FirstName]           NVARCHAR (100) NULL,
    [LastName]            NVARCHAR (100) NULL,
    [MiddleName]          NVARCHAR (50)  NULL,
    [IsConfirmed]         BIT            NOT NULL,
    [StatusId]            INT            NOT NULL,
    [UserRoleId]          INT            NOT NULL,
    [RecieveNews]         BIT            NOT NULL,
    [BirthDate]           DATETIME       NOT NULL,
    [Phone]               VARCHAR (50)   NULL,
    [GenderId]            INT            NOT NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_User_Gender] FOREIGN KEY ([GenderId]) REFERENCES [dbo].[Dictionary] ([Id]),
    CONSTRAINT [FK_User_Status] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Dictionary] ([Id]),
    CONSTRAINT [FK_User_UserRole] FOREIGN KEY ([UserRoleId]) REFERENCES [dbo].[Dictionary] ([Id])
);

