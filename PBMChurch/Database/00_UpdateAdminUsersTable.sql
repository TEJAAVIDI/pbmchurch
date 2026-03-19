-- ============================================
-- Migration Script: Update AdminUsers Table
-- This script updates the existing AdminUsers table structure
-- Run this BEFORE running the other scripts if database already exists
-- ============================================

USE PBM;
GO

-- Check if we need to update the table structure
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
BEGIN
    PRINT 'Migrating AdminUsers table structure...';
    
    -- Step 1: Add new columns if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
    BEGIN
        ALTER TABLE AdminUsers ADD UserId INT NULL;
        PRINT 'Added UserId column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Password')
    BEGIN
        ALTER TABLE AdminUsers ADD Password NVARCHAR(200) NULL;
        PRINT 'Added Password column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Role')
    BEGIN
        ALTER TABLE AdminUsers ADD Role NVARCHAR(50) NULL DEFAULT 'Admin';
        PRINT 'Added Role column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'RefreshToken')
    BEGIN
        ALTER TABLE AdminUsers ADD RefreshToken NVARCHAR(500) NULL;
        PRINT 'Added RefreshToken column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'RefreshTokenExpiry')
    BEGIN
        ALTER TABLE AdminUsers ADD RefreshTokenExpiry DATETIME2 NULL;
        PRINT 'Added RefreshTokenExpiry column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'ResetOtp')
    BEGIN
        ALTER TABLE AdminUsers ADD ResetOtp NVARCHAR(10) NULL;
        PRINT 'Added ResetOtp column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'OtpExpiry')
    BEGIN
        ALTER TABLE AdminUsers ADD OtpExpiry DATETIME2 NULL;
        PRINT 'Added OtpExpiry column';
    END

    -- Step 2: Migrate data from old columns to new columns
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
       AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
    BEGIN
        UPDATE AdminUsers 
        SET UserId = AdminId,
            Password = 'Admin@123', -- Set default password (you can change this)
            Role = 'Admin'
        WHERE UserId IS NULL;
        
        PRINT 'Migrated existing data';
    END

    -- Step 3: Make new columns NOT NULL where needed
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
    BEGIN
        ALTER TABLE AdminUsers ALTER COLUMN UserId INT NOT NULL;
        PRINT 'Set UserId as NOT NULL';
    END
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Password')
    BEGIN
        ALTER TABLE AdminUsers ALTER COLUMN Password NVARCHAR(200) NOT NULL;
        PRINT 'Set Password as NOT NULL';
    END
    
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Role')
    BEGIN
        ALTER TABLE AdminUsers ALTER COLUMN Role NVARCHAR(50) NOT NULL;
        PRINT 'Set Role as NOT NULL';
    END
    
    PRINT 'Updated column constraints';

    -- Step 4: Drop old primary key constraint
    DECLARE @constraintName NVARCHAR(200);
    SELECT @constraintName = name 
    FROM sys.key_constraints 
    WHERE type = 'PK' AND parent_object_id = OBJECT_ID('AdminUsers');
    
    IF @constraintName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE AdminUsers DROP CONSTRAINT ' + @constraintName);
        PRINT 'Dropped old primary key constraint: ' + @constraintName;
    END

    -- Step 5: Add new primary key on UserId
    ALTER TABLE AdminUsers ADD CONSTRAINT PK_AdminUsers PRIMARY KEY (UserId);
    PRINT 'Added new primary key on UserId';

    -- Step 6: Drop old columns (PasswordHash, AdminId)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'PasswordHash')
    BEGIN
        ALTER TABLE AdminUsers DROP COLUMN PasswordHash;
        PRINT 'Dropped PasswordHash column';
    END

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
    BEGIN
        ALTER TABLE AdminUsers DROP COLUMN AdminId;
        PRINT 'Dropped AdminId column';
    END

    PRINT 'AdminUsers table migration completed successfully!';
    PRINT '---';
    PRINT 'IMPORTANT: All users now have password: Admin@123';
    PRINT 'Please change passwords after login.';
END
ELSE
BEGIN
    PRINT 'AdminUsers table is already in the new format or does not exist.';
    PRINT 'If table does not exist, run 01_CreateTables.sql instead.';
END
GO

-- Display final table structure
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminUsers')
BEGIN
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'AdminUsers'
    ORDER BY ORDINAL_POSITION;
END
GO

-- Display current users (only if columns exist)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Password')
BEGIN
    SELECT UserId, Username, Password, FullName, Email, Role, IsActive
    FROM AdminUsers;
END
ELSE
BEGIN
    PRINT 'Note: New columns not fully migrated yet. Please re-run this script.';
END
GO

PRINT '';
PRINT '======================================';
PRINT 'Migration script completed!';
PRINT '======================================';
GO
