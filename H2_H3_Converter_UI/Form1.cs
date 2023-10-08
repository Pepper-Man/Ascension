using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H2_H3_Converter_UI
{
    public partial class form1 : Form
    {
        List<string> bsp_paths = new List<string>();
        string scen_path = "";

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

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
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
            }
            else
            {
                bsps_box.Enabled = false;
                bsps_box.BackColor = SystemColors.Control;
                bsp_label.Enabled = false;
                bsp_add.Enabled = false;
                bsp_remove.Enabled = false;
                scenario_label.Enabled = false;
                scen_box.Enabled = false;
                browse_scen.Enabled = false;
            }
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
                    }
                    else
                    {
                        // Scenario is not in H3EK, alert user, don't add
                        MessageBox.Show("Scenario doesn't seem to be in the H3EK directory.\nPlease try again.", "Invalid scenario path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
