# SQL Scripts Execution Guide

## Execute these scripts in SQL Server Management Studio in this exact order:

---

## ⚠️ IMPORTANT: Before You Start

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Create the database if it doesn't exist:

```sql
CREATE DATABASE PBM;
GO
USE PBM;
GO
```

---

## 📋 Script Execution Order

### ✅ Step 1: Create Database Structure
**File:** `Database/01_CreateTables.sql`
- Creates all 10 tables
- Sets up relationships and constraints
- Adds indexes for performance

### ✅ Step 2: Insert Initial Data
**File:** `Database/02_InsertSeedData.sql`
- Creates default admin user (admin / Admin@123)
- Inserts automation settings
- Creates 3 sample churches
- Adds 12 sample members
- Inserts sample income/expense records

### ✅ Step 3: Create Stored Procedures & Views
**File:** `Database/03_CreateProceduresAndViews.sql`
- Creates 5 views for reporting
- Creates 4 stored procedures for business logic
- Optimizes queries for dashboard

---

## 🔍 Verification Queries

After running all scripts, verify the setup:

```sql
-- Check all tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Should return 10 tables:
-- AdminUsers, Attendance, AutomationSettings, BirthdayWishHistory,
-- Churches, Expenses, Income, Members, Verses, YouTubeLinks

-- Check sample data
SELECT 'Churches' AS TableName, COUNT(*) AS RecordCount FROM Churches
UNION ALL
SELECT 'Members', COUNT(*) FROM Members
UNION ALL
SELECT 'AdminUsers', COUNT(*) FROM AdminUsers
UNION ALL
SELECT 'AutomationSettings', COUNT(*) FROM AutomationSettings;

-- Should show:
-- Churches: 3
-- Members: 12
-- AdminUsers: 1
-- AutomationSettings: 7

-- Test stored procedure
EXEC sp_GetDashboardStats;

-- Test views
SELECT * FROM vw_TodayAttendanceSummary;
SELECT * FROM vw_TodayBirthdays;
SELECT TOP 5 * FROM vw_UpcomingBirthdays;
```

---

## 🎯 What Gets Created

### Tables (10)
1. **AdminUsers** - System administrators
2. **Churches** - Church locations with meeting schedules
3. **Members** - Church members with details
4. **Attendance** - Attendance records
5. **Income** - Financial income tracking
6. **Expenses** - Financial expense tracking
7. **Verses** - Daily Bible verse images
8. **YouTubeLinks** - Sermon video links
9. **AutomationSettings** - WhatsApp automation config
10. **BirthdayWishHistory** - Birthday wish tracking

### Views (3)
1. **vw_TodayAttendanceSummary** - Today's attendance by church
2. **vw_TodayBirthdays** - Members with birthdays today
3. **vw_UpcomingBirthdays** - Birthdays in next 30 days

### Stored Procedures (4)
1. **sp_GetAttendanceReport** - Flexible attendance reporting
2. **sp_GetFinancialSummary** - Monthly financial summary
3. **sp_MarkAutoAbsent** - Auto-mark absent members
4. **sp_GetDashboardStats** - Dashboard statistics

---

## 📊 Sample Data Included

### 3 Churches:
- Grace Community Church (Wednesday + Sunday)
- Faith Fellowship Church (Tuesday + Sunday)
- Hope Chapel (Thursday + Sunday)

### 12 Members:
- 5 members at Grace Community Church
- 4 members at Faith Fellowship Church
- 3 members at Hope Chapel

### 1 Admin User:
- Username: `admin`
- Password: `Admin@123`

### 7 Automation Settings:
- WhatsApp configuration
- Birthday wish settings
- Daily verse settings
- YouTube auto-post settings

### Sample Financial Records:
- 4 income transactions
- 4 expense transactions

---

## 🔧 Connection String

After running scripts, use this connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=PBM;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Or for specific server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PBM;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

---

## ⚡ Quick Execution (All-in-One)

If you want to run all scripts at once:

```sql
-- Step 1: Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PBM')
BEGIN
    CREATE DATABASE PBM;
END
GO

USE PBM;
GO

-- Then paste contents of:
-- 01_CreateTables.sql
-- 02_InsertSeedData.sql
-- 03_CreateProceduresAndViews.sql
-- (in that order)
```

---

## 🐛 Troubleshooting

### Error: "Database already exists"
```sql
-- Drop and recreate (⚠️ This deletes all data!)
USE master;
GO
DROP DATABASE IF EXISTS PBM;
GO
CREATE DATABASE PBM;
GO
```

### Error: "Cannot drop table because it is referenced by a foreign key"
- Run scripts in correct order
- Use the DROP statements provided in 01_CreateTables.sql

### Error: "Login failed for user"
- Check SQL Server authentication mode
- Verify user has appropriate permissions
- Use Windows Authentication if possible

---

## ✅ Success Indicators

You'll know setup is successful when:

1. All 10 tables created without errors
2. Sample data inserts show "X rows affected"
3. Verification queries return expected counts
4. No red error messages in SSMS
5. Application connects and shows login page

---

## 📱 Next Steps

After successful database setup:

1. Update `appsettings.json` with your connection string
2. Run `dotnet restore` in project folder
3. Run `dotnet build` to compile
4. Run `dotnet run` to start application
5. Navigate to `https://localhost:5001`
6. Login with: admin / Admin@123

---

## 🎓 Understanding the Schema

### Key Relationships:
- **Members** belong to **Churches**
- **Attendance** tracks **Members** at **Churches**
- **Income** and **Expenses** are linked to **Churches**
- **BirthdayWishHistory** references **Members**
- **YouTubeLinks** are associated with **Churches**

### Important Constraints:
- Members must have unique `UserId`
- One attendance record per member per date
- Status fields accept only "Active" or "Inactive"
- All amounts must be non-negative

---

## 📈 Performance Optimizations

The scripts include:
- Indexes on frequently queried columns
- Unique constraints to prevent duplicates
- Foreign key relationships for data integrity
- Efficient views for common queries
- Stored procedures for complex operations

---

## 🔐 Security Notes

- Default admin password should be changed immediately
- Consider implementing password hashing in production
- Restrict database user permissions appropriately
- Use parameterized queries (handled by EF Core)
- Enable SQL Server encryption for sensitive data

---

**Ready to Execute!** 🚀

Run the scripts, verify the data, and launch your church management system!
