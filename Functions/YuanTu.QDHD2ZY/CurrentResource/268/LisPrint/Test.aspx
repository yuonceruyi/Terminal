<%@ Page Language="C#"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ftp测试</title>
    <link rel="shortcut icon" href="../../resource/common/images/favicon.ico" />
    <script src="../../resource/easyUI/jquery.min.js" type="text/javascript"></script>
    <script type="text/jscript">
        $(function () {
            $.ajax({
                type: "GET",
                dataType: "text", //text, json, xml
                cache: false, //
                async: false, //为true时，异步，不等待后台返回值，为false时强制等待；-asir
                url: '../ashx/ashTest.ashx?Method=Test',
                success: function (result, status) {
                    //alert(result);
                }
            });
        });
 

function startApp() {
    var evt = document.createEvent("CustomEvent");
  
    var printFlag = "0";       ///0:打印所有报告 1:循环打印每一份报告
    var connectString = "http://localhost/csp/lis/LIS.WS.DHCLISService.cls";
    var rowids = "45684";
	var userCode = "demo";
    var printType = "PrintPreview";    ///PrintOut:打印  PrintPreview打印预览
    var paramList = "1";               ///1:报告处理打印 2:自助打印 3:医生打印
    var claName = "";
    var funName = ""
 	var Param = printFlag +"@" + connectString +"@" + rowids +"@" + userCode +"@" + printType +"@" + paramList +"@" + claName +"@" + funName
 	alert(Param);
    evt.initCustomEvent('myCustomEvent', true, false, Param);
    // fire the event
    document.dispatchEvent(evt);
}

    </script>
</head>
<body>
   <img id="img" alt="图片" />
   <button type="button" onClick="startApp()" id="startApp">startApp</button>

</body>
</html>
