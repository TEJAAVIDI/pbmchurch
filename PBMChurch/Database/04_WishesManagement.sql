-- Add WishTemplates table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WishTemplates' AND xtype='U')
BEGIN
    CREATE TABLE WishTemplates (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Type NVARCHAR(50) NOT NULL,
        ImageData VARBINARY(MAX) NULL,
        ImageFileName NVARCHAR(255) NULL,
        ImageContentType NVARCHAR(100) NULL,
        MessageText NVARCHAR(MAX) NULL,
        YouTubeUrl NVARCHAR(500) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedDate DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL
    );
    
    -- Add check constraint for Type
    ALTER TABLE WishTemplates 
    ADD CONSTRAINT CK_WishTemplates_Type 
    CHECK (Type IN ('Birthday', 'Anniversary', 'Event', 'Bible', 'General'));
    
    PRINT 'WishTemplates table created successfully';
END
ELSE
BEGIN
    PRINT 'WishTemplates table already exists';
END

-- Add BibleReadings table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BibleReadings' AND xtype='U')
BEGIN
    CREATE TABLE BibleReadings (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TotalChapters INT NOT NULL DEFAULT 1189,
        CurrentDay INT NOT NULL,
        ChaptersPerDay DECIMAL(5,2) NOT NULL,
        StartChapter INT NOT NULL,
        EndChapter INT NOT NULL,
        ReadingDate DATE NOT NULL,
        Year INT NOT NULL,
        IsCompleted BIT NOT NULL DEFAULT 0
    );
    
    -- Add unique constraint for date
    CREATE UNIQUE INDEX IX_BibleReadings_ReadingDate ON BibleReadings(ReadingDate);
    
    PRINT 'BibleReadings table created successfully';
END
ELSE
BEGIN
    PRINT 'BibleReadings table already exists';
END

-- Insert sample wish templates
IF NOT EXISTS (SELECT * FROM WishTemplates WHERE Type = 'Birthday')
BEGIN
    INSERT INTO WishTemplates (Type, MessageText, IsActive, CreatedBy, CreatedDate)
    VALUES 
    ('Birthday', 'Happy Birthday {Name}! May God bless you abundantly on your special day. 🎂🎉', 1, 'System', GETDATE()),
    ('Anniversary', 'Happy Anniversary {Name}! May God continue to bless your marriage with love, joy, and happiness. 💕', 1, 'System', GETDATE()),
    ('Event', 'Dear {Name}, You are cordially invited to our special church event. Please join us for fellowship and worship.', 1, 'System', GETDATE()),
    ('Bible', 'Today''s Bible Reading: Chapters {StartChapter} to {EndChapter}. May God''s word guide and bless you today.', 1, 'System', GETDATE()),
    ('General', 'Hello {Name}, Greetings from PBM Church! May God bless you and your family.', 1, 'System', GETDATE());
    
    PRINT 'Sample wish templates inserted successfully';
END
ELSE
BEGIN
    PRINT 'Sample wish templates already exist';
END

-- Create uploads directory structure (this would be done by the application)
PRINT 'Remember to create the following directories in wwwroot:';
PRINT '- wwwroot/uploads/wishes/';
PRINT 'Database setup for Wishes Management completed successfully!';