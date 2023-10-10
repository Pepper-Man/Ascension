using Bungie;
using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

class Area
{
    public string AreaName { get; set; }
    public string AreaFlags { get; set; }
    public string AreaRefFrame { get; set; }
}

class Fpos
{
    public string XYZCoord { get; set; }
    public string FposFlags { get; set; }
    public string AreaRef { get; set; }
    public string ClusterIndex { get; set; }
    public string NormalDirection { get; set; }
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
    public static void ZoneConverter(string scen_path, string xml_path)
    {
        string h3ek_path = scen_path.Substring(0, scen_path.IndexOf("H3EK") + "H3EK".Length);

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
        Convert_XML(xml_path, h3ek_path, scen_path);
    }

    static void Convert_XML(string xml_path, string h3ek_path, string scen_path)
    {
        Console.WriteLine("\nBeginning XML Conversion:\n");

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
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
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
                    i++;
                }
                else
                {
                    zones_end = true;
                }
            }
        }

        Console.WriteLine("\nBegin patching data:\n");

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
        Edit_The_Tags(h3ek_path, scen_path);
    }

    static void Edit_The_Tags(string h3ek_path, string scen_path)
    {
        List<Zone> zone_data = new List<Zone>();
        List<int> total_area_counts = new List<int>();
        List<int> total_fpos_counts = new List<int>();
        List<string> zone_names = new List<string>();
        List<string> area_names = new List<string>();
        List<string> area_flags = new List<string>();
        List<string> area_refs = new List<string>();
        List<string> fpos_xyz = new List<string>();
        List<string> fpos_flags = new List<string>();
        List<string> fpos_area = new List<string>();
        List<string> fpos_cluster = new List<string>();
        List<string> fpos_normal = new List<string>();

        Console.WriteLine("Beginning tag writing process...");

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
        bool previousWasActualFlag = false;

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
                        tempAreasMain.Add(new Area { AreaName = area_names[countMain], AreaFlags = area_flags[countMain], AreaRefFrame = area_refs[countMain] });
                        countMain++;
                    }
                    foreach (var xyz in fpos_xyz)
                    {
                        tempFposMain.Add(new Fpos { XYZCoord = fpos_xyz[countFposMain], FposFlags = fpos_flags[countFposMain], AreaRef = fpos_area[countFposMain], ClusterIndex = fpos_cluster[countFposMain], NormalDirection = fpos_normal[countFposMain] });
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
                if (previousWasActualFlag)
                {
                    previousWasActualFlag = false;
                    continue;
                }
                if (areaDataCount == 0)
                {
                    // Add new area
                    //PatchTag("area", line, -1, 0);
                    // Patch area name
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].areas[{areaIndex}].name";
                    Console.WriteLine($"patching {line.Trim()}");
                    area_names.Add(line.Trim());
                    //PatchTag("area", line.Trim(), 0, totalAreaCount);
                    areaDataCount++;
                }
                else if (areaDataCount == 1)
                {
                    // Patch area flags
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].areas[{areaIndex}].area flags";
                    Console.WriteLine("area flags");
                    area_flags.Add(line.Trim());
                    //PatchTag("area", line.Trim(), 1, totalAreaCount);
                    areaDataCount++;
                    if (line.Trim() != "0")
                    {
                        previousWasActualFlag = true;
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
                    area_refs.Add(line.Trim());
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
                    //PatchTag("fpos", line.Trim(), 0, totalFposCount);
                    fposDataCount++;
                }
                else if (fposDataCount == 1)
                {
                    // Patch firing position position
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].position (local)";
                    Console.WriteLine("patching position");
                    fpos_xyz.Add(line.Trim());
                    //PatchTag("fpos", line.Trim(), 1, totalFposCount);
                    fposDataCount++;
                }
                else if (fposDataCount == 2)
                {
                    // Patch firing position ref frame
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].reference frame";
                    Console.WriteLine("patching ref frame");
                    fposDataCount++;
                }
                else if (fposDataCount == 3)
                {
                    // Patch firing position flags
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].flags";
                    Console.WriteLine("patching flags");
                    fpos_flags.Add(line.Trim());
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
                    fpos_area.Add(line.Trim());
                    //PatchTag("fpos", line.Trim(), 4, totalFposCount);
                    fposDataCount++;
                }
                else if (fposDataCount == 5)
                {
                    // Patch firing position cluster index
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].cluster index";
                    Console.WriteLine("patching cluster index");
                    fpos_cluster.Add(line.Trim());
                    //PatchTag("fpos", line.Trim(), 5, totalFposCount);
                    fposDataCount++;
                }
                else if (fposDataCount == 6)
                {
                    // Patch firing position normal
                    string fieldPath = $"scenario_struct_definition[0].zones[{zone}].firing positions[{fposIndex}].normal";
                    Console.WriteLine("patching normal");
                    fpos_normal.Add(line.Trim());
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
            tempAreas.Add(new Area { AreaName = area_names[count], AreaFlags = area_flags[count], AreaRefFrame = area_refs[count] });
            count++;
        }
        foreach (var xyz in fpos_xyz)
        {
            tempFpos.Add(new Fpos { XYZCoord = fpos_xyz[countFpos], FposFlags = fpos_flags[countFpos], AreaRef = fpos_area[countFpos], ClusterIndex = fpos_cluster[countFpos], NormalDirection = fpos_normal[countFpos] });
            countFpos++;
        }
        zone_data.Add(new Zone
        {
            ZoneName = zone_names[zone],
            AreasCount = total_area_counts[zone],
            FposCount = total_fpos_counts[zone],
            Areas = tempAreas,
            Fpos = tempFpos
        });
        ManagedBlamHandler(zone_data, h3ek_path, scen_path);
    }

    static void ManagedBlamHandler(List<Zone> zone_data, string h3ek_path, string scen_path)
    {
        // Variables
        var tag_path = Bungie.Tags.TagPath.FromPathAndType(Path.ChangeExtension(scen_path.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
        Console.WriteLine("\nSaving tag...\n");

        using (var tagFile = new Bungie.Tags.TagFile(tag_path))
        {
            int zones_max_index = ((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements.Count() - 1;

            // Add all zone entries
            int i = 0;
            foreach (var zone in zone_data)
            {
                ((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).AddElement(); // 83 is the position of the Zones block
                var zone_name = (Bungie.Tags.TagFieldElementString)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[i].Fields[0];
                zone_name.Data = zone.ZoneName;
                i++;
            }

            // Add all area entries
            int j = 0;
            foreach (var zone in zone_data)
            {
                int loopcount = zone.AreasCount;
                for (int k = 0; k < loopcount; k++)
                {
                    // Name
                    ((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[j].Fields[4]).AddElement();
                    var area_name = (Bungie.Tags.TagFieldElementString)((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[j].Fields[4]).Elements[k].Fields[0];
                    area_name.Data = zone.Areas[k].AreaName;

                    // Flags
                    var area_flags = (Bungie.Tags.TagFieldFlags)((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[j].Fields[4]).Elements[k].Fields[1];
                    area_flags.RawValue = uint.Parse(zone.Areas[k].AreaFlags);

                    // Reference Frame
                    if (zone.Areas[k].AreaRefFrame != "0")
                    {
                        var reference_frame = (Bungie.Tags.TagFieldBlockIndex)((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[j].Fields[4]).Elements[k].Fields[9];
                        reference_frame.Value = int.Parse(zone.Areas[k].AreaRefFrame);
                    }
                }
                j++;
            }

            // Add all firing position entries
            int x = 0;
            foreach (var zone in zone_data)
            {
                int loopcount = zone.FposCount;
                for (int y = 0; y < loopcount; y++)
                {
                    // Add
                    ((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[x].Fields[3]).AddElement();

                    // XYZ Position
                    var xyz_pos = (Bungie.Tags.TagFieldElementArraySingle)((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[x].Fields[3]).Elements[y].Fields[1];
                    // Splits the string into a float array of xyz coordinates
                    xyz_pos.Data = zone.Fpos[y].XYZCoord.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();

                    // Flags
                    var fpos_flags = (Bungie.Tags.TagFieldFlags)((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[x].Fields[3]).Elements[y].Fields[4];
                    fpos_flags.RawValue = uint.Parse(zone.Fpos[y].FposFlags);

                    // Area Reference
                    var area_ref = (Bungie.Tags.TagFieldBlockIndex)((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[x].Fields[3]).Elements[y].Fields[6];
                    area_ref.Value = int.Parse(zone.Fpos[y].AreaRef);

                    // Cluster Index
                    var cluster_index = (Bungie.Tags.TagFieldElementInteger)((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[x].Fields[3]).Elements[y].Fields[7];
                    cluster_index.Data = int.Parse(zone.Fpos[y].ClusterIndex);

                    // Normal Direction
                    var normal_dir = (Bungie.Tags.TagFieldElementArraySingle)((Bungie.Tags.TagFieldBlock)((Bungie.Tags.TagFieldBlock)tagFile.Fields[83]).Elements[x].Fields[3]).Elements[y].Fields[10];
                    // Splits the string into a float array of xy coordinates
                    normal_dir.Data = zone.Fpos[y].NormalDirection.Split(',').Select(valueString => float.TryParse(valueString, out float floatValue) ? floatValue : float.NaN).ToArray();
                }
                x++;
            }
            // Aaaaand save everything in one go
            tagFile.Save();

            Console.WriteLine("All zones, areas and firing positions have been successfully transferred into the H3 scenario!");
        }
    }
}