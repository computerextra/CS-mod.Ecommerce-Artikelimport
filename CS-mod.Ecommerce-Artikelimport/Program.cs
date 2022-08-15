using CS_mod.Ecommerce_Artikelimport;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;


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
const string tableManufacturersInfo     =   "manufacturers_info";                   // Hersteller Beschreibungen
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
apiConfig.CreateDownloadUrl();
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

// Shop auslesen
Console.WriteLine("Lese Shop Daten ein...");
// Artikel in Liste speichern
string query = "SELECT * FROM " + tableProducts + ";";
List<Product> shopProducts = ProductsReader(shopConfig, query);
query = "SELECT * FROM " + tableProductsDescription + ";";
shopProducts = ProductsReader(shopConfig, query, shopProducts);
query = "SELECT * FROM " + tableProductsToCategories + ";";
shopProducts = ProductsReader(shopConfig, query, shopProducts);

// Kategorien in Liste speichern
query = "SELECT * FROM " + tableCategories + ";";
List<Categorie> shopCategories = CategoriesReader(shopConfig, query);
query = "SELECT * FROM " + tableCategoriesDescription + ";";
shopCategories = CategoriesReader(shopConfig, query, shopCategories);

// Hersteller in Liste speichern
query = "SELECT * FROM " + tableManufacturers + ";";
List<Manufacturer> shopManufacturers = ManufacturerReader(shopConfig, query);
query = "SELECT * FROM " + tableManufacturersInfo + ";";
shopManufacturers = ManufacturerReader(shopConfig, query, shopManufacturers);

// Bilder in Liste speichern
query = "SELECT * FROM " + tableProductsImages + ";";
List<Images> shopImages = ImagesReader(shopConfig, query);

Console.WriteLine("Shop vollständig eingelesen.");

// Prüfen ob Import IDs passen.
CheckImportIds();

// Download der CSV Dateien
Console.WriteLine("Lade Preislisten.");
if (!Directory.Exists(downloadFolder)) { Directory.CreateDirectory(downloadFolder); }
// API Download
Console.WriteLine("Lade API Liste");
apiConfig.DownloadCSV(downloadFolder + "/" + apiFile);
// Kosatec Download
Console.WriteLine("Lade Kosatec Liste");
kosatecConfig.DownloadCSV(downloadFolder + "/" + kosatecFile);
// Intos Download
Console.WriteLine("Lade Intos Liste");
intosConfig.DownloadCSV(downloadFolder + "/" + intosFile);
// Wortmann Download
Console.WriteLine("Lade Wortmann Dateien.");
wortmannConfig.DownloadCSV(downloadFolder + "/" + wortmannProdukte, "/Preisliste/productcatalog.csv");
wortmannConfig.DownloadCSV(downloadFolder + "/" + wortmannContent, "/Preisliste/content.csv");
if (!Directory.Exists(bilderFolder)) { Directory.CreateDirectory(bilderFolder); }
wortmannConfig.DownloadCSV(bilderFolder + "/" + wortmannBilder, "/Produktbilder/productimages.zip");
Console.WriteLine("Alle Dateien runtergeladen.");

// Dateien einlesen
Console.WriteLine("Lese Artikellisten ein...");
// API einlesen
List<Product> apiProducts = ReadCsvFile(apiFile, apiConfig, "api");


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
                "useSage:false\n\n" +
                "#Aktuelle MwSt in %.\n" +
                "MWST:\n";
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
                "#Prefix für die Artikelnummer. Std leer.\n" +
                "Prefix:\n" +
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
                "#Prefix für die Artikelnummer. Std leer.\n" +
                "Prefix:\n" +
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
                "#Prefix für die Artikelnummer. Std leer.\n" +
                "Prefix:\n" +
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
                "#Prefix für die Artikelnummer. Std leer.\n" +
                "Prefix:\n" +
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

static List<Product> ProductsReader(Config config, string query, [Optional] List<Product> tmp)
{
    List<Product> products = new();
    if (tmp != null)
    {
        products = tmp;
    }
    MySqlConnection conn = new("server=" + config.Server + ";user=" + config.User + ";password=" + config.Password + ";database=" + config.Database + ";");
    conn.Open();
    MySqlCommand cmd = new(query, conn);
    MySqlDataReader reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        // Hier sollte es products_description sein, falls keine falsche Tabelle übergeben wird ;) .
        if (reader.FieldCount == 13 && reader[1].GetType().Equals(typeof(Int32)) && reader[2].GetType().Equals(typeof(string))){
            if (!reader.IsDBNull(0))
            {
                // Wenn Artikel noch nicht exisitert wird übersprungen.
                if(!products.Exists(x => x.Id == reader.GetInt32(0))){ continue; }
                var index = products.FindIndex(x => x.Id == reader.GetInt32(0));
                if (!reader.IsDBNull(2)) { products[index].Name = reader.GetString(2); }
                if (!reader.IsDBNull(4)) { products[index].Description = reader.GetString(4); }
                if (!reader.IsDBNull(5)) { products[index].ShortDiscription = reader.GetString(5); }
            }
        }
        // Hier sollte es products sein, falls keine falsche Tabelle übergeben wird ;) .
        if (reader.FieldCount == 31 && reader[1].GetType().Equals(typeof(string)))
        {
            if (!reader.IsDBNull(0))
            {
                // Wenn Artikel bereits vorhanden wird übersprungen, sonst wird ein neuer Artikel angelegt.
                if (products.Exists(x => x.Id == reader.GetInt32(0))) { continue; }
                // temporäre Variablen
                string ean = "", model = "", image = "", manModel = "";
                int quant = 0, sort = 0, status = 0, manId = 0;
                decimal price = 0, weight = 0;
                if (!reader.IsDBNull(1)) { ean = reader.GetString(1); }
                if (!reader.IsDBNull(2)) { quant = reader.GetInt32(2); }
                if (!reader.IsDBNull(4)) { model = reader.GetString(4); }
                if (!reader.IsDBNull(10)) { sort = reader.GetInt32(10); }
                if (!reader.IsDBNull(11)) { image = reader.GetString(11); }
                if (!reader.IsDBNull(12)) { price = reader.GetDecimal(12); }
                if (!reader.IsDBNull(17)) { weight = reader.GetDecimal(17); }
                if (!reader.IsDBNull(18)) { status = reader.GetInt32(18); }
                if (!reader.IsDBNull(22)) { manId = reader.GetInt32(22); }
                if (!reader.IsDBNull(23)) { manModel = reader.GetString(23); }
                products.Add(new Product()
                {
                    Id = reader.GetInt32(0),
                    Ean = ean,
                    Quantity = quant,
                    Model = model,
                    Sort = sort,
                    ProductImage = image,
                    Price = price,
                    Weight = weight,
                    Status = status,
                    ManufacturerId = manId,
                    ManufacturersModel = manModel
                });
            }
        }
        // Hier sollte es products_to_categories sein, falls keine falsche Tabelle übergeben wird ;) .
        if (reader.FieldCount == 2 && reader[1].GetType().Equals(typeof(Int32)))
        {
            if (!reader.IsDBNull(0))
            {
                if (!products.Exists(x => x.Id == reader.GetInt32(0))) { continue; }
                var index = products.FindIndex(x => x.Id == reader.GetInt32(0));
                if (!reader.IsDBNull(1)) { products[index].CategoriesId = reader.GetInt32(1); }
            }
        }
    }
    reader.Close();
    cmd.Dispose();
    conn.Close();
    return products;
}

static List<Categorie> CategoriesReader (Config config, string query, [Optional] List<Categorie> tmp)
{
    List<Categorie> cat = new();
    if (tmp != null) { cat = tmp; }
    MySqlConnection conn = new("server=" + config.Server + ";user=" + config.User + ";password=" + config.Password + ";database=" + config.Database + ";");
    conn.Open();
    MySqlCommand cmd = new(query, conn);
    MySqlDataReader reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        if (!reader.IsDBNull(0))
        {
            // Hier sind wir in categories_description
            if (reader.FieldCount == 8 && reader[1].GetType().Equals(typeof(Int32))) 
            {
                if (!cat.Exists(x => x.Id == reader.GetInt32(0))) { continue; }
                var index = cat.FindIndex(x => x.Id == reader.GetInt32(0));
                if (!reader.IsDBNull(2)) { cat[index].Name = reader.GetString(2); }
            }

            // Hier sind wir in categories
            if (reader.FieldCount == 18 && reader[1].GetType().Equals(typeof(string))) 
            {
                // Überspringen, wenn es die Categorie schon gibt
                if (cat.Exists(x => x.Id == reader.GetInt32(0))) { continue; }
                int parentid = 0, status = 0;
                if (!reader.IsDBNull(2)) { parentid = reader.GetInt32(2); }
                if (!reader.IsDBNull(3)) { status = reader.GetInt32(3); }
                cat.Add(new Categorie()
                {
                    Id = reader.GetInt32(0),
                    ParentId = parentid,
                    Status = status
                });
            }
        }
    }
    reader.Close();
    cmd.Dispose();
    conn.Close();
    return cat;
}

static List<Manufacturer> ManufacturerReader (Config config, string query, [Optional] List<Manufacturer> tmp)
{
    List<Manufacturer> man = new();
    if (tmp != null) { man = tmp; }
    MySqlConnection conn = new("server=" + config.Server + ";user=" + config.User + ";password=" + config.Password + ";database=" + config.Database + ";");
    conn.Open();
    MySqlCommand cmd = new(query, conn);
    MySqlDataReader reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        if (!reader.IsDBNull(0))
        {
            // Hier Table manufacturers
            if(reader.FieldCount == 5 && reader[1].GetType().Equals(typeof(string))) 
            {
                if (man.Exists(x => x.Id == reader.GetInt32(0))) { continue; }
                string name = "";
                if (!reader.IsDBNull(1)) { name = reader.GetString(1); }
                man.Add(new Manufacturer()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            // Hier Table manufacturers_info
            if (reader.FieldCount == 9 && reader[1].GetType().Equals(typeof(Int32))) 
            {
                if(!man.Exists(x => x.Id == reader.GetInt32(0))) { continue; }
                var index = man.FindIndex(x => x.Id == reader.GetInt32(0));
                if (!reader.IsDBNull(2)) { man[index].Description = reader.GetString(2); }
                if (!reader.IsDBNull(6)) { man[index].Url = reader.GetString(6); }
            }
        }
    }
    reader.Close();
    cmd.Dispose();
    conn.Close();
    return man;
}

static List<Images> ImagesReader (Config config, string query)
{
    List<Images> images = new();
    MySqlConnection conn = new("server=" + config.Server + ";user=" + config.User + ";password=" + config.Password + ";database=" + config.Database + ";");
    conn.Open();
    MySqlCommand cmd = new(query, conn);
    MySqlDataReader reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        if (!reader.IsDBNull(0))
        {
            if (images.Exists(x => x.Id == reader.GetInt32(0))) { continue; }
            int prodId = 0, num = 0;
            string name = "";
            if (!reader.IsDBNull(1)) { prodId = reader.GetInt32(1); }
            if (!reader.IsDBNull(2)) { num = reader.GetInt32(2); }
            if (!reader.IsDBNull(3)) { name = reader.GetString(3); }
            images.Add(new Images()
            {
                Id = reader.GetInt32(0),
                ProductsId = prodId,
                ImageNumber = num,
                Name = name
            });
        }
    }
    reader.Close();
    cmd.Dispose();
    conn.Close();
    return images;
}

List<Product> ReadCsvFile(string filename, Config config, string name)
{
    List<Product> list = new();
    if (!config.IsUsed) { Console.WriteLine("Laut Config ungenutzt, wird übersprungen."); return list; }
    // Unterscheidungen der Listen:
    switch (name)
    {
        case "":
            Console.WriteLine("Kein Lieferant übertragen, breche ab!"); break;
        case "api":
            list = ReadApi(filename, config);
            break;
        case "kosatec":
            break;
        case "intos":
            break;
        case "wortmannProducts":
            break;
        case "wortmannContent":
            break;
    }

    return list;
}
List<Product> ReadApi(string filename, Config config)
{
    List<Product> list = new();
    string[] erlaubteHersteller = { "Ultron", "Ultron PCs", "Rasurbo", "Terratec", "Nanoxia", "Cooltek", "Thermalright", "Realpower" };
    foreach(string row in File.ReadAllLines(filename))
    {
        var splitRow = row.Split(config.Trennzeichen);
        // Prüfen auf unerlaubte Hersteller oder durch config ignorierte Artikelnummern / Kategorien
        if (!erlaubteHersteller.Contains(splitRow[3])) { continue; }
        if(config.IgnoredCategories != null)
        {
            if (!config.IgnoredCategories.Contains(splitRow[12])) { continue; }
            if (!config.IgnoredCategories.Contains(splitRow[13])) { continue; }
        }
        if (config.IgnoredItems != null)
            if (!config.IgnoredItems.Contains(splitRow[0])) { continue; }
        // Lese jede Zeile, die nicht abgebrochen wurde.
        /*
         * Benutzt werden folgende Indizies:
         * 0 -> Artikelnummer -> drin
         * 1 -> Titel -> drin
         * 2 -> Beschreibung -> drin
         * 3 -> Hersteller -> drin
         * 4 -> Herstellernummer -> drin
         * 7 -> Gewicht -> drin
         * 8 -> Ean -> drin
         * 9 -> EK netto  -> drin
         * 10 + 11 -> Bestand (extern) -> drin
         * 12 -> Kat -> drin
         * 16 -> Bilder
         */
        int bestand = Convert.ToInt32(splitRow[10]) + Convert.ToInt32(splitRow[11]);
        Product tmp = new()
        {
            Model = config.Prefix + splitRow[0],
            Name = splitRow[1],
            Description = splitRow[2],
            ShortDiscription = splitRow[2],
            ManufacturersModel = splitRow[4],
            Weight = Convert.ToDecimal(splitRow[7]),
            Ean = splitRow[8],
            Quantity = bestand
        };
        // Status des Produktes setzen
        tmp.SetStatus();

        // Prüfe ob Hersteller exisitert, wenn nicht wird er direkt neu angelegt.
        string hersteller = splitRow[3].Trim();
        tmp.ManufacturerId = GetManufacturerId(tmp, hersteller);
       
        // Prüfe ob Kategorie exisitert, wenn nicht, direkt neu anlegen. Könnte ein wenig komisch werden...
        string kategorie = splitRow[12].Trim();
        tmp.CategoriesId = OverloadedMethdos.GetCategoryId(kategorie, config, ref shopCategories, shopConfig);
        Categorie categorie = shopCategories[shopCategories.FindIndex(x => x.Id == tmp.CategoriesId)];

        // Berechne Preis (geht nur bei API, alle anderen brauchen Array an Kategorien...)
        tmp.CalculatePrice(Convert.ToDecimal(splitRow[8]), config, categorie);
        /*
         * TODO!!!!
         * Bilder können noch nicht verarbeitet werden, da nicht klar ist, in welchem Format diese angeboten werden.. 
         * API scheint es nicht wirklich auf die Kette zu bekommen das zu klären...
         */
        list.Add(tmp);
    }
    return list;
}

int GetManufacturerId(Product product, string manufacturerName)
{
    if (shopManufacturers.Contains(new Manufacturer { Name = manufacturerName }))
    {
        int index = shopManufacturers.FindIndex(x => x.Name == manufacturerName);
        return shopManufacturers[index].Id;
    }
    else
    {
        Manufacturer newManufacturer = new()
        {
            Name = manufacturerName
        };
        newManufacturer.GenerateNewManufacturer(shopConfig);
        shopManufacturers.Add(newManufacturer);
        return newManufacturer.Id;
    }
}

void CheckImportIds()
{
    // API
    if (apiConfig.IsUsed && !shopCategories.Contains(new Categorie { Id = apiConfig.ImportID }))
        Console.WriteLine("Fehler in der Api Konfiguration, ImportID exisitert nicht.");
    // Kosatec 
    if (kosatecConfig.IsUsed && !shopCategories.Contains(new Categorie { Id = kosatecConfig.ImportID}))
        Console.WriteLine("Fehler in der Kosatec Konfiguration, ImportID exisitert nicht.");
    // Intos 
    if (intosConfig.IsUsed && !shopCategories.Contains(new Categorie { Id = intosConfig.ImportID}))
        Console.WriteLine("Fehler in der Kosatec Konfiguration, ImportID exisitert nicht.");
    // Wortmann 
    if (wortmannConfig.IsUsed && !shopCategories.Contains(new Categorie { Id = wortmannConfig.ImportID}))
        Console.WriteLine("Fehler in der Kosatec Konfiguration, ImportID exisitert nicht.");
}
