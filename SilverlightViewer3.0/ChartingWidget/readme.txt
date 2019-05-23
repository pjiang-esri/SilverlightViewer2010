How to use this template?

1. Create a new solution with Visual Studio 2010
2. Add the SilverlightViewer.Web project into the solution
3. Add a new project with this Template under the same folder as the Silverlight.Web project
4. If this project is not referencing to any other assemblies than those used in the SilverlightViewer.Xap 
   project, you may change Post-Build event in the project properties to

	cd $(ProjectDir)
	copy $(OutDir)$(ProjectName).dll  ..\SilverlightViewer.Web\ClientBin
	echo DONE

5. When add a reference to those assemblies used in the SilverlightViewer.Xap project, please change the 
   reference's "Copy Local" property to 'false'

6. Referenced assemblies in the SilverlightViewer.Xap project include

	ESRI.SilverlightViewer.dll
	ESRI.SilverlightViewer.Controls.dll

   and

	11/29/2010  06:54 PM    ESRI.ArcGIS.Client.Bing.dll
	11/29/2010  06:54 PM    ESRI.ArcGIS.Client.dll
	11/29/2010  06:54 PM    ESRI.ArcGIS.Client.Toolkit.dll
	08/26/2010  02:17 AM    System.ComponentModel.DataAnnotations.dll
	08/26/2010  02:17 AM    System.Runtime.Serialization.Json.dll
	08/26/2010  02:17 AM    System.Windows.Controls.Data.dll
	08/26/2010  02:17 AM    System.Windows.Controls.Data.Input.dll
	08/26/2010  02:17 AM    System.Windows.Controls.dll
	04/13/2010  08:15 PM    System.Windows.Controls.Toolkit.dll
	04/13/2010  07:59 PM    System.Windows.Controls.Toolkit.Internals.dll
	08/26/2010  02:17 AM    System.Windows.Data.dll
	08/26/2010  02:17 AM    System.Xml.Linq.dll
	08/26/2010  02:17 AM    System.Xml.Serialization.dll


6. Please follow the instructions in the README file included in the SilverlightViewer package to learn
   how to add this widget into the viewer and config it