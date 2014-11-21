CREATE TABLE [dbo].[Dictionary] (
    [Id]               INT            NOT NULL,
    [VariableTypeID]   INT            NOT NULL,
    [VariableTypeName] NVARCHAR (25)  NOT NULL,
    [VariableValue]    NVARCHAR (512) NULL,
    [IsActive]         BIT            NULL,
    CONSTRAINT [PK_Dictionary] PRIMARY KEY CLUSTERED ([Id] ASC)
);

