using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebSphere.WebUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}/{*catchcall}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "ContentType/Index",
                url: "Admin",
                defaults: new { controller = "ContentType", action = "Index" }
            );

            routes.MapRoute(
                name: "ContentType/Group",
                url: "Admin/Group",
                defaults: new { controller = "ContentType", action = "Group" }
            );

            routes.MapRoute(
                name: "System-User",
                url: "System/User/",
                defaults: new { controller = "Account", action = "Index" }
            );

            routes.MapRoute(
                name: "System-Group",
                url: "System/Group/",
                defaults: new { controller = "Group", action = "Index" }
            );

            routes.MapRoute(
                name: "System-Log",
                url: "System/Log/",
                defaults: new { controller = "Log", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{*catchcall}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
