-- ============================================
-- Seed Data for Church Management System
-- ============================================

USE PBM;
GO

-- ============================================
-- Clean up existing data (if re-running script)
-- ============================================
DELETE FROM BirthdayWishHistory;
DELETE FROM Attendance;
DELETE FROM Expenses;
DELETE FROM Income;
DELETE FROM YouTubeLinks;
DELETE FROM Members;
DELETE FROM Churches;
DELETE FROM Verses;
DELETE FROM AutomationSettings;
DELETE FROM AdminUsers;
GO

-- ============================================
-- Insert Default Admin Users
-- Passwords are stored in plain text (not hashed)
-- ============================================
INSERT INTO AdminUsers (Username, Password, FullName, Email, Phone, Role, IsActive, CreatedDate)
VALUES 
('admin', 'Admin@123', 'System Administrator', 'admin@pbmchurch.com', '1234567890', 'Admin', 1, GETDATE()),
('pastor', 'Pastor@123', 'Senior Pastor', 'pastor@pbmchurch.com', '9876543210', 'Admin', 1, GETDATE()),
('secretary', 'Secretary@123', 'Church Secretary', 'secretary@pbmchurch.com', '5555555555', 'Admin', 1, GETDATE()),
('member1', 'Member@123', 'John Smith', 'john@pbmchurch.com', '9876543210', 'Member', 1, GETDATE()),
('member2', 'Member@123', 'Mary Johnson', 'mary@pbmchurch.com', '9876543211', 'Member', 1, GETDATE());
GO

-- ============================================
-- Insert Default Automation Settings
-- ============================================
INSERT INTO AutomationSettings (SettingKey, SettingValue, Description, IsActive)
VALUES 
('WhatsApp_Enabled', 'true', 'Enable/Disable WhatsApp automation', 1),
('WhatsApp_API_URL', 'https://api.whatsapp.com/send', 'WhatsApp API endpoint', 1),
('WhatsApp_Group_ID', '', 'WhatsApp Group ID for daily verse posting', 1),
('Birthday_Wish_Time', '07:00', 'Time to send birthday wishes (24-hour format)', 1),
('Daily_Verse_Time', '06:00', 'Time to send daily verse (24-hour format)', 1),
('Birthday_Wish_Message', 'Happy Birthday {Name}! May God bless you abundantly on your special day. 🎂🎉', 'Birthday wish message template', 1),
('YouTube_Auto_Post', 'true', 'Automatically post YouTube links to WhatsApp', 1);
GO

-- ============================================
-- Insert Sample Churches
-- ============================================
INSERT INTO Churches (ChurchName, Location, MeetingDay1, MeetingDay2, Status, CreatedBy)
VALUES 
('Mummidivaram', 'Downtown District', 'Wednesday', 'Sunday', 'Active', 1),
('Amalapuram', 'North Avenue', 'Tuesday', 'Sunday', 'Active', 1),
('Other', 'East Side', 'Thursday', 'Sunday', 'Active', 1);
GO

-- ============================================
-- Insert Sample Members
-- ============================================
DECLARE @ChurchId1 INT = (SELECT TOP 1 ChurchId FROM Churches WHERE ChurchName = 'Mummidivaram');
DECLARE @ChurchId2 INT = (SELECT TOP 1 ChurchId FROM Churches WHERE ChurchName = 'Amalapuram');
DECLARE @ChurchId3 INT = (SELECT TOP 1 ChurchId FROM Churches WHERE ChurchName = 'Other');

INSERT INTO Members (UserId, Name, Phone, Gender, Family, DateOfBirth, JoinedDate, ChurchId, Status, CreatedBy)
VALUES 
-- Mummidivaram Church Members
('M01', 'John Smith', '9876543210', 'Male', 'Smith Family', '1985-03-15', '2020-01-10', @ChurchId1, 'Active', 1),
('M02', 'Mary Smith', '9876543211', 'Female', 'Smith Family', '1987-07-22', '2020-01-10', @ChurchId1, 'Active', 1),
('M03', 'David Johnson', '9876543212', 'Male', 'Johnson Family', '1990-12-05', '2020-05-15', @ChurchId1, 'Active', 1),
('M04', 'Sarah Wilson', '9876543213', 'Female', 'Wilson Family', '1992-09-18', '2021-02-20', @ChurchId1, 'Active', 1),
('M05', 'Michael Brown', '9876543214', 'Male', 'Brown Family', '1988-04-30', '2021-06-12', @ChurchId1, 'Active', 1),

-- Amalapuram Church Members
('A01', 'Robert Davis', '9876543215', 'Male', 'Davis Family', '1983-11-10', '2019-08-05', @ChurchId2, 'Active', 1),
('A02', 'Jennifer Davis', '9876543216', 'Female', 'Davis Family', '1985-02-14', '2019-08-05', @ChurchId2, 'Active', 1),
('A03', 'James Miller', '9876543217', 'Male', 'Miller Family', '1991-06-25', '2020-03-18', @ChurchId2, 'Active', 1),
('A04', 'Lisa Anderson', '9876543218', 'Female', 'Anderson Family', '1989-08-08', '2020-07-22', @ChurchId2, 'Active', 1),

-- Other Church Members
('O01', 'William Taylor', '9876543219', 'Male', 'Taylor Family', '1986-01-12', '2021-01-15', @ChurchId3, 'Active', 1),
('O02', 'Emily Taylor', '9876543220', 'Female', 'Taylor Family', '1988-05-20', '2021-01-15', @ChurchId3, 'Active', 1),
('O03', 'Daniel Thomas', '9876543221', 'Male', 'Thomas Family', '1993-10-03', '2021-04-10', @ChurchId3, 'Active', 1);

-- ============================================
-- Insert Sample Income Records
-- ============================================
INSERT INTO Income (ChurchId, Amount, Source, IncomeDate, Description, AddedBy)
VALUES 
(@ChurchId1, 5000.00, 'Sunday Offerings', '2024-12-01', 'Regular Sunday offerings', 1),
(@ChurchId1, 10000.00, 'Special Donation', '2024-11-25', 'Thanksgiving special donation', 1),
(@ChurchId2, 3500.00, 'Sunday Offerings', '2024-12-01', 'Regular Sunday offerings', 1),
(@ChurchId3, 4200.00, 'Sunday Offerings', '2024-12-01', 'Regular Sunday offerings', 1);

-- ============================================
-- Insert Sample Expense Records
-- ============================================
INSERT INTO Expenses (ChurchId, Amount, Category, ExpenseDate, Description, AddedBy)
VALUES 
(@ChurchId1, 1500.00, 'Utilities', '2024-11-30', 'Electricity and water bills', 1),
(@ChurchId1, 2000.00, 'Maintenance', '2024-11-28', 'Building repairs and maintenance', 1),
(@ChurchId2, 1200.00, 'Utilities', '2024-11-30', 'Electricity and water bills', 1),
(@ChurchId3, 800.00, 'Supplies', '2024-11-29', 'Office and cleaning supplies', 1);
GO

PRINT 'Seed data inserted successfully!';
PRINT '';
PRINT '============================================';
PRINT 'Default Login Credentials:';
PRINT 'Admin Role:';
PRINT 'Username: admin';
PRINT 'Password: Admin@123';
PRINT '';
PRINT 'Member Role:';
PRINT 'Username: member1';
PRINT 'Password: Member@123';
PRINT '============================================';
GO
