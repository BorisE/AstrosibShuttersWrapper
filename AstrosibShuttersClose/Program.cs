using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AstrosibShuttersWrapper;

namespace AstrosibShuttersClose
{
    static class Program
    {
        private static Form1 frm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            frm = new AstrosibShuttersWrapper.Form1();
            frm.SHUTTER_DIR = false; //close
            Application.Run(frm);
        }
    }
}
