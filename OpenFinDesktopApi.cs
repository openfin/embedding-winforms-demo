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

            openFinConnection = new DesktopConnection("c# demo endpoint", "localhost", 9696);
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
        
        public void embedWindowInTarget(Openfin.Desktop.Window window, IntPtr containerhWnd)
        {
            window.getNativeId((ack) =>
            {
                var windowhWnd = DesktopUtils.getJSONString(ack.getJsonObject(), "data");

                embedWindow(hWndPonterFromHexString(windowhWnd), containerhWnd, 0, 0, 742, 484);
                window.show();

            });
        }

        private void onApplicationReady(Application application,  InterAppMessageHandler callback)
        {
            openFinConnection
                .getInterApplicationBus()
                .subscribe(application.getUuid(), "application:ready", callback);

        }

        private IntPtr hWndPonterFromHexString(string hex)
        {
            long id;

            if (hex != null && hex.StartsWith("0x"))
            {
                hex = hex.Substring(2);
            }

            long.TryParse(hex, System.Globalization.NumberStyles.AllowHexSpecifier, null, out id);

            return new IntPtr(id);

        }

        private void embedWindow(IntPtr childWindowHandle, IntPtr containerHandle, int x, int y, int newWidth, int newHeight)
        {
            SetParent(childWindowHandle, containerHandle);
            MoveWindow(childWindowHandle, 2, 2, 2, 2, true);
            MoveWindow(childWindowHandle, x, y, newWidth, newHeight, true);
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWmd, int x, int y, int nWidth, int nHeight, bool bRepaint);
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
