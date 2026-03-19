-- ============================================
-- MANUAL FIX: Complete the Migration Manually
-- Use this if 00_UpdateAdminUsersTable.sql had errors
-- ============================================

USE PBM;
GO

PRINT 'Starting manual migration fix...';
PRINT '';

-- ============================================
-- STEP 1: Verify we have the new columns
-- ============================================
PRINT 'STEP 1: Ensuring all new columns exist...';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
BEGIN
    ALTER TABLE AdminUsers ADD UserId INT NULL;
    PRINT '  Added UserId column';
END
ELSE
BEGIN
    PRINT '  UserId column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Password')
BEGIN
    ALTER TABLE AdminUsers ADD Password NVARCHAR(200) NULL;
    PRINT '  Added Password column';
END
ELSE
BEGIN
    PRINT '  Password column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'Role')
BEGIN
    ALTER TABLE AdminUsers ADD Role NVARCHAR(50) DEFAULT 'Admin';
    PRINT '  Added Role column';
END
ELSE
BEGIN
    PRINT '  Role column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'RefreshToken')
BEGIN
    ALTER TABLE AdminUsers ADD RefreshToken NVARCHAR(500) NULL;
    PRINT '  Added RefreshToken column';
END
ELSE
BEGIN
    PRINT '  RefreshToken column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'RefreshTokenExpiry')
BEGIN
    ALTER TABLE AdminUsers ADD RefreshTokenExpiry DATETIME2 NULL;
    PRINT '  Added RefreshTokenExpiry column';
END
ELSE
BEGIN
    PRINT '  RefreshTokenExpiry column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'ResetOtp')
BEGIN
    ALTER TABLE AdminUsers ADD ResetOtp NVARCHAR(10) NULL;
    PRINT '  Added ResetOtp column';
END
ELSE
BEGIN
    PRINT '  ResetOtp column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'OtpExpiry')
BEGIN
    ALTER TABLE AdminUsers ADD OtpExpiry DATETIME2 NULL;
    PRINT '  Added OtpExpiry column';
END
ELSE
BEGIN
    PRINT '  OtpExpiry column already exists';
END

PRINT '';
GO

-- ============================================
-- STEP 2: Copy data from old columns to new
-- ============================================
PRINT 'STEP 2: Migrating data from old columns to new...';

-- Only migrate if old columns exist and new ones are empty
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'UserId')
BEGIN
    UPDATE AdminUsers 
    SET UserId = AdminId,
        Password = 'Admin@123',
        Role = 'Admin'
    WHERE UserId IS NULL OR UserId = 0;
    
    PRINT '  Migrated AdminId -> UserId';
    PRINT '  Set default Password = Admin@123';
    PRINT '  Set default Role = Admin';
END
ELSE
BEGIN
    PRINT '  Data already migrated or no old columns found';
END

PRINT '';
GO

-- ============================================
-- STEP 3: Set columns as NOT NULL
-- ============================================
PRINT 'STEP 3: Setting required columns as NOT NULL...';

BEGIN TRY
    ALTER TABLE AdminUsers ALTER COLUMN UserId INT NOT NULL;
    PRINT '  UserId set to NOT NULL';
END TRY
BEGIN CATCH
    PRINT '  UserId already NOT NULL or has issues';
END CATCH

BEGIN TRY
    ALTER TABLE AdminUsers ALTER COLUMN Password NVARCHAR(200) NOT NULL;
    PRINT '  Password set to NOT NULL';
END TRY
BEGIN CATCH
    PRINT '  Password already NOT NULL or has issues';
END CATCH

BEGIN TRY
    ALTER TABLE AdminUsers ALTER COLUMN Role NVARCHAR(50) NOT NULL;
    PRINT '  Role set to NOT NULL';
END TRY
BEGIN CATCH
    PRINT '  Role already NOT NULL or has issues';
END CATCH

PRINT '';
GO

-- ============================================
-- STEP 4: Fix Primary Key
-- ============================================
PRINT 'STEP 4: Fixing primary key...';

-- Drop old primary key if exists
DECLARE @pkName NVARCHAR(200);
SELECT @pkName = name 
FROM sys.key_constraints 
WHERE type = 'PK' AND parent_object_id = OBJECT_ID('AdminUsers');

IF @pkName IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC('ALTER TABLE AdminUsers DROP CONSTRAINT ' + @pkName);
        PRINT '  Dropped old primary key: ' + @pkName;
    END TRY
    BEGIN CATCH
        PRINT '  Could not drop old primary key (might be in use)';
    END CATCH
END

-- Add new primary key on UserId
BEGIN TRY
    ALTER TABLE AdminUsers ADD CONSTRAINT PK_AdminUsers PRIMARY KEY (UserId);
    PRINT '  Added new primary key on UserId';
END TRY
BEGIN CATCH
    PRINT '  Primary key already exists on UserId or could not be created';
END CATCH

PRINT '';
GO

-- ============================================
-- STEP 5: Remove old columns
-- ============================================
PRINT 'STEP 5: Removing old columns...';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'PasswordHash')
BEGIN
    BEGIN TRY
        ALTER TABLE AdminUsers DROP COLUMN PasswordHash;
        PRINT '  Dropped PasswordHash column';
    END TRY
    BEGIN CATCH
        PRINT '  Could not drop PasswordHash column';
    END CATCH
END
ELSE
BEGIN
    PRINT '  PasswordHash column already removed';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AdminUsers') AND name = 'AdminId')
BEGIN
    BEGIN TRY
        ALTER TABLE AdminUsers DROP COLUMN AdminId;
        PRINT '  Dropped AdminId column';
    END TRY
    BEGIN CATCH
        PRINT '  Could not drop AdminId column (might be primary key - run STEP 4 first)';
    END CATCH
END
ELSE
BEGIN
    PRINT '  AdminId column already removed';
END

PRINT '';
GO

-- ============================================
-- STEP 6: Verify final structure
-- ============================================
PRINT '======================================';
PRINT 'FINAL TABLE STRUCTURE:';
PRINT '======================================';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AdminUsers'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '======================================';
PRINT 'CURRENT USERS:';
PRINT '======================================';

SELECT UserId, Username, Password, FullName, Email, Role, IsActive
FROM AdminUsers;
GO

PRINT '';
PRINT '======================================';
PRINT 'Migration completed!';
PRINT 'All users have password: Admin@123';
PRINT 'You can now run: dotnet run';
PRINT '======================================';
GO
