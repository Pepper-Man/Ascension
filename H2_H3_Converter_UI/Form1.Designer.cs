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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(form1));
            this.title = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.squad_folder_names_browse = new System.Windows.Forms.Button();
            this.squad_folder_names_text = new System.Windows.Forms.TextBox();
            this.layout_h2_scen = new System.Windows.Forms.TableLayoutPanel();
            this.browse_scen_h2 = new System.Windows.Forms.Button();
            this.h2_scen_box = new System.Windows.Forms.TextBox();
            this.hintlabel = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.bsp_label = new System.Windows.Forms.Label();
            this.bsps_box = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.bsp_add = new System.Windows.Forms.Button();
            this.bsp_remove = new System.Windows.Forms.Button();
            this.scenario_label = new System.Windows.Forms.Label();
            this.layout_scenario = new System.Windows.Forms.TableLayoutPanel();
            this.browse_scen = new System.Windows.Forms.Button();
            this.scen_box = new System.Windows.Forms.TextBox();
            this.h2_scen_label = new System.Windows.Forms.Label();
            this.start_button = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.existing_bitmaps = new System.Windows.Forms.CheckBox();
            this.info_label = new System.Windows.Forms.LinkLabel();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.use_squad_folder_names = new System.Windows.Forms.CheckBox();
            this.help_squad_folders = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.create_objects = new System.Windows.Forms.CheckBox();
            this.help_create_objects = new System.Windows.Forms.LinkLabel();
            this.versionLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.layout_h2_scen.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.layout_scenario.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AccessibleName = "";
            this.title.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Myanmar Text", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(236, 0);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(110, 30);
            this.title.TabIndex = 0;
            this.title.Text = "Ascension";
            this.title.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.title.Click += new System.EventHandler(this.label1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox1.AutoSize = true;
            this.checkBox1.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox1.Location = new System.Drawing.Point(251, 65);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 20);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "Shaders";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 19);
            this.tableLayoutPanel1.Controls.Add(this.layout_h2_scen, 0, 17);
            this.tableLayoutPanel1.Controls.Add(this.title, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.hintlabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBox2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBox3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.bsp_label, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.bsps_box, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.scenario_label, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.layout_scenario, 0, 12);
            this.tableLayoutPanel1.Controls.Add(this.h2_scen_label, 0, 16);
            this.tableLayoutPanel1.Controls.Add(this.start_button, 0, 21);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 13);
            this.tableLayoutPanel1.Controls.Add(this.checkBox4, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 18);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 14);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 22;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
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
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(583, 736);
            this.tableLayoutPanel1.TabIndex = 3;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.90543F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.09456F));
            this.tableLayoutPanel4.Controls.Add(this.squad_folder_names_browse, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.squad_folder_names_text, 0, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(65, 639);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(453, 30);
            this.tableLayoutPanel4.TabIndex = 15;
            // 
            // squad_folder_names_browse
            // 
            this.squad_folder_names_browse.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.squad_folder_names_browse.Enabled = false;
            this.squad_folder_names_browse.Location = new System.Drawing.Point(369, 3);
            this.squad_folder_names_browse.Name = "squad_folder_names_browse";
            this.squad_folder_names_browse.Size = new System.Drawing.Size(75, 23);
            this.squad_folder_names_browse.TabIndex = 12;
            this.squad_folder_names_browse.Text = "Browse";
            this.squad_folder_names_browse.UseVisualStyleBackColor = true;
            this.squad_folder_names_browse.Click += new System.EventHandler(this.squad_folder_names_browse_Click);
            // 
            // squad_folder_names_text
            // 
            this.squad_folder_names_text.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.squad_folder_names_text.Enabled = false;
            this.squad_folder_names_text.Location = new System.Drawing.Point(3, 4);
            this.squad_folder_names_text.Name = "squad_folder_names_text";
            this.squad_folder_names_text.Size = new System.Drawing.Size(355, 22);
            this.squad_folder_names_text.TabIndex = 13;
            // 
            // layout_h2_scen
            // 
            this.layout_h2_scen.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.layout_h2_scen.ColumnCount = 2;
            this.layout_h2_scen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.90543F));
            this.layout_h2_scen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.09456F));
            this.layout_h2_scen.Controls.Add(this.browse_scen_h2, 1, 0);
            this.layout_h2_scen.Controls.Add(this.h2_scen_box, 0, 0);
            this.layout_h2_scen.Location = new System.Drawing.Point(65, 569);
            this.layout_h2_scen.Name = "layout_h2_scen";
            this.layout_h2_scen.RowCount = 2;
            this.layout_h2_scen.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout_h2_scen.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.layout_h2_scen.Size = new System.Drawing.Size(453, 30);
            this.layout_h2_scen.TabIndex = 14;
            // 
            // browse_scen_h2
            // 
            this.browse_scen_h2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.browse_scen_h2.Enabled = false;
            this.browse_scen_h2.Location = new System.Drawing.Point(369, 3);
            this.browse_scen_h2.Name = "browse_scen_h2";
            this.browse_scen_h2.Size = new System.Drawing.Size(75, 23);
            this.browse_scen_h2.TabIndex = 12;
            this.browse_scen_h2.Text = "Browse";
            this.browse_scen_h2.UseVisualStyleBackColor = true;
            this.browse_scen_h2.Click += new System.EventHandler(this.browse_scen_h2_Click);
            // 
            // h2_scen_box
            // 
            this.h2_scen_box.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.h2_scen_box.Enabled = false;
            this.h2_scen_box.Location = new System.Drawing.Point(3, 3);
            this.h2_scen_box.Name = "h2_scen_box";
            this.h2_scen_box.Size = new System.Drawing.Size(355, 22);
            this.h2_scen_box.TabIndex = 13;
            // 
            // hintlabel
            // 
            this.hintlabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.hintlabel.AutoSize = true;
            this.hintlabel.Location = new System.Drawing.Point(153, 37);
            this.hintlabel.Name = "hintlabel";
            this.hintlabel.Size = new System.Drawing.Size(276, 16);
            this.hintlabel.TabIndex = 3;
            this.hintlabel.Text = "Please check the conversion(s) that you want:";
            this.hintlabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.hintlabel.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox2.AutoSize = true;
            this.checkBox2.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox2.Location = new System.Drawing.Point(180, 95);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(222, 20);
            this.checkBox2.TabIndex = 4;
            this.checkBox2.Text = "Zones, areas and firing positions";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox3.AutoSize = true;
            this.checkBox3.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox3.Location = new System.Drawing.Point(151, 125);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(281, 20);
            this.checkBox3.TabIndex = 5;
            this.checkBox3.Text = "Scenario object data (objects, spawns etc)";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // bsp_label
            // 
            this.bsp_label.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.bsp_label.AutoSize = true;
            this.bsp_label.Enabled = false;
            this.bsp_label.Location = new System.Drawing.Point(160, 217);
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
            this.bsps_box.Location = new System.Drawing.Point(65, 243);
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
            this.tableLayoutPanel2.Location = new System.Drawing.Point(191, 317);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 30);
            this.tableLayoutPanel2.TabIndex = 9;
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
            // scenario_label
            // 
            this.scenario_label.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.scenario_label.AutoSize = true;
            this.scenario_label.Enabled = false;
            this.scenario_label.Location = new System.Drawing.Point(219, 387);
            this.scenario_label.Name = "scenario_label";
            this.scenario_label.Size = new System.Drawing.Size(145, 16);
            this.scenario_label.TabIndex = 10;
            this.scenario_label.Text = "Select H3 scenario tag:";
            // 
            // layout_scenario
            // 
            this.layout_scenario.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.layout_scenario.ColumnCount = 2;
            this.layout_scenario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.90543F));
            this.layout_scenario.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.09456F));
            this.layout_scenario.Controls.Add(this.browse_scen, 1, 0);
            this.layout_scenario.Controls.Add(this.scen_box, 0, 0);
            this.layout_scenario.Location = new System.Drawing.Point(65, 413);
            this.layout_scenario.Name = "layout_scenario";
            this.layout_scenario.RowCount = 2;
            this.layout_scenario.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout_scenario.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
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
            this.scen_box.Location = new System.Drawing.Point(3, 3);
            this.scen_box.Name = "scen_box";
            this.scen_box.Size = new System.Drawing.Size(355, 22);
            this.scen_box.TabIndex = 13;
            // 
            // h2_scen_label
            // 
            this.h2_scen_label.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.h2_scen_label.AutoSize = true;
            this.h2_scen_label.Enabled = false;
            this.h2_scen_label.Location = new System.Drawing.Point(205, 543);
            this.h2_scen_label.Name = "h2_scen_label";
            this.h2_scen_label.Size = new System.Drawing.Size(172, 16);
            this.h2_scen_label.TabIndex = 14;
            this.h2_scen_label.Text = "Select H2 scenario XML file:";
            // 
            // start_button
            // 
            this.start_button.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.start_button.AutoSize = true;
            this.start_button.Enabled = false;
            this.start_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.start_button.Location = new System.Drawing.Point(206, 698);
            this.start_button.Name = "start_button";
            this.start_button.Size = new System.Drawing.Size(170, 28);
            this.start_button.TabIndex = 13;
            this.start_button.Text = "Begin conversion!";
            this.start_button.UseVisualStyleBackColor = true;
            this.start_button.Click += new System.EventHandler(this.start_button_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 87.16216F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.83784F));
            this.tableLayoutPanel3.Controls.Add(this.existing_bitmaps, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.info_label, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(69, 450);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(444, 26);
            this.tableLayoutPanel3.TabIndex = 15;
            // 
            // existing_bitmaps
            // 
            this.existing_bitmaps.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.existing_bitmaps.AutoSize = true;
            this.existing_bitmaps.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.existing_bitmaps.Enabled = false;
            this.existing_bitmaps.Location = new System.Drawing.Point(16, 3);
            this.existing_bitmaps.Name = "existing_bitmaps";
            this.existing_bitmaps.Size = new System.Drawing.Size(354, 20);
            this.existing_bitmaps.TabIndex = 12;
            this.existing_bitmaps.Text = "Use existing .tif files from the scenario\'s bitmaps folder?";
            this.existing_bitmaps.UseVisualStyleBackColor = true;
            this.existing_bitmaps.CheckedChanged += new System.EventHandler(this.existing_bitmaps_CheckedChanged);
            // 
            // info_label
            // 
            this.info_label.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.info_label.Enabled = false;
            this.info_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.info_label.Location = new System.Drawing.Point(390, 5);
            this.info_label.Name = "info_label";
            this.info_label.Size = new System.Drawing.Size(51, 16);
            this.info_label.TabIndex = 13;
            this.info_label.TabStop = true;
            this.info_label.Text = "help";
            this.info_label.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.info_label_LinkClicked);
            // 
            // checkBox4
            // 
            this.checkBox4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checkBox4.AutoSize = true;
            this.checkBox4.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox4.Location = new System.Drawing.Point(84, 155);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(414, 20);
            this.checkBox4.TabIndex = 16;
            this.checkBox4.Text = "Scenario AI/script data (jump/flight hints, flags, point sets, squads)";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 87.16216F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.83784F));
            this.tableLayoutPanel5.Controls.Add(this.use_squad_folder_names, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.help_squad_folders, 1, 0);
            this.tableLayoutPanel5.Location = new System.Drawing.Point(69, 605);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(444, 26);
            this.tableLayoutPanel5.TabIndex = 17;
            this.tableLayoutPanel5.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel5_Paint);
            // 
            // use_squad_folder_names
            // 
            this.use_squad_folder_names.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.use_squad_folder_names.AutoSize = true;
            this.use_squad_folder_names.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.use_squad_folder_names.Enabled = false;
            this.use_squad_folder_names.Location = new System.Drawing.Point(47, 3);
            this.use_squad_folder_names.Name = "use_squad_folder_names";
            this.use_squad_folder_names.Size = new System.Drawing.Size(293, 20);
            this.use_squad_folder_names.TabIndex = 12;
            this.use_squad_folder_names.Text = "Use squad folder names TXT file? (Optional)";
            this.use_squad_folder_names.UseVisualStyleBackColor = true;
            this.use_squad_folder_names.CheckedChanged += new System.EventHandler(this.use_squad_folder_names_CheckedChanged);
            // 
            // help_squad_folders
            // 
            this.help_squad_folders.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.help_squad_folders.Enabled = false;
            this.help_squad_folders.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.help_squad_folders.Location = new System.Drawing.Point(391, 5);
            this.help_squad_folders.Name = "help_squad_folders";
            this.help_squad_folders.Size = new System.Drawing.Size(48, 16);
            this.help_squad_folders.TabIndex = 13;
            this.help_squad_folders.TabStop = true;
            this.help_squad_folders.Text = "help";
            this.help_squad_folders.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.help_squad_folders_LinkClicked);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 87.16216F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.83784F));
            this.tableLayoutPanel6.Controls.Add(this.create_objects, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.help_create_objects, 1, 0);
            this.tableLayoutPanel6.Location = new System.Drawing.Point(116, 483);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(350, 26);
            this.tableLayoutPanel6.TabIndex = 18;
            // 
            // create_objects
            // 
            this.create_objects.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.create_objects.AutoSize = true;
            this.create_objects.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.create_objects.Enabled = false;
            this.create_objects.Location = new System.Drawing.Point(4, 3);
            this.create_objects.Name = "create_objects";
            this.create_objects.Size = new System.Drawing.Size(297, 20);
            this.create_objects.TabIndex = 12;
            this.create_objects.Text = "Create missing .model and object-level tags?";
            this.create_objects.UseVisualStyleBackColor = true;
            this.create_objects.CheckedChanged += new System.EventHandler(this.create_objects_CheckedChanged);
            // 
            // help_create_objects
            // 
            this.help_create_objects.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.help_create_objects.Enabled = false;
            this.help_create_objects.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.help_create_objects.Location = new System.Drawing.Point(308, 5);
            this.help_create_objects.Name = "help_create_objects";
            this.help_create_objects.Size = new System.Drawing.Size(39, 16);
            this.help_create_objects.TabIndex = 13;
            this.help_create_objects.TabStop = true;
            this.help_create_objects.Text = "help";
            this.help_create_objects.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.help_create_objects_LinkClicked);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(526, 745);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(44, 16);
            this.versionLabel.TabIndex = 4;
            this.versionLabel.Text = "label1";
            this.versionLabel.Click += new System.EventHandler(this.versionLabel_Click);
            // 
            // form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 770);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "form1";
            this.Text = "Ascension";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.layout_h2_scen.ResumeLayout(false);
            this.layout_h2_scen.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.layout_scenario.ResumeLayout(false);
            this.layout_scenario.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Label h2_scen_label;
        private System.Windows.Forms.TableLayoutPanel layout_h2_scen;
        private System.Windows.Forms.Button browse_scen_h2;
        private System.Windows.Forms.TextBox h2_scen_box;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.LinkLabel info_label;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button squad_folder_names_browse;
        private System.Windows.Forms.TextBox squad_folder_names_text;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.CheckBox use_squad_folder_names;
        private System.Windows.Forms.LinkLabel help_squad_folders;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.CheckBox create_objects;
        private System.Windows.Forms.LinkLabel help_create_objects;
    }
}

