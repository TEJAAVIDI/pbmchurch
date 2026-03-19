-- ============================================
-- Create NotificationReads Table
-- ============================================

USE PBM;
GO

-- Drop table if exists
IF OBJECT_ID('NotificationReads', 'U') IS NOT NULL DROP TABLE NotificationReads;
GO

-- Create NotificationReads table
CREATE TABLE NotificationReads (
    ReadId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NULL, -- Nullable for PrayerRequests
    RequestId INT NULL, -- For PrayerRequest notifications
    NotificationType NVARCHAR(50) NOT NULL, -- Birthday, Anniversary, FamilyBirthday, FamilyAnniversary, PrayerRequest
    ReadDate DATE NOT NULL,
    AdminId INT NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_NotificationReads_Members FOREIGN KEY (MemberId) REFERENCES Members(MemberId),
    CONSTRAINT FK_NotificationReads_Admin FOREIGN KEY (AdminId) REFERENCES AdminUsers(UserId)
);
GO

-- Create index for faster queries
CREATE NONCLUSTERED INDEX IX_NotificationReads_Member_Type_Date 
ON NotificationReads(MemberId, NotificationType, ReadDate);
GO

CREATE NONCLUSTERED INDEX IX_NotificationReads_Request_Type_Date 
ON NotificationReads(RequestId, NotificationType, ReadDate);
GO

PRINT 'NotificationReads table created successfully!';
GO
