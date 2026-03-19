# 🚨 Quick Fix Instructions

## Your migration had errors. Here's what to do:

### Step 1: Check Current Status
Run this script in SQL Server Management Studio:
```
Database/CHECK_STATUS.sql
```

This will tell you exactly what state your database is in.

---

### Step 2: Apply the Fix

Run this script in SQL Server Management Studio:
```
Database/MANUAL_FIX.sql
```

This will:
- ✓ Add any missing new columns
- ✓ Copy data from old columns to new
- ✓ Set proper constraints
- ✓ Fix the primary key
- ✓ Remove old columns
- ✓ Show you the final result

---

### Step 3: Verify It Worked

After running MANUAL_FIX.sql, you should see output like:

```
FINAL TABLE STRUCTURE:
COLUMN_NAME          DATA_TYPE    
UserId               int          
Username             nvarchar     
Password             nvarchar     
FullName             nvarchar     
Email                nvarchar     
Phone                nvarchar     
Role                 nvarchar     
IsActive             bit          
...

CURRENT USERS:
UserId  Username  Password    FullName              Role
1       admin     Admin@123   System Administrator  Admin
```

---

### Step 4: Run Your App

```powershell
dotnet run
```

Then visit: https://localhost:5001/
Login with: **admin** / **Admin@123**

---

## ✅ Success Checklist

- [ ] CHECK_STATUS.sql shows "SUCCESS! Table has new structure"
- [ ] MANUAL_FIX.sql completed without errors
- [ ] Can see UserId, Password, Role columns
- [ ] Old AdminId and PasswordHash columns are gone
- [ ] Application runs without SqlException errors
- [ ] Can login with admin/Admin@123

---

## 🆘 Still Having Issues?

### If CHECK_STATUS shows "MIXED STATE":
The table has both old and new columns. MANUAL_FIX.sql will handle this.

### If you see "Primary key could not be created":
You might have duplicate UserId values. Check with:
```sql
SELECT UserId, COUNT(*) 
FROM AdminUsers 
GROUP BY UserId 
HAVING COUNT(*) > 1;
```

### If nothing works - Nuclear Option:
Start completely fresh (⚠️ **DELETES ALL DATA**):

```sql
USE PBM;
GO
DROP TABLE IF EXISTS AdminUsers;
GO
```

Then run:
1. Database/01_CreateTables.sql
2. Database/02_InsertSeedData.sql  
3. Database/03_CreateProceduresAndViews.sql

---

## 📝 Quick Reference

**Scripts in order of use:**
1. **CHECK_STATUS.sql** - See what's wrong
2. **MANUAL_FIX.sql** - Fix it automatically
3. Restart app with `dotnet run`

**Default Login:**
- Username: `admin`
- Password: `Admin@123`

---

**Run CHECK_STATUS.sql first to see exactly what needs to be fixed!**
