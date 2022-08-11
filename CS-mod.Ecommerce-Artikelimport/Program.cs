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
const string configFolder               =   @"config";                              // Ordner in dem alle Konfigdateien gespeichert sind.
const string configMain                 =   configFolder + "/" + "main.config";     // Haupt Konfigurationsdatei
const string configShop                 =   configFolder + "/" + "shop.config";     // Konfigurationsdatei für Shop
const string configSage                 =   configFolder + "/" + "sage.config";     // Konfigurationsdatei für Sage
const string configKosatec              =   configFolder + "/" + "kosatec.config";  // Konfigurationsdatei für Kosatec
const string configWortmann             =   configFolder + "/" + "wortmann.config"; // Konfigurationsdatei für Wortmann
const string configIntos                =   configFolder + "/" + "intos.config";    // Konfigurationsdatei für Intos
const string configApi                  =   configFolder + "/" + "api.config";      // Konfigurationsdatei für Api

// Temporäre Pfade für CSV Dateien. Werden nach Programmende wieder gelöscht.
const string downloadFolder             =   @"download";                            // Temp. Ordner für Downloads
const string bilderFolder               =   downloadFolder + "/bilder";             // Temp. Ordner für Bilderdownload
const string kosatecFile                =   "kosatec.csv";                          // Kosatec Preisliste
const string intosFile                  =   "intos.csv";                            // Intos Preisliste
const string apiFile                    =   "api.csv";                              // API Preisliste
const string wortmannProdukte           =   "wortmann.csv";                         // Wortmann Produktdaten
const string wortmannContent            =   "wortmannContent.csv";                  // Wortmann Langtexte
const string wortmannBilder             =   "bilder.zip";                           // Wortmann Artikelbilder

// Shop Datenbank Tabellen, die bearbeitet werden müssen
const string tableProducts              =   "products";                             // Stammdaten der Artikel
const string tableProductsDescription   =   "products_description";                 // Beschreibungen der Artikel
const string tableProductsImages        =   "products_images";                      // Produktbilder
const string tableManufacturers         =   "manufacturers";                        // Hersteller
const string tableCategories            =   "categories";                           // Kategorien
const string tableCategoriesDescription =   "categories_description";               // Beschreibungen der Kategorien
const string tableProductsToCategories  =   "products_to_categories";               // Verknüpfen der Artikel mit den Kategorien

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
    bool            sage = false,                           // Wenn true: Sage wird benutzt, Config wird ausgelesen.
                    kosatec = false,                        // Wenn true: Kosatec wird benutzt, Config wird ausgelesen.
                    wortmann = false,                       // Wenn true: Wortmann wird benutzt, Config wird ausgelesen.
                    intos = false,                          // Wenn true: Intos wird benutzt, Config wird ausgelesen.
                    api = false,                            // Wenn true: Api wird benutzt, Config wird ausgelesen.
                    kosatecAufschlagsart,                   // true = Prozentualer Aufschlag auf EK / false = Fester Wert in € als Aufschlag
                    wortmannAufschlagsart,                  // true = Prozentualer Aufschlag auf EK / false = Fester Wert in € als Aufschlag
                    intosAufschlagsart,                     // true = Prozentualer Aufschlag auf EK / false = Fester Wert in € als Aufschlag
                    apiAufschlagsart;                       // true = Prozentualer Aufschlag auf EK / false = Fester Wert in € als Aufschlag

    string          // Online Shop
                    shopDatabaseIPAddess        = "",       // IP Adresse der Datenbank für den Onlineshop, Alternativ auch Domain möglich.
                    shopDatabaseUser            = "",       // Datenbank Benutzer mit Lese- und Schreibrechten
                    shopDatabasePassword        = "",       // Passwort für Datenbank Benutzer
                    shopDatabase                = "",       // Datenbank vom Shop

                    shopFtpServer               = "",       // FTP Server mit Zugriff auf den Shop
                    shopFtpUser                 = "",       // FTP Benutzer
                    shopFtpPassword             = "",       // Passwort für FTP Benutzer
                    shopFtpRoot                 = "",       // Pfad vom Loginroot des Benutzers zum Onlineshop (Bsp: shop/ )
                    shopAdminFolder             = "",       // Benennung des Admin Orderns innerhalb des Shops (Bsp: admin_xyz )
                    // SAGE
                    sageServer                  = "",       // SQL Server für Warenwirtschaft
                    sageUser                    = "",       // Benutzer mit Zugriff auf die Datenbank
                    sagePasswort                = "",       // Passwort für Datenbank Benutzer
                    sageDatabase                = "",       // Name der Datenbank für SAGE
                    // Kosatec
                    kosatecDownloadURL          = "",       // Downloadlink für Artikeldaten von Kosatec
                    kosatecPreisStandard        = "",       // Option aus "netto" und "brutto"
                    // Wortmann
                    wortmannUser                = "",       // FTP Benutzer, gestellt von Wortmann
                    wortmannPassword            = "",       // Passwort für FTP Benutzer
                    wortmannPreisStandard       = "",       // Option aus "netto" und "brutto"
                    // Intos
                    intosUser                   = "",       // FTP Benutzer, gestellt von Intos
                    intosPassword               = "",       // Passwort für FTP Benutzer
                    intosPreisStandard          = "",       // Option aus "netto" und "brutto"
                    // API
                    apiUser                     = "",       // Kundennummer von API
                    apiPassword                 = "",       // Passwort für den Onlineshop
                    apiPreisStandard            = "";       // Option aus "netto" und "brutto"

    int             pictureQuantity,                        // Anzahl der im Shop aktivierten Bilder pro Artikel

                    kosatecImportID,                        // ID Der Kategorie in die Importiert werden soll
                    kosatecPreisaufschlag,                  // Aufschlag als Ganzzahl, wird entweder Prozentual oder als fester Wert genommen.

                    wortmannImportID,                       // ID Der Kategorie in die Importiert werden soll
                    wortmannPreisaufschlag,                 // Aufschlag als Ganzzahl, wird entweder Prozentual oder als fester Wert genommen.

                    intosImportID,                          // ID Der Kategorie in die Importiert werden soll
                    intosPreisaufschlag,                    // Aufschlag als Ganzzahl, wird entweder Prozentual oder als fester Wert genommen.

                    apiImportID,                            // ID Der Kategorie in die Importiert werden soll
                    apiPreisaufschlag;                      // Aufschlag als Ganzzahl, wird entweder Prozentual oder als fester Wert genommen.

    string[]        kosatecIgnoredItems,                    // Array aus Artikelnummern, die nicht importiert werden sollen.
                    kosatecIgnoredCategories,               // Array aus Kategorie Namen, die nicht imortiert werden sollen.

                    wortmannIgnoredItems,                   // Array aus Artikelnummern, die nicht importiert werden sollen.
                    wortmannIgnoredCategories,              // Array aus Kategorie Namen, die nicht imortiert werden sollen.

                    intosIgnoredItems,                      // Array aus Artikelnummern, die nicht importiert werden sollen.
                    intosIgnoredCategories,                 // Array aus Kategorie Namen, die nicht imortiert werden sollen.

                    apiIgnoredItems,                        // Array aus Artikelnummern, die nicht importiert werden sollen.
                    apiIgnoredCategories;                   // Array aus Kategorie Namen, die nicht imortiert werden sollen.

    List<string>    // Aufschläge für einzelne Kategorien. Aufbau: (Name der Kategorie, Wert, Art (0 = Aufschlag in € | 1 = Aufschlag in %))
                    kosatecEigenerAufschlag     = new(),    // Eigener Aufschlag für einzelne Kategorie als Liste. Aufbau: string,int,bool
         
                    wortmannEigenerAufschlag    = new(),    // Eigener Aufschlag für einzelne Kategorie als Liste. Aufbau: string,int,bool

                    intosEigenerAufschlag       = new(),    // Eigener Aufschlag für einzelne Kategorie als Liste. Aufbau: string,int,bool

                    apiEigenerAufschlag         = new();    // Eigener Aufschlag für einzelne Kategorie als Liste. Aufbau: string,int,bool

    // Main Config einlesen
    foreach (string line in File.ReadAllLines(configMain))
    {
        if (line.StartsWith("#")) { continue; }
        if (line.StartsWith("\n")) { continue; }
        if (line.Contains(':'))
        {
            var part = line.Split(":");
            part[1] = part[1].ToLower();
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
    if (!checkFtpZugang(shopFtpServer, shopFtpUser, shopFtpPassword, shopFtpRoot, shopAdminFolder))
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
    Console.WriteLine("Datenbank Verbindung okay.");
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

bool checkFtpZugang(string server, string user, string pass, string root = "", string adminfolder = "")
{
    FtpClient client = new(server, user, pass);
    try
    {
        client.Connect();
        if(root != "")
        {
            if(!client.DirectoryExists("/" + root + adminfolder))
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