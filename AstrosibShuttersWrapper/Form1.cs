using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using ASCOM;
using ASCOM.DeviceInterface;



namespace AstrosibShuttersWrapper
{
    public partial class Form1 : Form
    {

        public string DRIVER_NAME = "";

        private ASCOM.DriverAccess.Switch objSwitch = null;
        public bool SHUTTER_DIR= true; //true = open, false = close
        private string stBuffer="";
        private short WAIT_TIME=5;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DRIVER_NAME = "ASCOM.Astrosib_Shutters.Switch";
            //DRIVER_NAME = "ASCOM.Simulator.Switch";

            txtInfo.Text += (SHUTTER_DIR ? "OPENING" : "CLOSING") + " CONFIGURATION" + Environment.NewLine;
            txtInfo.Text += "Using driver:  " + DRIVER_NAME + "" + Environment.NewLine;
            txtInfo.Text += "----------------------------------------" + Environment.NewLine;


            //Connect
            try
            {
                objSwitch = new ASCOM.DriverAccess.Switch(DRIVER_NAME);
                objSwitch.Connected = true;
                txtInfo.Text += "Driver [" + DRIVER_NAME + "] is connected" + Environment.NewLine;
            }
            catch (Exception Ex)
            {
                txtInfo.Text += Ex.ToString() + Environment.NewLine;
            }

            //Opening
            txtInfo.Text += "Starting auto run..." + Environment.NewLine;
            backgroundWorker_Open.RunWorkerAsync();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                objSwitch.Connected = false;
            }
            catch (Exception Ex)
            {
                txtInfo.Text += Ex.ToString();
            }
        }


        private void btnRead_Click(object sender, EventArgs e)
        {
            //st = GetSwitchValues(st); //not implemented here
            GetSwitchStatus();

        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            SetSwitchStatus(true);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            SetSwitchStatus(false);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            backgroundWorker_Open.CancelAsync();
            backgroundWorker_wait.CancelAsync();
            txtInfo.Text += "Canceling..." + Environment.NewLine;
        }

        #region Open / Close worker
        private void backgroundWorkerOpen_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                for (short i = 0; i < objSwitch.MaxSwitch; i++)
                {
                    if (backgroundWorker_Open.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    objSwitch.SetSwitch(i, SHUTTER_DIR);

                    backgroundWorker_Open.ReportProgress(i);
                }

            }
            catch (Exception Ex)
            {
                stBuffer += Ex.ToString() + Environment.NewLine;
            }
        }


        private void backgroundWorker_Open_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txtInfo.Text += "" + stBuffer + (stBuffer != "" ? Environment.NewLine : "");
            stBuffer = "";
            txtInfo.Text += (SHUTTER_DIR? "Opening" : "Closing") + "... " + e.ProgressPercentage + Environment.NewLine;
        }

        private void backgroundWorker_Open_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                txtInfo.Text += (SHUTTER_DIR ? "Opening" : "Closing") + " aborted" + Environment.NewLine;

                //Switch status
                GetSwitchStatus();
            }
            else
            {
                txtInfo.Text += (SHUTTER_DIR ? "Opening" : "Closing") + " completed" + Environment.NewLine;

                //Switch status
                GetSwitchStatus();

                //Counter for exiting
                backgroundWorker_wait.RunWorkerAsync();
            }

        }

        #endregion

        #region Wait Worker
        private void backgroundWorker_wait_DoWork(object sender, DoWorkEventArgs e)
        {
            for (short i = 0; i < WAIT_TIME; i++)
            {
                if (backgroundWorker_wait.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(1000);
                backgroundWorker_wait.ReportProgress(i);
            }
        }

        private void backgroundWorker_wait_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txtInfo.Text += "Waiting ... " + e.ProgressPercentage + Environment.NewLine;
        }

        private void backgroundWorker_wait_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                txtInfo.Text += "Aborting autorun" + Environment.NewLine;
            }
            else
            { 
                this.Invoke(new Action(() => this.endRunAction()));
            }

        }

        private void endRunAction()
        {
           this.Close();
        }
        #endregion

        #region Driver part
        private string GetSwitchStatus()
        {
            string st = "";
            try
            {
                for (short i = 0; i < objSwitch.MaxSwitch; i++)
                {
                    st = objSwitch.GetSwitchName(i).ToString() + "=" + objSwitch.GetSwitch(i).ToString();
                    txtInfo.Text += st + Environment.NewLine;
                }

            }
            catch (Exception Ex)
            {
                txtInfo.Text += Ex.ToString() + Environment.NewLine;
            }

            return st;
        }

        private string SetSwitchStatus(bool statusval)
        {
            string st = "";
            try
            {
                for (short i = 0; i < objSwitch.MaxSwitch; i++)
                {
                    objSwitch.SetSwitch(i, statusval);
                }

            }
            catch (Exception Ex)
            {
                txtInfo.Text += Ex.ToString() + Environment.NewLine;
            }

            return st;
        }

        private string GetSwitchValues()
        {
            string st = "";
            try
            {
                for (short i = 0; i < objSwitch.MaxSwitch; i++)
                {
                    st = objSwitch.GetSwitchName(i).ToString() + "=" + objSwitch.GetSwitchValue(i).ToString();
                    txtInfo.Text += st + Environment.NewLine;
                }

            }
            catch (Exception Ex)
            {
                txtInfo.Text += Ex.ToString() + Environment.NewLine;
            }

            return st;
        }

        #endregion

        private void txtInfo_TextChanged(object sender, EventArgs e)
        {
            txtInfo.SelectionStart = txtInfo.TextLength;
            txtInfo.SelectionLength = 0;
            txtInfo.ScrollToCaret();
        }

        const string CMD1 = "start";
        const string CMD2 = "com";

        /// <summary>
        /// Test and Parse command line arguments, including usual coomand line and ClickOnce URI parameters passing
        /// </summary>
        /// <param name="outAutoStart">(out) Returns autostart parameter</param>
        /// <param name="outComport">(out) Returns comport name</param>
        public static void CheckStartParams(out bool outAutoStart, out string outComport)
        {
            bool autostart = false;
            string Comport = string.Empty;

            //1. USUAL COMMAND LINE ARGUMENTS
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].IndexOf(CMD1) >= 0)
                {
                    //AUTOSTART MONITORING
                    autostart = true;
                }
                else if (args[i].ToLower().Substring(0, CMD2.Length) == CMD2)
                {
                    //RESET COM PORT NAME
                    Comport = args[i].ToLower();
                }
            }

            //2. ClickOnce parameters pass algorithm
            try
            {
                string cmdLine = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];

                if (cmdLine != "")
                {
                    ////// for debug
                    //MessageBox.Show(cmdLine);
                    /////////
                    NameValueCollection nvc = HttpUtility.ParseQueryString(cmdLine);
                    string[] theKeys = nvc.AllKeys;

                    // if cmdline wasn't in GetQuery format, then force it to be one string delimetered by space
                    if (theKeys.Count() == 1 && string.IsNullOrEmpty(theKeys[0]))
                    {
                        theKeys = cmdLine.Split(' ');
                    }

                    foreach (string theKey in theKeys)
                    {
                        if (theKey.IndexOf(CMD1) >= 0)
                        {
                            //AUTOSTART MONITORING
                            autostart = true;
                        }
                        else if (theKey.ToLower().Substring(0, CMD2.Length) == CMD2)
                        {
                            //RESET COM PORT NAME
                            Comport = theKey.ToLower();
                        }
                    }
                }
            }
            catch { }

            outAutoStart = autostart;
            outComport = Comport;
        }
    }

}
