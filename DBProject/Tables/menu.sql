CREATE TABLE [dbo].[menu] (
    [ID]       INT           IDENTITY (1, 1) NOT NULL,
    [Text]     VARCHAR (100) NULL,
    [href]     VARCHAR (50)  NOT NULL,
    [title]    VARCHAR (50)  NOT NULL,
    [level]    INT           NOT NULL,
    [roleArea] INT           NOT NULL,
    [oreder]   INT           NULL,
    CONSTRAINT [PK_menu] PRIMARY KEY CLUSTERED ([ID] ASC)
);



