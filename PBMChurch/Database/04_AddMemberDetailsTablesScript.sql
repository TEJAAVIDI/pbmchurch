-- ============================================
-- Add Member Details Tables (Notes, Prayer Requests, Offerings)
-- ============================================

USE PBM;
GO

PRINT 'Creating MemberNotes table...';

-- Create MemberNotes table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'MemberNotes') AND type in (N'U'))
BEGIN
    CREATE TABLE MemberNotes (
        NoteId INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NOT NULL,
        NoteText NVARCHAR(MAX) NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_MemberNotes_Members FOREIGN KEY (MemberId) 
            REFERENCES Members(MemberId)
    );
    PRINT 'MemberNotes table created successfully';
END
ELSE
BEGIN
    PRINT 'MemberNotes table already exists';
END

GO

PRINT 'Creating PrayerRequests table...';

-- Create PrayerRequests table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'PrayerRequests') AND type in (N'U'))
BEGIN
    CREATE TABLE PrayerRequests (
        RequestId INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NOT NULL,
        Title NVARCHAR(200) NULL,
        Request NVARCHAR(MAX) NOT NULL,
        RequestDate DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        CreatedBy INT NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedBy INT NULL,
        ModifiedDate DATETIME NULL,
        CONSTRAINT FK_PrayerRequests_Members FOREIGN KEY (MemberId) 
            REFERENCES Members(MemberId)
    );
    PRINT 'PrayerRequests table created successfully';
END
ELSE
BEGIN
    PRINT 'PrayerRequests table already exists';
END

GO

PRINT 'Creating MemberOfferings table...';

-- Create MemberOfferings table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'MemberOfferings') AND type in (N'U'))
BEGIN
    CREATE TABLE MemberOfferings (
        OfferingId INT IDENTITY(1,1) PRIMARY KEY,
        MemberId INT NOT NULL,
        Date DATETIME NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Purpose NVARCHAR(500) NULL,
        CreatedBy INT NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_MemberOfferings_Members FOREIGN KEY (MemberId) 
            REFERENCES Members(MemberId)
    );
    PRINT 'MemberOfferings table created successfully';
END
ELSE
BEGIN
    PRINT 'MemberOfferings table already exists';
END

GO

-- Create indexes for better performance
PRINT 'Creating indexes...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MemberNotes_MemberId')
BEGIN
    CREATE INDEX IX_MemberNotes_MemberId ON MemberNotes(MemberId);
    PRINT 'Index IX_MemberNotes_MemberId created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PrayerRequests_MemberId')
BEGIN
    CREATE INDEX IX_PrayerRequests_MemberId ON PrayerRequests(MemberId);
    PRINT 'Index IX_PrayerRequests_MemberId created';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MemberOfferings_MemberId')
BEGIN
    CREATE INDEX IX_MemberOfferings_MemberId ON MemberOfferings(MemberId);
    PRINT 'Index IX_MemberOfferings_MemberId created';
END

GO

PRINT 'Migration completed successfully!';
