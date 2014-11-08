CREATE TABLE [dbo].[login] (
    [id]              BIGINT     IDENTITY (1, 1)  NOT NULL,
    [userId]          INT          NOT NULL,
    [ipAddress]       VARCHAR (20) NOT NULL,
    [loginTime]       DATETIME     NULL,
    [loginActiveTime] DATETIME     NULL,
    [logoutTime]      DATETIME     NULL,
    [service]         BIT          CONSTRAINT [DF_login_service] DEFAULT ((0)) NOT NULL,
    [origUserId]      INT          NULL,
    CONSTRAINT [PKlogin] PRIMARY KEY CLUSTERED ([id] ASC)
);

