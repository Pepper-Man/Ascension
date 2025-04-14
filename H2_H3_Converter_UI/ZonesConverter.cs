using Bungie;
using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using H2_H3_Converter_UI;
using Bungie.Tags;
using System.Windows.Forms;

class Area
{
    public string Name { get; set; }
    public uint Flags { get; set; }
    public int RefFrame { get; set; }
}

class Fpos
{
    public float[] Position { get; set; }
    public int AreaRef { get; set; }
    public float[] NormalDirection { get; set; }
}

class Zone
{
    public string ZoneName { get; set; }
    public uint Flags { get; set; }
    public int BspIndex { get; set; }
    public List<Area> Areas { get; set; } = new List<Area>();
    public List<Fpos> Fpos { get; set; } = new List<Fpos>();
}


class MB_Zones
{
    public static bool ZoneConverter(string scenPath, string xmlPath, Loading loadingForm)
    {
        string h3ekPath = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);

        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ekPath);
        xmlPath = Utils.ConvertXML(xmlPath, loadingForm);

        XmlDocument scenfile = new XmlDocument();
        try
        {
            scenfile.Load(xmlPath);
        }
        catch (Exception e)
        {
            MessageBox.Show("Error when loading XML file!\n\n" + e, "XML Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        
        XmlNode zonesBlock = scenfile.DocumentElement.SelectSingleNode(".//block[@name='zones']");

        if (zonesBlock == null)
        {
            Console.WriteLine("\nNo zones block found! Skipping...\n");
            loadingForm.UpdateOutputBox("\nNo zones block found! Skipping...\n", false);
            return true;
        }

        Console.WriteLine("\nBegin reading zones data:\n");
        loadingForm.UpdateOutputBox("\nBegin reading zones data:\n", false);

        List<Zone> allZones = new List<Zone>();

        // Get all zone data
        bool zonesEnd = false;
        int zoneIndex = 0;
        while (!zonesEnd)
        {
            XmlNode zoneElement = zonesBlock.SelectSingleNode("./element[@index='" + zoneIndex + "']");

            if (zoneElement != null)
            {
                Zone zone = new Zone
                {
                    ZoneName = zoneElement.SelectSingleNode("./field[@name='name']").InnerText.Trim(),
                    Flags = uint.Parse(zoneElement.SelectSingleNode("./field[@name='flags']").InnerText.Trim().Split('\n').First()),
                    BspIndex = int.Parse(zoneElement.SelectSingleNode("./block_index[@type='manual bsp']").Attributes["index"]?.Value)
                };

                // Get all areas for zone
                bool areasEnd = false;
                int areaIndex = 0;
                List<Area> allAreas = new List<Area>();
                while (!areasEnd)
                {
                    XmlNode areaElement = zonesBlock.SelectSingleNode("./element[@index='" + zoneIndex + "']/block[@name='areas']/element[@index='" + areaIndex + "']");

                    if (areaElement != null)
                    {
                        Area area = new Area
                        {
                            Name = areaElement.SelectSingleNode("./field[@name='name']").InnerText.Trim(),
                            RefFrame = int.Parse(areaElement.SelectSingleNode("./field[@name='manual reference frame']").InnerText.Trim())
                        };

                        // Area flags don't match 1:1 with H3, only want to use the flag value if it is none (0), vehicle area (1), manual ref frame (4) or a combo of those
                        uint flagValue = uint.Parse(areaElement.SelectSingleNode("./field[@name='area flags']").InnerText.Trim().Split('\n').First());
                        if (flagValue == 0 || flagValue == 1 || flagValue == 4 || flagValue == 5)
                        {
                            area.Flags = flagValue;
                        }

                        allAreas.Add(area);
                        areaIndex++;
                    }
                    else
                    {
                        // End of areas for zone
                        areasEnd = true;
                        zone.Areas = allAreas;
                    }
                }

                // Get all firing positions for zone
                bool fposEnd = false;
                int fposIndex = 0;
                List<Fpos> allFpos = new List<Fpos>();
                while (!fposEnd)
                {
                    XmlNode fposElement = zonesBlock.SelectSingleNode("./element[@index='" + zoneIndex + "']/block[@name='firing positions']/element[@index='" + fposIndex + "']");
                
                    if (fposElement != null)
                    {
                        Fpos fpos = new Fpos
                        {
                            Position = fposElement.SelectSingleNode("./field[@name='position (local)']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                            AreaRef = int.Parse(fposElement.SelectSingleNode("./block_index[@type='area']").Attributes["index"]?.Value),
                            NormalDirection = fposElement.SelectSingleNode("./field[@name='normal']").InnerText.Trim().Split(',').Select(float.Parse).ToArray()
                        };

                        allFpos.Add(fpos);
                        fposIndex++;
                    }
                    else
                    {
                        // End of firing positions for zone
                        fposEnd = true;
                        zone.Fpos = allFpos;
                    }
                }


                allZones.Add(zone);
                Console.WriteLine($"Read data for Zone {zone.ZoneName}");
                loadingForm.UpdateOutputBox($"Read data for zone \"{zone.ZoneName}\"", false);
                zoneIndex++;
            }
            else
            {
                zonesEnd = true;
                Console.WriteLine("Finished reading all zone data!");
                loadingForm.UpdateOutputBox("Finished reading all zone data!", false);
            }
        }

        ZonesManagedBlam(allZones, h3ekPath, scenPath, loadingForm);
        return true;
    }

    static void ZonesManagedBlam(List<Zone> allZones, string h3ekPath, string scenPath, Loading loadingForm)
    {
        var tagPath = TagPath.FromPathAndType(Path.ChangeExtension(scenPath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
        ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ekPath);

        using (var tagFile = new TagFile(tagPath))
        {
            // Clear all existing zone data
            ((TagFieldBlock)tagFile.SelectField("Block:zones")).RemoveAllElements();

            // Add all zone entries
            int zoneIndex = 0;
            foreach (var zone in allZones)
            {
                Console.WriteLine($"Writing data for zone {zone.ZoneName}");
                loadingForm.UpdateOutputBox($"Writing data for zone {zone.ZoneName}", false);
                ((TagFieldBlock)tagFile.SelectField("Block:zones")).AddElement();

                // Zone data
                ((TagFieldElementString)tagFile.SelectField($"Block:zones[{zoneIndex}]/String:name")).Data = zone.ZoneName;
                ((TagFieldFlags)tagFile.SelectField($"Block:zones[{zoneIndex}]/Flags:flags")).RawValue = zone.Flags;
                ((TagFieldBlockIndex)tagFile.SelectField($"Block:zones[{zoneIndex}]/ShortBlockIndex:manual bsp")).Value = zone.BspIndex;

                // Add all area entries
                int areaIndex = 0;
                foreach (Area area in zone.Areas)
                {
                    // Add new
                    ((TagFieldBlock)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:areas")).AddElement();
                  
                    // Name
                    ((TagFieldElementString)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:areas[{areaIndex}]/String:name")).Data = area.Name;

                    // Flags
                    ((TagFieldFlags)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:areas[{areaIndex}]/Flags:area flags")).RawValue = area.Flags;

                    // Reference Frame
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:areas[{areaIndex}]/ShortBlockIndex:manual reference frame")).Value = area.RefFrame;
                    
                    areaIndex++;
                }

                // Add all firing position entries
                int fposIndex = 0;
                foreach (Fpos fpos in zone.Fpos)
                {
                    // Add new
                    ((TagFieldBlock)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions")).AddElement();

                    // XYZ Position
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions[{fposIndex}]/RealPoint3d:position (local)")).Data = fpos.Position;

                    // Area Reference
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions[{fposIndex}]/ShortBlockIndex:area")).Value = fpos.AreaRef;

                    // Normal Direction
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:zones[{zoneIndex}]/Block:firing positions[{fposIndex}]/RealEulerAngles2d:normal")).Data = fpos.NormalDirection;

                    fposIndex++;
                }

                zoneIndex++;
            }

            // Aaaaand save everything in one go
            tagFile.Save();

            Console.WriteLine("All zones, areas and firing positions have been successfully transferred into the H3 scenario!");
            loadingForm.UpdateOutputBox("All zones, areas and firing positions have been successfully transferred into the H3 scenario!", false);
        }
    }
}