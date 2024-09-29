using Bungie.Tags;
using Bungie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace H2_H3_Converter_UI
{
    class PointElement
    {
        public string Name { get; set; }
        public float[] Position { get; set; }
        public float[] Facing { get; set; }
    }

    class PointSet
    {
        public string SetName { get; set; }
        public int BspIndex { get; set; }
        public List<PointElement> Elements { get; set; }
    }

    public class ScriptPointConverter
    {
        public static void ConvertScriptPoints(string scenPath, string xmlPath, Loading loadingForm, XmlDocument scenfile)
        {
            loadingForm.UpdateOutputBox("Begin reading scenario script points from XML...", false);

            XmlNode root = scenfile.DocumentElement;
            XmlNodeList pointSetsParentBlock = root.SelectNodes(".//block[@name='source files']");
            loadingForm.UpdateOutputBox("Located point sets data block.", false);

            List<PointSet> allPointSets = new List<PointSet>();
            bool pointSetDataEnd = false;
            int i = 0;

            if (pointSetsParentBlock.Count > 0)
            {
                while (!pointSetDataEnd)
                {
                    XmlNode setEntry = pointSetsParentBlock[0].SelectSingleNode($"./element[@index='0']/block[@name='point sets']/element[@index='{i}']");
                    if (setEntry != null)
                    {
                        loadingForm.UpdateOutputBox($"Reading data for point set {i}.", false);
                        PointSet set = new PointSet();
                        List<PointElement> allPointsForSet = new List<PointElement>();
                        set.SetName = setEntry.SelectSingleNode($".//field[@name='name']").InnerText.Trim();
                        set.BspIndex = Int32.Parse(setEntry.SelectSingleNode($".//block_index[@name='short block index']").Attributes["index"]?.Value);
                        XmlNode setPointsBlock = setEntry.SelectSingleNode($".//block[@name='points']");

                        // Loop over all points within current point set
                        if (setPointsBlock != null)
                        {
                            int j = 0;
                            XmlNodeList setPointsElements = setPointsBlock.SelectNodes("./element");
                            foreach (XmlNode point in setPointsElements)
                            {
                                PointElement pointElement = new PointElement();
                                pointElement.Name = point.SelectSingleNode("./field[@name='name']").InnerText.Trim();
                                pointElement.Position = point.SelectSingleNode("./field[@name='position']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();
                                pointElement.Facing = point.SelectSingleNode("./field[@name='facing direction']").InnerText.Trim().Split(',').Select(float.Parse).ToArray();

                                allPointsForSet.Add(pointElement);
                                loadingForm.UpdateOutputBox($"Read data for point set \"{set.SetName}\", point element {j}.", false);
                                j++;
                            }
                        }

                        set.Elements = allPointsForSet;
                        allPointSets.Add(set);
                        i++;
                    }
                    else
                    {
                        pointSetDataEnd = true;
                        loadingForm.UpdateOutputBox("Finished processing point set data.", false);
                    }
                }
            }
            else
            {
                loadingForm.UpdateOutputBox("No point set data!", false);
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

                // Make sure scripting data block exists
                if (((TagFieldBlock)scenTag.SelectField($"Block:scripting data")).Elements.Count() == 0)
                {
                    ((TagFieldBlock)scenTag.SelectField($"Block:scripting data")).AddElement();
                }
                ((TagFieldBlock)scenTag.SelectField($"Block:scripting data[0]/Block:point sets")).RemoveAllElements();

                i = 0;
                foreach (PointSet pointSet in allPointSets)
                {
                    loadingForm.UpdateOutputBox($"Writing data for point set {pointSet.SetName}", false);
                    ((TagFieldBlock)scenTag.SelectField($"Block:scripting data[0]/Block:point sets")).AddElement();
                    ((TagFieldElementString)scenTag.SelectField($"Block:scripting data[0]/Block:point sets[{i}]/String:name")).Data = pointSet.SetName;
                    ((TagFieldBlockIndex)scenTag.SelectField($"Block:scripting data[0]/Block:point sets[{i}]/ShortBlockIndex:bsp index")).Value = pointSet.BspIndex;

                    int j = 0;
                    foreach (PointElement point in pointSet.Elements)
                    {
                        ((TagFieldBlock)scenTag.SelectField($"Block:scripting data[0]/Block:point sets[{i}]/Block:points")).AddElement();
                        ((TagFieldElementString)scenTag.SelectField($"Block:scripting data[0]/Block:point sets[{i}]/Block:points[{j}]/String:name")).Data = point.Name;
                        ((TagFieldElementArraySingle)scenTag.SelectField($"Block:scripting data[0]/Block:point sets[{i}]/Block:points[{j}]/RealPoint3d:position")).Data = point.Position;
                        ((TagFieldElementArraySingle)scenTag.SelectField($"Block:scripting data[0]/Block:point sets[{i}]/Block:points[{j}]/RealEulerAngles2d:facing direction")).Data = point.Facing;
                        ((TagFieldElementInteger)scenTag.SelectField($"Block:scripting data[0]/Block:point sets[{i}]/Block:points[{j}]/ShortInteger:bsp index")).Data = pointSet.BspIndex;
                        ((TagFieldBlockIndex)scenTag.SelectField($"Block:scripting data[0]/Block:point sets[{i}]/Block:points[{j}]/ShortBlockIndex:structure bsp")).Value = pointSet.BspIndex;
                        j++;
                    }

                    i++;
                }
            }
            catch
            {
                loadingForm.UpdateOutputBox($"Unknown managedblam error! Point set data will not have been written correctly!", false);
                return;
            }
            finally
            {
                scenTag.Save();
                scenTag.Dispose();
                loadingForm.UpdateOutputBox($"Finished writing point set data to scenario tag!", false);
            }
        }
    }
}
