-- Create ChurchActivities table
CREATE TABLE ChurchActivities (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    FromDateTime DATETIME2 NOT NULL,
    ToDateTime DATETIME2 NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Location NVARCHAR(300) NOT NULL,
    ImageData VARBINARY(MAX) NULL,
    ImageContentType NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Insert sample data
INSERT INTO ChurchActivities (Title, FromDateTime, ToDateTime, Description, Location, IsActive, CreatedDate)
VALUES 
('Sunday Worship Service', '2025-12-28 09:00:00', '2025-12-28 11:00:00', 'Join us for inspiring worship, powerful sermons, and heartfelt fellowship.', 'Main Church Hall', 1, GETDATE()),
('Bible Study', '2025-12-30 19:00:00', '2025-12-30 20:30:00', 'Deepen your understanding of God''s Word through interactive Bible study sessions.', 'Fellowship Hall', 1, GETDATE()),
('Youth Ministry', '2026-01-03 18:30:00', '2026-01-03 20:00:00', 'Fun, faith-building activities for teens and young adults in a supportive environment.', 'Youth Center', 1, GETDATE()),
('Prayer Meeting', '2026-01-05 06:00:00', '2026-01-05 07:00:00', 'United in prayer, seeking God''s presence and power for our lives and community.', 'Prayer Room', 1, GETDATE()),
('Community Outreach', '2026-01-10 10:00:00', '2026-01-10 15:00:00', 'Serving our community through food drives, charity events, and volunteer programs.', 'Community Center', 1, GETDATE()),
('Special Celebration', '2026-01-15 17:00:00', '2026-01-15 20:00:00', 'Birthdays, anniversaries, baptisms, and special events celebrated together as family.', 'Main Church Hall', 1, GETDATE());

GO