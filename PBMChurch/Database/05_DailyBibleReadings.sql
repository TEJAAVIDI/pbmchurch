-- Create DailyBibleReadings Table
CREATE TABLE DailyBibleReadings (
    Id INT IDENTITY PRIMARY KEY,
    ReadingDate DATE UNIQUE,
    DayOfYear INT,
    ReadingRange NVARCHAR(100),
    StartBook NVARCHAR(50),
    StartChapter INT,
    EndBook NVARCHAR(50), 
    EndChapter INT
);

-- Insert sample Bible template
INSERT INTO WishTemplates (Type, MessageText, IsActive, CreatedBy, CreatedDate)
VALUES ('Bible', 'Today''s Bible Reading: {StartChapter}. May God''s word guide and bless you today and forever.', 1, 'System', GETDATE());