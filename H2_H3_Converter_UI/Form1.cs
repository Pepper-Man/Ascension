using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
        string squad_folder_txt_path = "";
        bool use_existing_tifs = false;
        bool bsps_valid = false;
        bool h3_valid = false;
        bool h2_valid = false;
        bool create_object_tags = false;

        public form1()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            versionLabel.Text = $"v{version.Major}.{version.Minor}.{version.Build}";
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
                create_objects.Enabled = true;
                help_create_objects.Enabled = true;
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

                create_objects.Enabled = false;
                help_create_objects.Enabled = false;
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
                use_squad_folder_names.Enabled = true;
                help_squad_folders.Enabled = true;
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
                use_squad_folder_names.Enabled = false;
                squad_folder_names_browse.Enabled = false;
                squad_folder_names_text.Enabled = false;
                help_squad_folders.Enabled = false;
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

        private void use_squad_folder_names_CheckedChanged(object sender, EventArgs e)
        {
            if (use_squad_folder_names.Checked)
            {
                squad_folder_names_browse.Enabled = true;
                squad_folder_names_text.Enabled = true;
            }
            else
            {
                squad_folder_names_browse.Enabled = false;
                squad_folder_names_text.Enabled = false;
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
            bool success = true;
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
                        ShaderConverter.ConvertShaders(bsp_paths, scen_path, use_existing_tifs, loadingForm);
                    }
                    if (checkBox2.Checked)
                    {
                        // Zones conversion
                        if(!MB_Zones.ZoneConverter(scen_path, h2_xml_path, loadingForm))
                        {
                            success = false;
                            return;
                        }
                    }
                    if (checkBox3.Checked)
                    {
                        // Scenario conversion
                        if(!ScenData.ConvertScenarioData(scen_path, h2_xml_path, create_object_tags, loadingForm))
                        {
                            success = false;
                            return;
                        }
                    }
                    if (checkBox4.Checked)
                    {
                        // Hint conversion
                        XmlDocument scenFile = HintConverter.HintsToXML(scen_path, h2_xml_path, loadingForm);

                        if (scenFile == null)
                        {
                            success = false;
                            return;
                        }

                        // Flags conversion
                        FlagConverter.ConvertCutsceneFlags(scen_path, loadingForm, scenFile);

                        // Point set conversion
                        ScriptPointConverter.ConvertScriptPoints(scen_path, loadingForm, scenFile);

                        // Squads conversion
                        Utils.ConvertPalette(scen_path, h2_xml_path, loadingForm, scenFile, "character", create_object_tags);

                        // Only convert weapon and vehicle palettes if scenario data conversion was not already done, since it is done there too
                        if (!checkBox3.Checked)
                        {
                            Utils.ConvertPalette(scen_path, h2_xml_path, loadingForm, scenFile, "weapon", create_object_tags);
                            Utils.ConvertPalette(scen_path, h2_xml_path, loadingForm, scenFile, "vehicle", create_object_tags);
                        }
                        
                        SquadsConverter.ConvertSquadGroups(scen_path, loadingForm, scenFile);
                        SquadsConverter.ConvertSquads(scen_path, loadingForm, scenFile, use_squad_folder_names.Checked);
                    }
                });

                if (success)
                {
                    loadingForm.UpdateOutputBox("All conversions complete! Click \"close\" to exit the program.", false);
                }
                else
                {
                    loadingForm.UpdateOutputBox("A critical error was encountered. Click \"close\" to exit the program.", false);
                }

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

        private void help_squad_folders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SquadFolderHelp squad_folder_info_window = new SquadFolderHelp();
            squad_folder_info_window.ShowDialog();
        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void squad_folder_names_browse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "TXT Files (*.txt)|*.txt";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    squad_folder_txt_path = openFileDialog.FileName;
                    squad_folder_names_text.Text = openFileDialog.FileName;

                    // Scroll to end
                    squad_folder_names_text.SelectionStart = squad_folder_names_text.Text.Length;
                    squad_folder_names_text.ScrollToCaret();
                }
            }
        }

        private void versionLabel_Click(object sender, EventArgs e)
        {

        }

        private void help_create_objects_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ObjectCreatorHelp object_crt_help_window = new ObjectCreatorHelp();
            object_crt_help_window.ShowDialog();
        }

        private void create_objects_CheckedChanged(object sender, EventArgs e)
        {
            if (create_objects.Checked)
            {
                create_object_tags = true;
            }
            else
            {
                create_object_tags = false;
            }
        }
    }
}
