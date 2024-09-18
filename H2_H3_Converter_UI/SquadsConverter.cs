using Bungie.Tags;
using Bungie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using static System.Windows.Forms.LinkLabel;
using System.Reflection;

namespace H2_H3_Converter_UI
{
    public class SquadsConverter
    {
        class SquadGroup
        {
            public string Name { get; set; }
            public int ParentIndex { get; set; }
        }

        class StartLoc
        {
            public string Name { get; set; }
            public float[] Position { get; set; }
            public float[] Facing { get; set; }
            public int Flags { get; set; }
            public int SeatType { get; set; }
            public int Grenade {  get; set; }
            public int Swarm { get; set; }
            public string ActorVar { get; set; }
            public string VehiVar { get; set; }
            public float MoveDist { get; set; }
            public int MoveMode { get; set; }
            public string PlaceScript { get; set; }
        }

        class Squad
        {
            public string Name { get; set; }
            public int Flags { get; set; }
            public int Team { get; set; }
            public int ParentIndex { get; set; }
            public int NormalDiff {  get; set; }
            public int InsaneDiff { get; set; }
            public int Upgrade {  get; set; }
            public int Zone {  get; set; }
            public int Grenade { get; set; }
            public string VehiVariant { get; set; }
            public string PlaceScript { get; set; }
            public int EditorFolder { get; set; }
            public List<StartLoc> StartingLocations { get; set; }
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
    
        public static void ConvertSquads(string scenPath, string xmlPath, Loading loadingForm, XmlDocument scenfile)
        {
            // Make sure we have a scenario backup
            Utils.BackupScenario(scenPath, xmlPath, loadingForm);
            loadingForm.UpdateOutputBox("Begin reading scenario squads from XML...", false);

            // Grab user-specified folder names for squads
            string[] squadFolderNames = null;
            try
            {
                squadFolderNames = File.ReadAllLines("squad_folders_CHANGME.txt");
                loadingForm.UpdateOutputBox("Using user-specified squad folders.", false);
            }
            catch (FileNotFoundException)
            {
                loadingForm.UpdateOutputBox("Squad folder names txt not found, no squad sub-folders will be used.", false);
            }
            catch (Exception e)
            {
                loadingForm.UpdateOutputBox($"Error when reading squad folder name txt - {e}", false);
            }
            

            XmlNode root = scenfile.DocumentElement;
            XmlNodeList squadsBlock = root.SelectNodes(".//block[@name='squads']");
            loadingForm.UpdateOutputBox("Located squads data block.", false);

            List<Squad> allSquads = new List<Squad>();
            bool squadDataEnd = false;
            int i = 0;

            while (!squadDataEnd)
            {
                XmlNode squadEntry = squadsBlock[0].SelectSingleNode("./element[@index='" + i + "']");
                if (squadEntry != null)
                {
                    // Squad stuff
                    Squad squad = new Squad();
                    squad.Name = squadEntry.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                    loadingForm.UpdateOutputBox($"Reading data for squad {i} ({squad.Name}).", false);
                    squad.Flags = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='flags']").InnerText.Trim().Substring(0, 1));
                    squad.ParentIndex = Int32.Parse(squadEntry.SelectSingleNode("./block_index[@name='short block index' and @type='parent']").Attributes["index"]?.Value);
                    squad.NormalDiff = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='normal diff count']").InnerText.Trim());
                    squad.InsaneDiff = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='insane diff count']").InnerText.Trim());
                    squad.Upgrade = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='major upgrade']").InnerText.Trim().Substring(0, 1));
                    squad.Zone = Int32.Parse(squadEntry.SelectSingleNode("./block_index[@name='short block index' and @type='initial zone']").Attributes["index"]?.Value);
                    squad.Grenade = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='grenade type']").InnerText.Trim().Substring(0, 1));
                    squad.VehiVariant = squadEntry.SelectSingleNode("./field[@name='vehicle variant']").InnerText.Trim();
                    squad.PlaceScript = squadEntry.SelectSingleNode("./field[@name='Placement script']").InnerText.Trim();
                    
                    // Determine editor folder based on user-provided txt of folder names, matching against squad names
                    if (squadFolderNames != null)
                    {
                        bool foundFolder = false;
                        for (int x = 0; x < squadFolderNames.Length; x++)
                        {
                            string line = squadFolderNames[x];
                            if (squad.Name.ToLower().Contains(line.ToLower()))
                            {
                                loadingForm.UpdateOutputBox($"Squad {squad.Name} using editor folder index {x} ({line}).", false);
                                squad.EditorFolder = x;
                                foundFolder = true;
                                break;
                            }
                        }

                        if (!foundFolder)
                        {
                            loadingForm.UpdateOutputBox($"Squad {squad.Name} has no corresponding editor folder", false);
                            squad.EditorFolder = -1;
                        }
                    }
                    else
                    {
                        squad.EditorFolder = -1;
                    }
                    
                    // Squad's starting locations stuff
                    List<StartLoc> startLocs = new List<StartLoc>();
                    XmlNode startLocsBlock = root.SelectSingleNode($".//block[@name='squads']/element[@index='{i}']/block[@name='starting locations']");

                    int j = 0;
                    XmlNodeList startLocsElements = startLocsBlock.SelectNodes("./element");
                    foreach (XmlNode locEntry in startLocsElements)
                    {
                        StartLoc startLoc = new StartLoc();
                        startLoc.Name = locEntry.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                        loadingForm.UpdateOutputBox($"Squad {squad.Name} starting position {j} ({startLoc.Name})", false);
                        startLoc.Position = locEntry.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                        startLoc.Facing = locEntry.SelectSingleNode("./field[@name='facing (yaw, pitch)']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                        startLoc.Flags = Int32.Parse(locEntry.SelectSingleNode("./field[@name='flags']").InnerText.Trim().Substring(0, 1));
                        startLoc.SeatType = Int32.Parse(locEntry.SelectSingleNode("./field[@name='seat type']").InnerText.Trim().Substring(0, 1));
                        startLoc.Grenade = Int32.Parse(locEntry.SelectSingleNode("./field[@name='grenade type']").InnerText.Trim().Substring(0, 1));
                        startLoc.Swarm = Int32.Parse(locEntry.SelectSingleNode("./field[@name='swarm count']").InnerText.Trim());
                        startLoc.ActorVar = locEntry.SelectSingleNode("./field[@name='actor variant name']").InnerText.Trim();
                        startLoc.VehiVar = locEntry.SelectSingleNode("./field[@name='vehicle variant name']").InnerText.Trim();
                        startLoc.MoveDist = float.Parse(locEntry.SelectSingleNode("./field[@name='initial movement distance']").InnerText.Trim());
                        startLoc.MoveMode = Int32.Parse(locEntry.SelectSingleNode("./field[@name='initial movement mode']").InnerText.Trim().Substring(0, 1));
                        startLoc.PlaceScript = locEntry.SelectSingleNode("./field[@name='Placement script']").InnerText.Trim();

                        startLocs.Add(startLoc);
                        j++;
                    }

                    squad.StartingLocations = startLocs;
                    allSquads.Add(squad);
                    i++;
                }
                else
                {
                    squadDataEnd = true;
                    loadingForm.UpdateOutputBox("Finished processing scenario squad data.", false);
                }
            }

            // Now time to write all that data with MB!

        }
    }
}
