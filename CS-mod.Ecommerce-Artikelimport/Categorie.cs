using MySql.Data.MySqlClient;

namespace CS_mod.Ecommerce_Artikelimport
{
    public class Categorie
    {

        // Table categories
        public int Id { get; set; } = 0; // Index 0
        public int ParentId { get; set; } = 0; // Index 2 (ID von Parent Categorie / Root = 0 )
        public int Status { get; set; } = 0; // Index 3 (0 / 1) Boolean

        // Table categories_description
        /*
         * Nimmt:
         * Index 0 => categories_id
         * Index 1 => language_id (1 = en / 2 = de )
         */
        public string Name { get; set; } = ""; // Index 2


        public void GenerateNewCategorie(Config config)
        {
            if(Id != 0) { return; }

            // Neue Kategorie anlegen
            string date = GetDate();
            string query = "INSERT INTO categories (`categories_id`,`categories_image`,`categories_image_mobile`,`categories_image_list`," +
                "`parent_id`,`categories_status`,`categories_template`,`group_permission_0`,`group_permission_1`,`group_permission_2`," +
                "`group_permission_3`,`group_permission_4`,`listing_template`,`sort_order`,`products_sorting`,`products_sorting2`," +
                "`date_added`,`last_modified`) " +
                "VALUES('NULL','','','','"+ParentId+"','"+Status+ "','','','','','','','','0','NULL','NULL','"+date+"','NULL');";
            MySqlConnection conn = new("server=" + config.Server + ";user=" + config.User + ";password=" + config.Password + ";database=" + config.Database + ";");
            try
            {
                conn.Open();
                MySqlCommand cmd = new(query, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            query = "SELECT * FROM categories WHERE parent_id='" + ParentId + "' AND date_added='" + date + "';";
            try
            {
                MySqlCommand cmd = new(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Id = reader.GetInt32("categories_id");
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            query = "INSERT INTO categories_description (`categories_id`,`language_id`,`categories_name`,`categories_heading_title`," +
                "`categories_description`,`categories_meta_title`,`categories_meta_description`,`categories_meta_keywords`) " +
                "VALUES('"+Id+"','2','"+Name+"','','','','','',);";
            try
            {
                conn.Open();
                MySqlCommand cmd = new(query, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            conn.Close();
        }

        private string GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }
    }
}
