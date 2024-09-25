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
    public string position { get; set; }
    public string facing { get; set; }
    public string team { get; set; }
    public string type0 { get; set; }
    public string type1 { get; set; }
    public string type2 { get; set; }
    public string type3 { get; set; }
    public string spawnType0 { get; set; }
    public string spawnType1 { get; set; }
    public string spawnType2 { get; set; }
    public string spawnType3 { get; set; }
}

public class ObjectPlacement
{
    public int typeIndex { get; set; }
    public int nameIndex { get; set; }
    public uint flags { get; set; }
    public float[] position { get; set; }
    public float[] rotation { get; set; }
    public float scale { get; set; }
    public string varName { get; set; }
    public uint manualBsp {  get; set; }
    public int originBsp { get; set; }
    public int bspPolicy { get; set; }
}

class NetEquip : ObjectPlacement
{
    public string spawnTime { get; set; }
    public string collectionType { get; set; }
}

class SpWeapLoc : ObjectPlacement
{
    public int roundsLeft { get; set; }
    public int roundsLoaded { get; set; }
}

class Scenery : ObjectPlacement
{
    public int pathfindingType { get; set; }
    public int lightmappingType { get; set; }
}

class TrigVol
{
    public string name { get; set; }
    public string position { get; set; }
    public string extents { get; set; }
    public string forward { get; set; }
    public string up { get; set; }
}

class Vehicle : ObjectPlacement
{
    public float bodyVitality { get; set; }
}

class Crate : ObjectPlacement {}

class NetFlag
{
    public string name { get; set; }
    public string position { get; set; }
    public string rotation { get; set; }
    public string type { get; set; }
    public string team { get; set; }
}

class Decal
{
    public string type { get; set; }
    public string yaw { get; set; }
    public string pitch { get; set; }
    public string position { get; set; }
}

class ScenData
{
    public static void ConvertScenarioData(string scenPath, string xmlPath, Loading loadingForm)
    {
        string h3ekPath = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);

        // Make sure we have a scenario backup
        Utils.BackupScenario(scenPath, xmlPath, loadingForm);

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
                    StartLoc startLocation = new StartLoc();
                    startLocation.position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    startLocation.facing = element.SelectSingleNode("./field[@name='facing']").InnerText.Trim();
                    startLocation.team = element.SelectSingleNode("./field[@name='team designator']").InnerText.Trim();
                    startLocation.type0 = element.SelectSingleNode("./field[@name='type 0']").InnerText.Trim();
                    startLocation.type1 = element.SelectSingleNode("./field[@name='type 1']").InnerText.Trim();
                    startLocation.type2 = element.SelectSingleNode("./field[@name='type 2']").InnerText.Trim();
                    startLocation.type3 = element.SelectSingleNode("./field[@name='type 3']").InnerText.Trim();
                    startLocation.spawnType0 = element.SelectSingleNode("./field[@name='spawn type 0']").InnerText.Trim();
                    startLocation.spawnType1 = element.SelectSingleNode("./field[@name='spawn type 1']").InnerText.Trim();
                    startLocation.spawnType2 = element.SelectSingleNode("./field[@name='spawn type 2']").InnerText.Trim();
                    startLocation.spawnType3 = element.SelectSingleNode("./field[@name='spawn type 3']").InnerText.Trim();

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
                        NetEquip netgameEquip = new NetEquip();
                        netgameEquip.position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                        netgameEquip.rotation = element.SelectSingleNode("./field[@name='orientation']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                        netgameEquip.spawnTime = element.SelectSingleNode("./field[@name='spawn time (in seconds, 0 = default)']").InnerText.Trim();
                        netgameEquip.collectionType = element.SelectSingleNode("./tag_reference[@name='item/vehicle collection']").InnerText.Trim();

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
            Utils.ConvertPalette(scenPath, xmlPath, loadingForm, scenfile, "weapon");
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
            Utils.ConvertPalette(scenPath, xmlPath, loadingForm, scenfile, "vehicle");
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
                    TrigVol triggerVolume = new TrigVol();
                    triggerVolume.name = element.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                    triggerVolume.position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    triggerVolume.extents = element.SelectSingleNode("./field[@name='extents']").InnerText.Trim();
                    triggerVolume.forward = element.SelectSingleNode("./field[@name='forward']").InnerText.Trim();
                    triggerVolume.up = element.SelectSingleNode("./field[@name='up']").InnerText.Trim();

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
                    NetFlag netFlag = new NetFlag();
                    netFlag.name = element.Attributes["name"].Value;
                    netFlag.position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    netFlag.rotation = element.SelectSingleNode("./field[@name='facing']").InnerText.Trim();
                    netFlag.type = element.SelectSingleNode("./field[@name='type']").InnerText.Trim();
                    netFlag.team = element.SelectSingleNode("./field[@name='team designator']").InnerText.Trim();

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
                    Decal decal = new Decal();
                    decal.type = element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.ToString();
                    decal.yaw = element.SelectSingleNode("./field[@name='yaw[-127,127]']").InnerText.Trim();
                    decal.pitch = element.SelectSingleNode("./field[@name='pitch[-127,127]']").InnerText.Trim();
                    decal.position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();

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

        XmlToTag(allObjectNames, allStartingLocs, allNetgameEquipLocs, allSpWeaponLocs, allScenTypes, allScenEntries, allTrigVols, allVehiEntries, allCrateTypes, allCrateEntries, allNetgameFlags, allDecalTypes, allDecalEntries, h3ekPath, scenPath, loadingForm, scenarioType);
    }

    static void XmlToTag(List<string> allObjectNames, List<StartLoc> startLocations, List<NetEquip> netgameEquipment, List<SpWeapLoc> allSpWeapLocs, List<TagPath> allScenTypes, List<Scenery> allScenEntries, List<TrigVol> allTrigVols, List<Vehicle> allVehiEntries, List<TagPath> allCrateTypes, List<Crate> allCrateEntries, List<NetFlag> allNetgameFlags, List<TagPath> allDecalTypes, List<Decal> allDecalEntries, string h3ekPath, string scenpath, Loading loadingForm, string scenarioType)
    {
        Utils utilsInstance = new Utils();
        var tagPath = TagPath.FromPathAndType(Path.ChangeExtension(scenpath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
        var respawnScenPath = TagPath.FromPathAndType(@"objects\multi\spawning\respawn_point", "scen*");
        int respawnScenIndex = 0;
        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ekPath);

        using (var tagFile = new TagFile(tagPath))
        {
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
                int i = 0;
                int tempIndex = 0;
                bool respawnFound = false;
                int totalScenCount = 0;
                int totalVehiCount = 0;

                ((TagFieldBlock)tagFile.Fields[21]).RemoveAllElements(); // Remove all scenery from palette
                ((TagFieldBlock)tagFile.Fields[20]).RemoveAllElements(); // Remove all scenery

                // Add respawn point scenery, if it doesn't already exist
                if (((TagFieldBlock)tagFile.Fields[21]).Elements.Count() != 0)
                {
                    foreach (var scenType in ((TagFieldBlock)tagFile.Fields[21]).Elements)
                    {
                        if (((TagFieldReference)scenType.Fields[0]).Path == respawnScenPath)
                        {
                            // Respawn point scenery already in palette, set index
                            respawnScenIndex = tempIndex;
                            respawnFound = true;
                            break;
                        }
                        tempIndex++;
                    }
                    if (respawnFound == false)
                    {
                        // Respawn point is not in the palette, add it and set the index
                        respawnScenIndex = ((TagFieldBlock)tagFile.Fields[21]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                        var sceneryTag = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[respawnScenIndex].Fields[0];
                        sceneryTag.Path = respawnScenPath;
                        totalScenCount++;
                    }
                }
                else
                {
                    Console.WriteLine("\nNo existing sceneries, adding respawn point\n");
                    loadingForm.UpdateOutputBox("\nNo existing sceneries, adding respawn point\n", false);
                    ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                    var sceneryTag = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[0].Fields[0];
                    sceneryTag.Path = respawnScenPath;
                }



                foreach (var spawn in startLocations)
                {
                    // Add new
                    ((TagFieldBlock)tagFile.Fields[20]).AddElement();

                    // Type
                    var sceneryType = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[1];
                    sceneryType.Value = respawnScenIndex;

                    // Dropdown type and source (won't be valid without these)
                    var dropdownType = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                    var dropdownSource = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                    dropdownType.Value = 6; // 6 for scenery
                    dropdownSource.Value = 1; // 1 for editor

                    // Position
                    var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[0].FieldName;
                    var xyzPos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[2];
                    xyzPos.Data = spawn.position.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Rotation
                    var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[3];
                    string xyzAngle = spawn.facing + ",0,0";
                    rotation.Data = xyzAngle.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Team
                    var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[7]).Elements[0].Fields[0].FieldName;
                    var team = (TagFieldEnum)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[7]).Elements[0].Fields[3];
                    team.Value = int.Parse(new string(spawn.team.TakeWhile(c => c != ',').ToArray()));

                    i++;
                }

                Console.WriteLine("Done spawns");
                loadingForm.UpdateOutputBox("Done spawns", false);

                // MP netgame equipment
                ((TagFieldBlock)tagFile.Fields[27]).RemoveAllElements(); // Remove all equip palette entries
                ((TagFieldBlock)tagFile.Fields[26]).RemoveAllElements(); // Remove all equipment
                ((TagFieldBlock)tagFile.Fields[25]).RemoveAllElements(); // Remove all vehicle palette entries
                ((TagFieldBlock)tagFile.Fields[24]).RemoveAllElements(); // Remove all vehicles
                ((TagFieldBlock)tagFile.Fields[29]).RemoveAllElements(); // Remove all weapon palette entries
                ((TagFieldBlock)tagFile.Fields[28]).RemoveAllElements(); // Remove all weapons
                Dictionary<string, int> weapPaletteMapping = new Dictionary<string, int>();

                foreach (NetEquip netgameEquipEntry in netgameEquipment)
                {
                    string equipType = netgameEquipEntry.collectionType.Split('\\')[netgameEquipEntry.collectionType.Split('\\').Length - 1];

                    if (equipType == "frag_grenades" || equipType == "plasma_grenades")
                    {
                        // Grenade stuff, need to treat as equipment not weapon
                        Console.WriteLine("Adding " + equipType + " netgame equipment");
                        loadingForm.UpdateOutputBox("Adding " + equipType + " netgame equipment", false);

                        // Equipment, check if palette entry exists first
                        bool equipEntryExists = false;
                        foreach (var paletteEntry in ((TagFieldBlock)tagFile.Fields[27]).Elements)
                        {
                            var tempType = utilsInstance.mpWeapMapping[equipType];
                            if (((TagFieldReference)paletteEntry.Fields[0]).Path == tempType)
                            {
                                equipEntryExists = true;
                            }
                        }

                        // Add palette entry if needed
                        if (!equipEntryExists)
                        {
                            int currentPaletteCount = ((TagFieldBlock)tagFile.Fields[27]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[27]).AddElement();
                            var equipRef = (TagFieldReference)((TagFieldBlock)tagFile.Fields[27]).Elements[currentPaletteCount].Fields[0];
                            equipRef.Path = utilsInstance.mpWeapMapping[equipType];
                            weapPaletteMapping.Add(equipType, currentPaletteCount);
                        }

                        // Now add the equipment itself
                        int equipCount = ((TagFieldBlock)tagFile.Fields[26]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[26]).AddElement(); // Add new equipment entry

                        // XYZ
                        var equipXyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[4]).Elements[0].Fields[2];
                        equipXyz.Data = netgameEquipEntry.position;

                        // Rotation
                        var equipRotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[4]).Elements[0].Fields[3];
                        equipRotation.Data = netgameEquipEntry.rotation;

                        // Type
                        var equipTagType = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[1];
                        equipTagType.Value = weapPaletteMapping[equipType];

                        // Spawn timer
                        var equipTimer = (TagFieldElementInteger)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[6]).Elements[0].Fields[8];
                        equipTimer.Data = uint.Parse(netgameEquipEntry.spawnTime);

                        // Dropdown type and source (won't be valid without these)
                        var dropdownType = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdownSource = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdownType.Value = 3; // 3 for equipment
                        dropdownSource.Value = 1; // 1 for editor

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
                    else if (equipType.Contains("powerup"))
                    {
                        // Powerup, add equipment, check if palette entry exists first
                        bool equipEntryExists = false;
                        foreach (var paletteEntry in ((TagFieldBlock)tagFile.Fields[27]).Elements)
                        {
                            var tempType = utilsInstance.mpWeapMapping["powerup"];
                            if (((TagFieldReference)paletteEntry.Fields[0]).Path == tempType)
                            {
                                equipEntryExists = true;
                            }
                        }

                        // Add palette entry if needed
                        if (!equipEntryExists)
                        {
                            int currentPaletteCount = ((TagFieldBlock)tagFile.Fields[27]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[27]).AddElement();
                            var equipRef = (TagFieldReference)((TagFieldBlock)tagFile.Fields[27]).Elements[currentPaletteCount].Fields[0];
                            equipRef.Path = utilsInstance.mpWeapMapping["powerup"];
                            weapPaletteMapping.Add("powerup", currentPaletteCount);
                        }

                        // Now add the equipment itself
                        int equipCount = ((TagFieldBlock)tagFile.Fields[26]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[26]).AddElement(); // Add new equipment entry

                        // XYZ
                        var equipXyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[4]).Elements[0].Fields[2];
                        equipXyz.Data = netgameEquipEntry.position;

                        // Rotation
                        var equipRotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[4]).Elements[0].Fields[3];
                        equipRotation.Data = netgameEquipEntry.rotation;

                        // Type
                        var equipTagType = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[1];
                        equipTagType.Value = weapPaletteMapping["powerup"];

                        // Dropdown type and source (won't be valid without these)
                        var dropdownType = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdownSource = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equipCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdownType.Value = 3; // 3 for equipment
                        dropdownSource.Value = 1; // 1 for editor
                    }
                    else if (netgameEquipEntry.collectionType.Contains("vehicles"))
                    {
                        // Check if current type exists in palette
                        bool typeAlreadyExists = false;
                        foreach (var paletteEntry in ((TagFieldBlock)tagFile.Fields[25]).Elements)
                        {
                            var x = ((TagFieldReference)paletteEntry.Fields[0]).Path;
                            if (x == utilsInstance.netVehiMapping[equipType])
                            {
                                typeAlreadyExists = true;
                                break;
                            }
                        }

                        // Add palette entry if needed
                        int currentPaletteCount = ((TagFieldBlock)tagFile.Fields[25]).Elements.Count();
                        if (!typeAlreadyExists)
                        {
                            ((TagFieldBlock)tagFile.Fields[25]).AddElement();
                            var vehiRef = (TagFieldReference)((TagFieldBlock)tagFile.Fields[25]).Elements[currentPaletteCount].Fields[0];
                            vehiRef.Path = utilsInstance.netVehiMapping[equipType];
                            totalVehiCount++;
                        }

                        int vehiCount = ((TagFieldBlock)tagFile.Fields[24]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[24]).AddElement();
                        var typeRef = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[24]).Elements[vehiCount].Fields[1];
                        typeRef.Value = currentPaletteCount;

                        // Dropdown type and source (won't be valid without these)
                        var dropdownType = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehiCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdownSource = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehiCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdownType.Value = 1; // 1 for vehicle
                        dropdownSource.Value = 1; // 1 for editor

                        // Position
                        var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehiCount].Fields[4]).Elements[0].Fields[0].FieldName;
                        var position = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehiCount].Fields[4]).Elements[0].Fields[2];
                        position.Data = netgameEquipEntry.position;

                        // Rotation
                        var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehiCount].Fields[4]).Elements[0].Fields[3];
                        rotation.Data = netgameEquipEntry.rotation;
                    }
                    else
                    {
                        // Weapon, check if palette entry exists first
                        Console.WriteLine("Adding " + equipType + " weapon");
                        loadingForm.UpdateOutputBox("Adding " + equipType + " weapon", false);
                        bool weapEntryExists = false;
                        foreach (var paletteEntry in ((TagFieldBlock)tagFile.Fields[29]).Elements)
                        {
                            var tempType = utilsInstance.mpWeapMapping[equipType];
                            if (((TagFieldReference)paletteEntry.Fields[0]).Path == tempType)
                            {
                                weapEntryExists = true;
                            }
                        }

                        // Add palette entry if needed
                        if (!weapEntryExists)
                        {
                            int currentCount = ((TagFieldBlock)tagFile.Fields[29]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[29]).AddElement();
                            var weapRef = (TagFieldReference)((TagFieldBlock)tagFile.Fields[29]).Elements[currentCount].Fields[0];
                            weapRef.Path = utilsInstance.mpWeapMapping[equipType];
                            weapPaletteMapping.Add(equipType, currentCount);
                        }

                        // Now add the weapon itself
                        int weapCount = ((TagFieldBlock)tagFile.Fields[28]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[28]).AddElement(); // Add new weapon entry

                        // XYZ
                        var weapXyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapCount].Fields[4]).Elements[0].Fields[2];
                        weapXyz.Data = netgameEquipEntry.position;

                        // Rotation
                        var weapRotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapCount].Fields[4]).Elements[0].Fields[3];
                        weapRotation.Data = netgameEquipEntry.rotation;

                        // Type
                        var weapTagRef = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[28]).Elements[weapCount].Fields[1];
                        weapTagRef.Value = weapPaletteMapping[equipType];

                        // Spawn timer
                        var weapTimer = (TagFieldElementInteger)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapCount].Fields[7]).Elements[0].Fields[8];
                        weapTimer.Data = uint.Parse(netgameEquipEntry.spawnTime);

                        // Dropdown type and source (won't be valid without these)
                        var dropdownType = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdownSource = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdownType.Value = 2; // 2 for weapon
                        dropdownSource.Value = 1; // 1 for editor
                    }

                    Console.WriteLine("Done netgame equipment");
                    loadingForm.UpdateOutputBox("Done netgame equipment", false);

                    // Scenery Section - the idea is to place blank scenery with bad references so they can be easily changed to ported versions by the user

                    foreach (TagPath scenType in allScenTypes)
                    {
                        // Check if current type exists in palette
                        bool typeAlreadyExists = false;
                        foreach (var paletteEntry in ((TagFieldBlock)tagFile.Fields[21]).Elements)
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
                            int currentCount = ((TagFieldBlock)tagFile.Fields[21]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                            var scenTypeRef = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[currentCount].Fields[0];
                            scenTypeRef.Path = scenType;
                        }
                    }

                    // Now add all of the scenery placements
                    foreach (Scenery scenery in allScenEntries)
                    {
                        int currentCount = ((TagFieldBlock)tagFile.Fields[20]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[20]).AddElement();
                        var typeRef = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[20]).Elements[currentCount].Fields[1];
                        int index = scenery.typeIndex + totalScenCount;
                        typeRef.Value = scenery.typeIndex + totalScenCount;

                        // Dropdown type and source (won't be valid without these)
                        var dropdownType = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[currentCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdownSource = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[currentCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdownType.Value = 6; // 6 for scenery
                        dropdownSource.Value = 1; // 1 for editor

                        // Position
                        var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[currentCount].Fields[4]).Elements[0].Fields[0].FieldName;
                        var position = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[currentCount].Fields[4]).Elements[0].Fields[2];
                        position.Data = scenery.position;

                        // Rotation
                        var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[currentCount].Fields[4]).Elements[0].Fields[3];
                        rotation.Data = scenery.rotation;

                        ((TagFieldBlockFlags)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/WordBlockFlags:manual bsp flags")).Value = scenery.manualBsp;
                        ((TagFieldBlockIndex)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/Struct:object id/ShortBlockIndex:origin bsp index")).Value = scenery.originBsp;
                        ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{currentCount}]/Struct:object data/CharEnum:bsp policy")).Value = scenery.bspPolicy;

                        // Variant
                        var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[currentCount].Fields[5]).Elements[0].Fields[0].FieldName;
                        var variant = (TagFieldElementStringID)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[currentCount].Fields[5]).Elements[0].Fields[0];
                        variant.Data = scenery.varName;
                    }

                    Console.WriteLine("Done scenery");
                    loadingForm.UpdateOutputBox("Done scenery", false);

                    // Crates section

                    // Begin with creating the editor folders
                    ((TagFieldBlock)tagFile.Fields[125]).RemoveAllElements(); // Remove all editor folders
                    ((TagFieldBlock)tagFile.Fields[118]).RemoveAllElements(); // Remove all crates
                    ((TagFieldBlock)tagFile.Fields[119]).RemoveAllElements(); // Remove all crate types from palette
                    for (int z = 0; z < 5; z++)
                    {
                        int currentCount = ((TagFieldBlock)tagFile.Fields[125]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[125]).AddElement();
                        // Name
                        var name = (TagFieldElementLongString)((TagFieldBlock)tagFile.Fields[125]).Elements[currentCount].Fields[1];
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
                        foreach (var paletteEntry in ((TagFieldBlock)tagFile.Fields[119]).Elements)
                        {
                            var x = ((TagFieldReference)paletteEntry.Fields[0]).Path;
                            if (x == crateType)
                            {
                                typeAlreadyExists = true;
                                break;
                            }
                        }

                        // Add palette entry if needed
                        if (!typeAlreadyExists)
                        {
                            int currentCount = ((TagFieldBlock)tagFile.Fields[119]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[119]).AddElement();
                            var crateRef = (TagFieldReference)((TagFieldBlock)tagFile.Fields[119]).Elements[currentCount].Fields[0];
                            crateRef.Path = crateType;
                        }
                    }

                    foreach (Crate crate in allCrateEntries)
                    {
                        int currentCount = ((TagFieldBlock)tagFile.Fields[118]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[118]).AddElement();
                        var typeRef = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[1];
                        typeRef.Value = crate.typeIndex;

                        // Name
                        var name = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[3];
                        name.Value = crate.nameIndex;

                        // Dropdown type and source (won't be valid without these)
                        var dropdownType = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdownSource = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdownType.Value = 10; // 1 for crate
                        dropdownSource.Value = 1; // 1 for editor

                        // Position
                        var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[0].FieldName;
                        var position = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[2];
                        position.Data = crate.position;

                        // Rotation
                        var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[3];
                        rotation.Data = crate.rotation;

                        ((TagFieldBlockFlags)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/WordBlockFlags:manual bsp flags")).Value = crate.manualBsp;
                        ((TagFieldBlockIndex)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/Struct:object id/ShortBlockIndex:origin bsp index")).Value = crate.originBsp;
                        ((TagFieldEnum)tagFile.SelectField($"Block:crates[{currentCount}]/Struct:object data/CharEnum:bsp policy")).Value = crate.bspPolicy;

                        // Variant
                        var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[5]).Elements[0].Fields[0].FieldName;
                        var variant = (TagFieldElementStringID)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[5]).Elements[0].Fields[0];
                        variant.Data = crate.varName;
                    }

                    Dictionary<string, int> existingGametypeCrates = new Dictionary<string, int>();

                    // Netgame flags to gametype crates section
                    foreach (NetFlag netflag in allNetgameFlags)
                    {
                        int typeIndex = 0;
                        string temp = Regex.Replace(netflag.type, @"^.*?,\s*", "");
                        string strippedName = Regex.Replace(temp, @"\d+$", "").Trim();
                        if (!existingGametypeCrates.ContainsKey(strippedName))
                        {
                            // Add type to crate palette
                            typeIndex = ((TagFieldBlock)tagFile.Fields[119]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[119]).AddElement();
                            var crateRef = (TagFieldReference)((TagFieldBlock)tagFile.Fields[119]).Elements[typeIndex].Fields[0];
                            crateRef.Path = utilsInstance.netflagMapping[netflag.type];
                            existingGametypeCrates.Add(strippedName, typeIndex);
                        }
                        else
                        {
                            typeIndex = existingGametypeCrates[strippedName];
                        }

                        int currentCount = ((TagFieldBlock)tagFile.Fields[118]).Elements.Count(); // Get current crate count
                        ((TagFieldBlock)tagFile.Fields[118]).AddElement();
                        var typeRef = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[1];
                        typeRef.Value = existingGametypeCrates[strippedName];

                        // Name
                        var name = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[3];
                        name.Value = allObjectNames.IndexOf(netflag.name);

                        // Dropdown type and source (won't be valid without these)
                        var dropdownType = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdownSource = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdownType.Value = 10; // 1 for crate
                        dropdownSource.Value = 1; // 1 for editor

                        // Position
                        var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[0].FieldName;
                        var position = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[2];
                        position.Data = netflag.position.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                        // Rotation
                        var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[3];
                        string angleXyz = netflag.rotation + ",0,0";
                        rotation.Data = angleXyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                        // Team
                        var team = (TagFieldEnum)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[7]).Elements[0].Fields[3];
                        team.Value = int.Parse(new string(netflag.team.TakeWhile(c => c != ',').ToArray()));

                        // Grab editor folder
                        var folder = (TagFieldBlockIndex)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[currentCount].Fields[4]).Elements[0].Fields[11];

                        // Choose folder based on type
                        if (strippedName.ToLower().Contains("oddball"))
                        {
                            folder.Value = 0;
                        }
                        else if (strippedName.ToLower().Contains("ctf"))
                        {
                            folder.Value = 1;
                        }
                        else if (strippedName.ToLower().Contains("hill"))
                        {
                            folder.Value = 2;
                        }
                        else if (strippedName.ToLower().Contains("assault"))
                        {
                            folder.Value = 3;
                        }
                        else if (strippedName.ToLower().Contains("territories"))
                        {
                            folder.Value = 4;
                        }
                        else
                        {
                            folder.Value = -1;
                        }
                    }

                    Console.WriteLine("Done crates");
                    loadingForm.UpdateOutputBox("Done crates", false);
                }
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
                int currentCount = ((TagFieldBlock)tagFile.Fields[55]).Elements.Count();
                ((TagFieldBlock)tagFile.Fields[55]).AddElement();

                // Name
                var name = (TagFieldElementStringID)((TagFieldBlock)tagFile.Fields[55]).Elements[currentCount].Fields[0];
                name.Data = vol.name;

                // Forward
                var forward = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[currentCount].Fields[4];
                forward.Data = vol.forward.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Up
                var up = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[currentCount].Fields[5];
                up.Data = vol.up.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Position
                var position = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[currentCount].Fields[6];
                position.Data = vol.position.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Extents
                var extents = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[currentCount].Fields[7];
                extents.Data = vol.extents.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();
            }

            Console.WriteLine("Done trigger volumes");
            loadingForm.UpdateOutputBox("Done trigger volumes", false);

            // Decals section
            ((TagFieldBlock)tagFile.Fields[78]).RemoveAllElements(); // Remove all decals from palette
            ((TagFieldBlock)tagFile.Fields[77]).RemoveAllElements(); // Remove all decals
            foreach (TagPath decalType in allDecalTypes)
            {
                // Check if current type exists in palette
                bool typeAlreadyExists = false;
                foreach (var paletteEntry in ((TagFieldBlock)tagFile.Fields[78]).Elements)
                {
                    var x = ((TagFieldReference)paletteEntry.Fields[0]).Path;
                    if (x == decalType)
                    {
                        typeAlreadyExists = true;
                        break;
                    }
                }

                // Add palette entry if needed
                if (!typeAlreadyExists)
                {
                    int currentCount = ((TagFieldBlock)tagFile.Fields[78]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[78]).AddElement();
                    var sceneryRef = (TagFieldReference)((TagFieldBlock)tagFile.Fields[78]).Elements[currentCount].Fields[0];
                    sceneryRef.Path = decalType;
                }
            }

            foreach (Decal decalEntry in allDecalEntries)
            {
                int currentCount = ((TagFieldBlock)tagFile.Fields[77]).Elements.Count();
                ((TagFieldBlock)tagFile.Fields[77]).AddElement();
                var typeRef = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[77]).Elements[currentCount].Fields[1];
                typeRef.Value = int.Parse(decalEntry.type);

                // Position
                var position = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[77]).Elements[currentCount].Fields[4];
                position.Data = decalEntry.position.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Rotation stuff below - only god fucking knows what this is doing, and either way it doesnt work properly
                double pitchDegrees = double.Parse(decalEntry.pitch);
                double yawDegrees = double.Parse(decalEntry.yaw);

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
                var rotation = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[77]).Elements[currentCount].Fields[3];
                rotation.Data = quaternionString.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();
            }

            try
            {
                tagFile.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tag save failure: {ex}");
                loadingForm.UpdateOutputBox("Tag failed to save. Usually it isn't necessary, but close Sapien/TagTest and try again.", false);
            }
            
            Console.WriteLine("Done decals");
            loadingForm.UpdateOutputBox("Done decals", false);

            Console.WriteLine("\nScenario data conversion complete!");
            loadingForm.UpdateOutputBox("\nScenario data conversion complete!", false);
        }
    }
}