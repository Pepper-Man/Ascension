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

class WeapLoc
{
    public string weap_xyz { get; set; }
    public string weap_orient { get; set; }
    public string spawn_time { get; set; }
    public string weap_type { get; set; }
}

class Scenery
{
    public string scen_type { get; set; }
    public string scen_xyz { get; set; }
    public string scen_orient { get; set; }
    public string scen_vrnt { get; set; }
}

class TrigVol
{
    public string vol_name { get; set; }
    public string vol_xyz { get; set; }
    public string vol_ext { get; set; }
    public string vol_fwd { get; set; }
    public string vol_up { get; set; }
}

class Vehicle
{
    public string vehi_type { get; set; }
    public string vehi_xyz { get; set; }
    public string vehi_orient { get; set; }
    public string vehi_vrnt { get; set; }
}

class Crate
{
    public string crate_type { get; set; }
    public string crate_name { get; set; }
    public string crate_xyz { get; set; }
    public string crate_orient { get; set; }
    public string crate_vrnt { get; set; }
}

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
    public static void ScenarioConverter(string scen_path, string xml_path, Loading loadingForm)
    {
        string h3ek_path = scen_path.Substring(0, scen_path.IndexOf("H3EK") + "H3EK".Length);

        // Create scenario backup
        string backup_folderpath = Path.GetDirectoryName(scen_path) + @"\scenario_backup";
        Directory.CreateDirectory(backup_folderpath);
        string backup_filepath = Path.Combine(backup_folderpath, scen_path.Split('\\').Last());

        if (!File.Exists(backup_filepath))
        {
            File.Copy(scen_path, backup_filepath);
            Console.WriteLine("Backup created successfully.");
            loadingForm.UpdateOutputBox("Backup created successfully.", false);
        }
        else
        {
            Console.WriteLine("Backup already exists.");
            loadingForm.UpdateOutputBox("Backup already exists.", false);
        }

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
        Convert_XML(xml_path, h3ek_path, scen_path, loadingForm);
    }

    static void Convert_XML(string xml_path, string h3ek_path, string scen_path, Loading loadingForm)
    {
        Console.WriteLine("\nBeginning XML Conversion:\n");
        loadingForm.UpdateOutputBox("\nBeginning XML Conversion:\n", false);

        string newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "modified_input.xml");

        try
        {
            string[] lines = File.ReadAllLines(xml_path);
            bool removeLines = false;

            using (StreamWriter writer = new StreamWriter(newFilePath))
            {
                foreach (string line in lines)
                {
                    if (line.Contains("<block name=\"source files\">"))
                    {
                        removeLines = true;
                        writer.WriteLine(line);
                    }
                    else if (line.Contains("<block name=\"scripting data\">"))
                    {
                        removeLines = false;
                    }
                    else if (!removeLines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            Console.WriteLine("Modified file saved successfully.\n\nPreparing to patch tag data:\n");
            loadingForm.UpdateOutputBox("Modified file saved successfully.\n\nPreparing to patch tag data:\n", false);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            loadingForm.UpdateOutputBox("An error occurred: " + ex.Message, false);
        }

        xml_path = newFilePath;

        XmlDocument scenfile = new XmlDocument();
        scenfile.Load(xml_path);

        XmlNode root = scenfile.DocumentElement;

        XmlNodeList player_start_loc_block = root.SelectNodes(".//block[@name='player starting locations']");
        XmlNodeList weapon_placements_block = root.SelectNodes(".//block[@name='netgame equipment']");
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
        List<WeapLoc> all_weapon_locs = new List<WeapLoc>();
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

        foreach (XmlNode weapon in weapon_placements_block)
        {
            bool weaps_end = false;
            int i = 0;
            while (!weaps_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = weapon.SelectSingleNode(search_string);
                if (element != null)
                {
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string orient = element.SelectSingleNode("./field[@name='orientation']").InnerText.Trim();
                    string time = element.SelectSingleNode("./field[@name='spawn time (in seconds, 0 = default)']").InnerText.Trim();
                    string type = element.SelectSingleNode("./tag_reference[@name='item/vehicle collection']").InnerText.Trim();

                    all_weapon_locs.Add(new WeapLoc
                    {
                        weap_xyz = xyz,
                        weap_orient = orient,
                        spawn_time = time,
                        weap_type = type
                    });

                    Console.WriteLine("Process netgame equipment " + i);
                    loadingForm.UpdateOutputBox("Process netgame equipment " + i, false);
                    i++;
                }
                else
                {
                    weaps_end = true;
                    Console.WriteLine("\nFinished processing netgame equipment (weapon) data.");
                    loadingForm.UpdateOutputBox("\nFinished processing netgame equipment (weapon) data.", false);
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

        foreach (XmlNode scenery in scen_entries_block)
        {
            bool scen_end = false;
            int i = 0;
            while (!scen_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = scenery.SelectSingleNode(search_string);
                if (element != null)
                {
                    string type = element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.ToString();
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string orient = element.SelectSingleNode("./field[@name='rotation']").InnerText.Trim();
                    string variant = element.SelectSingleNode("./field[@name='variant name']").InnerText.Trim();

                    all_scen_entries.Add(new Scenery
                    {
                        scen_type = type,
                        scen_xyz = xyz,
                        scen_orient = orient,
                        scen_vrnt = variant
                    });

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

        foreach (XmlNode entry in vehi_palette_block)
        {
            bool vehi_end = false;
            int i = 0;
            while (!vehi_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = entry.SelectSingleNode(search_string);
                if (element != null)
                {
                    string vehi_type = element.SelectSingleNode("./tag_reference[@name='name']").InnerText.Trim();
                    all_vehi_types.Add(TagPath.FromPathAndType(vehi_type, "vehi*"));
                    i++;
                }
                else
                {
                    vehi_end = true;
                    Console.WriteLine("Finished processing vehicle palette data.");
                    loadingForm.UpdateOutputBox("Finished processing vehicle palette data.", false);
                }
            }
        }

        foreach (XmlNode vehicle in vehi_entries_block)
        {
            bool vehi_end = false;
            int i = 0;
            while (!vehi_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = vehicle.SelectSingleNode(search_string);
                if (element != null)
                {
                    string type = element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.ToString();
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string orient = element.SelectSingleNode("./field[@name='rotation']").InnerText.Trim();
                    string variant = element.SelectSingleNode("./field[@name='variant name']").InnerText.Trim();

                    all_vehi_entries.Add(new Vehicle
                    {
                        vehi_type = type,
                        vehi_xyz = xyz,
                        vehi_orient = orient,
                        vehi_vrnt = variant
                    });

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

        foreach (XmlNode crate in crate_entries_block)
        {
            bool crates_end = false;
            int i = 0;
            while (!crates_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = crate.SelectSingleNode(search_string);
                if (element != null)
                {
                    string type = element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.ToString();
                    string name = element.SelectSingleNode("./block_index[@type='name']").Attributes["index"].Value.ToString();
                    string xyz = element.SelectSingleNode("./field[@name='position']").InnerText.Trim();
                    string orient = element.SelectSingleNode("./field[@name='rotation']").InnerText.Trim();
                    string variant = element.SelectSingleNode("./field[@name='variant name']").InnerText.Trim();

                    all_crate_entries.Add(new Crate
                    {
                        crate_type = type,
                        crate_name = name,
                        crate_xyz = xyz,
                        crate_orient = orient,
                        crate_vrnt = variant
                    });

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

        ManagedBlamHandler(all_object_names, all_starting_locs, all_weapon_locs, all_scen_types, all_scen_entries, all_trig_vols, all_vehi_types, all_vehi_entries, all_crate_types, all_crate_entries, all_netgame_flags, all_dec_types, all_dec_entries, h3ek_path, scen_path, loadingForm);
    }

    static void ManagedBlamHandler(List<string> all_object_names, List<StartLoc> spawn_data, List<WeapLoc> weap_data, List<TagPath> all_scen_types, List<Scenery> all_scen_entries, List<TrigVol> all_trig_vols, List<TagPath> all_vehi_types, List<Vehicle> all_vehi_entries, List<TagPath> all_crate_types, List<Crate> all_crate_entries, List<NetFlag> all_netgame_flags, List<TagPath> all_dec_types, List<Decal> all_dec_entries, string h3ek_path, string scen_path, Loading loadingForm)
    {
        // Weapons dictionary
        Dictionary<string, TagPath> weapMapping = new Dictionary<string, TagPath>
        {
            {"frag_grenades", TagPath.FromPathAndType(@"objects\weapons\grenade\frag_grenade\frag_grenade", "eqip*")},
            {"plasma_grenades", TagPath.FromPathAndType(@"objects\weapons\grenade\plasma_grenade\plasma_grenade", "eqip*")},
            {"energy_sword", TagPath.FromPathAndType(@"objects\weapons\melee\energy_sword\energy_sword", "weap*")},
            {"magnum", TagPath.FromPathAndType(@"objects\weapons\pistol\magnum\magnum", "weap*")},
            {"needler", TagPath.FromPathAndType(@"objects\weapons\pistol\needler\needler", "weap*")},
            {"plasma_pistol", TagPath.FromPathAndType(@"objects\weapons\pistol\plasma_pistol\plasma_pistol", "weap*")},
            {"battle_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\battle_rifle\battle_rifle", "weap*")},
            {"beam_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\beam_rifle\beam_rifle", "weap*")},
            {"carbine", TagPath.FromPathAndType(@"objects\weapons\rifle\covenant_carbine\covenant_carbine", "weap*")},
            {"plasma_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\plasma_rifle\plasma_rifle", "weap*")},
            {"brute_plasma_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\plasma_rifle_red\plasma_rifle_red", "weap*")},
            {"shotgun", TagPath.FromPathAndType(@"objects\weapons\rifle\shotgun\shotgun", "weap*")},
            {"smg", TagPath.FromPathAndType(@"objects\weapons\rifle\smg\smg", "weap*")},
            {"smg_silenced", TagPath.FromPathAndType(@"objects\weapons\rifle\smg_silenced\smg_silenced", "weap*")},
            {"sniper_rifle", TagPath.FromPathAndType(@"objects\weapons\rifle\sniper_rifle\sniper_rifle", "weap*")},
            {"rocket_launcher", TagPath.FromPathAndType(@"objects\weapons\support_high\rocket_launcher\rocket_launcher", "weap*")},
            {"fuel_rod_gun", TagPath.FromPathAndType(@"objects\weapons\support_high\flak_cannon\flak_cannon", "weap*")},
            {"sentinel_beam", TagPath.FromPathAndType(@"objects\weapons\support_low\sentinel_gun\sentinel_gun", "weap*")},
            {"brute_shot", TagPath.FromPathAndType(@"objects\weapons\support_low\brute_shot\brute_shot", "weap*")},
            {"powerup", TagPath.FromPathAndType(@"objects\multi\powerups\powerup_red\powerup_red", "eqip*")}
        };

        // Netgame flag dictionary
        Dictionary<string, TagPath> netflagMapping = new Dictionary<string, TagPath>
        {
            {"0,CTF flag spawn", TagPath.FromPathAndType(@"objects\multi\ctf\ctf_flag_spawn_point", "bloc*")},
            {"1,CTF flag return", TagPath.FromPathAndType(@"objects\multi\ctf\ctf_flag_return_area", "bloc*")},
            {"2,Assault bomb spawn", TagPath.FromPathAndType(@"objects\multi\assault\assault_bomb_spawn_point", "bloc*")},
            {"3,Assault bomb return", TagPath.FromPathAndType(@"objects\multi\assault\assault_bomb_goal_area", "bloc*")},
            {"4,Oddball spawn", TagPath.FromPathAndType(@"objects\multi\oddball\oddball_ball_spawn_point", "bloc*")},
            {"5,unused", TagPath.FromPathAndType(@"objects\gear\forerunner\power_core_for\power_core_for", "bloc*")},
            {"6,Race checkpoint", TagPath.FromPathAndType(@"objects\gear\forerunner\power_core_for\power_core_for", "bloc*")},
            {"7,Teleporter (src)", TagPath.FromPathAndType(@"objects\multi\teleporter_sender\teleporter_sender", "bloc*")},
            {"8,Teleporter (dest)", TagPath.FromPathAndType(@"objects\multi\teleporter_reciever\teleporter_reciever", "bloc*")},
            {"9,Headhunter bin", TagPath.FromPathAndType(@"objects\gear\forerunner\power_core_for\power_core_for", "bloc*")},
            {"10,Territories flag", TagPath.FromPathAndType(@"objects\multi\territories\territory_static", "bloc*")},
            {"11,King Hill 0", TagPath.FromPathAndType(@"objects\multi\koth\koth_hill_static", "bloc*")},
            {"12,King Hill 1", TagPath.FromPathAndType(@"objects\multi\koth\koth_hill_static", "bloc*")},
            {"13,King Hill 2", TagPath.FromPathAndType(@"objects\multi\koth\koth_hill_static", "bloc*")},
            {"14,King Hill 3", TagPath.FromPathAndType(@"objects\multi\koth\koth_hill_static", "bloc*")},
            {"15,King Hill 4", TagPath.FromPathAndType(@"objects\multi\koth\koth_hill_static", "bloc*")},
            {"16,King Hill 5", TagPath.FromPathAndType(@"objects\multi\koth\koth_hill_static", "bloc*")},
            {"17,King Hill 6", TagPath.FromPathAndType(@"objects\multi\koth\koth_hill_static", "bloc*")},
            {"18,King Hill 7", TagPath.FromPathAndType(@"objects\multi\koth\koth_hill_static", "bloc*")},
        };

        // Netgame vehicle dictionary
        Dictionary<string, TagPath> netVehiMapping = new Dictionary<string, TagPath>
        {
            {"banshee", TagPath.FromPathAndType(@"objects\vehicles\banshee\banshee", "vehi*")},
            {"banshee_heretic", TagPath.FromPathAndType(@"objects\vehicles\banshee\banshee", "vehu*")},
            {"ghost", TagPath.FromPathAndType(@"objects\vehicles\ghost\ghost", "vehi*")},
            {"c_turret_mp", TagPath.FromPathAndType(@"objects\weapons\turret\plasma_cannon\plasma_cannon", "vehi*")},
            {"h_turret_ap", TagPath.FromPathAndType(@"objects\weapons\turret\machinegun_turret\machinegun_turret", "vehi*")},
            {"h_turret_mp", TagPath.FromPathAndType(@"objects\weapons\turret\machinegun_turret\machinegun_turret", "vehi*")},
            {"scorpion", TagPath.FromPathAndType(@"objects\vehicles\scorpion\scorpion", "vehi*")},
            {"warthog", TagPath.FromPathAndType(@"objects\vehicles\warthog\warthog", "vehi*")},
            {"warthog_gauss", TagPath.FromPathAndType(@"objects\vehicles\warthog\warthog", "vehi*")},
            {"wraith", TagPath.FromPathAndType(@"objects\vehicles\wraith\wraith", "vehi*")}
        };

        // Variables
        var tag_path = TagPath.FromPathAndType(Path.ChangeExtension(scen_path.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
        var respawn_scen_path = TagPath.FromPathAndType(@"objects\multi\spawning\respawn_point", "scen*");
        int respawn_scen_index = 0;

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);

        using (var tagFile = new TagFile(tag_path))
        {
            // Object names section
            foreach (var name in all_object_names)
            {
                // Add new
                int count = ((TagFieldBlock)tagFile.Fields[19]).Elements.Count();
                ((TagFieldBlock)tagFile.Fields[19]).AddElement();
                var name_field = (TagFieldElementStringNormal)((TagFieldBlock)tagFile.Fields[19]).Elements[count].Fields[0];
                name_field.Data = name;
            }

            // Spawns Section
            int i = 0;
            int temp_index = 0;
            bool respawn_found = false;
            int totalScenCount = 0;
            int totalVehiCount = 0;

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

            Dictionary<string, int> weapPaletteMapping = new Dictionary<string, int>();

            // Weapons Section
            foreach (var weapon in weap_data)
            {
                string weap_type = weapon.weap_type.Split('\\')[weapon.weap_type.Split('\\').Length - 1];

                if (weap_type == "frag_grenades" || weap_type == "plasma_grenades")
                {
                    // Grenade stuff, need to treat as equipment not weapon
                    Console.WriteLine("Adding " + weap_type + " equipment");
                    loadingForm.UpdateOutputBox("Adding " + weap_type + " equipment", false);

                    // Equipment, check if palette entry exists first
                    bool equip_entry_exists = false;
                    foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[27]).Elements)
                    {
                        var temp_type = weapMapping[weap_type];
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
                        equip_tag_ref.Path = weapMapping[weap_type];
                        weapPaletteMapping.Add(weap_type, current_count);
                    }

                    // Now add the equipment itself
                    int equip_count = ((TagFieldBlock)tagFile.Fields[26]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[26]).AddElement(); // Add new equipment entry

                    // XYZ
                    var equip_xyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[2];
                    equip_xyz.Data = weapon.weap_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Rotation
                    var equip_orient = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[3];
                    equip_orient.Data = weapon.weap_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Type
                    var equip_tag = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[1];
                    equip_tag.Value = weapPaletteMapping[weap_type];

                    // Spawn timer
                    var equip_stime = (TagFieldElementInteger)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[6]).Elements[0].Fields[8];
                    equip_stime.Data = uint.Parse(weapon.spawn_time);

                    // Dropdown type and source (won't be valid without these)
                    var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                    var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                    dropdown_type.Value = 3; // 3 for equipment
                    dropdown_source.Value = 1; // 1 for editor

                    continue;
                }
                else if (weap_type == "")
                {
                    // All games entries, ignore
                    Console.WriteLine("Ignoring blank weapon collection");
                    loadingForm.UpdateOutputBox("Ignoring blank weapon collection", false);
                    continue;
                }
                else if (weap_type.Contains("ammo"))
                {
                    // Ammo placement, ignore for now
                }
                else if (weap_type.Contains("powerup"))
                {
                    // Powerup, add equipment, check if palette entry exists first
                    bool equip_entry_exists = false;
                    foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[27]).Elements)
                    {
                        var temp_type = weapMapping["powerup"];
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
                        equip_tag_ref.Path = weapMapping["powerup"];
                        weapPaletteMapping.Add("powerup", current_count);
                    }

                    // Now add the equipment itself
                    int equip_count = ((TagFieldBlock)tagFile.Fields[26]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[26]).AddElement(); // Add new equipment entry

                    // XYZ
                    var equip_xyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[2];
                    equip_xyz.Data = weapon.weap_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Rotation
                    var equip_orient = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[3];
                    equip_orient.Data = weapon.weap_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Type
                    var equip_tag = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[1];
                    equip_tag.Value = weapPaletteMapping["powerup"];

                    // Dropdown type and source (won't be valid without these)
                    var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                    var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[26]).Elements[equip_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                    dropdown_type.Value = 3; // 3 for equipment
                    dropdown_source.Value = 1; // 1 for editor
                }
                else if (weapon.weap_type.Contains("vehicles"))
                {
                    // Check if current type exists in palette
                    bool type_exists_already = false;
                    foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[25]).Elements)
                    {
                        var x = ((TagFieldReference)palette_entry.Fields[0]).Path;
                        if (x == netVehiMapping[weap_type])
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
                        vehi_type_ref.Path = netVehiMapping[weap_type];
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
                    xyz_pos.Data = weapon.weap_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Rotation
                    var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[vehi_count].Fields[4]).Elements[0].Fields[3];
                    rotation.Data = weapon.weap_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();
                }
                else
                {
                    // Weapon, check if palette entry exists first
                    Console.WriteLine("Adding " + weap_type + " weapon");
                    loadingForm.UpdateOutputBox("Adding " + weap_type + " weapon", false);
                    bool weap_entry_exists = false;
                    foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[29]).Elements)
                    {
                        var temp_type = weapMapping[weap_type];
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
                        weap_tag_ref.Path = weapMapping[weap_type];
                        weapPaletteMapping.Add(weap_type, current_count);
                    }

                    // Now add the weapon itself
                    int weapon_count = ((TagFieldBlock)tagFile.Fields[28]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[28]).AddElement(); // Add new weapon entry

                    // XYZ
                    var weap_xyz = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[2];
                    weap_xyz.Data = weapon.weap_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Rotation
                    var weap_orient = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[3];
                    weap_orient.Data = weapon.weap_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Type
                    var weap_tag = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[1];
                    weap_tag.Value = weapPaletteMapping[weap_type];

                    // Spawn timer
                    var weap_stime = (TagFieldElementInteger)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[7]).Elements[0].Fields[8];
                    weap_stime.Data = uint.Parse(weapon.spawn_time);

                    // Dropdown type and source (won't be valid without these)
                    var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                    var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[28]).Elements[weapon_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                    dropdown_type.Value = 2; // 2 for weapon
                    dropdown_source.Value = 1; // 1 for editor
                }
            }

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
                int index = int.Parse(scenery.scen_type.ToString()) + totalScenCount;
                type_ref.Value = int.Parse(scenery.scen_type) + totalScenCount;

                // Dropdown type and source (won't be valid without these)
                var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                dropdown_type.Value = 6; // 6 for scenery
                dropdown_source.Value = 1; // 1 for editor

                // Position
                var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[0].FieldName;
                var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[2];
                xyz_pos.Data = scenery.scen_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Rotation
                var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[4]).Elements[0].Fields[3];
                rotation.Data = scenery.scen_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Variant
                var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[5]).Elements[0].Fields[0].FieldName;
                var variant = (TagFieldElementStringID)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[20]).Elements[current_count].Fields[5]).Elements[0].Fields[0];
                variant.Data = scenery.scen_vrnt;

                tagFile.Save();
            }

            // Trigger volumes section
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

            // Vehicle section
            foreach (TagPath vehi_type in all_vehi_types)
            {
                // Check if current type exists in palette
                bool type_exists_already = false;
                foreach (var palette_entry in ((TagFieldBlock)tagFile.Fields[25]).Elements)
                {
                    var x = ((TagFieldReference)palette_entry.Fields[0]).Path;
                    if (x == vehi_type)
                    {
                        type_exists_already = true;
                        break;
                    }
                }

                // Add palette entry if needed
                if (!type_exists_already)
                {
                    int current_count = ((TagFieldBlock)tagFile.Fields[25]).Elements.Count();
                    ((TagFieldBlock)tagFile.Fields[25]).AddElement();
                    var vehi_type_ref = (TagFieldReference)((TagFieldBlock)tagFile.Fields[25]).Elements[current_count].Fields[0];
                    vehi_type_ref.Path = vehi_type;
                    totalVehiCount++;
                }
            }

            foreach (Vehicle vehicle in all_vehi_entries)
            {
                int current_count = ((TagFieldBlock)tagFile.Fields[24]).Elements.Count();
                ((TagFieldBlock)tagFile.Fields[24]).AddElement();
                var type_ref = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[24]).Elements[current_count].Fields[1];
                type_ref.Value = int.Parse(vehicle.vehi_type);

                // Dropdown type and source (won't be valid without these)
                var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                dropdown_type.Value = 1; // 1 for vehicle
                dropdown_source.Value = 1; // 1 for editor

                // Position
                var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[current_count].Fields[4]).Elements[0].Fields[0].FieldName;
                var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[current_count].Fields[4]).Elements[0].Fields[2];
                xyz_pos.Data = vehicle.vehi_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Rotation
                var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[current_count].Fields[4]).Elements[0].Fields[3];
                rotation.Data = vehicle.vehi_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Variant
                var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[current_count].Fields[5]).Elements[0].Fields[0].FieldName;
                var variant = (TagFieldElementStringID)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[24]).Elements[current_count].Fields[5]).Elements[0].Fields[0];
                variant.Data = vehicle.vehi_vrnt;
            }

            // Crates section

            // Begin with creating the editor folders
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
                type_ref.Value = int.Parse(crate.crate_type);

                // Name
                var name_field = (TagFieldBlockIndex)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[3];
                name_field.Value = int.Parse(crate.crate_name);

                // Dropdown type and source (won't be valid without these)
                var dropdown_type = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[2];
                var dropdown_source = (TagFieldEnum)((TagFieldStruct)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[9]).Elements[0].Fields[3];
                dropdown_type.Value = 10; // 1 for crate
                dropdown_source.Value = 1; // 1 for editor

                // Position
                var y = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[0].FieldName;
                var xyz_pos = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[2];
                xyz_pos.Data = crate.crate_xyz.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Rotation
                var rotation = (TagFieldElementArraySingle)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[4]).Elements[0].Fields[3];
                rotation.Data = crate.crate_orient.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                // Variant
                var z = ((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[5]).Elements[0].Fields[0].FieldName;
                var variant = (TagFieldElementStringID)((TagFieldStruct)((TagFieldBlock)tagFile.Fields[118]).Elements[current_count].Fields[5]).Elements[0].Fields[0];
                variant.Data = crate.crate_vrnt;
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
                    crate_type_ref.Path = netflagMapping[netflag.netflag_type];
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

            // Decals section
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

            tagFile.Save();

            Console.WriteLine("\nScenario data conversion complete!");
            loadingForm.UpdateOutputBox("\nScenario data conversion complete!", false);
        }
    }
}