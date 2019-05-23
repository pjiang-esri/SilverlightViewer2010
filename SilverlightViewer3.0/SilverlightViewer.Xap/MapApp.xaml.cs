using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.IO.IsolatedStorage;
using ESRI.SilverlightViewer.Config;

namespace ESRI.SilverlightViewer
{
    [ScriptableType]
    public partial class MapApp : Application
    {
        // BING parameters passed from the Host Web Application
        public const string BING_TOKEN = "BingToken";
        public const string BING_SERVER = "BingServer";

        public bool IsWindowless { get; private set; }
        public ApplicationConfig AppConfig { get; private set; }

        public MapApp()
        {
            this.Startup += this.OnStartup;
            this.Exit += this.OnExit;
            this.UnhandledException += this.Application_UnhandledException;
            this.IsWindowless = false;

            InitializeComponent();
        }

        // Get Silverlight Parameter Values
        public string GetAppParamValue(string key)
        {
            string value = "";
            if (this.Resources.Contains(key))
            {
                value = this.Resources[key] as string;
            }

            return value;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            foreach (var item in e.InitParams)
            {
                this.Resources.Add(item.Key, item.Value);
            }

            foreach (HtmlElement elem in HtmlPage.Plugin.Children)
            {
                if ("windowless".Equals(elem.GetProperty("name") as string, StringComparison.CurrentCultureIgnoreCase))
                {
                    IsWindowless = "true".Equals(elem.GetProperty("value") as string, StringComparison.CurrentCultureIgnoreCase);
                    break;
                }
            }

            //Register Scriptable Object for the Application
            HtmlPage.RegisterScriptableObject(HtmlPage.Plugin.Id, this);

            //Load Configuration File
            LoadViewerConfigXML();
        }

        private void LoadViewerConfigXML()
        {
            WebClient xmlClient = new WebClient();
            xmlClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadConfigXMLCompleted);
            xmlClient.DownloadStringAsync(new Uri("AppConfig.xml", UriKind.Relative));
        }

        private void DownloadConfigXMLCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string themeName = GetAppParamValue("Theme");

            try
            {
                AppConfig = ApplicationConfig.Deserialize(e.Result);
                if (AppConfig == null)
                {
                    MessageBox.Show("Sorry! Loading the application configuration failed. Please validate configuration file AppConfig.xml");
                    return;
                }

                if (string.IsNullOrEmpty(themeName))
                {
                    themeName = AppConfig.Theme.Trim();
                }

                if (!string.IsNullOrEmpty(themeName))
                {
                    //if (!UriParser.IsKnownScheme("pack")) UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), "pack", -1); 
                    string themeUri = string.Format("/ESRI.SilverlightViewer;component/Themes/{0}.xaml", themeName);
                    ResourceDictionary themeRes = new ResourceDictionary();
                    themeRes.Source = new Uri(themeUri, UriKind.Relative);
                    this.Resources.MergedDictionaries.Add(themeRes);
                }

                ResourceDictionary SymbolRes = new ResourceDictionary();
                SymbolRes.Source = new Uri("/ESRI.SilverlightViewer;component/Symbols/EsriSymbols.xaml", UriKind.Relative);
                this.Resources.MergedDictionaries.Add(SymbolRes);

                // Load the main control here
                MapPage mapPage = new MapPage();
                this.RootVisual = mapPage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load Theme Error: " + ex.Message);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + @"\n" + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace("\"", "\\\"").Replace("\r\n", @"\n");

                HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight 2 Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// A special help function to be used to get the printable content of Widgets
        /// </summary>
        /// <param name="isolatedFile">The name of an IsolatedStorageFile </param>
        /// <returns>The string of the print content</returns>
        [ScriptableMember]
        public string GetPrintContent(string isolatedFile)
        {
            string content = "";

            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.FileExists(isolatedFile))
            {
                using (IsolatedStorageFileStream isoStream = isf.OpenFile(isolatedFile, FileMode.Open))
                {
                    TextReader textReader = new StreamReader(isoStream);
                    content = textReader.ReadToEnd();
                    textReader.Close();
                }
            }

            return content;
        }

        /// <summary>
        /// Set Browser Window Status - Doesn't support FireFox
        /// </summary>
        /// <param name="statusMsg">Status Message</param>
        public void SetWindowStatus(string statusMsg)
        {
            // Works with IE only
            if (HtmlPage.BrowserInformation.ProductName.Equals("MSIE"))
            {
                HtmlPage.Window.SetProperty("status", statusMsg);
            }
        }
    }
}