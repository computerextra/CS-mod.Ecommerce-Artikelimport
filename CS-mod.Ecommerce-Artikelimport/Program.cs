using FluentFTP;
using System.Data.Odbc;
using MySql.Data;
using MySql.Data.MySqlClient;

/*
 * Doku für FluentFTP: https://github.com/robinrodricks/FluentFTP/wiki/Quick-Start-Example
 * Doku für Odbc: https://docs.microsoft.com/de-de/dotnet/api/system.data.odbc?view=dotnet-plat-ext-6.0
 * Doku für MySql: https://dev.mysql.com/doc/connector-net/en/connector-net-tutorials-sql-command.html
 */

// Config Files und Ordner
const string configFolder = @"config";
const string configMain = configFolder + "/" + "main.config";
const string configShop = configFolder + "/" + "shop.config";
const string configSage = configFolder + "/" + "sage.config";
const string configKosatec = configFolder + "/" + "kosatec.config";
const string configWortmann = configFolder + "/" + "wortmann.config";
const string configIntos = configFolder + "/" + "intos.config";
const string configApi = configFolder + "/" + "api.config";

// Temporäre Pfade für CSV Dateien.
const string downloadFolder = @"download";
const string bilderFolder = downloadFolder + @"/bilder";
const string kosatecFile = "kosatec.csv";
const string intosFile = "intos.csv";
const string apiFile = "api.csv";
const string wortmannProdukte = "wortmann.csv";
const string wortmannContent = "wortmannContent.csv";
const string wortmannBilder = "bilder.zip";

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

if (CheckIfConfigExists())
{
    Console.WriteLine("Config Dateien vorhanden.");
    Console.WriteLine("Lese Config ein.");
    
    // Variablen
    bool            sage = false, kosatec = false, wortmann = false, intos = false, api = false, 
                    kosatecAufschlagsart, wortmannAufschlagsart, intosAufschlagsart, apiAufschlagsart;
    string          shopDatabaseIPAddess = "", shopDatabaseUser = "", shopDatabasePassword = "", shopDatabase = "",
                    shopFtpServer = @"", shopFtpUser = "", shopFtpPassword = "", shopFtpRoot = @"", shopAdminFolder = "",
                    sageServer = "", sageUser = "", sagePasswort = "", sageDatabase = "",
                    kosatecDownloadURL = @"", kosatecPreisStandard = "",
                    wortmannUser = "", wortmannPassword = "", wortmannPreisStandard = "",
                    intosUser = "", intosPassword = "", intosPreisStandard = "",
                    apiUser = "", apiPassword = "", apiPreisStandard = "";
    int             pictureQuantity = 0, 
                    kosatecImportID = 0, kosatecPreisaufschlag = 0, 
                    wortmannImportID = 0, wortmannPreisaufschlag = 0,
                    intosImportID = 0, intosPreisaufschlag = 0,
                    apiImportID = 0, apiPreisaufschlag = 0;
    string[]        kosatecIgnoredItems, kosatecIgnoredCategories, 
                    wortmannIgnoredItems, wortmannIgnoredCategories,
                    intosIgnoredItems, intosIgnoredCategories,
                    apiIgnoredItems, apiIgnoredCategories;
    List<string>    kosatecEigenerAufschlag = new(),
                    wortmannEigenerAufschlag = new(), 
                    intosEigenerAufschlag = new(),
                    apiEigenerAufschlag = new();

    // Main Config einlesen
    foreach(string line in File.ReadAllLines(configMain))
    {
        if (line.StartsWith("#")) { continue; }
        if (line.StartsWith("\n")) { continue; }
        if (line.Contains(":"))
        {
            var part = line.Split(":");
            part[1].ToLower();
            if (part[1] == "true")
            {
                switch (part[0]) {
                    case "useKosatec":
                        kosatec = true;
                        break;
                    case "useWortmann":
                        wortmann = true;
                        break;
                    case "useIntos":
                        intos = true;
                        break;
                    case "useApi":
                        api = true;
                        break;
                    case "useSage":
                        sage = true;
                        break;
                    default:
                        continue;
                }
            }
        }
    }
    // Shop Config einlesen
    foreach(string line in File.ReadAllLines(configShop))
    {
        if (line.StartsWith("#")) { continue; }
        if (line.StartsWith("\n")) { continue; }
        if (line.StartsWith("IP")) { shopDatabaseIPAddess = line.Split(":")[1]; }
        if (line.StartsWith("User")) { shopDatabaseUser = line.Split(":")[1]; }
        if (line.StartsWith("Password")) { shopDatabasePassword = line.Split(":")[1]; }
        if (line.StartsWith("Database")) { shopDatabase = line.Split(":")[1]; }
        if (line.StartsWith("Bilder")) { pictureQuantity = Convert.ToInt32(line.Split(":")[1]); }
        if (line.StartsWith("ftpServer")) { shopFtpServer = line.Split(":")[1]; }
        if (line.StartsWith("ftpUser")) { shopFtpUser = line.Split(":")[1]; }
        if (line.StartsWith("ftpPassword")) { shopFtpPassword = line.Split(":")[1]; }
        if (line.StartsWith("ShopVerzeichnis")) { shopFtpRoot = line.Split(":")[1]; }
        if (line.StartsWith("AdminOrdnerName")) { shopAdminFolder = line.Split(":")[1]; }
    }
    // SAGE Config einlesen
    if (sage)
    {
        foreach(string line in File.ReadAllLines(configSage))
        {
            if (line.StartsWith("#")) { continue; }
            if (line.StartsWith("\n")) { continue; }
            if (line.StartsWith("Server")) { sageServer = line.Split(":")[1]; }
            if (line.StartsWith("User")) { sageUser = line.Split(":")[1]; }
            if (line.StartsWith("Password")) { sagePasswort = line.Split(":")[1]; }
            if (line.StartsWith("Database")) { sageDatabase = line.Split(":")[1]; }
        }
    }
    // Kosatec Config einlesen
    if (kosatec)
    {
        foreach(string line in File.ReadAllLines(configKosatec))
        {
            if (line.StartsWith("#")) { continue; }
            if (line.StartsWith("\n")) { continue; }
            if (line.StartsWith("DownloadURL")) { kosatecDownloadURL = line.Split(":")[1]; }
            if (line.StartsWith("ImportID")) { kosatecImportID = Convert.ToInt32(line.Split(":")[1]); }
            if (line.StartsWith("Preis")) { kosatecPreisStandard = line.Split(":")[1]; }
            if (line.StartsWith("AufschlagWert")) { kosatecPreisaufschlag = Convert.ToInt32(line.Split(":")[1]); }
            if (line.StartsWith("AufschlagArt")) { kosatecAufschlagsart = Convert.ToBoolean(line.Split(":")[1]); }
            if (line.StartsWith("IgnorierteArtikel")) 
            { 
                var list = line.Split(":"); 
                if(list.Length > 0)
                {
                    kosatecIgnoredItems = list[1].Split(",");
                }
            }
            if (line.StartsWith("IgnorierteKategorien"))
            {
                var list = line.Split(":");
                if (list.Length > 0)
                {
                    kosatecIgnoredCategories = list[1].Split(",");
                }
            }
            if (line.StartsWith("Kat:"))
            {
                kosatecEigenerAufschlag.Add(line.Split(":")[1]);
            }
        }
    }
    // Wortmann Config einlesen
    if (wortmann)
    {
        foreach (string line in File.ReadAllLines(configWortmann))
        {
            if (line.StartsWith("#")) { continue; }
            if (line.StartsWith("\n")) { continue; }
            if (line.StartsWith("User")) { wortmannUser = line.Split(":")[1]; }
            if (line.StartsWith("Password")) { wortmannPassword = line.Split(":")[1]; }
            if (line.StartsWith("ImportID")) { wortmannImportID = Convert.ToInt32(line.Split(":")[1]); }
            if (line.StartsWith("Preis")) { wortmannPreisStandard = line.Split(":")[1]; }
            if (line.StartsWith("AufschlagWert")) { wortmannPreisaufschlag = Convert.ToInt32(line.Split(":")[1]); }
            if (line.StartsWith("AufschlagArt")) { wortmannAufschlagsart = Convert.ToBoolean(line.Split(":")[1]); }
            if (line.StartsWith("IgnorierteArtikel"))
            {
                var list = line.Split(":");
                if (list.Length > 0)
                {
                    wortmannIgnoredItems = list[1].Split(",");
                }
            }
            if (line.StartsWith("IgnorierteKategorien"))
            {
                var list = line.Split(":");
                if (list.Length > 0)
                {
                    wortmannIgnoredCategories = list[1].Split(",");
                }
            }
            if (line.StartsWith("Kat:"))
            {
                wortmannEigenerAufschlag.Add(line.Split(":")[1]);
            }
        }
    }
    // Intos Config einlesen
    if (intos)
    {
        foreach (string line in File.ReadAllLines(configIntos))
        {
            if (line.StartsWith("#")) { continue; }
            if (line.StartsWith("\n")) { continue; }
            if (line.StartsWith("User")) { intosUser = line.Split(":")[1]; }
            if (line.StartsWith("Password")) { intosPassword = line.Split(":")[1]; }
            if (line.StartsWith("ImportID")) { intosImportID = Convert.ToInt32(line.Split(":")[1]); }
            if (line.StartsWith("Preis")) { intosPreisStandard = line.Split(":")[1]; }
            if (line.StartsWith("AufschlagWert")) { intosPreisaufschlag = Convert.ToInt32(line.Split(":")[1]); }
            if (line.StartsWith("AufschlagArt")) { intosAufschlagsart = Convert.ToBoolean(line.Split(":")[1]); }
            if (line.StartsWith("IgnorierteArtikel"))
            {
                var list = line.Split(":");
                if (list.Length > 0)
                {
                    intosIgnoredItems = list[1].Split(",");
                }
            }
            if (line.StartsWith("IgnorierteKategorien"))
            {
                var list = line.Split(":");
                if (list.Length > 0)
                {
                    intosIgnoredCategories = list[1].Split(",");
                }
            }
            if (line.StartsWith("Kat:"))
            {
                intosEigenerAufschlag.Add(line.Split(":")[1]);
            }
        }
    }
    // API Config einlesen
    if (api)
    {
        foreach (string line in File.ReadAllLines(configApi))
        {
            if (line.StartsWith("#")) { continue; }
            if (line.StartsWith("\n")) { continue; }
            if (line.StartsWith("User")) { apiUser = line.Split(":")[1]; }
            if (line.StartsWith("Password")) { apiPassword = line.Split(":")[1]; }
            if (line.StartsWith("ImportID")) { apiImportID = Convert.ToInt32(line.Split(":")[1]); }
            if (line.StartsWith("Preis")) { apiPreisStandard = line.Split(":")[1]; }
            if (line.StartsWith("AufschlagWert")) { apiPreisaufschlag = Convert.ToInt32(line.Split(":")[1]); }
            if (line.StartsWith("AufschlagArt")) { apiAufschlagsart = Convert.ToBoolean(line.Split(":")[1]); }
            if (line.StartsWith("IgnorierteArtikel"))
            {
                var list = line.Split(":");
                if (list.Length > 0)
                {
                    apiIgnoredItems = list[1].Split(",");
                }
            }
            if (line.StartsWith("IgnorierteKategorien"))
            {
                var list = line.Split(":");
                if (list.Length > 0)
                {
                    apiIgnoredCategories = list[1].Split(",");
                }
            }
            if (line.StartsWith("Kat:"))
            {
                apiEigenerAufschlag.Add(line.Split(":")[1]);
            }
        }
    }
    Console.WriteLine("Config vollständig eingelesen. Starte Funktionen.");
    Console.WriteLine("Teste Zugangsdaten für den Shop.");
    // FTP Zugangsdaten prüfen
    if (!checkFtpZugang(shopFtpServer, shopFtpUser, shopFtpPassword, shopFtpRoot))
    {
        Console.WriteLine("Zugangsdaten für den Shop sind falsch, bitte kontrollieren");
        Console.ReadKey();
        return;
    }
    else { Console.WriteLine("Zugansdaten für den Shop sind korrekt."); }
    if (wortmann)
    {
        if (!checkFtpZugang(@"ftp://www.wortmann.de", wortmannUser, wortmannPassword))
        {
            Console.WriteLine("Zugangsdaten für Wortmann sind falsch, bitte kontrollieren");
            Console.ReadKey();
            return;
        }
        else { Console.WriteLine("Zugansdaten für Wortmann sind korrekt."); }
    }
    if (intos)
    {
        if (!checkFtpZugang(@"ftp://ftp.intos.de", intosUser, intosPassword))
        {
            Console.WriteLine("Zugangsdaten für Intos sind falsch, bitte kontrollieren");
            Console.ReadKey();
            return;
        }
        else { Console.WriteLine("Zugansdaten für Intos sind korrekt."); }
    }

    // Zugriff auf Datenbanken prüfen
    string odbcConnectionString = "Driver={ODBC Driver 17 for SQL Server}; Server=" + sageServer + ";UID=" + sageUser +
       ";PWD=" + sagePasswort + ";DATABASE=" + sageDatabase;
    if (sage)
    {
        Console.WriteLine("Prüfe Verbindung zu SAGE Datenbank");
        using OdbcConnection conn = new(odbcConnectionString);
        try
        {
            conn.Open();
            conn.Close();
        }
        catch (Exception)
        {
            Console.WriteLine("Fehlerhafte eingabe. Bitte kontrollieren.\nGgf. muss der ODBC Treiber auf dem PC installiert werden.");
            Console.WriteLine("Download unter: https://www.microsoft.com/de-de/download/details.aspx?id=56567");
            Console.ReadKey();
            return;
        }
    }
    Console.WriteLine("Prüfe Datenbankverbindung zum Shop");
    string mySqlConnectionString = "server=" + shopDatabaseIPAddess + ";user=" + shopDatabaseUser + ";password=" + shopDatabasePassword +
        ";database=" + shopDatabase + ";";
    MySqlConnection mySqlConnection = new(mySqlConnectionString);
    try { mySqlConnection.Open(); mySqlConnection.Close(); }
    catch (Exception) { Console.WriteLine("Konfiguration der Datenbank falsch. Bitte korrigieren."); Console.ReadKey(); return; }
}



// Funktionen
static bool CheckIfConfigExists()
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

static void CreateConfigFiles()
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
                "Bilder:10\n\n" + 
                "# FTP Konfiguration für Bilder Upload\n" + 
                "ftpServer:example.com\n" +
                "ftpUser:user\n" +
                "ftpPassword:passswort1\n" +
                "ShopVerzeichnis:/shop\n" +
                "AdminOrdnerName:admin\n";
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
                "# Hier wird die ID für die Kategorie eingegeben, in welche die Artikel importiert werden sollen.\n" +
                "ImportID:1\n\n" +
                "# Einstellungen für Preisberechnung\n" +
                "# netto oder brutto Preise als Standard im Shop\n" +
                "Preis:netto\n\n" +
                "# Aufschlag auf Einkaufspreis\n" +
                "AufschlagWert:20\n\n" +
                "# Aufschlagsart: Standard: Prozentual (Alternativ: Wert in €) (true/false)\n" +
                "AufschlagArt:true\n\n\n" +
                "#################################################################\n" +
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
                "# Hier wird die ID für die Kategorie eingegeben, in welche die Artikel importiert werden sollen.\n" +
                "ImportID:1\n\n" +
                "# Einstellungen für Preisberechnung\n" +
                "# netto oder brutto Preise als Standard im Shop\n" +
                "Preis:netto\n\n" +
                "# Aufschlag auf Einkaufspreis\n" +
                "AufschlagWert:20\n\n" +
                "# Aufschlagsart: Standard: Prozentual (Alternativ: Wert in €) (true/false)\n" +
                "AufschlagArt:true\n\n\n" +
                "#################################################################\n" +
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
                "# Hier wird die ID für die Kategorie eingegeben, in welche die Artikel importiert werden sollen.\n" +
                "ImportID:1\n\n" +
                "# Einstellungen für Preisberechnung\n" +
                "# netto oder brutto Preise als Standard im Shop\n" +
                "Preis:netto\n\n" +
                "# Aufschlag auf Einkaufspreis\n" +
                "AufschlagWert:20\n\n" +
                "# Aufschlagsart: Standard: Prozentual (Alternativ: Wert in €) (true/false)\n" +
                "AufschlagArt:true\n\n\n" +
                "#################################################################\n" +
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
                "# Hier wird die ID für die Kategorie eingegeben, in welche die Artikel importiert werden sollen.\n" +
                "ImportID:1\n\n" +
                "# Einstellungen für Preisberechnung\n" +
                "# netto oder brutto Preise als Standard im Shop\n" +
                "Preis:netto\n\n" +
                "# Aufschlag auf Einkaufspreis\n" +
                "AufschlagWert:20\n\n" +
                "# Aufschlagsart: Standard: Prozentual (Alternativ: Wert in €) (true/false)\n" +
                "AufschlagArt:true\n\n\n" +
                "#################################################################\n" +
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

bool checkFtpZugang(string server, string user, string pass, string root = "")
{
    FtpClient client = new(server, user, pass);
    try
    {
        client.Connect();
        if(root != "")
        {
            if(!client.DirectoryExists("/" + root + "images/product_images/original_images/"))
            {
                Console.WriteLine("ShopVerzeichnis ist falsch. Bitte kontrollieren.");
                return false;
            }
        }
        client.Disconnect();
        return true;
    }catch(Exception)
    {
        return false;
    }
}