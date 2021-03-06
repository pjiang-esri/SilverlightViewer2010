<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<%@page language="java" contentType="text/html; charset=ISO-8859-1" pageEncoding="ISO-8859-1"%>
<%
String theme = request.getParameter("theme");

String initParams = String.format("Theme=%s", (theme != null) ? theme : "");
%>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=ISO-8859-1">
    <title>Silverlight API for ArcGIS Server Viewer</title>
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
        }
    </style>
    <script type="text/javascript" src="Silverlight.js"></script>
    <script type="text/javascript">
        var silverlightApp = null;

        function onPluginLoaded(sender, args) {
            silverlightApp = sender.getHost().content.MapViewer;
        }

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

        function getPrintContent(isolatedFile) {
            var content = silverlightApp.GetPrintContent(isolatedFile);
            return content;
        }
    </script>
</head>
<body id="Body.JSP">
    <form id="form1" style="height: 100%">
    <div id="silverlightControlHost">
        <object id="MapViewer" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
            <param name="source" value="ClientBin/ESRI.SilverlightViewer.xap" />
            <param name="onLoad" value="onPluginLoaded" />
            <param name="onError" value="onSilverlightError" />
            <param name="initParams" value="<%=initParams%>" />
            <param name="windowless" value="false" />  <!-- Only IE8 supports windowless = true. Stronly suggest not set to true -->
            <param name="background" value="white" />
            <param name="minRuntimeVersion" value="2.0.31005.0" />
            <param name="autoUpgrade" value="true" />
            <a href="http://go.microsoft.com/fwlink/?LinkID=124807" style="text-decoration: none;">
               <img src="http://go.microsoft.com/fwlink/?LinkId=108181" alt="Get Microsoft Silverlight" style="border-style: none"/>
            </a>
        </object>
        <iframe id="_sl_historyFrame" style="visibility: hidden; height: 0px; width: 0px; border: 0px"></iframe>
    </div>
    </form>
</body>

</html>
<!-- Reset windowless parameter if the broswer is not IE8 -->
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