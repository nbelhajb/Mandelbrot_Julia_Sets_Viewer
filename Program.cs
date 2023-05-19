using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MandelbrotSetViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 frm1 = new Form1();
            //frm1.LaunchJDrawingWhenStarting( new double[] { 0.32, 0.043 });
            if (args.Length > 0)
            {
                // frm1.LaunchJDrawingWhenStarting(new double[] { 0.32, 0.043 });
                frm1.LaunchJDrawingWhenStarting(Form1.ExtractCFromArgs (args));
            }
            Application.Run(frm1);

        }
    }
}
