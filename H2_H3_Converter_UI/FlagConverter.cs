using Bungie.Tags;
using Bungie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace H2_H3_Converter_UI
{
    class FlagElement
    {
        public string Name { get; set; }
        public float[] Position { get; set; }
        public float[] Facing { get; set; }
    }

    public class FlagConverter
    {
        public static void ConvertCutsceneFlags(string scenPath, string xmlPath, Loading loadingForm)
        {
            // Make sure we have a scenario backup
            Utils.BackupScenario(scenPath, xmlPath, loadingForm);

            loadingForm.UpdateOutputBox("Begin reading scenario cutscene flags from XML...", false);
            string newFilePath = Utils.ConvertXML(xmlPath, loadingForm);
            XmlDocument scenfile = new XmlDocument();
            scenfile.Load(newFilePath);

            loadingForm.UpdateOutputBox("Modified XML file loaded successfully.", false);

            XmlNode root = scenfile.DocumentElement;
            XmlNodeList cutFlagsBlock = root.SelectNodes(".//block[@name='cutscene flags']");
            loadingForm.UpdateOutputBox("Located cutscene flags data block.", false);

            List<FlagElement> allFlags = new List<FlagElement>();
            bool flagDataEnd = false;
            int i = 0;

            while (!flagDataEnd)
            {
                XmlNode flagEntry = cutFlagsBlock[0].SelectSingleNode("./element[@index='" + i + "']");
                if (flagEntry != null)
                {
                    loadingForm.UpdateOutputBox($"Reading data for cutscene flag {i}.", false);
                    FlagElement flag = new FlagElement();
                    flag.Name = flagEntry.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                    flag.Position = flagEntry.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                    flag.Facing = flagEntry.SelectSingleNode("./field[@name='facing']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                
                    allFlags.Add(flag);
                    i++;
                }
                else
                {
                    flagDataEnd = true;
                    loadingForm.UpdateOutputBox("Finished processing cutscene flag data.", false);
                }
            }


            // Now for the managedblam stuff
            string h3ek_path = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
            var relativeScenPath = TagPath.FromPathAndType(Path.ChangeExtension(scenPath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
            TagFile scenTag = new TagFile();

            try
            {
                scenTag.Load(relativeScenPath);
                loadingForm.UpdateOutputBox($"Successfully opened \"{relativeScenPath}\"", false);
                ((TagFieldBlock)scenTag.SelectField($"Block:cutscene flags")).RemoveAllElements();

                i = 0;
                foreach (FlagElement flag in allFlags)
                {
                    loadingForm.UpdateOutputBox($"Writing data for cutscene flag {i}", false);
                    ((TagFieldBlock)scenTag.SelectField($"Block:cutscene flags")).AddElement();
                    ((TagFieldElementStringID)scenTag.SelectField($"Block:cutscene flags[{i}]/StringId:name")).Data = flag.Name;
                    ((TagFieldElementArraySingle)scenTag.SelectField($"Block:cutscene flags[{i}]/RealPoint3d:position")).Data = flag.Position;
                    ((TagFieldElementArraySingle)scenTag.SelectField($"Block:cutscene flags[{i}]/RealEulerAngles2d:facing")).Data = flag.Facing;
                    i++;
                }
            }
            catch
            {
                loadingForm.UpdateOutputBox($"Unknown managedblam error! Flag data will not have been written correctly!", false);
                return;
            }
            finally
            {
                scenTag.Save();
                scenTag.Dispose();
                loadingForm.UpdateOutputBox($"Finished writing flag data to scenario tag!", false);
            }
        }
    }
}
