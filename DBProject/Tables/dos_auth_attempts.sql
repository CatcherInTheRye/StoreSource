CREATE TABLE [dbo].[dos_auth_attempts] (
    [dos_user_id] INT  NOT NULL,
    [last_access] DATETIME NOT NULL,
    CONSTRAINT [PK_dos_auth_attempts] PRIMARY KEY CLUSTERED ([dos_user_id] ASC)
);

