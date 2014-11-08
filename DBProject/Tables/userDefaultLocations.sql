CREATE TABLE [dbo].[userDefaultLocations] (
    [id]             INT IDENTITY (1, 1) NOT NULL,
    [userId]         INT NOT NULL,
    [organizationId] INT NOT NULL,
    [schoolId]       INT NULL,
    CONSTRAINT [PK_userDefaultLocations] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_userDefaultLocations_organizations] FOREIGN KEY ([organizationId]) REFERENCES [dbo].[organizations] ([id]),
    CONSTRAINT [FK_userDefaultLocations_schools] FOREIGN KEY ([schoolId]) REFERENCES [dbo].[schools] ([id]),
    CONSTRAINT [FK_userDefaultLocations_users] FOREIGN KEY ([userId]) REFERENCES [dbo].[users] ([id])
);



