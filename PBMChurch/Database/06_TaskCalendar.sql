-- ============================================
-- Task Calendar System
-- ============================================

USE PBM;
GO

-- Create Tasks table
CREATE TABLE Tasks (
    TaskId INT IDENTITY(1,1) PRIMARY KEY,
    Summary NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    StartDate DATE NOT NULL,
    StartTime TIME NULL,
    EndDate DATE NOT NULL,
    EndTime TIME NULL,
    IsAllDay BIT NOT NULL DEFAULT 0,
    TaskType NVARCHAR(50) NOT NULL DEFAULT 'Custom', -- Custom, Birthday, Anniversary, ChurchMeeting
    RelatedId INT NULL, -- MemberId for birthdays/anniversaries, ChurchId for meetings
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CreatedBy) REFERENCES AdminUsers(AdminId)
);
GO

-- Create index for better performance
CREATE INDEX IX_Tasks_StartDate ON Tasks(StartDate);
CREATE INDEX IX_Tasks_TaskType ON Tasks(TaskType);
CREATE INDEX IX_Tasks_IsActive ON Tasks(IsActive);
GO

PRINT 'Task Calendar tables created successfully!';