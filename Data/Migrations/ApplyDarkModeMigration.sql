-- Skrypt SQL do ręcznego zastosowania migracji AddDarkModeToUser
-- Uruchom ten skrypt w SQL Server Management Studio lub przez sqlcmd

-- Sprawdź czy kolumna już istnieje
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'IsDarkMode')
BEGIN
    -- Dodaj kolumnę IsDarkMode
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [IsDarkMode] bit NOT NULL DEFAULT 0;
    
    PRINT 'Kolumna IsDarkMode została dodana pomyślnie.';
END
ELSE
BEGIN
    PRINT 'Kolumna IsDarkMode już istnieje.';
END
