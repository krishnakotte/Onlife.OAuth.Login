using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OAuth2.Messages;
using Onlife.OAuth.AuthorizationServer;
using Onlife.OAuth.AuthorizationServer.Models;
using StructureMap.Attributes;
using DNOA = DotNetOpenAuth.OAuth2;
using UrlHelper = System.Web.Mvc.UrlHelper;
using Onlife.OAuth.AuthorizationServer.Utilities;

namespace Authorization.Api.Attributes
{
    public class ResourceAuthenticatedAttribute : AuthorizeAttribute
    {
        [SetterProperty]
        public DNOA.IAuthorizationServerHost _AuthorizationServerHost { get; set; }

        [SetterProperty]
        public IOAuthResourceService _OAuthResourceService { get; set; }

        private OAuth_ResourceModel _targetResource;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            
            var authorizationServer = new DNOA.AuthorizationServer(_AuthorizationServerHost);

            // Figure out what resource the request is intending to access to see if the
            // user has already authenticated to with it
            EndUserAuthorizationRequest pendingRequest = authorizationServer.ReadAuthorizationRequest();
            if (pendingRequest == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Missing authorization request.");
            }

            try
            {
                if (!pendingRequest.Scope.Any())
                {
                    pendingRequest.Scope.Add("basic");
                }

                _targetResource = _OAuthResourceService.FindWithSupportedScopes(pendingRequest.Scope);

                // Above will return null if no resource supports all of the requested scopes
                if (_targetResource == null || _targetResource.Resource == null)
                {
                    throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Bad authorization request.");
                }
            }
            catch (Exception ex)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Bad authorization request.");
            }

            // User is considered authorized if in possession of token that originated from the resource's login page,
            // Name of token is determined by the resource configuration
            string tokenName = _targetResource.Resource.AuthenticationTokenName;
            string encryptedToken = httpContext.Request[tokenName]; //could be in cookie if previously logged in, or querystring if just logged in

            if (string.IsNullOrWhiteSpace(encryptedToken))
            {
                // No token, so unauthorized
                return false;
            }

            // Validate this thing came from us via shared secret with the resource's login page
            // The implementation here ideally could be generalized a bit better or standardized
            string encryptionKey = _targetResource.Resource.AuthenticationKey;
            string decryptedToken = EncodingUtility.Decode(encryptedToken, encryptionKey);
            string[] tokenContentParts = decryptedToken.Split(';');

            var username = tokenContentParts[0];
            var loginDate = DateTime.Parse(tokenContentParts[1]);

            if ((DateTime.UtcNow.Subtract(loginDate) > TimeSpan.FromDays(7)))
            {
                // Expired, remove cookie if present and flag user as unauthorized
                //httpContext.Response.Cookies.Remove(tokenName);
                return false;
            }

            // Things look good.  
            // Set principal for the authorization server
            IIdentity identity = new GenericIdentity(username);
            httpContext.User = new GenericPrincipal(identity, null);

            // If desired, persist cookie so user doesn't have to authenticate with the resource over and over
            //var cookie = new HttpCookie(tokenName, encryptedToken);
            //if (storeCookie)
            //{
            //    cookie.Expires = DateTime.UtcNow.AddDays(7); // could parameterize lifetime              
            //}
            //httpContext.Response.AppendCookie(cookie);
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Unauthorized, redirect user to the target resource's login page, telling it to
            // redirect back here when complete 
            filterContext.Result = new RedirectResult(string.Format("{0}?returnUrl={1}",
                _targetResource.Resource.AuthenticationUrl, new UrlHelper(filterContext.RequestContext).Encode(filterContext.RequestContext.HttpContext.Request.Url.ToString())));
        }
    }
}