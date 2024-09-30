using Bungie;
using Bungie.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace H2_H3_Converter_UI
{
    public class FlightHint
    {
        public List<float[]> Points { get; set; }
    }

    public class JumpHint
    {
        public string Flags { get; set; }
        public string ParallelIndex { get; set; }
        public string JumpHeight { get; set; }
    }

    public class Parallelogram
    {
        public string Flags { get; set; }
        public string Point0 { get; set; }
        public string Point0Ref { get; set; }
        public string Point1 { get; set; }
        public string Point1Ref { get; set; }
        public string Point2 { get; set; }
        public string Point2Ref { get; set; }
        public string Point3 { get; set; }
        public string Point3Ref { get; set; }
    }

    // At some point I should really redo these names, this is not good for readability

    public class BspJumpHints
    {
        public List<JumpHint> jumpHints { get; set; }
    }

    public class BspParallelos
    {
        public List<Parallelogram> parallelograms { get; set;}
    }

    public class ScenarioHints
    {
        public List<BspJumpHints> scenarioHints { get; set; }
    }

    public class ScenarioParallelos
    {
        public List<BspParallelos> scenarioParallelos { get; set; }
    }

    public class HintConverter
    {
        public static XmlDocument HintsToXML(string scenPath, string xmlPath, Loading loadingForm)
        {
            // Make sure we have a scenario backup
            Utils.BackupScenario(scenPath, loadingForm);

            string newFilePath = Utils.ConvertXML(xmlPath, loadingForm);
            XmlDocument scenfile = new XmlDocument();
            scenfile.Load(newFilePath);

            loadingForm.UpdateOutputBox("Modified XML file loaded successfully.", false);

            XmlNode root = scenfile.DocumentElement;

            XmlNodeList aiPathDataBlock = root.SelectNodes(".//block[@name='ai pathfinding data']");
            loadingForm.UpdateOutputBox("Located AI pathfinding data block.", false);

            ScenarioHints scenarioHints = new ScenarioHints();
            ScenarioParallelos scenarioParallelos = new ScenarioParallelos();
            List<BspJumpHints> bspsJH = new List<BspJumpHints>();
            List<BspParallelos> bspsP = new List<BspParallelos>();
            List<List<FlightHint>> scenarioFlightHints = new List<List<FlightHint>>();
            foreach (XmlNode aiPathfinding in aiPathDataBlock)
            {
                // Each BSP has its own element
                bool pathDataEnd = false;
                int i = 0;
                while (!pathDataEnd)
                {
                    XmlNode bsp = aiPathfinding.SelectSingleNode("./element[@index='" + i + "']");
                    if (bsp != null)
                    {
                        loadingForm.UpdateOutputBox($"Reading pathfinding data for BSP index {i}.", false);
                        BspJumpHints bspHints = new BspJumpHints();
                        List<JumpHint> jumpHints = new List<JumpHint>();
                        XmlNode jumpHintsBlock = bsp.SelectSingleNode(".//block[@name='user-placed hints']/element[@index='0']/block[@name='jump hints']");

                        // Loop over all jump hints
                        if (jumpHintsBlock != null)
                        {
                            int j = 0;
                            XmlNodeList jumpHintElements = jumpHintsBlock.SelectNodes("./element");
                            foreach (XmlNode jumpHint in jumpHintElements)
                            {
                                JumpHint hint = new JumpHint
                                {
                                    Flags = jumpHint.SelectSingleNode("./field[@name='Flags']").InnerText.Trim(),
                                    ParallelIndex = jumpHint.SelectSingleNode("./block_index[@name='short block index']").Attributes["index"]?.Value,
                                    JumpHeight = jumpHint.SelectSingleNode("./field[@name='force jump height']").InnerText.Trim()
                                };
                                jumpHints.Add(hint);
                                loadingForm.UpdateOutputBox($"Read data for BSP {i}, jump hint {j}.", false);
                                j++;
                            }
                            bspHints.jumpHints = jumpHints;
                        }

                        BspParallelos bspParallelos = new BspParallelos();
                        List<Parallelogram> parallelograms = new List<Parallelogram>();
                        XmlNode parallelosBlock = bsp.SelectSingleNode(".//block[@name='user-placed hints']/element[@index='0']/block[@name='parallelogram geometry']");

                        // Loop over all parallelogram geometry data
                        if (parallelosBlock != null)
                        {
                            int j = 0;
                            XmlNodeList parallelogramElements = parallelosBlock.SelectNodes("./element");
                            foreach (XmlNode para in parallelogramElements)
                            {
                                Parallelogram parallelogram = new Parallelogram();
                                XmlNodeList referenceFields = para.SelectNodes("./field[@name='reference frame']");

                                parallelogram.Flags = para.SelectSingleNode("./field[@name='Flags']").InnerText.Trim();
                                parallelogram.Point0 = para.SelectSingleNode("./field[@name='Point 0']").InnerText.Trim();
                                parallelogram.Point0Ref = referenceFields[0].InnerText.Trim();
                                parallelogram.Point1 = para.SelectSingleNode("./field[@name='Point 1']").InnerText.Trim();
                                parallelogram.Point1Ref = referenceFields[1].InnerText.Trim();
                                parallelogram.Point2 = para.SelectSingleNode("./field[@name='Point 2']").InnerText.Trim();
                                parallelogram.Point2Ref = referenceFields[2].InnerText.Trim();
                                parallelogram.Point3 = para.SelectSingleNode("./field[@name='Point 3']").InnerText.Trim();
                                parallelogram.Point3Ref = referenceFields[3].InnerText.Trim();

                                parallelograms.Add(parallelogram);
                                loadingForm.UpdateOutputBox($"Read data for BSP {i}, parallelogram {j}.", false);
                                j++;
                            }
                            bspParallelos.parallelograms = parallelograms;
                        }

                        bspsJH.Add(bspHints);
                        bspsP.Add(bspParallelos);

                        // Flight hint section
                        List<FlightHint> allFlightHintsForBsp = new List<FlightHint>();
                        XmlNode flightHintsBlock = bsp.SelectSingleNode(".//block[@name='user-placed hints']/element[@index='0']/block[@name='flight hints']");

                        if (flightHintsBlock != null)
                        {
                            XmlNodeList flightHintElements = flightHintsBlock.SelectNodes("./element");
                            int j = 0;
                            foreach (XmlNode flightHintEntry in flightHintElements)
                            {
                                FlightHint flightHint = new FlightHint();
                                List<float[]> flightHintPoints = new List<float[]>();
                                XmlNodeList flightHintPointElements = flightHintEntry.SelectNodes($"./block[@name='points']/element/field[@name='point']");
                                foreach (XmlNode point in flightHintPointElements)
                                {
                                    flightHintPoints.Add(point.InnerText.Trim().Split(',').Select(float.Parse).ToArray());
                                }
                                flightHint.Points = flightHintPoints;
                                allFlightHintsForBsp.Add(flightHint);
                                j++;
                            }
                        }

                        scenarioFlightHints.Add(allFlightHintsForBsp);
                        loadingForm.UpdateOutputBox($"Successfully read hint data for BSP index {i}.", false);
                        i++;
                    }
                    else
                    {
                        pathDataEnd = true;
                        loadingForm.UpdateOutputBox("Finished processing bsp pathfinding data.", false);
                    }
                }
            }
            scenarioHints.scenarioHints = bspsJH;
            scenarioParallelos.scenarioParallelos = bspsP;
            loadingForm.UpdateOutputBox($"Successfully read hint data for all BSPs.", false);
            HintstoH3(scenPath, loadingForm, scenarioHints, scenarioParallelos, scenarioFlightHints);
            return scenfile;
        }

        public static void HintstoH3(string scenPath, Loading loadingForm, ScenarioHints scenarioHintsContainer, ScenarioParallelos scenarioParallelosContainer, List<List<FlightHint>> scenarioFlightHints)
        {
            string h3ek_path = scenPath.Substring(0, scenPath.IndexOf("H3EK") + "H3EK".Length);
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek_path);
            var relativeScenPath = TagPath.FromPathAndType(Path.ChangeExtension(scenPath.Split(new[] { "\\tags\\" }, StringSplitOptions.None).Last(), null).Replace('\\', Path.DirectorySeparatorChar), "scnr*");
            TagFile scenTag = new TagFile();

            try
            {
                scenTag.Load(relativeScenPath);
                loadingForm.UpdateOutputBox($"Successfully opened \"{relativeScenPath}\"", false);

                // Write parallelogram geometry data before jump hints to make sure they can reference something
                int bspIndex = 0;

                // Make sure hint data block exists
                if (((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data")).Elements.Count() == 0)
                {
                    ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data")).AddElement();
                }

                ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry")).RemoveAllElements();
                foreach (BspParallelos bsp in scenarioParallelosContainer.scenarioParallelos)
                {
                    loadingForm.UpdateOutputBox($"Start writing parallelogram geometry data for bsp {bspIndex}", false);

                    // Loops over every parallelogram entry and writes it to tag
                    int i = 0;
                    if (bsp.parallelograms != null)
                    {
                        foreach (Parallelogram para in bsp.parallelograms)
                        {
                            loadingForm.UpdateOutputBox($"Parallelogram {i}", false);

                            int parallelCount = ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry")).Count();
                            ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry")).AddElement();

                            // Flags
                            ((TagFieldFlags)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/Flags:Flags")).RawValue = UInt32.Parse(para.Flags.Substring(0, 1));

                            // Point 0
                            ((TagFieldElementArraySingle)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/RealPoint3d:Point 0")).Data = para.Point0.Split(',').Select(float.Parse).ToArray();
                            ((TagFieldBlockIndex)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/ShortBlockIndex:structure bsp 0")).Value = bspIndex;

                            // Point 1
                            ((TagFieldElementArraySingle)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/RealPoint3d:Point 1")).Data = para.Point1.Split(',').Select(float.Parse).ToArray();
                            ((TagFieldBlockIndex)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/ShortBlockIndex:structure bsp 1")).Value = bspIndex;

                            // Point 2
                            ((TagFieldElementArraySingle)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/RealPoint3d:Point 2")).Data = para.Point2.Split(',').Select(float.Parse).ToArray();
                            ((TagFieldBlockIndex)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/ShortBlockIndex:structure bsp 2")).Value = bspIndex;

                            // Point 3
                            ((TagFieldElementArraySingle)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/RealPoint3d:Point 3")).Data = para.Point3.Split(',').Select(float.Parse).ToArray();
                            ((TagFieldBlockIndex)scenTag.SelectField($"Block:ai user hint data[0]/Block:parallelogram geometry[{parallelCount}]/ShortBlockIndex:structure bsp 3")).Value = bspIndex;

                            i++;
                        }
                    }

                    bspIndex++;
                }

                loadingForm.UpdateOutputBox($"Finished writing parallelogram geometry data", false);

                // Time to write the jump hint data
                bspIndex = 0;
                int hintStartIndex = 0;
                ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:jump hints")).RemoveAllElements();
                foreach (BspJumpHints bsp in scenarioHintsContainer.scenarioHints)
                {
                    loadingForm.UpdateOutputBox($"Start writing jump hint data for bsp {bspIndex}", false);

                    // Loops over every parallelogram entry and writes it to tag
                    int i = 0;
                    if (bsp.jumpHints != null)
                    {
                        foreach (JumpHint jHint in bsp.jumpHints)
                        {
                            loadingForm.UpdateOutputBox($"Jump hint {i}", false);

                            int hintCount = ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:jump hints")).Count();
                            ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:jump hints")).AddElement();

                            // Flags
                            ((TagFieldFlags)scenTag.SelectField($"Block:ai user hint data[0]/Block:jump hints[{hintCount}]/WordFlags:Flags")).RawValue = UInt32.Parse(jHint.Flags.Substring(0, 1));

                            // Geometry index - H2 stores jump hints and their respective parallelograms on a per-bsp basis. H3 does not, so we must account for the previous parallelograms as well
                            ((TagFieldBlockIndex)scenTag.SelectField($"Block:ai user hint data[0]/Block:jump hints[{hintCount}]/ShortBlockIndex:geometry index")).Value = Int32.Parse(jHint.ParallelIndex) + hintStartIndex;

                            // Force jump height
                            string test = jHint.JumpHeight;
                            ((TagFieldEnum)scenTag.SelectField($"Block:ai user hint data[0]/Block:jump hints[{hintCount}]/ShortEnum:force jump height")).Value = Int32.Parse(jHint.JumpHeight.Substring(0, 1));

                            i++;
                        }
                    }

                    hintStartIndex = ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:jump hints")).Count();
                    bspIndex++;
                }

                // Flight hint data
                int hintIndex = 0;
                ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:flight hints")).RemoveAllElements();
                foreach (List<FlightHint> flightHintList in scenarioFlightHints)
                {
                    foreach (FlightHint flightHint in flightHintList)
                    {
                        int pointIndex = 0;
                        ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:flight hints")).AddElement();
                        foreach (float[] point in flightHint.Points)
                        {
                            ((TagFieldBlock)scenTag.SelectField($"Block:ai user hint data[0]/Block:flight hints[{hintIndex}]/Block:points")).AddElement();
                            ((TagFieldElementArraySingle)scenTag.SelectField($"Block:ai user hint data[0]/Block:flight hints[{hintIndex}]/Block:points[{pointIndex}]/RealVector3d:point")).Data = point;

                            pointIndex++;
                        }
                        hintIndex++;
                    }
                }

                loadingForm.UpdateOutputBox($"Finished writing hint data", false);
            }
            catch
            {
                loadingForm.UpdateOutputBox($"Unknown managedblam error! Hint data will not have been written correctly!", false);
                return;
            }
            finally
            {
                scenTag.Save();
                scenTag.Dispose();
                loadingForm.UpdateOutputBox($"Finished writing data to scenario tag!", false);
            }
        }
    }
}
