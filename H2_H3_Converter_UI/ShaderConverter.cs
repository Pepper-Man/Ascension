using System;
using Bungie;
using System.IO;
using System.Xml;
using Bungie.Tags;
using System.Linq;
using ImageMagick;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bungie.Game;
using System.Text;

namespace H2_H3_Converter_UI
{
    class Shader
    {
        public string name { get; set; }
        public string glob_mat { get; set; }
        public string template { get; set; }
        public List<Parameter> parameters { get; set; }
        public string spec_col { get; set; }
        public string spec_glnc { get; set; }
        public string env_tint { get; set; }
        public string env_glnc { get; set; }
        public string spec_type { get; set; }
        public string env_bitmap { get; set; }
    }

    class Parameter
    {
        public string name { get; set; }
        public string type { get; set; }
        public string bitmap { get; set; }
        public string value { get; set; }
        public string colour { get; set; }
        public sbyte scalex_1 { get; set; }
        public byte scalex_2 { get; set; }
        public sbyte scaley_1 { get; set; }
        public byte scaley_2 { get; set; }
        public byte[] scaley { get; set; }
    }

    class BitmapData
    {
        public string bitmap { get; set; }
        public string type { get; set; }
        public string compr { get; set; }
        public string fade { get; set; }
        public string bmp_hgt { get; set; }
    }

    class CookSettings
    {
        public float spec_coeff { get; set; }
        public float roughness { get; set; }
        public float area_contr { get; set; }
        public float anal_contr { get; set; }
        public float env_contr { get; set; }
    }

    public class ShaderConverter
    {
        /*
        Okay so, perhaps passing the secondary form to every function here is a terrible way to do this.
        But I tried many alternatives and determined that idk enough about asynchronous functions and UI blockers
        to do this in a better way.
        Each function gets passed the secondary loading form and appends its own output to it, thus mirroring the
        console output to the user.
        Pretty much everything else except the way in which tool output is captured is identical to the standalone
        version of the program.
        */
        public static async Task ConvertShaders(List<string> bsp_paths, string h3_scen, bool use_existing_bitmaps, Loading loadingForm)
        {
            string existing_bitmaps = "";

            Console.WriteLine("H2 to H3 Shader Converter by PepperMan\n\n");
            loadingForm.UpdateOutputBox("H2 to H3 Shader Converter by PepperMan\n\n", false);

            if (use_existing_bitmaps)
            {
                existing_bitmaps = Path.GetDirectoryName(h3_scen.Replace("tags", "data")) + "\\bitmaps";
                if (!Directory.Exists(existing_bitmaps))
                {
                    // Folder doesn't actually exist, ignore user request
                    Console.WriteLine("No existing data bitmap folder detected.");
                    loadingForm.UpdateOutputBox("No existing data bitmap folder detected.", false);
                    existing_bitmaps = "";
                }
            }
            
            // Temporary hardcoding for quick debugging
            //bsp_paths.Add(@"C:\Program Files (x86)\Steam\steamapps\common\H2EK\tags\scenarios\solo\03a_oldmombasa\earthcity_1.xml");
            //bsp_paths.Add(@"C:\Program Files (x86)\Steam\steamapps\common\H2EK\tags\scenarios\solo\03a_oldmombasa\earthcity_2.xml");
            //bsp_paths.Add(@"C:\Program Files (x86)\Steam\steamapps\common\H2EK\tags\scenarios\solo\03a_oldmombasa\earthcity_3.xml");
            //h3_scen = @"C:\Program Files (x86)\Steam\steamapps\common\H3EK\tags\halo_2\levels\singleplayer\oldmombasa\oldmombasa.scenario";
            //existing_bitmaps = @"C:\Program Files (x86)\Steam\steamapps\common\H3EK\data\halo_2\levels\singleplayer\oldmombasa\bitmaps";

            string bitmaps_dir = (h3_scen.Substring(0, h3_scen.LastIndexOf('\\')) + "\\bitmaps").Replace("tags", "data");
            string h2ek_path = bsp_paths[0].Substring(0, bsp_paths[0].IndexOf("H2EK") + "H2EK".Length);
            string h3ek_path = h3_scen.Substring(0, h3_scen.IndexOf("H3EK") + "H3EK".Length);
            List<string> all_h2_shader_paths = new List<string>();

            foreach (string bsp in bsp_paths)
            {
                List<string> bsp_shader_paths = GetShaders(bsp, loadingForm);
                foreach (string path in bsp_shader_paths)
                {
                    string full_path = h2ek_path + @"\tags\" + path + ".shader";
                    if (!all_h2_shader_paths.Contains(full_path))
                    {
                        all_h2_shader_paths.Add(full_path);
                    }
                }
            }

            // Create shader xml export folder
            string xml_output_path = Path.Combine(Directory.GetCurrentDirectory(), "shader_xml");
            if (!Directory.Exists(xml_output_path))
            {
                Directory.CreateDirectory(xml_output_path);
            }
            else
            {
                // Delete existing XML files
                string[] xmlFiles = Directory.GetFiles(xml_output_path, "*.xml");
                foreach (string xmlFile in xmlFiles)
                {
                    File.Delete(xmlFile);
                }
            }

            // Create bitmap tga export folder
            string tga_output_path = Path.Combine(Directory.GetCurrentDirectory(), "textures_output");

            if (!Directory.Exists(tga_output_path))
            {
                Directory.CreateDirectory(tga_output_path);
            }
            else
            {
                // Delete existing TGA/TIF files
                string[] tgaFiles = Directory.GetFiles(tga_output_path, "*.tga");
                string[] tifFiles = Directory.GetFiles(tga_output_path, "*.tif");
                foreach (string tgaFile in tgaFiles)
                {
                    File.Delete(tgaFile);
                }
                foreach (string tifFile in tifFiles)
                {
                    File.Delete(tifFile);
                }
            }

            // Create bitmap xml folder
            if (!Directory.Exists(tga_output_path.Replace("textures_output", "bitmap_xml")))
            {
                Directory.CreateDirectory(tga_output_path.Replace("textures_output", "bitmap_xml"));
            }
            else
            {
                // Delete existing XML files
                string[] xmlFiles = Directory.GetFiles(tga_output_path.Replace("textures_output", "bitmap_xml"), "*.xml");
                foreach (string xmlFile in xmlFiles)
                {
                    File.Delete(xmlFile);
                }
            }

            Console.WriteLine("\nBeginning .shader to .xml conversion...\nPlease wait...");
            loadingForm.UpdateOutputBox("\nBeginning .shader to .xml conversion...\nPlease wait...", false);
            ShaderExtractor(all_h2_shader_paths, h2ek_path, xml_output_path, loadingForm);
            Console.WriteLine("\nAll shaders converted to XML!\n\nGrabbing all referenced bitmap paths:\n");
            loadingForm.UpdateOutputBox("\nAll shaders converted to XML!\n\nGrabbing all referenced bitmap paths:\n", false);
            List<Shader> all_shader_data = GetShaderData(xml_output_path);
            List<string> all_bitmap_refs = new List<string>();

            foreach (Shader shader in all_shader_data)
            {
                foreach (Parameter param in shader.parameters)
                {
                    if (!String.IsNullOrEmpty(param.bitmap))
                    {
                        if (!all_bitmap_refs.Contains(param.bitmap))
                        {
                            all_bitmap_refs.Add(param.bitmap);
                            Console.WriteLine(param.bitmap);
                        }
                    }
                }
            }
            if (existing_bitmaps == "")
            {
                // Grab all bitmaps
                Console.WriteLine("\nObtained all referenced bitmaps!\n\nExtracting bitmap tags to TGA...");
                loadingForm.UpdateOutputBox("\nObtained all referenced bitmaps!\n\nExtracting bitmap tags to TGA...", false);
                Task task = ExtractBitmaps(all_bitmap_refs, h2ek_path, tga_output_path, loadingForm);
                await WaitForTaskCompletion(task);
                Console.WriteLine("\nExtracted all bitmaps to .TGA\nRunning .TIF conversion process...");
                loadingForm.UpdateOutputBox("\nExtracted all bitmaps to .TGA\nRunning .TIF conversion process...", false);
            }
            else
            {
                // Existing bitmaps folder has been provided, check if any are missing
                string[] existing_files = Directory.GetFiles(existing_bitmaps, "*.tif").Select(Path.GetFileNameWithoutExtension).ToArray();
                List<string> missing_files = all_bitmap_refs
                .Where(refName => !existing_files.Any(existFile => refName.Contains(existFile)))
                .ToList();

                // Grab missing bitmaps
                Console.WriteLine("\nObtained all referenced bitmaps!\n\nExtracting any missing bitmaps to TGA...");
                loadingForm.UpdateOutputBox("\nObtained all referenced bitmaps!\n\nExtracting any missing bitmaps to TGA...", false);
                Task task = ExtractBitmaps(missing_files, h2ek_path, tga_output_path, loadingForm);
                await WaitForTaskCompletion(task);
                Console.WriteLine("\nExtracted all missing bitmaps to .TGA\nRunning .TIF conversion process...");
                loadingForm.UpdateOutputBox("\nExtracted all missing bitmaps to .TGA\nRunning .TIF conversion process...", false);
            }

            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
            string[] errors = TGAToTIF(tga_output_path, bitmaps_dir, h3ek_path, loadingForm);
            Console.WriteLine("\nFinished importing bitmaps into H3.\nCreating H3 shader tags...");
            loadingForm.UpdateOutputBox("\nFinished importing bitmaps into H3.\nCreating H3 shader tags...", false);
            MakeShaderTags(all_shader_data, bitmaps_dir, h3ek_path, loadingForm);
            Console.WriteLine("\nSuccessfully created all shader tags.");
            loadingForm.UpdateOutputBox("\nSuccessfully created all shader tags.", false);
            if (errors.Count() != 0)
            {
                Console.WriteLine("The following errors were caught:\n");
                loadingForm.UpdateOutputBox("The following errors were caught:\n", false);
                foreach (string bitmap_issue in errors)
                {
                    Console.WriteLine(bitmap_issue);
                    loadingForm.UpdateOutputBox(bitmap_issue, true);
                }
            }
            Console.WriteLine("\nFinished converting shaders!");
            loadingForm.UpdateOutputBox("\nFinished converting shaders!", false);
        }

        static bool IsXmlChar(char ch)
        {
            // Check if the character is a valid XML character
            return ch == 0x9 || ch == 0xA || ch == 0xD ||
               (ch >= 0x20 && ch <= 0xD7FF) ||
               (ch >= 0xE000 && ch <= 0xFFFD) ||
               (ch >= 0x10000 && ch <= 0x10FFFF);
        }

        static string RemoveInvalidXmlCharacters(string input)
        {
            // Replace invalid characters with an empty string
            StringBuilder cleanedString = new StringBuilder();
            foreach (char ch in input)
            {
                if (IsXmlChar(ch))
                {
                    cleanedString.Append(ch);
                }
            }

            return cleanedString.ToString();
        }

        static string CleanXML(string filePath)
        {
            try
            {
                // Read as text
                string xmlContent = File.ReadAllText(filePath);

                // Clean invalid characters
                string cleanedXML = RemoveInvalidXmlCharacters(xmlContent);

                Console.WriteLine("XML Cleaned");

                return cleanedXML;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XML clean/load error: {ex}");
                return null;
            }
        }

        static List<string> GetShaders(string bsp_path, Loading loadingForm)
        {
            Console.WriteLine("Parsing XML for " + bsp_path);
            loadingForm.UpdateOutputBox("Parsing XML for " + bsp_path, false);

            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.None,
                DtdProcessing = DtdProcessing.Ignore
            };

            string xmlData = CleanXML(bsp_path);
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlData), settings))
            {
                XmlDocument bspfile = new XmlDocument();
                bspfile.Load(reader);
                XmlNode root = bspfile.DocumentElement;

                XmlNodeList materials_block = root.SelectNodes(".//block[@name='materials']");
                List<string> shader_paths = new List<string>();

                foreach (XmlNode material in materials_block)
                {
                    bool end = false;
                    int i = 0;
                    while (!end)
                    {
                        XmlNode element = material.SelectSingleNode("./element[@index='" + i + "']");
                        if (element != null)
                        {
                            shader_paths.Add(element.SelectSingleNode("./tag_reference[@name='shader']").InnerText.Trim());
                            i++;
                        }
                        else
                        {
                            end = true;
                            Console.WriteLine("\nFinished getting materials in bsp \"" + bsp_path + "\"\n");
                            loadingForm.UpdateOutputBox("\nFinished getting materials in bsp \"" + bsp_path + "\"\n", false);
                            Console.WriteLine("Shaders list:\n");
                            loadingForm.UpdateOutputBox("Shaders list:\n", false);
                            foreach (string shader in shader_paths)
                            {
                                Console.WriteLine(shader);
                                loadingForm.UpdateOutputBox(shader, false);
                            }
                        }
                    }
                }
                return shader_paths;
            }
        }

        static void ShaderExtractor(List<string> shader_paths, string h2ek_path, string xml_output_path, Loading loadingForm)
        {
            string tool_path = h2ek_path + @"\tool.exe";

            foreach (string shader_path in shader_paths)
            {
                List<string> argumentList = new List<string>
            {
                "export-tag-to-xml",
                "\"" + shader_path + "\"",
                "\"" + xml_output_path + "\\" + shader_path.Split('\\').Last().Replace(".shader", "") + ".xml" + "\""
            };

                string arguments = string.Join(" ", argumentList);

                RunTool(tool_path, arguments, h2ek_path, loadingForm);
            }
        }

        static void RunTool(string tool_path, string arguments, string ek_path, Loading loadingForm)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = tool_path,
                Arguments = arguments,
                WorkingDirectory = ek_path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = processStartInfo
            };

            if (tool_path.Contains("H3EK"))
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Console.WriteLine(e.Data);
                        loadingForm.UpdateOutputBox(e.Data + Environment.NewLine, true);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Console.WriteLine("Error: " + e.Data);
                        loadingForm.UpdateOutputBox("Error: " + e.Data + Environment.NewLine, true);
                    }
                };
            }

            process.Start();
            if (tool_path.Contains("H3EK"))
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            // Wait for the process to exit or the timeout to elapse
            Task processTask = Task.Run(() =>
            {
                if (!process.WaitForExit(5 * 1000)) // Wait with timeout
                {
                    // TODO: Implement failure case
                }
            });

            processTask.Wait(); // Wait for the process task to complete

            process.Close();
        }

        static List<Shader> GetShaderData(string xml_output_path)
        {
            List<Shader> all_shader_data = new List<Shader>();
            string[] xml_files = Directory.GetFiles(xml_output_path, "*.xml");
            foreach (string xml_file in xml_files)
            {
                XmlDocument shader_file = new XmlDocument();
                shader_file.Load(xml_file);
                XmlNode root = shader_file.DocumentElement;
                List<Parameter> shader_parameters = new List<Parameter>();
                string shader_name = (new DirectoryInfo(xml_file).Name).Replace(".xml", "");
                string specular_col = "";
                string specular_glc_col = "";
                string env_col = "";
                string env_glc_col = "";
                string env_bitm = "";

                XmlNodeList params_block = root.SelectNodes(".//block[@name='parameters']");
                string shd_templ = root.SelectSingleNode("./tag_reference[@name='template']").InnerText.Trim();
                string shd_globmat = root.SelectSingleNode("./field[@name='material name']").InnerText.Trim();
                string specular_setting = root.SelectSingleNode("./field[@name='lightmap type']").InnerText.Trim();

                foreach (XmlNode param in params_block)
                {
                    bool end = false;
                    int i = 0;
                    while (!end)
                    {
                        XmlNode element = param.SelectSingleNode("./element[@index='" + i + "']");
                        if (element != null)
                        {
                            string prm_name = element.SelectSingleNode("./field[@name='name']").InnerText.Trim();

                            // Check if it is specular or env colour info. If so, add to shader data and skip param
                            if (prm_name == "specular_color")
                            {
                                specular_col = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                            }
                            else if (prm_name == "specular_glancing_color")
                            {
                                specular_glc_col = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                            }
                            else if (prm_name == "env_tint_color")
                            {
                                env_col = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                            }
                            else if (prm_name == "env_glancing_tint_color")
                            {
                                env_glc_col = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                            }
                            else if (prm_name == "environment_map")
                            {
                                env_bitm = element.SelectSingleNode("./tag_reference[@name='bitmap']").InnerText.Trim();
                            }
                            else
                            {
                                string prm_type = element.SelectSingleNode("./field[@name='type']").InnerText.Trim();
                                string prm_bitmap = element.SelectSingleNode("./tag_reference[@name='bitmap']").InnerText.Trim();
                                string prm_value = element.SelectSingleNode("./field[@name='const value']").InnerText.Trim();
                                string prm_colour = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                                XmlNode anim_data_block = element.SelectSingleNode("./block[@name='animation properties']");
                                sbyte byte1_scaleX = new sbyte();
                                byte byte2_scaleX = new byte();
                                sbyte byte1_scaleY = new sbyte();
                                byte byte2_scaleY = new byte();

                                if (anim_data_block != null)
                                {
                                    // Animation data exists
                                    foreach (XmlNode anim_data in anim_data_block)
                                    {
                                        string type = anim_data.SelectSingleNode("./field[@name='type']").InnerText.Trim();
                                        if (type.Contains("bitmap scale x"))
                                        {
                                            // Grab x scale
                                            XmlNode data_block = anim_data.SelectSingleNode("./block[@name='data']");
                                            foreach (XmlNode index in data_block)
                                            {
                                                // Indices 6 and 7 contain the scale value bytes
                                                if (index.Attributes["index"]?.Value == "6")
                                                {
                                                    byte1_scaleX = sbyte.Parse(index.SelectSingleNode("./field[@name='Value']").InnerText.Trim());
                                                }
                                                else if (index.Attributes["index"]?.Value == "7")
                                                {
                                                    byte2_scaleX = byte.Parse(index.SelectSingleNode("./field[@name='Value']").InnerText.Trim());
                                                    break;
                                                }
                                            }
                                        }
                                        else if (type.Contains("bitmap scale y"))
                                        {
                                            // Grab y scale
                                            XmlNode data_block = anim_data.SelectSingleNode("./block[@name='data']");
                                            foreach (XmlNode index in data_block)
                                            {
                                                // Indices 6 and 7 contain the scale value bytes
                                                if (index.Attributes["index"]?.Value == "6")
                                                {
                                                    byte1_scaleY = sbyte.Parse(index.SelectSingleNode("./field[@name='Value']").InnerText.Trim());
                                                }
                                                else if (index.Attributes["index"]?.Value == "7")
                                                {
                                                    byte2_scaleY = byte.Parse(index.SelectSingleNode("./field[@name='Value']").InnerText.Trim());
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                shader_parameters.Add(new Parameter
                                {
                                    name = prm_name,
                                    type = prm_type,
                                    bitmap = prm_bitmap,
                                    value = prm_value,
                                    colour = prm_colour,
                                    scalex_1 = byte1_scaleX,
                                    scalex_2 = byte2_scaleX,
                                    scaley_1 = byte1_scaleY,
                                    scaley_2 = byte2_scaleY,

                                });
                            }

                            i++;
                        }
                        else
                        {
                            end = true;
                        }
                    }
                }
                all_shader_data.Add(new Shader
                {
                    name = shader_name,
                    glob_mat = shd_globmat,
                    template = shd_templ,
                    parameters = shader_parameters,
                    spec_col = specular_col,
                    spec_glnc = specular_glc_col,
                    env_tint = env_col,
                    env_glnc = env_glc_col,
                    spec_type = specular_setting,
                    env_bitmap = env_bitm
                }); ;
            }
            return all_shader_data;
        }

        static async Task ExtractBitmaps(List<string> all_bitmap_refs, string h2ek_path, string tga_output_path, Loading loadingForm)
        {
            List<Task> tasks = new List<Task>();
            string tool_path = h2ek_path + @"\tool.exe";

            foreach (string bitmap in all_bitmap_refs)
            {
                // Extracting bitmap to TGA
                List<string> argumentListTGA = new List<string>
            {
                "export-bitmap-tga",
                "\"" + bitmap + "\"",
                "\"" + tga_output_path + "\\\\" + "\""
            };

                string arguments = string.Join(" ", argumentListTGA);
                tasks.Add(Task.Run(() => RunTool(tool_path, arguments, h2ek_path, loadingForm)));

                Console.WriteLine("Extracted " + bitmap);
                loadingForm.UpdateOutputBox("Extracted " + bitmap, false);
            }

            await Task.WhenAll(tasks);

            tasks.Clear();

            foreach (string bitmap in all_bitmap_refs)
            {
                // Extracting bitmap to XML
                List<string> argumentListXML = new List<string>
            {
                "export-tag-to-xml",
                "\"" + h2ek_path + "\\tags\\" + bitmap + ".bitmap" + "\"",
                "\"" + tga_output_path.Replace("textures_output", "bitmap_xml") + "\\" + bitmap.Split('\\').Last() + ".xml" + "\""
            };

                string arguments = string.Join(" ", argumentListXML);
                tasks.Add(Task.Run(() => RunTool(tool_path, arguments, h2ek_path, loadingForm)));
            }

            await Task.WhenAll(tasks);
        }

        static async Task WaitForTaskCompletion(Task task)
        {
            while (task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Faulted && task.Status != TaskStatus.Canceled)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        static string[] TGAToTIF(string tga_output_path, string bitmaps_dir, string h3ek_path, Loading loadingForm)
        {
            List<string> error_files = new List<string>();
            string[] tga_files = Directory.GetFiles(tga_output_path, "*.tga");
            foreach (string tga_file in tga_files)
            {
                string tif_output_path = (tga_file.Replace("_00_00", "")).Replace(".tga", ".tif");

                using (var image = new MagickImage(tga_file))
                {
                    image.Format = MagickFormat.Tif;
                    image.Write(tif_output_path);
                }
            }

            string[] tifFiles = Directory.GetFiles(tga_output_path, "*.tif");

            // Delete TGA files
            foreach (string tgaFile in tga_files)
            {
                File.Delete(tgaFile);
            }

            // Move TIF files to scenario bitmaps directory
            if (!Directory.Exists(bitmaps_dir))
            {
                Directory.CreateDirectory(bitmaps_dir);
            }

            foreach (string tifFile in tifFiles)
            {
                string file_name = tifFile.Split('\\').Last();
                try
                {
                    File.Move(tifFile, bitmaps_dir + "\\" + file_name);
                }
                catch (IOException)
                {
                    // File already exists, dont worry about it
                }
            }

            Console.WriteLine("Importing bitmaps...");
            loadingForm.UpdateOutputBox("Importing bitmaps...", false);
            string tool_path = h3ek_path + @"\tool.exe";
            List<string> argumentList = new List<string>
        {
            "bitmaps",
            bitmaps_dir.Split(new[] { "\\data\\" }, StringSplitOptions.None).LastOrDefault()
        };

            string arguments = string.Join(" ", argumentList);
            RunTool(tool_path, arguments, h3ek_path, loadingForm);

            Console.WriteLine("Setting bitmap options...");
            loadingForm.UpdateOutputBox("Setting bitmap options...", false);
            List<BitmapData> all_bitmap_data = new List<BitmapData>();
            string[] xml_files = Directory.GetFiles(tga_output_path.Replace("textures_output", "bitmap_xml"), "*.xml");

            foreach (string xml_file in xml_files)
            {
                XmlDocument bitmap_file = new XmlDocument();
                bitmap_file.Load(xml_file);
                XmlNode root = bitmap_file.DocumentElement;
                string bitmap_name = (new DirectoryInfo(xml_file).Name).Replace(".xml", "");
                string usage = root.SelectSingleNode("./field[@name='usage']").InnerText.Trim();
                string compression = root.SelectSingleNode("./field[@name='format']").InnerText.Trim();
                string fade_factor = root.SelectSingleNode("./field[@name='detail fade factor']").InnerText.Trim();
                string bump_height = root.SelectSingleNode("./field[@name='bump height']").InnerText.Trim();

                all_bitmap_data.Add(new BitmapData
                {
                    bitmap = bitmap_name,
                    type = usage,
                    compr = compression,
                    fade = fade_factor,
                    bmp_hgt = bump_height
                });
            }

            foreach (BitmapData bitmap_data in all_bitmap_data)
            {
                string bitmap_file_path = (bitmaps_dir.Replace("data", "tags")).Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last() + "\\" + bitmap_data.bitmap;
                TagPath tag_path = TagPath.FromPathAndType(bitmap_file_path, "bitm*");

                try
                {
                    using (TagFile tagFile = new TagFile(tag_path))
                    {
                        // Usage
                        var type = (TagFieldEnum)tagFile.SelectField("LongEnum:Usage");
                        if (bitmap_data.type.Contains("default"))
                        {
                            type.Value = 0;
                        }
                        else if (bitmap_data.type.Contains("height"))
                        {
                            type.Value = 2;
                        }
                        else if (bitmap_data.type.Contains("detail"))
                        {
                            type.Value = 4;
                        }

                        // Compression
                        var compr = (TagFieldEnum)tagFile.SelectField("ShortEnum:force bitmap format");
                        if (bitmap_data.type.Contains("height"))
                        {
                            compr.Value = 3; // Best compressed bump
                        }
                        else if (bitmap_data.compr.Contains("color-key"))
                        {
                            compr.Value = 13; //DXT1
                        }
                        else if (bitmap_data.compr.Contains("explicit alpha"))
                        {
                            compr.Value = 14; //DXT3
                        }
                        else if (bitmap_data.compr.Contains("interpolated alpha"))
                        {
                            compr.Value = 15; //DXT5
                        }

                        // Curve mode - always set force pretty
                        var curve = (TagFieldEnum)tagFile.SelectField("CharEnum:curve mode");
                        curve.Value = 2; // force pretty

                        // Fade factor
                        var fade = (TagFieldElementSingle)tagFile.SelectField("RealFraction:fade factor");
                        fade.Data = float.Parse(bitmap_data.fade);

                        // Bump height
                        if (bitmap_data.type.Contains("height"))
                        {
                            var height = (TagFieldElementSingle)tagFile.SelectField("Real:bump map height");
                            if (float.Parse(bitmap_data.bmp_hgt) >= 15.0f)
                            {
                                height.Data = 10.0f;
                            }
                            else
                            {
                                height.Data = float.Parse(bitmap_data.bmp_hgt);
                            }
                        }

                        tagFile.Save();
                    }
                }
                catch (Bungie.Tags.TagLoadException)
                {
                    error_files.Add($"There was an issue loading the bitmap {bitmap_file_path}. It may not have been exported from H2 correctly.");
                }

            }

            // Run import again to update bitmaps
            Console.WriteLine("Reimport bitmaps...");
            loadingForm.UpdateOutputBox("Reimport bitmaps...", false);
            RunTool(tool_path, arguments, h3ek_path, loadingForm);
            return error_files.ToArray();
        }

        static void AddShaderScaleFunc(TagFile tagFile, int type, int index, byte byte1, byte byte2, int anim_index)
        {
            // Add scale element
            ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{index}]/Block:animated parameters")).AddElement();

            var func_name = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{index}]/Block:animated parameters[{anim_index}]/LongEnum:type");
            func_name.Value = type; // 2 is scale uniform, 3 is scale x, 4 is scale y

            var func_data = (TagFieldData)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{index}]/Block:animated parameters[{anim_index}]/Struct:function[0]/Data:data");
            byte[] data_array = new byte[32];
            data_array[0] = 1; // Seems to set the "basic" type
            data_array[6] = byte2; //byte2, unsigned
            data_array[7] = byte1; // byte1
            func_data.SetData(data_array);
        }

        static void MakeShaderTags(List<Shader> all_shader_data, string bitmaps_dir, string h3ek_path, Loading loadingForm)
        {
            string bitmap_tags_dir = bitmaps_dir.Replace("data", "tags").Split(new[] { "\\tags\\" }, StringSplitOptions.None).LastOrDefault();
            string shaders_dir = (bitmaps_dir.Split(new[] { "\\data\\" }, StringSplitOptions.None).LastOrDefault()).Replace("bitmaps", "shaders");

            CookSettings cook_diffuse = new CookSettings
            {
                spec_coeff = 0.06366198f,
                roughness = 0.3f,
                area_contr = 0.3f,
                anal_contr = 0.5f,
                env_contr = 0.0f
            };

            CookSettings cook_default = new CookSettings
            {
                spec_coeff = 0.2546479f,
                roughness = 0.2f,
                area_contr = 0.5f,
                anal_contr = 0.6f,
                env_contr = 1.0f
            };

            CookSettings cook_dull = new CookSettings
            {
                spec_coeff = 0.06366198f,
                roughness = 0.3f,
                area_contr = 0.1f,
                anal_contr = 0.2f,
                env_contr = 0.0f
            };

            CookSettings cook_shiny = new CookSettings
            {
                spec_coeff = 0.318309873f,
                roughness = 0.1f,
                area_contr = 0.2f,
                anal_contr = 0.5f,
                env_contr = 1.0f
            };

            foreach (Shader shader in all_shader_data)
            {

                if (shader.glob_mat.Contains("soft_organic_plant")) // Needs to be .shader_foliage
                {
                    string shader_name = Path.Combine(shaders_dir, shader.name);
                    var tag_path = TagPath.FromPathAndType(shader_name, "rmfl*");

                    // Create the tag
                    TagFile tagFile = new TagFile();
                    tagFile.New(tag_path);

                    // Set alpha test to simple
                    var alpha_test = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[1]/ShortInteger:short");
                    alpha_test.Data = 1;

                    // Global material
                    var global_mat = (TagFieldElementStringID)tagFile.SelectField("StringID:material name");
                    global_mat.Data = shader.glob_mat;

                    int param_index = 0;

                    foreach (Parameter param in shader.parameters)
                    {
                        if (param.name == "base_map")
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string base_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add base map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "base_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set base map
                            var base_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            base_map.Path = TagPath.FromPathAndType(base_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if (param.name == "detail_map")
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string detail_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "detail_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set detail map
                            var detail_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            detail_map.Path = TagPath.FromPathAndType(detail_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if (param.name == "bump_map" || param.name == "lightmap_alphatest_map")
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string alpha_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            if (param.name == "bump_map")
                            {
                                // Reimport bump map as colour map to get alpha test working
                                TagPath bitmap_path = TagPath.FromPathAndType(alpha_map_path, "bitm*");

                                using (var bitmapFile = new TagFile(bitmap_path))
                                {
                                    var compr = (TagFieldEnum)bitmapFile.SelectField("ShortEnum:force bitmap format");
                                    compr.Value = 2;
                                    bitmapFile.Save();
                                }

                                List<string> argumentList = new List<string>
                            {
                                "reimport-bitmaps-single",
                                alpha_map_path
                            };

                                string arguments = string.Join(" ", argumentList);
                                string tool_path = h3ek_path + @"\tool.exe";

                                Console.WriteLine($"Reimporting bitmap {bitmap_filename} as colour for shader foliage...");
                                loadingForm.UpdateOutputBox($"Reimporting bitmap {bitmap_filename} as colour for shader foliage...", false);
                                RunTool(tool_path, arguments, h3ek_path, loadingForm);
                            }

                            // Add alpha map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "alpha_test_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set alpha map
                            var bump_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            bump_map.Path = TagPath.FromPathAndType(alpha_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }


                            //tagFile.Save();

                            param_index++;
                        }
                    }
                    tagFile.Save();
                }
                else if (shader.template.Contains("plasma_alpha"))
                {
                    string shader_name = Path.Combine(shaders_dir, shader.name);
                    var tag_path = TagPath.FromPathAndType(shader_name, "rmsh*");

                    // Create the tag
                    TagFile tagFile = new TagFile();
                    tagFile.New(tag_path);

                    // Set pre multiplied alpha blend
                    var alpha_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[7]/ShortInteger:short");
                    alpha_option.Data = 5; // 5 for pre-multiplied

                    // Set plasma illum
                    var illum_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[6]/ShortInteger:short");
                    illum_option.Data = 3; // 5 for plasma

                    // Global material
                    var global_mat = (TagFieldElementStringID)tagFile.SelectField("StringID:material name");
                    global_mat.Data = shader.glob_mat;

                    int param_index = 0;

                    foreach (Parameter param in shader.parameters)
                    {
                        if (param.name == "noise_map_a")
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string noise_a_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add noise a map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "noise_map_a";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set noise a map
                            var noise_a_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            noise_a_map.Path = TagPath.FromPathAndType(noise_a_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if (param.name == "noise_map_b")
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string noise_b_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add noise b map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "noise_map_b";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set noise b map
                            var noise_b_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            noise_b_map.Path = TagPath.FromPathAndType(noise_b_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if (param.name == "alpha_map")
                        {
                            if (!string.IsNullOrEmpty(param.bitmap) && File.Exists(param.bitmap))
                            {
                                string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                                string alpha_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                                // Add alpha test map parameter as base
                                ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                                var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                                param_name.Data = "base_map";
                                var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                                param_type.Value = 0;

                                // Set alpha test map
                                var alpha_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                                alpha_map.Path = TagPath.FromPathAndType(alpha_map_path, "bitm*");

                                // Set aniso
                                var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                                flags.Data = 1;
                                var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                                aniso.Data = 6;

                                // Scale function data
                                byte byte1_x = param.scalex_2;
                                byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                                byte byte1_y = param.scaley_2;
                                byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                                byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                                bool all_zero = true;

                                foreach (byte scale in scales)
                                {
                                    if (scale != 0)
                                    {
                                        all_zero = false;
                                        break;
                                    }
                                }

                                if (!all_zero) // No need to bother if scale values arent provided
                                {
                                    if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                    {
                                        AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                    }
                                    else // Scale is non-uniform, handle separately
                                    {
                                        int anim_index = 0;
                                        if (!(byte1_x == 0 && byte2_x == 0))
                                        {
                                            AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                            anim_index++;
                                        }
                                        if (!(byte1_y == 0 && byte2_y == 0))
                                        {
                                            AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                        }
                                    }
                                }

                                param_index++;
                            }
                        }
                    }
                    tagFile.Save();
                }
                else
                {
                    string shader_name = Path.Combine(shaders_dir, shader.name);
                    var tag_path = TagPath.FromPathAndType(shader_name, "rmsh*");

                    // Create the tag
                    TagFile tagFile = new TagFile();
                    tagFile.New(tag_path);

                    // Set bump on
                    var bump_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[1]/ShortInteger:short");
                    bump_option.Data = 1; // 1 for standard bump

                    // Blend mode?
                    if (shader.template.Contains("opaque\\overlay"))
                    {
                        // Set blend mode to double multiply
                        var blend_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[7]/ShortInteger:short");
                        blend_option.Data = 4;
                    }

                    // Global material
                    var global_mat = (TagFieldElementStringID)tagFile.SelectField("StringID:material name");
                    global_mat.Data = shader.glob_mat;

                    // Specular mask
                    var spec_mask_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[3]/ShortInteger:short");
                    spec_mask_option.Data = 1;

                    int param_index = 0;

                    // Cook torrance data
                    if (shader.spec_col != "")
                    {
                        var mat_mdl_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[4]/ShortInteger:short");
                        mat_mdl_option.Data = 1;

                        // Add new parameter
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                        // Set parameter name
                        var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        param_name.Data = "specular_tint";

                        // Set parameter type
                        var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        param_type.Value = 1; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                        // Add animated parameter
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();

                        // Set animated parameter type to colour
                        var anim_param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                        anim_param_type.Value = 1; // 0 is "scale uniform", 1 is "color"

                        // Set function data to RGB colour
                        TagFieldCustomFunctionEditor spec_tint_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                        spec_tint_func.Value.ColorGraphType = FunctionEditorColorGraphType.OneColor;
                        spec_tint_func.Value.MasterType = FunctionEditorMasterType.Basic;
                        float[] spec_colour = shader.spec_col.Split(',').Select(float.Parse).ToArray();
                        spec_tint_func.Value.SetColor(0, GameColor.FromRgb(spec_colour[0], spec_colour[1], spec_colour[2]));
                        param_index++;

                        if (shader.spec_glnc != "")
                        {
                            // Add new parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                            // Set parameter name
                            var spec_glc_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            spec_glc_name.Data = "fresnel_color";

                            // Set parameter type
                            var spec_glc_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            spec_glc_type.Value = 1; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                            // Add animated parameter
                            ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();

                            // Set animated parameter type to colour
                            var spec_glc_anim_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                            spec_glc_anim_type.Value = 1; // 0 is "scale uniform", 1 is "color"

                            // Set function data to RGB colour
                            TagFieldCustomFunctionEditor spec_glc_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                            spec_glc_func.Value.ColorGraphType = FunctionEditorColorGraphType.OneColor;
                            spec_glc_func.Value.MasterType = FunctionEditorMasterType.Basic;
                            float[] spec_glc_colour = shader.spec_glnc.Split(',').Select(float.Parse).ToArray();
                            spec_glc_func.Value.SetColor(0, GameColor.FromRgb(spec_glc_colour[0], spec_glc_colour[1], spec_glc_colour[2]));
                            param_index++;
                        }

                        // Set generic cook torrance values
                        // Specular coefficient
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var spec_coeff_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        spec_coeff_name.Data = "specular_coefficient";
                        var spec_coeff_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        spec_coeff_type.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();
                        var anim_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                        anim_type.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor spec_coeff_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                        spec_coeff_func.Value.MasterType = FunctionEditorMasterType.Basic;
                        param_index++;

                        // Roughness
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var roughness_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        roughness_name.Data = "roughness";
                        var roughness_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        roughness_type.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();
                        var roughness_anim_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                        roughness_anim_type.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor rough_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                        rough_func.Value.MasterType = FunctionEditorMasterType.Basic;
                        param_index++;

                        // Area specular contribution
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var area_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        area_name.Data = "area_specular_contribution";
                        var area_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        area_type.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();
                        var area_anim_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                        area_anim_type.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor area_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                        area_func.Value.MasterType = FunctionEditorMasterType.Basic;
                        param_index++;

                        // Analytical specular contribution
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var anal_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        anal_name.Data = "analytical_specular_contribution";
                        var anal_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        anal_type.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();
                        var anal_anim_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                        anal_anim_type.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor anal_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                        anal_func.Value.MasterType = FunctionEditorMasterType.Basic;
                        param_index++;

                        // Env map specular contribution
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var env_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        env_name.Data = "environment_map_specular_contribution";
                        var env_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        env_type.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();
                        var env_anim_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                        env_anim_type.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor env_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                        env_func.Value.MasterType = FunctionEditorMasterType.Basic;
                        param_index++;

                        // Values need to change depending on h2 specular setting
                        if (shader.spec_type == "0,diffuse")
                        {
                            spec_coeff_func.Value.ClampRangeMin = cook_diffuse.spec_coeff;
                            rough_func.Value.ClampRangeMin = cook_diffuse.roughness;
                            area_func.Value.ClampRangeMin = cook_diffuse.area_contr;
                            anal_func.Value.ClampRangeMin = cook_diffuse.anal_contr;
                            env_func.Value.ClampRangeMin = cook_diffuse.env_contr;
                        }
                        else if (shader.spec_type == "1,default specular")
                        {
                            spec_coeff_func.Value.ClampRangeMin = cook_default.spec_coeff;
                            rough_func.Value.ClampRangeMin = cook_default.roughness;
                            area_func.Value.ClampRangeMin = cook_default.area_contr;
                            anal_func.Value.ClampRangeMin = cook_default.anal_contr;
                            env_func.Value.ClampRangeMin = cook_default.env_contr;
                        }
                        else if (shader.spec_type == "2,dull specular")
                        {
                            spec_coeff_func.Value.ClampRangeMin = cook_dull.spec_coeff;
                            rough_func.Value.ClampRangeMin = cook_dull.roughness;
                            area_func.Value.ClampRangeMin = cook_dull.area_contr;
                            anal_func.Value.ClampRangeMin = cook_dull.anal_contr;
                            env_func.Value.ClampRangeMin = cook_dull.env_contr;
                        }
                        else if (shader.spec_type == "3,shiny specular")
                        {
                            spec_coeff_func.Value.ClampRangeMin = cook_shiny.spec_coeff;
                            rough_func.Value.ClampRangeMin = cook_shiny.roughness;
                            area_func.Value.ClampRangeMin = cook_shiny.area_contr;
                            anal_func.Value.ClampRangeMin = cook_shiny.anal_contr;
                            env_func.Value.ClampRangeMin = cook_shiny.env_contr;
                        }
                        else
                        {
                            Console.WriteLine($"Shader {shader.name} had an invalid specular setting, odd.");
                            loadingForm.UpdateOutputBox($"Shader {shader.name} had an invalid specular setting, odd.", false);
                        }
                    }

                    // Dynamic env mapping?
                    if (shader.env_tint != "" && shader.env_bitmap == "")
                    {
                        var env_mapping = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[5]/ShortInteger:short");
                        env_mapping.Data = 2; // 2 is dynamic

                        // Add new parameter
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                        // Set parameter name
                        var env_param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        env_param_name.Data = "env_tint_color";

                        // Set parameter type
                        var env_param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        env_param_type.Value = 1; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                        // Add animated parameter
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();

                        // Set animated parameter type to colour
                        var anim_param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                        anim_param_type.Value = 1; // 0 is "scale uniform", 1 is "color"

                        // Set function data to RGB colour
                        TagFieldCustomFunctionEditor env_col_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                        env_col_func.Value.ColorGraphType = FunctionEditorColorGraphType.OneColor;
                        env_col_func.Value.MasterType = FunctionEditorMasterType.Basic;
                        float[] env_colour = shader.env_tint.Split(',').Select(float.Parse).ToArray();
                        env_col_func.Value.SetColor(0, GameColor.FromRgb(env_colour[0], env_colour[1], env_colour[2]));
                        param_index++;
                    }

                    // Per-pixel env mapping
                    else if (shader.env_tint != "" && shader.env_bitmap != "")
                    {
                        var env_mapping = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[5]/ShortInteger:short");
                        env_mapping.Data = 1; // 1 is per-pixel

                        // Add new parameter
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                        // Set parameter name
                        var env_param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        env_param_name.Data = "env_tint_color";

                        // Set parameter type
                        var env_param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        env_param_type.Value = 1; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                        // Add animated parameter
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters")).AddElement();

                        // Set animated parameter type to colour
                        var anim_param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/LongEnum:type");
                        anim_param_type.Value = 1; // 0 is "scale uniform", 1 is "color"

                        // Set function data to RGB colour
                        TagFieldCustomFunctionEditor env_col_func = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Block:animated parameters[0]/Custom:animation function");
                        env_col_func.Value.ColorGraphType = FunctionEditorColorGraphType.OneColor;
                        env_col_func.Value.MasterType = FunctionEditorMasterType.Basic;
                        float[] env_colour = shader.env_tint.Split(',').Select(float.Parse).ToArray();
                        env_col_func.Value.SetColor(0, GameColor.FromRgb(env_colour[0], env_colour[1], env_colour[2]));
                        param_index++;

                        // Add new parameter for env bitmap
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                        // Set parameter name
                        var env_bitm_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                        env_bitm_name.Data = "environment_map";

                        // Set parameter type
                        var env_bitm_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                        env_bitm_type.Value = 0; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                        // Set env bitmap
                        var env_bitm = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                        env_bitm.Path = TagPath.FromPathAndType(Path.Combine(bitmap_tags_dir, new DirectoryInfo(shader.env_bitmap).Name), "bitm*");
                        param_index++;
                    }

                    foreach (Parameter param in shader.parameters)
                    {
                        if (param.name == "base_map")
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string base_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add base map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "base_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set base map
                            var base_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            base_map.Path = TagPath.FromPathAndType(base_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if (param.name == "detail_map" && !shader.template.Contains("three"))
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string detail_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "detail_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set detail map
                            var detail_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            detail_map.Path = TagPath.FromPathAndType(detail_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if (param.name == "overlay_detail_map" && !shader.template.Contains("detail"))
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string detail_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "detail_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set detail map
                            var detail_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            detail_map.Path = TagPath.FromPathAndType(detail_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if ((param.name == "detail_map_a" || param.name == "blend_detail_map_1") && shader.template.Contains("detail"))
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string detail_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "detail_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set detail map
                            var detail_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            detail_map.Path = TagPath.FromPathAndType(detail_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if ((param.name == "secondary_detail_map" || param.name == "detail_map_b" || param.name == "blend_detail_map_2") && shader.template.Contains("detail"))
                        {
                            // Set two detail
                            var albedo_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[0]/ShortInteger:short");
                            albedo_option.Data = 1; // 7 for detail blend

                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string sec_detail_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "detail_map2";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set detail map
                            var detail2_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            detail2_map.Path = TagPath.FromPathAndType(sec_detail_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if ((param.name == "detail_map_c" || param.name == "overlay_detail_map") && shader.template.Contains("detail"))
                        {
                            // Set three detail blend
                            var albedo_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[0]/ShortInteger:short");
                            albedo_option.Data = 5; // 5 for three detail blend

                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string sec_detail_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "detail_map3";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set detail map
                            var detail2_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            detail2_map.Path = TagPath.FromPathAndType(sec_detail_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if (param.name == "bump_map")
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string bump_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Add bump map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "bump_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set bump map
                            var bump_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            bump_map.Path = TagPath.FromPathAndType(bump_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }


                            //tagFile.Save();

                            param_index++;
                        }

                        if (param.name == "alpha_test_map" || param.name == "lightmap_alphatest_map")
                        {
                            // Enable alpha test
                            var albedo_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[2]/ShortInteger:short");
                            albedo_option.Data = 1;

                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string alpha_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Reimport diffuse map as DXT5 to make sure the alpha works
                            TagPath bitmap_path = TagPath.FromPathAndType(alpha_map_path, "bitm*");

                            using (var bitmapFile = new TagFile(bitmap_path))
                            {
                                var compr = (TagFieldEnum)bitmapFile.SelectField("ShortEnum:force bitmap format");
                                compr.Value = 15; // 15 for DXT5
                                bitmapFile.Save();
                            }

                            List<string> argumentList = new List<string>
                            {
                                "reimport-bitmaps-single",
                                alpha_map_path
                            };

                            string arguments = string.Join(" ", argumentList);
                            string tool_path = h3ek_path + @"\tool.exe";

                            Console.WriteLine($"Reimporting bitmap {bitmap_filename} as DXT5 to make sure alpha works for transparency...");
                            loadingForm.UpdateOutputBox($"Reimporting bitmap {bitmap_filename} as DXT5 to make sure alpha works for transparency...", false);
                            RunTool(tool_path, arguments, h3ek_path, loadingForm);

                            // Add alpha test map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "alpha_test_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set alpha test map
                            var alpha_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            alpha_map.Path = TagPath.FromPathAndType(alpha_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }

                            param_index++;
                        }

                        if (param.name == "self_illum_map" && shader.template.Contains("illum"))
                        {
                            string bitmap_filename = new DirectoryInfo(param.bitmap).Name;
                            string illum_map_path = Path.Combine(bitmap_tags_dir, bitmap_filename);

                            // Enable self-illum
                            var illum_option = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[6]/ShortInteger:short");
                            if (shader.template.Contains("3_channel"))
                            {
                                illum_option.Data = 2; // 3 channel self illum
                            }
                            else
                            {
                                illum_option.Data = 1; // simple self illum
                            }


                            // Add illum map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var param_name = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/StringID:parameter name");
                            param_name.Data = "self_illum_map";
                            var param_type = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/LongEnum:parameter type");
                            param_type.Value = 0;

                            // Set illum map
                            var illum_map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/Reference:bitmap");
                            illum_map.Path = TagPath.FromPathAndType(illum_map_path, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{param_index}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1_x = param.scalex_2;
                            byte byte2_x = (byte)(256 + param.scalex_1); // Convert to unsigned
                            byte byte1_y = param.scaley_2;
                            byte byte2_y = (byte)(256 + param.scaley_1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1_x, byte2_x, byte1_y, byte2_y };
                            bool all_zero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    all_zero = false;
                                    break;
                                }
                            }

                            if (!all_zero) // No need to bother if scale values arent provided
                            {
                                if ((byte1_x == byte1_y) && (byte2_x == byte2_y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, param_index, byte1_x, byte2_x, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int anim_index = 0;
                                    if (!(byte1_x == 0 && byte2_x == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, param_index, byte1_x, byte2_x, anim_index);
                                        anim_index++;
                                    }
                                    if (!(byte1_y == 0 && byte2_y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, param_index, byte1_y, byte2_y, anim_index);
                                    }
                                }
                            }


                            //tagFile.Save();

                            param_index++;
                        }
                    }
                    tagFile.Save();
                }
            }
        }
    }
}
