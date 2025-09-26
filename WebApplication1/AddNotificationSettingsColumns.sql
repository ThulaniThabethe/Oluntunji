-- Add notification settings columns to Users table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'OrderUpdates')
BEGIN
    ALTER TABLE dbo.Users ADD OrderUpdates BIT NOT NULL DEFAULT 1
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'BookAlerts')
BEGIN
    ALTER TABLE dbo.Users ADD BookAlerts BIT NOT NULL DEFAULT 1
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'AccountUpdates')
BEGIN
    ALTER TABLE dbo.Users ADD AccountUpdates BIT NOT NULL DEFAULT 1
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'MarketingEmails')
BEGIN
    ALTER TABLE dbo.Users ADD MarketingEmails BIT NOT NULL DEFAULT 0
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'LowStockAlerts')
BEGIN
    ALTER TABLE dbo.Users ADD LowStockAlerts BIT NOT NULL DEFAULT 1
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'PriceDropAlerts')
BEGIN
    ALTER TABLE dbo.Users ADD PriceDropAlerts BIT NOT NULL DEFAULT 1
END

PRINT 'Notification settings columns added successfully to Users table'