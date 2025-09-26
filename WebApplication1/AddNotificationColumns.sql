-- Add notification columns to Users table
ALTER TABLE Users ADD EmailNotifications BIT NOT NULL DEFAULT 1;
ALTER TABLE Users ADD SmsNotifications BIT NOT NULL DEFAULT 0;
ALTER TABLE Users ADD PushNotifications BIT NOT NULL DEFAULT 1;