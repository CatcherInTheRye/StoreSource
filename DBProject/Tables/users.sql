CREATE TABLE [dbo].[users] (
    [id]             INT          IDENTITY (1, 1) NOT NULL,
    [email]          VARCHAR (60) NOT NULL,
    [firstName]      VARCHAR (40) NOT NULL,
    [lastName]       VARCHAR (40) NOT NULL,
    [middleName]     VARCHAR (40) NULL,
    [userName]       VARCHAR (40) NOT NULL,
    [salutation]     VARCHAR (10) NULL,
    [pwd]            VARCHAR (50) NOT NULL,
    [pwdChangedDate] DATETIME     NOT NULL,
    [pwdChangedBy]   INT          NULL,
    [active]         BIT          NOT NULL,
    [applyDate]      DATETIME     NULL,
    [userImageFile]  VARCHAR (40) NULL,
    [usertype]       CHAR (1)     NULL,
    [ps_user_id]     INT          NULL,
    [phone] VARCHAR(50) NULL, 
    [cell] VARCHAR(50) NULL, 
    CONSTRAINT [PKusers] PRIMARY KEY CLUSTERED ([id] ASC)
);

