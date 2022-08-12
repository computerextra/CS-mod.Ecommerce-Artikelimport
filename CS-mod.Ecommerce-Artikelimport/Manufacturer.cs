namespace CS_mod.Ecommerce_Artikelimport
{
    public class Manufacturer
    {
        // Table manufacturers
        public int? Id { get; set; } // Index 0
        public string? Name { get; set; } // Index 1

        // Table manufacturers_info
        /*
         * Nimmt:
         * Index 0 => manufacturers_id
         * Index 1 => language_id (1 = en / 2 = de )
         */
        public string? Description { get; set; } = "NULL"; // Index 2
        public string? Url { get; set; } = ""; // Index 6

    }
}
