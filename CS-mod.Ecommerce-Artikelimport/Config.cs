using FluentFTP;
using System.Data.Odbc;
using MySql.Data.MySqlClient;

namespace CS_mod.Ecommerce_Artikelimport
{
    public class Config
    {
        public bool IsLieferant                 { get; set; }
        public bool IsUsed                      { get; set; }
        public bool HasFTP                      { get; set; }
        public bool HasSQL                      { get; set; }
        public bool HasMySQL                    { get; set; }
        public bool IsOnlineshop                { get; set; }       // Sieht dumm aus, ist aber interessant, wenn es um die Logins geht, da der Onlineshop 2 Server und Logins hat...

        // Lieferanten Daten
        public bool Aufschlagsart               { get; set; }       // true = Prozentualer Aufschlag auf EK / false = Fester Wert in € als Aufschlag
        public int? ImportID                    { get; set; }       // ID Der Kategorie in die Importiert werden soll
        public string[]? IgnoredItems           { get; set; }       // Array aus Artikelnummern, die nicht importiert werden sollen.
        public string[]? IgnoredCategories      { get; set; }       // Array aus Kategorie Namen, die nicht imortiert werden sollen.
        public string? Trennzeichen             { get; set; }       // CSV Trennzeichen!

        // Aufschläge für einzelne Kategorien. Aufbau: (Name der Kategorie, Wert, Art (0 = Aufschlag in € | 1 = Aufschlag in %))
        public List<string>? EigenerAufschlag { get; set; }         // Eigener Aufschlag für einzelne Kategorie als Liste. Aufbau: string,int,bool


        // FTP / My- / SQL Daten
        public string? Server                   { get; set; }
        public string? FtpServer                { get; set; }       // Nur im Fall vom Onlineshop
        public string? User                     { get; set; }
        public string? FtpUser                  { get; set; }       // Nur im Fall vom Onlineshop
        public string? Password                 { get; set; }
        public string? FtpPassword              { get; set; }       // Nur im Fall vom Onlineshop
        public string? FtpRoot                  { get; set; }       // Nur im Fall vom Onlineshop
        public string? AdminFolder              { get; set; }       // Nur im Fall von Onlineshop
        public string? Database                 { get; set; }       // Nur im Fall von Onlineshop & SAGE
        private string? OdbcConnectionString    { get; set; }       // Nur im Fall von SAGE

        // Download Links
        public string? DownloadURL              { get; set; }       // Nur im Fall von Kosatec

        // Preise
        public string? PreisStandard            { get; set; }       // Option aus "netto" und "brutto"
        public int? Preisaufschlag              { get; set; }       // Aufschlag als Ganzzahl, wird entweder Prozentual oder als fester Wert genommen.

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

        public void SetConnectionString()
        {
            OdbcConnectionString = "Driver={ODBC Driver 17 for SQL Server}; Server=" + Server + ";UID=" + User + ";PWD=" + Password + ";DATABASE=" + Database;
        }

        private bool SqlLogin()
        {
            using OdbcConnection conn = new(OdbcConnectionString);
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
    }
}