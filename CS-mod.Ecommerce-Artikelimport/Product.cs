using System.Data.Odbc;

namespace CS_mod.Ecommerce_Artikelimport
{
    public class Product
    {
        // Table products
        public int? Id { get; set; } // Index 0
        public string? Ean { get; set; } // Index 1
        public int? Quantity { get; set; } // Index 2
        public string? Model { get; set; } // Index 4 (Artikelnummer)
        public int? Sort { get; set; } // Index 10
        public string? ProductImage { get; set; } // Index 12
        public decimal? Price { get; set; } // Index 13
        public decimal? Weight { get; set; } // Index 17
        public int? Status { get; set; } // Index 18
        public int? ManufacturerId { get; set; } // Index 22
        public string? ManufacturersModel { get; set; } // Index 23 (Hersteller Nummer)

        // Table products_description
        /*
         * Nimmt:
         * Index 0 => products_id
         * Index 1 => language_id (1 = en / 2 = de )
         */
        public string? Name { get; set; } // Index 2
        public string? Description { get; set; } // Index 4
        public string? ShortDiscription { get; set; } // Index 5

        // Table products_to_categories
        /*
         * Nimmt die ID von Products.Id als Index 0
         */
        public int? CategoriesId { get; set; } // Index 1

        // Für Bilder
        public string? Images {get; set;} // String für Alle Bilder, die genutzt werden sollen. ; Separiert. (Bei Wortmann jedoch mit |)

        public void CalculatePrice( decimal ek, Config config, Categorie categorie )
        {
            decimal preis;
            // Nimmt den hinterlegten EK (Intos / Kosatec / API) || Wortmann hat bereits Brutto UVPs in der Liste, diese werden direkt genuztzt.
            // True = Prozentual // false = Wert
            if (config.Aufschlagsart)
                preis = decimal.Multiply(ek, Convert.ToDecimal(config.Preisaufschlag));
            else
                preis = decimal.Add(ek, Convert.ToDecimal(config.Preisaufschlag));
            // Auf "Custom Aufschlag" prüfen. Den Std. Preis von oben wieder überschreiben, wird nicht mehr benötigt.
            if (config.EigenerAufschlag != null && config.EigenerAufschlag.Any())
            {
                config.EigenerAufschlag.ForEach(row =>
                {
                    var splitRow = row.Split(",");
                    if (splitRow[0] == categorie.Name)
                    {
                        if (splitRow[2] == "0") { preis = Decimal.Add(ek, Convert.ToDecimal(row[1])); }
                        if (splitRow[2] == "1") { preis = Decimal.Multiply(ek, Convert.ToDecimal(row[1])); }
                    } 
                });
            }
            // Rechne MwSt drauf, damit danach auf einen runden Preis gerechnet werden kann.
            preis = decimal.Multiply(preis, Convert.ToDecimal(config.MwSt));
            // Preisrundung || Std auf ,90 € runden in 5er Schritten (z.B. 4,90; 9,90; 14,90 etc.)
            preis = decimal.Round(preis, MidpointRounding.AwayFromZero);
            // Decimal Preis in Integer kovertieren, damit auf eine Ganze Zahl (in 5er Schritten) gekommen wird.
            preis = Convert.ToInt32(Math.Ceiling(preis / 5)) * 5;
            // 10ct abziehen um auf .90 zu kommen.
            preis -= 0.1m;
            // Gebe entweder den Netto oder den Bruttopreis zurück. Speichern direkt im Artikel.
            Price =  GetNettoPrice(preis, config);
        }
        public void CalculatePrice(decimal ek, Config config, List<Categorie> categorie)
        {
            decimal preis;
            // Nimmt den hinterlegten EK (Intos / Kosatec / API) || Wortmann hat bereits Brutto UVPs in der Liste, diese werden direkt genuztzt.
            // True = Prozentual // false = Wert
            if (config.Aufschlagsart)
                preis = decimal.Multiply(ek, Convert.ToDecimal(config.Preisaufschlag));
            else
                preis = decimal.Add(ek, Convert.ToDecimal(config.Preisaufschlag));
            // Auf "Custom Aufschlag" prüfen. Den Std. Preis von oben wieder überschreiben, wird nicht mehr benötigt.
            if (config.EigenerAufschlag != null && config.EigenerAufschlag.Any())
            {
                config.EigenerAufschlag.ForEach(row =>
                {
                    var splitRow = row.Split(",");
                    if (categorie.Contains(new Categorie() { Name = splitRow[1] }))
                    {
                        if (splitRow[2] == "0") { preis = Decimal.Add(ek, Convert.ToDecimal(row[1])); }
                        if (splitRow[2] == "1") { preis = Decimal.Multiply(ek, Convert.ToDecimal(row[1])); }
                    }
                });
            }
            // Rechne MwSt drauf, damit danach auf einen runden Preis gerechnet werden kann.
            preis = decimal.Multiply(preis, Convert.ToDecimal(config.MwSt));
            // Preisrundung || Std auf ,90 € runden in 5er Schritten (z.B. 4,90; 9,90; 14,90 etc.)
            preis = decimal.Round(preis, MidpointRounding.AwayFromZero);
            // Decimal Preis in Integer kovertieren, damit auf eine Ganze Zahl (in 5er Schritten) gekommen wird.
            preis = Convert.ToInt32(Math.Ceiling(preis / 5)) * 5;
            // 10ct abziehen um auf .90 zu kommen.
            preis -= 0.1m;
            // Gebe entweder den Netto oder den Bruttopreis zurück. Speichern direkt im Artikel.
            Price = GetNettoPrice(preis, config);
        }

        private decimal GetNettoPrice(decimal preis, Config config)
        {
            // Nimmt ein hinterlegten Brutto VK und erstellt einen netto VK daraus, wenn in der Config angegeben wurde, dass der Std. im Shop Netto ist.

            if (config.PreisStandard == "netto")
                return Decimal.Divide(preis, Convert.ToDecimal(config.MwSt));
            else
                return preis;
        }
        public void SetStatus()
        {
            if(Quantity > 0)
                Status = 1;
            else
                Status = 0;
        }

        public static List<Product> ReadSage(Config config, ref Config shopConfig, ref List<Manufacturer> shopManufacturer, ref List<Categorie> shopCategories)
        {
            List<Product> list = new();
            if(!config.IsUsed){Console.WriteLine("Laut Config ungenutzt, wird übersprungen."); return list;}
            // Connect 2 SAGE
            string connectionString = config.GetConnectionString();
            if(connectionString == ""){
                Console.WriteLine("Fehler in der Config, bitte kontrollieren."); return list;
            }
            using OdbcConnection conn = new(connectionString);
            try{
                conn.Open();
                string query = "SELECT * FROM sg_auf_artikel;";
                OdbcCommand cmd = new(query, conn);
                OdbcDataReader reader = cmd.ExecuteReader();
                while(reader.Read()){
                    // 1 => Artikelnummer
                    // 2 => Name
                    // 12 => Bestand
                    // 58 => EAN
                    Product tmp = new(){
                        Id = reader.GetInt32(0),
                        Model = reader.GetString(1),
                        Name = reader.GetString(2),
                        Quantity = Convert.ToInt32(reader.GetFloat(12)),
                        Ean = reader.GetString(58)
                    };
                    list.Add(tmp);
                }
                reader.Close();
                cmd.Dispose();
            }catch(Exception ex){
                Console.WriteLine(ex.Message);
            }
            list.ForEach(item => {
                try{
                    string query = "SELECT PR01 FROM sg_auf_vkpreis WHERE sg_auf_artikel_fk=" + item.Id;
                    OdbcCommand cmd = new(query, conn);
                    OdbcDataReader reader = cmd.ExecuteReader();
                    while(reader.Read()){
                        if(!DBNull.Value.Equals(reader[0]))
                            item.Price = Convert.ToDecimal(reader.GetDouble(0));
                    }
                    reader.Close();
                    cmd.Dispose();
                }catch(Exception ex){Console.WriteLine(ex.Message);}
            });
            conn.Close();
            return list;
        }
    }
}
