using Bungie.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H2_H3_Converter_UI
{
    public class Utils
    {
        public static string ConvertXML(string xmlPath, Loading loadingForm)
        {
            Console.WriteLine("\nBeginning XML Conversion:\n");
            loadingForm.UpdateOutputBox("\nBeginning XML Conversion:\n", false);

            string newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "modified_input.xml");

            try
            {
                string[] lines = File.ReadAllLines(xmlPath);
                bool removeLines = false;

                using (StreamWriter writer = new StreamWriter(newFilePath))
                {
                    foreach (string line in lines)
                    {
                        if (line.Contains("<block name=\"source files\">"))
                        {
                            removeLines = true;
                            writer.WriteLine(line);
                        }
                        else if (line.Contains("<block name=\"scripting data\">"))
                        {
                            removeLines = false;
                        }
                        else if (!removeLines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                Console.WriteLine("Modified file saved successfully.\n\nPreparing to patch tag data:\n");
                loadingForm.UpdateOutputBox("Modified file saved successfully. Preparing to patch tag data:\n", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                loadingForm.UpdateOutputBox("An error occurred: " + ex.Message, false);
            }

            return newFilePath;
        }
    
        public static void BackupScenario(string scenPath, string xmlPath, Loading loadingForm)
        {
            // Create scenario backup
            string backup_folderpath = Path.GetDirectoryName(scenPath) + @"\scenario_backup";
            Directory.CreateDirectory(backup_folderpath);
            string backup_filepath = Path.Combine(backup_folderpath, scenPath.Split('\\').Last());

            if (!File.Exists(backup_filepath))
            {
                File.Copy(scenPath, backup_filepath);
                Console.WriteLine("Backup created successfully.");
                loadingForm.UpdateOutputBox("Backup created successfully.", false);
            }
            else
            {
                Console.WriteLine("Backup already exists.");
                loadingForm.UpdateOutputBox("Backup already exists.", false);
            }
        }
    
        public Dictionary<string, TagPath> characterMapping = new Dictionary<string, TagPath>()
        {
            // Marines
            { "objects\\characters\\marine\\ai\\marine", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine", "character") },
            { "objects\\characters\\marine\\ai\\marine_female", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_female", "character") },
            { "objects\\characters\\marine\\ai\\marine_johnson", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_johnson", "character") },
            { "objects\\characters\\marine\\ai\\marine_odst", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_odst", "character") },
            { "objects\\characters\\marine\\ai\\marine_odst_sgt", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_odst_sgt", "character") },
            { "objects\\characters\\marine\\ai\\marine_pilot", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_pilot", "character") },
            { "objects\\characters\\marine\\ai\\marine_sgt", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_sgt", "character") },
            { "objects\\characters\\marine\\ai\\marine_wounded", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_wounded", "character") },
            { "objects\\characters\\marine\\ai\\naval_officer", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_naval_officer", "character") },

            // Grunts
            { "objects\\characters\\grunt\\ai\\grunt", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 grunt", "character") },
            { "objects\\characters\\grunt\\ai\\grunt_heavy", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 grunt_heavy", "character") },
            { "objects\\characters\\grunt\\ai\\grunt_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 grunt_major", "character") },
            { "objects\\characters\\grunt\\ai\\grunt_specops", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 spec_ops", "character") },
            { "objects\\characters\\grunt\\ai\\grunt_ultra", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 grunt_ultra", "character") },

            // Jackals
            { "objects\\characters\\jackal\\ai\\jackal", TagPath.FromPathAndExtension("h2\\objects\\characters\\jackal\\ai\\h2_jackal", "character") },
            { "objects\\characters\\jackal\\ai\\jackal_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\jackal\\ai\\h2_jackal_major", "character") },
            { "objects\\characters\\jackal\\ai\\jackal_sniper", TagPath.FromPathAndExtension("h2\\objects\\characters\\jackal\\ai\\h2_jackal_sniper", "character") },

            // Bugger
            { "objects\\characters\\bugger\\ai\\bugger", TagPath.FromPathAndExtension("h2\\objects\\characters\\bugger\\ai\\h2_bugger", "character") },

            // Hunter
            { "objects\\characters\\hunter\\ai\\hunter", TagPath.FromPathAndExtension("h2\\objects\\characters\\hunter\\ai\\h2_hunter", "character") },

            // Elites
            { "objects\\characters\\elite\\ai\\elite", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\h2 elite", "character") },
            { "objects\\characters\\elite\\ai\\elite_councilor", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_councilor", "character") },
            { "objects\\characters\\elite\\ai\\elite_honor_guard", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_honor_guard", "character") },
            { "objects\\characters\\elite\\ai\\elite_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\h2 elite_major", "character") },
            { "objects\\characters\\elite\\ai\\elite_ranger", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_ranger", "character") },
            { "objects\\characters\\elite\\ai\\elite_specops", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\h2 elite_specops", "character") },
            { "objects\\characters\\elite\\ai\\elite_specops_commander", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_specops_commander", "character") },
            { "objects\\characters\\elite\\ai\\elite_stealth", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_stealth", "character") },
            { "objects\\characters\\elite\\ai\\elite_stealth_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_stealth_major", "character") },
            { "objects\\characters\\elite\\ai\\elite_ultra", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_ultra", "character") },
            { "objects\\characters\\elite\\ai\\elite_zealot", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_zealot", "character") },

            // Brutes
            { "objects\\characters\\brute\\ai\\brute", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\h2 brute", "character") },
            { "objects\\characters\\brute\\ai\\brute_captain", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\h2 brute_captain", "character") },
            { "objects\\characters\\brute\\ai\\brute_honor_guard", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\h2 brute_honor_guard", "character") },
            { "objects\\characters\\brute\\ai\\brute_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\h2 brute_major", "character") },
            { "objects\\characters\\brute\\ai\\brute_tartarus", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\tartarus", "character") },

            // Sentinels
            { "objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor", "character") },
            { "objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor_eliminator", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor", "character") },
            { "objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor_halo1", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor", "character") },
            { "objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor_major", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor_major", "character") },

            // Flood
            { "objects\\characters\\floodcombat_elite\\ai\\floodcombat_elite", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\floodcombat_elite_shush\\ai\\floodcombat_elite_shush", "character") },
            { "objects\\characters\\floodcombat_elite\\ai\\floodcombat_elite_shielded", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\floodcombat_elite_shush\\ai\\floodcombat_elite_shielded_shush", "character") },
            { "objects\\characters\\floodcarrier\\ai\\flood_carrier", TagPath.FromPathAndExtension("objects\\characters\\floodcarrier\\ai\\flood_carrier", "character") },
            { "objects\\characters\\flood_infection\\ai\\flood_infection", TagPath.FromPathAndExtension("objects\\characters\\flood_infection\\ai\\flood_infection", "character") },
            { "objects\\characters\\floodcombat_human\\ai\\flood_combat_human", TagPath.FromPathAndExtension("objects\\characters\\floodcombat_human\\ai\\flood_combat_human", "character") },
        
            // HERETICS, HERETICS!
            { "objects\\characters\\heretic\\ai\\heretic", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\heretic\\ai\\heretic", "character") },
            { "objects\\characters\\heretic\\ai\\heretic_leader", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\heretic\\ai\\heretic_leader", "character") },
            { "objects\\characters\\heretic\\ai\\heretic_leader_hologram", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\heretic\\ai\\heretic_leader_hologram", "character") },
            { "objects\\characters\\heretic\\ai\\heretic_major", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\heretic\\ai\\heretic_major", "character") },
            { "objects\\characters\\heretic_grunt\\ai\\heretic_grunt", TagPath.FromPathAndExtension("objects\\characters\\heretic_grunt\\ai\\heretic_grunt", "character") },
        };
    }
}
