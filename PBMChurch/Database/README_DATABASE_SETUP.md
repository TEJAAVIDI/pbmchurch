# Database Setup Instructions

## Quick Start Guide

Follow these steps to set up your database:

---

## Option 1: Fresh Database Setup (Recommended if starting new)

If you're creating the database from scratch:

### Step 1: Create Database
```sql
CREATE DATABASE PBM;
GO
```

### Step 2: Execute Scripts in Order
Execute the following scripts in **SQL Server Management Studio** in this exact order:

1. **01_CreateTables.sql** - Creates all tables with new structure
2. **02_InsertSeedData.sql** - Inserts sample data with 3 users
3. **03_CreateProceduresAndViews.sql** - Creates stored procedures and views

---

## Option 2: Update Existing Database

If you already have a PBM database with old AdminUsers table structure:

### Step 1: Run Migration Script First
Execute this script to update the existing AdminUsers table:
- **00_UpdateAdminUsersTable.sql** - Migrates old table to new structure

⚠️ **IMPORTANT**: This will set all existing users' passwords to `Admin@123`

### Step 2: Verify Migration
After running the migration script, verify the table structure:

```sql
-- Check table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AdminUsers'
ORDER BY ORDINAL_POSITION;

-- Check existing users
SELECT UserId, Username, Password, FullName, Email, Role, IsActive
FROM AdminUsers;
```

You should see these columns:
- UserId (INT, NOT NULL, PRIMARY KEY)
- Username (NVARCHAR(100), NOT NULL)
- Password (NVARCHAR(200), NOT NULL)
- FullName (NVARCHAR(200), NOT NULL)
- Email (NVARCHAR(200), NULL)
- Phone (NVARCHAR(20), NULL)
- Role (NVARCHAR(50), NOT NULL)
- IsActive (BIT, NOT NULL)
- CreatedDate (DATETIME2, NOT NULL)
- LastLoginDate (DATETIME2, NULL)
- RefreshToken (NVARCHAR(500), NULL)
- RefreshTokenExpiry (DATETIME2, NULL)
- ResetOtp (NVARCHAR(10), NULL)
- OtpExpiry (DATETIME2, NULL)

### Step 3: Run Remaining Scripts (if needed)
If you need to add more sample data or recreate procedures:
- **02_InsertSeedData.sql** (optional - only if you want sample data)
- **03_CreateProceduresAndViews.sql** (run if procedures don't exist)

---

## Connection String

Update your `appsettings.json` connection string if needed:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=PBM;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Change `Server=.` to your SQL Server instance name if different.

---

## Default Users After Migration/Setup

| Username  | Password      | Role      | Notes                           |
|-----------|---------------|-----------|---------------------------------|
| admin     | Admin@123     | Admin     | System administrator            |
| pastor    | Pastor@123    | Pastor    | Created by seed script only     |
| secretary | Secretary@123 | Secretary | Created by seed script only     |

⚠️ **If you ran the migration script (Option 2)**, all your existing users will have password: `Admin@123`

---

## Troubleshooting

### Error: "Invalid column name 'UserId'"
**Solution**: You need to run the migration script `00_UpdateAdminUsersTable.sql` first.

### Error: "Invalid object name 'AdminUsers'"
**Solution**: Run `01_CreateTables.sql` to create the table.

### Error: "Violation of PRIMARY KEY constraint"
**Solution**: The migration might have failed. Check if UserId column exists and is set as primary key.

### Need to Reset Everything?
If you want to start completely fresh:

```sql
-- WARNING: This deletes all data!
USE PBM;
GO

-- Drop all tables
DROP TABLE IF EXISTS BirthdayWishHistory;
DROP TABLE IF EXISTS Attendance;
DROP TABLE IF EXISTS Expenses;
DROP TABLE IF EXISTS Income;
DROP TABLE IF EXISTS YouTubeLinks;
DROP TABLE IF EXISTS Members;
DROP TABLE IF EXISTS Churches;
DROP TABLE IF EXISTS Verses;
DROP TABLE IF EXISTS AutomationSettings;
DROP TABLE IF EXISTS AdminUsers;
GO

-- Drop views and procedures
DROP VIEW IF EXISTS vw_TodayAttendanceSummary;
DROP VIEW IF EXISTS vw_TodayBirthdays;
DROP VIEW IF EXISTS vw_UpcomingBirthdays;
DROP PROCEDURE IF EXISTS sp_GetAttendanceReport;
DROP PROCEDURE IF EXISTS sp_GetFinancialSummary;
DROP PROCEDURE IF EXISTS sp_MarkAutoAbsent;
DROP PROCEDURE IF EXISTS sp_GetDashboardStats;
GO
```

Then run Option 1 (Fresh Database Setup).

---

## Verification Queries

After setup, run these to verify everything is correct:

```sql
-- Check all tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;

-- Check AdminUsers structure and data
SELECT * FROM AdminUsers;

-- Check views exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.VIEWS
ORDER BY TABLE_NAME;

-- Check stored procedures exist
SELECT ROUTINE_NAME 
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE'
ORDER BY ROUTINE_NAME;

-- Test login
SELECT UserId, Username, Password, Role 
FROM AdminUsers 
WHERE Username = 'admin' AND Password = 'Admin@123';
```

If the last query returns a row, your database is ready!

---

## Next Steps

After database setup is complete:

1. Update `appsettings.json` with correct connection string
2. Run `dotnet build` in the project folder
3. Run `dotnet run` to start the application
4. Navigate to `https://localhost:5001/`
5. Click "Login" and use credentials: `admin` / `Admin@123`

---

## Script Execution Order Summary

### For New Database:
```
CREATE DATABASE PBM
↓
01_CreateTables.sql
↓
02_InsertSeedData.sql
↓
03_CreateProceduresAndViews.sql
↓
Done! ✓
```

### For Existing Database:
```
00_UpdateAdminUsersTable.sql (Migration)
↓
Verify table structure
↓
(Optional) 02_InsertSeedData.sql for additional users
↓
(If needed) 03_CreateProceduresAndViews.sql
↓
Done! ✓
```

---

**Remember**: All passwords are stored in plain text as requested. This is for demo/internal use only and not recommended for production environments.
