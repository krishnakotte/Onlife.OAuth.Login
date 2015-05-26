using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Authorization.Api
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "OAuth2",
                url: "oauth2/{action}",
                defaults: new { controller = "OAuth" }
            );

            routes.MapRoute(
               name: "values",
               url: "value/{action}",
               defaults: new { controller = "Values" }
           );

            routes.MapRoute(
             name: "samples",
             url: "sample/{action}",
             defaults: new { controller = "Sample" }
         );

            routes.MapHttpRoute(
                "Api",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { action = "Get", id = RouteParameter.Optional }  // Parameter defaults
            );
        }
    }
}