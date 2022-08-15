using MySql.Data.MySqlClient;

namespace CS_mod.Ecommerce_Artikelimport
{
    public class Manufacturer
    {
        // Table manufacturers
        public int Id { get; set; } = 0; // Index 0
        public string Name { get; set; } = ""; // Index 1

        // Table manufacturers_info
        /*
         * Nimmt:
         * Index 0 => manufacturers_id
         * Index 1 => language_id (1 = en / 2 = de )
         */
        public string Description { get; set; } = "NULL"; // Index 2
        public string Url { get; set; } = ""; // Index 6



        public void GenerateNewManufacturer(Config config)
        {
            if(Id != 0){return;}

            // Neuen Lieferanten anlegen
            string query = "INSERT INTO manufacturers (`manufacturers_id`, `manufacturers_name`, `manufacturers_image`, `date_added`, " +
                "`last_modified`) VALUES(NULL,'" + Name + "', '', '" + GetDate() + "', NULL);";
            MySqlConnection conn = new("server=" + config.Server + ";user=" + config.User + ";password=" + config.Password + ";database=" + config.Database + ";");
            try
            {
                conn.Open();
                MySqlCommand cmd = new(query, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }catch(Exception ex) { Console.WriteLine(ex.Message); }
            // Neuen Lieferanten auslesen, damit ID gezogen werden kann!
            query = "SELECT * FROM manufacturers WHERE manufacturers_name = '" + Name + "';";
            try
            {
                MySqlCommand cmd = new(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Id = reader.GetInt32("manufacturers_id");
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            query = "INSERT INTO manufacturers_info (`manufacturers_id`, `languages_id`, `manufacturers_description`, " +
                "`manufacturers_meta_title`, `manufacturers_meta_description`, `manufacturers_meta_keywords`, `manufacturers_url`, " +
                "`url_clicked`, `date_last_click`) " +
                "VALUES ('"+Id+"','2','"+Description+"','','','','"+Url+"','0','NULL');";
            try
            {
                MySqlCommand cmd = new(query, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

        }

        private string GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }
    }
}