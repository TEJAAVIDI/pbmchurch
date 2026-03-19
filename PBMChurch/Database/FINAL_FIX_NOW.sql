-- ============================================
-- FINAL FIX - Your columns exist, just need to finish migration
-- Run this entire script in SSMS
-- ============================================

USE PBM;
GO

PRINT 'Starting final migration steps...';
GO

-- Check if AdminId exists and copy data
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
BEGIN
    PRINT 'Copying AdminId to UserId...';
    
    UPDATE AdminUsers 
    SET UserId = AdminId,
        Password = ISNULL(Password, 'Admin@123'),
        Role = ISNULL(Role, 'Admin')
    WHERE UserId IS NULL OR UserId = 0 OR UserId <> AdminId;
    
    PRINT 'Data copied successfully';
END
GO

-- Make columns NOT NULL
PRINT 'Setting NOT NULL constraints...';
ALTER TABLE AdminUsers ALTER COLUMN UserId INT NOT NULL;
ALTER TABLE AdminUsers ALTER COLUMN Password NVARCHAR(200) NOT NULL;
ALTER TABLE AdminUsers ALTER COLUMN Role NVARCHAR(50) NOT NULL;
PRINT 'Constraints set';
GO

-- Drop old primary key
PRINT 'Dropping old primary key...';
DECLARE @pkName NVARCHAR(200);
SELECT @pkName = name 
FROM sys.key_constraints 
WHERE type = 'PK' AND parent_object_id = OBJECT_ID('AdminUsers');

IF @pkName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE AdminUsers DROP CONSTRAINT ' + @pkName);
    PRINT 'Old primary key dropped: ' + @pkName;
END
GO

-- Add new primary key
PRINT 'Adding new primary key on UserId...';
ALTER TABLE AdminUsers ADD CONSTRAINT PK_AdminUsers PRIMARY KEY (UserId);
PRINT 'New primary key added';
GO

-- Drop old columns
PRINT 'Removing old columns...';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE AdminUsers DROP COLUMN PasswordHash;
    PRINT 'PasswordHash column removed';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
BEGIN
    ALTER TABLE AdminUsers DROP COLUMN AdminId;
    PRINT 'AdminId column removed';
END
GO

-- Verify
PRINT '';
PRINT '========================================';
PRINT 'MIGRATION COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT 'Final columns:';

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AdminUsers'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'Users (all passwords are: Admin@123):';
SELECT UserId, Username, Password, FullName, Email, Role, IsActive
FROM AdminUsers;
GO

PRINT '';
PRINT '========================================';
PRINT 'You can now run your application!';
PRINT 'Use: dotnet run';
PRINT '========================================';
