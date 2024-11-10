using Bungie.Tags;
using Bungie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        class StartLoc
        {
            public string Name { get; set; }
            public float[] Position { get; set; }
            public float[] Facing { get; set; }
            public uint Flags { get; set; }
            public int CharIndex { get; set; }
            public int WeapIndex { get; set; }
            public int WeapIndex2 { get; set; }
            public int VehiIndex { get; set; }
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
            public uint Flags { get; set; }
            public int Team { get; set; }
            public int ParentIndex { get; set; }
            public int NormalDiff {  get; set; }
            public int InsaneDiff { get; set; }
            public int Upgrade {  get; set; }
            public int VehiIndex { get; set; }
            public int CharIndex { get; set; }
            public int WeapIndex { get; set; }
            public int WeapIndex2 { get; set; }
            public int Zone {  get; set; }
            public int Grenade { get; set; }
            public string VehiVariant { get; set; }
            public string PlaceScript { get; set; }
            public int EditorFolder { get; set; }
            public List<StartLoc> StartingLocations { get; set; }
        }

        public static void ConvertSquadGroups(string scenPath, Loading loadingForm, XmlDocument scenfile)
        {
            loadingForm.UpdateOutputBox("Begin reading scenario squad groups from XML...", false);

            XmlNode root = scenfile.DocumentElement;
            XmlNodeList squadGroupsBlock = root.SelectNodes(".//block[@name='squad groups']");
            loadingForm.UpdateOutputBox("Located squad group data block.", false);

            List<SquadGroup> allGroups = new List<SquadGroup>();
            bool groupDataEnd = false;
            int i = 0;

            if (squadGroupsBlock.Count > 0)
            {
                while (!groupDataEnd)
                {
                    XmlNode groupEntry = squadGroupsBlock[0].SelectSingleNode("./element[@index='" + i + "']");
                    if (groupEntry != null)
                    {
                        loadingForm.UpdateOutputBox($"Reading data for squad group {i}.", false);
                        SquadGroup sqGroup = new SquadGroup
                        {
                            Name = groupEntry.SelectSingleNode("./field[@name='name']").InnerText.Trim(),
                            ParentIndex = Int32.Parse(groupEntry.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"]?.Value)
                        };

                        allGroups.Add(sqGroup);
                        i++;
                    }
                    else
                    {
                        groupDataEnd = true;
                        loadingForm.UpdateOutputBox("Finished processing squad group data.", false);
                    }
                }
            }
            else
            {
                loadingForm.UpdateOutputBox("No squad group data!", false);
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

        public static void ConvertSquads(string scenPath, Loading loadingForm, XmlDocument scenfile)
        {
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

            Dictionary<string, uint> squadFlagMapping = new Dictionary<string, uint>()
            {
                { "0", 0 },
                { "unused", 1 },
                { "blind", 2 },
                { "deaf", 4 },
                { "braindead", 8 },
                { "initially placed", 16 },
                { "units not enterable by player", 32 }
            };

            Dictionary<string, uint> startlocFlagMapping = new Dictionary<string, uint>()
            {
                { "0", 0 },
                { "infection form explode", 1 },
                { "n/a", 2 },
                { "always place", 4 },
                { "initially hidden", 8 },
                { "vehicle destroyed when no driver", 16 },
                { "vehicle open", 32 }
            };

            XmlNode root = scenfile.DocumentElement;
            XmlNodeList squadsBlock = root.SelectNodes(".//block[@name='squads']");
            loadingForm.UpdateOutputBox("Located squads data block.", false);

            List<Squad> allSquads = new List<Squad>();
            bool squadDataEnd = false;
            int i = 0;

            if (squadsBlock.Count > 0)
            {
                while (!squadDataEnd)
                {
                    XmlNode squadEntry = squadsBlock[0].SelectSingleNode("./element[@index='" + i + "']");
                    if (squadEntry != null)
                    {
                        // Squad stuff
                        Squad squad = new Squad();
                        squad.Name = squadEntry.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                        loadingForm.UpdateOutputBox($"Reading data for squad {i} ({squad.Name}).", false);
                        string[] checkedFlags = squadEntry.SelectSingleNode("./field[@name='flags']").InnerText.Trim().Split('\n');
                        squad.ParentIndex = Int32.Parse(squadEntry.SelectSingleNode("./block_index[@name='short block index' and @type='parent']").Attributes["index"]?.Value);
                        squad.NormalDiff = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='normal diff count']").InnerText.Trim());
                        squad.InsaneDiff = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='insane diff count']").InnerText.Trim());
                        squad.Upgrade = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='major upgrade']").InnerText.Trim().Substring(0, 1));
                        squad.VehiIndex = Int32.Parse(squadEntry.SelectSingleNode("./block_index[@name='short block index' and @type='vehicle type']").Attributes["index"]?.Value);
                        squad.CharIndex = Int32.Parse(squadEntry.SelectSingleNode("./block_index[@name='short block index' and @type='character type']").Attributes["index"]?.Value);
                        squad.WeapIndex = Int32.Parse(squadEntry.SelectSingleNode("./block_index[@name='short block index' and @type='initial weapon']").Attributes["index"]?.Value);
                        squad.WeapIndex2 = Int32.Parse(squadEntry.SelectSingleNode("./block_index[@name='short block index' and @type='initial secondary weapon']").Attributes["index"]?.Value);
                        squad.Zone = Int32.Parse(squadEntry.SelectSingleNode("./block_index[@name='short block index' and @type='initial zone']").Attributes["index"]?.Value);
                        squad.Grenade = Int32.Parse(squadEntry.SelectSingleNode("./field[@name='grenade type']").InnerText.Trim().Substring(0, 1));
                        squad.VehiVariant = squadEntry.SelectSingleNode("./field[@name='vehicle variant']").InnerText.Trim();
                        squad.PlaceScript = squadEntry.SelectSingleNode("./field[@name='Placement script']").InnerText.Trim();

                        // Determine flag value
                        uint flagVal = 0;
                        if (checkedFlags.Count() != 1 && checkedFlags[0] != "0") // Only bother if squad actually has any flags set, e.g. non-zero
                        {
                            foreach (string flagString in checkedFlags.Skip(1)) // First is always the numerical value that can be discarded
                            {
                                try
                                {
                                    flagVal += squadFlagMapping[flagString.Trim('\t', '\r')];
                                }
                                catch (KeyNotFoundException)
                                {
                                    Console.WriteLine("Unknown or missing flag, skipping");
                                }
                            }

                            squad.Flags = flagVal;
                        }
                        else
                        {
                            squad.Flags = 0;
                        }

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
                        if (startLocsBlock != null)
                        {
                            XmlNodeList startLocsElements = startLocsBlock.SelectNodes("./element");
                            foreach (XmlNode locEntry in startLocsElements)
                            {
                                StartLoc startLoc = new StartLoc();
                                startLoc.Name = locEntry.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                                loadingForm.UpdateOutputBox($"Squad {squad.Name} starting position {j} ({startLoc.Name})", false);
                                startLoc.Position = locEntry.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                                startLoc.Facing = locEntry.SelectSingleNode("./field[@name='facing (yaw, pitch)']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                                checkedFlags = locEntry.SelectSingleNode("./field[@name='flags']").InnerText.Trim().Split('\n');
                                startLoc.CharIndex = Int32.Parse(locEntry.SelectSingleNode("./block_index[@name='short block index' and @type='character type']").Attributes["index"]?.Value);
                                startLoc.WeapIndex = Int32.Parse(locEntry.SelectSingleNode("./block_index[@name='short block index' and @type='initial weapon']").Attributes["index"]?.Value);
                                startLoc.WeapIndex2 = Int32.Parse(locEntry.SelectSingleNode("./block_index[@name='short block index' and @type='initial secondary weapon']").Attributes["index"]?.Value);
                                startLoc.VehiIndex = Int32.Parse(locEntry.SelectSingleNode("./block_index[@name='short block index' and @type='vehicle type']").Attributes["index"]?.Value);
                                startLoc.SeatType = Int32.Parse(locEntry.SelectSingleNode("./field[@name='seat type']").InnerText.Trim().Substring(0, 1));
                                startLoc.Grenade = Int32.Parse(locEntry.SelectSingleNode("./field[@name='grenade type']").InnerText.Trim().Substring(0, 1));
                                startLoc.Swarm = Int32.Parse(locEntry.SelectSingleNode("./field[@name='swarm count']").InnerText.Trim());
                                startLoc.ActorVar = locEntry.SelectSingleNode("./field[@name='actor variant name']").InnerText.Trim();
                                startLoc.VehiVar = locEntry.SelectSingleNode("./field[@name='vehicle variant name']").InnerText.Trim();
                                startLoc.MoveDist = float.Parse(locEntry.SelectSingleNode("./field[@name='initial movement distance']").InnerText.Trim());
                                startLoc.MoveMode = Int32.Parse(locEntry.SelectSingleNode("./field[@name='initial movement mode']").InnerText.Trim().Substring(0, 1));
                                startLoc.PlaceScript = locEntry.SelectSingleNode("./field[@name='Placement script']").InnerText.Trim();

                                // Determine flag value
                                flagVal = 0;
                                if (checkedFlags.Count() != 1 && checkedFlags[0] != "0") // Only bother if squad actually has any flags set, e.g. non-zero
                                {
                                    foreach (string flagString in checkedFlags.Skip(1)) // First is always the numerical value that can be discarded
                                    {
                                        try
                                        {
                                            flagVal += startlocFlagMapping[flagString.Trim('\t', '\r')];
                                        }
                                        catch (KeyNotFoundException)
                                        {
                                            Console.WriteLine("Unknown or missing flag, skipping");
                                        }
                                    }

                                    startLoc.Flags = flagVal;
                                }
                                else
                                {
                                    startLoc.Flags = 0;
                                }


                                startLocs.Add(startLoc);
                                j++;
                            }
                        }
                        else
                        {
                            loadingForm.UpdateOutputBox($"Squad {squad.Name} has no starting locations.", false);
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
            }
            else
            {
                loadingForm.UpdateOutputBox("No squad data!", false);
            }
            

            // Now time to write all that data with MB!
            string h3ek_path = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
            var relativeScenPath = TagPath.FromPathAndType(Path.ChangeExtension(scenPath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
            TagFile scenTag = new TagFile();

            try
            {
                scenTag.Load(relativeScenPath);
                loadingForm.UpdateOutputBox($"Successfully opened \"{relativeScenPath}\"", false);

                // First, lets sort out the editor folders
                if (squadFolderNames != null)
                {
                    loadingForm.UpdateOutputBox($"Writing editor folder data", false);
                    ((TagFieldBlock)scenTag.SelectField($"Block:editor folders")).RemoveAllElements();
                    i = 0;
                    foreach (string name in squadFolderNames)
                    {
                        ((TagFieldBlock)scenTag.SelectField($"Block:editor folders")).AddElement();
                        ((TagFieldElementString)scenTag.SelectField($"Block:editor folders[{i}]/LongString:name")).Data = name;
                        ((TagFieldBlockIndex)scenTag.SelectField($"Block:editor folders[{i}]/LongBlockIndex:parent folder")).Value = -1;
                        i++;
                    }
                }


                // Now for the actual squad data
                loadingForm.UpdateOutputBox($"Begin writing squad data", false);
                ((TagFieldBlock)scenTag.SelectField($"Block:squads")).RemoveAllElements();
                i = 0;
                foreach (Squad squad in allSquads)
                {
                    // Squad-level data
                    ((TagFieldBlock)scenTag.SelectField($"Block:squads")).AddElement();
                    ((TagFieldElementString)scenTag.SelectField($"Block:squads[{i}]/String:name")).Data = squad.Name;
                    ((TagFieldFlags)scenTag.SelectField($"Block:squads[{i}]/Flags:flags")).RawValue = squad.Flags;
                    ((TagFieldEnum)scenTag.SelectField($"Block:squads[{i}]/ShortEnum:team")).Value = squad.Team;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/ShortBlockIndex:parent")).Value = squad.ParentIndex;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/ShortBlockIndex:initial zone")).Value = squad.Zone;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/ShortBlockIndex:editor folder")).Value = squad.EditorFolder;

                    // Fireteam-level data
                    ((TagFieldBlock)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams")).AddElement();
                    ((TagFieldElementInteger)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/ShortInteger:normal diff count")).Data = squad.NormalDiff;
                    ((TagFieldEnum)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/ShortEnum:major upgrade")).Value = squad.Upgrade;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/ShortBlockIndex:character type")).Value = squad.CharIndex;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/ShortBlockIndex:initial weapon")).Value = squad.WeapIndex;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/ShortBlockIndex:initial secondary weapon")).Value = squad.WeapIndex2;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/ShortBlockIndex:vehicle type")).Value = squad.VehiIndex;
                    ((TagFieldEnum)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/ShortEnum:grenade type")).Value = squad.Grenade;
                    ((TagFieldElementStringID)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/StringId:vehicle variant")).Data = squad.VehiVariant;
                    ((TagFieldElementString)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/String:Placement script")).Data = squad.PlaceScript;

                    // Starting positions
                    int j = 0;
                    foreach (StartLoc startingLoc in squad.StartingLocations)
                    {
                        ((TagFieldBlock)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations")).AddElement();
                        ((TagFieldElementOldStringID)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/OldStringId:name")).Data = startingLoc.Name;
                        ((TagFieldElementArraySingle)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/RealPoint3d:position")).Data = startingLoc.Position;
                        ((TagFieldElementArraySingle)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/RealEulerAngles2d:facing (yaw, pitch)")).Data = startingLoc.Facing;
                        ((TagFieldFlags)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/Flags:flags")).RawValue = startingLoc.Flags;
                        ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/ShortBlockIndex:character type")).Value = startingLoc.CharIndex;
                        ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/ShortBlockIndex:initial weapon")).Value = startingLoc.WeapIndex;
                        ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/ShortBlockIndex:initial secondary weapon")).Value = startingLoc.WeapIndex2;
                        ((TagFieldBlockIndex)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/ShortBlockIndex:vehicle type")).Value = startingLoc.VehiIndex;
                        ((TagFieldEnum)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/ShortEnum:seat type")).Value = startingLoc.SeatType;
                        ((TagFieldEnum)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/ShortEnum:grenade type")).Value = startingLoc.Grenade;
                        ((TagFieldElementInteger)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/ShortInteger:swarm count")).Data = startingLoc.Swarm;
                        ((TagFieldElementStringID)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/StringId:actor variant name")).Data = startingLoc.ActorVar;
                        ((TagFieldElementStringID)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/StringId:vehicle variant name")).Data = startingLoc.VehiVar;
                        ((TagFieldElementSingle)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/Real:initial movement distance")).Data = startingLoc.MoveDist;
                        ((TagFieldEnum)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/ShortEnum:initial movement mode")).Value = startingLoc.MoveMode;
                        ((TagFieldElementString)scenTag.SelectField($"Block:squads[{i}]/Block:fire-teams[0]/Block:starting locations[{j}]/String:Placement script")).Data = startingLoc.PlaceScript;

                        j++;
                    }

                    i++;
                }
            }
            catch
            {
                loadingForm.UpdateOutputBox($"Unknown managedblam error! Squad data will not have been written correctly!", false);
                return;
            }
            finally
            {
                scenTag.Save();
                scenTag.Dispose();
                loadingForm.UpdateOutputBox($"Finished writing squad data to scenario tag!", false);
            }
        }
    }
}
