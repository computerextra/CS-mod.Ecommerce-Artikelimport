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
    }
}
