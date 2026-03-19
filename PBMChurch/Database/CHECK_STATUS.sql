-- ============================================
-- Verification and Fix Script
-- Run this to check current state and fix any issues
-- ============================================

USE PBM;
GO

PRINT '======================================';
PRINT 'STEP 1: Checking Current Table State';
PRINT '======================================';
PRINT '';

-- Check which columns currently exist
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AdminUsers'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '======================================';
PRINT 'STEP 2: Checking for Issues';
PRINT '======================================';
PRINT '';

-- Check if old structure still exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
BEGIN
    PRINT '❌ OLD STRUCTURE FOUND: AdminId column still exists';
END
ELSE
BEGIN
    PRINT '✓ AdminId column removed';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'PasswordHash')
BEGIN
    PRINT '❌ OLD STRUCTURE FOUND: PasswordHash column still exists';
END
ELSE
BEGIN
    PRINT '✓ PasswordHash column removed';
END

-- Check if new structure exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
BEGIN
    PRINT '✓ NEW STRUCTURE: UserId column exists';
END
ELSE
BEGIN
    PRINT '❌ MISSING: UserId column does not exist';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Password')
BEGIN
    PRINT '✓ NEW STRUCTURE: Password column exists';
END
ELSE
BEGIN
    PRINT '❌ MISSING: Password column does not exist';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Role')
BEGIN
    PRINT '✓ NEW STRUCTURE: Role column exists';
END
ELSE
BEGIN
    PRINT '❌ MISSING: Role column does not exist';
END

PRINT '';
PRINT '======================================';
PRINT 'STEP 3: Current Data (if readable)';
PRINT '======================================';
PRINT '';

-- Try to read data with old structure
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
BEGIN
    PRINT 'Reading with OLD structure:';
    SELECT AdminId, Username, FullName, Email, IsActive FROM AdminUsers;
END

-- Try to read data with new structure
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Password')
BEGIN
    PRINT 'Reading with NEW structure:';
    SELECT UserId, Username, Password, FullName, Email, Role, IsActive FROM AdminUsers;
END

PRINT '';
PRINT '======================================';
PRINT 'STEP 4: Recommendations';
PRINT '======================================';
PRINT '';

-- Determine what needs to be done
DECLARE @hasOldStructure BIT = 0;
DECLARE @hasNewStructure BIT = 0;
DECLARE @needsReMigration BIT = 0;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
   OR EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'PasswordHash')
BEGIN
    SET @hasOldStructure = 1;
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Password')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Role')
BEGIN
    SET @hasNewStructure = 1;
END

IF @hasOldStructure = 1 AND @hasNewStructure = 1
BEGIN
    SET @needsReMigration = 1;
END

IF @needsReMigration = 1
BEGIN
    PRINT '⚠️  MIXED STATE DETECTED!';
    PRINT 'Both old and new columns exist. Migration partially completed.';
    PRINT '';
    PRINT 'SOLUTION: Re-run the migration script:';
    PRINT '  Database/00_UpdateAdminUsersTable.sql';
    PRINT '';
END
ELSE IF @hasOldStructure = 1 AND @hasNewStructure = 0
BEGIN
    PRINT '⚠️  OLD STRUCTURE DETECTED!';
    PRINT 'Table still has old column names.';
    PRINT '';
    PRINT 'SOLUTION: Run the migration script:';
    PRINT '  Database/00_UpdateAdminUsersTable.sql';
    PRINT '';
END
ELSE IF @hasOldStructure = 0 AND @hasNewStructure = 1
BEGIN
    PRINT '✓ SUCCESS! Table has new structure.';
    PRINT '';
    PRINT 'You can now run your application:';
    PRINT '  dotnet run';
    PRINT '';
    PRINT 'Login with: admin / Admin@123';
    PRINT '';
END
ELSE
BEGIN
    PRINT '❌ TABLE NOT FOUND OR CORRUPT!';
    PRINT '';
    PRINT 'SOLUTION: Run fresh setup:';
    PRINT '  1. Database/01_CreateTables.sql';
    PRINT '  2. Database/02_InsertSeedData.sql';
    PRINT '  3. Database/03_CreateProceduresAndViews.sql';
    PRINT '';
END

PRINT '======================================';
GO
