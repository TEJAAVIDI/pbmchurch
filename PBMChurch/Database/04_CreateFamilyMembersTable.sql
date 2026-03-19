-- ============================================
-- Add FamilyMembers Table
-- This table was missing from the original schema
-- ============================================

USE PBM;
GO

-- ============================================
-- Table: FamilyMembers
-- ============================================
CREATE TABLE FamilyMembers (
    FamilyMemberId INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Relation NVARCHAR(50) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(200) NULL,
    DateOfBirth DATE NULL,
    Gender NVARCHAR(10) NULL,
    AnniversaryDate DATE NULL,
    RelatedMemberId INT NULL, -- Link to MemberId of family member
    CONSTRAINT FK_FamilyMembers_Members FOREIGN KEY (MemberId) REFERENCES Members(MemberId) ON DELETE CASCADE,
    CONSTRAINT FK_FamilyMembers_RelatedMembers FOREIGN KEY (RelatedMemberId) REFERENCES Members(MemberId),
    CONSTRAINT CK_FamilyMembers_Gender CHECK (Gender IN ('Male', 'Female', 'Other', NULL))
);
GO

PRINT 'FamilyMembers table created successfully!';
GO