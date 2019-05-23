using System.Windows.Browser;

namespace ESRI.SilverlightViewer.Utility
{
    public class PageScriptHelper
    {
        private static bool _isJSWinAdded = false;

        /// <summary>
        /// Check if Window.js has been added into the Html Page 
        /// </summary>
        public static bool IsJavaScriptWindowAdded()
        {
            if (!_isJSWinAdded)
            {
                _isJSWinAdded = true;
                return false;
            }
            else
            {
                return true;
            }

            /// Following code does NOT work with Firefox and Chrome
            /*******************************************************
            HtmlElement pageHead = HtmlPage.Document.GetElementsByTagName("head")[0] as HtmlElement;

            foreach (ScriptObject htmlObj in pageHead.Children)
            {
                if (htmlObj is HtmlElement)
                {
                    string jsSrc = ((HtmlElement)htmlObj).GetAttribute("src");
                    if (jsSrc != null && jsSrc.Equals("Window.js", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        isJSAdded = true;
                        break;
                    }
                }
            }
            ********************************************************/
        }
    }
}
