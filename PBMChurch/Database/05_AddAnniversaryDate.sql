-- Add AnniversaryDate column to Members table
ALTER TABLE [dbo].[Members] ADD [AnniversaryDate] DATETIME NULL;

-- Add Gender and DateOfBirth to FamilyMembers table if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FamilyMembers' AND COLUMN_NAME = 'Gender')
BEGIN
    ALTER TABLE [dbo].[FamilyMembers] ADD [Gender] NVARCHAR(10) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FamilyMembers' AND COLUMN_NAME = 'DateOfBirth')
BEGIN
    ALTER TABLE [dbo].[FamilyMembers] ADD [DateOfBirth] DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FamilyMembers' AND COLUMN_NAME = 'AnniversaryDate')
BEGIN
    ALTER TABLE [dbo].[FamilyMembers] ADD [AnniversaryDate] DATETIME NULL;
END

-- Add MessageSent column to BirthdayWishHistory if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BirthdayWishHistory' AND COLUMN_NAME = 'MessageSent')
BEGIN
    ALTER TABLE [dbo].[BirthdayWishHistory] ADD [MessageSent] NVARCHAR(500) NULL;
END

-- Add Anniversary Wish History table
CREATE TABLE AnniversaryWishHistory (
    WishId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    WishSentDate DATE NOT NULL,
    MessageSent NVARCHAR(500) NULL,
    Status NVARCHAR(20) DEFAULT 'Sent',
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_AnniversaryWishHistory_Members FOREIGN KEY (MemberId) REFERENCES Members(MemberId),
    CONSTRAINT UQ_AnniversaryWishHistory_Member_Date UNIQUE (MemberId, WishSentDate)
);

-- Add Anniversary automation settings
INSERT INTO AutomationSettings (SettingKey, SettingValue, Description) VALUES
('Anniversary_Wish_Enabled', 'true', 'Enable/Disable anniversary wish automation'),
('Anniversary_Wish_Time', '02:42', 'Time when anniversary wishes should be sent (24-hour format)'),
('Anniversary_Wish_Message', 'Happy Anniversary {Name}! May God bless your marriage with many more years of love, joy, and happiness. Wishing you both a wonderful anniversary celebration!', 'Anniversary wish message template');