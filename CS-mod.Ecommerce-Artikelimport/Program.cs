// Config Files und Ordner
const string configFolder = @"config";
const string configMain = "main.config";
const string configShop = "shop.config";
const string configSage = "sage.config";
const string configKosatec = "kosatec.config";
const string configWortmann = "wortmann.config";
const string configIntos = "intos.config";
const string configApi = "api.config";

// Prüfe ob Config vorhanden
if (!CheckIfConfigExists())
{
    Console.WriteLine("Config Dateien fehlen und werden angelegt.");
    Console.WriteLine("Die Dateien manuell ausfüllen, damit sie im Programm genutzt werden können.");
    CreateConfigFiles();
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
    Directory.CreateDirectory(configFolder);
}