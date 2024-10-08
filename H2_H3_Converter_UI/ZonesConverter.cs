﻿using Bungie;
using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using H2_H3_Converter_UI;
using Bungie.Tags;

class Area
{
    public string Name { get; set; }
    public uint Flags { get; set; }
    public int RefFrame { get; set; }
}

class Fpos
{
    public float[] Position { get; set; }
    public uint Flags { get; set; }
    public int AreaRef { get; set; }
    public int ClusterIndex { get; set; }
    public float[] NormalDirection { get; set; }
}

class Zone
{
    public string ZoneName { get; set; }
    public int AreasCount { get; set; }
    public int FposCount { get; set; }
    public List<Area> Areas { get; set; } = new List<Area>();
    public List<Fpos> Fpos { get; set; } = new List<Fpos>();
}


class MB_Zones
{
    public static void ZoneConverter(string scen_path, string xml_path, Loading loadingForm)
    {
        string h3ek_path = scen_path.Substring(0, scen_path.IndexOf("H3EK") + "H3EK".Length);

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

            Console.WriteLine("Modified file saved successfully.\n\nPreparing to patch tag data.\n\nLoaded zones:\n");
            loadingForm.UpdateOutputBox("Modified file saved successfully.\n\nPreparing to patch tag data.\n\nLoaded zones:\n", false);
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

        XmlNodeList zones_start_blocks = root.SelectNodes(".//block[@name='zones']");

        List<string> zones_list = new List<string>();

        // Get all zone names
        foreach (XmlNode zones_block in zones_start_blocks)
        {
            bool zones_end = false;
            int i = 0;
            while (!zones_end)
            {
                string search_string = "./element[@index='" + i + "']";
                XmlNode element = zones_block.SelectSingleNode(search_string);

                if (element != null)
                {
                    string zoneName = element.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                    zones_list.Add(zoneName);
                    Console.WriteLine(zoneName);
                    loadingForm.UpdateOutputBox(zoneName, false);
                    i++;
                }
                else
                {
                    zones_end = true;
                }
            }
        }

        Console.WriteLine("\nBegin patching data:\n");
        loadingForm.UpdateOutputBox("\nBegin patching data:\n", false);

        // Temp file creation/wiping

        using (FileStream fs = new FileStream("temp_output.txt", FileMode.Create, FileAccess.Write))
        using (StreamWriter sw = new StreamWriter(fs))
        {
            // Get areas and firing positions for each name
            int i = 0;
            foreach (string zone_name in zones_list)
            {
                // Areas
                sw.WriteLine("Zone: " + zone_name + "\nAreas:");
                XmlNodeList areasBlockList = root.SelectNodes(".//block[@name='zones']/element[@index='" + i + "']/block[@name='areas']");
                foreach (XmlNode area_block in areasBlockList)
                {
                    bool areas_end = false;
                    int j = 0;
                    while (areas_end == false)
                    {
                        string search_string = "./element[@index='" + j.ToString() + "']";
                        var element = area_block.SelectSingleNode(search_string);

                        if (element != null)
                        {
                            sw.WriteLine(element.SelectSingleNode("./field[@name='name']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='area flags']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='runtime starting index']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='runtime count']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='manual reference frame']").InnerText.Trim());
                            j++;
                        }
                        else
                        {
                            areas_end = true;
                        }
                    }
                }

                // Firing Positions
                sw.WriteLine("Firing Positions:");
                XmlNodeList fpos_block_list = root.SelectNodes(".//block[@name='zones']/element[@index='" + i + "']/block[@name='firing positions']");
                foreach (XmlNode fpos_block in fpos_block_list)
                {
                    bool fpos_end = false;
                    int j = 0;
                    while (fpos_end == false)
                    {
                        string search_string = "./element[@index='" + j.ToString() + "']";
                        var element = fpos_block.SelectSingleNode(search_string);

                        if (element != null)
                        {
                            sw.WriteLine("index = " + j.ToString());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='position (local)']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='reference frame']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='flags']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"].Value.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='cluster index']").InnerText.Trim());
                            sw.WriteLine(element.SelectSingleNode("./field[@name='normal']").InnerText.Trim());
                            j++;
                        }
                        else
                        {
                            fpos_end = true;
                        }
                    }
                }
                i++;
            }
        }
        Edit_The_Tags(h3ek_path, scen_path, loadingForm);
    }

    static void Edit_The_Tags(string h3ek_path, string scen_path, Loading loadingForm)
    {
        List<Zone> zone_data = new List<Zone>();
        List<int> total_area_counts = new List<int>();
        List<int> total_fpos_counts = new List<int>();
        List<string> zone_names = new List<string>();
        List<string> area_names = new List<string>();
        List<uint> area_flags = new List<uint>();
        List<int> area_refs = new List<int>();
        List<float[]> fpos_xyz = new List<float[]>();
        List<uint> fpos_flags = new List<uint>();
        List<int> fpos_area = new List<int>();
        List<int> fpos_cluster = new List<int>();
        List<float[]> fpos_normal = new List<float[]>();

        Console.WriteLine("Beginning tag writing process...");
        loadingForm.UpdateOutputBox("Beginning tag writing process...", false);

        int zone = -1;
        bool areas = false;
        bool fpos = false;
        int areaIndex = -1;
        int fposIndex = -1;
        int areaDataCount = 0;
        int fposDataCount = 0;
        int totalFposCount = 0;
        int totalAreaCount = 0;
        int fposFlagLineSkip = 0;
        int areaFlagLinesToSkip = 0;

        Dictionary<String, int> flagIntToLinesSkip = new Dictionary<String, int>()
        {
            { "1", 1 }, // Vehicle area only
            { "2", 1 }, // Perch only
            { "4", 1 }, // Manual reference frame only
            { "3", 2 }, // Perch and vehicle area
            { "5", 2 }, // Manual reference frame and vehicle area
            { "6", 2 }, // Manual reference frame and perch
            { "7", 3 }, // All 3
        };

        string[] text = File.ReadAllLines("temp_output.txt");

        foreach (string line in text)
        {
            if (line.Contains("Zone:"))
            {
                zone++;
                zone_names.Add(line.Substring(line.IndexOf(' ') + 1).Trim());
                if (zone != 0)
                {
                    total_area_counts.Add(totalAreaCount);
                    total_fpos_counts.Add(totalFposCount);
                    List<Area> tempAreasMain = new List<Area>();
                    List<Fpos> tempFposMain = new List<Fpos>();
                    int countMain = 0;
                    int countFposMain = 0;
                    foreach (var name in area_names)
                    {
                        tempAreasMain.Add(new Area { Name = area_names[countMain], Flags = area_flags[countMain], RefFrame = area_refs[countMain] });
                        countMain++;
                    }
                    foreach (var xyz in fpos_xyz)
                    {
                        tempFposMain.Add(new Fpos { Position = fpos_xyz[countFposMain], Flags = fpos_flags[countFposMain], AreaRef = fpos_area[countFposMain], ClusterIndex = fpos_cluster[countFposMain], NormalDirection = fpos_normal[countFposMain] });
                        countFposMain++;
                    }
                    zone_data.Add(new Zone
                    {
                        ZoneName = zone_names[zone - 1],
                        AreasCount = total_area_counts[zone - 1],
                        FposCount = total_fpos_counts[zone - 1],
                        Areas = tempAreasMain,
                        Fpos = tempFposMain
                    });
                }
                Console.WriteLine(line.Trim());
                loadingForm.UpdateOutputBox(line.Trim(), false);
                //PatchTag("zone", line.Substring(line.IndexOf(' ') + 1).Trim(), 0, 0);
                continue;
            }
            else if (line.Contains("Areas:"))
            {
                areas = true;
                fpos = false;
                areaIndex = 0;
                areaDataCount = 0;
                totalAreaCount = 0;
                area_names.Clear();
                area_flags.Clear();
                area_refs.Clear();
                continue;
            }
            else if (line.Contains("Firing Positions:"))
            {
                fpos = true;
                areas = false;
                fposDataCount = 0;
                fposIndex = 0;
                totalFposCount = 0;
                fpos_xyz.Clear();
                fpos_flags.Clear();
                fpos_area.Clear();
                fpos_cluster.Clear();
                fpos_normal.Clear();
                continue;
            }
            else if (areas)
            {
                if (areaFlagLinesToSkip != 0)
                {
                    areaFlagLinesToSkip--;
                    continue;
                }
                if (areaDataCount == 0)
                {
                    // Add new area
                    //PatchTag("area", line, -1, 0);
                    // Patch area name
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].areas[{areaIndex}].name";
                    Console.WriteLine($"patching {line.Trim()}");
                    //loadingForm.UpdateOutputBox($"patching {line.Trim()}", false);
                    area_names.Add(line.Trim());
                    //PatchTag("area", line.Trim(), 0, totalAreaCount);
                    areaDataCount++;
                }
                else if (areaDataCount == 1)
                {
                    // Patch area flags
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].areas[{areaIndex}].area flags";
                    Console.WriteLine("area flags");
                    //loadingForm.UpdateOutputBox("area flags", false);
                    area_flags.Add(uint.Parse(line.Trim()));
                    //PatchTag("area", line.Trim(), 1, totalAreaCount);
                    areaDataCount++;
                    if (line.Trim() != "0")
                    {
                        areaFlagLinesToSkip = flagIntToLinesSkip[line.Trim()];
                    }
                }
                else if (areaDataCount == 2)
                {
                    // line is runtime starting index
                    areaDataCount++;
                    continue;
                }
                else if (areaDataCount == 3)
                {
                    // line is runtime count
                    areaDataCount++;
                    continue;
                }
                else if (areaDataCount == 4)
                {
                    // Patch manual reference frame
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].areas[{areaIndex}].manual reference frame";
                    Console.WriteLine($"manual ref frame = {line.Trim()}");
                    //loadingForm.UpdateOutputBox($"manual ref frame = {line.Trim()}", false);
                    area_refs.Add(int.Parse(line.Trim()));
                    //PatchTag("area", line.Trim(), 4, totalAreaCount);
                    areaDataCount = 0;
                    areaIndex++;
                    totalAreaCount++;
                }
            }
            else if (fpos)
            {
                if (fposFlagLineSkip > 0)
                {
                    fposFlagLineSkip--;
                    continue;
                }
                if (fposDataCount == 0)
                {
                    // Patch firing position index
                    Console.WriteLine($"firing position {line.Trim()} patch start");
                    loadingForm.UpdateOutputBox($"firing position {line.Trim()} patch start", false);
                    //PatchTag("fpos", line.Trim(), 0, totalFposCount);
                    fposDataCount++;
                }
                else if (fposDataCount == 1)
                {
                    // Patch firing position position
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].position (local)";
                    Console.WriteLine("patching position");
                    //loadingForm.UpdateOutputBox("patching position", false);
                    fpos_xyz.Add(line.Trim().Split(',').Select(float.Parse).ToArray());
                    //PatchTag("fpos", line.Trim(), 1, totalFposCount);
                    fposDataCount++;
                }
                else if (fposDataCount == 2)
                {
                    // Patch firing position ref frame
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].reference frame";
                    Console.WriteLine("patching ref frame");
                    //loadingForm.UpdateOutputBox("patching ref frame", false);
                    fposDataCount++;
                }
                else if (fposDataCount == 3)
                {
                    // Patch firing position flags
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].flags";
                    Console.WriteLine("patching flags");
                    //loadingForm.UpdateOutputBox("patching flags", false);
                    fpos_flags.Add(uint.Parse(line.Trim()));
                    //PatchTag("fpos", line.Trim(), 3, totalFposCount);
                    if (int.Parse(line.Trim()) >= 90)
                    {
                        fposFlagLineSkip = 4;
                    }
                    else if (int.Parse(line.Trim()) > 70)
                    {
                        fposFlagLineSkip = 3;
                    }
                    else if (int.Parse(line.Trim()) < 9 && int.Parse(line.Trim()) >= 1)
                    {
                        fposFlagLineSkip = 1;
                    }
                    else if (int.Parse(line.Trim()) == 0)
                    {
                        fposFlagLineSkip = 0;
                    }
                    else
                    {
                        fposFlagLineSkip = 2;
                    }
                    fposDataCount++;
                }
                else if (fposDataCount == 4)
                {
                    // Patch firing position area
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].area";
                    Console.WriteLine("patching area");
                    //loadingForm.UpdateOutputBox("patching area", false);
                    fpos_area.Add(int.Parse(line.Trim()));
                    //PatchTag("fpos", line.Trim(), 4, totalFposCount);
                    fposDataCount++;
                }
                else if (fposDataCount == 5)
                {
                    // Patch firing position cluster index
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].cluster index";
                    Console.WriteLine("patching cluster index");
                    //loadingForm.UpdateOutputBox("patching cluster index", false);
                    fpos_cluster.Add(int.Parse(line.Trim()));
                    //PatchTag("fpos", line.Trim(), 5, totalFposCount);
                    fposDataCount++;
                }
                else if (fposDataCount == 6)
                {
                    // Patch firing position normal
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].normal";
                    Console.WriteLine("patching normal");
                    //loadingForm.UpdateOutputBox("patching normal", false);
                    fpos_normal.Add(line.Trim().Split(',').Select(float.Parse).ToArray());
                    //PatchTag("fpos", line.Trim(), 6, totalFposCount);
                    fposDataCount = 0;
                    fposIndex++;
                    totalFposCount++;
                }
            }
        }
        // All lines are done, lets add the final zone
        total_area_counts.Add(totalAreaCount);
        total_fpos_counts.Add(totalFposCount);
        List<Area> tempAreas = new List<Area>();
        List<Fpos> tempFpos = new List<Fpos>();
        int count = 0;
        int countFpos = 0;
        foreach (var name in area_names)
        {
            tempAreas.Add(new Area { Name = area_names[count], Flags = area_flags[count], RefFrame = area_refs[count] });
            count++;
        }
        foreach (var xyz in fpos_xyz)
        {
            tempFpos.Add(new Fpos { Position = fpos_xyz[countFpos], Flags = fpos_flags[countFpos], AreaRef = fpos_area[countFpos], ClusterIndex = fpos_cluster[countFpos], NormalDirection = fpos_normal[countFpos] });
            countFpos++;
        }
        try
        {
            zone_data.Add(new Zone
            {
                ZoneName = zone_names[zone],
                AreasCount = total_area_counts[zone],
                FposCount = total_fpos_counts[zone],
                Areas = tempAreas,
                Fpos = tempFpos
            });
        }
        catch (Exception)
        {
            // User tried to generate zones on a multiplayer map? Let's not just crash huh
            Console.WriteLine("Tried to generate zone data, but none found!");
        }
        ManagedBlamHandler(zone_data, h3ek_path, scen_path, loadingForm);
    }

    static void ManagedBlamHandler(List<Zone> zone_data, string h3ek_path, string scen_path, Loading loadingForm)
    {
        // Variables
        var tag_path = TagPath.FromPathAndType(Path.ChangeExtension(scen_path.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
        Console.WriteLine("\nSaving tag...\n");
        loadingForm.UpdateOutputBox("\nSaving tag...\n", false);

        using (var tagFile = new TagFile(tag_path))
        {
            // Clear all existing zone data
            ((TagFieldBlock)tagFile.SelectField("Block:zones")).RemoveAllElements();

            // Add all zone entries
            int i = 0;
            foreach (var zone in zone_data)
            {
                ((TagFieldBlock)tagFile.SelectField("Block:zones")).AddElement(); // 83 is the position of the Zones block
                ((TagFieldElementString)tagFile.SelectField($"Block:zones[{i}]/String:name")).Data = zone.ZoneName;
                i++;
            }

            // Add all area entries
            int zoneIndex = 0;
            int areaIndex = 0;
            foreach (var zone in zone_data)
            {
                foreach (Area area in zone.Areas)
                {
                    // Name
                    ((TagFieldBlock)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:areas")).AddElement();
                    Console.WriteLine(zoneIndex);
                    Console.WriteLine(areaIndex);
                    Console.WriteLine(area);
                    Console.WriteLine(area.Name);
                    ((TagFieldElementString)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:areas[{areaIndex}]/String:name")).Data = area.Name;

                    // Flags
                    ((TagFieldFlags)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:areas[{areaIndex}]/Flags:area flags")).RawValue = area.Flags;

                    // Reference Frame
                    if (area.RefFrame != 0)
                    {
                        ((TagFieldBlockIndex)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:areas[{areaIndex}]/ShortBlockIndex:manual reference frame")).Value = area.RefFrame;
                    }
                    areaIndex++;
                }
                areaIndex = 0;
                zoneIndex++;
            }

            // Add all firing position entries
            zoneIndex = 0;
            int fposIndex = 0;
            foreach (var zone in zone_data)
            {
                foreach (Fpos fpos in zone.Fpos)
                {
                    // Add
                    ((TagFieldBlock)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions")).AddElement();

                    // XYZ Position
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions[{fposIndex}]/RealPoint3d:position (local)")).Data = fpos.Position;

                    // Flags
                    ((TagFieldFlags)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions[{fposIndex}]/WordFlags:flags")).RawValue = fpos.Flags;

                    // Area Reference
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions[{fposIndex}]/ShortBlockIndex:area")).Value = fpos.AreaRef;

                    // Cluster Index
                    ((TagFieldElementInteger)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions[{fposIndex}]/ShortInteger:cluster index")).Data = fpos.ClusterIndex;

                    // Normal Direction
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions[{fposIndex}]/RealEulerAngles2d:normal")).Data = fpos.NormalDirection;

                    fposIndex++;
                }
                fposIndex = 0;
                zoneIndex++;
            }
            // Aaaaand save everything in one go
            tagFile.Save();

            Console.WriteLine("All zones, areas and firing positions have been successfully transferred into the H3 scenario!");
            loadingForm.UpdateOutputBox("All zones, areas and firing positions have been successfully transferred into the H3 scenario!", false);
        }
    }
}