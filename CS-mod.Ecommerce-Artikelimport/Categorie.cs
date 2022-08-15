namespace CS_mod.Ecommerce_Artikelimport
{
    public class Categorie
    {

        // Table categories
        public int? Id { get; set; } // Index 0
        public int? ParentId { get; set; } // Index 2 (ID von Parent Categorie / Root = 0 )
        public int? Status { get; set; } // Index 3 (0 / 1) Boolean

        // Table categories_description
        /*
         * Nimmt:
         * Index 0 => categories_id
         * Index 1 => language_id (1 = en / 2 = de )
         */
        public string Name { get; set; } = ""; // Index 2

    }
}
