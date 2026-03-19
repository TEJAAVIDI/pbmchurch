# Database Migration Steps for Member Details Feature

## Overview
This document outlines the steps to enable the complete Member Details functionality including Notes, Prayer Requests, Offerings, and Attendance tracking.

## Migration Files to Run (in order)

### 1. Add ProfileImage Column (if not already done)
**File:** `03_AddProfileImageColumn.sql`

This adds the ProfileImage column to the Members table for storing member photos.

```sql
-- Run in SQL Server Management Studio
-- Connected to your PBM database
```

### 2. Add Member Details Tables
**File:** `04_AddMemberDetailsTablesScript.sql`

This creates three new tables:
- **MemberNotes** - Store notes about members
- **PrayerRequests** - Track prayer requests from members
- **MemberOfferings** - Record special offerings by members

```sql
-- Run in SQL Server Management Studio
-- Connected to your PBM database
```

## How to Run Migrations

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Open the migration file
4. Execute the script (F5 or Execute button)
5. Verify success messages in the Messages pane

## Expected Output

### For 03_AddProfileImageColumn.sql:
```
Adding ProfileImage column to Members table...
ProfileImage column added successfully
Migration completed!
```

### For 04_AddMemberDetailsTablesScript.sql:
```
Creating MemberNotes table...
MemberNotes table created successfully
Creating PrayerRequests table...
PrayerRequests table created successfully
Creating MemberOfferings table...
MemberOfferings table created successfully
Creating indexes...
Index IX_MemberNotes_MemberId created
Index IX_PrayerRequests_MemberId created
Index IX_MemberOfferings_MemberId created
Migration completed successfully!
```

## Tables Created

### MemberNotes
| Column | Type | Description |
|--------|------|-------------|
| NoteId | INT IDENTITY | Primary Key |
| MemberId | INT | Foreign Key to Members |
| NoteText | NVARCHAR(MAX) | Note content |
| CreatedBy | INT | Admin who created the note |
| CreatedDate | DATETIME | When note was created |

### PrayerRequests
| Column | Type | Description |
|--------|------|-------------|
| RequestId | INT IDENTITY | Primary Key |
| MemberId | INT | Foreign Key to Members |
| Title | NVARCHAR(200) | Optional title |
| Request | NVARCHAR(MAX) | Prayer request content |
| RequestDate | DATETIME | When request was made |
| Status | NVARCHAR(50) | Pending/In Progress/Answered |
| CreatedBy | INT | Admin who created |
| CreatedDate | DATETIME | When created |
| ModifiedBy | INT | Last modified by |
| ModifiedDate | DATETIME | Last modified date |

### MemberOfferings
| Column | Type | Description |
|--------|------|-------------|
| OfferingId | INT IDENTITY | Primary Key |
| MemberId | INT | Foreign Key to Members |
| Date | DATETIME | Offering date |
| Category | NVARCHAR(100) | Offering category |
| Amount | DECIMAL(18,2) | Offering amount |
| Purpose | NVARCHAR(500) | Optional purpose |
| CreatedBy | INT | Admin who recorded |
| CreatedDate | DATETIME | When recorded |

## Features Enabled After Migration

### Member Details Page (`/Member/Details/{id}`)

1. **Notes Tab**
   - Add notes about member
   - View all notes with timestamps
   - Track who added each note

2. **Attendance Tab**
   - View complete attendance history
   - See dates marked present/absent
   - Church-wise attendance records

3. **Prayer Requests Tab**
   - Add prayer requests for member
   - Track request status (Pending/In Progress/Answered)
   - View all historical requests

4. **Offerings Tab**
   - Record special offerings by member
   - Categorize offerings (Tithe, Building Fund, Missions, etc.)
   - View total offerings amount
   - Track offering history

5. **History Tab**
   - Unified timeline of all activities
   - Shows notes, attendance, prayers, offerings
   - Chronological activity feed

## Testing the Features

After running migrations:

1. **Start the application:**
   ```powershell
   dotnet run
   ```

2. **Navigate to Members:**
   - Go to `/Member/Index`
   - Click on any member name

3. **Test each tab:**
   - **Notes:** Add a test note, verify it appears in the list
   - **Attendance:** Verify attendance records display
   - **Prayer Requests:** Click "Add Request" button, fill modal, submit
   - **Offerings:** Click "Record Offering" button, fill modal, submit
   - **History:** Verify all activities appear in timeline

## Troubleshooting

### If tables already exist:
The scripts check for existing tables and will skip creation. You'll see:
```
MemberNotes table already exists
```

### If you get foreign key errors:
Ensure the Members table exists before running `04_AddMemberDetailsTablesScript.sql`

### If modals don't appear:
1. Check browser console for JavaScript errors
2. Verify Bootstrap 5 is loaded in `_Layout.cshtml`
3. Clear browser cache

### If data doesn't save:
1. Check SQL Server for errors in tables
2. Verify database connection string in `appsettings.json`
3. Check application logs for errors

## Rollback (if needed)

To remove the new tables:

```sql
USE PBM;
GO

DROP TABLE IF EXISTS MemberOfferings;
DROP TABLE IF EXISTS PrayerRequests;
DROP TABLE IF EXISTS MemberNotes;
GO

-- To remove ProfileImage column (optional)
-- ALTER TABLE Members DROP COLUMN ProfileImage;
```

## Support

If you encounter any issues:
1. Check the Messages tab in SSMS for detailed error messages
2. Verify you're connected to the correct database (PBM)
3. Ensure you have appropriate permissions to create tables
4. Check that Members table exists before running migrations
