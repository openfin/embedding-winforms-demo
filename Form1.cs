using System.Windows.Forms;
using Openfin.WinForm;
using Openfin.Desktop;
using Newtonsoft.Json.Linq;

namespace EmbeddingWindowsExample
{
    public partial class Form1 : Form
    {
        private EmbeddedView chartEmbeddedView;
        private EmbeddedView docsEmbeddedView;
        const string version = "alpha";
        public Form1()
        {
            InitializeComponent();
            this.lblConnectionStatus.Text = "Connecting...";
            //Runtime options will hold our runtime 
            var runtimeOptions = new RuntimeOptions {
                Version = version,
                EnableRemoteDevTools = true,
                RemoteDevToolsPort = 9090
            };

            //Create our two embedded view controlls.
            this.chartEmbeddedView = new EmbeddedView();
            this.docsEmbeddedView = new EmbeddedView();

            //Add the embedded view controlls to each tabPage
            tabPage1.Controls.Add(chartEmbeddedView);
            tabPage2.Controls.Add(docsEmbeddedView);

            //Initialize the chart view by passing the runtime Options and the ApplicationOptions
            chartEmbeddedView.Initialize(runtimeOptions, new ApplicationOptions("of-chart-tab", "of-chart-tab", "http://cdn.openfin.co/embed-web/chart.html"));

            //We want to re-use the chart application and create a new window for it, lets wait until its ready.
            chartEmbeddedView.OnReady += (sender, e) =>
            {
                var windowOptions = new WindowOptions("jsdocs", "http://cdn.openfin.co/jsdocs/3.0.1.5/");
                //by passing the runtimeOptions, an existing application and windowOptions we will create a new window for that application that will be embedded in our view.
                docsEmbeddedView.Initialize(runtimeOptions, chartEmbeddedView.OpenfinApplication, windowOptions);
            };


            //We can get the instance of the singleton runtime object by usig the GetRuntimeInstance function and passing 
            var openFinRuntime = Runtime.GetRuntimeInstance(runtimeOptions);
            
            //THIS IS OPTIONAL, Each Embedded view will connect directly or use an existing connection, the connect function accepts an action that will either be called when the runtime connects or be called right away if the runtime is already connected.
            openFinRuntime.Connect(() => 
            {
                //Update the conection status:
                Utils.InvokeOnUiThreadIfRequired(this, () => { this.lblConnectionStatus.Text = "Connected"; });

                //subscribe to chart-click messages from the chartEmbeddedView
                openFinRuntime.InterApplicationBus.subscribe(chartEmbeddedView.OpenfinApplication.getUuid(), "chart-click", (senderUuid, topic, data) =>
                {
                    var dataAsJObject = JObject.FromObject(data);
                    Utils.InvokeOnUiThreadIfRequired(this, () => { 
                        label5.Text = string.Format("Key:{0}, {1} at {2}", dataAsJObject.GetValue("key"), dataAsJObject.GetValue("y"), dataAsJObject.GetValue("x")); 
                    });
                });
            });
        }

    }
}
