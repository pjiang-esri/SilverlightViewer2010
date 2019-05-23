using System;

namespace ESRI.SilverlightViewer.Web
{
    public partial class _Default : System.Web.UI.Page
    {
        public string InitParameters = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            InitParameters = string.Format("Theme={0}", Request.QueryString["theme"]);

            DataBind();
        }
    }
}
