<%@ page language="java" contentType="text/html; charset=ISO-8859-1" pageEncoding="ISO-8859-1"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<%
String IsolatedFile = request.getParameter("file");
%>
<html>
<head>
    <title>SilverlightViewer - Print Page</title>

    <script language="javascript" type="text/javascript">
        var isolatedFile = "<%=IsolatedFile%>";

        function OnPageLoad() {
            var content = opener.getPrintContent(isolatedFile);
            document.getElementById("divPrintContent").innerHTML = content;
        }
    </script>

    <style type="text/css">        
        .ValueTable
        {
            font-size: 9px;
            font-family: Arial;
            padding: 4px;
        }
        
        .TableTitle
        {
            font-size: 10px;
            font-weight: 800;
            text-align: center;
            padding: 4px 0px 4px 0px;
        }
    </style>
</head>
<body onload="javascript: OnPageLoad();">
    <form id="form1">
    <div id="divPrintContent">
    </div>
    </form>
</body>
</html>