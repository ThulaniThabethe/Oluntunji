-- Create SavedCards table
CREATE TABLE [dbo].[SavedCards] (
    [CardId] INT IDENTITY(1,1) NOT NULL,
    [UserId] INT NOT NULL,
    [CardholderName] NVARCHAR(100) NOT NULL,
    [LastFourDigits] NVARCHAR(4) NOT NULL,
    [CardType] NVARCHAR(50) NOT NULL,
    [ExpiryDate] DATETIME NOT NULL,
    [IsDefault] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DateAdded] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_SavedCards] PRIMARY KEY CLUSTERED ([CardId] ASC),
    CONSTRAINT [FK_SavedCards_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE
);

-- Create index for better performance
CREATE NONCLUSTERED INDEX [IX_SavedCards_UserId] ON [dbo].[SavedCards] ([UserId]);
CREATE NONCLUSTERED INDEX [IX_SavedCards_IsActive] ON [dbo].[SavedCards] ([IsActive]);