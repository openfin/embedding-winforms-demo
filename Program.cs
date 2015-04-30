using System;
using System.Windows.Forms;

namespace EmbeddingWindowsExample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var appForm = new Form1();
            Application.Run(appForm);
        }
    }
}
