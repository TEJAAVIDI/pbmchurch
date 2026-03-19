-- ============================================
-- Stored Procedures and Views for Reports
-- ============================================

USE PBM;
GO

-- ============================================
-- View: Today's Attendance Summary
-- ============================================
CREATE OR ALTER VIEW vw_TodayAttendanceSummary
AS
SELECT 
    c.ChurchId,
    c.ChurchName,
    COUNT(DISTINCT m.MemberId) AS TotalMembers,
    SUM(CASE WHEN a.IsPresent = 1 THEN 1 ELSE 0 END) AS PresentCount,
    SUM(CASE WHEN a.IsPresent = 0 OR a.AttendanceId IS NULL THEN 1 ELSE 0 END) AS AbsentCount
FROM Churches c
LEFT JOIN Members m ON c.ChurchId = m.ChurchId AND m.Status = 'Active'
LEFT JOIN Attendance a ON m.MemberId = a.MemberId 
    AND a.AttendanceDate = CAST(GETDATE() AS DATE)
WHERE c.Status = 'Active'
GROUP BY c.ChurchId, c.ChurchName;
GO

-- ============================================
-- View: Today's Birthdays
-- ============================================
CREATE OR ALTER VIEW vw_TodayBirthdays
AS
SELECT 
    m.MemberId,
    m.UserId,
    m.Name,
    m.Phone,
    m.DateOfBirth,
    c.ChurchName,
    DATEDIFF(YEAR, m.DateOfBirth, GETDATE()) AS Age
FROM Members m
INNER JOIN Churches c ON m.ChurchId = c.ChurchId
WHERE m.Status = 'Active'
    AND MONTH(m.DateOfBirth) = MONTH(GETDATE())
    AND DAY(m.DateOfBirth) = DAY(GETDATE());
GO

-- ============================================
-- View: Upcoming Birthdays (Next 30 Days)
-- ============================================
CREATE OR ALTER VIEW vw_UpcomingBirthdays
AS
SELECT 
    m.MemberId,
    m.UserId,
    m.Name,
    m.Phone,
    m.DateOfBirth,
    c.ChurchName,
    DATEFROMPARTS(YEAR(GETDATE()), MONTH(m.DateOfBirth), DAY(m.DateOfBirth)) AS NextBirthday,
    DATEDIFF(DAY, GETDATE(), 
        CASE 
            WHEN DATEFROMPARTS(YEAR(GETDATE()), MONTH(m.DateOfBirth), DAY(m.DateOfBirth)) >= CAST(GETDATE() AS DATE)
            THEN DATEFROMPARTS(YEAR(GETDATE()), MONTH(m.DateOfBirth), DAY(m.DateOfBirth))
            ELSE DATEFROMPARTS(YEAR(GETDATE()) + 1, MONTH(m.DateOfBirth), DAY(m.DateOfBirth))
        END
    ) AS DaysUntilBirthday
FROM Members m
INNER JOIN Churches c ON m.ChurchId = c.ChurchId
WHERE m.Status = 'Active'
    AND m.DateOfBirth IS NOT NULL
    AND DATEDIFF(DAY, GETDATE(), 
        CASE 
            WHEN DATEFROMPARTS(YEAR(GETDATE()), MONTH(m.DateOfBirth), DAY(m.DateOfBirth)) >= CAST(GETDATE() AS DATE)
            THEN DATEFROMPARTS(YEAR(GETDATE()), MONTH(m.DateOfBirth), DAY(m.DateOfBirth))
            ELSE DATEFROMPARTS(YEAR(GETDATE()) + 1, MONTH(m.DateOfBirth), DAY(m.DateOfBirth))
        END
    ) BETWEEN 0 AND 30;
GO

-- ============================================
-- Stored Procedure: Get Attendance Report
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetAttendanceReport
    @ChurchId INT = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @Status NVARCHAR(20) = NULL -- 'Present', 'Absent', or NULL for all
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Default dates if not provided
    IF @FromDate IS NULL SET @FromDate = DATEADD(MONTH, -1, GETDATE());
    IF @ToDate IS NULL SET @ToDate = GETDATE();
    
    SELECT 
        a.AttendanceId,
        a.AttendanceDate,
        a.WeekNumber,
        c.ChurchId,
        c.ChurchName,
        m.UserId,
        m.Name AS MemberName,
        m.Phone,
        m.Family,
        a.IsPresent,
        CASE WHEN a.IsPresent = 1 THEN 'Present' ELSE 'Absent' END AS AttendanceStatus,
        a.MarkedDate
    FROM Attendance a
    INNER JOIN Members m ON a.MemberId = m.MemberId
    INNER JOIN Churches c ON a.ChurchId = c.ChurchId
    WHERE 
        (@ChurchId IS NULL OR a.ChurchId = @ChurchId)
        AND a.AttendanceDate BETWEEN @FromDate AND @ToDate
        AND (@Status IS NULL 
            OR (@Status = 'Present' AND a.IsPresent = 1)
            OR (@Status = 'Absent' AND a.IsPresent = 0))
    ORDER BY a.AttendanceDate DESC, c.ChurchName, m.Name;
END;
GO

-- ============================================
-- Stored Procedure: Get Monthly Income/Expense Summary
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetFinancialSummary
    @ChurchId INT = NULL,
    @Year INT = NULL,
    @Month INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Default to current year/month if not provided
    IF @Year IS NULL SET @Year = YEAR(GETDATE());
    IF @Month IS NULL SET @Month = MONTH(GETDATE());
    
    SELECT 
        c.ChurchId,
        c.ChurchName,
        ISNULL(SUM(i.Amount), 0) AS TotalIncome,
        ISNULL(SUM(e.Amount), 0) AS TotalExpense,
        ISNULL(SUM(i.Amount), 0) - ISNULL(SUM(e.Amount), 0) AS Balance
    FROM Churches c
    LEFT JOIN Income i ON c.ChurchId = i.ChurchId 
        AND YEAR(i.IncomeDate) = @Year 
        AND MONTH(i.IncomeDate) = @Month
    LEFT JOIN Expenses e ON c.ChurchId = e.ChurchId 
        AND YEAR(e.ExpenseDate) = @Year 
        AND MONTH(e.ExpenseDate) = @Month
    WHERE 
        (@ChurchId IS NULL OR c.ChurchId = @ChurchId)
        AND c.Status = 'Active'
    GROUP BY c.ChurchId, c.ChurchName;
END;
GO

-- ============================================
-- Stored Procedure: Mark Auto Absent for Unmarked Members
-- ============================================
CREATE OR ALTER PROCEDURE sp_MarkAutoAbsent
    @ChurchId INT,
    @AttendanceDate DATE,
    @MarkedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get week number
    DECLARE @WeekNumber INT = DATEPART(WEEK, @AttendanceDate);
    
    -- Insert absent records for members who don't have attendance marked
    INSERT INTO Attendance (ChurchId, MemberId, AttendanceDate, WeekNumber, IsPresent, MarkedBy, MarkedDate)
    SELECT 
        @ChurchId,
        m.MemberId,
        @AttendanceDate,
        @WeekNumber,
        0, -- Absent
        @MarkedBy,
        GETDATE()
    FROM Members m
    WHERE m.ChurchId = @ChurchId
        AND m.Status = 'Active'
        AND NOT EXISTS (
            SELECT 1 
            FROM Attendance a 
            WHERE a.MemberId = m.MemberId 
                AND a.AttendanceDate = @AttendanceDate
        );
    
    SELECT @@ROWCOUNT AS RecordsInserted;
END;
GO

-- ============================================
-- Stored Procedure: Get Dashboard Statistics
-- ============================================
CREATE OR ALTER PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Total Active Members
    DECLARE @TotalMembers INT = (SELECT COUNT(*) FROM Members WHERE Status = 'Active');
    
    -- Today's Present Count
    DECLARE @PresentToday INT = (
        SELECT COUNT(DISTINCT a.MemberId) 
        FROM Attendance a 
        WHERE a.AttendanceDate = CAST(GETDATE() AS DATE) 
            AND a.IsPresent = 1
    );
    
    -- Today's Absent Count
    DECLARE @AbsentToday INT = @TotalMembers - @PresentToday;
    
    -- Today's Birthdays
    DECLARE @BirthdaysToday INT = (
        SELECT COUNT(*) 
        FROM Members 
        WHERE Status = 'Active'
            AND MONTH(DateOfBirth) = MONTH(GETDATE())
            AND DAY(DateOfBirth) = DAY(GETDATE())
    );
    
    -- This Month Income
    DECLARE @MonthIncome DECIMAL(18,2) = (
        SELECT ISNULL(SUM(Amount), 0)
        FROM Income
        WHERE YEAR(IncomeDate) = YEAR(GETDATE())
            AND MONTH(IncomeDate) = MONTH(GETDATE())
    );
    
    -- This Month Expense
    DECLARE @MonthExpense DECIMAL(18,2) = (
        SELECT ISNULL(SUM(Amount), 0)
        FROM Expenses
        WHERE YEAR(ExpenseDate) = YEAR(GETDATE())
            AND MONTH(ExpenseDate) = MONTH(GETDATE())
    );
    
    -- Return results
    SELECT 
        @TotalMembers AS TotalMembers,
        @PresentToday AS PresentToday,
        @AbsentToday AS AbsentToday,
        @BirthdaysToday AS BirthdaysToday,
        @MonthIncome AS MonthIncome,
        @MonthExpense AS MonthExpense,
        @MonthIncome - @MonthExpense AS MonthBalance;
END;
GO

PRINT 'Stored procedures and views created successfully!';
GO
