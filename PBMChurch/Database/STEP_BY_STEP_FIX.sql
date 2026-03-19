-- ============================================
-- STEP BY STEP FIX - Run each section separately
-- Copy and paste ONE section at a time into SSMS
-- ============================================

USE PBM;
GO

-- ============================================
-- SECTION 1: Check what we have
-- Run this first to see current state
-- ============================================
PRINT '=== CURRENT COLUMNS ===';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AdminUsers'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '=== PRIMARY KEY ===';
SELECT name as PrimaryKeyName
FROM sys.key_constraints 
WHERE type = 'PK' AND parent_object_id = OBJECT_ID('AdminUsers');
GO

-- ============================================
-- SECTION 2: Populate UserId from AdminId
-- Run this only if UserId column exists and is empty
-- ============================================
/*
UPDATE AdminUsers 
SET UserId = AdminId,
    Password = 'Admin@123',
    Role = 'Admin'
WHERE UserId IS NULL OR UserId = 0;

PRINT 'Data migrated to new columns';
GO
*/

-- ============================================
-- SECTION 3: Make new columns NOT NULL
-- Run this only after Section 2 completes
-- ============================================
/*
ALTER TABLE AdminUsers ALTER COLUMN UserId INT NOT NULL;
ALTER TABLE AdminUsers ALTER COLUMN Password NVARCHAR(200) NOT NULL;
ALTER TABLE AdminUsers ALTER COLUMN Role NVARCHAR(50) NOT NULL;
PRINT 'Columns set to NOT NULL';
GO
*/

-- ============================================
-- SECTION 4: Drop old primary key
-- Run this after Section 3
-- Replace 'PK_AdminUsers' with actual name from Section 1
-- ============================================
/*
-- Check the primary key name from Section 1 output
-- Then replace PK_AdminUsers below with the actual name

ALTER TABLE AdminUsers DROP CONSTRAINT PK_AdminUsers;
PRINT 'Old primary key dropped';
GO
*/

-- ============================================
-- SECTION 5: Add new primary key on UserId
-- Run this after Section 4
-- ============================================
/*
ALTER TABLE AdminUsers ADD CONSTRAINT PK_AdminUsers PRIMARY KEY (UserId);
PRINT 'New primary key added on UserId';
GO
*/

-- ============================================
-- SECTION 6: Drop old columns
-- Run this after Section 5
-- ============================================
/*
ALTER TABLE AdminUsers DROP COLUMN PasswordHash;
ALTER TABLE AdminUsers DROP COLUMN AdminId;
PRINT 'Old columns removed';
GO
*/

-- ============================================
-- SECTION 7: Verify final structure
-- Run this last to confirm everything worked
-- ============================================
/*
PRINT '=== FINAL STRUCTURE ===';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AdminUsers'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '=== USERS ===';
SELECT UserId, Username, Password, FullName, Email, Role, IsActive
FROM AdminUsers;
GO
*/
