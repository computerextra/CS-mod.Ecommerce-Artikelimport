using CS_mod.Ecommerce_Artikelimport;


/*
 * Doku für FluentFTP:  https://github.com/robinrodricks/FluentFTP/wiki/Quick-Start-Example
 * Doku für Odbc:       https://docs.microsoft.com/de-de/dotnet/api/system.data.odbc?view=dotnet-plat-ext-6.0
 * Doku für MySql:      https://dev.mysql.com/doc/connector-net/en/connector-net-tutorials-sql-command.html
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

Console.WriteLine("Config Dateien vorhanden.");
Console.WriteLine("Lese Config ein.");

// Variablen
bool            sage = false,                           // Wenn true: Sage wird benutzt, Config wird ausgelesen.
                kosatec = false,                        // Wenn true: Kosatec wird benutzt, Config wird ausgelesen.
                wortmann = false,                       // Wenn true: Wortmann wird benutzt, Config wird ausgelesen.
                intos = false,                          // Wenn true: Intos wird benutzt, Config wird ausgelesen.
                api = false;                            // Wenn true: Api wird benutzt, Config wird ausgelesen.

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


// Configs anlegen
Config shopConfig = new()
{
    IsUsed = true,
    HasFTP = true,
    HasMySQL = true,
    IsOnlineshop = true
};
shopConfig.ReadConfig(configShop);
Config kosatecConfig = new()
{
    IsLieferant = true,
    IsUsed = kosatec,
};
Config wortmannConfig = new()
{
    IsLieferant = true,
    IsUsed = wortmann,
    HasFTP = true,
};
Config intosConfig = new()
{
    IsLieferant = true,
    IsUsed = intos,
    HasFTP = true,
};
Config apiConfig = new()
{
    IsLieferant = true,
    IsUsed = api,
};
Config sageConfig = new()
{
    IsUsed = sage,
    HasSQL = true,
};

Console.WriteLine("Lese Konfiguration ... ");
Console.WriteLine("Lese Kosatec Konfiguration");
kosatecConfig.ReadConfig(configKosatec);
Console.WriteLine("Lese Wortmann Konfiguration");
wortmannConfig.ReadConfig(configWortmann);
wortmannConfig.Server = @"ftp://www.wortmann.de";
Console.WriteLine("Lese Intos Konfiguration");
intosConfig.ReadConfig(configIntos);
intosConfig.Server = @"ftp://ftp.intos.de";
Console.WriteLine("Lese Api Konfiguration");
apiConfig.ReadConfig(configApi);
Console.WriteLine("Lese Sage Konfiguration");
sageConfig.ReadConfig(configSage);
Console.WriteLine("Konfiguration eingelesen.");

Console.WriteLine("Teste Logindaten...");
Console.WriteLine("Teste Onlineshop");
if (!shopConfig.CheckLogin()) { Console.WriteLine("Fehler in der Konfiguration. Bitte überprüfen."); Console.ReadKey(); return; }
Console.WriteLine("Teste Wortmann");
if (!wortmannConfig.CheckLogin()) { Console.WriteLine("Fehler in der Konfiguration. Bitte überprüfen."); Console.ReadKey(); return; }
Console.WriteLine("Teste Intos");
if (!intosConfig.CheckLogin()) { Console.WriteLine("Fehler in der Konfiguration. Bitte überprüfen."); Console.ReadKey(); return; }
Console.WriteLine("Teste Sage");
if (!sageConfig.CheckLogin()) { Console.WriteLine("Fehler in der Konfiguration. Bitte überprüfen."); Console.ReadKey(); return; }
Console.WriteLine("Logindaten geprüft.");


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
                "Server:0.0.0.0\n" +
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
                "# CSV Trennzeichen:\n" + 
                "Trennzeichen:\"\n" +
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
                 "# CSV Trennzeichen:\n" + 
                "Trennzeichen:\"\n" +
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
                 "# CSV Trennzeichen:\n" + 
                "Trennzeichen:\"\n" +
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
                 "# CSV Trennzeichen:\n" + 
                "Trennzeichen:\"\n" +
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
