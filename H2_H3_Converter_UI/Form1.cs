using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace H2_H3_Converter_UI
{
    public partial class form1 : Form
    {
        List<string> bsp_paths = new List<string>();
        string scen_path = "";
        string h2_xml_path = "";
        bool use_existing_tifs = false;
        bool bsps_valid = false;
        bool h3_valid = false;
        bool h2_valid = false;

        public form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        

        private void label1_Click_2(object sender, EventArgs e)
        {
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                bsps_box.Enabled = true;
                bsps_box.BackColor = SystemColors.Window;
                bsp_label.Enabled = true;
                bsp_add.Enabled = true;
                bsp_remove.Enabled = true;
                scenario_label.Enabled = true;
                scen_box.Enabled = true;
                browse_scen.Enabled = true;
                existing_bitmaps.Enabled = true;
                info_label.Enabled = true;
            }
            else
            {
                if (!checkBox2.Checked && !checkBox3.Checked)
                {
                    scenario_label.Enabled = false;
                    scen_box.Enabled = false;
                    browse_scen.Enabled = false;
                }
                bsps_box.Enabled = false;
                bsps_box.BackColor = SystemColors.Control;
                bsp_label.Enabled = false;
                bsp_add.Enabled = false;
                bsp_remove.Enabled = false;
                existing_bitmaps.Enabled = false;
                info_label.Enabled = false;
            }
            update_start_button();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                h2_scen_label.Enabled = true;
                h2_scen_box.Enabled = true;
                browse_scen_h2.Enabled = true;
                scenario_label.Enabled = true;
                scen_box.Enabled = true;
                browse_scen.Enabled = true;
            }
            else
            {
                if (!checkBox1.Checked && !checkBox3.Checked && !checkBox4.Checked)
                {
                    scenario_label.Enabled = false;
                    scen_box.Enabled = false;
                    browse_scen.Enabled = false;
                }
                if (!checkBox3.Checked && !checkBox4.Checked)
                {
                    h2_scen_label.Enabled = false;
                    h2_scen_box.Enabled = false;
                    browse_scen_h2.Enabled = false;
                }
            }
            update_start_button();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                h2_scen_label.Enabled = true;
                h2_scen_box.Enabled = true;
                browse_scen_h2.Enabled = true;
                scenario_label.Enabled = true;
                scen_box.Enabled = true;
                browse_scen.Enabled = true;
            }
            else
            {
                if (!checkBox1.Checked && !checkBox2.Checked && !checkBox4.Checked)
                {
                    scenario_label.Enabled = false;
                    scen_box.Enabled = false;
                    browse_scen.Enabled = false;
                }
                if (!checkBox2.Checked && !checkBox4.Checked)
                {
                    h2_scen_label.Enabled = false;
                    h2_scen_box.Enabled = false;
                    browse_scen_h2.Enabled = false;
                }
            }
            update_start_button();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                h2_scen_label.Enabled = true;
                h2_scen_box.Enabled = true;
                browse_scen_h2.Enabled = true;
                scenario_label.Enabled = true;
                scen_box.Enabled = true;
                browse_scen.Enabled = true;
            }
            else
            {
                if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
                {
                    scenario_label.Enabled = false;
                    scen_box.Enabled = false;
                    browse_scen.Enabled = false;
                }
                if (!checkBox2.Checked && !checkBox3.Checked)
                {
                    h2_scen_label.Enabled = false;
                    h2_scen_box.Enabled = false;
                    browse_scen_h2.Enabled = false;
                }
            }
            update_start_button();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void bsps_box_TextChanged(object sender, EventArgs e)
        {

        }

        private void bsps_box_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void add_bsp_Click(object sender, EventArgs e)
        {

        }

        private void bsp_add_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML Files (*.xml)|*.xml";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string bsp_path = openFileDialog.FileName;
                    if (!bsp_paths.Contains(bsp_path))
                    {
                        if (bsp_path.Contains("H2EK"))
                        {
                            bsp_paths.Add(bsp_path);
                            bsps_box.Items.Add(bsp_path.Split('\\').Last());
                            bsps_valid = true;
                            update_start_button();
                        }
                        else
                        {
                            // BSP not from H2? Alert user, don't add
                            MessageBox.Show("XML doesn't seem to be in the H2EK directory.\nPlease try again.", "Invalid XML path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        
                    }
                    else
                    {
                        // Already added this BSP, alert user and don't add
                        MessageBox.Show("BSP already added!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void bsp_remove_Click(object sender, EventArgs e)
        {
            int selectedIndex = bsps_box.SelectedIndex;
            if (selectedIndex != -1)
            {
                bsps_box.Items.RemoveAt(selectedIndex);
            }
            
            if (bsps_box.Items.Count == 0)
            {
                // No more remaining items
                bsps_valid = false;
                update_start_button();
            }
        }

        private void browse_scen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Scenario Files (*.scenario)|*.scenario";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string h3_scen = openFileDialog.FileName;
                    if (h3_scen.Contains("H3EK"))
                    {
                        scen_path = openFileDialog.FileName;
                        scen_box.Text = openFileDialog.FileName;

                        // Scroll to end
                        scen_box.SelectionStart = scen_box.Text.Length;
                        scen_box.ScrollToCaret();

                        h3_valid = true;
                        update_start_button();
                    }
                    else
                    {
                        // Scenario is not in H3EK, alert user, don't add
                        MessageBox.Show("Scenario doesn't seem to be in the H3EK directory.\nPlease try again.", "Invalid scenario path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void browse_scen_h2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML Files (*.xml)|*.xml";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string xml_file = openFileDialog.FileName;
                    if (xml_file.Contains("H2EK"))
                    {
                        h2_xml_path = openFileDialog.FileName;
                        h2_scen_box.Text = openFileDialog.FileName;

                        // Scroll to end
                        h2_scen_box.SelectionStart = h2_scen_box.Text.Length;
                        h2_scen_box.ScrollToCaret();

                        h2_valid = true;
                        update_start_button();
                    }
                    else
                    {
                        // Scenario XML is not in H2EK, alert user, don't add
                        MessageBox.Show("XML doesn't seem to be in the H2EK directory.\nPlease try again.", "Invalid XML path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void existing_bitmaps_CheckedChanged(object sender, EventArgs e)
        {
            if (existing_bitmaps.Checked)
            {
                use_existing_tifs = true;
            }
            else
            {
                use_existing_tifs = false;
            }
        }

        private void update_start_button()
        {
            if (checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked && !checkBox4.Checked)
            {
                if (bsps_valid && h3_valid)
                {
                    start_button.Enabled = true;
                }
                else
                {
                    start_button.Enabled = false;
                }
            }
            if ((checkBox2.Checked || checkBox3.Checked || checkBox4.Checked) && !checkBox1.Checked)
            {
                if (h2_valid && h3_valid)
                {
                    start_button.Enabled = true;
                }
                else
                {
                    start_button.Enabled = false;
                }
            }
            if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && !checkBox4.Checked)
            {
                if (bsps_valid && h2_valid && h3_valid)
                {
                    start_button.Enabled = true;
                }
                else
                {
                    start_button.Enabled= false;
                }
            }
            if (checkBox1.Checked && (checkBox2.Checked || checkBox3.Checked || checkBox4.Checked))
            {
                if (bsps_valid && h2_valid && h3_valid)
                {
                    start_button.Enabled = true;
                }
                else
                {
                    start_button.Enabled = false;
                }
            }
        }

        private async void start_button_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            using (var loadingForm = new Loading())
            {
                // Show loading
                loadingForm.Show();

                await Task.Run(async () =>
                {
                    // It's go time
                    if (checkBox1.Checked)
                    {
                        // Shader conversion
                        await ShaderConverter.ConvertShaders(bsp_paths, scen_path, use_existing_tifs, loadingForm);
                    }
                    if (checkBox2.Checked)
                    {
                        // Zones conversion
                        MB_Zones.ZoneConverter(scen_path, h2_xml_path, loadingForm);
                    }
                    if (checkBox3.Checked)
                    {
                        // Scenario conversion
                        ScenData.ScenarioConverter(scen_path, h2_xml_path, loadingForm);
                    }
                    if (checkBox4.Checked)
                    {
                        // Hint conversion
                        XmlDocument scenFile = HintConverter.JumpHintsToXML(scen_path, h2_xml_path, loadingForm);

                        // Flags conversion
                        //FlagConverter.ConvertCutsceneFlags(scen_path, h2_xml_path, loadingForm, scenFile);

                        // Point set conversion
                        //ScriptPointConverter.ConvertScriptPoints(scen_path, h2_xml_path, loadingForm, scenFile);

                        // Squads conversion
                        //SquadsConverter.ConvertSquadGroups(scen_path, h2_xml_path, loadingForm, scenFile);
                        SquadsConverter.ConvertSquads(scen_path, h2_xml_path, loadingForm, scenFile);
                    }
                });

                loadingForm.UpdateOutputBox("All conversions complete! Click \"close\" to exit the program.", false);

                loadingForm.Enable_Close();

                // God only knows there must be a better way to do this
                await loadingForm.WaitForCloseAsync();
            }

            // Conversion complete, exit program
            Environment.Exit(0);
        }

        private void info_label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Info info_window = new Info();
            info_window.ShowDialog();
        }
    }
}
