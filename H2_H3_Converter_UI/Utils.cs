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
    }
}
