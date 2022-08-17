using FluentFTP;
using System.Data.Odbc;
using MySql.Data.MySqlClient;
using System.Net;

namespace CS_mod.Ecommerce_Artikelimport
{
    public class Config
    {
        public bool IsLieferant                 { get; set; } = false;
        public bool IsUsed                      { get; set; } = false;
        public bool HasFTP                      { get; set; } = false;
        public bool HasSQL                      { get; set; } = false;
        public bool HasMySQL                    { get; set; } = false;
        public bool IsOnlineshop                { get; set; } = false;      // Sieht dumm aus, ist aber interessant, wenn es um die Logins geht, da der Onlineshop 2 Server und Logins hat...

        // Lieferanten Daten
        public bool Aufschlagsart               { get; set; }       // true = Prozentualer Aufschlag auf EK / false = Fester Wert in € als Aufschlag
        public int ImportID                    { get; set; } = 0;       // ID Der Kategorie in die Importiert werden soll
        public string[]? IgnoredItems           { get; set; }        // Array aus Artikelnummern, die nicht importiert werden sollen.
        public string[]? IgnoredCategories      { get; set; }       // Array aus Kategorie Namen, die nicht imortiert werden sollen.
        public string? Trennzeichen             { get; set; }       // CSV Trennzeichen!
        public string Prefix                    { get; set; } = "";

        // Aufschläge für einzelne Kategorien. Aufbau: (Name der Kategorie, Wert, Art (0 = Aufschlag in € | 1 = Aufschlag in %))
        public List<string>? EigenerAufschlag { get; set; }         // Eigener Aufschlag für einzelne Kategorie als Liste. Aufbau: string,int,bool


        // FTP / My- / SQL Daten
        public string? Server                   { get; set; }
        public string? FtpServer                { get; set; }       // Nur im Fall vom Onlineshop
        public string? User { get; set; } = "";
        public string? FtpUser                  { get; set; }       // Nur im Fall vom Onlineshop
        public string? Password                 { get; set; } = "";
        public string? FtpPassword              { get; set; }       // Nur im Fall vom Onlineshop
        public string? FtpRoot                  { get; set; }       // Nur im Fall vom Onlineshop
        public string? AdminFolder              { get; set; }       // Nur im Fall von Onlineshop
        public string? Database                 { get; set; }       // Nur im Fall von Onlineshop & SAGE
        private string? OdbcConnectionString { get; set; } = "";       // Nur im Fall von SAGE

        // Download Links
        public string? DownloadURL { get; set; } = "";      // Nur im Fall von Kosatec und API

        // Preise
        public string? PreisStandard            { get; set; }       // Option aus "netto" und "brutto"
        public int? Preisaufschlag              { get; set; }       // Aufschlag als Ganzzahl, wird entweder Prozentual oder als fester Wert genommen.
        public int? MwSt                        { get; set; }       // Aktuelle MwSt aus Config,.

        // Anzahl der im Shop aktivierten Bilder pro Artikel
        public int? AnzahlBilder                { get; set; }

        public void ReadConfig(string filename)
        {
            if (!IsUsed) { Console.WriteLine("Übershprungen, da nicht genutzt."); return; }
            EigenerAufschlag = new();
            foreach (string line in File.ReadAllLines(filename))
            {
                var split = line.Split(":");
                switch (split[0])
                {
                    case "Server":
                        Server = split[1];
                        break;
                    case "User":
                        User = split[1];
                        break;
                    case "Password":
                        Password = split[1];
                        break ;
                    case "Database":
                        Database = split[1];
                        break;
                    case "Bilder":
                        AnzahlBilder = Convert.ToInt32(split[1]);
                        break;
                    case "ftpServer":
                        FtpServer = split[1];
                        break;
                    case "ftpUser":
                        FtpUser = split[1];
                        break;
                    case "ftpPassword":
                        FtpPassword = split[1];
                        break;
                    case "ShopVerzeichnis":
                        FtpRoot = split[1];
                        break;
                    case "AdminOrdnerName":
                        AdminFolder = split[1];
                        break;
                    case "DownloadURL":
                        if(split.Length > 2)
                            DownloadURL = split[1]+":"+split[2];
                        else
                            DownloadURL = split[1];
                        break;
                    case "ImportID":
                        ImportID = Convert.ToInt32(split[1]);
                        break;
                    case "Preis":
                        PreisStandard = split[1];
                        break;
                    case "AufschlagWert":
                        Preisaufschlag = Convert.ToInt32(split[1]);
                        break;
                    case "AufschlagArt":
                        Aufschlagsart = Convert.ToBoolean(split[1]);
                        break;
                    case "IgnorierteArtikel":
                        if (split[1].Length > 0) { IgnoredItems = split[1].Split(","); }
                        break;
                    case "IgnorierteKategorien":
                        if (split[1].Length > 0) { IgnoredCategories = split[1].Split(","); }
                        break;
                    case "Kat":
                        EigenerAufschlag.Add(split[1]);
                        break;
                    case "Trennzeichen":
                        Trennzeichen = split[1];
                        break;
                    case "Prefix":
                        Prefix = split[1];
                        break;
                    case "MWST":
                        MwSt = Convert.ToInt32(split[1]);
                        break;
                    default: continue;
                }
            }
            Console.WriteLine("Eingelesen.");
        }
        public bool CheckLogin()
        {
            if (!IsUsed) { Console.WriteLine("Übersprungen, da nicht genutzt."); return true; }
            bool passed = false;
            if (HasFTP) { passed = FtpLogin(); }
            if (HasSQL) { passed = SqlLogin(); }
            if (HasMySQL) { passed = MySqlLogin(); }
            return passed;
        }

        private void SetConnectionString()
        {
            OdbcConnectionString = "Driver={ODBC Driver 17 for SQL Server}; Server=" + Server + ";UID=" + User + ";PWD=" + Password + ";DATABASE=" + Database;
        }
        public string GetConnectionString()
        {
            SetConnectionString();
            if(OdbcConnectionString != null)
                return OdbcConnectionString;
            return "";
        }
        private bool SqlLogin()
        {
            using OdbcConnection conn = new(GetConnectionString());
            try
            {
                conn.Open();
                conn.Close();
            }
            catch (Exception) { return false; }
            return true;
        }
        private bool FtpLogin()
        {
            bool passed = false;
            FtpClient client;
            if (IsOnlineshop)
            {
                client = new(FtpServer, FtpUser, FtpPassword);
            }
            else
            {
                client = new(Server, User, Password);
            }
            try
            {
                client.Connect();
                if(FtpRoot != "" && client.DirectoryExists("/" + FtpRoot + AdminFolder)) { passed = true; }
                client.Disconnect();
            }
            catch (Exception) { return false; }
            return passed;
        }
        private bool MySqlLogin()
        {
            MySqlConnection conn = new("server=" + Server + ";user=" + User + ";password=" + Password + ";database=" + Database + ";");
            try
            {
                conn.Open();
                conn.Close();
            }
            catch (Exception) { return false; }
            return true;
        }
        public void CreateDownloadUrl()
        {
            DownloadURL = @"https://pricelist.api.de/pricelist/?cid=" + User + "&shop=API&action=FULL&map=sku;title;description;manufacturer;msku;itemGroup;availability;weight;ean;price;stock;externalStock;lwg1;lwg2;productCat;imageLinks;productUrl1;productUrl2;productUrl3;isEol";
        }
        public void DownloadCSV(string savePath, string serverPath = "")
        {
            if (!IsLieferant) { return; }
            if (!IsUsed) { Console.WriteLine("Nicht genutzt, wird übersprungen."); return; }
            if(HasFTP && serverPath == "") { Console.WriteLine("Keinen Serverpfad angegeben."); return; }
            if (HasFTP && serverPath != "")
            {
                FtpClient client = new(Server, User, Password);
                client.Connect();
                if (client.FileExists(serverPath))
                {
                    client.DownloadFile(savePath, serverPath);
                }
                client.Disconnect();
            }
            // Hier ist dann der Downloadlink gefragt.
            if (!HasFTP)
            {
                if(DownloadURL == "") { Console.WriteLine("DownloadUrl nicht gesetzt."); return; }
#pragma warning disable SYSLIB0014 // Typ oder Element ist veraltet
                using var client = new WebClient();
                try 
                {
                    client.Credentials = new NetworkCredential(User, Password);
#pragma warning disable CS8604 // Mögliches Nullverweisargument. Nicht möglich, da im If bereits gecatched.
                    client.DownloadFile(DownloadURL, savePath);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
                }
                catch (Exception)
                {
                    // Für Kosatec, weil der Download irgendwie beim ersten Mal nie klappt... beim zweiten mal aber ohne Probleme...
                    client.Credentials = new NetworkCredential(User, Password);
#pragma warning disable CS8604 // Mögliches Nullverweisargument. Nicht möglich, da im If bereits gecatched.
                    client.DownloadFile(DownloadURL, savePath);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
                }
#pragma warning restore SYSLIB0014 // Typ oder Element ist veraltet
            }
        }
    }
}