using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceLinkGui
{
    public partial class Form1 : Form
    {
        DeviceLinkHandler dHandler;
        SerialPort mySerialPort;

        public Form1()
        {
            InitializeComponent();

            dHandler = new DeviceLinkHandler(1711);
            dHandler.TextPrinter = new DeviceLinkHandler.printHandler(printText);
            dHandler.Run();

            timer1.Start();

            /* Set up COM port. */
            comboBoxCOMPorts.Items.AddRange(SerialPort.GetPortNames());
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
                    richTextBox1.ScrollToCaret();
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (dHandler != null)
            {
                textBoxAirspeed.Text = dHandler.CurrentAirspeed.ToString();
                textBoxAltitude.Text = dHandler.CurrentAltitude.ToString();

                /* Very initial test... The servo gives us a range of 0-150. So lets set airspeed 600 to 150. */
                int servoVal = (int)(dHandler.CurrentAirspeed / 4);
                servoVal = Math.Min(servoVal, 150);
                
                if (mySerialPort != null)
                {
                    if (mySerialPort.IsOpen)
                    {
                        int gearValue = dHandler.CurrentGearStatus ? 1 : 0;
                        mySerialPort.WriteLine("S" + servoVal.ToString() + "G" + gearValue.ToString());

                        string res = mySerialPort.ReadLine();
                        //textBoxEngineState.Text = res;
                        int power;
                        bool engineState;
                        
                        if(parseArduinoInputValues(res, out power, out engineState) == true)
                        {
                            textBoxEnginePower.Text = power.ToString();
                            
                            if (engineState)
                            {
                                textBoxEngineState.Text = "ON";
                            }
                            else
                            {
                                textBoxEngineState.Text = "OFF";
                            }

                            //textBoxEngineState.Text = res;
                            dHandler.EnginePower = power;
                            dHandler.EngineState = engineState;
                        }
                    }
                }
            }
        }

        private bool parseArduinoInputValues(string str, out int power, out bool engineState)
        {
            int x = 1;
            string enginePowerString = "";
            if (str[0] == 'P')
            {
                while (char.IsDigit(str[x]))
                {
                    enginePowerString += str[x];
                    x++;
                }

                if(str[x] == 'I')
                {
                    if (str[x + 1] == '1') 
                    {
                        engineState = true;
                    }
                    else
                    {
                        engineState = false;
                    }

                    power = int.Parse(enginePowerString);
                    return true;

                }
                else
                {
                    power = 0;
                    engineState = false;
                    return false;
                }

            }
            else
            {
                power = 0;
                engineState = false;
                return false;
            }
        }


        private void buttonOpenCOM_Click(object sender, EventArgs e)
        {
            if (comboBoxCOMPorts.SelectedIndex > -1)
            {
                mySerialPort = new SerialPort(comboBoxCOMPorts.SelectedItem.ToString(), 115200);
                mySerialPort.NewLine = "\n";
                // Set the read/write timeouts
                mySerialPort.ReadTimeout = 500;
                mySerialPort.WriteTimeout = 500;
                mySerialPort.Parity = Parity.None;
                mySerialPort.StopBits = StopBits.One;
                mySerialPort.RtsEnable = true;
                mySerialPort.DtrEnable = true;
                try
                {
                    mySerialPort.Open();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No serial port selected");
            }
        }

        private void buttonCloseComm_Click(object sender, EventArgs e)
        {
            try
            {
                mySerialPort.Close();
            }
            catch (Exception)
            {

            }
        }
    }
}
