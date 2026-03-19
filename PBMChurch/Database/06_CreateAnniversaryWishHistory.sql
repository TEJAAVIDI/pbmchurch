-- Create AnniversaryWishHistory table
CREATE TABLE AnniversaryWishHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MemberId INT NOT NULL,
    WishDate DATE NOT NULL,
    SentAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (MemberId) REFERENCES Members(Id),
    UNIQUE(MemberId, WishDate)
);

-- Add anniversary automation settings
INSERT INTO AutomationSettings (SettingKey, SettingValue, Description) 
VALUES 
('Anniversary_Wish_Enabled', 'true', 'Enable anniversary wish automation'),
('Anniversary_Wish_Time', '07:00', 'Time to send anniversary wishes (24-hour format)'),
('Anniversary_Wish_Message', 'Happy Anniversary {Name}! 🎉 May God continue to bless your marriage with love, joy, and happiness. Wishing you many more wonderful years together! 💕', 'Anniversary wish message template');