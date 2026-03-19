-- ============================================
-- Add ProfileImage column to Members table
-- ============================================

USE PBM;
GO

PRINT 'Adding ProfileImage column to Members table...';

-- Check if column already exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'ProfileImage')
BEGIN
    ALTER TABLE Members ADD ProfileImage NVARCHAR(500) NULL;
    PRINT 'ProfileImage column added successfully';
END
ELSE
BEGIN
    PRINT 'ProfileImage column already exists';
END

GO

PRINT 'Migration completed!';
