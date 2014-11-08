CREATE TABLE [dbo].[userActions] (
    [id]             INT IDENTITY (1, 1) NOT NULL,
    [userId]         INT NOT NULL,
    [canRequest]     BIT CONSTRAINT [DF_userActions_canRequest] DEFAULT ((1)) NOT NULL,
    [canApprove]     BIT CONSTRAINT [DF_userActions_canApprove] DEFAULT ((0)) NOT NULL,
    [canViewReports] BIT CONSTRAINT [DF_userActions_canViewReports] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_userActions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_userActions_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id])
);

