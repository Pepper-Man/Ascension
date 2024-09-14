using Bungie;
using Bungie.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace H2_H3_Converter_UI
{
    class JumpHint
    {
        public string Flags { get; set; }
        public string ParallelIndex { get; set; }
        public string JumpHeight { get; set; }
    }

    class Parallelogram
    {
        public string Flags { get; set; }
        public string Point0 { get; set; }
        public string Point0Ref { get; set; }
        public string Point1 { get; set; }
        public string Point1Ref { get; set; }
        public string Point2 { get; set; }
        public string Point2Ref { get; set; }
        public string Point3 { get; set; }
        public string Point3Ref { get; set; }
    }

    class BspHints
    {
        public List<JumpHint> jumpHints { get; set; }
    }

    class BspParallelos
    {
        public List<Parallelogram> parallelograms { get; set;}
    }

    class ScenarioHints
    {
        public List<BspHints> scenarioHints { get; set; }
    }

    class ScenarioParallelos
    {
        public List<BspParallelos> scenarioParallelos { get; set; }
    }

    public class HintConverter
    {
        public static void JumpHintsToXML(string scenPath, string xmlPath, Loading loadingForm)
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

            string newFilePath = Convert_XML(xmlPath, loadingForm);
            XmlDocument scenfile = new XmlDocument();
            scenfile.Load(newFilePath);

            loadingForm.UpdateOutputBox("Modified XML file loaded successfully.", false);

            XmlNode root = scenfile.DocumentElement;

            XmlNodeList aiPathDataBlock = root.SelectNodes(".//block[@name='ai pathfinding data']");
            loadingForm.UpdateOutputBox("Located AI pathfinding data block.", false);

            ScenarioHints scenarioHints = new ScenarioHints();
            ScenarioParallelos scenarioParallelos = new ScenarioParallelos();
            List<BspHints> bspsH = new List<BspHints>();
            List<BspParallelos> bspsP = new List<BspParallelos>();
            foreach (XmlNode aiPathfinding in aiPathDataBlock)
            {
                // Each BSP has its own element
                bool pathDataEnd = false;
                int i = 0;
                while (!pathDataEnd)
                {
                    XmlNode bsp = aiPathfinding.SelectSingleNode("./element[@index='" + i + "']");
                    if (bsp != null)
                    {
                        loadingForm.UpdateOutputBox($"Reading pathfinding data for BSP index {i}.", false);
                        BspHints bspHints = new BspHints();
                        List<JumpHint> jumpHints = new List<JumpHint>();
                        XmlNode jumpHintsBlock = bsp.SelectSingleNode(".//block[@name='user-placed hints']/element[@index='0']/block[@name='jump hints']");

                        // Loop over all jump hints
                        if (jumpHintsBlock != null)
                        {
                            int j = 0;
                            XmlNodeList jumpHintElements = jumpHintsBlock.SelectNodes("./element");
                            foreach (XmlNode jumpHint in jumpHintElements)
                            {
                                JumpHint hint = new JumpHint();
                                hint.Flags = jumpHint.SelectSingleNode("./field[@name='Flags']").InnerText.Trim();
                                hint.ParallelIndex = jumpHint.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"]?.Value;
                                hint.JumpHeight = jumpHint.SelectSingleNode("./field[@name='force jump height']").InnerText.Trim();
                                jumpHints.Add(hint);
                                loadingForm.UpdateOutputBox($"Read data for BSP {i}, jump hint {j}.", false);
                                j++;
                            }
                            bspHints.jumpHints = jumpHints;
                        }

                        BspParallelos bspParallelos = new BspParallelos();
                        List<Parallelogram> parallelograms = new List<Parallelogram>();
                        XmlNode parallelosBlock = bsp.SelectSingleNode(".//block[@name='user-placed hints']/element[@index='0']/block[@name='parallelogram geometry']");

                        // Loop over all parallelogram geometry data
                        if (parallelosBlock != null)
                        {
                            int j = 0;
                            XmlNodeList parallelogramElements = parallelosBlock.SelectNodes("./element");
                            foreach (XmlNode para in parallelogramElements)
                            {
                                Parallelogram parallelogram = new Parallelogram();
                                XmlNodeList referenceFields = para.SelectNodes("./field[@name='reference frame']");

                                parallelogram.Flags = para.SelectSingleNode("./field[@name='Flags']").InnerText.Trim();
                                parallelogram.Point0 = para.SelectSingleNode("./field[@name='Point 0']").InnerText.Trim();
                                parallelogram.Point0Ref = referenceFields[0].InnerText.Trim();
                                parallelogram.Point1 = para.SelectSingleNode("./field[@name='Point 1']").InnerText.Trim();
                                parallelogram.Point1Ref = referenceFields[1].InnerText.Trim();
                                parallelogram.Point2 = para.SelectSingleNode("./field[@name='Point 2']").InnerText.Trim();
                                parallelogram.Point2Ref = referenceFields[2].InnerText.Trim();
                                parallelogram.Point3 = para.SelectSingleNode("./field[@name='Point 3']").InnerText.Trim();
                                parallelogram.Point3Ref = referenceFields[3].InnerText.Trim();

                                parallelograms.Add(parallelogram);
                                loadingForm.UpdateOutputBox($"Read data for BSP {i}, parallelogram {j}.", false);
                                j++;
                            }
                            bspParallelos.parallelograms = parallelograms;
                        }

                        bspsH.Add(bspHints);
                        bspsP.Add(bspParallelos);
                        loadingForm.UpdateOutputBox($"Successfully read hint data for BSP index {i}.", false);
                        i++;
                    }
                    else
                    {
                        pathDataEnd = true;
                        loadingForm.UpdateOutputBox("Finished processing bsp pathfinding data.", false);
                    }
                }
            }
            scenarioHints.scenarioHints = bspsH;
            scenarioParallelos.scenarioParallelos = bspsP;
            loadingForm.UpdateOutputBox($"Successfully read hint data for all BSPs.", false);
            JumpHintstoH3(scenPath, loadingForm);
        }

        static string Convert_XML(string xmlPath, Loading loadingForm)
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
                loadingForm.UpdateOutputBox("Modified file saved successfully.\n\nPreparing to patch tag data:\n", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                loadingForm.UpdateOutputBox("An error occurred: " + ex.Message, false);
            }

            return newFilePath;
        }
    
        public static void JumpHintstoH3(string scenPath, Loading loadingForm)
        {
            string h3ek_path = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
            var relativeScenPath = TagPath.FromPathAndType(Path.ChangeExtension(scenPath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
            TagFile scenTag = new TagFile();

            try
            {
                scenTag.Load(relativeScenPath);
                loadingForm.UpdateOutputBox($"Successfully opened \"{relativeScenPath}\"", false);
            }
            catch
            {
                loadingForm.UpdateOutputBox($"Unknown managedblam error", false);
                return;
            }
            finally
            {
                scenTag.Save();
                scenTag.Dispose();
                loadingForm.UpdateOutputBox($"Finished writing data to scenario tag!", false);
            }
        }
    }
}
