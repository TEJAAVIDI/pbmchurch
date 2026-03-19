-- Fix automation settings for birthday and anniversary
-- Update or insert birthday settings
IF NOT EXISTS (SELECT 1 FROM AutomationSettings WHERE SettingKey = 'Birthday_Wish_Enabled')
    INSERT INTO AutomationSettings (SettingKey, SettingValue, Description) 
    VALUES ('Birthday_Wish_Enabled', 'true', 'Enable birthday wish automation');

IF NOT EXISTS (SELECT 1 FROM AutomationSettings WHERE SettingKey = 'Birthday_Wish_Time')
    INSERT INTO AutomationSettings (SettingKey, SettingValue, Description) 
    VALUES ('Birthday_Wish_Time', '07:00', 'Time to send birthday wishes (24-hour format)');

IF NOT EXISTS (SELECT 1 FROM AutomationSettings WHERE SettingKey = 'Birthday_Wish_Message')
    INSERT INTO AutomationSettings (SettingKey, SettingValue, Description) 
    VALUES ('Birthday_Wish_Message', '🎂 Happy Birthday {Name}! 🎉 May God bless you abundantly on your special day and grant you many more years of joy, health, and happiness! 🙏✨', 'Birthday wish message template');

-- Update or insert anniversary settings  
IF NOT EXISTS (SELECT 1 FROM AutomationSettings WHERE SettingKey = 'Anniversary_Wish_Enabled')
    INSERT INTO AutomationSettings (SettingKey, SettingValue, Description) 
    VALUES ('Anniversary_Wish_Enabled', 'true', 'Enable anniversary wish automation');

IF NOT EXISTS (SELECT 1 FROM AutomationSettings WHERE SettingKey = 'Anniversary_Wish_Time')
    INSERT INTO AutomationSettings (SettingKey, SettingValue, Description) 
    VALUES ('Anniversary_Wish_Time', '07:00', 'Time to send anniversary wishes (24-hour format)');

IF NOT EXISTS (SELECT 1 FROM AutomationSettings WHERE SettingKey = 'Anniversary_Wish_Message')
    INSERT INTO AutomationSettings (SettingKey, SettingValue, Description) 
    VALUES ('Anniversary_Wish_Message', '💒 Happy Anniversary {Name}! 🎉 May God continue to bless your marriage with love, joy, and happiness. Wishing you many more wonderful years together! 💕🙏', 'Anniversary wish message template');