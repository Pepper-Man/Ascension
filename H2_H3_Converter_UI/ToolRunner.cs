using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H2_H3_Converter_UI
{
    internal class ToolRunner
    {
        // Overloads
        public static void RunTool(string toolExePath, string command, string editingKitPath, string filePath, Loading loadingForm, string outputPath, string type)
        {
            RunToolInternal(toolExePath, command, editingKitPath, filePath, loadingForm, outputPath, type);
        }

        public static void RunTool(string toolExePath, string command, string editingKitPath, string filePath, Loading loadingForm, string outputPath)
        {
            RunToolInternal(toolExePath, command, editingKitPath, filePath, loadingForm, outputPath, null);
        }

        public static void RunTool(string toolExePath, string command, string editingKitPath, string filePath, Loading loadingForm)
        {
            RunToolInternal(toolExePath, command, editingKitPath, filePath, loadingForm, null, null);
        }

        public static void RunTool(string toolExePath, string command, string editingKitPath, string filePath)
        {
            RunToolInternal(toolExePath, command, editingKitPath, filePath, null, null, null);
        }

        public static void RunTool(string toolExePath, string command, string editingKitPath)
        {
            RunToolInternal(toolExePath, command, editingKitPath, null, null, null, null);
        }

        // Main Tool.exe running method
        private static void RunToolInternal(string toolExePath, string command, string editingKitPath, string filePath, Loading loadingForm, string outputPath, string type)
        {
            // Build arguments
            List<string> argumentsList = new List<string>();

            if (command.Contains("extract"))
            {
                if (filePath == null)
                {
                    throw new ArgumentNullException("filePath", "Filepath cannot be null for extract tool command!");
                }

                argumentsList.Add(command);
                argumentsList.Add("\"" + Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "\"");
            }
            else if (command == "export-tag-to-xml")
            {
                if (filePath == null)
                {
                    throw new ArgumentNullException("filePath", "Filepath cannot be null for export xml tool command!");
                }

                argumentsList.Add(command);

                if (type == "shader")
                {
                    argumentsList.Add("\"" + filePath + "\"");
                    argumentsList.Add("\"" + outputPath + "\\" + filePath.Split('\\').Last().Replace(".shader", "") + ".xml" + "\"");
                }
                else if (type == "bitmap")
                {
                    argumentsList.Add("\"" + editingKitPath + "\\tags\\" + filePath + ".bitmap" + "\"");
                    argumentsList.Add("\"" + outputPath.Replace("textures_output", "bitmap_xml") + "\\" + filePath.Split('\\').Last() + ".xml" + "\"");
                }
            }
            else if (command == "export-bitmap-tga")
            {
                if (filePath == null)
                {
                    throw new ArgumentNullException("filePath", "Bitmap filepath cannot be null for bitmap export command!");
                }

                argumentsList.Add(command);
                argumentsList.Add("\"" + filePath + "\"");
                argumentsList.Add("\"" + outputPath + "\\\\" + "\"");
            }
            else if (command == "bitmaps")
            {
                if (filePath == null)
                {
                    throw new ArgumentNullException("filePath", "Folder path cannot be null for bitmaps import command!");
                }

                argumentsList.Add(command);
                argumentsList.Add("\"" + filePath.Split(new[] { "\\data\\" }, StringSplitOptions.None).LastOrDefault() + "\"");
            }
            else if (command == "reimport-bitmaps-single")
            {
                if (filePath == null)
                {
                    throw new ArgumentNullException("filePath", "File path cannot be null for bitmap reimport command!");
                }

                argumentsList.Add(command);
                argumentsList.Add("\"" + filePath + "\"");
            }
            else if (command == "render")
            {
                if (filePath == null)
                {
                    throw new ArgumentNullException("filePath", "Filepath cannot be null for render command!");
                }

                argumentsList.Add(command);
                argumentsList.Add("\"" + filePath + "\"");
                argumentsList.Add("draft");
            }
            else if (command == "collision" || command == "physics")
            {
                if (filePath == null)
                {
                    throw new ArgumentNullException("filePath", "Filepath cannot be null for collision/physics command!");
                }

                argumentsList.Add(command);
                argumentsList.Add("\"" + filePath + "\"");
            }
            else
            {
                throw new ArgumentException("Unrecognised tool command argument!");
            }

            string arguments = string.Join(" ", argumentsList);

            // Set up process
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

            bool isH3 = toolExePath.IndexOf("H3EK", StringComparison.OrdinalIgnoreCase) >= 0;

            // If we are running H3 tool.exe rather than the H2 one, we want to capture the output so it can be shown to the user
            if (isH3 && loadingForm != null)
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.Write(e.Data);
                        loadingForm.UpdateOutputBox(e.Data + Environment.NewLine, true);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine("Error: " + e.Data);
                        loadingForm.UpdateOutputBox("Error: " + e.Data + Environment.NewLine, true);
                    }
                };
            }

            // Start process
            process.Start();

            if (isH3 && loadingForm != null)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            try
            {
                process.WaitForExit();
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException ?? ae;
            }

            process.Close();
        }
    }
}
