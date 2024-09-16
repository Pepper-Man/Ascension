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
        public List<PointElement> Elements { get; set; }
    }

    public class ScriptPointConverter
    {
        public static void ConvertScriptPoints(string scenPath, string xmlPath, Loading loadingForm)
        {
            // Make sure we have a scenario backup
            Utils.BackupScenario(scenPath, xmlPath, loadingForm);

            loadingForm.UpdateOutputBox("Begin reading scenario script points from XML...", false);
            string newFilePath = Utils.ConvertXML(xmlPath, loadingForm);
            XmlDocument scenfile = new XmlDocument();
            scenfile.Load(newFilePath);

            loadingForm.UpdateOutputBox("Modified XML file loaded successfully.", false);

            XmlNode root = scenfile.DocumentElement;
            XmlNodeList pointSetsParentBlock = root.SelectNodes(".//block[@name='source files']");
            loadingForm.UpdateOutputBox("Located point sets data block.", false);

            List<PointSet> allPointSets = new List<PointSet>();
            bool pointSetDataEnd = false;
            int i = 0;

            while (!pointSetDataEnd)
            {
                XmlNode setEntry = pointSetsParentBlock[0].SelectSingleNode($"./element[@index='0']/block[@name='point sets']/element[@index='{i}']");
                if (setEntry != null)
                {
                    loadingForm.UpdateOutputBox($"Reading data for point set {i}.", false);
                    PointSet set = new PointSet();
                    List<PointElement> allPointsForSet = new List<PointElement>();
                    set.SetName = setEntry.SelectSingleNode($".//field[@name='name']").InnerText.Trim();
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
                    Console.WriteLine(allPointSets.ToString());
                    i++;
                }
                else
                {
                    pointSetDataEnd = true;
                    loadingForm.UpdateOutputBox("Finished processing point set data.", false);
                }
            }

            /*
            // Now for the managedblam stuff
            string h3ek_path = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
            var relativeScenPath = TagPath.FromPathAndType(Path.ChangeExtension(scenPath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
            TagFile scenTag = new TagFile();

            try
            {
                scenTag.Load(relativeScenPath);
                loadingForm.UpdateOutputBox($"Successfully opened \"{relativeScenPath}\"", false);
                ((TagFieldBlock)scenTag.SelectField($"Block:cutscene flags")).RemoveAllElements();

                i = 0;
                foreach (FlagElement flag in allFlags)
                {
                    loadingForm.UpdateOutputBox($"Writing data for cutscene flag {i}", false);
                    ((TagFieldBlock)scenTag.SelectField($"Block:cutscene flags")).AddElement();
                    ((TagFieldElementStringID)scenTag.SelectField($"Block:cutscene flags[{i}]/StringId:name")).Data = flag.Name;
                    ((TagFieldElementArraySingle)scenTag.SelectField($"Block:cutscene flags[{i}]/RealPoint3d:position")).Data = flag.Position;
                    ((TagFieldElementArraySingle)scenTag.SelectField($"Block:cutscene flags[{i}]/RealEulerAngles2d:facing")).Data = flag.Facing;
                    i++;
                }
            }
            catch
            {
                loadingForm.UpdateOutputBox($"Unknown managedblam error! Flag data will not have been written correctly!", false);
                return;
            }
            finally
            {
                scenTag.Save();
                scenTag.Dispose();
                loadingForm.UpdateOutputBox($"Finished writing flag data to scenario tag!", false);
            }
            */
        }
    }
}
