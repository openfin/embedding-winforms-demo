using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Openfin.Desktop;

namespace embeddingWindowsExample
{
    public partial class Form1 : Form
    {
        OpenFinDesktopApi openFinApi;
        public delegate void embedDelegate(Openfin.Desktop.Application application, TabPage tab);

        List<Openfin.Desktop.Application> applications = new List<Openfin.Desktop.Application>();
        public Form1()
        {
            InitializeComponent();
            SetText("Closed");
            openFinApi = new OpenFinDesktopApi(onOpenFinReady, onOpenFinError, onOpenFinClose);
        }

        private void onOpenFinReady(string status)
        {
            SetText("Ready");
            createAndEmbed("HelloWorldApp2", "http://cdn.openfin.co/embed-web/chart.html", tabPage1);
            createAndEmbed("HelloWorldApp", "http://cdn.openfin.co/embed-web/", tabPage2);
        }

        private void createAndEmbed (string name, string url, TabPage tab)
        {
            var appOptions = new ApplicationOptions(name, name, url);
            appOptions.MainWindowOptions.Frame = false;
            appOptions.MainWindowOptions.AutoShow = false;
            
           openFinApi.createApplication(appOptions,(app) =>
            {
                applications.Add(app);
                embed(app, tab);
            });
        }

        private void embed(Openfin.Desktop.Application application, TabPage tab)
        {
            
            if (tab.InvokeRequired)
            {
                var d = new embedDelegate(embed);
                this.Invoke(d, new object[] { application, tab });
            }
            else
            {
                var window = application.getWindow();
                var d = new OpenFinDesktopApi.embedWindowDelegate(openFinApi.embedWindowInTarget);
                this.Invoke(d, new object[] { window, tab.Handle });
                tab.Refresh();
                //openFinApi.embedWindowInTarget(window, tab.Handle);
            }
        }
        private void onOpenFinError(string status)
        {
            SetText("Error");
        }
        private void onOpenFinClose(string status)
        {
            SetText("Closed");
        }

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.lblConnectionStatus.InvokeRequired)
            {
                var d = new OpenFinDesktopApi.stringParamDelegate(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lblConnectionStatus.Text = text;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            applications.ForEach(app =>
            {
                app.close();
            });

            Thread.Sleep(500);
        }
    }
}
