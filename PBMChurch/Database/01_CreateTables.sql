-- ============================================
-- Church Management System - Database Schema
-- Database: PBM
-- ============================================

USE PBM;
GO

-- Drop existing tables if they exist (in correct order due to foreign keys)
IF OBJECT_ID('Attendance', 'U') IS NOT NULL DROP TABLE Attendance;
IF OBJECT_ID('Income', 'U') IS NOT NULL DROP TABLE Income;
IF OBJECT_ID('Expenses', 'U') IS NOT NULL DROP TABLE Expenses;
IF OBJECT_ID('Members', 'U') IS NOT NULL DROP TABLE Members;
IF OBJECT_ID('Churches', 'U') IS NOT NULL DROP TABLE Churches;
IF OBJECT_ID('Verses', 'U') IS NOT NULL DROP TABLE Verses;
IF OBJECT_ID('YouTubeLinks', 'U') IS NOT NULL DROP TABLE YouTubeLinks;
IF OBJECT_ID('AutomationSettings', 'U') IS NOT NULL DROP TABLE AutomationSettings;
IF OBJECT_ID('BirthdayWishHistory', 'U') IS NOT NULL DROP TABLE BirthdayWishHistory;
IF OBJECT_ID('AdminUsers', 'U') IS NOT NULL DROP TABLE AdminUsers;
GO

-- ============================================
-- Table: AdminUsers
-- ============================================
CREATE TABLE AdminUsers (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(200) NOT NULL, -- Plain text password (not hashed)
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200),
    Phone NVARCHAR(20),
    Role NVARCHAR(50) NOT NULL DEFAULT 'Admin',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastLoginDate DATETIME2 NULL,
    RefreshToken NVARCHAR(500) NULL,
    RefreshTokenExpiry DATETIME2 NULL,
    ResetOtp NVARCHAR(10) NULL,
    OtpExpiry DATETIME2 NULL
);
GO

-- ============================================
-- Table: Churches
-- ============================================
CREATE TABLE Churches (
    ChurchId INT IDENTITY(1,1) PRIMARY KEY,
    ChurchName NVARCHAR(200) NOT NULL,
    Location NVARCHAR(300),
    MeetingDay1 NVARCHAR(20) NOT NULL, -- e.g., 'Tuesday'
    MeetingDay2 NVARCHAR(20) NOT NULL, -- e.g., 'Sunday'
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active/Inactive
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy INT NULL,
    CONSTRAINT CK_Churches_Status CHECK (Status IN ('Active', 'Inactive'))
);
GO

-- ============================================
-- Table: Members
-- ============================================
CREATE TABLE Members (
    MemberId INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(50) NOT NULL UNIQUE, -- Unique ID for attendance entry
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Gender NVARCHAR(10) NULL, -- Male/Female/Other
    Family NVARCHAR(200) NULL,
    DateOfBirth DATE NULL,
    JoinedDate DATE NOT NULL,
    ChurchId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active/Inactive
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedBy INT,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy INT NULL,
    CONSTRAINT FK_Members_Churches FOREIGN KEY (ChurchId) REFERENCES Churches(ChurchId),
    CONSTRAINT CK_Members_Status CHECK (Status IN ('Active', 'Inactive')),
    CONSTRAINT CK_Members_Gender CHECK (Gender IN ('Male', 'Female', 'Other', NULL))
);
GO

-- ============================================
-- Table: Attendance
-- ============================================
CREATE TABLE Attendance (
    AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
    ChurchId INT NOT NULL,
    MemberId INT NOT NULL,
    AttendanceDate DATE NOT NULL,
    WeekNumber INT NOT NULL, -- Week number of the year
    IsPresent BIT NOT NULL DEFAULT 0,
    MarkedBy INT,
    MarkedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(500) NULL,
    CONSTRAINT FK_Attendance_Churches FOREIGN KEY (ChurchId) REFERENCES Churches(ChurchId),
    CONSTRAINT FK_Attendance_Members FOREIGN KEY (MemberId) REFERENCES Members(MemberId),
    CONSTRAINT UQ_Attendance_Member_Date UNIQUE (MemberId, AttendanceDate)
);
GO

-- Create index for faster attendance queries
CREATE NONCLUSTERED INDEX IX_Attendance_Date_Church 
ON Attendance(AttendanceDate, ChurchId, IsPresent);
GO

-- ============================================
-- Table: Income
-- ============================================
CREATE TABLE Income (
    IncomeId INT IDENTITY(1,1) PRIMARY KEY,
    ChurchId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Source NVARCHAR(200) NOT NULL, -- Offerings, Donations, etc.
    IncomeDate DATE NOT NULL,
    Description NVARCHAR(500) NULL,
    AddedBy INT NOT NULL,
    AddedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NULL,
    ModifiedBy INT NULL,
    CONSTRAINT FK_Income_Churches FOREIGN KEY (ChurchId) REFERENCES Churches(ChurchId),
    CONSTRAINT CK_Income_Amount CHECK (Amount >= 0)
);
GO

-- ============================================
-- Table: Expenses
-- ============================================
CREATE TABLE Expenses (
    ExpenseId INT IDENTITY(1,1) PRIMARY KEY,
    ChurchId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Category NVARCHAR(200) NOT NULL, -- Utilities, Maintenance, etc.
    ExpenseDate DATE NOT NULL,
    Description NVARCHAR(500) NULL,
    AddedBy INT NOT NULL,
    AddedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NULL,
    ModifiedBy INT NULL,
    CONSTRAINT FK_Expenses_Churches FOREIGN KEY (ChurchId) REFERENCES Churches(ChurchId),
    CONSTRAINT CK_Expenses_Amount CHECK (Amount >= 0)
);
GO

-- ============================================
-- Table: Verses
-- ============================================
CREATE TABLE Verses (
    VerseId INT IDENTITY(1,1) PRIMARY KEY,
    ImageFileName NVARCHAR(300) NOT NULL,
    StorageURL NVARCHAR(500) NOT NULL,
    UploadedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UploadedBy INT,
    LastPostedDate DATETIME2 NULL,
    PostCount INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- ============================================
-- Table: YouTubeLinks
-- ============================================
CREATE TABLE YouTubeLinks (
    LinkId INT IDENTITY(1,1) PRIMARY KEY,
    ChurchId INT NOT NULL,
    Title NVARCHAR(300) NOT NULL,
    YouTubeURL NVARCHAR(500) NOT NULL,
    Description NVARCHAR(1000) NULL,
    PostedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    PostedBy INT,
    IsSentToWhatsApp BIT NOT NULL DEFAULT 0,
    SentDate DATETIME2 NULL,
    CONSTRAINT FK_YouTubeLinks_Churches FOREIGN KEY (ChurchId) REFERENCES Churches(ChurchId)
);
GO

-- ============================================
-- Table: AutomationSettings
-- ============================================
CREATE TABLE AutomationSettings (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy INT
);
GO

-- ============================================
-- Table: BirthdayWishHistory
-- ============================================
CREATE TABLE BirthdayWishHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    WishSentDate DATE NOT NULL,
    SentDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(50) NOT NULL, -- Sent/Failed
    ErrorMessage NVARCHAR(500) NULL,
    CONSTRAINT FK_BirthdayWishHistory_Members FOREIGN KEY (MemberId) REFERENCES Members(MemberId),
    CONSTRAINT UQ_BirthdayWish_Member_Date UNIQUE (MemberId, WishSentDate)
);
GO

PRINT 'All tables created successfully!';
GO
