﻿namespace CS_mod.Ecommerce_Artikelimport
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
        
    }
}