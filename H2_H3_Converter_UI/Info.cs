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
    public partial class Info : Form
    {
        public Info()
        {
            InitializeComponent();
            textBox1.SelectionStart = 9999;
            textBox1.SelectionLength = 0;
        }
    }
}
