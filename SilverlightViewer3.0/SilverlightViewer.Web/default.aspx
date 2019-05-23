<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="ESRI.SilverlightViewer.Web._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
   <title>ArcGIS Template Viewer for Silverlight 2.4</title>
   <style type="text/css">
      html, body
      {
         height: 100%;
         overflow: auto;
      }
      
      body
      {
         padding: 0;
         margin: 0;
      }
      
      #silverlightControlHost
      {
         height: 100%;
         text-align: center;
         z-index: 0;
      }
   </style>
   <script type="text/javascript" src="Silverlight.js"></script>
   <script type="text/javascript">

      function onSilverlightError(sender, args) {
         var appSource = "";
         if (sender != null && sender != 0) {
            appSource = sender.getHost().Source;
         }

         var errorType = args.ErrorType;
         var iErrorCode = args.ErrorCode;

         if (errorType == "ImageError" || errorType == "MediaError") {
            return;
         }

         var errMsg = "Unhandled Error in Silverlight Application " + appSource + "\n";

         errMsg += "Code: " + iErrorCode + "    \n";
         errMsg += "Category: " + errorType + "       \n";
         errMsg += "Message: " + args.ErrorMessage + "     \n";

         if (errorType == "ParserError") {
            errMsg += "File: " + args.xamlFile + "     \n";
            errMsg += "Line: " + args.lineNumber + "     \n";
            errMsg += "Position: " + args.charPosition + "     \n";
         }
         else if (errorType == "RuntimeError") {
            if (args.lineNumber != 0) {
               errMsg += "Line: " + args.lineNumber + "     \n";
               errMsg += "Position: " + args.charPosition + "     \n";
            }
            errMsg += "MethodName: " + args.methodName + "     \n";
         }

         throw new Error(errMsg);
      }
   </script>
</head>
<body id="Body.ASP">
   <form id="form1" runat="server" style="height: 100%; overflow: hidden">
   <div id="silverlightControlHost">
      <object id="MapViewer" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
         <param name="source" value="ClientBin/ESRI.SilverlightViewer.xap" />
         <param name="onError" value="onSilverlightError" />
         <param name="initParams" value="<%#InitParameters%>" />
         <param name="windowless" value="false" />  <!-- Only IE8 supports windowless = true. Strongly suggest NOT change to true -->
         <param name="background" value="white" />
         <param name="minRuntimeVersion" value="4.0.50826.0" />
         <param name="autoUpgrade" value="true" />
         <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50826.0" style="text-decoration: none">
            <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight" style="border-style: none" />
         </a>
      </object>
      <iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px; border: 0px; display: none"></iframe>
   </div>
   </form>
</body>
</html>
<!-- Reset windowless parameter to false if the broswer is not IE8 -->
<script type="text/javascript">
   if (navigator.appName.indexOf('Microsoft') == -1 || parseFloat(navigator.appVersion) > 4) {
      var params = document.getElementsByTagName('param');
      for (var i = 0; i < params.length; i++) {
         if (params[i].name == 'windowless') {
            params[i].setAttribute('value', 'false');
            break;
         }
      }
   }
</script>
