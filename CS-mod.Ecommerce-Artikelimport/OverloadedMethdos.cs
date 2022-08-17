namespace CS_mod.Ecommerce_Artikelimport
{
    public class OverloadedMethdos
    {
        public static int GetCategoryId(string kategorie, Config config, ref List<Categorie> shopCategories, Config shopConfig)
        {
            // Easy: Import Kategorie ist directer Parent von Kategorie. (Funktioniert nur bei API da nur 1 Kategorie angegeben wird :( )
            int index = shopCategories.FindIndex(x => x.Name == kategorie && x.ParentId == config.ImportID);
            if (index > 0)
            {
                return shopCategories[index].Id;
            }
            // Kategorie nicht vorhanden, wird neu angelegt.
            Categorie newKat = new() { Name = kategorie, ParentId = config.ImportID };
            newKat.GenerateNewCategorie(shopConfig);
            shopCategories.Add(newKat);
            return newKat.Id;
        }
        public static int GetCategoryId(string[] kategorie, Config config, ref List<Categorie> shopCategories, Config shopConfig, ref List<Categorie> productCategories)
        {
            // Prüfe auf leere Einträge im Array:
            var tmp = kategorie.ToList();
            for(int i = 0; i < kategorie.Length; i++)
            {
                if (kategorie[i] == "")
                    tmp.RemoveAt(i);
            }
            kategorie = tmp.ToArray();
            // Nicht mehr Easy:  Alles muss gecheckt werden.
            // Einfach mal die Schleife durchlaufen
            int lastId = config.ImportID;
            for (int i = 0; i < kategorie.Length; i++)
            {
                int index = shopCategories.FindIndex(x => x.Name == kategorie[i] && x.ParentId == lastId);
               
                // Kategorie noch nich vorhanden... Muss angelegt werden.
                if (index < 0) 
                {
                    Categorie newKat = new() { Name = kategorie[i], ParentId = lastId };
                    newKat.GenerateNewCategorie(shopConfig);
                    shopCategories.Add(newKat);
                    productCategories.Add(newKat);
                    lastId = newKat.Id;
                }
                else
                {
                    lastId = shopCategories[index].Id;
                    productCategories.Add(shopCategories[index]);
                }
            }
            // Alle Kategorien inkl. Parents angelegt. Jetzt geben wir die letzte gesetzte "lastId" zurück, da diese auf die letzte Kategorie im Array verweist.
            return lastId;
        }


        public static List<Product> ReadCsvFile(string filename, Config config, string name, ref Config shopConfig, ref List<Manufacturer> shopManufacturer, ref List<Categorie> shopCategories)
        {
            List<Product> list = new();
            if (!config.IsUsed) { Console.WriteLine("Laut Config ungenutzt, wird übersprungen."); return list; }
            // Unterscheidungen der Listen:
            switch (name)
            {
                case "":
                    Console.WriteLine("Kein Lieferant übertragen, breche ab!"); break;
                case "api":
                    list = ReadApi(filename, config, ref shopConfig, ref shopManufacturer, ref shopCategories);
                    break;
                case "kosatec":
                    list = ReadKosatec(filename, config, ref shopConfig, ref shopManufacturer, ref shopCategories);
                    break;
                case "intos":
                    list = ReadIntos(filename, config, ref shopConfig, ref shopManufacturer, ref shopCategories);
                    break;
            }
            return list;
        }

        private static List<Product> ReadIntos(string filename, Config config, ref Config shopConfig, ref List<Manufacturer> shopManufacturer, ref List<Categorie> shopCategories)
        {
            List<Product> list = new();
            foreach (string row in File.ReadAllLines(filename))
            {
                var splitRow = row.Split(config.Trennzeichen);
                // Prüfe auf Ignorierte Kategorien und Artikelnummern
                if (config.IgnoredCategories != null)
                {
                    if (splitRow[20] != "")
                        if (config.IgnoredCategories.Contains(splitRow[20]))
                            continue;
                    if (splitRow[21] != "")
                        if (config.IgnoredCategories.Contains(splitRow[21]))
                            continue;
                    if (splitRow[22] != "")
                        if (config.IgnoredCategories.Contains(splitRow[22]))
                            continue;
                }
                if (config.IgnoredItems != null)
                    if (splitRow[0] != "")
                        if (config.IgnoredItems.Contains(splitRow[0]))
                            continue;
                // Benötigte Indizies:
                /*
                 * 0 => Artikelnummer 
                 * 0 => Herstellernummer 
                 * 1 => Artikelname 
                 * 2 => Short Desc 
                 * 2 => Long Summary 
                 * 3 => EAN 
                 * 5 => MEnge 
                 * 6 => Gewicht 
                 * 7 => UVP Brutto
                 * 8 => Hersteller
                 * 9 - 19 => Bilder
                 * 20 - 22 => Kategorien 1-3
                 */
                Product product = new()
                {
                    Model = config.Prefix + splitRow[0],
                    ManufacturersModel = splitRow[0],
                    Name = splitRow[1],
                    Ean = splitRow[3],
                    Quantity = Convert.ToInt32(splitRow[5]),
                    Weight = Convert.ToDecimal(splitRow[6]),
                    ShortDiscription = splitRow[2],
                    Description = splitRow[2],
                };
                product.SetStatus();
                string hersteller = splitRow[8].Trim();
                product.ManufacturerId = GetManufacturerId(product, hersteller, ref shopManufacturer, ref shopConfig);
                string[] categories = { splitRow[20].Trim(), splitRow[21].Trim(), splitRow[22].Trim()};
                List<Categorie> allCats = new();
                product.CategoriesId = OverloadedMethdos.GetCategoryId(categories, config, ref shopCategories, shopConfig, ref allCats);
                product.CalculatePrice(Convert.ToDecimal(splitRow[6]), config, allCats);
                product.ProductImage = Path.GetFileName(splitRow[9]);
                product.Images = splitRow[9] + ";" + splitRow[10] + ";" + splitRow[11] + ";" + splitRow[12] + ";" + splitRow[13] + ";" + 
                    splitRow[14] + ";" + splitRow[15] + ";" + splitRow[16] + ";" + splitRow[17] + ";" + splitRow[18] + ";" + splitRow[19];
                
                list.Add(product);
            }
            return list;

        }

        public static List<Product> ReadCsvFile(string file1, string file2, Config config, string name, ref Config shopConfig, ref List<Manufacturer> shopManufacturer, ref List<Categorie> shopCategories)
        {
            List<Product> list = new();
            if (name == "wortmann")
                list = ReadWortmann(file1, file2, config, ref shopConfig, ref shopManufacturer, ref shopCategories);

            return list;
        }

        private static List<Product> ReadWortmann(string file1, string file2, Config config, ref Config shopConfig, ref List<Manufacturer> shopManufacturer, ref List<Categorie> shopCategories)
        {
            List<Product> list = new();
            string erlaubteHersteller = "WORTMANN AG";
            // Erste CSV Datei.
            foreach (string row in File.ReadAllLines(file1))
            {
                var rowSplit = row.Split(config.Trennzeichen);
                string[] categories = rowSplit[30].Split("|");
                // Prüfe, das nur Wortmann verarbeitet wird.
                if (rowSplit[3].Trim() != erlaubteHersteller) { continue; }
                // Prüfe auf Ignorierte Artikelnummern
                if(config.IgnoredItems != null)
                    if (config.IgnoredItems.Contains(rowSplit[0])) { continue; }
                if(config.IgnoredCategories != null)
                {
                    bool abbrechen = false;
                    foreach(string category in categories)
                    {
                        if(config.IgnoredCategories.Contains(category.Trim())) { abbrechen = true; }
                    }
                    if(abbrechen) { continue; }
                }
                /*
                 * 0 => Artikelnummer
                 * 1 => Herstellernummer
                 * 2 => EAN
                 * 3 => Hersteller
                 * 4 => EK
                 * 8 => UVP netto
                 * 9 => UVP Brutto
                 * 13 => Bestand
                 * 21 => Produktbild
                 * 22 => Alle anderen Bilder
                 * 24 => Gewicht
                 * 28 => Artikelname DE
                 * 30 => Kategorie Path
                 */
                Product tmp = new()
                {
                    Model = config.Prefix + rowSplit[0],
                    ManufacturersModel = rowSplit[1],
                    Ean = rowSplit[2],
                    Quantity = Convert.ToInt32(rowSplit[13]),
                    ProductImage = rowSplit[21],
                    Weight = Convert.ToDecimal(rowSplit[24]),
                    Name = rowSplit[28],
                };
                List<Categorie> allKats = new();
                tmp.CategoriesId = OverloadedMethdos.GetCategoryId(categories, config, ref shopCategories, shopConfig, ref allKats);
                tmp.SetStatus();
                tmp.ManufacturerId = GetManufacturerId(tmp, tmp.ManufacturersModel, ref shopManufacturer, ref shopConfig);
                if(config.EigenerAufschlag == null) {
                    if(config.PreisStandard == "brutto")
                        tmp.Price = Convert.ToDecimal(rowSplit[9]);
                    if (config.PreisStandard == "netto")
                        tmp.Price = Convert.ToDecimal(rowSplit[8]);
                }
                else
                {
                    tmp.CalculatePrice(Convert.ToDecimal(rowSplit[4]),config, allKats);
                }
                tmp.Images = rowSplit[22];

                list.Add(tmp);
            }
            // Zweite CSV Datei.
            foreach(string row in File.ReadAllLines(file2))
            {
                var splitRow = row.Split(config.Trennzeichen);
                /*
                 * 0 => Artikelnummer
                 * 1 => PrintText
                 * 5 => Beschreibung
                 */
                int index = list.FindIndex(x => x.Model == config.Prefix + splitRow[0]);
                if(index > 0)
                {
                    list[index].ShortDiscription = splitRow[1];
                    list[index].Description = splitRow[5];
                }
            }
            return list;
        }

        private static List<Product> ReadApi(string filename, Config config, ref Config shopConfig, ref List<Manufacturer> shopManufacturers, ref List<Categorie> shopCategories)
        {
            List<Product> list = new();
            string[] erlaubteHersteller = { "Ultron", "Ultron PCs", "Rasurbo", "Terratec", "Nanoxia", "Cooltek", "Thermalright", "Realpower" };
            foreach (string row in File.ReadAllLines(filename))
            {
                var splitRow = row.Split(config.Trennzeichen);
                // Prüfen auf unerlaubte Hersteller oder durch config ignorierte Artikelnummern / Kategorien
                if (erlaubteHersteller.Contains(splitRow[3])) { continue; }
                if (config.IgnoredCategories != null)
                {
                    if (config.IgnoredCategories.Contains(splitRow[12])) { continue; }
                    if (config.IgnoredCategories.Contains(splitRow[13])) { continue; }
                }
                if (config.IgnoredItems != null)
                    if (config.IgnoredItems.Contains(splitRow[0])) { continue; }
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
                tmp.ManufacturerId = GetManufacturerId(tmp, hersteller, ref shopManufacturers, ref shopConfig);

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

        private static List<Product> ReadKosatec(string filename, Config config, ref Config shopConfig, ref List<Manufacturer> shopManufacturers, ref List<Categorie> shopCategories)
        {
            List<Product> list = new();
            foreach (string row in File.ReadAllLines(filename))
            {
                var splitRow = row.Split(config.Trennzeichen);
                // Prüfe auf Ignorierte Kategorien und Artikelnummern
                if (config.IgnoredCategories != null)
                {
                    if (splitRow[14] != "")
                        if (config.IgnoredCategories.Contains(splitRow[14]))
                            continue;
                    if (splitRow[15] != "")
                        if (config.IgnoredCategories.Contains(splitRow[15]))
                            continue;
                    if (splitRow[16] != "")
                        if (config.IgnoredCategories.Contains(splitRow[16]))
                            continue;
                    if (splitRow[18] != "")
                        if (config.IgnoredCategories.Contains(splitRow[17]))
                            continue;
                    if (splitRow[19] != "")
                        if (config.IgnoredCategories.Contains(splitRow[18]))
                            continue;
                }
                if (config.IgnoredItems != null)
                    if (splitRow[0] != "")
                        if (config.IgnoredItems.Contains(splitRow[0]))
                            continue;
                // Benötigte Indizies:
                /*
                 * 0 => Artikelnummer drin
                 * 1 => Herstellernummer drin
                 * 2 => Artikelname drin
                 * 3 => Hersteller
                 * 5 => EAN drin
                 * 6 => EK netto
                 * 9 => MEnge drin
                 * 12 => Gewicht drin
                 * 14 - 19 => Kategorien 1-6
                 * 21 => Short Desc drin
                 * 23 => Long Summary drin
                 * 24 => Marketing Text drin
                 * 25 => Specs drin
                 * 28 - 31 => Bilder (Semikolon getrennte Liste von Links) S/M/L/XL
                 */
                Product product = new()
                {
                    Model = config.Prefix + splitRow[0],
                    ManufacturersModel = splitRow[1],
                    Name = splitRow[2],
                    Ean = splitRow[5],
                    Quantity = Convert.ToInt32(splitRow[9]),
                    Weight = Convert.ToDecimal(splitRow[12]),
                    ShortDiscription = splitRow[21],
                    Description = splitRow[24] + "<br>" + splitRow[23] + "<br>" + splitRow[25],
                };
                product.SetStatus();
                string hersteller = splitRow[3].Trim();
                product.ManufacturerId = GetManufacturerId(product, hersteller, ref shopManufacturers, ref shopConfig);
                string[] categories = { splitRow[14].Trim(), splitRow[15].Trim(), splitRow[16].Trim(), splitRow[17].Trim(), splitRow[18].Trim(), splitRow[19].Trim() };
                List<Categorie> allCats = new();
                product.CategoriesId = OverloadedMethdos.GetCategoryId(categories, config, ref shopCategories, shopConfig, ref allCats);
                product.CalculatePrice(Convert.ToDecimal(splitRow[6]), config, allCats);

                // Prüfe auf Existierende Bilder, XL bevorzugt.
                if (splitRow[31] != "") // XL Bilder
                {
                    product.Images = splitRow[31];
                    var split = splitRow[31].Split(";");
                    product.ProductImage = Path.GetFileName(split[0]);

                }
                else if (splitRow[30] != "") // L Bilder
                {
                    product.Images = splitRow[30];
                    var split = splitRow[30].Split(";");
                    product.ProductImage = Path.GetFileName(split[0]);
                }
                else if (splitRow[29] != "") // M Bilder
                {
                    product.Images = splitRow[29];
                    var split = splitRow[29].Split(";");
                    product.ProductImage = Path.GetFileName(split[0]);
                }
                else if (splitRow[28] != "") // S Bilder
                {
                    product.Images = splitRow[28];
                    var split = splitRow[28].Split(";");
                    product.ProductImage = Path.GetFileName(split[0]);
                }
                list.Add(product);
            }
            return list;
        }


        private static int GetManufacturerId(Product product, string manufacturerName, ref List<Manufacturer> shopManufacturers, ref Config shopConfig)
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
    }
}
