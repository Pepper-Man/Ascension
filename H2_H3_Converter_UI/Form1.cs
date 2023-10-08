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
    public partial class Form1 : Form
    {
        List<string> bsp_paths = new List<string>();

        public Form1()
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
                bsp_label.Enabled = true;
                bsp_add.Enabled = true;
                bsp_remove.Enabled = true;
            }
            else
            {
                bsps_box.Enabled = false;
                bsp_label.Enabled = false;
                bsp_add.Enabled = false;
                bsp_remove.Enabled = false;
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
                    bsp_paths.Add(bsp_path);
                    bsps_box.Items.Add(bsp_path.Split('\\').Last());
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
    }
}
