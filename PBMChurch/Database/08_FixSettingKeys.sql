-- Fix inconsistent setting keys
UPDATE AutomationSettings 
SET SettingKey = 'Birthday_Wish_Enabled' 
WHERE SettingKey = 'Birthday_Wishes_Enabled';

-- Ensure all required settings exist with correct keys
IF NOT EXISTS (SELECT 1 FROM AutomationSettings WHERE SettingKey = 'Birthday_Wish_Enabled')
    INSERT INTO AutomationSettings (SettingKey, SettingValue, Description, IsActive) 
    VALUES ('Birthday_Wish_Enabled', 'true', 'Enable birthday wish automation', 1);

IF NOT EXISTS (SELECT 1 FROM AutomationSettings WHERE SettingKey = 'Anniversary_Wish_Enabled')
    INSERT INTO AutomationSettings (SettingKey, SettingValue, Description, IsActive) 
    VALUES ('Anniversary_Wish_Enabled', 'true', 'Enable anniversary wish automation', 1);