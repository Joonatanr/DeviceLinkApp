using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceLinkGui
{
    public partial class Form1 : Form
    {
        DeviceLinkHandler dHandler;
        
        public Form1()
        {
            InitializeComponent();

            dHandler = new DeviceLinkHandler(1711);
            dHandler.TextPrinter = new DeviceLinkHandler.printHandler(printText);
            dHandler.Run();
        }

        private void printText(string str)
        {
            try
            {
                if (this.richTextBox1.InvokeRequired)
                {
                    // Call this same method but append THREAD2 to the text
                    Action safeWrite = delegate { printText($"{str} (THREAD2)"); };
                    richTextBox1.Invoke(safeWrite);
                }
                else
                {
                    richTextBox1.AppendText(str);
                }
            }
            catch (Exception)
            {
                //Not really much to do here. Not a catastrophe if we cannot write to the richtextbox.
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            dHandler.Close();
        }
    }
}
