using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Authorization.TestClient.Config;
using DotNetOpenAuth.OAuth2;

namespace Authorization.TestClient.Attributes
{
    public class OAuthAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var auth = (AuthorizationState)httpContext.Session["Authorization"];
            
            if (auth == null)
                return false;

            try
            {
                if (auth.AccessTokenExpirationUtc.HasValue)
                {
                    if (ClientConfig.AuthorizationServerClient.RefreshAuthorization(auth, TimeSpan.FromSeconds(30)))
                    {
                        httpContext.Session["Authorization"] = auth;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Unauthorized, redirect user to the target resource's login page, telling it to
            // redirect back here when complete
            filterContext.Result = new RedirectResult("/Home/Welcome");
        }
    }
}
