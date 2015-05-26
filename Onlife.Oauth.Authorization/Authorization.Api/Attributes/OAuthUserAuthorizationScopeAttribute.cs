using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Authorization.Api.DependencyResolution;
using DotNetOpenAuth.Messaging;
using Onlife.OAuth.AuthorizationServer;
using Onlife.OAuth.AuthorizationServer.Server;
using StructureMap.Attributes;
using DNOA = DotNetOpenAuth.OAuth2;

namespace Authorization.Api.Attributes
{
    // Attribute to apply to methods on WebAPI controller methods to restrict access to
    // those in possession of an authorization token with specified scopes
    public class OAuthUserAuthorizationScopeAttribute : AuthorizeAttribute
    {
        [SetterProperty]
        public IOAuthAuthorizationService _OAuthAuthorizationService { get; set; }

        ////[SetterProperty]
        //public IAuthenticateRequestService _AuthenticateRequestService { get; set; }

        private static readonly RSACryptoServiceProvider Decrypter;
        private static readonly RSACryptoServiceProvider SignatureVerifier;

        // Get the keys from wherever they are stored
        static OAuthUserAuthorizationScopeAttribute()
        {
            Decrypter = ResourceServerKeyManager.GetDecrypter();
            SignatureVerifier = new AuthorizationServerSigningKeyManagerService().GetSigner();
        }

        // Which scopes are required to gain access
        public string[] RequiredScopes { get; set; }

        public OAuthUserAuthorizationScopeAttribute(params string[] requiredScopes)
        {
            RequiredScopes = requiredScopes;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                var overridingAttributes = actionContext.ActionDescriptor.GetCustomAttributes<OAuthClientAuthorizationScopeAttribute>();
                if (overridingAttributes != null && overridingAttributes.Any() && overridingAttributes[0] != null)
                    return;


                base.OnAuthorization(actionContext);

                var dependancy = actionContext.Request.Properties["MS_DependencyScope"] as StructureMapDependencyResolver;
                //var _OAuthAuthorizationService = dependancy.GetInstance<IOAuthAuthorizationService>();
                _OAuthAuthorizationService = new OAuthAuthorizationService();

                var context = new HttpContextWrapper(HttpContext.Current);
                HttpRequestBase request = context.Request;

                var userData = _OAuthAuthorizationService.AuthenticateUser(request, HttpContext.Current,
                    RequiredScopes);
                if (userData != null)
                {
                    //_AuthenticateRequestService = new AuthenticateRequestService(new WebHttpContext(HttpContext.Current), new WebAuthentication());
                    //var identity = _AuthenticateRequestService.GetIdentity(userData);
                    //var userPrinciple = new AuthenticatedPrincipal(identity);

                    //// Things look good.  Set principal for the resource to use in identifying the user so it can act accordingly
                    //Thread.CurrentPrincipal = ;
                    //HttpContext.Current.User = userPrinciple;

                    //actionContext.Request.Properties.Add("RequestId", Guid.NewGuid());
                    //actionContext.Request.Properties.Add("PersonId", identity.UserContext.PersonId);
                    //actionContext.Request.Properties.Add("OAuthClientIdentifier", identity.UserContext.OAuthClientIdentifier);
                    //// Don't understand why the call to GetPrincipal is setting actionContext.Response to be unauthorized
                    //// even when the principal returned is non-null
                    //// If I do this code the same way in a delegating handler, that doesn't happen
                    actionContext.Response = null;
                }
                else
                {
                    throw new AuthenticationException("Access token not found.");
                }
            }
            catch (SecurityTokenValidationException ex)
            {
                //var logger = new Logger();
                //logger.LogError(ex);

                throw new AuthenticationException();
            }
            catch (ProtocolFaultResponseException ex)
            {
                //var logger = new Logger();
                //logger.LogError(ex);
                throw new AuthenticationException();
            }
            catch (AuthenticationException ex)
            {
                //var logger = new Logger();
                //logger.LogError(ex);

                throw new HttpException((int)HttpStatusCode.BadRequest, "Authentication credentials were missing or incorrect.");
            }
            catch (Exception ex)
            {
                //var logger = new Logger();
                //logger.LogError(ex);

                throw new HttpException((int)HttpStatusCode.BadRequest, "Authentication credentials were missing or incorrect.");
            }
        }
    }
}
