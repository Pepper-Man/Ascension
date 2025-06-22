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
        public string Name { get; set; }
        public string GlobMat { get; set; }
        public string Template { get; set; }
        public List<Parameter> Parameters { get; set; }
        public string SpecCol { get; set; }
        public string SpecGlnc { get; set; }
        public string EnvTint { get; set; }
        public string EnvGlnc { get; set; }
        public string SpecType { get; set; }
        public string EnvBitmap { get; set; }
    }

    class Parameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Bitmap { get; set; }
        public string Value { get; set; }
        public string Colour { get; set; }
        public sbyte ScaleX1 { get; set; }
        public byte ScaleX2 { get; set; }
        public sbyte ScaleY1 { get; set; }
        public byte ScaleY2 { get; set; }
        public byte[] ScaleY { get; set; }
    }

    class BitmapData
    {
        public string Bitmap { get; set; }
        public string Type { get; set; }
        public string Compression { get; set; }
        public string Fade { get; set; }
        public string BumpHeight { get; set; }
    }

    class CookSettings
    {
        public float SpecCoeff { get; set; }
        public float Roughness { get; set; }
        public float AreaContr { get; set; }
        public float AnalContr { get; set; }
        public float EnvContr { get; set; }
    }

    public class ShaderConverter
    {
        public static void ConvertShaders(List<string> bspPaths, string h3ScenarioPath, bool useExistingBitmaps, Loading loadingForm)
        {
            string existingBitmaps = "";

            Console.WriteLine("Shader converter starting...\n\n");
            loadingForm.UpdateOutputBox("Shader converter starting...\n\n", false);

            if (useExistingBitmaps)
            {
                existingBitmaps = Path.GetDirectoryName(h3ScenarioPath.Replace("tags", "data")) + "\\bitmaps";
                if (!Directory.Exists(existingBitmaps))
                {
                    // Folder doesn't actually exist, ignore user request
                    Console.WriteLine("No existing data bitmap folder detected.");
                    loadingForm.UpdateOutputBox("No existing data bitmap folder detected.", false);
                    existingBitmaps = "";
                }
            }

            string bitmapsDir = (h3ScenarioPath.Substring(0, h3ScenarioPath.LastIndexOf('\\')) + "\\bitmaps").Replace("tags", "data");
            string h2ekPath = bspPaths[0].Substring(0, bspPaths[0].IndexOf("H2EK") + "H2EK".Length);
            string h3ekPath = h3ScenarioPath.Substring(0, h3ScenarioPath.IndexOf("H3EK") + "H3EK".Length);
            List<string> allH2ShaderPaths = new List<string>();

            foreach (string bsp in bspPaths)
            {
                List<string> bspShaderPaths = GetShaders(bsp, loadingForm);
                foreach (string path in bspShaderPaths)
                {
                    string fullPath = h2ekPath + @"\tags\" + path + ".shader";
                    if (!allH2ShaderPaths.Contains(fullPath))
                    {
                        allH2ShaderPaths.Add(fullPath);
                    }
                }
            }

            // Create shader xml export folder
            string xmlOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "shader_xml");
            if (!Directory.Exists(xmlOutputPath))
            {
                Directory.CreateDirectory(xmlOutputPath);
            }
            else
            {
                // Delete existing XML files
                string[] xmlFiles = Directory.GetFiles(xmlOutputPath, "*.xml");
                foreach (string xmlFile in xmlFiles)
                {
                    File.Delete(xmlFile);
                }
            }

            // Create bitmap tga export folder
            string tgaOutputPath = Path.Combine(Directory.GetCurrentDirectory(), "textures_output");

            if (!Directory.Exists(tgaOutputPath))
            {
                Directory.CreateDirectory(tgaOutputPath);
            }
            else
            {
                // Delete existing TGA/TIF files
                string[] tgaFiles = Directory.GetFiles(tgaOutputPath, "*.tga");
                string[] tifFiles = Directory.GetFiles(tgaOutputPath, "*.tif");
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
            if (!Directory.Exists(tgaOutputPath.Replace("textures_output", "bitmap_xml")))
            {
                Directory.CreateDirectory(tgaOutputPath.Replace("textures_output", "bitmap_xml"));
            }
            else
            {
                // Delete existing XML files
                string[] xmlFiles = Directory.GetFiles(tgaOutputPath.Replace("textures_output", "bitmap_xml"), "*.xml");
                foreach (string xmlFile in xmlFiles)
                {
                    File.Delete(xmlFile);
                }
            }

            Console.WriteLine("\nBeginning .shader to .xml conversion...");
            loadingForm.UpdateOutputBox("\nBeginning .shader to .xml conversion...", false);
            ShaderExtractor(allH2ShaderPaths, h2ekPath, xmlOutputPath, loadingForm);
            Console.WriteLine("\nAll shaders converted to XML!Grabbing all referenced bitmap paths:\n");
            loadingForm.UpdateOutputBox("\nAll shaders converted to XML!Grabbing all referenced bitmap paths:\n", false);
            List<Shader> allShaderData = GetShaderData(xmlOutputPath);
            List<string> allBitmapRefs = new List<string>();

            foreach (Shader shader in allShaderData)
            {
                foreach (Parameter param in shader.Parameters)
                {
                    if (!String.IsNullOrEmpty(param.Bitmap))
                    {
                        string fullBitmapPath = h2ekPath + "\\tags\\" + param.Bitmap + ".bitmap";

                        if (File.Exists(fullBitmapPath)) // Check bitmap tag actually exists, export-tag-to-xml doesnt error when path is invalid and will cause a program crash
                        {
                            if (!allBitmapRefs.Contains(param.Bitmap))
                            {
                                allBitmapRefs.Add(param.Bitmap);
                                Console.WriteLine(param.Bitmap);
                            }
                        }
                    }
                }
            }
            if (existingBitmaps == "")
            {
                // Grab all bitmaps
                Console.WriteLine("\nObtained all referenced bitmaps! Extracting bitmap tags to TGA...");
                loadingForm.UpdateOutputBox("\nObtained all referenced bitmaps! Extracting bitmap tags to TGA...", false);
                ExtractBitmaps(allBitmapRefs, h2ekPath, tgaOutputPath, loadingForm);
                Console.WriteLine("\nExtracted all bitmaps to .TGA");
                Console.WriteLine("\nRunning .TIF conversion process...");
                loadingForm.UpdateOutputBox("\nExtracted all bitmaps to .TGA", false);
                loadingForm.UpdateOutputBox("\nRunning .TIF conversion process...", false);
            }
            else
            {
                // Existing bitmaps folder has been provided, check if any are missing
                string[] existingFiles = Directory.GetFiles(existingBitmaps, "*.tif").Select(Path.GetFileNameWithoutExtension).ToArray();
                List<string> missingFiles = allBitmapRefs
                .Where(refName => !existingFiles.Any(existFile => refName.Contains(existFile)))
                .ToList();

                // Grab missing bitmaps
                Console.WriteLine("\nObtained all referenced bitmaps!Extracting any missing bitmaps to TGA...");
                loadingForm.UpdateOutputBox("\nObtained all referenced bitmaps!Extracting any missing bitmaps to TGA...", false);
                ExtractBitmaps(missingFiles, h2ekPath, tgaOutputPath, loadingForm);
                Console.WriteLine("\nExtracted all missing bitmaps to .TGA, running .TIF conversion process...");
                loadingForm.UpdateOutputBox("\nExtracted all missing bitmaps to .TGA, running .TIF conversion process...", false);
            }

            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ekPath);
            string[] errors = TGAToTIF(tgaOutputPath, bitmapsDir, h3ekPath, loadingForm);
            Console.WriteLine("\nFinished importing bitmaps into H3. Creating H3 shader tags...");
            loadingForm.UpdateOutputBox("\nFinished importing bitmaps into H3. Creating H3 shader tags...", false);
            MakeShaderTags(allShaderData, bitmapsDir, h3ekPath, loadingForm);
            Console.WriteLine("\nSuccessfully created all shader tags.");
            loadingForm.UpdateOutputBox("\nSuccessfully created all shader tags.", false);
            if (errors.Count() != 0)
            {
                Console.WriteLine("The following errors were caught:\n");
                loadingForm.UpdateOutputBox("The following errors were caught:\n", false);
                foreach (string bitmapIssue in errors)
                {
                    Console.WriteLine(bitmapIssue);
                    loadingForm.UpdateOutputBox(bitmapIssue, true);
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

        static List<string> GetShaders(string bspPath, Loading loadingForm)
        {
            Console.WriteLine("Parsing XML for " + bspPath);
            loadingForm.UpdateOutputBox("Parsing XML for " + bspPath, false);

            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.None,
                DtdProcessing = DtdProcessing.Ignore
            };

            string xmlData = CleanXML(bspPath);
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlData), settings))
            {
                XmlDocument bspfile = new XmlDocument();
                bspfile.Load(reader);
                XmlNode root = bspfile.DocumentElement;

                XmlNodeList materialsBlock = root.SelectNodes(".//block[@name='materials']");
                List<string> shadersPath = new List<string>();

                foreach (XmlNode material in materialsBlock)
                {
                    bool end = false;
                    int i = 0;
                    while (!end)
                    {
                        XmlNode element = material.SelectSingleNode("./element[@index='" + i + "']");
                        if (element != null)
                        {
                            shadersPath.Add(element.SelectSingleNode("./tag_reference[@name='shader']").InnerText.Trim());
                            i++;
                        }
                        else
                        {
                            end = true;
                            Console.WriteLine("\nFinished getting materials in bsp \"" + bspPath + "\"\n");
                            loadingForm.UpdateOutputBox("\nFinished getting materials in bsp \"" + bspPath + "\"\n", false);
                            Console.WriteLine("Shaders list:\n");
                            loadingForm.UpdateOutputBox("Shaders list:\n", false);
                            foreach (string shader in shadersPath)
                            {
                                Console.WriteLine(shader);
                                loadingForm.UpdateOutputBox(shader, false);
                            }
                        }
                    }
                }
                return shadersPath;
            }
        }

        static void ShaderExtractor(List<string> shaderPaths, string h2ekPath, string xmlOutputPath, Loading loadingForm)
        {
            string toolExePath = h2ekPath + @"\tool.exe";

            foreach (string shaderPath in shaderPaths)
            {
                ToolRunner.RunTool(toolExePath, "export-tag-to-xml", h2ekPath, shaderPath, loadingForm, xmlOutputPath, "shader");
            }
        }

        static List<Shader> GetShaderData(string xmlOutputPath)
        {
            List<Shader> allShaderData = new List<Shader>();
            string[] xmlFiles = Directory.GetFiles(xmlOutputPath, "*.xml");
            foreach (string xmlFile in xmlFiles)
            {
                XmlDocument shaderFile = new XmlDocument();
                shaderFile.Load(xmlFile);
                XmlNode root = shaderFile.DocumentElement;
                List<Parameter> shaderParameters = new List<Parameter>();
                string shaderName = (new DirectoryInfo(xmlFile).Name).Replace(".xml", "");
                string specCol = "";
                string specGlncCol = "";
                string envCol = "";
                string envGlncCol = "";
                string envBitmap = "";

                XmlNodeList paramsBlock = root.SelectNodes(".//block[@name='parameters']");
                string shaderTemplate = root.SelectSingleNode("./tag_reference[@name='template']").InnerText.Trim();
                string shaderGlobMat = root.SelectSingleNode("./field[@name='material name']").InnerText.Trim();
                string specularSetting = root.SelectSingleNode("./field[@name='lightmap type']").InnerText.Trim();

                foreach (XmlNode param in paramsBlock)
                {
                    bool end = false;
                    int i = 0;
                    while (!end)
                    {
                        XmlNode element = param.SelectSingleNode("./element[@index='" + i + "']");
                        if (element != null)
                        {
                            string paramName = element.SelectSingleNode("./field[@name='name']").InnerText.Trim();

                            // Check if it is specular or env colour info. If so, add to shader data and skip param
                            if (paramName == "specular_color")
                            {
                                specCol = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                            }
                            else if (paramName == "specular_glancing_color")
                            {
                                specGlncCol = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                            }
                            else if (paramName == "env_tint_color")
                            {
                                envCol = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                            }
                            else if (paramName == "env_glancing_tint_color")
                            {
                                envGlncCol = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                            }
                            else if (paramName == "environment_map")
                            {
                                envBitmap = element.SelectSingleNode("./tag_reference[@name='bitmap']").InnerText.Trim();
                            }
                            else
                            {
                                string paramType = element.SelectSingleNode("./field[@name='type']").InnerText.Trim();
                                string paramBitmap = element.SelectSingleNode("./tag_reference[@name='bitmap']").InnerText.Trim();
                                string paramValue = element.SelectSingleNode("./field[@name='const value']").InnerText.Trim();
                                string paramColour = element.SelectSingleNode("./field[@name='const color']").InnerText.Trim();
                                XmlNode animDataBlock = element.SelectSingleNode("./block[@name='animation properties']");
                                sbyte byte1ScaleX = new sbyte();
                                byte byte2ScaleX = new byte();
                                sbyte byte1ScaleY = new sbyte();
                                byte byte2ScaleY = new byte();

                                // GROUND SHADER HACK //

                                // If shader template is two detail + overlay, ignore "normal" detail maps (map, map_a, map_b, secondary) so as to only grab blend details maps
                                if (shaderTemplate.Contains("detail_blend_detail"))
                                {
                                    if (paramName == "detail_map" || paramName == "detail_map_a" || paramName == "detail_map_b" || paramName == "secondary_detail_map")
                                    {
                                        i++;
                                        continue;
                                    }
                                }
                                // If shader template is three detail blend, ignore "blend_detail", "overlay" and "secondary" detail maps (3DB template uses detail_map, a, b or c)
                                else if (shaderTemplate.Contains("three_detail_blend"))
                                {
                                    if (paramName == "blend_detail_map_1" || paramName == "blend_detail_map_2" || paramName == "overlay_detail_map" || paramName == "secondary_detail_map")
                                    {
                                        i++;
                                        continue;
                                    }
                                }

                                // END GROUND SHADER HACK //

                                if (animDataBlock != null)
                                {
                                    // Animation data exists
                                    foreach (XmlNode animData in animDataBlock)
                                    {
                                        string type = animData.SelectSingleNode("./field[@name='type']").InnerText.Trim();
                                        if (type.Contains("bitmap scale x"))
                                        {
                                            // Grab x scale
                                            XmlNode dataBlock = animData.SelectSingleNode("./block[@name='data']");
                                            foreach (XmlNode index in dataBlock)
                                            {
                                                // Indices 6 and 7 contain the scale value bytes
                                                if (index.Attributes["index"]?.Value == "6")
                                                {
                                                    byte1ScaleX = sbyte.Parse(index.SelectSingleNode("./field[@name='Value']").InnerText.Trim());
                                                }
                                                else if (index.Attributes["index"]?.Value == "7")
                                                {
                                                    byte2ScaleX = byte.Parse(index.SelectSingleNode("./field[@name='Value']").InnerText.Trim());
                                                    break;
                                                }
                                            }
                                        }
                                        else if (type.Contains("bitmap scale y"))
                                        {
                                            // Grab y scale
                                            XmlNode dataBlock = animData.SelectSingleNode("./block[@name='data']");
                                            foreach (XmlNode index in dataBlock)
                                            {
                                                // Indices 6 and 7 contain the scale value bytes
                                                if (index.Attributes["index"]?.Value == "6")
                                                {
                                                    byte1ScaleY = sbyte.Parse(index.SelectSingleNode("./field[@name='Value']").InnerText.Trim());
                                                }
                                                else if (index.Attributes["index"]?.Value == "7")
                                                {
                                                    byte2ScaleY = byte.Parse(index.SelectSingleNode("./field[@name='Value']").InnerText.Trim());
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                shaderParameters.Add(new Parameter
                                {
                                    Name = paramName,
                                    Type = paramType,
                                    Bitmap = paramBitmap,
                                    Value = paramValue,
                                    Colour = paramColour,
                                    ScaleX1 = byte1ScaleX,
                                    ScaleX2 = byte2ScaleX,
                                    ScaleY1 = byte1ScaleY,
                                    ScaleY2 = byte2ScaleY,

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
                allShaderData.Add(new Shader
                {
                    Name = shaderName,
                    GlobMat = shaderGlobMat,
                    Template = shaderTemplate,
                    Parameters = shaderParameters,
                    SpecCol = specCol,
                    SpecGlnc = specGlncCol,
                    EnvTint = envCol,
                    EnvGlnc = envGlncCol,
                    SpecType = specularSetting,
                    EnvBitmap = envBitmap
                }); ;
            }
            return allShaderData;
        }

        static void ExtractBitmaps(List<string> allBitmapRefs, string h2ekPath, string tgaOutputPath, Loading loadingForm)
        {
            string toolExePath = h2ekPath + @"\tool.exe";

            // Export bitmaps in parallel
            Parallel.ForEach(allBitmapRefs, bitmap =>
            {
                try
                {
                    ToolRunner.RunTool(toolExePath, "export-bitmap-tga", h2ekPath, bitmap, loadingForm, tgaOutputPath);

                    lock (loadingForm)
                    {
                        Console.WriteLine("Extracted " + bitmap);
                        loadingForm.UpdateOutputBox("Extracted " + bitmap, false);
                    }
                }
                catch (Exception e)
                {
                    lock (loadingForm)
                    {
                        loadingForm.UpdateOutputBox($"Failed to extract {bitmap}: {e.Message}", false);
                    }
                }
            });

            // Extract bitmaps to XML in parallel
            Parallel.ForEach(allBitmapRefs, bitmap =>
            {
                try
                {
                    ToolRunner.RunTool(toolExePath, "export-tag-to-xml", h2ekPath, bitmap, loadingForm, tgaOutputPath, "bitmap");
                }
                catch (Exception e)
                {
                    lock (loadingForm)
                    {
                        loadingForm.UpdateOutputBox($"Failed to export XML for {bitmap}: {e.Message}", false);
                    }
                }
            });
        }

        static string[] TGAToTIF(string tgaOutputPath, string bitmapsDir, string h3ekPath, Loading loadingForm)
        {
            List<string> errorFiles = new List<string>();
            string[] tgaFiles = Directory.GetFiles(tgaOutputPath, "*.tga");
            foreach (string tgaFile in tgaFiles)
            {
                string tifOutputPath = (tgaFile.Replace("_00_00", "")).Replace(".tga", ".tif");

                using (var image = new MagickImage(tgaFile))
                {
                    image.Format = MagickFormat.Tif;
                    image.Write(tifOutputPath);
                }
            }

            string[] tifFiles = Directory.GetFiles(tgaOutputPath, "*.tif");

            // Delete TGA files
            foreach (string tgaFile in tgaFiles)
            {
                File.Delete(tgaFile);
            }

            // Move TIF files to scenario bitmaps directory
            if (!Directory.Exists(bitmapsDir))
            {
                Directory.CreateDirectory(bitmapsDir);
            }

            foreach (string tifFile in tifFiles)
            {
                string fileName = tifFile.Split('\\').Last();
                try
                {
                    File.Move(tifFile, bitmapsDir + "\\" + fileName);
                }
                catch (IOException)
                {
                    // File already exists, dont worry about it
                }
            }

            Console.WriteLine("Importing bitmaps...");
            loadingForm.UpdateOutputBox("Importing bitmaps...", false);
            string toolExePath = h3ekPath + @"\tool.exe";

            ToolRunner.RunTool(toolExePath, "bitmaps", h3ekPath, bitmapsDir, loadingForm);

            Console.WriteLine("Setting bitmap options...");
            loadingForm.UpdateOutputBox("Setting bitmap options...", false);
            List<BitmapData> allBitmapData = new List<BitmapData>();
            string[] xmlFiles = Directory.GetFiles(tgaOutputPath.Replace("textures_output", "bitmap_xml"), "*.xml");

            foreach (string xmlFile in xmlFiles)
            {
                XmlDocument bitmapFile = new XmlDocument();
                bitmapFile.Load(xmlFile);
                XmlNode root = bitmapFile.DocumentElement;
                string bitmapName = (new DirectoryInfo(xmlFile).Name).Replace(".xml", "");
                string usage = root.SelectSingleNode("./field[@name='usage']").InnerText.Trim();
                string compression = root.SelectSingleNode("./field[@name='format']").InnerText.Trim();
                string fadeFactor = root.SelectSingleNode("./field[@name='detail fade factor']").InnerText.Trim();
                string bumpHeight = root.SelectSingleNode("./field[@name='bump height']").InnerText.Trim();

                allBitmapData.Add(new BitmapData
                {
                    Bitmap = bitmapName,
                    Type = usage,
                    Compression = compression,
                    Fade = fadeFactor,
                    BumpHeight = bumpHeight
                });
            }

            foreach (BitmapData bitmapData in allBitmapData)
            {
                string bitmapFilePath = (bitmapsDir.Replace("data", "tags")).Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last() + "\\" + bitmapData.Bitmap;
                TagPath tagPath = TagPath.FromPathAndType(bitmapFilePath, "bitm*");

                try
                {
                    using (TagFile tagFile = new TagFile(tagPath))
                    {
                        // Usage
                        var type = (TagFieldEnum)tagFile.SelectField("LongEnum:Usage");
                        if (bitmapData.Type.Contains("default"))
                        {
                            type.Value = 0;
                        }
                        else if (bitmapData.Type.Contains("height"))
                        {
                            type.Value = 2;
                        }
                        else if (bitmapData.Type.Contains("detail"))
                        {
                            type.Value = 4;
                        }

                        // Compression
                        var compr = (TagFieldEnum)tagFile.SelectField("ShortEnum:force bitmap format");
                        if (bitmapData.Type.Contains("height"))
                        {
                            compr.Value = 3; // Best compressed bump
                        }
                        else if (bitmapData.Compression.Contains("color-key"))
                        {
                            compr.Value = 13; //DXT1
                        }
                        else if (bitmapData.Compression.Contains("explicit alpha"))
                        {
                            compr.Value = 14; //DXT3
                        }
                        else if (bitmapData.Compression.Contains("interpolated alpha"))
                        {
                            compr.Value = 15; //DXT5
                        }

                        // Curve mode - always set force pretty
                        var curve = (TagFieldEnum)tagFile.SelectField("CharEnum:curve mode");
                        curve.Value = 2; // force pretty

                        // Fade factor
                        var fade = (TagFieldElementSingle)tagFile.SelectField("RealFraction:fade factor");
                        fade.Data = float.Parse(bitmapData.Fade);

                        // Bump height
                        if (bitmapData.Type.Contains("height"))
                        {
                            var height = (TagFieldElementSingle)tagFile.SelectField("Real:bump map height");
                            if (float.Parse(bitmapData.BumpHeight) >= 15.0f)
                            {
                                height.Data = 10.0f;
                            }
                            else
                            {
                                height.Data = float.Parse(bitmapData.BumpHeight);
                            }
                        }

                        tagFile.Save();
                    }
                }
                catch (Bungie.Tags.TagLoadException)
                {
                    errorFiles.Add($"There was an issue loading the bitmap {bitmapFilePath}. It may not have been exported from H2 correctly.");
                }

            }

            // Run import again to update bitmaps
            Console.WriteLine("Reimport bitmaps...");
            loadingForm.UpdateOutputBox("Reimport bitmaps...", false);
            ToolRunner.RunTool(toolExePath, "bitmaps", h3ekPath, bitmapsDir, loadingForm);
            return errorFiles.ToArray();
        }

        static void AddShaderScaleFunc(TagFile tagFile, int type, int index, byte byte1, byte byte2, int animIndex)
        {
            // Add scale element
            ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{index}]/Block:animated parameters")).AddElement();

            var funcName = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{index}]/Block:animated parameters[{animIndex}]/LongEnum:type");
            funcName.Value = type; // 2 is scale uniform, 3 is scale x, 4 is scale y

            var funcData = (TagFieldData)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{index}]/Block:animated parameters[{animIndex}]/Struct:function[0]/Data:data");
            byte[] dataArray = new byte[32];
            dataArray[0] = 1; // Seems to set the "basic" type
            dataArray[6] = byte2; //byte2, unsigned
            dataArray[7] = byte1; // byte1
            funcData.SetData(dataArray);
        }

        static void MakeShaderTags(List<Shader> allShaderData, string bitmapsDir, string h3ekPath, Loading loadingForm)
        {
            string bitmapTagsDir = bitmapsDir.Replace("data", "tags").Split(new[] { "\\tags\\" }, StringSplitOptions.None).LastOrDefault();
            string shadersDir = (bitmapsDir.Split(new[] { "\\data\\" }, StringSplitOptions.None).LastOrDefault()).Replace("bitmaps", "shaders");

            CookSettings cookDiffuse = new CookSettings
            {
                SpecCoeff = 0.06366198f,
                Roughness = 0.3f,
                AreaContr = 0.3f,
                AnalContr = 0.5f,
                EnvContr = 0.0f
            };

            CookSettings cookDefault = new CookSettings
            {
                SpecCoeff = 0.2546479f,
                Roughness = 0.2f,
                AreaContr = 0.3f,
                AnalContr = 0.6f,
                EnvContr = 1.0f
            };

            CookSettings cookDull = new CookSettings
            {
                SpecCoeff = 0.06366198f,
                Roughness = 0.3f,
                AreaContr = 0.1f,
                AnalContr = 0.2f,
                EnvContr = 0.0f
            };

            CookSettings cookShiny = new CookSettings
            {
                SpecCoeff = 0.318309873f,
                Roughness = 0.1f,
                AreaContr = 0.2f,
                AnalContr = 0.5f,
                EnvContr = 1.0f
            };

            foreach (Shader shader in allShaderData)
            {

                if (shader.GlobMat.Contains("soft_organic_plant")) // Needs to be .shader_foliage
                {
                    string shaderName = Path.Combine(shadersDir, shader.Name);
                    var tagPath = TagPath.FromPathAndType(shaderName, "rmfl*");

                    // Create the tag
                    TagFile tagFile = new TagFile();
                    tagFile.New(tagPath);

                    // Set alpha test to simple
                    var alphaTest = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[1]/ShortInteger:short");
                    alphaTest.Data = 1;

                    // Global material
                    var globalMat = (TagFieldElementStringID)tagFile.SelectField("StringID:material name");
                    globalMat.Data = shader.GlobMat;

                    int paramIndex = 0;

                    foreach (Parameter param in shader.Parameters)
                    {
                        if (param.Name == "base_map")
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string baseMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add base map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "base_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set base map
                            var baseMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            baseMap.Path = TagPath.FromPathAndType(baseMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if (param.Name == "detail_map")
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string detailMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "detail_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set detail map
                            var detailMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            detailMap.Path = TagPath.FromPathAndType(detailMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if (param.Name == "bump_map" || param.Name == "lightmap_alphatest_map")
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string alphaMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            if (param.Name == "bump_map")
                            {
                                // Reimport bump map as colour map to get alpha test working
                                TagPath bitmapPath = TagPath.FromPathAndType(alphaMapPath, "bitm*");

                                using (var bitmapFile = new TagFile(bitmapPath))
                                {
                                    var compr = (TagFieldEnum)bitmapFile.SelectField("ShortEnum:force bitmap format");
                                    compr.Value = 2;
                                    bitmapFile.Save();
                                }

                                List<string> argumentList = new List<string>
                                {
                                    "reimport-bitmaps-single",
                                    "\"" + alphaMapPath + "\""
                                };

                                string arguments = string.Join(" ", argumentList);
                                string toolExePath = h3ekPath + @"\tool.exe";

                                Console.WriteLine($"Reimporting bitmap {bitmapFilename} as colour for shader foliage...");
                                loadingForm.UpdateOutputBox($"Reimporting bitmap {bitmapFilename} as colour for shader foliage...", false);
                                ToolRunner.RunTool(toolExePath, "reimport-bitmaps-single", h3ekPath, alphaMapPath, loadingForm);
                            }

                            // Add alpha map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "alpha_test_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set alpha map
                            var bumpMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            bumpMap.Path = TagPath.FromPathAndType(alphaMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }


                            //tagFile.Save();

                            paramIndex++;
                        }
                    }
                    tagFile.Save();
                }
                else if (shader.Template.Contains("plasma_alpha"))
                {
                    string shaderName = Path.Combine(shadersDir, shader.Name);
                    var tagPath = TagPath.FromPathAndType(shaderName, "rmsh*");

                    // Create the tag
                    TagFile tagFile = new TagFile();
                    tagFile.New(tagPath);

                    // Set pre multiplied alpha blend
                    var alphaOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[7]/ShortInteger:short");
                    alphaOption.Data = 5; // 5 for pre-multiplied

                    // Set plasma illum
                    var illumOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[6]/ShortInteger:short");
                    illumOption.Data = 3; // 5 for plasma

                    // Global material
                    var globalMat = (TagFieldElementStringID)tagFile.SelectField("StringID:material name");
                    globalMat.Data = shader.GlobMat;

                    int paramIndex = 0;

                    foreach (Parameter param in shader.Parameters)
                    {
                        if (param.Name == "noise_map_a")
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string noiseAMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add noise a map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "noise_map_a";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set noise a map
                            var noiseAMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            noiseAMap.Path = TagPath.FromPathAndType(noiseAMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if (param.Name == "noise_map_b")
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string noiseBMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add noise b map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "noise_map_b";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set noise b map
                            var noiseBMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            noiseBMap.Path = TagPath.FromPathAndType(noiseBMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if (param.Name == "alpha_map")
                        {
                            if (!string.IsNullOrEmpty(param.Bitmap) && File.Exists(param.Bitmap))
                            {
                                string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                                string alphaMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                                // Add alpha test map parameter as base
                                ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                                var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                                paramName.Data = "base_map";
                                var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                                paramType.Value = 0;

                                // Set alpha test map
                                var alphaMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                                alphaMap.Path = TagPath.FromPathAndType(alphaMapPath, "bitm*");

                                // Set aniso
                                var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                                flags.Data = 1;
                                var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                                aniso.Data = 6;

                                // Scale function data
                                byte byte1X = param.ScaleX2;
                                byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                                byte byte1Y = param.ScaleY2;
                                byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                                byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                                bool allZero = true;

                                foreach (byte scale in scales)
                                {
                                    if (scale != 0)
                                    {
                                        allZero = false;
                                        break;
                                    }
                                }

                                if (!allZero) // No need to bother if scale values arent provided
                                {
                                    if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                    {
                                        AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                    }
                                    else // Scale is non-uniform, handle separately
                                    {
                                        int animIndex = 0;
                                        if (!(byte1X == 0 && byte2X == 0))
                                        {
                                            AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                            animIndex++;
                                        }
                                        if (!(byte1Y == 0 && byte2Y == 0))
                                        {
                                            AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                        }
                                    }
                                }

                                paramIndex++;
                            }
                        }
                    }
                    tagFile.Save();
                }
                else
                {
                    string shaderName = Path.Combine(shadersDir, shader.Name);
                    var tagPath = TagPath.FromPathAndType(shaderName, "rmsh*");

                    // Create the tag
                    TagFile tagFile = new TagFile();
                    tagFile.New(tagPath);

                    // Set bump on
                    var bumpOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[1]/ShortInteger:short");
                    bumpOption.Data = 1; // 1 for standard bump

                    // Blend mode?
                    if (shader.Template.Contains("opaque\\overlay"))
                    {
                        // Set blend mode to double multiply
                        var blendOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[7]/ShortInteger:short");
                        blendOption.Data = 4;
                    }

                    // Global material
                    var globalMat = (TagFieldElementStringID)tagFile.SelectField("StringID:material name");
                    globalMat.Data = shader.GlobMat;

                    // Specular mask
                    var specMaskOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[3]/ShortInteger:short");
                    specMaskOption.Data = 1;

                    int paramIndex = 0;

                    // Cook torrance data
                    if (shader.SpecCol != "")
                    {
                        var matModelOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[4]/ShortInteger:short");
                        matModelOption.Data = 1;

                        // Add new parameter
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                        // Set parameter name
                        var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        paramName.Data = "specular_tint";

                        // Set parameter type
                        var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        paramType.Value = 1; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                        // Add animated parameter
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();

                        // Set animated parameter type to colour
                        var animParamType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                        animParamType.Value = 1; // 0 is "scale uniform", 1 is "color"

                        // Set function data to RGB colour
                        TagFieldCustomFunctionEditor specTintFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                        specTintFunc.Value.ColorGraphType = FunctionEditorColorGraphType.OneColor;
                        specTintFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                        float[] specColour = shader.SpecCol.Split(',').Select(float.Parse).ToArray();
                        specTintFunc.Value.SetColor(0, GameColor.FromRgb(specColour[0], specColour[1], specColour[2]));
                        paramIndex++;

                        if (shader.SpecGlnc != "")
                        {
                            // Add new parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                            // Set parameter name
                            var specGlanceName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            specGlanceName.Data = "fresnel_color";

                            // Set parameter type
                            var specGlanceType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            specGlanceType.Value = 1; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                            // Add animated parameter
                            ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();

                            // Set animated parameter type to colour
                            var specGlanceAnimType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                            specGlanceAnimType.Value = 1; // 0 is "scale uniform", 1 is "color"

                            // Set function data to RGB colour
                            TagFieldCustomFunctionEditor specGlanceFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                            specGlanceFunc.Value.ColorGraphType = FunctionEditorColorGraphType.OneColor;
                            specGlanceFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                            float[] specGlanceColour = shader.SpecGlnc.Split(',').Select(float.Parse).ToArray();
                            specGlanceFunc.Value.SetColor(0, GameColor.FromRgb(specGlanceColour[0], specGlanceColour[1], specGlanceColour[2]));
                            paramIndex++;
                        }

                        // Set generic cook torrance values
                        // Specular coefficient
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var specCoeffName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        specCoeffName.Data = "specular_coefficient";
                        var specCoeffType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        specCoeffType.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();
                        var animType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                        animType.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor specCoeffFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                        specCoeffFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                        paramIndex++;

                        // Roughness
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var roughnessName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        roughnessName.Data = "roughness";
                        var roughnessType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        roughnessType.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();
                        var roughnessAnimType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                        roughnessAnimType.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor roughFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                        roughFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                        paramIndex++;

                        // Area specular contribution
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var areaName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        areaName.Data = "area_specular_contribution";
                        var areaType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        areaType.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();
                        var areaAnimType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                        areaAnimType.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor areaFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                        areaFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                        paramIndex++;

                        // Analytical specular contribution
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var analName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        analName.Data = "analytical_specular_contribution";
                        var analType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        analType.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();
                        var analAnimType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                        analAnimType.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor analFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                        analFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                        paramIndex++;

                        // Env map specular contribution
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                        var envName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        envName.Data = "environment_map_specular_contribution";
                        var envType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        envType.Value = 2; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();
                        var envAnimType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                        envAnimType.Value = 0; // 0 is "value", 1 is "color", 2 is "scale uniform", 3 is "scale x", 4 is "scale y"
                        TagFieldCustomFunctionEditor envFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                        envFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                        paramIndex++;

                        // Values need to change depending on h2 specular setting
                        if (shader.SpecType == "0,diffuse")
                        {
                            specCoeffFunc.Value.ClampRangeMin = cookDiffuse.SpecCoeff;
                            roughFunc.Value.ClampRangeMin = cookDiffuse.Roughness;
                            areaFunc.Value.ClampRangeMin = cookDiffuse.AreaContr;
                            analFunc.Value.ClampRangeMin = cookDiffuse.AnalContr;
                            envFunc.Value.ClampRangeMin = cookDiffuse.EnvContr;
                        }
                        else if (shader.SpecType == "1,default specular")
                        {
                            specCoeffFunc.Value.ClampRangeMin = cookDefault.SpecCoeff;
                            roughFunc.Value.ClampRangeMin = cookDefault.Roughness;
                            areaFunc.Value.ClampRangeMin = cookDefault.AreaContr;
                            analFunc.Value.ClampRangeMin = cookDefault.AnalContr;
                            envFunc.Value.ClampRangeMin = cookDefault.EnvContr;
                        }
                        else if (shader.SpecType == "2,dull specular")
                        {
                            specCoeffFunc.Value.ClampRangeMin = cookDull.SpecCoeff;
                            roughFunc.Value.ClampRangeMin = cookDull.Roughness;
                            areaFunc.Value.ClampRangeMin = cookDull.AreaContr;
                            analFunc.Value.ClampRangeMin = cookDull.AnalContr;
                            envFunc.Value.ClampRangeMin = cookDull.EnvContr;
                        }
                        else if (shader.SpecType == "3,shiny specular")
                        {
                            specCoeffFunc.Value.ClampRangeMin = cookShiny.SpecCoeff;
                            roughFunc.Value.ClampRangeMin = cookShiny.Roughness;
                            areaFunc.Value.ClampRangeMin = cookShiny.AreaContr;
                            analFunc.Value.ClampRangeMin = cookShiny.AnalContr;
                            envFunc.Value.ClampRangeMin = cookShiny.EnvContr;
                        }
                        else
                        {
                            Console.WriteLine($"Shader {shader.Name} had an invalid specular setting, odd.");
                            loadingForm.UpdateOutputBox($"Shader {shader.Name} had an invalid specular setting, odd.", false);
                        }
                    }

                    // Dynamic env mapping?
                    if (shader.EnvTint != "" && shader.EnvBitmap == "" && shader.SpecCol != "")
                    {
                        var envMapping = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[5]/ShortInteger:short");
                        envMapping.Data = 2; // 2 is dynamic

                        // Add new parameter
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                        // Set parameter name
                        var envParamName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        envParamName.Data = "env_tint_color";

                        // Set parameter type
                        var envParamType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        envParamType.Value = 1; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                        // Add animated parameter
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();

                        // Set animated parameter type to colour
                        var animParamType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                        animParamType.Value = 1; // 0 is "scale uniform", 1 is "color"

                        // Set function data to RGB colour
                        TagFieldCustomFunctionEditor envColFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                        envColFunc.Value.ColorGraphType = FunctionEditorColorGraphType.OneColor;
                        envColFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                        float[] envColour = shader.EnvTint.Split(',').Select(float.Parse).ToArray();
                        envColFunc.Value.SetColor(0, GameColor.FromRgb(envColour[0], envColour[1], envColour[2]));
                        paramIndex++;
                    }

                    // Per-pixel env mapping
                    else if (shader.EnvTint != "" && shader.EnvBitmap != "")
                    {
                        var envMapping = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[5]/ShortInteger:short");
                        envMapping.Data = 1; // 1 is per-pixel

                        // Add new parameter
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                        // Set parameter name
                        var envParamName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        envParamName.Data = "env_tint_color";

                        // Set parameter type
                        var envParamType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        envParamType.Value = 1; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                        // Add animated parameter
                        ((TagFieldBlock)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters")).AddElement();

                        // Set animated parameter type to colour
                        var animParamType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/LongEnum:type");
                        animParamType.Value = 1; // 0 is "scale uniform", 1 is "color"

                        // Set function data to RGB colour
                        TagFieldCustomFunctionEditor envColFunc = (TagFieldCustomFunctionEditor)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Block:animated parameters[0]/Custom:animation function");
                        envColFunc.Value.ColorGraphType = FunctionEditorColorGraphType.OneColor;
                        envColFunc.Value.MasterType = FunctionEditorMasterType.Basic;
                        float[] envColour = shader.EnvTint.Split(',').Select(float.Parse).ToArray();
                        envColFunc.Value.SetColor(0, GameColor.FromRgb(envColour[0], envColour[1], envColour[2]));
                        paramIndex++;

                        // Add new parameter for env bitmap
                        ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();

                        // Set parameter name
                        var envBitmapName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                        envBitmapName.Data = "environment_map";

                        // Set parameter type
                        var envBitmapType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                        envBitmapType.Value = 0; // 0 is "bitmap", 1 is "color", 2 is "real", 3 is "int"

                        // Set env bitmap
                        var envBitmap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                        envBitmap.Path = TagPath.FromPathAndType(Path.Combine(bitmapTagsDir, new DirectoryInfo(shader.EnvBitmap).Name), "bitm*");
                        paramIndex++;
                    }

                    foreach (Parameter param in shader.Parameters)
                    {
                        if (param.Name == "base_map")
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string baseMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add base map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "base_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set base map
                            var baseMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            baseMap.Path = TagPath.FromPathAndType(baseMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if (param.Name == "detail_map" && !shader.Template.Contains("three"))
                        {
                            string bitmapFileName = new DirectoryInfo(param.Bitmap).Name;
                            string detailMapPath = Path.Combine(bitmapTagsDir, bitmapFileName);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "detail_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set detail map
                            var detailMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            detailMap.Path = TagPath.FromPathAndType(detailMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if (param.Name == "overlay_detail_map" && !shader.Template.Contains("detail"))
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string detailMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "detail_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set detail map
                            var detailMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            detailMap.Path = TagPath.FromPathAndType(detailMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if ((param.Name == "detail_map_a" || param.Name == "blend_detail_map_1") && shader.Template.Contains("detail"))
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string detailMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "detail_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set detail map
                            var detailMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            detailMap.Path = TagPath.FromPathAndType(detailMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if ((param.Name == "secondary_detail_map" || param.Name == "detail_map_b" || param.Name == "blend_detail_map_2") && shader.Template.Contains("detail"))
                        {
                            // Set two detail
                            var albedoOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[0]/ShortInteger:short");
                            albedoOption.Data = 1; // 7 for detail blend

                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string secDetailMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "detail_map2";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set detail map
                            var detail2Map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            detail2Map.Path = TagPath.FromPathAndType(secDetailMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if ((param.Name == "detail_map_c" || param.Name == "overlay_detail_map") && shader.Template.Contains("detail"))
                        {
                            // Either two_detail_overlay or three_detail_blend

                            // Set albedo type (2DT or 3DB)
                            var albedoOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[0]/ShortInteger:short");
                            if (shader.Template.Contains("detail_blend_detail"))
                            {
                                albedoOption.Data = 6; // 6 for two detail overlay
                            }
                            else
                            {
                                albedoOption.Data = 5; // 5 for three detail blend
                            }

                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string secDetailMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add detail map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            if (shader.Template.Contains("detail_blend_detail"))
                            {
                                paramName.Data = "detail_map_overlay";
                            }
                            else
                            {
                                paramName.Data = "detail_map3";
                            }
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set detail map
                            var detail2Map = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            detail2Map.Path = TagPath.FromPathAndType(secDetailMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if (param.Name == "bump_map")
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string bumpMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Add bump map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "bump_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set bump map
                            var bumpMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            bumpMap.Path = TagPath.FromPathAndType(bumpMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }


                            //tagFile.Save();

                            paramIndex++;
                        }

                        if ((param.Name == "alpha_test_map" || param.Name == "lightmap_alphatest_map") && !shader.Template.Contains("detail_blend")) 
                        {
                            // Enable alpha test
                            var albedoOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[2]/ShortInteger:short");
                            albedoOption.Data = 1;

                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string alphaMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Reimport diffuse map as DXT5 to make sure the alpha works
                            TagPath bitmapPath = TagPath.FromPathAndType(alphaMapPath, "bitm*");

                            using (var bitmapFile = new TagFile(bitmapPath))
                            {
                                var compr = (TagFieldEnum)bitmapFile.SelectField("ShortEnum:force bitmap format");
                                compr.Value = 15; // 15 for DXT5
                                bitmapFile.Save();
                            }

                            List<string> argumentList = new List<string>
                            {
                                "reimport-bitmaps-single",
                                "\"" + alphaMapPath + "\""
                            };

                            string arguments = string.Join(" ", argumentList);
                            string toolExePath = h3ekPath + @"\tool.exe";

                            Console.WriteLine($"Reimporting bitmap {bitmapFilename} as DXT5 to make sure alpha works for transparency...");
                            loadingForm.UpdateOutputBox($"Reimporting bitmap {bitmapFilename} as DXT5 to make sure alpha works for transparency...", false);
                            ToolRunner.RunTool(toolExePath, "reimport-bitmaps-single", h3ekPath, alphaMapPath, loadingForm);

                            // Add alpha test map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "alpha_test_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set alpha test map
                            var alphaMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            alphaMap.Path = TagPath.FromPathAndType(alphaMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }

                            paramIndex++;
                        }

                        if (param.Name == "self_illum_map" && shader.Template.Contains("illum"))
                        {
                            string bitmapFilename = new DirectoryInfo(param.Bitmap).Name;
                            string illumMapPath = Path.Combine(bitmapTagsDir, bitmapFilename);

                            // Enable self-illum
                            var illumOption = (TagFieldElementInteger)tagFile.SelectField("Struct:render_method[0]/Block:options[6]/ShortInteger:short");
                            if (shader.Template.Contains("3_channel"))
                            {
                                illumOption.Data = 2; // 3 channel self illum
                            }
                            else
                            {
                                illumOption.Data = 1; // simple self illum
                            }


                            // Add illum map parameter
                            ((TagFieldBlock)tagFile.SelectField("Struct:render_method[0]/Block:parameters")).AddElement();
                            var paramName = (TagFieldElementStringID)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/StringID:parameter name");
                            paramName.Data = "self_illum_map";
                            var paramType = (TagFieldEnum)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/LongEnum:parameter type");
                            paramType.Value = 0;

                            // Set illum map
                            var illumMap = (TagFieldReference)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/Reference:bitmap");
                            illumMap.Path = TagPath.FromPathAndType(illumMapPath, "bitm*");

                            // Set aniso
                            var flags = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap flags");
                            flags.Data = 1;
                            var aniso = (TagFieldElementInteger)tagFile.SelectField($"Struct:render_method[0]/Block:parameters[{paramIndex}]/ShortInteger:bitmap filter mode");
                            aniso.Data = 6;

                            // Scale function data
                            byte byte1X = param.ScaleX2;
                            byte byte2X = (byte)(256 + param.ScaleX1); // Convert to unsigned
                            byte byte1Y = param.ScaleY2;
                            byte byte2Y = (byte)(256 + param.ScaleY1); // Convert to unsigned
                            byte[] scales = new byte[] { byte1X, byte2X, byte1Y, byte2Y };
                            bool allZero = true;

                            foreach (byte scale in scales)
                            {
                                if (scale != 0)
                                {
                                    allZero = false;
                                    break;
                                }
                            }

                            if (!allZero) // No need to bother if scale values arent provided
                            {
                                if ((byte1X == byte1Y) && (byte2X == byte2Y)) // Uniform scale check
                                {
                                    AddShaderScaleFunc(tagFile, 2, paramIndex, byte1X, byte2X, 0);
                                }
                                else // Scale is non-uniform, handle separately
                                {
                                    int animIndex = 0;
                                    if (!(byte1X == 0 && byte2X == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 3, paramIndex, byte1X, byte2X, animIndex);
                                        animIndex++;
                                    }
                                    if (!(byte1Y == 0 && byte2Y == 0))
                                    {
                                        AddShaderScaleFunc(tagFile, 4, paramIndex, byte1Y, byte2Y, animIndex);
                                    }
                                }
                            }


                            //tagFile.Save();

                            paramIndex++;
                        }
                    }
                    tagFile.Save();
                }
            }
        }
    }
}
