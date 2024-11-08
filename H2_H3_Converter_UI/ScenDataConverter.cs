using Bungie;
using Bungie.Tags;
using System;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using H2_H3_Converter_UI;

class StartLoc
{
    public float[] Position { get; set; }
    public float Facing { get; set; }
    public int Team { get; set; }
    public int PlayerType { get; set; }
}

public class ObjectPlacement
{
    public int TypeIndex { get; set; }
    public int NameIndex { get; set; }
    public uint Flags { get; set; }
    public float[] Position { get; set; }
    public float[] Rotation { get; set; }
    public float Scale { get; set; }
    public string VarName { get; set; }
    public uint ManualBsp {  get; set; }
    public int OriginBsp { get; set; }
    public int BspPolicy { get; set; }
}

class NetEquip : ObjectPlacement
{
    public long SpawnTime { get; set; }
    public string CollectionType { get; set; }
}

class SpWeapLoc : ObjectPlacement
{
    public int RoundsLeft { get; set; }
    public int RoundsLoaded { get; set; }
}

class Scenery : ObjectPlacement
{
    public int PathfindingType { get; set; }
    public int LightmappingType { get; set; }
}

class TrigVol
{
    public string Name { get; set; }
    public float[] Position { get; set; }
    public float[] Extents { get; set; }
    public float[] Forward { get; set; }
    public float[] Up { get; set; }
}

class Vehicle : ObjectPlacement
{
    public float BodyVitality { get; set; }
}

class Device : ObjectPlacement
{
    public int PowerGroupIndex { get; set; }
    public int PositionGroupIndex { get; set; }
    public uint DeviceFlags1 { get; set; }
    public uint DeviceFlags2 { get; set; }
}

class Crate : ObjectPlacement {}

class NetFlag
{
    public string Name { get; set; }
    public float[] Position { get; set; }
    public float Facing { get; set; }
    public string Type { get; set; }
    public int Team { get; set; }
}

class Decal
{
    public string Type { get; set; }
    public string Yaw { get; set; }
    public string Pitch { get; set; }
    public float[] Position { get; set; }
}

class DeviceGroup
{
    public string Name { get; set; }
    public float InitVal { get; set; }
    public uint Flags { get; set; }
}

class ScenData
{
    public static void ConvertScenarioData(string scenPath, string xmlPath, Loading loadingForm)
    {
        string h3ekPath = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);

        // Make sure we have a scenario backup
        Utils.BackupScenario(scenPath, loadingForm);

        // Initialise MB
        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ekPath);

        xmlPath = Utils.ConvertXML(xmlPath, loadingForm);
        XmlDocument scenfile = new XmlDocument();
        scenfile.Load(xmlPath);

        XmlNode root = scenfile.DocumentElement;

        string scenarioType = root.SelectNodes(".//field[@name='type']")[0].InnerText.Trim();
        XmlNodeList playerStartLocBlock = root.SelectNodes(".//block[@name='player starting locations']");
        XmlNodeList netgameObjEntriesBlock = root.SelectNodes(".//block[@name='netgame equipment']");
        XmlNodeList weaponSpEntriesBlock = root.SelectNodes(".//block[@name='weapons']");
        XmlNodeList scenPaletteBlock = root.SelectNodes(".//block[@name='scenery palette']");
        XmlNodeList scenEntriesBlock = root.SelectNodes(".//block[@name='scenery']");
        XmlNodeList trigVolBlock = root.SelectNodes(".//block[@name='trigger volumes']");
        XmlNodeList vehiPaletteBlock = root.SelectNodes(".//block[@name='vehicle palette']");
        XmlNodeList vehiEntriesBlock = root.SelectNodes(".//block[@name='vehicles']");
        XmlNodeList cratePaletteBlock = root.SelectNodes(".//block[@name='crate palette']");
        XmlNodeList crateEntriesBlock = root.SelectNodes(".//block[@name='crates']");
        XmlNodeList objectNamesBlock = root.SelectNodes(".//block[@name='object names']");
        XmlNodeList netgameFlagsBlock = root.SelectNodes(".//block[@name='netgame flags']");
        XmlNodeList decalPaletteBlock = root.SelectNodes(".//block[@name='decal palette']");
        XmlNodeList decalEntriesBlock = root.SelectNodes(".//block[@name='decals']");
        XmlNodeList machEntriesBlock = root.SelectNodes(".//block[@name='machines']");
        XmlNodeList deviceGroupsBlock = root.SelectNodes(".//block[@name='device groups']");
        XmlNodeList ctrlEntriesBlock = root.SelectNodes(".//block[@name='controls']");

        List<StartLoc> allStartingLocs = new List<StartLoc>();
        List<NetEquip> allNetgameEquipLocs = new List<NetEquip>();
        List<SpWeapLoc> allSpWeaponLocs = new List<SpWeapLoc>();
        List<TagPath> allScenTypes = new List<TagPath>();
        List<Scenery> allScenEntries = new List<Scenery>();
        List<TrigVol> allTrigVols = new List<TrigVol>();
        List<TagPath> allVehiTypes = new List<TagPath>();
        List<Vehicle> allVehiEntries = new List<Vehicle>();
        List<TagPath> allCrateTypes = new List<TagPath>();
        List<Crate> allCrateEntries = new List<Crate>();
        List<string> allObjectNames = new List<string>();
        List<NetFlag> allNetgameFlags = new List<NetFlag>();
        List<Decal> allDecalEntries = new List<Decal>();
        List<TagPath> allDecalTypes = new List<TagPath>();
        List<Device> allMachineEntries = new List<Device>();
        List<DeviceGroup> allDeviceGroups = new List<DeviceGroup>();
        List<Device> allControlEntries = new List<Device>();

        foreach (XmlNode name in objectNamesBlock)
        {
            bool objNamesEnd = false;
            int i = 0;
            while (!objNamesEnd)
            {
                XmlNode element = name.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    allObjectNames.Add(element.SelectSingleNode("./field[@name='name']").InnerText.Trim());
                    i++;
                }
                else
                {
                    objNamesEnd = true;
                    Console.WriteLine("Finished processing object name data.");
                    loadingForm.UpdateOutputBox("Finished processing object name data.", false);
                }
            }
        }

        foreach (XmlNode name in netgameFlagsBlock)
        {
            bool netFlagsEnd = false;
            int i = 0;
            while (!netFlagsEnd)
            {
                XmlNode element = name.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    allObjectNames.Add(element.Attributes["name"].Value);
                    i++;
                }
                else
                {
                    netFlagsEnd = true;
                    Console.WriteLine("Finished processing netgame flag name data.");
                    loadingForm.UpdateOutputBox("Finished processing netgame flag name data.", false);
                }
            }
        }

        foreach (XmlNode location in playerStartLocBlock)
        {
            bool startLocsEnd = false;
            int i = 0;
            while (!startLocsEnd)
            {
                XmlNode element = location.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    StartLoc startLocation = new StartLoc
                    {
                        Position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                        Facing = float.Parse(element.SelectSingleNode("./field[@name='facing']").InnerText.Trim()),
                        Team = Int32.Parse(element.SelectSingleNode("./field[@name='team designator']").InnerText.Trim().Substring(0, 1)),
                        PlayerType = Int32.Parse(element.SelectSingleNode("./field[@name='campaign player type']").InnerText.Trim().Substring(0, 1))
                    };

                    allStartingLocs.Add(startLocation);
                    Console.WriteLine("Processed starting position " + i);
                    loadingForm.UpdateOutputBox("Processed starting position " + i, false);
                    i++;
                }
                else
                {
                    startLocsEnd = true;
                    Console.WriteLine("\nFinished processing starting positions data.");
                    loadingForm.UpdateOutputBox("\nFinished processing starting positions data.", false);
                }
            }
        }

        // Have to handle weapons/vehicles/equipment differently for solo vs mult
        if (scenarioType == "1,multiplayer")
        {
            foreach (XmlNode netgameObjEntry in netgameObjEntriesBlock)
            {
                bool netgameObjsEnd = false;
                int i = 0;
                while (!netgameObjsEnd)
                {
                    XmlNode element = netgameObjEntry.SelectSingleNode("./element[@index='" + i + "']");
                    if (element != null)
                    {
                        NetEquip netgameEquip = new NetEquip
                        {
                            Position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                            Rotation = element.SelectSingleNode("./field[@name='orientation']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                            SpawnTime = long.Parse(element.SelectSingleNode("./field[@name='spawn time (in seconds, 0 = default)']").InnerText.Trim()),
                            CollectionType = element.SelectSingleNode("./tag_reference[@name='item/vehicle collection']").InnerText.Trim()
                        };

                        allNetgameEquipLocs.Add(netgameEquip);
                        Console.WriteLine("Process netgame equipment " + i);
                        loadingForm.UpdateOutputBox("Process netgame equipment " + i, false);
                        i++;
                    }
                    else
                    {
                        netgameObjsEnd = true;
                        Console.WriteLine("\nFinished processing netgame equipment data.");
                        loadingForm.UpdateOutputBox("\nFinished processing netgame equipment data.", false);
                    }
                }
            }
        }
        else if (scenarioType == "0,solo")
        {
            // Before we can do anything, gotta transfer the weapon palette data so the indices line up
            Utils.ConvertPalette(scenPath, loadingForm, scenfile, "weapon");
            loadingForm.UpdateOutputBox("\nBegin reading weapon placement data.", false);
            foreach (XmlNode weaponEntry in weaponSpEntriesBlock)
            {
                bool weaponsEnd = false;
                int i = 0;
                while (!weaponsEnd)
                {
                    XmlNode element = weaponEntry.SelectSingleNode("./element[@index='" + i + "']");

                    if (element != null)
                    {
                        SpWeapLoc weapon = Utils.GetObjectDataFromXML<SpWeapLoc>(element);
                        allSpWeaponLocs.Add(weapon);
                        loadingForm.UpdateOutputBox($"Processed weapon placement {i}.", false);
                        i++;
                    }
                    else
                    {
                        weaponsEnd = true;
                        Console.WriteLine("\nFinished processing weapon data.");
                        loadingForm.UpdateOutputBox("\nFinished processing weapon placement data.", false);
                    }
                }
            }

            // SP vehicles - MP vehicles from H2 don't actually use the vehicle palette

            // Transfer the vehicle palette data so the indices line up
            Utils.ConvertPalette(scenPath, loadingForm, scenfile, "vehicle");
            loadingForm.UpdateOutputBox("\nBegin reading vehicle placement data.", false);
            foreach (XmlNode vehicleEntry in vehiEntriesBlock)
            {
                bool vehiclesEnd = false;
                int i = 0;
                while (!vehiclesEnd)
                {
                    XmlNode element = vehicleEntry.SelectSingleNode("./element[@index='" + i + "']");
                    if (element != null)
                    {
                        Vehicle vehicle = Utils.GetObjectDataFromXML<Vehicle>(element);
                        allVehiEntries.Add(vehicle);
                        i++;
                    }
                    else
                    {
                        vehiclesEnd = true;
                        Console.WriteLine("Finished processing vehicle placement data.");
                        loadingForm.UpdateOutputBox("Finished processing vehicle placement data.", false);
                    }
                }
            }
        }

        foreach (XmlNode sceneryType in scenPaletteBlock)
        {
            bool scenTypesEnd = false;
            int i = 0;
            while (!scenTypesEnd)
            {
                XmlNode element = sceneryType.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    string scenRef = element.SelectSingleNode("./tag_reference[@name='name']").InnerText.Trim();
                    allScenTypes.Add(TagPath.FromPathAndType(scenRef, "scen*"));
                    i++;
                }
                else
                {
                    scenTypesEnd = true;
                    Console.WriteLine("Finished processing scenery palette data.");
                    loadingForm.UpdateOutputBox("Finished processing scenery palette data.", false);
                }
            }
        }

        foreach (XmlNode sceneryEntry in scenEntriesBlock)
        {
            bool sceneriesEnd = false;
            int i = 0;
            while (!sceneriesEnd)
            {
                XmlNode element = sceneryEntry.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    Scenery scenery = Utils.GetObjectDataFromXML<Scenery>(element);
                    allScenEntries.Add(scenery);
                    i++;
                }
                else
                {
                    sceneriesEnd = true;
                    Console.WriteLine("Finished processing scenery placement data.");
                    loadingForm.UpdateOutputBox("Finished processing scenery placement data.", false);
                }
            }
        }

        foreach (XmlNode trigVolume in trigVolBlock)
        {
            bool trigVolsEnd = false;
            int i = 0;
            while (!trigVolsEnd)
            {
                XmlNode element = trigVolume.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    TrigVol triggerVolume = new TrigVol
                    {
                        Name = element.SelectSingleNode("./field[@name='name']").InnerText.Trim(),
                        Position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                        Extents = element.SelectSingleNode("./field[@name='extents']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                        Forward = element.SelectSingleNode("./field[@name='forward']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                        Up = element.SelectSingleNode("./field[@name='up']").InnerText.Trim().Split(',').Select(float.Parse).ToArray()
                    };

                    allTrigVols.Add(triggerVolume);
                    i++;
                }
                else
                {
                    trigVolsEnd = true;
                    Console.WriteLine("Finished processing trigger volume data.");
                    loadingForm.UpdateOutputBox("Finished processing trigger volume data.", false);
                }
            }
        }

        foreach (XmlNode crateType in cratePaletteBlock)
        {
            bool crateTypesEnd = false;
            int i = 0;
            while (!crateTypesEnd)
            {
                XmlNode element = crateType.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    string crateRef = element.SelectSingleNode("./tag_reference[@name='name']").InnerText.Trim();
                    allCrateTypes.Add(TagPath.FromPathAndType(crateRef, "bloc*"));
                    i++;
                }
                else
                {
                    crateTypesEnd = true;
                    Console.WriteLine("Finished processing crate palette data.");
                    loadingForm.UpdateOutputBox("Finished processing crate palette data.", false);
                }
            }
        }

        foreach (XmlNode crateEntry in crateEntriesBlock)
        {
            bool cratesEnd = false;
            int i = 0;
            while (!cratesEnd)
            {
                XmlNode element = crateEntry.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    Crate crate = Utils.GetObjectDataFromXML<Crate>(element);
                    allCrateEntries.Add(crate);
                    i++;
                }
                else
                {
                    cratesEnd = true;
                    Console.WriteLine("Finished processing crate placement data.");
                    loadingForm.UpdateOutputBox("Finished processing crate placement data.", false);
                }
            }
        }

        foreach (XmlNode netFlagEntry in netgameFlagsBlock)
        {
            bool netFlagsEnd = false;
            int i = 0;
            while (!netFlagsEnd)
            {
                XmlNode element = netFlagEntry.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    NetFlag netFlag = new NetFlag
                    {
                        Name = element.Attributes["name"].Value,
                        Position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                        Facing = float.Parse(element.SelectSingleNode("./field[@name='facing']").InnerText.Trim()),
                        Type = element.SelectSingleNode("./field[@name='type']").InnerText.Trim(),
                        Team = Int32.Parse(element.SelectSingleNode("./field[@name='team designator']").InnerText.Trim().Substring(0, 1))
                    };

                    allNetgameFlags.Add(netFlag);
                    i++;
                }
                else
                {
                    netFlagsEnd = true;
                    Console.WriteLine("Finished processing netgame flags data.");
                    loadingForm.UpdateOutputBox("Finished processing netgame flags data.", false);
                }
            }
        }

        foreach (XmlNode decalType in decalPaletteBlock)
        {
            bool decalTypesEnd = false;
            int i = 0;
            while (!decalTypesEnd)
            {
                XmlNode element = decalType.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    string decRef = element.SelectSingleNode("./tag_reference[@name='reference']").InnerText.Trim();
                    allDecalTypes.Add(TagPath.FromPathAndType(decRef, "decs*"));
                    i++;
                }
                else
                {
                    decalTypesEnd = true;
                    Console.WriteLine("Finished processing decal palette data.");
                    loadingForm.UpdateOutputBox("Finished processing decal palette data.", false);
                }
            }
        }

        foreach (XmlNode decalEntry in decalEntriesBlock)
        {
            bool decalsEnd = false;
            int i = 0;
            while (!decalsEnd)
            {
                XmlNode element = decalEntry.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    Decal decal = new Decal
                    {
                        Type = element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.ToString(),
                        Yaw = element.SelectSingleNode("./field[@name='yaw[-127,127]']").InnerText.Trim(),
                        Pitch = element.SelectSingleNode("./field[@name='pitch[-127,127]']").InnerText.Trim(),
                        Position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray()
                    };

                    allDecalEntries.Add(decal);
                    i++;
                }
                else
                {
                    decalsEnd = true;
                    Console.WriteLine("Finished processing decal placement data.");
                    loadingForm.UpdateOutputBox("Finished processing decal placement data.", false);
                }
            }
        }

        // Device machines
        Utils.ConvertPalette(scenPath, loadingForm, scenfile, "machine");
        loadingForm.UpdateOutputBox("\nBegin reading device machine placement data.", false);
        foreach (XmlNode machineEntry in machEntriesBlock)
        {
            bool machinesEnd = false;
            int i = 0;
            while (!machinesEnd)
            {
                XmlNode element = machineEntry.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    Device machine = Utils.GetObjectDataFromXML<Device>(element);
                    allMachineEntries.Add(machine);
                    i++;
                }
                else
                {
                    machinesEnd = true;
                    Console.WriteLine("Finished processing device machine placement data.");
                    loadingForm.UpdateOutputBox("Finished processing device machine placement data.", false);
                }
            }
        }

        // Device controls
        Utils.ConvertPalette(scenPath, loadingForm, scenfile, "control");
        loadingForm.UpdateOutputBox("\nBegin reading device control placement data.", false);
        foreach (XmlNode controlEntry in ctrlEntriesBlock)
        {
            bool controlsEnd = false;
            int i = 0;
            while (!controlsEnd)
            {
                XmlNode element = controlEntry.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    Device control = Utils.GetObjectDataFromXML<Device>(element);
                    allControlEntries.Add(control);
                    i++;
                }
                else
                {
                    controlsEnd = true;
                    Console.WriteLine("Finished processing device control placement data.");
                    loadingForm.UpdateOutputBox("Finished processing device control placement data.", false);
                }
            }
        }

        foreach (XmlNode devGroupEntry in deviceGroupsBlock)
        {
            bool devGroupsEnd = false;
            int i = 0;
            while (!devGroupsEnd)
            {
                XmlNode element = devGroupEntry.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    DeviceGroup deviceGroup = new DeviceGroup
                    {
                        Name = element.SelectSingleNode("./field[@name='name']").InnerText.Trim(),
                        InitVal = float.Parse(element.SelectSingleNode("./field[@name='initial value']").InnerText.Trim()),
                        Flags = uint.Parse(element.SelectSingleNode("./field[@name='flags']").InnerText.Trim().Substring(0, 1))
                    };

                    allDeviceGroups.Add(deviceGroup);
                    i++;
                }
                else
                {
                    devGroupsEnd = true;
                    Console.WriteLine("Finished processing device group data.");
                    loadingForm.UpdateOutputBox("Finished processing device group data.", false);
                }
            }
        }

        XmlToTag(allObjectNames, allStartingLocs, allNetgameEquipLocs, allSpWeaponLocs, allScenTypes, allScenEntries, allTrigVols, allVehiEntries, allCrateTypes, allCrateEntries, allNetgameFlags, allDecalTypes, allDecalEntries, allMachineEntries, allControlEntries, allDeviceGroups, h3ekPath, scenPath, loadingForm, scenarioType);
    }

    static void XmlToTag(List<string> allObjectNames, List<StartLoc> startLocations, List<NetEquip> netgameEquipment, List<SpWeapLoc> allSpWeapLocs, List<TagPath> allScenTypes, List<Scenery> allScenEntries, List<TrigVol> allTrigVols, List<Vehicle> allVehiEntries, List<TagPath> allCrateTypes, List<Crate> allCrateEntries, List<NetFlag> allNetgameFlags, List<TagPath> allDecalTypes, List<Decal> allDecalEntries, List<Device> allMachineEntries, List<Device> allControlEntries, List<DeviceGroup> allDeviceGroups, string h3ekPath, string scenpath, Loading loadingForm, string scenarioType)
    {
        Utils utilsInstance = new Utils();
        var tagPath = TagPath.FromPathAndType(Path.ChangeExtension(scenpath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
        var respawnScenPath = TagPath.FromPathAndType(@"objects\multi\spawning\respawn_point", "scen*");
        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ekPath);

        using (var tagFile = new TagFile(tagPath))
        {
            void AddToPaletteIfNotExists(string blockName, string fieldName, string itemType, Dictionary<string, int> paletteMapping, TagPath path)
            {
                // Check if the itemType has already been added using the paletteMapping dictionary
                if (!paletteMapping.ContainsKey(itemType))
                {
                    loadingForm.UpdateOutputBox($"Adding {path} to {blockName}", false);

                    int currentCount = ((TagFieldBlock)tagFile.SelectField($"Block:{blockName}")).Elements.Count();
                    ((TagFieldBlock)tagFile.SelectField($"Block:{blockName}")).AddElement();
                    ((TagFieldReference)tagFile.SelectField($"Block:{blockName}[{currentCount}]/Reference:{fieldName}")).Path = path;

                    paletteMapping.Add(itemType, currentCount);
                }
            }

            void AddElementToBlock(string blockName, int index, NetEquip netgameEquipEntry, int typeIndex, int sourceType, int objectType)
            {
                // Position
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:{blockName}[{index}]/Struct:object data/RealPoint3d:position")).Data = netgameEquipEntry.Position;

                // Rotation
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:{blockName}[{index}]/Struct:object data/RealEulerAngles3d:rotation")).Data = netgameEquipEntry.Rotation;

                // Type
                ((TagFieldBlockIndex)tagFile.SelectField($"Block:{blockName}[{index}]/ShortBlockIndex:type")).Value = typeIndex;

                // Spawn timer
                ((TagFieldElementInteger)tagFile.SelectField($"Block:{blockName}[{index}]/Struct:multiplayer data/ShortInteger:spawn time")).Data = netgameEquipEntry.SpawnTime;

                // Dropdown type and source
                ((TagFieldEnum)tagFile.SelectField($"Block:{blockName}[{index}]/Struct:object data/Struct:object id/CharEnum:type")).Value = objectType;
                ((TagFieldEnum)tagFile.SelectField($"Block:{blockName}[{index}]/Struct:object data/Struct:object id/CharEnum:source")).Value = sourceType;
            }

            // Object names section
            ((TagFieldBlock)tagFile.SelectField($"Block:object names")).RemoveAllElements();
            int nameIndex = 0;
            foreach (string name in allObjectNames)
            {
                // Add new
                ((TagFieldBlock)tagFile.SelectField($"Block:object names")).AddElement();
                ((TagFieldElementString)tagFile.SelectField($"Block:object names[{nameIndex}]/String:name")).Data = name;

                nameIndex++;
            }

            if (scenarioType == "1,multiplayer")
            {
                // Spawns Section
                int totalScenCount = 0;
                ((TagFieldBlock)tagFile.SelectField($"Block:scenery palette")).RemoveAllElements(); // Remove all scenery from palette
                ((TagFieldBlock)tagFile.SelectField($"Block:scenery")).RemoveAllElements(); // Remove all scenery

                // Add respawn point scenery to palette
                Console.WriteLine("\nNo existing sceneries, adding respawn point\n");
                loadingForm.UpdateOutputBox("\nNo existing sceneries, adding respawn point\n", false);
                ((TagFieldBlock)tagFile.SelectField($"Block:scenery palette")).AddElement();
                ((TagFieldReference)tagFile.SelectField($"Block:scenery palette[0]/Reference:name")).Path = respawnScenPath;

                // Now add the spawn points
                int i = 0;
                foreach (var spawn in startLocations)
                {
                    ((TagFieldBlock)tagFile.SelectField($"Block:scenery")).AddElement();

                    // Type
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:scenery[{i}]/ShortBlockIndex:type")).Value = 0;

                    // Dropdown type and source (won't be valid without these)
                    ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{i}]/Struct:object data/Struct:object id/CharEnum:type")).Value = 6; // 6 is scenery
                    ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{i}]/Struct:object data/Struct:object id/CharEnum:source")).Value = 1; // 1 is editor

                    // Position
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:scenery[{i}]/Struct:object data/RealPoint3d:position")).Data = spawn.Position;

                    // Rotation
                    float[] rotation = new float[3] { spawn.Facing, 0.0f, 0.0f };
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:scenery[{i}]/Struct:object data/RealEulerAngles3d:rotation")).Data = rotation;

                    // Team
                    ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{i}]/Struct:multiplayer data/ShortEnum:owner team")).Value = spawn.Team;

                    i++;
                }

                Console.WriteLine("Done spawns");
                loadingForm.UpdateOutputBox("Done spawns", false);

                // MP netgame equipment section
                // First, remove all palette and entry data for equipment, vehicles and weapon
                ((TagFieldBlock)tagFile.SelectField($"Block:equipment palette")).RemoveAllElements();
                ((TagFieldBlock)tagFile.SelectField($"Block:equipment")).RemoveAllElements();
                ((TagFieldBlock)tagFile.SelectField($"Block:vehicle palette")).RemoveAllElements();
                ((TagFieldBlock)tagFile.SelectField($"Block:vehicles")).RemoveAllElements();
                ((TagFieldBlock)tagFile.SelectField($"Block:weapon palette")).RemoveAllElements();
                ((TagFieldBlock)tagFile.SelectField($"Block:weapons")).RemoveAllElements();

                Dictionary<string, int> netgamePaletteMapping = new Dictionary<string, int>();

                foreach (NetEquip netgameEquipEntry in netgameEquipment)
                {
                    string equipType = netgameEquipEntry.CollectionType.Split('\\')[netgameEquipEntry.CollectionType.Split('\\').Length - 1];

                    if (equipType == "frag_grenades" || equipType == "plasma_grenades" || equipType.Contains("powerup"))
                    {
                        if (equipType.Contains("powerup"))
                        {
                            equipType = "powerup";
                        }

                        AddToPaletteIfNotExists("equipment palette", "name", equipType, netgamePaletteMapping, utilsInstance.mpWeapMapping[equipType]);

                        int equipCount = ((TagFieldBlock)tagFile.SelectField("Block:equipment")).Elements.Count();
                        ((TagFieldBlock)tagFile.SelectField("Block:equipment")).AddElement();
                        int typeIndex = equipType != "powerup" ? netgamePaletteMapping[equipType] : netgamePaletteMapping["powerup"];
                        AddElementToBlock("equipment", equipCount, netgameEquipEntry, typeIndex, 1, 3);

                        continue;
                    }
                    else if (equipType == "")
                    {
                        // All games entries, ignore
                        Console.WriteLine("Ignoring blank weapon collection");
                        loadingForm.UpdateOutputBox("Ignoring blank weapon collection", false);
                        continue;
                    }
                    else if (equipType.Contains("ammo"))
                    {
                        // Ammo placement, ignore for now
                    }        
                    else if (netgameEquipEntry.CollectionType.Contains("vehicles"))
                    {
                        try
                        {
                            AddToPaletteIfNotExists("vehicle palette", "name", equipType, netgamePaletteMapping, utilsInstance.netVehiMapping[equipType]);

                            int vehiCount = ((TagFieldBlock)tagFile.SelectField("Block:vehicles")).Elements.Count();
                            ((TagFieldBlock)tagFile.SelectField("Block:vehicles")).AddElement();
                            AddElementToBlock("vehicles", vehiCount, netgameEquipEntry, netgamePaletteMapping[equipType], 1, 1);
                        }
                        catch (KeyNotFoundException)
                        {
                            loadingForm.UpdateOutputBox($"Vehicle type {equipType} is unknown, will be ignored", false);
                        }
                    }
                    else
                    {
                        try
                        {
                            AddToPaletteIfNotExists("weapon palette", "name", equipType, netgamePaletteMapping, utilsInstance.mpWeapMapping[equipType]);

                            int weapCount = ((TagFieldBlock)tagFile.SelectField("Block:weapons")).Elements.Count();
                            ((TagFieldBlock)tagFile.SelectField("Block:weapons")).AddElement();
                            AddElementToBlock("weapons", weapCount, netgameEquipEntry, netgamePaletteMapping[equipType], 1, 2);
                        }
                        catch (KeyNotFoundException)
                        {
                            loadingForm.UpdateOutputBox($"Weapon type {equipType} is unknown, will be ignored", false);
                        }
                    }
                }
                Console.WriteLine("Done netgame equipment");
                loadingForm.UpdateOutputBox("Done netgame equipment", false);

                // Scenery Section - the idea is to place blank scenery with bad references so they can be easily changed to ported versions by the user
                foreach (TagPath scenType in allScenTypes)
                {
                    // Check if current type exists in palette
                    bool typeAlreadyExists = false;
                    foreach (var paletteEntry in ((TagFieldBlock)tagFile.SelectField("Block:scenery palette")).Elements)
                    {
                        var x = ((TagFieldReference)paletteEntry.Fields[0]).Path;
                        if (x == scenType)
                        {
                            typeAlreadyExists = true;
                            break;
                        }
                    }

                    // Add palette entry if needed
                    if (!typeAlreadyExists)
                    {
                        int currentCount = ((TagFieldBlock)tagFile.SelectField("Block:scenery palette")).Elements.Count();
                        ((TagFieldBlock)tagFile.SelectField("Block:scenery palette")).AddElement();
                        ((TagFieldReference)tagFile.SelectField($"Block:scenery palette[{currentCount}]/Reference:name")).Path = scenType;
                    }
                }

                // Now add all of the scenery placements
                foreach (Scenery scenery in allScenEntries)
                {
                    int currentCount = ((TagFieldBlock)tagFile.SelectField("Block:scenery")).Elements.Count();
                    ((TagFieldBlock)tagFile.SelectField("Block:scenery")).AddElement();
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:scenery[{currentCount}]/ShortBlockIndex:type")).Value = scenery.TypeIndex + totalScenCount;

                    // Dropdown type and source (won't be valid without these)
                    ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/Struct:object id/CharEnum:type")).Value = 6; // 6 is scenery
                    ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/Struct:object id/CharEnum:source")).Value = 1; // 1 is editor

                    // Position
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/RealPoint3d:position")).Data = scenery.Position;

                    // Rotation
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/RealEulerAngles3d:rotation")).Data = scenery.Rotation;

                    // BSP placement related stuff
                    ((TagFieldBlockFlags)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/WordBlockFlags:manual bsp flags")).Value = scenery.ManualBsp;
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/Struct:object id/ShortBlockIndex:origin bsp index")).Value = scenery.OriginBsp;
                    ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/CharEnum:bsp policy")).Value = scenery.BspPolicy;

                    // Variant
                    ((TagFieldElementStringID)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:permutation data/StringId:variant name")).Data = scenery.VarName;
                }

                Console.WriteLine("Done scenery");
                loadingForm.UpdateOutputBox("Done scenery", false);

                // Crates section

                // Begin with creating the editor folders
                ((TagFieldBlock)tagFile.SelectField("Block:editor folders")).RemoveAllElements(); // Remove all editor folders
                ((TagFieldBlock)tagFile.SelectField("Block:crates")).RemoveAllElements(); // Remove all crates
                ((TagFieldBlock)tagFile.SelectField("Block:crate palette")).RemoveAllElements(); // Remove all crate types from palette
                for (int z = 0; z < 5; z++)
                {
                    int currentCount = ((TagFieldBlock)tagFile.SelectField("Block:editor folders")).Elements.Count();
                    ((TagFieldBlock)tagFile.SelectField("Block:editor folders")).AddElement();
                    // Name
                    var name = (TagFieldElementLongString)tagFile.SelectField($"Block:editor folders[{currentCount}]/LongString:name");
                    if (z == 0)
                    {
                        name.Data = "oddball";
                    }
                    else if (z == 1)
                    {
                        name.Data = "ctf";
                    }
                    else if (z == 2)
                    {
                        name.Data = "koth";
                    }
                    else if (z == 3)
                    {
                        name.Data = "assault";
                    }
                    else if (z == 4)
                    {
                        name.Data = "territories";
                    }
                }

                foreach (TagPath crateType in allCrateTypes)
                {
                    // Check if current type exists in palette
                    bool typeAlreadyExists = false;
                    foreach (var paletteEntry in ((TagFieldBlock)tagFile.SelectField("Block:crate palette")).Elements)
                    {
                        var x = ((TagFieldReference)paletteEntry.SelectField("Reference:name")).Path;
                        if (x == crateType)
                        {
                            typeAlreadyExists = true;
                            break;
                        }
                    }

                    // Add palette entry if needed
                    if (!typeAlreadyExists)
                    {
                        int currentCount = ((TagFieldBlock)tagFile.SelectField("Block:crate palette")).Elements.Count();
                        ((TagFieldBlock)tagFile.SelectField("Block:crate palette")).AddElement();
                        ((TagFieldReference)tagFile.SelectField($"Block:crate palette[{currentCount}]/Reference:name")).Path = crateType;
                    }
                }

                foreach (Crate crate in allCrateEntries)
                {
                    int currentCount = ((TagFieldBlock)tagFile.SelectField("Block:crates")).Elements.Count();
                    ((TagFieldBlock)tagFile.SelectField("Block:crates")).AddElement();
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:crates[{currentCount}]/ShortBlockIndex:type")).Value = crate.TypeIndex;

                    // Name
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:crates[{currentCount}]/ShortBlockIndex:name")).Value = crate.NameIndex;

                    // Dropdown type and source (won't be valid without these)
                    ((TagFieldEnum)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/Struct:object id/CharEnum:type")).Value = 10; // 10 is crate
                    ((TagFieldEnum)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/Struct:object id/CharEnum:source")).Value = 1; // 1 is editor

                    // Position
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/RealPoint3d:position")).Data = crate.Position;

                    // Rotation
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/RealEulerAngles3d:rotation")).Data = crate.Rotation;

                    // BSP placement related stuff
                    ((TagFieldBlockFlags)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/WordBlockFlags:manual bsp flags")).Value = crate.ManualBsp;
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/Struct:object id/ShortBlockIndex:origin bsp index")).Value = crate.OriginBsp;
                    ((TagFieldEnum)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/CharEnum:bsp policy")).Value = crate.BspPolicy;

                    // Variant
                    ((TagFieldElementStringID)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:permutation data/StringId:variant name")).Data = crate.VarName;
                }

                Dictionary<string, int> existingGametypeCrates = new Dictionary<string, int>();

                // Netgame flags to gametype crates section

                foreach (NetFlag netflag in allNetgameFlags)
                {
                    int currentCount = 0;
                    string temp = Regex.Replace(netflag.Type, @"^.*?,\s*", "");
                    string strippedName = Regex.Replace(temp, @"\d+$", "").Trim();
                    if (!existingGametypeCrates.ContainsKey(strippedName))
                    {
                        // Add type to crate palette
                        currentCount = ((TagFieldBlock)tagFile.SelectField("Block:crate palette")).Elements.Count();
                        ((TagFieldBlock)tagFile.SelectField("Block:crate palette")).AddElement();
                        ((TagFieldReference)tagFile.SelectField($"Block:crate palette[{currentCount}]/Reference:name")).Path = utilsInstance.netflagMapping[netflag.Type];
                        existingGametypeCrates.Add(strippedName, currentCount);
                    }
                    else
                    {
                        currentCount = existingGametypeCrates[strippedName];
                    }

                    currentCount = ((TagFieldBlock)tagFile.SelectField("Block:crates")).Elements.Count(); // Get current crate count
                    ((TagFieldBlock)tagFile.SelectField("Block:crates")).AddElement();
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:crates[{currentCount}]/ShortBlockIndex:type")).Value = existingGametypeCrates[strippedName];

                    // Name
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:crates[{currentCount}]/ShortBlockIndex:name")).Value = allObjectNames.IndexOf(netflag.Name);

                    // Dropdown type and source (won't be valid without these)
                    ((TagFieldEnum)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/Struct:object id/CharEnum:type")).Value = 10; // 10 is crate
                    ((TagFieldEnum)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/Struct:object id/CharEnum:source")).Value = 1; // 1 is editor

                    // Position
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/RealPoint3d:position")).Data = netflag.Position;

                    // Rotation
                    float[] rotation = new float[3] { netflag.Facing, 0.0f, 0.0f };
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/RealEulerAngles3d:rotation")).Data = rotation;

                    // Team
                    ((TagFieldEnum)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:multiplayer data/ShortEnum:owner team")).Value = netflag.Team;

                    // Grab editor folder
                    var editorFolder = ((TagFieldBlockIndex)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/ShortBlockIndex:editor folder"));

                    // Choose folder based on type
                    if (strippedName.ToLower().Contains("oddball"))
                    {
                        editorFolder.Value = 0;
                    }
                    else if (strippedName.ToLower().Contains("ctf"))
                    {
                        editorFolder.Value = 1;
                    }
                    else if (strippedName.ToLower().Contains("hill"))
                    {
                        editorFolder.Value = 2;
                    }
                    else if (strippedName.ToLower().Contains("assault"))
                    {
                        editorFolder.Value = 3;
                    }
                    else if (strippedName.ToLower().Contains("territories"))
                    {
                        editorFolder.Value = 4;
                    }
                    else
                    {
                        editorFolder.Value = -1;
                    }
                }

                Console.WriteLine("Done crates");
                loadingForm.UpdateOutputBox("Done crates", false);
            }
            else if (scenarioType == "0,solo")
            {
                // SP weapons
                Utils.WriteObjectData(tagFile, allSpWeapLocs, "weapons", loadingForm);

                // SP scenery section

                // Scenery palette
                ((TagFieldBlock)tagFile.SelectField($"Block:scenery palette")).RemoveAllElements();
                int x = 0;

                foreach (TagPath sceneryType in allScenTypes)
                {
                    ((TagFieldBlock)tagFile.SelectField($"Block:scenery palette")).AddElement();
                    ((TagFieldReference)tagFile.SelectField($"Block:scenery palette[{x}]/Reference:name")).Path = sceneryType;
                    x++;
                }

                Utils.WriteObjectData(tagFile, allScenEntries, "scenery", loadingForm);

                // SP crate section

                // Crate palette
                ((TagFieldBlock)tagFile.SelectField($"Block:crate palette")).RemoveAllElements();
                x = 0;

                foreach (TagPath crateType in allCrateTypes)
                {
                    ((TagFieldBlock)tagFile.SelectField($"Block:crate palette")).AddElement();
                    ((TagFieldReference)tagFile.SelectField($"Block:crate palette[{x}]/Reference:name")).Path = crateType;
                    x++;
                }

                Utils.WriteObjectData(tagFile, allCrateEntries, "crates", loadingForm);

                // Vehicle section
                Utils.WriteObjectData(tagFile, allVehiEntries, "vehicles", loadingForm);
            }

            // Trigger volumes section
            ((TagFieldBlock)tagFile.SelectField($"Block:trigger volumes")).RemoveAllElements();
            foreach (TrigVol vol in allTrigVols)
            {
                int currentCount = ((TagFieldBlock)tagFile.SelectField($"Block:trigger volumes")).Elements.Count();
                ((TagFieldBlock)tagFile.SelectField($"Block:trigger volumes")).AddElement();

                // Name
                ((TagFieldElementStringID)tagFile.SelectField($"Block:trigger volumes[{currentCount}]/StringId:name")).Data = vol.Name;

                // Forward
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:trigger volumes[{currentCount}]/RealVector3d:forward")).Data = vol.Forward;

                // Up
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:trigger volumes[{currentCount}]/RealVector3d:up")).Data = vol.Up;

                // Position
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:trigger volumes[{currentCount}]/RealPoint3d:position")).Data = vol.Position;

                // Extents
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:trigger volumes[{currentCount}]/RealPoint3d:extents")).Data = vol.Extents;
            }

            Console.WriteLine("Done trigger volumes");
            loadingForm.UpdateOutputBox("Done trigger volumes", false);

            // Decals section
            ((TagFieldBlock)tagFile.SelectField($"Block:decal palette")).RemoveAllElements(); // Remove all decals from palette
            ((TagFieldBlock)tagFile.SelectField($"Block:decals")).RemoveAllElements(); // Remove all decals
            foreach (TagPath decalType in allDecalTypes)
            {
                // Check if current type exists in palette
                bool typeAlreadyExists = false;
                foreach (var paletteEntry in ((TagFieldBlock)tagFile.SelectField($"Block:decal palette")).Elements)
                {
                    var x = ((TagFieldReference)paletteEntry.SelectField($"Reference:reference")).Path;
                    if (x == decalType)
                    {
                        typeAlreadyExists = true;
                        break;
                    }
                }

                // Add palette entry if needed
                if (!typeAlreadyExists)
                {
                    int currentCount = ((TagFieldBlock)tagFile.SelectField($"Block:decal palette")).Elements.Count();
                    ((TagFieldBlock)tagFile.SelectField($"Block:decal palette")).AddElement();
                    ((TagFieldReference)tagFile.SelectField($"Block:decal palette[{currentCount}]/Reference:reference")).Path = decalType;
                }
            }

            foreach (Decal decalEntry in allDecalEntries)
            {
                int currentCount = ((TagFieldBlock)tagFile.SelectField($"Block:decals")).Elements.Count();
                ((TagFieldBlock)tagFile.SelectField($"Block:decals")).AddElement();
                ((TagFieldBlockIndex)tagFile.SelectField($"Block:decals[{currentCount}]/ShortBlockIndex:decal palette index")).Value = int.Parse(decalEntry.Type);

                // Position
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:decals[{currentCount}]/RealPoint3d:position")).Data = decalEntry.Position;

                // Rotation stuff below - only god fucking knows what this is doing, and either way it doesnt work properly
                double pitchDegrees = double.Parse(decalEntry.Pitch);
                double yawDegrees = double.Parse(decalEntry.Yaw);

                // Convert pitch and yaw angles from degrees to radians
                double pitchRadians = Math.PI * pitchDegrees / 180.0;
                double yawRadians = Math.PI * yawDegrees / 180.0;

                // Calculate quaternion components directly
                double halfYaw = yawRadians * 0.5;
                double halfPitch = pitchRadians * 0.5;

                Quaternion finalRotation = new Quaternion(
                    (float)(Math.Sin(halfYaw) * Math.Cos(halfPitch)),
                    (float)(-Math.Sin(halfPitch)),
                    (float)(Math.Cos(halfYaw) * Math.Sin(halfPitch)),
                    (float)(Math.Cos(halfPitch) * Math.Cos(halfYaw))
                );

                string quaternionString = $"{finalRotation.X},{finalRotation.Y},{finalRotation.Z},{finalRotation.W}";
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:decals[{currentCount}]/RealQuaternion:rotation")).Data = quaternionString.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();
            }

            Console.WriteLine("Done decals");
            loadingForm.UpdateOutputBox("Done decals", false);

            // Device machine section
            Utils.WriteObjectData(tagFile, allMachineEntries, "machines", loadingForm);
            Console.WriteLine("Done device machines");
            loadingForm.UpdateOutputBox("Done device machines", false);

            // Device controls section
            Utils.WriteObjectData(tagFile, allControlEntries, "controls", loadingForm);
            Console.WriteLine("Done device controls");
            loadingForm.UpdateOutputBox("Done device controls", false);

            ((TagFieldBlock)tagFile.SelectField($"Block:device groups")).RemoveAllElements();
            int groupIndex = 0;
            foreach (DeviceGroup deviceGroupEntry in allDeviceGroups)
            {
                ((TagFieldBlock)tagFile.SelectField($"Block:device groups")).AddElement();
                ((TagFieldElementString)tagFile.SelectField($"Block:device groups[{groupIndex}]/String:name")).Data = deviceGroupEntry.Name;
                ((TagFieldElementSingle)tagFile.SelectField($"Block:device groups[{groupIndex}]/Real:initial value")).Data = deviceGroupEntry.InitVal;
                ((TagFieldFlags)tagFile.SelectField($"Block:device groups[{groupIndex}]/Flags:flags")).RawValue = deviceGroupEntry.Flags;
                groupIndex++;
            }

            Console.WriteLine("Done device groups");
            loadingForm.UpdateOutputBox("Done device groups", false);

            try
            {
                tagFile.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tag save failure: {ex}");
                loadingForm.UpdateOutputBox("Tag failed to save. Usually it isn't necessary, but close Sapien/TagTest and try again.", false);
            }

            Console.WriteLine("\nScenario data conversion complete!");
            loadingForm.UpdateOutputBox("\nScenario data conversion complete!", false);
        }
    }
}