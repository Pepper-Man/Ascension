﻿using Bungie;
using Bungie.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace H2_H3_Converter_UI
{
    public class Utils
    {
        public static string ConvertXML(string xmlPath, Loading loadingForm)
        {
            loadingForm.UpdateOutputBox("\nBeginning XML Conversion:\n", false);

            string newFilePath = Path.Combine(Directory.GetCurrentDirectory(), "modified_input.xml");

            try
            {
                string[] lines = File.ReadAllLines(xmlPath);
                bool removeLines = false;

                // This code isn't easy to understand at first glance but
                // essentially it just uses a bool to stop writing the lines
                // to the file whilst inside the "source files" block
                using (StreamWriter writer = new StreamWriter(newFilePath))
                {
                    foreach (string line in lines)
                    {
                        if (line.Contains("<block name=\"source files\">"))
                        {
                            removeLines = true;
                            writer.WriteLine(line + "</block>");
                        }
                        else if (line.Contains("<block name=\"scripting data\">"))
                        {
                            writer.WriteLine(line);
                            removeLines = false;
                        }
                        else if (!removeLines)
                        {
                            writer.WriteLine(line);
                        }
                    }
                }

                loadingForm.UpdateOutputBox("Modified file saved successfully. Preparing to patch tag data:\n", false);
            }
            catch (Exception ex)
            {
                loadingForm.UpdateOutputBox("An error occurred: " + ex.Message, false);
            }

            return newFilePath;
        }
    
        public static void BackupScenario(string scenPath, Loading loadingForm)
        {
            // Create scenario backup
            string backup_folderpath = Path.GetDirectoryName(scenPath) + @"\scenario_backup";
            Directory.CreateDirectory(backup_folderpath);
            string backup_filepath = Path.Combine(backup_folderpath, scenPath.Split('\\').Last());

            if (!File.Exists(backup_filepath))
            {
                File.Copy(scenPath, backup_filepath);
                loadingForm.UpdateOutputBox("Backup created successfully.", false);
            }
            else
            {
                loadingForm.UpdateOutputBox("Backup already exists.", false);
            }
        }

        public static void ConvertPalette(string scenPath, string h2ekPath, Loading loadingForm, XmlDocument scenfile, string paletteType, bool createObjects)
        {
            loadingForm.UpdateOutputBox($"Begin reading scenario {paletteType} palette from XML...", false);

            XmlNode root = scenfile.DocumentElement;
            string scenarioType = root.SelectSingleNode(".//field[@name='type']").InnerText.Trim();

            // Don't use sp object data for mp, unless its scenery, a device or sound scenery
            if (scenarioType.Contains("multiplayer") && paletteType != "machine" && paletteType != "control" && paletteType != "sound scenery" && paletteType != "scenery")
            {
                loadingForm.UpdateOutputBox($"Scenario type is MP, not processing {paletteType} palette from SP data.", false);
                return;
            }

            XmlNodeList paletteBlock = root.SelectNodes($".//block[@name='{paletteType} palette']");
            loadingForm.UpdateOutputBox($"Located {paletteType} palette data block.", false);

            List<string> h2ObjRefs = new List<string>();
            bool h2DataEnd = false;
            int i = 0;

            if (paletteBlock.Count > 0)
            {
                while (!h2DataEnd)
                {
                    XmlNode paletteEntry = paletteBlock[0].SelectSingleNode("./element[@index='" + i + "']");
                    if (paletteEntry != null)
                    {
                        string objRef;
                        if (paletteType == "character") // For some godforsaken reason, the character palette uses a different tag reference name
                        {
                            objRef = paletteEntry.SelectSingleNode("./tag_reference[@name='reference']").InnerText.Trim();
                        }
                        else
                        {
                            objRef = paletteEntry.SelectSingleNode("./tag_reference[@name='name']").InnerText.Trim();
                        }

                        h2ObjRefs.Add(objRef);
                        loadingForm.UpdateOutputBox($"{i}: {paletteType} reference {i}: \"{objRef}\".", false);
                        i++;
                    }
                    else
                    {
                        h2DataEnd = true;
                        loadingForm.UpdateOutputBox($"Finished reading {paletteType} palette data.", false);
                    }
                }
            }
            else
            {
                loadingForm.UpdateOutputBox($"No {paletteType} palette data!", false);
            }

            // MB tiem
            string h3ekPath = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ekPath);
            var relativeScenPath = TagPath.FromPathAndType(Path.ChangeExtension(scenPath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
            TagFile scenTag = new TagFile();

            // Convert H2 .weapon paths to H3 versions
            Utils utilsInstance = new Utils();
            List<TagPath> h3ObjPaths = new List<TagPath>();

            if (paletteType == "character")
            {
                foreach (string h2Path in h2ObjRefs)
                {
                    TagPath h3Obj;
                    try
                    {
                        h3Obj = utilsInstance.characterMapping[h2Path];
                    }
                    catch (KeyNotFoundException)
                    {
                        loadingForm.UpdateOutputBox($"H3 equivalent not found for character \"{h2Path}\", using H2 tag path.", false);
                        h3Obj = TagPath.FromPathAndExtension(h2Path, "character");
                    }
                    h3ObjPaths.Add(h3Obj);
                }
            }
            else if (paletteType == "weapon")
            {
                foreach (string h2Path in h2ObjRefs)
                {
                    TagPath h3Obj;
                    try
                    {
                        h3Obj = utilsInstance.weaponMapping[h2Path];
                    }
                    catch (KeyNotFoundException)
                    {
                        loadingForm.UpdateOutputBox($"H3 equivalent not found for weapon \"{h2Path}\", using H2 tag path.", false);
                        h3Obj = TagPath.FromPathAndExtension(h2Path, "weapon");
                    }
                    h3ObjPaths.Add(h3Obj);
                }
            }
            else if (paletteType == "vehicle")
            {
                foreach (string h2Path in h2ObjRefs)
                {
                    TagPath h3Obj;
                    try
                    {
                        h3Obj = utilsInstance.vehicleMapping[h2Path];
                    }
                    catch (KeyNotFoundException)
                    {
                        loadingForm.UpdateOutputBox($"H3 equivalent not found for vehicle \"{h2Path}\", using H2 tag path.", false);
                        h3Obj = TagPath.FromPathAndExtension(h2Path, "vehicle");
                    }
                    h3ObjPaths.Add(h3Obj);
                }
            }
            else if (paletteType == "machine")
            {
                // Not gonna bother trying to convert machine types as 99% of the time they have no equivalent
                foreach (string h2TagPath in h2ObjRefs)
                {
                    string modifiedPath = String.Format("halo_2\\{0}", h2TagPath);
                    TagPath h3ObjectPath = TagPath.FromPathAndExtension(modifiedPath, "device_machine");
                    h3ObjPaths.Add(h3ObjectPath);

                    // Create .model and .device_machine tags if requested
                    if (createObjects)
                    {
                        Utils.CreateObjectTags(h3ObjectPath, h3ekPath, h2ekPath, h2TagPath, loadingForm);
                    }
                }
            }
            else if (paletteType == "control")
            {
                // Same goes for controls
                foreach (string h2TagPath in h2ObjRefs)
                {
                    string modifiedPath = String.Format("halo_2\\{0}", h2TagPath);
                    TagPath h3ObjectPath = TagPath.FromPathAndExtension(modifiedPath, "device_control");
                    h3ObjPaths.Add(h3ObjectPath);

                    // Create .model and .device_control tags if requested
                    if (createObjects)
                    {
                        Utils.CreateObjectTags(h3ObjectPath, h3ekPath, h2ekPath, h2TagPath, loadingForm);
                    }
                }
            }
            else if (paletteType == "biped")
            {
                foreach (string h2Path in h2ObjRefs)
                {
                    TagPath h3Obj = TagPath.FromPathAndExtension(h2Path, "biped");
                    h3ObjPaths.Add(h3Obj);
                }
            }
            else if (paletteType == "sound scenery")
            {
                foreach (string h2TagPath in h2ObjRefs)
                {
                    // I want to write sound data to a "sound" folder within the scenario's directory
                    // So we are gonna have to do some file/folder path messing around
                    // Get tags-relative scenario folder path
                    string scenarioFolder = Path.GetDirectoryName(relativeScenPath.RelativePath);
                    // Combine
                    string modifiedPath = Path.Combine(scenarioFolder, h2TagPath);

                    TagPath h3ObjectPath = TagPath.FromPathAndExtension(modifiedPath, "sound_scenery");
                    h3ObjPaths.Add(h3ObjectPath);

                    // Create .sound_scenery tag if requested
                    if (createObjects)
                    {
                        Utils.CreateObjectTags(h3ObjectPath, h3ekPath, h2ekPath, h2TagPath, loadingForm);
                    }
                }
            }
            else if (paletteType == "crate")
            {
                foreach (string h2Path in h2ObjRefs)
                {
                    TagPath h3Obj = TagPath.FromPathAndExtension(h2Path, "crate");
                    h3ObjPaths.Add(h3Obj);
                }
            }
            else if (paletteType == "scenery")
            {
                foreach (string h2Path in h2ObjRefs)
                {
                    TagPath h3Obj = TagPath.FromPathAndExtension(h2Path, "scenery");
                    h3ObjPaths.Add(h3Obj);
                }
            }
            else
            {
                loadingForm.UpdateOutputBox($"Unknown palette type {paletteType}, aborting palette conversion.", false);
            }


            // Now lets write the weapon palette for the H3 scenario
            try
            {
                scenTag.Load(relativeScenPath);
                loadingForm.UpdateOutputBox($"Successfully opened \"{relativeScenPath}\"", false);

                loadingForm.UpdateOutputBox($"Begin writing {paletteType} palette data", false);
                ((TagFieldBlock)scenTag.SelectField($"Block:{paletteType} palette")).RemoveAllElements();
                i = 0;

                foreach (TagPath objPath in h3ObjPaths)
                {
                    ((TagFieldBlock)scenTag.SelectField($"Block:{paletteType} palette")).AddElement();
                    if (paletteType == "character")
                    {
                        ((TagFieldReference)scenTag.SelectField($"Block:{paletteType} palette[{i}]/Reference:reference")).Path = objPath;
                    }
                    else
                    {
                        ((TagFieldReference)scenTag.SelectField($"Block:{paletteType} palette[{i}]/Reference:name")).Path = objPath;
                    }

                    i++;
                }
            }
            catch
            {
                loadingForm.UpdateOutputBox($"Unknown managedblam error! {paletteType} palette data will not have been written correctly!", false);
                return;
            }
            finally
            {
                scenTag.Save();
                scenTag.Dispose();
                loadingForm.UpdateOutputBox($"Finished writing {paletteType} palette data to scenario tag!", false);
            }
        }

        public static Dictionary<string, uint> h3ObjectFlagValues = new Dictionary<string, uint>()
        {
            { "unused", 0 },
            { "not automatically", 1 },
            { "lock type to env. object", 2 },
            { "lock transform to env. object", 4 },
            { "never placed", 8 },
            { "lock name to env. object", 16 },
            { "create at rest", 32 },
            { "store orientations", 64 },
            { "pvs bound", 128 },
            { "startup", 256 },
        };

        public static TPlacement GetObjectDataFromXML<TPlacement>(XmlNode element, Loading loadingForm) where TPlacement : ObjectPlacement, new()
        {
            TPlacement objPlacement = new TPlacement
            {
                TypeIndex = Int32.Parse(element.SelectSingleNode("./block_index[@name='short block index' and @type='type']").Attributes["index"]?.Value),
                NameIndex = Int32.Parse(element.SelectSingleNode("./block_index[@name='short block index' and @type='name']").Attributes["index"]?.Value),
                Position = element.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                Rotation = element.SelectSingleNode("./field[@name='rotation']").InnerText.Trim().Split(',').Select(float.Parse).ToArray(),
                ManualBsp = UInt32.Parse(element.SelectSingleNode("./field[@name='manual bsp flags']").InnerText.Trim().Substring(0, 1)),
                OriginBsp = Int32.Parse(element.SelectSingleNode("./block_index[@name='short block index' and @type='origin bsp index']").Attributes["index"].Value),
                BspPolicy = Int32.Parse(element.SelectSingleNode("./field[@name='bsp policy']").InnerText.Trim().Substring(0, 1))
            };

            // Devices and sound scenery don't have variant fields
            if (!(objPlacement is Device) && !(objPlacement is SoundScenery))
            {
                objPlacement.VarName = element.SelectSingleNode("./field[@name='variant name']").InnerText.Trim();
            }

            // Figure out enum value for object flags
            string[] flagsList = element.SelectSingleNode("./field[@name='placement flags']").InnerText.Trim().Split('\n').Skip(1).Select(s => s.Trim('\t', '\r')).ToArray();
            uint flagEnum = 0;
            foreach (string flagString in flagsList)
            {
                try
                {
                    flagEnum += h3ObjectFlagValues[flagString];
                }
                catch (KeyNotFoundException)
                {
                    loadingForm.UpdateOutputBox($"Couldn't find H3 equivalent for flag {flagString}, ignoring", false);
                }
            }
            objPlacement.Flags = flagEnum;

            // Extra properties specific to certain object types
            if (objPlacement is SpWeapLoc weapon)
            {
                weapon.RoundsLeft = Int32.Parse(element.SelectSingleNode("./field[@name='rounds left']").InnerText.Trim());
                weapon.RoundsLoaded = Int32.Parse(element.SelectSingleNode("./field[@name='rounds loaded']").InnerText.Trim());
                weapon.Scale = float.Parse(element.SelectSingleNode("./field[@name='scale']").InnerText.Trim());
            }
            else if (objPlacement is Scenery scenery)
            {
                scenery.PathfindingType = Int32.Parse(element.SelectSingleNode("./field[@name='Pathfinding policy']").InnerText.Trim().Substring(0, 1));
                scenery.LightmappingType = Int32.Parse(element.SelectSingleNode("./field[@name='Lightmapping policy']").InnerText.Trim().Substring(0, 1));
            }
            else if (objPlacement is Vehicle vehicle)
            {
                vehicle.BodyVitality = float.Parse(element.SelectSingleNode("./field[@name='body vitality']").InnerText.Trim());
            }
            else if (objPlacement is Device device)
            {
                device.PowerGroupIndex = Int32.Parse(element.SelectSingleNode("./block_index[@name='short block index' and @type='power group']").Attributes["index"]?.Value);
                device.PositionGroupIndex = Int32.Parse(element.SelectSingleNode("./block_index[@name='short block index' and @type='position group']").Attributes["index"]?.Value);
                var deviceFlagFields = element.SelectNodes("./field[@name='flags' and @type='long flags']");
                device.DeviceFlags1 = uint.Parse(deviceFlagFields[0].InnerText.Trim().Split('\n').First());
                device.DeviceFlags2 = uint.Parse(deviceFlagFields[1].InnerText.Trim().Split('\n').First());
            }
            else if (objPlacement is Biped biped)
            {
                biped.BodyVitality = float.Parse(element.SelectSingleNode("./field[@name='body vitality']").InnerText.Trim());
                biped.BipedFlags = uint.Parse(element.SelectSingleNode("./field[@name='flags' and @type='long flags']").InnerText.Trim().Split('\n').First());
            }
            else if (objPlacement is SoundScenery sscen)
            {
                sscen.VolumeType = int.Parse(element.SelectSingleNode("./field[@name='volume type']").InnerText.Trim().Substring(0, 1));
                sscen.Height = float.Parse(element.SelectSingleNode("./field[@name='height']").InnerText.Trim());
                sscen.DistBounds = element.SelectSingleNode("./field[@name='override distance bounds']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                sscen.ConeAngleBounds = element.SelectSingleNode("./field[@name='override cone angle bounds']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                sscen.OuterConeGain = float.Parse(element.SelectSingleNode("./field[@name='override outer cone gain']").InnerText.Trim());
            }
            else if (objPlacement is Crate crate) { } // No extra data for crates

            return objPlacement;
        }

        public static Dictionary<string, int> objTypeToIndex = new Dictionary<string, int>()
        {
            { "weapons", 2 },
            { "scenery", 6 },
            { "crates", 10 },
            { "vehicles", 1 },
            { "machines", 7 },
            { "controls", 8 },
            { "bipeds", 0 },
            { "sound scenery", 9 }
        };
        
        public static void WriteObjectData<T>(TagFile tagFile, List<T> allObjPlacements, string type, Loading loadingForm) where T : ObjectPlacement
        {
            loadingForm.UpdateOutputBox($"Begin writing {type} data to scenario tag", false);
            ((TagFieldBlock)tagFile.SelectField($"Block:{type}")).RemoveAllElements();
            int index = 0;

            foreach (ObjectPlacement placement in allObjPlacements)
            {
                ((TagFieldBlock)tagFile.SelectField($"Block:{type}")).AddElement();

                // Common properties
                ((TagFieldBlockIndex)tagFile.SelectField($"Block:{type}[{index}]/ShortBlockIndex:type")).Value = placement.TypeIndex;
                ((TagFieldBlockIndex)tagFile.SelectField($"Block:{type}[{index}]/ShortBlockIndex:name")).Value = placement.NameIndex;
                ((TagFieldFlags)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/Flags:placement flags")).RawValue = placement.Flags;
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/RealPoint3d:position")).Data = placement.Position;
                ((TagFieldElementArraySingle)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/RealEulerAngles3d:rotation")).Data = placement.Rotation;
                ((TagFieldElementSingle)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/Real:scale")).Data = placement.Scale;
                ((TagFieldBlockFlags)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/WordBlockFlags:manual bsp flags")).Value = placement.ManualBsp;
                ((TagFieldBlockIndex)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/Struct:object id/ShortBlockIndex:origin bsp index")).Value = placement.OriginBsp;
                ((TagFieldEnum)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/CharEnum:bsp policy")).Value = placement.BspPolicy;
                ((TagFieldEnum)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/Struct:object id/CharEnum:type")).Value = objTypeToIndex[type]; // 2 for weapon
                ((TagFieldEnum)tagFile.SelectField($"Block:{type}[{index}]/Struct:object data/Struct:object id/CharEnum:source")).Value = 1; // 1 for editor

                if (!(placement is Device) && !(placement is SoundScenery)) // Devices and sound scenery don't have variant info from H2
                {
                    ((TagFieldElementStringID)tagFile.SelectField($"Block:{type}[{index}]/Struct:permutation data/StringId:variant name")).Data = placement.VarName;
                }

                // SP weapon-specific properties
                if (placement is SpWeapLoc weapon)
                {
                    ((TagFieldElementInteger)tagFile.SelectField($"Block:weapons[{index}]/Struct:weapon data/ShortInteger:rounds left")).Data = weapon.RoundsLeft;
                    ((TagFieldElementInteger)tagFile.SelectField($"Block:weapons[{index}]/Struct:weapon data/ShortInteger:rounds loaded")).Data = weapon.RoundsLoaded;
                }
                // Scenery-specific properties
                else if (placement is Scenery scenery)
                {
                    ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{index}]/Struct:scenery data/ShortEnum:Pathfinding policy")).Value = scenery.PathfindingType;
                    ((TagFieldEnum)tagFile.SelectField($"Block:scenery[{index}]/Struct:scenery data/ShortEnum:Lightmapping policy")).Value = scenery.LightmappingType;
                }
                // Vehicle-specific properties
                else if (placement is Vehicle vehicle)
                {
                    ((TagFieldElementSingle)tagFile.SelectField($"Block:vehicles[{index}]/Struct:unit data/Real:body vitality")).Data = vehicle.BodyVitality;
                }
                // Device-specific properties
                else if (placement is Device device)
                {
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:machines[{index}]/Struct:device data/ShortBlockIndex:power group")).Value = device.PowerGroupIndex;
                    ((TagFieldBlockIndex)tagFile.SelectField($"Block:machines[{index}]/Struct:device data/ShortBlockIndex:position group")).Value = device.PositionGroupIndex;
                    ((TagFieldFlags)tagFile.SelectField($"Block:{type}[{index}]/Struct:device data/Flags:flags")).RawValue = device.DeviceFlags1;

                    // Machines and controls have different struct names
                    if (type == "machines")
                    {
                        ((TagFieldFlags)tagFile.SelectField($"Block:{type}[{index}]/Struct:machine data/Flags:flags")).RawValue = device.DeviceFlags2;
                    }
                    else
                    {
                        ((TagFieldFlags)tagFile.SelectField($"Block:{type}[{index}]/Struct:control data/Flags:flags")).RawValue = device.DeviceFlags2;
                    }
                }
                // Biped-specific properties
                else if (placement is Biped biped)
                {
                    ((TagFieldElementSingle)tagFile.SelectField($"Block:bipeds[{index}]/Struct:unit data/Real:body vitality")).Data = biped.BodyVitality;
                    ((TagFieldFlags)tagFile.SelectField($"Block:bipeds[{index}]/Struct:unit data/Flags:flags")).RawValue = biped.BipedFlags;
                }
                // Sound scenery-specific properties
                else if (placement is SoundScenery sscen)
                {
                    ((TagFieldEnum)tagFile.SelectField($"Block:sound scenery[{index}]/Struct:sound_scenery/LongEnum:volume type")).Value = sscen.VolumeType;
                    ((TagFieldElementSingle)tagFile.SelectField($"Block:sound scenery[{index}]/Struct:sound_scenery/Real:height")).Data = sscen.Height;
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:sound scenery[{index}]/Struct:sound_scenery/RealBounds:override distance bounds")).Data = sscen.DistBounds;
                    ((TagFieldElementArraySingle)tagFile.SelectField($"Block:sound scenery[{index}]/Struct:sound_scenery/AngleBounds:override cone angle bounds")).Data = sscen.ConeAngleBounds;
                    ((TagFieldElementSingle)tagFile.SelectField($"Block:sound scenery[{index}]/Struct:sound_scenery/Real:override outer cone gain")).Data = sscen.OuterConeGain;
                }
                else if (placement is Crate crate) { }

                index++;
            }
            loadingForm.UpdateOutputBox($"Finished writing {type} data to scenario tag!", false);
        }
        
        public static void CreateObjectTags(TagPath objectTagPath, string h3ekPath, string h2ekPath, string relativeH2TagPath, Loading loadingForm)
        {
            string fullTagPath = Path.Combine(h3ekPath, "tags\\", objectTagPath.RelativePathWithExtension);
            if (!File.Exists(fullTagPath))
            {
                loadingForm.UpdateOutputBox($"Creating tags for object \"{objectTagPath.RelativePathWithExtension}\"", false);

                // First we need to determine the bounding radius so that the object will actually be visible
                // Build full H2 tag path
                string fullH2TagPath = Path.ChangeExtension(Path.Combine(h2ekPath, "tags", relativeH2TagPath), objectTagPath.Extension);

                // Read tag as bytes. Bounding radius is stored as a 4-byte single-precision float at 0x54-0x57
                float boundingRadius = 1.0f; // Default to 1.0
                using (FileStream fs = new FileStream(fullH2TagPath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        fs.Seek(0x54, SeekOrigin.Begin); // Jump to 0x54
                        boundingRadius = reader.ReadSingle();
                        loadingForm.UpdateOutputBox($"Bounding radius of {objectTagPath}: {boundingRadius}", false);
                    }
                }

                // Now, let's try to get the render model, collision, physics for the .model tag
                string[] modelTagReferences = ObjectPorter.GetTagsForModel(objectTagPath, h2ekPath, h3ekPath, relativeH2TagPath, loadingForm);

                TagPath referenceTagPath;
                
                // Create .model tag
                if (objectTagPath.Extension != "sound_scenery")
                {
                    referenceTagPath = TagPath.FromPathAndType(objectTagPath.RelativePath, "hlmt*");
                    TagFile modelTag = new TagFile();
                    modelTag.New(referenceTagPath);

                    foreach (string reference in modelTagReferences)
                    {
                        switch (Path.GetExtension(reference))
                        {
                            case ".render_model":
                                ((TagFieldReference)modelTag.SelectField("Reference:render model")).Path = TagPath.FromPathAndExtension(Path.Combine(Path.GetDirectoryName(reference), Path.GetFileNameWithoutExtension(reference)), "render_model");
                                break;
                            case ".collision_model":
                                ((TagFieldReference)modelTag.SelectField("Reference:collision model")).Path = TagPath.FromPathAndExtension(Path.Combine(Path.GetDirectoryName(reference), Path.GetFileNameWithoutExtension(reference)), "collision_model");
                                break;
                            case ".physics_model":
                                ((TagFieldReference)modelTag.SelectField("Reference:physics_model")).Path = TagPath.FromPathAndExtension(Path.Combine(Path.GetDirectoryName(reference), Path.GetFileNameWithoutExtension(reference)), "physics_model");
                                break;
                        }
                    }

                    ((TagFieldEnum)modelTag.SelectField("CharEnum:PRT shadow detail")).Value = 2;
                    ((TagFieldEnum)modelTag.SelectField("CharEnum:PRT shadow bounces")).Value = 2;

                    modelTag.Save();
                }
                // Create sound_looping tag
                else
                {
                    referenceTagPath = TagPath.FromPathAndExtension(objectTagPath.RelativePath, "sound_looping");
                    TagFile loopTag = new TagFile();
                    loopTag.New(referenceTagPath);
                    loopTag.Save();
                }

                // Create top-level object tag
                TagFile objectTag = new TagFile();
                objectTag.New(objectTagPath);

                // Set .model or .sound_looping reference
                switch (objectTagPath.Extension)
                {
                    case "scenery":
                    case "crate":
                        ((TagFieldReference)objectTag.SelectField("Struct:object[0]/Reference:model")).Path = referenceTagPath;
                        ((TagFieldElementSingle)objectTag.SelectField("Struct:object[0]/Real:bounding radius")).Data = boundingRadius;
                        break;

                    case "device_machine": 
                    case "device_control":
                        ((TagFieldReference)objectTag.SelectField("Struct:device[0]/Struct:object[0]/Reference:model")).Path = referenceTagPath;
                        ((TagFieldElementSingle)objectTag.SelectField("Struct:device[0]/Struct:object[0]/Real:bounding radius")).Data = boundingRadius;
                        break;

                    case "sound_scenery":
                        ((TagFieldEnum)objectTag.SelectField("Struct:object[0]/CharEnum:sweetener size")).Value = 1;
                        ((TagFieldBlock)objectTag.SelectField("Struct:object[0]/Block:attachments")).AddElement();
                        ((TagFieldReference)objectTag.SelectField("Struct:object[0]/Block:attachments[0]/Reference:type")).Path = referenceTagPath;
                        ((TagFieldElementSingle)objectTag.SelectField("Struct:object[0]/Real:bounding radius")).Data = boundingRadius;
                        break;
                }

                objectTag.Save();
            }
            else
            {
                loadingForm.UpdateOutputBox($"Tag \"{objectTagPath.RelativePathWithExtension}\" already exists! Skipping tag creation...", false);
            }
        }
        
        // Dictionaries that use Bungie stuff cant be static
        public Dictionary<string, TagPath> characterMapping = new Dictionary<string, TagPath>()
        {
            // Marines
            { "objects\\characters\\marine\\ai\\marine", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine", "character") },
            { "objects\\characters\\marine\\ai\\marine_female", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_female", "character") },
            { "objects\\characters\\marine\\ai\\marine_johnson", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_johnson", "character") },
            { "objects\\characters\\marine\\ai\\marine_odst", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_odst", "character") },
            { "objects\\characters\\marine\\ai\\marine_odst_sgt", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_odst_sgt", "character") },
            { "objects\\characters\\marine\\ai\\marine_pilot", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_pilot", "character") },
            { "objects\\characters\\marine\\ai\\marine_sgt", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_sgt", "character") },
            { "objects\\characters\\marine\\ai\\marine_wounded", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_wounded", "character") },
            { "objects\\characters\\marine\\ai\\naval_officer", TagPath.FromPathAndExtension("objects\\characters\\marine\\ai\\marine_naval_officer", "character") },

            // Grunts
            { "objects\\characters\\grunt\\ai\\grunt", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 grunt", "character") },
            { "objects\\characters\\grunt\\ai\\grunt_heavy", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 grunt_heavy", "character") },
            { "objects\\characters\\grunt\\ai\\grunt_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 grunt_major", "character") },
            { "objects\\characters\\grunt\\ai\\grunt_specops", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 spec_ops", "character") },
            { "objects\\characters\\grunt\\ai\\grunt_ultra", TagPath.FromPathAndExtension("h2\\objects\\characters\\grunt\\ai\\h2 grunt_ultra", "character") },

            // Jackals
            { "objects\\characters\\jackal\\ai\\jackal", TagPath.FromPathAndExtension("h2\\objects\\characters\\jackal\\ai\\h2_jackal", "character") },
            { "objects\\characters\\jackal\\ai\\jackal_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\jackal\\ai\\h2_jackal_major", "character") },
            { "objects\\characters\\jackal\\ai\\jackal_sniper", TagPath.FromPathAndExtension("h2\\objects\\characters\\jackal\\ai\\h2_jackal_sniper", "character") },

            // Bugger
            { "objects\\characters\\bugger\\ai\\bugger", TagPath.FromPathAndExtension("h2\\objects\\characters\\bugger\\ai\\h2_bugger", "character") },

            // Hunter
            { "objects\\characters\\hunter\\ai\\hunter", TagPath.FromPathAndExtension("h2\\objects\\characters\\hunter\\ai\\h2_hunter", "character") },

            // Elites
            { "objects\\characters\\elite\\ai\\elite", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\h2 elite", "character") },
            { "objects\\characters\\elite\\ai\\elite_councilor", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_councilor", "character") },
            { "objects\\characters\\elite\\ai\\elite_honor_guard", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_honor_guard", "character") },
            { "objects\\characters\\elite\\ai\\elite_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\h2 elite_major", "character") },
            { "objects\\characters\\elite\\ai\\elite_ranger", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_ranger", "character") },
            { "objects\\characters\\elite\\ai\\elite_specops", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\h2 elite_specops", "character") },
            { "objects\\characters\\elite\\ai\\elite_specops_commander", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_specops_commander", "character") },
            { "objects\\characters\\elite\\ai\\elite_stealth", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_stealth", "character") },
            { "objects\\characters\\elite\\ai\\elite_stealth_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_stealth_major", "character") },
            { "objects\\characters\\elite\\ai\\elite_ultra", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_ultra", "character") },
            { "objects\\characters\\elite\\ai\\elite_zealot", TagPath.FromPathAndExtension("h2\\objects\\characters\\elites\\ai\\elite_zealot", "character") },

            // Brutes
            { "objects\\characters\\brute\\ai\\brute", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\h2 brute", "character") },
            { "objects\\characters\\brute\\ai\\brute_captain", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\h2 brute_captain", "character") },
            { "objects\\characters\\brute\\ai\\brute_honor_guard", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\h2 brute_honor_guard", "character") },
            { "objects\\characters\\brute\\ai\\brute_major", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\h2 brute_major", "character") },
            { "objects\\characters\\brute\\ai\\brute_tartarus", TagPath.FromPathAndExtension("h2\\objects\\characters\\brute\\ai\\tartarus", "character") },

            // Sentinels
            { "objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor", "character") },
            { "objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor_eliminator", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor", "character") },
            { "objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor_halo1", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor", "character") },
            { "objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor_major", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\ai\\sentinel_aggressor_major", "character") },

            // Flood
            { "objects\\characters\\floodcombat_elite\\ai\\floodcombat_elite", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\floodcombat_elite_shush\\ai\\floodcombat_elite_shush", "character") },
            { "objects\\characters\\floodcombat_elite\\ai\\floodcombat_elite_shielded", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\floodcombat_elite_shush\\ai\\floodcombat_elite_shielded_shush", "character") },
            { "objects\\characters\\floodcarrier\\ai\\flood_carrier", TagPath.FromPathAndExtension("objects\\characters\\floodcarrier\\ai\\flood_carrier", "character") },
            { "objects\\characters\\flood_infection\\ai\\flood_infection", TagPath.FromPathAndExtension("objects\\characters\\flood_infection\\ai\\flood_infection", "character") },
            { "objects\\characters\\floodcombat_human\\ai\\flood_combat_human", TagPath.FromPathAndExtension("objects\\characters\\floodcombat_human\\ai\\flood_combat_human", "character") },
        
            // HERETICS, HERETICS!
            { "objects\\characters\\heretic\\ai\\heretic", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\heretic\\ai\\heretic", "character") },
            { "objects\\characters\\heretic\\ai\\heretic_leader", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\heretic\\ai\\heretic_leader", "character") },
            { "objects\\characters\\heretic\\ai\\heretic_leader_hologram", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\heretic\\ai\\heretic_leader_hologram", "character") },
            { "objects\\characters\\heretic\\ai\\heretic_major", TagPath.FromPathAndExtension("halo_2\\objects\\characters\\heretic\\ai\\heretic_major", "character") },
            { "objects\\characters\\heretic_grunt\\ai\\heretic_grunt", TagPath.FromPathAndExtension("objects\\characters\\heretic_grunt\\ai\\heretic_grunt", "character") },
        };

        public Dictionary<string, TagPath> weaponMapping = new Dictionary<string, TagPath>()
        {
            // Fixed
            { "objects\\weapons\\fixed\\plasma_cannon\\plasma_cannon", TagPath.FromPathAndExtension("objects\\weapons\\turret\\plasma_cannon\\plasma_cannon", "weapon") },
            { "objects\\weapons\\fixed\\plasma_cannon\\item\\c_turret_mp_item", TagPath.FromPathAndExtension("objects\\weapons\\turret\\plasma_cannon\\plasma_cannon", "weapon") },

            // Melee
            { "objects\\weapons\\melee\\energy_blade\\energy_blade", TagPath.FromPathAndExtension("objects\\weapons\\melee\\energy_blade\\energy_blade", "weapon") },
            { "objects\\weapons\\melee\\gravity_hammer\\gravity_hammer", TagPath.FromPathAndExtension("objects\\weapons\\melee\\gravity_hammer\\gravity_hammer", "weapon") },
            
            // Multiplayer
            { "objects\\weapons\\multiplayer\\ball\\head_sp", TagPath.FromPathAndExtension("objects\\weapons\\multiplayer\\ball\\primary_skull", "weapon") },
        
            // Pistols
            { "objects\\weapons\\pistol\\magnum\\magnum", TagPath.FromPathAndExtension("objects\\weapons\\pistol\\magnum\\magnum", "weapon") },
            { "objects\\weapons\\pistol\\needler\\needler", TagPath.FromPathAndExtension("objects\\weapons\\pistol\\needler\\needler", "weapon") },
            { "objects\\weapons\\pistol\\plasma_pistol\\plasma_pistol", TagPath.FromPathAndExtension("objects\\weapons\\pistol\\plasma_pistol\\plasma_pistol", "weapon") },
        
            // Rifles
            { "objects\\weapons\\rifle\\battle_rifle\\battle_rifle", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\battle_rifle\\battle_rifle", "weapon") },
            { "objects\\weapons\\rifle\\beam_rifle\\beam_rifle", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\beam_rifle\\beam_rifle", "weapon") },
            { "objects\\weapons\\rifle\\brute_plasma_rifle\\brute_plasma_rifle", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\plasma_rifle_red\\plasma_rifle_red", "weapon") },
            { "objects\\weapons\\rifle\\covenant_carbine\\covenant_carbine", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\covenant_carbine\\covenant_carbine", "weapon") },
            { "objects\\weapons\\rifle\\plasma_rifle\\plasma_rifle", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\plasma_rifle\\plasma_rifle", "weapon") },
            { "objects\\weapons\\rifle\\shotgun\\shotgun", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\shotgun\\shotgun", "weapon") },
            { "objects\\weapons\\rifle\\smg\\smg", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\smg\\smg", "weapon") },
            { "objects\\weapons\\rifle\\smg_silenced\\smg_silenced", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\smg_silenced\\smg_silenced", "weapon") },
            { "objects\\weapons\\rifle\\sniper_rifle\\sniper_rifle", TagPath.FromPathAndExtension("objects\\weapons\\rifle\\sniper_rifle\\sniper_rifle", "weapon") },
        
            // Support high
            { "objects\\weapons\\support_high\\flak_cannon\\flak_cannon", TagPath.FromPathAndExtension("objects\\weapons\\support_high\\flak_cannon\\flak_cannon", "weapon") },
            { "objects\\weapons\\support_high\\rocket_launcher\\rocket_launcher", TagPath.FromPathAndExtension("objects\\weapons\\support_high\\rocket_launcher\\rocket_launcher", "weapon") },

            // Support low
            { "objects\\weapons\\support_low\\brute_shot\\brute_shot", TagPath.FromPathAndExtension("objects\\weapons\\support_low\\brute_shot\\brute_shot", "weapon") },

            // Sentinel-specific stuff
            { "objects\\characters\\sentinel_aggressor\\weapons\\welder\\sentinel_aggressor_welder", TagPath.FromPathAndExtension("objects\\characters\\sentinel_aggressor\\weapons\\welder\\sentinel_aggressor_welder", "weapon") },
            { "objects\\characters\\sentinel_aggressor\\weapons\\beam\\sentinel_aggressor_beam", TagPath.FromPathAndExtension("objects\\weapons\\support_low\\sentinel_gun\\sentinel_gun", "weapon") },
        
            // Misc
            { "objects\\vehicles\\c_turret_ap\\weapon\\big_needler_handheld", TagPath.FromPathAndExtension("halo_2\\objects\\vehicles\\c_turret_ap\\weapon\\big_needler_handheld", "weapon") },
            // This is temporary - H3 jackals have the shield built in
            { "objects\\weapons\\melee\\jackal_shield\\jackal_shield", TagPath.FromPathAndExtension("objects\\weapons\\multiplayer\\flag\\flag", "weapon") },
        };

        public Dictionary<string, TagPath> vehicleMapping = new Dictionary<string, TagPath>()
        {
            { "objects\\vehicles\\banshee\\banshee", TagPath.FromPathAndExtension("objects\\vehicles\\banshee\\banshee", "vehicle") },
            { "objects\\vehicles\\c_turret_ap\\c_turret_ap", TagPath.FromPathAndExtension("h2\\objects\\vehicles\\c_turret_ap\\c_turret_ap", "vehicle") },
            { "objects\\vehicles\\creep\\creep", TagPath.FromPathAndExtension("halo_2\\objects\\vehicles\\shadow\\shadow_h2_other", "vehicle") },
            { "objects\\vehicles\\ghost\\ghost", TagPath.FromPathAndExtension("objects\\vehicles\\ghost\\ghost", "vehicle") },
            { "objects\\vehicles\\h_turret_ap\\h_turret_ap", TagPath.FromPathAndExtension("objects\\weapons\\turret\\machinegun_turret\\machinegun_turret", "vehicle") },
            { "objects\\vehicles\\insertion_pod\\insertion_pod", TagPath.FromPathAndExtension("objects\\vehicles\\insertion_pod\\insertion_pod", "vehicle") },
            { "objects\\vehicles\\pelican\\pelican", TagPath.FromPathAndExtension("halo_2\\objects\\vehicles\\pelican\\pelican", "vehicle") },
            { "objects\\vehicles\\phantom\\phantom", TagPath.FromPathAndExtension("halo_2\\objects\\vehicles\\phantom\\h2_phantom", "vehicle") },
            { "objects\\vehicles\\scorpion\\scorpion", TagPath.FromPathAndExtension("objects\\vehicles\\scorpion\\scorpion", "vehicle") },
            { "objects\\vehicles\\spectre\\spectre", TagPath.FromPathAndExtension("objects\\vehicles\\spectre\\spectre", "vehicle") },
            { "objects\\vehicles\\warthog\\warthog", TagPath.FromPathAndExtension("objects\\vehicles\\warthog\\warthog", "vehicle") },
            { "objects\\vehicles\\warthog\\warthog_rocket", TagPath.FromPathAndExtension("objects\\vehicles\\warthog\\warthog", "vehicle") },
            { "objects\\vehicles\\wraith\\wraith", TagPath.FromPathAndExtension("objects\\vehicles\\wraith\\wraith", "vehicle") },
        };

        public Dictionary<string, TagPath> mpWeapMapping = new Dictionary<string, TagPath>
        {
            {"frag_grenades", TagPath.FromPathAndType(@"objects\weapons\grenade\frag_grenade\frag_grenade", "eqip*")},
            {"plasma_grenades", TagPath.FromPathAndType(@"objects\weapons\grenade\plasma_grenade\plasma_grenade", "eqip*")},
            {"energy_sword", TagPath.FromPathAndType(@"objects\weapons\melee\energy_blade\energy_blade", "weap*")},
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

        public Dictionary<string, TagPath> netflagMapping = new Dictionary<string, TagPath>
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

        public Dictionary<string, TagPath> netVehiMapping = new Dictionary<string, TagPath>
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
    }
}
