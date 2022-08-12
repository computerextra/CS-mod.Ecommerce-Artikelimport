namespace CS_mod.Ecommerce_Artikelimport
{
    public class Images
    {
        // Table products_images
         public int? Id { get; set; } // Index 0
        public int? ProductsId { get; set; } // Index 1 (Zuweisung von Bild zu Product)
        public int? ImageNumber { get; set; } // Index 2 (Zahl 1-8 beziehungsweise max. Bilder aus Konfig -1)
        public string? Name { get; set; } // Index 3 (Dateiname ohne Pfad!)
    }
}
