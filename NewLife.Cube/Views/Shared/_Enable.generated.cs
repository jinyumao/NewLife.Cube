﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    
    #line 4 "..\..\Views\Shared\_Enable.cshtml"
    using System.Linq;
    
    #line default
    #line hidden
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    
    #line 1 "..\..\Views\Shared\_Enable.cshtml"
    using NewLife;
    
    #line default
    #line hidden
    using NewLife.Cube;
    using NewLife.Reflection;
    
    #line 2 "..\..\Views\Shared\_Enable.cshtml"
    using NewLife.Web;
    
    #line default
    #line hidden
    
    #line 3 "..\..\Views\Shared\_Enable.cshtml"
    using XCode;
    
    #line default
    #line hidden
    using XCode.Membership;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Shared/_Enable.cshtml")]
    public partial class _Views_Shared__Enable_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Views_Shared__Enable_cshtml()
        {
        }
        public override void Execute()
        {
            
            #line 5 "..\..\Views\Shared\_Enable.cshtml"
  
    var page = ViewBag.Page as Pager;

    var dic = new Dictionary<Int32, String>();
    dic.Add(1, "启用");
    dic.Add(0, "禁用");

            
            #line default
            #line hidden
WriteLiteral("\n<div");

WriteLiteral(" class=\"form-group\"");

WriteLiteral(">\n    <label");

WriteLiteral(" for=\"enable\"");

WriteLiteral(" class=\"control-label\"");

WriteLiteral(">状态：</label>\n");

WriteLiteral("    ");

            
            #line 14 "..\..\Views\Shared\_Enable.cshtml"
Write(Html.ForDropDownList("enable", dic, page["enable"], "全部", true));

            
            #line default
            #line hidden
WriteLiteral("\n</div>\n");

        }
    }
}
#pragma warning restore 1591
