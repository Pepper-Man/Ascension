namespace H2_H3_Converter_UI
{
    partial class Form1
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
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(583, 552);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 553);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
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
    }
}

