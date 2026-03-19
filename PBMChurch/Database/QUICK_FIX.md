# 🚨 IMMEDIATE FIX FOR YOUR ERROR

## The Error You're Seeing
```
SqlException: Invalid column name 'UserId'.
Invalid column name 'Password'.
Invalid column name 'Role'.
...
```

## ⚡ Quick Fix (Choose One Option)

---

### 🆕 OPTION A: You Have Existing Database with Data
**Use this if you want to keep your existing data**

1. Open SQL Server Management Studio
2. Execute this script: `Database/00_UpdateAdminUsersTable.sql`
3. This will:
   - Add new columns (UserId, Password, Role, RefreshToken, etc.)
   - Migrate your data
   - Remove old columns (AdminId, PasswordHash)
   - Set all passwords to `Admin@123`

**Then restart your application.**

---

### 🔄 OPTION B: Fresh Start (Lose All Data)
**Use this if you want to start completely fresh**

1. Open SQL Server Management Studio

2. Delete everything:
   ```sql
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
   ```

3. Execute these scripts in order:
   - `Database/01_CreateTables.sql`
   - `Database/02_InsertSeedData.sql`
   - `Database/03_CreateProceduresAndViews.sql`

**Then restart your application.**

---

## ✅ Verify It Worked

Run this query in SQL Server Management Studio:

```sql
-- Should return a row with the new structure
SELECT UserId, Username, Password, Role, RefreshToken, ResetOtp
FROM AdminUsers
WHERE Username = 'admin';
```

If you see columns like `UserId`, `Password`, `Role`, `RefreshToken` - you're good! ✓

---

## 🔐 After Fix - Login Credentials

| Username | Password   |
|----------|------------|
| admin    | Admin@123  |

---

## 📋 Why This Happened

The code expects the new AdminUsers table structure with:
- `UserId` (instead of `AdminId`)
- `Password` (instead of `PasswordHash`)
- `Role`, `RefreshToken`, `ResetOtp`, `OtpExpiry` (new columns)

But your database still has the old structure.

The migration script fixes this automatically!

---

## 🆘 Still Having Issues?

Check which structure you currently have:

```sql
-- See your current columns
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AdminUsers'
ORDER BY ORDINAL_POSITION;
```

**Old Structure (causing error):**
- AdminId
- PasswordHash
- No Role, RefreshToken, etc.

**New Structure (what you need):**
- UserId
- Password
- Role
- RefreshToken
- ResetOtp
- OtpExpiry

---

## 📞 Quick Command Reference

### After running migration, restart the app:
```powershell
# In PowerShell at project folder
dotnet run
```

### Test the app:
1. Go to: https://localhost:5001/
2. Click "Login"
3. Use: admin / Admin@123
4. Should see dashboard with sidebar ✓

---

**Choose Option A if you have data to keep, Option B for fresh start!**
