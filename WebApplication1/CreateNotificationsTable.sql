-- Create Notifications table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [NotificationId] INT IDENTITY (1, 1) NOT NULL,
        [UserId] INT NOT NULL,
        [Title] NVARCHAR (200) NOT NULL,
        [Message] NVARCHAR (1000) NOT NULL,
        [NotificationType] NVARCHAR (50) NOT NULL,
        [Priority] NVARCHAR (20) NULL,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [RelatedLink] NVARCHAR (500) NULL,
        CONSTRAINT [PK_dbo.Notifications] PRIMARY KEY CLUSTERED ([NotificationId] ASC),
        CONSTRAINT [FK_dbo.Notifications_dbo.Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE
    );

    -- Create index for better performance
    CREATE NONCLUSTERED INDEX [IX_UserId_IsRead]
        ON [dbo].[Notifications]([UserId] ASC, [IsRead] ASC);

    CREATE NONCLUSTERED INDEX [IX_CreatedDate]
        ON [dbo].[Notifications]([CreatedDate] DESC);
END
GO