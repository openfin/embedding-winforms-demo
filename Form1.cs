using System.Windows.Forms;
using Openfin.WinForm;
using Openfin.Desktop;
using Newtonsoft.Json.Linq;

namespace EmbeddingWindowsExample
{
    public partial class Form1 : Form
    {
        private EmbeddedView chartEmbeddedView;
        public Form1()
        {
            InitializeComponent();
            this.lblConnectionStatus.Text = "Connecting...";
            //Runtime options is how we set up the OpenFin Runtime environment,
            var runtimeOptions = new RuntimeOptions {
                Version = "stable",
                EnableRemoteDevTools = true,
                RemoteDevToolsPort = 9090
            };

            //Create our two embedded view controlls.
            chartEmbeddedView = new EmbeddedView();

            //Add the embedded view controlls to each tabPage
            panel1.Controls.Add(chartEmbeddedView);

            //Initialize the chart view by passing the runtime Options and the ApplicationOptions
            chartEmbeddedView.Initialize(runtimeOptions, new ApplicationOptions("of-chart-tab", "of-chart-tab-uuid", "http://cdn.openfin.co/embed-web/chart.html"));

            //We can get the instance of the singleton runtime object by usig the GetRuntimeInstance function and passing 
            var openFinRuntime = Runtime.GetRuntimeInstance(runtimeOptions);

            chartEmbeddedView.OnReady += (sender, e) =>
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
            };
        }

    }
}
