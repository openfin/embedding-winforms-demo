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

        public const string OPENFIN_HOST = "127.0.0.1";
        public const int OPENFIN_PORT = 9696;
        public const string OPENFIN_VERSION = "beta";

        public OpenFinDesktopApi(stringParamDelegate onReady, stringParamDelegate onError, stringParamDelegate onClose)
        {
            readyCallback = onReady;
            errorCallback = onError;
            closeCallback = onClose;

            openFinConnection = new DesktopConnection("c# Embed OpenFin Window", OPENFIN_HOST, OPENFIN_PORT);

            openFinConnection.connectToVersion(OPENFIN_VERSION, this);
        }
        public void createApplication(string name, string url, appicationParamDelegate callback)
        {
            //We will create a hidden frameless application
            var appOptions = new ApplicationOptions(name, name, url);
            
            appOptions.MainWindowOptions.Frame = false;
            appOptions.MainWindowOptions.AutoShow = false;

            var app = new Openfin.Desktop.Application(appOptions, openFinConnection);

            //Make sure the application is ready before we execute the callback
            onApplicationReady(app, (a,b,c) => {
                callback(app);
            });

            app.run();
        }

        private void onApplicationReady(Application application,  InterAppMessageHandler callback)
        {
            //The Application will send a ready message once its done bootstrappning.
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
            System.Diagnostics.Debug.WriteLine(message);
            //nothing of interest.
        }

        public void onOutgoingMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            //nothing of interest.
        }

        public void onReady()
        {
            readyCallback(null);
        }
    }
}
