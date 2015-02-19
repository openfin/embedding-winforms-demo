using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Openfin.Desktop;
using System.Runtime.InteropServices;
using System.IO;

namespace embeddingWindowsExample
{
    class OpenFinDesktopApi: DesktopStateListener
    {
        private stringParamDelegate readyCallback;
        private stringParamDelegate errorCallback;
        private stringParamDelegate closeCallback;
        private DesktopConnection openFinConnection;

        public delegate void stringParamDelegate(string str);
        public delegate void appicationParamDelegate(Application application);
        public delegate void embedWindowDelegate(Openfin.Desktop.Window window, IntPtr containerhWnd);

        public OpenFinDesktopApi(stringParamDelegate onReady, stringParamDelegate onError, stringParamDelegate onClose)
        {
            string host = "localhost";
            int port = 9696;
            string runtimeVersion = "3.0.2.16";
            string args = "--config=\"http://cdn.openfin.co/embed-web/null.json\"";

            // Get default installed location for runtime
            string path = (Environment.OSVersion.Version.Major > 5 ?
                // Vista, Win7 or Win8
                Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\OpenFin\\runtime\\" + runtimeVersion + "\\OpenFin\\openfin.exe"
                    :
                // XP
                Environment.ExpandEnvironmentVariables("%APPDATA%") + "..\\Local Settings\\Application Data\\OpenFin\\runtime\\" + runtimeVersion + "\\OpenFin\\openfin.exe"
            );
            
            readyCallback = onReady;
            errorCallback = onError;
            closeCallback = onClose;

            openFinConnection = new DesktopConnection("c# Embed OpenFin Window", host, port);
            openFinConnection.launchAndConnect(path, args, this, 30);
        }
        public void createApplication(ApplicationOptions options, appicationParamDelegate callback)
        {
            var app = new Openfin.Desktop.Application(options, openFinConnection);

            onApplicationReady(app, (a,b,c) => {
                callback(app);
            });

            app.run();
        }

        private void onApplicationReady(Application application,  InterAppMessageHandler callback)
        {
            openFinConnection
                .getInterApplicationBus()
                .subscribe(application.getUuid(), "application:ready", callback);

        }

        public void onClosed()
        {
            closeCallback("Closed");
        }

        public void onError(string reason)
        {
            errorCallback(reason);
        }

        public void onMessage(string message)
        {
            //nothing of interest.
        }

        public void onOutgoingMessage(string message)
        {
            //nothing of interest.
        }

        public void onReady()
        {
            readyCallback(null);
        }
    }
}
