using Bungie;
using Bungie.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace H2_H3_Converter_UI
{
    internal class ObjectPorter
    {
        public static string[] GetTagsForModel(TagPath objectTagPath, string h2ekPath, string h3ekPath, Loading loadingForm)
        {
            List<string> extractedTags = new List<string>();

            // Remove "halo_2"
            string[] parts = objectTagPath.RelativePathWithExtension.Split(Path.DirectorySeparatorChar);
            string h2ObjectTagPath = Path.Combine(parts.Skip(1).ToArray());

            // Determine relative filepaths for render, collision and physics models (they may not actually exist, so we need to check)
            string renderRelativeH2Path = Path.ChangeExtension(h2ObjectTagPath, "render_model");
            string collisionRelativeH2Path = Path.ChangeExtension(h2ObjectTagPath, "collision_model");
            string physicsRelativeH2Path = Path.ChangeExtension(h2ObjectTagPath, "physics_model");

            // Determine full filepaths
            string renderFullH2Path = Path.Combine(h2ekPath, "tags\\", renderRelativeH2Path);
            string collisionFullH2Path = Path.Combine(h2ekPath, "tags\\", collisionRelativeH2Path);
            string physicsFullH2Path = Path.Combine(h2ekPath, "tags\\", physicsRelativeH2Path);

            // Determine H2 Tool.exe path
            string h2ToolPath = h2ekPath + @"\tool.exe";

            // Determine H3 Tool.exe path
            string h3ToolPath = h3ekPath + @"\tool.exe";

            // Now extract existing files, then import with H3 tool
            if (File.Exists(renderFullH2Path))
            {
                // Extraction
                loadingForm.UpdateOutputBox($"Extracting {renderRelativeH2Path}", false);
                RunTool(renderRelativeH2Path, h2ToolPath, "extract-render-data", h2ekPath);
                extractedTags.Add(renderFullH2Path);
                // File moving
                string jmsFile = MoveJMSToH3(renderRelativeH2Path, renderFullH2Path, h3ekPath, "render", loadingForm);
                // Shader creation
                ShadersFromJMS(jmsFile, loadingForm);
                // Importing
                RunTool(Path.Combine("halo_2", Path.GetDirectoryName(renderRelativeH2Path)), h3ToolPath, "render", h3ekPath);
            }
            if (File.Exists(collisionFullH2Path))
            {
                // Extraction
                loadingForm.UpdateOutputBox($"Extracting {collisionRelativeH2Path}", false);
                RunTool(collisionRelativeH2Path, h2ToolPath, "extract-collision-data", h2ekPath);
                extractedTags.Add(collisionFullH2Path);
                // File moving
                MoveJMSToH3(collisionRelativeH2Path, collisionFullH2Path, h3ekPath, "collision", loadingForm);
                // Importing
                RunTool(Path.Combine("halo_2", Path.GetDirectoryName(collisionRelativeH2Path)), h3ToolPath, "collision", h3ekPath);
            }
            if (File.Exists(physicsFullH2Path))
            {
                // Extraction
                loadingForm.UpdateOutputBox($"Extracting {physicsRelativeH2Path}", false);
                RunTool(physicsRelativeH2Path, h2ToolPath, "extract-physics-data", h2ekPath);
                extractedTags.Add(physicsFullH2Path);
                // File moving
                MoveJMSToH3(physicsRelativeH2Path, physicsFullH2Path, h3ekPath, "physics", loadingForm);
                // Importing
                RunTool(Path.Combine("halo_2", Path.GetDirectoryName(physicsRelativeH2Path)), h3ToolPath, "physics", h3ekPath);
            }

            Console.WriteLine("Done importing!");
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
                return MoveFile(fullH2JMSPath, Path.ChangeExtension(fullH3JMSPath, "jms"));
            }
            catch (Exception ex)
            {
                throw new IOException($"Error when moving file \"{fullH2JMSPath}\" to \"{fullH3JMSPath}\"! Exception: {ex}");
            }
        }

        private static string MoveFile(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"Source .JMS file not found: {sourcePath}");
            }

            string destDir = Path.GetDirectoryName(destinationPath);
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(destDir);

            // Never overwrite
            if (!File.Exists(destinationPath))
            {
                File.Move(sourcePath, destinationPath);
            }

            return destinationPath;
        }

        private static void ShadersFromJMS(string jmsPath, Loading loadingForm)
        {
            loadingForm.UpdateOutputBox($"Beginning shader creation for \"{jmsPath}\"", false);

            // Simple check for files in shaders folder - let's not mess with/regenerate shaders if they already exist
            string destinationShadersPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(jmsPath).Replace("H3EK\\data", "H3EK\\tags").TrimEnd()), "shaders");

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
        private static string[] ReadJMSMaterials(string jmsPath)
        {
            string line;
            List<string> shaders = new List<string>();
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
                string[] extras = { "lm:", "lp:", "hl:", "ds:", "pf:", "lt:", "to:", "at:", "ro:" };
                string shaderNameStripped;

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

            shaders = shaders.Distinct().ToList();
            return shaders.ToArray();
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
        
        private static void RunTool(string filePath, string toolExePath, string command, string editingKitPath)
        {
            List<string> argumentsList = new List<string>();

            if (command.Contains("extract"))
            {
                argumentsList.Add(command);
                argumentsList.Add("\"" + Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "\"");
            }
            else if (command == "render")
            {
                argumentsList.Add(command);
                argumentsList.Add("\"" + filePath + "\"");
                argumentsList.Add("draft");
            }
            else if (command == "collision")
            {
                argumentsList.Add(command);
                argumentsList.Add("\"" + filePath + "\"");
            }
            else if (command == "physics")
            {
                argumentsList.Add(command);
                argumentsList.Add("\"" + filePath + "\"");
            }
            else
            {
                argumentsList.Add(command);
            }

                string arguments = string.Join(" ", argumentsList);

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = toolExePath,
                Arguments = arguments,
                WorkingDirectory = editingKitPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = processStartInfo
            };

            process.Start();

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
    }
}
