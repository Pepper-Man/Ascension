using Bungie.Tags;
using Bungie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace H2_H3_Converter_UI
{
    public class SquadsConverter
    {
        class SquadGroup
        {
            public string Name { get; set; }
            public int ParentIndex { get; set; }
        }

        public static void ConvertSquadGroups(string scenPath, string xmlPath, Loading loadingForm, XmlDocument scenfile)
        {
            // Make sure we have a scenario backup
            Utils.BackupScenario(scenPath, xmlPath, loadingForm);
            loadingForm.UpdateOutputBox("Begin reading scenario squad groups from XML...", false);

            XmlNode root = scenfile.DocumentElement;
            XmlNodeList squadGroupsBlock = root.SelectNodes(".//block[@name='squad groups']");
            loadingForm.UpdateOutputBox("Located squad group data block.", false);

            List<SquadGroup> allGroups = new List<SquadGroup>();
            bool groupDataEnd = false;
            int i = 0;

            while (!groupDataEnd)
            {
                XmlNode groupEntry = squadGroupsBlock[0].SelectSingleNode("./element[@index='" + i + "']");
                if (groupEntry != null)
                {
                    loadingForm.UpdateOutputBox($"Reading data for squad group {i}.", false);
                    SquadGroup sqGroup = new SquadGroup();
                    sqGroup.Name = groupEntry.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                    sqGroup.ParentIndex = Int32.Parse(groupEntry.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"]?.Value);

                    allGroups.Add(sqGroup);
                    i++;
                }
                else
                {
                    groupDataEnd = true;
                    loadingForm.UpdateOutputBox("Finished processing squad group data.", false);
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
                ((TagFieldBlock)scenTag.SelectField($"Block:squad groups")).RemoveAllElements();

                i = 0;
                foreach (SquadGroup group in allGroups)
                {
                    loadingForm.UpdateOutputBox($"Writing data for squad group \"{group.Name}\"", false);
                    ((TagFieldBlock)scenTag.SelectField($"Block:squad groups")).AddElement();
                    ((TagFieldElementString)scenTag.SelectField($"Block:squad groups[{i}]/String:name")).Data = group.Name;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:squad groups[{i}]/ShortBlockIndex:parent")).Value = group.ParentIndex;
                    i++;
                }
            }
            catch
            {
                loadingForm.UpdateOutputBox($"Unknown managedblam error! Squad group data will not have been written correctly!", false);
                return;
            }
            finally
            {
                scenTag.Save();
                scenTag.Dispose();
                loadingForm.UpdateOutputBox($"Finished writing squad group data to scenario tag!", false);
            }
        }
    }
}
