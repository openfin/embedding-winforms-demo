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

        private Size sizeSnapShot;
        public delegate void embedDelegate(Openfin.Desktop.Application application, TabPage tab);

        List<Openfin.Desktop.Application> ofApplications = new List<Openfin.Desktop.Application>();

        public Form1()
        {
            
            InitializeComponent();
            SetText("Connecting...");
            openFinApi = new OpenFinDesktopApi(onOpenFinReady, onOpenFinError, onOpenFinClose);
        }
        private void setSizeSnapShot(Size size)
        {
            sizeSnapShot = size;
        }

        private void resizeTo(Size size)
        {
            var deltaHeight = size.Height - sizeSnapShot.Height;
            var deltaWidth = size.Width - sizeSnapShot.Width;

            ofApplications.ForEach(app =>
            {
                var window = app.getWindow();

                window.resizeBy(deltaWidth, deltaHeight, ack =>
                {
                    window.moveTo(0, 0);
                });
            });

            var tabControllSize = tabControl1.Size;
            tabControllSize.Height = tabControllSize.Height + deltaHeight;
            tabControllSize.Width = tabControllSize.Width + deltaWidth;
            
            tabControl1.Size = tabControllSize;

        }

        private void onOpenFinReady(string status)
        {
            SetText("Ready");

            //Create Both Applications.
            createApplication("of-chart-tab", "http://cdn.openfin.co/embed-web/chart.html", tabPage1);
            createApplication("hello-world-tab", "http://cdn.openfin.co/embed-web/", tabPage2);
        }

        private void createApplication (string name, string url, TabPage tab)
        {            
           openFinApi.createApplication(name, url,(app) =>
            {
                //once the application is ready add it to the list of apps and embed it in its tab.
                ofApplications.Add(app);
                embedWindow(app, tab);
            });
        }

        private void embedWindow(Openfin.Desktop.Application application, TabPage tab)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (tab.InvokeRequired)
            {
                //make sure we make UI changes on the UI thread.
                var d = new embedDelegate(embedWindow);
                this.Invoke(d, new object[] { application, tab });
            }
            else
            {
                //We are in the UI thread lets embed the window.
                var window = application.getWindow();
                window.embedInto(tab.Handle, 742, 484, 0, 0, ack =>
                {
                    window.show();
                });
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
            //lets close out our tab applications.
            ofApplications.ForEach(app =>
            {
                app.close();
            });

            //lets give OpenFin some time to close.
            Thread.Sleep(500);
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            resizeTo(this.Size);
        }

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            setSizeSnapShot(this.Size);
        }
    }
}
