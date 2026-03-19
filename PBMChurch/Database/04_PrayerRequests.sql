-- Alter existing PrayerRequests table to match requirements
ALTER TABLE [dbo].[PrayerRequests] ADD [Name] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[PrayerRequests] ADD [Phone] NVARCHAR(15) NULL;
ALTER TABLE [dbo].[PrayerRequests] ADD [ChurchId] INT NULL;
ALTER TABLE [dbo].[PrayerRequests] ADD [AnsweredDate] DATETIME NULL;
ALTER TABLE [dbo].[PrayerRequests] ADD [Notes] NVARCHAR(500) NULL;

-- Add foreign key constraint for ChurchId
ALTER TABLE [dbo].[PrayerRequests] ADD CONSTRAINT FK_PrayerRequests_Churches 
FOREIGN KEY (ChurchId) REFERENCES Churches(ChurchId);

-- Update Status values to match new system
UPDATE [dbo].[PrayerRequests] SET Status = 'Active' WHERE Status = 'Pending';

-- Add check constraint for Status
ALTER TABLE [dbo].[PrayerRequests] ADD CONSTRAINT CK_PrayerRequests_Status 
CHECK (Status IN ('Active', 'Answered', 'Closed', 'Pending'));

-- Make MemberId nullable for non-member requests
ALTER TABLE [dbo].[PrayerRequests] ALTER COLUMN MemberId INT NULL;

-- Create indexes for better performance
CREATE INDEX IX_PrayerRequests_ChurchId ON PrayerRequests(ChurchId);
CREATE INDEX IX_PrayerRequests_Status ON PrayerRequests(Status);
CREATE INDEX IX_PrayerRequests_RequestDate ON PrayerRequests(RequestDate);