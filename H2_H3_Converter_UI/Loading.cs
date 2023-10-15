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
    public partial class Loading : Form
    {
        private TaskCompletionSource<bool> closeSignal = new TaskCompletionSource<bool>();

        public Loading()
        {
            InitializeComponent();
        }

        public void UpdateOutputBox(string text, bool fromTool)
        {
            if (output_box.InvokeRequired)
            {
                // The code is running on a different thread, so use Invoke to marshal the operation to the UI thread.
                if (!fromTool)
                {
                    output_box.Invoke(new Action(() => UpdateOutputBox(text + Environment.NewLine, fromTool)));
                }
                else
                {
                    output_box.Invoke(new Action(() => UpdateOutputBox(text, fromTool)));
                }
                
            }
            else
            {
                // Update the UI control on the UI thread.
                if (!fromTool)
                {
                    output_box.AppendText(text + Environment.NewLine);
                }
                else
                {
                    output_box.AppendText(text);
                }
                
            }
        }

        public async Task WaitForCloseAsync()
        {
            await closeSignal.Task;
        }

        private void close_button_Click(object sender, EventArgs e)
        {
            closeSignal.SetResult(true);
        }

        public void Enable_Close()
        {
            close_button.Enabled = true;
        }
    }
}
