-- Add AttendanceMode column to AdminUsers table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AdminUsers]') AND name = 'AttendanceMode')
BEGIN
    ALTER TABLE AdminUsers
    ADD AttendanceMode NVARCHAR(20) NULL DEFAULT 'individual';
    
    PRINT 'AttendanceMode column added successfully';
END
ELSE
BEGIN
    PRINT 'AttendanceMode column already exists';
END
GO
