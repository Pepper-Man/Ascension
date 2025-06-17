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
        public static void GetTagsForModel(TagPath objectTagPath, string h2ekPath, Loading loadingForm)
        {
            bool render = false;
            bool collision = false;
            bool physics = false;

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

            //// Determine which tags exist
            //if (File.Exists(renderFullH2Path)) { render = true; }
            //if (File.Exists(collisionFullH2Path)) { collision = true; }
            //if (File.Exists(physicsFullH2Path)) { physics = true; }

            // Determine H2 Tool.exe path
            string toolExePath = h2ekPath + @"\tool.exe";

            // Now extract existing files
            if (File.Exists(renderFullH2Path))
            {
                loadingForm.UpdateOutputBox($"Extracting {renderRelativeH2Path}", false);
                ExtractTag(renderRelativeH2Path, toolExePath, "extract-render-data", h2ekPath);
            }
            if (File.Exists(collisionFullH2Path))
            {
                loadingForm.UpdateOutputBox($"Extracting {collisionRelativeH2Path}", false);
                ExtractTag(collisionRelativeH2Path, toolExePath, "extract-collision-data", h2ekPath);
            }
            if (File.Exists(physicsFullH2Path))
            {
                loadingForm.UpdateOutputBox($"Extracting {physicsRelativeH2Path}", false);
                ExtractTag(physicsRelativeH2Path, toolExePath, "extract-physics-data", h2ekPath);
            }
        }

        private static void ExtractTag(string tagPath, string toolExePath, string extractCommand, string h2ekPath)
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
