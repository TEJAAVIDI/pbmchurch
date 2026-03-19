-- SQL Script to create Landing Page Content and Gallery Images tables
-- Run this script in your database

-- Create LandingPageContents table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LandingPageContents')
BEGIN
    CREATE TABLE LandingPageContents (
        ContentId INT IDENTITY(1,1) PRIMARY KEY,
        SectionName NVARCHAR(200) NOT NULL,
        Title NVARCHAR(500) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        ImagePath NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL
    );

    PRINT 'LandingPageContents table created successfully.';
END
ELSE
BEGIN
    PRINT 'LandingPageContents table already exists.';
END
GO

-- Create GalleryImages table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GalleryImages')
BEGIN
    CREATE TABLE GalleryImages (
        ImageId INT IDENTITY(1,1) PRIMARY KEY,
        ImagePath NVARCHAR(500) NOT NULL,
        Title NVARCHAR(200) NULL,
        Description NVARCHAR(500) NULL,
        ChurchId INT NULL,
        Category NVARCHAR(100) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        UploadedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_GalleryImages_Churches FOREIGN KEY (ChurchId) REFERENCES Churches(ChurchId)
    );

    PRINT 'GalleryImages table created successfully.';
END
ELSE
BEGIN
    PRINT 'GalleryImages table already exists.';
END
GO

-- Insert sample landing page content
INSERT INTO LandingPageContents (SectionName, Title, Content, DisplayOrder, IsActive)
VALUES 
('Hero', 'Welcome to PBM Church', 'Building a Community of Faith, Hope, and Love', 1, 1),
('About', 'Our Mission', 'PBM Church is committed to spreading the Gospel of Jesus Christ and nurturing spiritual growth within our communities. We believe in creating a welcoming environment where everyone can experience God''s love, develop their faith, and serve others with compassion.', 2, 1),
('Vision', 'Our Vision', 'To be a light in our communities, transforming lives through the power of the Gospel and demonstrating Christ''s love in practical ways.', 3, 1);

PRINT 'Sample content inserted successfully.';
GO

-- Insert sample gallery images (using placeholder URLs - replace with actual paths)
-- Note: You will need to upload actual images to wwwroot/images/gallery/ folder
-- and update these paths accordingly

PRINT 'Tables created successfully!';
PRINT '';
PRINT '=================================================================';
PRINT 'IMPORTANT INSTRUCTIONS FOR IMAGE UPLOAD:';
PRINT '=================================================================';
PRINT '';
PRINT '1. CREATE GALLERY FOLDER:';
PRINT '   Create a folder: wwwroot/images/gallery/';
PRINT '';
PRINT '2. UPLOAD YOUR CHURCH IMAGES:';
PRINT '   - Upload church service photos';
PRINT '   - Upload worship event photos';
PRINT '   - Upload community gathering photos';
PRINT '   - Upload youth ministry photos';
PRINT '';
PRINT '3. INSERT GALLERY IMAGES:';
PRINT '   After uploading images, run this query:';
PRINT '';
PRINT '   INSERT INTO GalleryImages (ImagePath, Title, Description, ChurchId, Category, DisplayOrder, IsActive)';
PRINT '   VALUES ';
PRINT '   (''/images/gallery/service1.jpg'', ''Sunday Worship'', ''Our vibrant Sunday service'', 1, ''Service'', 1, 1),';
PRINT '   (''/images/gallery/worship1.jpg'', ''Praise & Worship'', ''Worshipping together'', 1, ''Worship'', 2, 1),';
PRINT '   (''/images/gallery/community1.jpg'', ''Community Gathering'', ''Fellowship time'', 1, ''Community'', 3, 1);';
PRINT '';
PRINT '4. SUGGESTED IMAGE CATEGORIES:';
PRINT '   - Service: Sunday service photos';
PRINT '   - Worship: Praise and worship moments';
PRINT '   - Community: Fellowship and gatherings';
PRINT '   - Event: Special events and celebrations';
PRINT '   - Youth: Youth ministry activities';
PRINT '   - Outreach: Community outreach programs';
PRINT '';
PRINT '5. IMAGE SPECIFICATIONS:';
PRINT '   - Format: JPG, PNG';
PRINT '   - Recommended size: 1200x800 pixels (landscape)';
PRINT '   - Max file size: 5MB';
PRINT '   - Aspect ratio: 3:2 or 16:9';
PRINT '';
PRINT '=================================================================';
GO
