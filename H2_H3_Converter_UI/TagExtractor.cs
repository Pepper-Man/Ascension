using Bungie;
using Bungie.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H2_H3_Converter_UI
{
    internal class TagExtractor
    {
        public static void GetTagsForModel(TagPath objectTagPath, string h2ekPath, string h3ekPath, Loading loadingForm)
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
            string toolExePath = h2ekPath + @"\tool.exe";

            // Now extract existing files
            if (File.Exists(renderFullH2Path))
            {
                loadingForm.UpdateOutputBox($"Extracting {renderRelativeH2Path}", false);
                ToolExtractTag(renderRelativeH2Path, toolExePath, "extract-render-data", h2ekPath);
                extractedTags.Add(renderFullH2Path);
                MoveJMSToH3(renderRelativeH2Path, renderFullH2Path, h3ekPath, "render", loadingForm);
            }
            if (File.Exists(collisionFullH2Path))
            {
                loadingForm.UpdateOutputBox($"Extracting {collisionRelativeH2Path}", false);
                ToolExtractTag(collisionRelativeH2Path, toolExePath, "extract-collision-data", h2ekPath);
                extractedTags.Add(collisionFullH2Path);
                MoveJMSToH3(collisionRelativeH2Path, collisionFullH2Path, h3ekPath, "collision", loadingForm);
            }
            if (File.Exists(physicsFullH2Path))
            {
                loadingForm.UpdateOutputBox($"Extracting {physicsRelativeH2Path}", false);
                ToolExtractTag(physicsRelativeH2Path, toolExePath, "extract-physics-data", h2ekPath);
                extractedTags.Add(physicsFullH2Path);
                MoveJMSToH3(physicsRelativeH2Path, physicsFullH2Path, h3ekPath, "physics", loadingForm);
            }
        }

        private static void MoveJMSToH3(string relativeH2TagPath, string fullH2TagPath, string h3ekPath, string type, Loading loadingForm)
        {
            // Get location of JMS in H2EK\data\!extracted
            string marker = "H2EK\\tags" + Path.DirectorySeparatorChar;
            int markerIndex = fullH2TagPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex == -1)
            {
                Console.WriteLine($"Error when attempting to locate JMS file of \"{relativeH2TagPath}\"");
                loadingForm.UpdateOutputBox($"Error when attempting to locate JMS file of \"{relativeH2TagPath}\"", false);
                return;
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
                MoveFile(fullH2JMSPath, Path.ChangeExtension(fullH3JMSPath, "jms"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when moving file \"{fullH2JMSPath}\" to \"{fullH3JMSPath}\"! Exception: {ex.ToString()}");
                loadingForm.UpdateOutputBox($"Error when moving file \"{fullH2JMSPath}\" to \"{fullH3JMSPath}\"! Exception: {ex.ToString()}", false);
            }
            
        }

        private static void MoveFile(string sourcePath, string destinationPath)
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
        }

        private static void ToolExtractTag(string tagPath, string toolExePath, string extractCommand, string h2ekPath)
        {
            List<string> argumentsList = new List<string>
            {
                extractCommand,
                "\"" + Path.Combine(Path.GetDirectoryName(tagPath), Path.GetFileNameWithoutExtension(tagPath)) + "\""
            };

            string arguments = string.Join(" ", argumentsList);

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = toolExePath,
                Arguments = arguments,
                WorkingDirectory = h2ekPath,
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
