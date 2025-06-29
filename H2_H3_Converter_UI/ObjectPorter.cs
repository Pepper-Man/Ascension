﻿using Bungie;
using Bungie.Tags;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace H2_H3_Converter_UI
{
    internal class ObjectPorter
    {
        public static string[] GetTagsForModel(TagPath objectTagPath, string h2ekPath, string h3ekPath, string h2RelativeObjectTagFolder, Loading loadingForm)
        {
            ConcurrentBag<string> extractedTags = new ConcurrentBag<string>();
            ConcurrentBag<string> jmsPathsForShaders = new ConcurrentBag<string>();
            int tagsFolderOffset = h2ekPath.Length + @"\tags\".Length;

            // Determine H2 Tool.exe path
            string h2ToolPath = h2ekPath + @"\tool.exe";

            // Determine H3 Tool.exe path
            string h3ToolPath = h3ekPath + @"\tool.exe";

            string h2FullObjectTagFolder = Path.GetDirectoryName(Path.Combine(h2ekPath, "tags", h2RelativeObjectTagFolder));
            string renderFullH2Path = "";
            string collisionFullH2Path = "";
            string physicsFullH2Path = "";

            // Attempt to locate render, collision and physics tags
            try { renderFullH2Path = Directory.GetFiles(h2FullObjectTagFolder, "*.render_model", SearchOption.TopDirectoryOnly).First(); }
            catch (InvalidOperationException) { loadingForm.UpdateOutputBox($"Could not locate render model tag in \"{h2FullObjectTagFolder}\", skipping...", false); }

            try { collisionFullH2Path = Directory.GetFiles(h2FullObjectTagFolder, "*.collision_model", SearchOption.TopDirectoryOnly).First(); }
            catch (InvalidOperationException) { loadingForm.UpdateOutputBox($"Could not locate collision model tag in \"{h2FullObjectTagFolder}\", skipping...", false); }

            try { physicsFullH2Path = Directory.GetFiles(h2FullObjectTagFolder, "*.physics_model", SearchOption.TopDirectoryOnly).First(); }
            catch (InvalidOperationException) { loadingForm.UpdateOutputBox($"Could not locate physics model tag in \"{h2FullObjectTagFolder}\", skipping...", false); }

            var modelTypes = new List<(string fullPath, string type, string extractCmd)>
            {
                (renderFullH2Path, "render", "extract-render-data"),
                (collisionFullH2Path, "collision", "extract-collision-data"),
                (physicsFullH2Path, "physics", "extract-physics-data")
            };

            // Exporting and moving
            Parallel.ForEach(modelTypes.Where(m => File.Exists(m.fullPath)), modelTag =>
            {
                // Export
                string relativePath = modelTag.fullPath.Substring(tagsFolderOffset);
                loadingForm.UpdateOutputBox($"Extracting {relativePath}", false);
                ToolRunner.RunTool(h2ToolPath, modelTag.extractCmd, h2ekPath, relativePath);
                extractedTags.Add(Path.Combine("halo_2", relativePath));

                // Move
                string jmsFile = MoveJMSToH3(relativePath, modelTag.fullPath, h3ekPath, modelTag.type, loadingForm);

                // Queue shader creation if needed - has to be done on main thread
                if (modelTag.type == "render")
                {
                    jmsPathsForShaders.Add(jmsFile);
                }
            });

            // Shader generation, on main thread
            foreach (string jmsPath in jmsPathsForShaders)
            {
                ShadersFromJMS(jmsPath, loadingForm);
            }

            // H3 importing - can run this in parallel
            Parallel.ForEach(modelTypes.Where(m => File.Exists(m.fullPath)), modelTag =>
            {
                string relativePath = modelTag.fullPath.Substring(tagsFolderOffset);
                string importFolder = Path.Combine("halo_2", Path.GetDirectoryName(relativePath));
                ToolRunner.RunTool(h3ToolPath, modelTag.type, h3ekPath, importFolder);
            });

            return extractedTags.ToArray();
        }

        private static string MoveJMSToH3(string relativeH2TagPath, string fullH2TagPath, string h3ekPath, string type, Loading loadingForm)
        {
            // Get location of JMS in H2EK\data\!extracted
            string marker = "H2EK\\tags" + Path.DirectorySeparatorChar;
            int markerIndex = fullH2TagPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex == -1)
            {
                throw new IOException($"Error when attempting to locate JMS file of \"{relativeH2TagPath}\"");
            }

            // Extract root up to "H2EK\\data\\"
            string root = fullH2TagPath.Substring(0, markerIndex + marker.Length).Replace("H2EK\\tags", "H2EK\\data");

            // Get file name and object folder
            string filename = Path.GetFileNameWithoutExtension(fullH2TagPath);
            string objectFolder = Path.GetFileName(Path.GetDirectoryName(fullH2TagPath));

            // Rebuild new path
            string fullH2JMSPath = Path.ChangeExtension(Path.Combine(root, "!extracted", objectFolder, type, filename), "jms");

            // Determine correct location for the file in the H3EK data folder
            string fullH3JMSPath = Path.Combine(h3ekPath, "data\\halo_2", Path.GetDirectoryName(relativeH2TagPath), type, Path.GetFileName(relativeH2TagPath));

            // Move that file!
            try
            {
                return MoveFile(Path.GetDirectoryName(fullH2JMSPath), Path.GetDirectoryName(fullH3JMSPath));
            }
            catch (Exception ex)
            {
                throw new IOException($"Error when moving folder \"{Path.GetDirectoryName(fullH2JMSPath)}\" to \"{Path.GetDirectoryName(fullH3JMSPath)}\"! Exception: {ex}");
            }
        }

        private static string MoveFile(string sourceFolder, string destinationFolder)
        {
            if (!Directory.Exists(sourceFolder))
            {
                throw new DirectoryNotFoundException($"Source .JMS folder not found: {sourceFolder}");
            }

            // Create the output directory if it doesn't exist
            Directory.CreateDirectory(destinationFolder);

            foreach (var filePath in Directory.GetFiles(sourceFolder, "*.jms", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(filePath);
                string destinationPath = Path.Combine(destinationFolder, fileName);

                // Create any needed subdirectories
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

                if (!File.Exists(destinationPath))
                {
                    File.Copy(filePath, destinationPath);
                }
            }

            return destinationFolder;
        }

        private static void ShadersFromJMS(string jmsPath, Loading loadingForm)
        {
            loadingForm.UpdateOutputBox($"Beginning shader creation for \"{jmsPath}\"", false);

            // Simple check for files in shaders folder - let's not mess with/regenerate shaders if they already exist
            string destinationShadersPath = Path.Combine(Path.GetDirectoryName(jmsPath).Replace("H3EK\\data", "H3EK\\tags").TrimEnd(), "shaders");

            try
            {
                if (!(Directory.GetFiles(destinationShadersPath) == Array.Empty<string>()))
                {
                    loadingForm.UpdateOutputBox("Shaders folder exists and has files! Skipping shader generation...", false);
                    return;
                }
                else
                {
                    // Shaders folder exists but is empty, good to go
                    loadingForm.UpdateOutputBox("Shaders folder exists but is empty, proceeding with shader generation...", false);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // No shaders folder exists, so we're good to go
                loadingForm.UpdateOutputBox("Shaders folder does not exist, proceeding with shader generation...", false);
            }

            // Get all the material names from the render JMS file
            string[] materials = ReadJMSMaterials(jmsPath);

            // Aaaand send them off to be made
            CreateShaderTags(materials, destinationShadersPath);
        }

        // Modified from similar code I wrote for Osoyoos
        private static string[] ReadJMSMaterials(string jmsFolderPath)
        {
            // Get all .jms files directly in the folder (no subdirectories)
            string[] jmsFiles = Directory.GetFiles(jmsFolderPath, "*.jms", SearchOption.TopDirectoryOnly);

            List<string> shaders = new List<string>();
            string[] extras = { "lm:", "lp:", "hl:", "ds:", "pf:", "lt:", "to:", "at:", "ro:" };
            string shaderNameStripped;

            foreach (string jmsPath in jmsFiles)
            {
                string line;
                int counter = 0;

                using (StreamReader reader = new StreamReader(jmsPath))
                {
                    // Find materials definition header in JMS file
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains(";### MATERIALS ###"))
                        {
                            break;
                        }
                        counter++;
                    }

                    // Grab number of materials from file
                    int materialCount = int.Parse(File.ReadLines(jmsPath).Skip(counter + 1).Take(1).First());

                    // Line number of first material name
                    int currentLine = counter + 7;

                    // Take each material name, strip symbols, add to list
                    // Typically the most "complex" materials come in the format: prefix name extra1 extra2 extra3...
                    // I am also ignoring shader collections here as the collections are unlikely to exist
                    for (int i = 0; i < materialCount; i++)
                    {
                        string[] shaderNameSections = File.ReadLines(jmsPath).Skip(currentLine - 1).Take(1).First().Split(' ');
                        if (shaderNameSections.Length < 2)
                        {
                            shaderNameStripped = Regex.Replace(shaderNameSections[0], "[^0-9a-zA-Z_.]", string.Empty);
                        }
                        else // More complex material name
                        {
                            shaderNameStripped = Regex.Replace(shaderNameSections[1], "[^0-9a-zA-Z_.]", string.Empty).Trim();
                        }

                        shaders.Add(shaderNameStripped);
                        currentLine += 4;
                    }
                }
            }

            return shaders.Distinct().ToArray();
        }

        private static void CreateShaderTags(string[] shadersToMake, string shadersFolder)
        {
            // Make the shaders folder if it doesn't exist already
            Directory.CreateDirectory(shadersFolder);

            // Get relative shader folder path
            string marker = @"H3EK\tags\";
            int index = shadersFolder.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            string relativeShadersFolder = shadersFolder.Substring(index + marker.Length);

            // Create tag for each shader
            foreach (string shaderName in shadersToMake)
            {
                TagPath shaderPath = TagPath.FromPathAndExtension(Path.Combine(relativeShadersFolder, shaderName), "shader");
                TagFile tagFile = new TagFile();
                tagFile.New(shaderPath);
                tagFile.Save();
            }
        }
    }
}
