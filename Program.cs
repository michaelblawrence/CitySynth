using CitySynth.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CitySynth
{
    static class Program
    {
        /// <summary>
        /// Displays no dialog boxes on load.
        /// </summary>
        public static bool displayNoDialog = false;
        /// <summary>
        /// Limits UI elements to be updated with screen refresh.
        /// </summary>
        public static bool displayNoUI = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Convert args to lower case
            List<string> lowercaseArgs = args.ToList().ConvertAll<string>(s => s.ToLower());

            // Check args for startup qualifiers
            displayNoDialog = lowercaseArgs.IndexOf("nodialog") >= 0;
            displayNoUI = lowercaseArgs.Contains("noui");

            // Check args for file paths
            List<string> ss = args.Where<string>(s => System.IO.File.Exists(s)).ToList();

            // Run CitySynth UI windows form
            Application.Run(new Form1(ss.Count > 0 ? ss[0] : null));
        }
    }
}
