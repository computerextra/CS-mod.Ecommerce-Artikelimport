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
    }
}
