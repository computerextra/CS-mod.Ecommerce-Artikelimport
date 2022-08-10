// Config Files und Ordner
const string configFolder = @"config";
const string configMain = configFolder + "/" + "main.config";
const string configShop = configFolder + "/" + "shop.config";
const string configSage = configFolder + "/" + "sage.config";
const string configKosatec = configFolder + "/" + "kosatec.config";
const string configWortmann = configFolder + "/" + "wortmann.config";
const string configIntos = configFolder + "/" + "intos.config";
const string configApi = configFolder + "/" + "api.config";

// Prüfe ob Config vorhanden
if (!CheckIfConfigExists())
{
    Console.WriteLine("Config Dateien fehlen und werden angelegt.");
    Console.WriteLine("Die Dateien manuell ausfüllen, damit sie im Programm genutzt werden können.");
    CreateConfigFiles();
    Console.WriteLine("Mit beliebiger Taste fortfahren...");
    Console.ReadKey();
    return;
}
Console.WriteLine("Alles da...");

// Funktionen
bool CheckIfConfigExists()
{
    // Prüfen ob Ordner vorhanden sind
    if (!Directory.Exists(configFolder)) { return false; }
    if (!File.Exists(configMain)) { return false; }
    if (!File.Exists(configShop)) { return false; }
    if (!File.Exists(configSage)) { return false; }
    if (!File.Exists(configKosatec)) { return false; }
    if (!File.Exists(configWortmann)) { return false; }
    if (!File.Exists(configIntos)) { return false; }
    if (!File.Exists(configApi)) { return false; }
    return true;
}

void CreateConfigFiles()
{
    string main, shop, sage, kosatec, wortmann, intos, api;
    main =      "# Main Config Datei.\n" +
                "# Konfiguration der Funktionen des Programms.\n" +
                "# Zulässige Werte sind immer: true oder false.\n\n\n" +
                "# Lieferanten ein/aus\n" + 
                "useKosatec:false\n" + 
                "useWortmann:false\n" + 
                "useIntos:false\n" + 
                "useApi:false\n\n\n" + 
                "# Warenwirtschaft ein/aus\n" + 
                "useSage:false";
    shop =      "# Shop Config Datei.\n" +
                "# Konfiguration der Funktionen des Programms.\n\n\n" +
                "# Datenbank einstellungen\n" +
                "IP:0.0.0.0\n" +
                "User:root\n" +
                "Password:passwort1\n" +
                "Database:onlineshop\n\n\n" + 
                "# Anzahl der im Shop Konfigurierten Bilder\n" + 
                "Bilder:10\n";
    sage =      "# SAGE WaWi Config Datei.\n" +
                "# Konfiguration der Funktionen des Programms.\n\n\n" +
                "# Datenbank Einstellungen\n" +
                "Server:SEVER\\XXY\n" +
                "User:root\n" +
                "Password:passwort1\n" +
                "Database:onlineshop\n";
    kosatec =   "# Kosatec Config Datei.\n" +
                "# Konfiguration der Funktionen des Programms.\n\n\n" +
                "# Download einstellungen\n" +
                "DownloadURL:http://data.kosatec.de/xxxx/xxxx/artikeldaten.txt\n\n" +
                "# Einstellungen für Import\n" +
                "# Hier wird die ID für die Kategorie eingegeben, in welche die Artikel importiert werden sollen." +
                "ImportID:1\n\n" +
                "# Einstellungen für Preisberechnung\n" +
                "# netto oder brutto Preise als Standard im Shop\n" +
                "Preis:netto\n\n" +
                "# Aufschlag auf Einkaufspreis\n" +
                "AufschlagWert:20\n\n" +
                "# Aufschlagsart: Standard: Prozentual (Alternativ: Wert in €) (true/false)\n" +
                "AufschlagArt:true\n\n\n" +
                "#################################################################" +
                "# Aus Import ausgeschlossene Artikel als Kommaseparierte Liste.\n" +
                "# Beispiel: 2828000,738658,74356\n" +
                "IgnorierteArtikel:\n\n" +
                "# Aus Import ausgesschlossene Kategorien als Kommaseparierte Liste.\n" +
                "# Beispiel: Software,Server,Papier,Server Mainboards\n" +
                "IgnorierteKategorien:\n\n" +
                "# Eigener Aufschlag für bestimmte Kategorien\n" +
                "# 1. Kategorie Name\n" +
                "# 2. Aufschlag als Ganzzahl\n" +
                "# 3. 1 = Prozentualer Aufschlag / 0 = Aufschlag in €\n" +
                "# immer in dem Format des Beispiels ohne #\n" +
                "# Kat:Used IT,100,0\n";
    wortmann =  "# Wortmann Config Datei.\n" +
                "# Konfiguration der Funktionen des Programms.\n\n\n" +
                "# Download Einstellungen\n" +
                "User:root\n" +
                "Password:passwort1\n" +
                "# Einstellungen für Import\n" +
                "# Hier wird die ID für die Kategorie eingegeben, in welche die Artikel importiert werden sollen." +
                "ImportID:1\n\n" +
                "# Einstellungen für Preisberechnung\n" +
                "# netto oder brutto Preise als Standard im Shop\n" +
                "Preis:netto\n\n" +
                "# Aufschlag auf Einkaufspreis\n" +
                "AufschlagWert:20\n\n" +
                "# Aufschlagsart: Standard: Prozentual (Alternativ: Wert in €) (true/false)\n" +
                "AufschlagArt:true\n\n\n" +
                "#################################################################" +
                "# Aus Import ausgeschlossene Artikel als Kommaseparierte Liste.\n" +
                "# Beispiel: 2828000,738658,74356\n" +
                "IgnorierteArtikel:\n\n" +
                "# Aus Import ausgesschlossene Kategorien als Kommaseparierte Liste.\n" +
                "# Beispiel: Software,Server,Papier,Server Mainboards\n" +
                "IgnorierteKategorien:\n\n" +
                "# Eigener Aufschlag für bestimmte Kategorien\n" +
                "# 1. Kategorie Name\n" +
                "# 2. Aufschlag als Ganzzahl\n" +
                "# 3. 1 = Prozentualer Aufschlag / 0 = Aufschlag in €\n" +
                "# immer in dem Format des Beispiels ohne #\n" +
                "# Kat:Used IT,100,0\n";
    intos =     "# Intos Config Datei.\n" +
                "# Konfiguration der Funktionen des Programms.\n\n\n" +
                "# Download Einstellungen\n" +
                "User:root\n" +
                "Password:passwort1\n" +
                "# Einstellungen für Import\n" +
                "# Hier wird die ID für die Kategorie eingegeben, in welche die Artikel importiert werden sollen." +
                "ImportID:1\n\n" +
                "# Einstellungen für Preisberechnung\n" +
                "# netto oder brutto Preise als Standard im Shop\n" +
                "Preis:netto\n\n" +
                "# Aufschlag auf Einkaufspreis\n" +
                "AufschlagWert:20\n\n" +
                "# Aufschlagsart: Standard: Prozentual (Alternativ: Wert in €) (true/false)\n" +
                "AufschlagArt:true\n\n\n" +
                "#################################################################" +
                "# Aus Import ausgeschlossene Artikel als Kommaseparierte Liste.\n" +
                "# Beispiel: 2828000,738658,74356\n" +
                "IgnorierteArtikel:\n\n" +
                "# Aus Import ausgesschlossene Kategorien als Kommaseparierte Liste.\n" +
                "# Beispiel: Software,Server,Papier,Server Mainboards\n" +
                "IgnorierteKategorien:\n\n" +
                "# Eigener Aufschlag für bestimmte Kategorien\n" +
                "# 1. Kategorie Name\n" +
                "# 2. Aufschlag als Ganzzahl\n" +
                "# 3. 1 = Prozentualer Aufschlag / 0 = Aufschlag in €\n" +
                "# immer in dem Format des Beispiels ohne #\n" +
                "# Kat:Used IT,100,0\n";
    api =       "# API Config Datei.\n" +
                "# Konfiguration der Funktionen des Programms.\n\n\n" +
                "# Download Einstellungen\n" +
                "User:root\n" +
                "Password:passwort1\n" +
                "# Einstellungen für Import\n" +
                "# Hier wird die ID für die Kategorie eingegeben, in welche die Artikel importiert werden sollen." +
                "ImportID:1\n\n" +
                "# Einstellungen für Preisberechnung\n" +
                "# netto oder brutto Preise als Standard im Shop\n" +
                "Preis:netto\n\n" +
                "# Aufschlag auf Einkaufspreis\n" +
                "AufschlagWert:20\n\n" +
                "# Aufschlagsart: Standard: Prozentual (Alternativ: Wert in €) (true/false)\n" +
                "AufschlagArt:true\n\n\n" +
                "#################################################################" +
                "# Aus Import ausgeschlossene Artikel als Kommaseparierte Liste.\n" +
                "# Beispiel: 2828000,738658,74356\n" +
                "IgnorierteArtikel:\n\n" +
                "# Aus Import ausgesschlossene Kategorien als Kommaseparierte Liste.\n" +
                "# Beispiel: Software,Server,Papier,Server Mainboards\n" +
                "IgnorierteKategorien:\n\n" +
                "# Eigener Aufschlag für bestimmte Kategorien\n" +
                "# 1. Kategorie Name\n" +
                "# 2. Aufschlag als Ganzzahl\n" +
                "# 3. 1 = Prozentualer Aufschlag / 0 = Aufschlag in €\n" +
                "# immer in dem Format des Beispiels ohne #\n" +
                "# Kat:Used IT,100,0\n";

    if (!Directory.Exists(configFolder)) { Directory.CreateDirectory(configFolder); }
    if (!File.Exists(configMain)) { WriteFile(configMain, main); }
    if (!File.Exists(configShop)) { WriteFile(configShop, shop); }
    if (!File.Exists(configSage)) { WriteFile(configSage, sage); }
    if (!File.Exists(configKosatec)) { WriteFile(configKosatec, kosatec); }
    if (!File.Exists(configWortmann)) { WriteFile(configWortmann, wortmann); }
    if (!File.Exists(configIntos)) { WriteFile(configIntos, intos); }
    if (!File.Exists(configApi)) { WriteFile(configApi, api); }

    void WriteFile(string path, string content) 
    {
        using StreamWriter sw = new(path);
        sw.Write(content);
        sw.Close();
    }
}