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
    public string position_xyz { get; set; }
    public string facing_angle { get; set; }
    public string team_enum { get; set; }
    public string type_0 { get; set; }
    public string type_1 { get; set; }
    public string type_2 { get; set; }
    public string type_3 { get; set; }
    public string spawn_type_0 { get; set; }
    public string spawn_type_1 { get; set; }
    public string spawn_type_2 { get; set; }
    public string spawn_type_3 { get; set; }
}

public class ObjectPlacement
{
    public int type_index { get; set; }
    public int name_index { get; set; }
    public uint flags { get; set; }
    public float[] position { get; set; }
    public float[] rotation { get; set; }
    public float scale { get; set; }
    public string var_name { get; set; }
    public uint manual_bsp {  get; set; }
    public int origin_bsp { get; set; }
    public int bsp_policy { get; set; }
}

class NetEquip : ObjectPlacement
{
    public string spawn_time { get; set; }
    public string collection_type { get; set; }
}

class SpWeapLoc : ObjectPlacement
{
    public int rounds_left { get; set; }
    public int rounds_loaded { get; set; }
}

class Scenery : ObjectPlacement
{
    public int pathfinding_type { get; set; }
    public int lightmapping_type { get; set; }
}

class TrigVol
{
    public string vol_name { get; set; }
    public string vol_xyz { get; set; }
    public string vol_ext { get; set; }
    public string vol_fwd { get; set; }
    public string vol_up { get; set; }
}

class Vehicle : ObjectPlacement
{
    public float body_vitality { get; set; }
}

class Crate : ObjectPlacement {}

class NetFlag
{
    public string netflag_name { get; set; }
    public string netflag_xyz { get; set; }
    public string netflag_orient { get; set; }
    public string netflag_type { get; set; }
    public string netflag_team { get; set; }
}

class Decal
{
    public string decal_type { get; set; }
    public string decal_yaw { get; set; }
    public string decal_pitch { get; set; }
    public string decal_xyz { get; set; }
}


class ScenData
{
    public static void ScenarioConverter(string scenPath, string xmlPath, Loading loadingForm)
    {
        string h3ek_path = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);

        // Make sure we have a scenario backup
        Utils.BackupScenario(scenPath, xmlPath, loadingForm);

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
        ConvertScenData(xmlPath, h3ek_path, scenPath, loadingForm);
    }

    static void ConvertScenData(string xml_path, string h3ek_path, string scen_path, Loading loadingForm)
    {
        xml_path = Utils.ConvertXML(xml_path, loadingForm);

        XmlDocument scenfile = new XmlDocument();
        scenfile.Load(xml_path);

        XmlNode root = scenfile.DocumentElement;

        string scenario_type = root.SelectNodes(".//field[@name='type']")[0].InnerText.Trim();
        XmlNodeList player_start_loc_block = root.SelectNodes(".//block[@name='player starting locations']");
        XmlNodeList netgame_objs_entries_block = root.SelectNodes(".//block[@name='netgame equipment']");
        XmlNodeList weapon_sp_entries_block = root.SelectNodes(".//block[@name='weapons']");
        XmlNodeList scen_palette_block = root.SelectNodes(".//block[@name='scenery palette']");
        XmlNodeList scen_entries_block = root.SelectNodes(".//block[@name='scenery']");
        XmlNodeList trig_vol_block = root.SelectNodes(".//block[@name='trigger volumes']");
        XmlNodeList vehi_palette_block = root.SelectNodes(".//block[@name='vehicle palette']");
        XmlNodeList vehi_entries_block = root.SelectNodes(".//block[@name='vehicles']");
        XmlNodeList crate_palette_block = root.SelectNodes(".//block[@name='crate palette']");
        XmlNodeList crate_entries_block = root.SelectNodes(".//block[@name='crates']");
        XmlNodeList object_names_block = root.SelectNodes(".//block[@name='object names']");
        XmlNodeList netgame_flags_block = root.SelectNodes(".//block[@name='netgame flags']");
        XmlNodeList decal_palette_block = root.SelectNodes(".//block[@name='decal palette']");
        XmlNodeList decal_entries_block = root.SelectNodes(".//block[@name='decals']");

        List<StartLoc> all_starting_locs = new List<StartLoc>();
        List<NetEquip> all_netgame_equip_locs = new List<NetEquip>();
        List<SpWeapLoc> all_sp_weapon_locs = new List<SpWeapLoc>();
        List<TagPath> all_scen_types = new List<TagPath>();
        List<Scenery> all_scen_entries = new List<Scenery>();
        List<TrigVol> all_trig_vols = new List<TrigVol>();
        List<TagPath> all_vehi_types = new List<TagPath>();
        List<Vehicle> all_vehi_entries = new List<Vehicle>();
        List<TagPath> all_crate_types = new List<TagPath>();
        List<Crate> all_crate_entries = new List<Crate>();
        List<string> all_object_names = new List<string>();
        List<NetFlag> all_netgame_flags = new List<NetFlag>();
        List<Decal> all_dec_entries = new List<Decal>();
        List<TagPath> all_dec_types = new List<TagPath>();

        foreach (XmlNode name in object_names_block)
        {
            bool names_end = false;
            int i = 0;
            while (!names_end)
            {
                XmlNode element = name.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    all_object_names.Add(element.SelectSingleNode("./field[@name='name']").InnerText.Trim());
                    i++;
                }
                else
                {
                    names_end = true;
                    Console.WriteLine("Finished processing object name data.");
                    loadingForm.UpdateOutputBox("Finished processing object name data.", false);
                }
            }
        }

        foreach (XmlNode name in netgame_flags_block)
        {
            bool names_end = false;
            int i = 0;
            while (!names_end)
            {
                XmlNode element = name.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    all_object_names.Add(element.Attributes["name"].Value);
                    i++;
                }
                else
                {
                    names_end = true;
                    Console.WriteLine("Finished processing netgame flag name data.");
                    loadingForm.UpdateOutputBox("Finished processing netgame flag name data.", false);
                }
            }
        }

        foreach (XmlNode location in player_start_loc_block)
        {
            bool locs_end = false;
            int i = 0;
            while (!locs_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = location.SelectSingleNode(search_string);
                if (element != null)
                {
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string facing = element.SelectSingleNode("./field[@name='facing']").InnerText.Trim();
                    string team = element.SelectSingleNode("./field[@name='team designator']").InnerText.Trim();
                    string type0 = element.SelectSingleNode("./field[@name='type 0']").InnerText.Trim();
                    string type1 = element.SelectSingleNode("./field[@name='type 1']").InnerText.Trim();
                    string type2 = element.SelectSingleNode("./field[@name='type 2']").InnerText.Trim();
                    string type3 = element.SelectSingleNode("./field[@name='type 3']").InnerText.Trim();
                    string spawn_type0 = element.SelectSingleNode("./field[@name='spawn type 0']").InnerText.Trim();
                    string spawn_type1 = element.SelectSingleNode("./field[@name='spawn type 1']").InnerText.Trim();
                    string spawn_type2 = element.SelectSingleNode("./field[@name='spawn type 2']").InnerText.Trim();
                    string spawn_type3 = element.SelectSingleNode("./field[@name='spawn type 3']").InnerText.Trim();

                    all_starting_locs.Add(new StartLoc
                    {
                        position_xyz = xyz,
                        facing_angle = facing,
                        team_enum = team,
                        type_0 = type0,
                        type_1 = type1,
                        type_2 = type2,
                        type_3 = type3,
                        spawn_type_0 = spawn_type0,
                        spawn_type_1 = spawn_type1,
                        spawn_type_2 = spawn_type2,
                        spawn_type_3 = spawn_type3,
                    });

                    Console.WriteLine("Processed starting position " + i);
                    loadingForm.UpdateOutputBox("Processed starting position " + i, false);
                    i++;
                }
                else
                {
                    locs_end = true;
                    Console.WriteLine("\nFinished processing starting positions data.");
                    loadingForm.UpdateOutputBox("\nFinished processing starting positions data.", false);
                }
            }
        }

        // Have to handle weapons/vehicles/equipment differently for solo vs mult
        if (scenario_type == "1,multiplayer")
        {
            foreach (XmlNode netgame_entry in netgame_objs_entries_block)
            {
                bool netgame_end = false;
                int i = 0;
                while (!netgame_end)
                {
                    string search_string = "./element[@index='" + i + "']";
                    XmlNode element = netgame_entry.SelectSingleNode(search_string);
                    if (element != null)
                    {
                        NetEquip netgame_equip = new NetEquip();
                        netgame_equip.position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                        netgame_equip.rotation = element.SelectSingleNode("./field[@name='orientation']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                        netgame_equip.spawn_time = element.SelectSingleNode("./field[@name='spawn time (in seconds, 0 = default)']").InnerText.Trim();
                        netgame_equip.collection_type = element.SelectSingleNode("./tag_reference[@name='item/vehicle collection']").InnerText.Trim();

                        all_netgame_equip_locs.Add(netgame_equip);
                        Console.WriteLine("Process netgame equipment " + i);
                        loadingForm.UpdateOutputBox("Process netgame equipment " + i, false);
                        i++;
                    }
                    else
                    {
                        netgame_end = true;
                        Console.WriteLine("\nFinished processing netgame equipment data.");
                        loadingForm.UpdateOutputBox("\nFinished processing netgame equipment data.", false);
                    }
                }
            }
        }
        else if (scenario_type == "0,solo")
        {
            // Before we can do anything, gotta transfer the weapon palette data so the indices line up
            Utils.ConvertPalette(scen_path, xml_path, loadingForm, scenfile, "weapon");
            loadingForm.UpdateOutputBox("\nBegin reading weapon placement data.", false);

            foreach (XmlNode weapon_entry in weapon_sp_entries_block)
            {
                bool weaps_end = false;
                int i = 0;
                while (!weaps_end)
                {
                    string search_string = "./element[@index='" + i + "']";
                    XmlNode element = weapon_entry.SelectSingleNode(search_string);

                    if (element != null)
                    {
                        SpWeapLoc weapon = Utils.GetObjectDataFromXML<SpWeapLoc>(element);
                        all_sp_weapon_locs.Add(weapon);
                        loadingForm.UpdateOutputBox($"Processed weapon placement {i}.", false);
                        i++;
                    }
                    else
                    {
                        weaps_end = true;
                        Console.WriteLine("\nFinished processing weapon data.");
                        loadingForm.UpdateOutputBox("\nFinished processing weapon placement data.", false);
                    }
                }
            }

            // SP vehicles - MP vehicles from H2 don't actually use the vehicle palette

            // Transfer the vehicle palette data so the indices line up
            Utils.ConvertPalette(scen_path, xml_path, loadingForm, scenfile, "vehicle");
            foreach (XmlNode vehicle_entry in vehi_entries_block)
            {
                bool vehi_end = false;
                int i = 0;
                while (!vehi_end)
                {
                    string search_string = "./element[@index='" + i + "']";
                    XmlNode element = vehicle_entry.SelectSingleNode(search_string);
                    if (element != null)
                    {
                        Vehicle vehicle = Utils.GetObjectDataFromXML<Vehicle>(element);
                        all_vehi_entries.Add(vehicle);
                        i++;
                    }
                    else
                    {
                        vehi_end = true;
                        Console.WriteLine("Finished processing vehicle placement data.");
                        loadingForm.UpdateOutputBox("Finished processing vehicle placement data.", false);
                    }
                }
            }
        }
        

        foreach (XmlNode entry in scen_palette_block)
        {
            bool scen_end = false;
            int i = 0;
            while (!scen_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = entry.SelectSingleNode(search_string);
                if (element != null)
                {
                    string scen_type = element.SelectSingleNode("./tag_reference[@name='name']").InnerText.Trim();
                    all_scen_types.Add(TagPath.FromPathAndType(scen_type, "scen*"));
                    i++;
                }
                else
                {
                    scen_end = true;
                    Console.WriteLine("Finished processing scenery palette data.");
                    loadingForm.UpdateOutputBox("Finished processing scenery palette data.", false);
                }
            }
        }

        foreach (XmlNode scenery_entry in scen_entries_block)
        {
            bool scen_end = false;
            int i = 0;
            while (!scen_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = scenery_entry.SelectSingleNode(search_string);
                if (element != null)
                {
                    Scenery scenery = Utils.GetObjectDataFromXML<Scenery>(element);
                    all_scen_entries.Add(scenery);
                    i++;
                }
                else
                {
                    scen_end = true;
                    Console.WriteLine("Finished processing scenery placement data.");
                    loadingForm.UpdateOutputBox("Finished processing scenery placement data.", false);
                }
            }
        }

        foreach (XmlNode volume in trig_vol_block)
        {
            bool vols_end = false;
            int i = 0;
            while (!vols_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = volume.SelectSingleNode(search_string);
                if (element != null)
                {
                    string name = element.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string ext = element.SelectSingleNode("./field[@name='extents']").InnerText.Trim();
                    string fwd = element.SelectSingleNode("./field[@name='forward']").InnerText.Trim();
                    string up = element.SelectSingleNode("./field[@name='up']").InnerText.Trim();

                    all_trig_vols.Add(new TrigVol
                    {
                        vol_name = name,
                        vol_xyz = xyz,
                        vol_ext = ext,
                        vol_fwd = fwd,
                        vol_up = up
                    });

                    i++;
                }
                else
                {
                    vols_end = true;
                    Console.WriteLine("Finished processing trigger volume data.");
                    loadingForm.UpdateOutputBox("Finished processing trigger volume data.", false);
                }
            }
        }

        foreach (XmlNode crate in crate_palette_block)
        {
            bool crates_end = false;
            int i = 0;
            while (!crates_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = crate.SelectSingleNode(search_string);
                if (element != null)
                {
                    string vehi_type = element.SelectSingleNode("./tag_reference[@name='name']").InnerText.Trim();
                    all_crate_types.Add(TagPath.FromPathAndType(vehi_type, "bloc*"));
                    i++;
                }
                else
                {
                    crates_end = true;
                    Console.WriteLine("Finished processing crate palette data.");
                    loadingForm.UpdateOutputBox("Finished processing crate palette data.", false);
                }
            }
        }

        foreach (XmlNode crate_entry in crate_entries_block)
        {
            bool crates_end = false;
            int i = 0;
            while (!crates_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = crate_entry.SelectSingleNode(search_string);
                if (element != null)
                {
                    Crate crate = Utils.GetObjectDataFromXML<Crate>(element);
                    all_crate_entries.Add(crate);
                    i++;
                }
                else
                {
                    crates_end = true;
                    Console.WriteLine("Finished processing crate placement data.");
                    loadingForm.UpdateOutputBox("Finished processing crate placement data.", false);
                }
            }
        }

        foreach (XmlNode netflag in netgame_flags_block)
        {
            bool netflags_end = false;
            int i = 0;
            while (!netflags_end)
            {
                XmlNode element = netflag.SelectSingleNode("./element[@index='" + i + "']");
                if (element != null)
                {
                    string name = element.Attributes["name"].Value;
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string orient = element.SelectSingleNode("./field[@name='facing']").InnerText.Trim();
                    string type = element.SelectSingleNode("./field[@name='type']").InnerText.Trim();
                    string team = element.SelectSingleNode("./field[@name='team designator']").InnerText.Trim();

                    all_netgame_flags.Add(new NetFlag
                    {
                        netflag_name = name,
                        netflag_xyz = xyz,
                        netflag_orient = orient,
                        netflag_type = type,
                        netflag_team = team
                    });

                    i++;
                }
                else
                {
                    netflags_end = true;
                    Console.WriteLine("Finished processing netgame flags data.");
                    loadingForm.UpdateOutputBox("Finished processing netgame flags data.", false);
                }
            }
        }

        foreach (XmlNode entry in decal_palette_block)
        {
            bool decs_end = false;
            int i = 0;
            while (!decs_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = entry.SelectSingleNode(search_string);
                if (element != null)
                {
                    string dec_type = element.SelectSingleNode("./tag_reference[@name='reference']").InnerText.Trim();
                    all_dec_types.Add(TagPath.FromPathAndType(dec_type, "decs*"));
                    i++;
                }
                else
                {
                    decs_end = true;
                    Console.WriteLine("Finished processing decal palette data.");
                    loadingForm.UpdateOutputBox("Finished processing decal palette data.", false);
                }
            }
        }

        foreach (XmlNode decal in decal_entries_block)
        {
            bool decs_end = false;
            int i = 0;
            while (!decs_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = decal.SelectSingleNode(search_string);
                if (element != null)
                {
                    string type = element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.ToString();
                    string yaw = element.SelectSingleNode("./field[@name='yaw[-127,127]']").InnerText.Trim();
                    string pitch = element.SelectSingleNode("./field[@name='pitch[-127,127]']").InnerText.Trim();
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();

                    all_dec_entries.Add(new Decal
                    {
                        decal_type = type,
                        decal_xyz = xyz,
                        decal_pitch = pitch,
                        decal_yaw = yaw
                    });

                    i++;
                }
                else
                {
                    decs_end = true;
                    Console.WriteLine("Finished processing decal placement data.");
                    loadingForm.UpdateOutputBox("Finished processing decal placement data.", false);
                }
            }
        }

        ManagedBlamHandler(all_object_names, all_starting_locs, all_netgame_equip_locs, all_sp_weapon_locs, all_scen_types, all_scen_entries, all_trig_vols, all_vehi_entries, all_crate_types, all_crate_entries, all_netgame_flags, all_dec_types, all_dec_entries, h3ek_path, scen_path, loadingForm, scenario_type);
    }

    static void ManagedBlamHandler(List<string> all_object_names, List<StartLoc> spawn_data, List<NetEquip> netgame_equip_data, List<SpWeapLoc> all_sp_weap_locs, List<TagPath> all_scen_types, List<Scenery> all_scen_entries, List<TrigVol> all_trig_vols, List<Vehicle> all_vehi_entries, List<TagPath> all_crate_types, List<Crate> all_crate_entries, List<NetFlag> all_netgame_flags, List<TagPath> all_dec_types, List<Decal> all_dec_entries, string h3ek_path, string scen_path, Loading loadingForm, string scenario_type)
    {
        var tag_path = TagPath.FromPathAndType(Path.ChangeExtension(scen_path.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
        var respawn_scen_path = TagPath.FromPathAndType(@"objects\multi\spawning\respawn_point", "scen*");
        int respawn_scen_index = 0;

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);

        using (var tagFile = new TagFile(tag_path))
        {
            // Object names section
            ((TagFieldBlock)tagFile.SelectField($"Block:object names")).RemoveAllElements();
            int nameIndex = 0;
            foreach (string name in all_object_names)
            {
                // Add new
                ((TagFieldBlock)tagFile.SelectField($"Block:object names")).AddElement();
                ((TagFieldElementString)tagFile.SelectField($"Block:object names[{nameIndex}]/String:name")).Data = name;

                nameIndex++;
            }

            if (scenario_type == "1,multiplayer")
            {
                // Spawns Section
                int i = 0;
                int temp_index = 0;
                bool respawn_found = false;
                int totalScenCount = 0;
                int totalVehiCount = 0;

                ((TagFieldBlock)tagFile.Fields[21]).RemoveAllElements(); // Remove all scenery from palette
                ((TagFieldBlock)tagFile.Fields[20]).RemoveAllElements(); // Remove all scenery

                // Add respawn point scenery, if it doesn't already exist
                if (((TagFieldBlock)tagFile.Fields[21]).Elements.Count() != 0)
                {
                    foreach (var scen_type in ((TagFieldBlock)tagFile.Fields[21]).Elements)
                    {
                        if (((TagFieldReference)scen_type.Fields[0]).Path == respawn_scen_path)
                        {
                            // Respawn point scenery already in palette, set index
                            respawn_scen_index = temp_index;
                            respawn_found = true;
                            break;
                        }
                        temp_index++;
                    }
                    if (respawn_found == false)
                    {
                        // Respawn point is not in the palette, add it and set the index
                        respawn_scen_index = ((TagFieldBlock)tagFile.Fields[21]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                        var scen_tag = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[respawn_scen_index].Fields[0];
                        scen_tag.Path = respawn_scen_path;
                        totalScenCount++;
                    }
                }
                else
                {
                    Console.WriteLine("\nNo existing sceneries, adding respawn point\n");
                    loadingForm.UpdateOutputBox("\nNo existing sceneries, adding respawn point\n", false);
                    ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                    var scen_tag = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[0].Fields[0];
                    scen_tag.Path = respawn_scen_path;
                }



                foreach (var spawn in spawn_data)
                {
                    // Add new
                    ((TagFieldBlock)tagFile.Fields[20]).AddElement();

                    // Type
                    var scen_type = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[1];
                    scen_type.Value = respawn_scen_index;

                    // Dropdown type and source (won't be valid without these)
                    var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                    var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                    dropdown_type.Value = 6; // 6 for scenery
                    dropdown_source.Value = 1; // 1 for editor

                    // Position
                    var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[0].FieldName;
                    var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[2];
                    xyz_pos.Data = spawn.position_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Rotation
                    var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[4]).Elements[0].Fields[3];
                    string angle_xyz = spawn.facing_angle + ",0,0";
                    rotation.Data = angle_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Team
                    var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[7]).Elements[0].Fields[0].FieldName;
                    var team = (TagFieldEnum)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[i].Fields[7]).Elements[0].Fields[3];
                    team.Value = int.Parse(new string(spawn.team_enum.TakeWhile(c => c != ',').ToArray()));


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

                foreach (NetEquip netgame_equip in netgame_equip_data)
                {
                    string eqip_type = netgame_equip.collection_type.Split('\\')[netgame_equip.collection_type.Split('\\').Length - 1];

                    if (eqip_type == "frag_grenades" || eqip_type == "plasma_grenades")
                    {
                        // Grenade stuff, need to treat as equipment not weapon
                        Console.WriteLine("Adding " + eqip_type + " netgame equipment");
                        loadingForm.UpdateOutputBox("Adding " + eqip_type + " netgame equipment", false);

                        // Equipment, check if palette entry exists first
                        bool equip_entry_exists = false;
                        foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[27]).Elements)
                        {
                            var temp_type = Utils.mpWeapMapping[eqip_type];
                            if (((TagFieldReference)palette_entry.Fields[0]).Path == temp_type)
                            {
                                equip_entry_exists = true;
                            }
                        }

                        // Add palette entry if needed
                        if (!equip_entry_exists)
                        {
                            int current_count = ((TagFieldBlock)tagFile.Fields[27]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[27]).AddElement();
                            var equip_tag_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[27]).Elements[current_count].Fields[0];
                            equip_tag_ref.Path = Utils.mpWeapMapping[eqip_type];
                            weapPaletteMapping.Add(eqip_type, current_count);
                        }

                        // Now add the equipment itself
                        int equip_count = ((TagFieldBlock)tagFile.Fields[26]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[26]).AddElement(); // Add new equipment entry

                        // XYZ
                        var equip_xyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[2];
                        equip_xyz.Data = netgame_equip.position;

                        // Rotation
                        var equip_orient = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[3];
                        equip_orient.Data = netgame_equip.rotation;

                        // Type
                        var equip_tag = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[1];
                        equip_tag.Value = weapPaletteMapping[eqip_type];

                        // Spawn timer
                        var equip_stime = (TagFieldElementInteger)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[6]).Elements[0].Fields[8];
                        equip_stime.Data = uint.Parse(netgame_equip.spawn_time);

                        // Dropdown type and source (won't be valid without these)
                        var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdown_type.Value = 3; // 3 for equipment
                        dropdown_source.Value = 1; // 1 for editor

                        continue;
                    }
                    else if (eqip_type == "")
                    {
                        // All games entries, ignore
                        Console.WriteLine("Ignoring blank weapon collection");
                        loadingForm.UpdateOutputBox("Ignoring blank weapon collection", false);
                        continue;
                    }
                    else if (eqip_type.Contains("ammo"))
                    {
                        // Ammo placement, ignore for now
                    }
                    else if (eqip_type.Contains("powerup"))
                    {
                        // Powerup, add equipment, check if palette entry exists first
                        bool equip_entry_exists = false;
                        foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[27]).Elements)
                        {
                            var temp_type = Utils.mpWeapMapping["powerup"];
                            if (((TagFieldReference)palette_entry.Fields[0]).Path == temp_type)
                            {
                                equip_entry_exists = true;
                            }
                        }

                        // Add palette entry if needed
                        if (!equip_entry_exists)
                        {
                            int current_count = ((TagFieldBlock)tagFile.Fields[27]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[27]).AddElement();
                            var equip_tag_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[27]).Elements[current_count].Fields[0];
                            equip_tag_ref.Path = Utils.mpWeapMapping["powerup"];
                            weapPaletteMapping.Add("powerup", current_count);
                        }

                        // Now add the equipment itself
                        int equip_count = ((TagFieldBlock)tagFile.Fields[26]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[26]).AddElement(); // Add new equipment entry

                        // XYZ
                        var equip_xyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[2];
                        equip_xyz.Data = netgame_equip.position;

                        // Rotation
                        var equip_orient = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[3];
                        equip_orient.Data = netgame_equip.rotation;

                        // Type
                        var equip_tag = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[1];
                        equip_tag.Value = weapPaletteMapping["powerup"];

                        // Dropdown type and source (won't be valid without these)
                        var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdown_type.Value = 3; // 3 for equipment
                        dropdown_source.Value = 1; // 1 for editor
                    }
                    else if (netgame_equip.collection_type.Contains("vehicles"))
                    {
                        // Check if current type exists in palette
                        bool type_exists_already = false;
                        foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[25]).Elements)
                        {
                            var x = ((TagFieldReference)palette_entry.Fields[0]).Path;
                            if (x == Utils.netVehiMapping[eqip_type])
                            {
                                type_exists_already = true;
                                break;
                            }
                        }

                        // Add palette entry if needed
                        int current_count = ((TagFieldBlock)tagFile.Fields[25]).Elements.Count();
                        if (!type_exists_already)
                        {
                            ((TagFieldBlock)tagFile.Fields[25]).AddElement();
                            var vehi_type_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[25]).Elements[current_count].Fields[0];
                            vehi_type_ref.Path = Utils.netVehiMapping[eqip_type];
                            totalVehiCount++;
                        }

                        int vehi_count = ((TagFieldBlock)tagFile.Fields[24]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[24]).AddElement();
                        var type_ref = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[24]).Elements[vehi_count].Fields[1];
                        type_ref.Value = current_count;

                        // Dropdown type and source (won't be valid without these)
                        var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehi_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehi_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdown_type.Value = 1; // 1 for vehicle
                        dropdown_source.Value = 1; // 1 for editor

                        // Position
                        var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehi_count].Fields[4]).Elements[0].Fields[0].FieldName;
                        var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehi_count].Fields[4]).Elements[0].Fields[2];
                        xyz_pos.Data = netgame_equip.position;

                        // Rotation
                        var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehi_count].Fields[4]).Elements[0].Fields[3];
                        rotation.Data = netgame_equip.rotation;
                    }
                    else
                    {
                        // Weapon, check if palette entry exists first
                        Console.WriteLine("Adding " + eqip_type + " weapon");
                        loadingForm.UpdateOutputBox("Adding " + eqip_type + " weapon", false);
                        bool weap_entry_exists = false;
                        foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[29]).Elements)
                        {
                            var temp_type = Utils.mpWeapMapping[eqip_type];
                            if (((TagFieldReference)palette_entry.Fields[0]).Path == temp_type)
                            {
                                weap_entry_exists = true;
                            }
                        }

                        // Add palette entry if needed
                        if (!weap_entry_exists)
                        {
                            int current_count = ((TagFieldBlock)tagFile.Fields[29]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[29]).AddElement();
                            var weap_tag_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[29]).Elements[current_count].Fields[0];
                            weap_tag_ref.Path = Utils.mpWeapMapping[eqip_type];
                            weapPaletteMapping.Add(eqip_type, current_count);
                        }

                        // Now add the weapon itself
                        int weapon_count = ((TagFieldBlock)tagFile.Fields[28]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[28]).AddElement(); // Add new weapon entry

                        // XYZ
                        var weap_xyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[2];
                        weap_xyz.Data = netgame_equip.position;

                        // Rotation
                        var weap_orient = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[3];
                        weap_orient.Data = netgame_equip.rotation;

                        // Type
                        var weap_tag = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[1];
                        weap_tag.Value = weapPaletteMapping[eqip_type];

                        // Spawn timer
                        var weap_stime = (TagFieldElementInteger)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[7]).Elements[0].Fields[8];
                        weap_stime.Data = uint.Parse(netgame_equip.spawn_time);

                        // Dropdown type and source (won't be valid without these)
                        var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdown_type.Value = 2; // 2 for weapon
                        dropdown_source.Value = 1; // 1 for editor
                    }

                    Console.WriteLine("Done netgame equipment");
                    loadingForm.UpdateOutputBox("Done netgame equipment", false);

                    // Scenery Section - the idea is to place blank scenery with bad references so they can be easily changed to ported versions by the user

                    foreach (TagPath scen_type in all_scen_types)
                    {
                        // Check if current type exists in palette
                        bool type_exists_already = false;
                        foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[21]).Elements)
                        {
                            var x = ((TagFieldReference)palette_entry.Fields[0]).Path;
                            if (x == scen_type)
                            {
                                type_exists_already = true;
                                break;
                            }
                        }

                        // Add palette entry if needed
                        if (!type_exists_already)
                        {
                            int current_count = ((TagFieldBlock)tagFile.Fields[21]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[21]).AddElement();
                            var scen_type_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[21]).Elements[current_count].Fields[0];
                            scen_type_ref.Path = scen_type;
                        }
                    }

                    // Now add all of the scenery placements
                    foreach (Scenery scenery in all_scen_entries)
                    {
                        int current_count = ((TagFieldBlock)tagFile.Fields[20]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[20]).AddElement();
                        var type_ref = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[1];
                        int index = scenery.type_index + totalScenCount;
                        type_ref.Value = scenery.type_index + totalScenCount;

                        // Dropdown type and source (won't be valid without these)
                        var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdown_type.Value = 6; // 6 for scenery
                        dropdown_source.Value = 1; // 1 for editor

                        // Position
                        var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[0].FieldName;
                        var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[2];
                        xyz_pos.Data = scenery.position;

                        // Rotation
                        var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[3];
                        rotation.Data = scenery.rotation;

                        ((TagFieldBlockFlags)tagFile.SelectField($"Block:scenery[{current_count}]/Struct:object data/WordBlockFlags:manual bsp flags")).Value = scenery.manual_bsp;
                        ((TagFieldBlockIndex)tagFile.SelectField($"Block:scenery[{current_count}]/Struct:object data/Struct:object id/ShortBlockIndex:origin bsp index")).Value = scenery.origin_bsp;
                        ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{current_count}]/Struct:object data/CharEnum:bsp policy")).Value = scenery.bsp_policy;

                        // Variant
                        var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[5]).Elements[0].Fields[0].FieldName;
                        var variant = (TagFieldElementStringID)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[5]).Elements[0].Fields[0];
                        variant.Data = scenery.var_name;
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
                        int current_count = ((TagFieldBlock)tagFile.Fields[125]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[125]).AddElement();
                        // Name
                        var name = (TagFieldElementLongString)((TagFieldBlock)tagFile.Fields[125]).Elements[current_count].Fields[1];
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

                    foreach (TagPath crate_type in all_crate_types)
                    {
                        // Check if current type exists in palette
                        bool type_exists_already = false;
                        foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[119]).Elements)
                        {
                            var x = ((TagFieldReference)palette_entry.Fields[0]).Path;
                            if (x == crate_type)
                            {
                                type_exists_already = true;
                                break;
                            }
                        }

                        // Add palette entry if needed
                        if (!type_exists_already)
                        {
                            int current_count = ((TagFieldBlock)tagFile.Fields[119]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[119]).AddElement();
                            var crate_type_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[119]).Elements[current_count].Fields[0];
                            crate_type_ref.Path = crate_type;
                        }
                    }

                    foreach (Crate crate in all_crate_entries)
                    {
                        int current_count = ((TagFieldBlock)tagFile.Fields[118]).Elements.Count();
                        ((TagFieldBlock)tagFile.Fields[118]).AddElement();
                        var type_ref = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[1];
                        type_ref.Value = crate.type_index;

                        // Name
                        var name_field = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[3];
                        name_field.Value = crate.name_index;

                        // Dropdown type and source (won't be valid without these)
                        var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdown_type.Value = 10; // 1 for crate
                        dropdown_source.Value = 1; // 1 for editor

                        // Position
                        var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[0].FieldName;
                        var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[2];
                        xyz_pos.Data = crate.position;

                        // Rotation
                        var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[3];
                        rotation.Data = crate.rotation;

                        ((TagFieldBlockFlags)tagFile.SelectField($"Block:crates[{current_count}]/Struct:object data/WordBlockFlags:manual bsp flags")).Value = crate.manual_bsp;
                        ((TagFieldBlockIndex)tagFile.SelectField($"Block:crates[{current_count}]/Struct:object data/Struct:object id/ShortBlockIndex:origin bsp index")).Value = crate.origin_bsp;
                        ((TagFieldEnum)tagFile.SelectField($"Block:crates[{current_count}]/Struct:object data/CharEnum:bsp policy")).Value = crate.bsp_policy;

                        // Variant
                        var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[5]).Elements[0].Fields[0].FieldName;
                        var variant = (TagFieldElementStringID)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[5]).Elements[0].Fields[0];
                        variant.Data = crate.var_name;
                    }

                    Dictionary<string, int> existing_gametype_crates = new Dictionary<string, int>();

                    // Netgame flags to gametype crates section
                    foreach (NetFlag netflag in all_netgame_flags)
                    {
                        int type_index = 0;
                        string temp = Regex.Replace(netflag.netflag_type, @"^.*?,\s*", "");
                        string name_stripped = Regex.Replace(temp, @"\d+$", "").Trim();
                        if (!existing_gametype_crates.ContainsKey(name_stripped))
                        {
                            // Add type to crate palette
                            type_index = ((TagFieldBlock)tagFile.Fields[119]).Elements.Count();
                            ((TagFieldBlock)tagFile.Fields[119]).AddElement();
                            var crate_type_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[119]).Elements[type_index].Fields[0];
                            crate_type_ref.Path = Utils.netflagMapping[netflag.netflag_type];
                            existing_gametype_crates.Add(name_stripped, type_index);
                        }
                        else
                        {
                            type_index = existing_gametype_crates[name_stripped];
                        }

                        int current_count = ((TagFieldBlock)tagFile.Fields[118]).Elements.Count(); // Get current crate count
                        ((TagFieldBlock)tagFile.Fields[118]).AddElement();
                        var type_ref = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[1];
                        type_ref.Value = existing_gametype_crates[name_stripped];

                        // Name
                        var name_field = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[3];
                        name_field.Value = all_object_names.IndexOf(netflag.netflag_name);

                        // Dropdown type and source (won't be valid without these)
                        var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                        var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                        dropdown_type.Value = 10; // 1 for crate
                        dropdown_source.Value = 1; // 1 for editor

                        // Position
                        var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[0].FieldName;
                        var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[2];
                        xyz_pos.Data = netflag.netflag_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                        // Rotation
                        var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[3];
                        string angle_xyz = netflag.netflag_orient + ",0,0";
                        rotation.Data = angle_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                        // Team
                        var team = (TagFieldEnum)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[7]).Elements[0].Fields[3];
                        team.Value = int.Parse(new string(netflag.netflag_team.TakeWhile(c => c != ',').ToArray()));

                        // Grab editor folder
                        var folder = (TagFieldBlockIndex)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[11];

                        // Choose folder based on type
                        if (name_stripped.ToLower().Contains("oddball"))
                        {
                            folder.Value = 0;
                        }
                        else if (name_stripped.ToLower().Contains("ctf"))
                        {
                            folder.Value = 1;
                        }
                        else if (name_stripped.ToLower().Contains("hill"))
                        {
                            folder.Value = 2;
                        }
                        else if (name_stripped.ToLower().Contains("assault"))
                        {
                            folder.Value = 3;
                        }
                        else if (name_stripped.ToLower().Contains("territories"))
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
            else if (scenario_type == "0,solo")
            {
                // SP weapons
                Utils.WriteObjectData(tagFile, all_sp_weap_locs, "weapons", loadingForm);

                // SP scenery section

                // Scenery palette
                ((TagFieldBlock)tagFile.SelectField($"Block:scenery palette")).RemoveAllElements();
                int x = 0;

                foreach (TagPath scenery_type in all_scen_types)
                {
                    ((TagFieldBlock)tagFile.SelectField($"Block:scenery palette")).AddElement();
                    ((TagFieldReference)tagFile.SelectField($"Block:scenery palette[{x}]/Reference:name")).Path = scenery_type;
                    x++;
                }

                Utils.WriteObjectData(tagFile, all_scen_entries, "scenery", loadingForm);

                // SP crate section

                // Crate palette
                ((TagFieldBlock)tagFile.SelectField($"Block:crate palette")).RemoveAllElements();
                x = 0;

                foreach (TagPath crate_type in all_crate_types)
                {
                    ((TagFieldBlock)tagFile.SelectField($"Block:crate palette")).AddElement();
                    ((TagFieldReference)tagFile.SelectField($"Block:crate palette[{x}]/Reference:name")).Path = crate_type;
                    x++;
                }

                Utils.WriteObjectData(tagFile, all_crate_entries, "crates", loadingForm);

                // Vehicle section
                Utils.WriteObjectData(tagFile, all_vehi_entries, "vehicles", loadingForm);
            }

            // Trigger volumes section
            ((TagFieldBlock)tagFile.SelectField($"Block:trigger volumes")).RemoveAllElements();
            foreach (TrigVol vol in all_trig_vols)
            {
                int current_count = ((TagFieldBlock)tagFile.Fields[55]).Elements.Count();
                ((TagFieldBlock)tagFile.Fields[55]).AddElement();

                // Name
                var name = (TagFieldElementStringID)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[0];
                name.Data = vol.vol_name;

                // Forward
                var fwd = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[4];
                fwd.Data = vol.vol_fwd.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Up
                var up = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[5];
                up.Data = vol.vol_up.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Position
                var xyz = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[6];
                xyz.Data = vol.vol_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Extents
                var ext = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[55]).Elements[current_count].Fields[7];
                ext.Data = vol.vol_ext.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();
            }

            Console.WriteLine("Done trigger volumes");
            loadingForm.UpdateOutputBox("Done trigger volumes", false);

            // Decals section
            ((TagFieldBlock)tagFile.Fields[78]).RemoveAllElements(); // Remove all decals from palette
            ((TagFieldBlock)tagFile.Fields[77]).RemoveAllElements(); // Remove all decals
            foreach (TagPath dec_type in all_dec_types)
            {
                // Check if current type exists in palette
                bool type_exists_already = false;
                foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[78]).Elements)
                {
                    var x = ((TagFieldReference)palette_entry.Fields[0]).Path;
                    if (x == dec_type)
                    {
                        type_exists_already = true;
                        break;
                    }
                }

                // Add palette entry if needed
                if (!type_exists_already)
                {
                    int current_count = ((TagFieldBlock)tagFile.Fields[78]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[78]).AddElement();
                    var scen_type_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[78]).Elements[current_count].Fields[0];
                    scen_type_ref.Path = dec_type;
                }
            }

            foreach (Decal decal in all_dec_entries)
            {
                int current_count = ((TagFieldBlock)tagFile.Fields[77]).Elements.Count();
                ((TagFieldBlock)tagFile.Fields[77]).AddElement();
                var type_ref = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[77]).Elements[current_count].Fields[1];
                type_ref.Value = int.Parse(decal.decal_type);

                // Position
                var xyz_pos = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[77]).Elements[current_count].Fields[4];
                xyz_pos.Data = decal.decal_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Rotation stuff below - only god fucking knows what this is doing, and either way it doesnt work properly
                double pitchDegrees = double.Parse(decal.decal_pitch);
                double yawDegrees = double.Parse(decal.decal_yaw);

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
                var rotation = (TagFieldElementArraySingle)((TagFieldBlock)tagFile.Fields[77]).Elements[current_count].Fields[3];
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