CREATE TABLE [dbo].[userOrganization] (
    [userId]         INT NOT NULL,
    [organizationId] INT NOT NULL,
    [schoolId]       INT NULL,
    CONSTRAINT [PK_userOrganization] PRIMARY KEY CLUSTERED ([userId] ASC, [organizationId] ASC),
    CONSTRAINT [FK_userOrganization_organizations] FOREIGN KEY ([organizationId]) REFERENCES [dbo].[organizations] ([id]),
    CONSTRAINT [FK_userOrganization_schools] FOREIGN KEY ([schoolId]) REFERENCES [dbo].[schools] ([id]),
    CONSTRAINT [FK_userOrganization_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id])
);




GO
CREATE NONCLUSTERED INDEX [IX_userOrganization]
    ON [dbo].[userOrganization]([userId] ASC);

