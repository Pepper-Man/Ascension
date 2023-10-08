namespace H2_H3_Converter_UI
{
    partial class form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.title = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.hintlabel = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.bsp_label = new System.Windows.Forms.Label();
            this.bsps_box = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.bsp_remove = new System.Windows.Forms.Button();
            this.bsp_add = new System.Windows.Forms.Button();
            this.scenario_label = new System.Windows.Forms.Label();
            this.layout_scenario = new System.Windows.Forms.TableLayoutPanel();
            this.browse_scen = new System.Windows.Forms.Button();
            this.scen_box = new System.Windows.Forms.TextBox();
            this.existing_bitmaps = new System.Windows.Forms.CheckBox();
            this.start_button = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.layout_scenario.SuspendLayout();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AccessibleName = "";
            this.title.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(123, 0);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(337, 25);
            this.title.TabIndex = 0;
            this.title.Text = "Pepper\'s H2 to H3 Converter Tool";
            this.title.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.title.Click += new System.EventHandler(this.label1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox1.AutoSize = true;
            this.checkBox1.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox1.Location = new System.Drawing.Point(224, 65);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(134, 20);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "Convert shaders?";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.title, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.hintlabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBox2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBox3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.bsp_label, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.bsps_box, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.scenario_label, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.layout_scenario, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.existing_bitmaps, 0, 12);
            this.tableLayoutPanel1.Controls.Add(this.start_button, 0, 19);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 21;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(583, 851);
            this.tableLayoutPanel1.TabIndex = 3;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // hintlabel
            // 
            this.hintlabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.hintlabel.AutoSize = true;
            this.hintlabel.Location = new System.Drawing.Point(157, 37);
            this.hintlabel.Name = "hintlabel";
            this.hintlabel.Size = new System.Drawing.Size(268, 16);
            this.hintlabel.TabIndex = 3;
            this.hintlabel.Text = "Please check the conversions that you want:";
            this.hintlabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.hintlabel.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox2.AutoSize = true;
            this.checkBox2.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox2.Location = new System.Drawing.Point(174, 95);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(235, 20);
            this.checkBox2.TabIndex = 4;
            this.checkBox2.Text = "Convert zones and firing positions?";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox3.AutoSize = true;
            this.checkBox3.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox3.Location = new System.Drawing.Point(144, 125);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(295, 20);
            this.checkBox3.TabIndex = 5;
            this.checkBox3.Text = "Convert scenario data (objects, spawns etc)?";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // bsp_label
            // 
            this.bsp_label.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bsp_label.AutoSize = true;
            this.bsp_label.Enabled = false;
            this.bsp_label.Location = new System.Drawing.Point(160, 187);
            this.bsp_label.Name = "bsp_label";
            this.bsp_label.Size = new System.Drawing.Size(263, 16);
            this.bsp_label.TabIndex = 6;
            this.bsp_label.Text = "Add BSP XML files to convert shaders from:";
            this.bsp_label.Click += new System.EventHandler(this.label1_Click_2);
            // 
            // bsps_box
            // 
            this.bsps_box.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bsps_box.BackColor = System.Drawing.SystemColors.Control;
            this.bsps_box.Enabled = false;
            this.bsps_box.FormattingEnabled = true;
            this.bsps_box.ItemHeight = 16;
            this.bsps_box.Location = new System.Drawing.Point(65, 213);
            this.bsps_box.Name = "bsps_box";
            this.bsps_box.Size = new System.Drawing.Size(453, 68);
            this.bsps_box.TabIndex = 8;
            this.bsps_box.SelectedIndexChanged += new System.EventHandler(this.bsps_box_SelectedIndexChanged);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.bsp_add, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.bsp_remove, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(191, 287);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 30);
            this.tableLayoutPanel2.TabIndex = 9;
            // 
            // bsp_remove
            // 
            this.bsp_remove.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bsp_remove.Enabled = false;
            this.bsp_remove.Location = new System.Drawing.Point(112, 3);
            this.bsp_remove.Name = "bsp_remove";
            this.bsp_remove.Size = new System.Drawing.Size(75, 23);
            this.bsp_remove.TabIndex = 1;
            this.bsp_remove.Text = "Remove";
            this.bsp_remove.UseVisualStyleBackColor = true;
            this.bsp_remove.Click += new System.EventHandler(this.bsp_remove_Click);
            // 
            // bsp_add
            // 
            this.bsp_add.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bsp_add.Enabled = false;
            this.bsp_add.Location = new System.Drawing.Point(12, 3);
            this.bsp_add.Name = "bsp_add";
            this.bsp_add.Size = new System.Drawing.Size(75, 23);
            this.bsp_add.TabIndex = 0;
            this.bsp_add.Text = "Add BSP";
            this.bsp_add.UseVisualStyleBackColor = true;
            this.bsp_add.Click += new System.EventHandler(this.bsp_add_Click);
            // 
            // scenario_label
            // 
            this.scenario_label.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.scenario_label.AutoSize = true;
            this.scenario_label.Enabled = false;
            this.scenario_label.Location = new System.Drawing.Point(215, 357);
            this.scenario_label.Name = "scenario_label";
            this.scenario_label.Size = new System.Drawing.Size(152, 16);
            this.scenario_label.TabIndex = 10;
            this.scenario_label.Text = "Select H3 scenario path:";
            // 
            // layout_scenario
            // 
            this.layout_scenario.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.layout_scenario.ColumnCount = 2;
            this.layout_scenario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.90543F));
            this.layout_scenario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.09456F));
            this.layout_scenario.Controls.Add(this.browse_scen, 1, 0);
            this.layout_scenario.Controls.Add(this.scen_box, 0, 0);
            this.layout_scenario.Location = new System.Drawing.Point(65, 383);
            this.layout_scenario.Name = "layout_scenario";
            this.layout_scenario.RowCount = 1;
            this.layout_scenario.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout_scenario.Size = new System.Drawing.Size(453, 30);
            this.layout_scenario.TabIndex = 11;
            // 
            // browse_scen
            // 
            this.browse_scen.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.browse_scen.Enabled = false;
            this.browse_scen.Location = new System.Drawing.Point(369, 3);
            this.browse_scen.Name = "browse_scen";
            this.browse_scen.Size = new System.Drawing.Size(75, 23);
            this.browse_scen.TabIndex = 12;
            this.browse_scen.Text = "Browse";
            this.browse_scen.UseVisualStyleBackColor = true;
            this.browse_scen.Click += new System.EventHandler(this.browse_scen_Click);
            // 
            // scen_box
            // 
            this.scen_box.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.scen_box.Enabled = false;
            this.scen_box.Location = new System.Drawing.Point(3, 4);
            this.scen_box.Name = "scen_box";
            this.scen_box.Size = new System.Drawing.Size(355, 22);
            this.scen_box.TabIndex = 13;
            // 
            // existing_bitmaps
            // 
            this.existing_bitmaps.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.existing_bitmaps.AutoSize = true;
            this.existing_bitmaps.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.existing_bitmaps.Enabled = false;
            this.existing_bitmaps.Location = new System.Drawing.Point(95, 421);
            this.existing_bitmaps.Name = "existing_bitmaps";
            this.existing_bitmaps.Size = new System.Drawing.Size(392, 20);
            this.existing_bitmaps.TabIndex = 12;
            this.existing_bitmaps.Text = "Use existing .tif files from the scenario\'s (data) bitmaps folder?";
            this.existing_bitmaps.UseVisualStyleBackColor = true;
            // 
            // start_button
            // 
            this.start_button.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.start_button.AutoSize = true;
            this.start_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.start_button.Location = new System.Drawing.Point(206, 629);
            this.start_button.Name = "start_button";
            this.start_button.Size = new System.Drawing.Size(170, 28);
            this.start_button.TabIndex = 13;
            this.start_button.Text = "Begin conversion!";
            this.start_button.UseVisualStyleBackColor = true;
            this.start_button.Click += new System.EventHandler(this.start_button_Click);
            // 
            // form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 853);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.layout_scenario.ResumeLayout(false);
            this.layout_scenario.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label hintlabel;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label bsp_label;
        private System.Windows.Forms.ListBox bsps_box;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button bsp_add;
        private System.Windows.Forms.Button bsp_remove;
        private System.Windows.Forms.Label scenario_label;
        private System.Windows.Forms.TableLayoutPanel layout_scenario;
        private System.Windows.Forms.Button browse_scen;
        private System.Windows.Forms.TextBox scen_box;
        private System.Windows.Forms.CheckBox existing_bitmaps;
        private System.Windows.Forms.Button start_button;
    }
}

