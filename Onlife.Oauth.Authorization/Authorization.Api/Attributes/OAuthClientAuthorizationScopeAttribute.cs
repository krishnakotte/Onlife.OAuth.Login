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
using DotNetOpenAuth.Messaging;
using Onlife.OAuth.AuthorizationServer.Server;
using StructureMap.Attributes;
using DNOA = DotNetOpenAuth.OAuth2;

namespace Authorization.Api.Attributes
{
    // Attribute to apply to methods on WebAPI controller methods to restrict access to
    // those in possession of an authorization token with specified scopes
    public class OAuthClientAuthorizationScopeAttribute : AuthorizeAttribute
    {
        //[SetterProperty]
        //public ILoginManager _LoginManager { get; set; }
        //[SetterProperty]
        //public IAuthenticateRequestService _AuthenticateRequestService { get; set; }

        private static readonly RSACryptoServiceProvider Decrypter;
        private static readonly RSACryptoServiceProvider SignatureVerifier;

        // Get the keys from wherever they are stored
        static OAuthClientAuthorizationScopeAttribute()
        {
            Decrypter = ResourceServerKeyManager.GetDecrypter();
            SignatureVerifier = new AuthorizationServerSigningKeyManagerService().GetSigner();
        }

        // Which scopes are required to gain access
        public string[] RequiredScopes { get; set; }

        public OAuthClientAuthorizationScopeAttribute(params string[] requiredScopes)
        {
            RequiredScopes = requiredScopes;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                base.OnAuthorization(actionContext);

                // Bail if no auth header or the header isn't bearing a token for us
                var authHeader = actionContext.Request.Headers.FirstOrDefault(x => x.Key == "Authorization");
                if (authHeader.Value == null || !authHeader.Value.Any())
                {
                    throw new AuthenticationException("Authorization header is missing.");
                }
                var authHeaderValue = authHeader.Value.FirstOrDefault(x => x.StartsWith("Bearer "));
                if (authHeaderValue == null)
                {
                    throw new AuthenticationException("Bearer value is missing for Authorization header.");
                }

                // Have the DotNetOpenAuth resource server inspect the provided request using the configured keys
                // This checks both that the token is ok and that the token grants the scope required by
                // the required scope parameters to this attribute
                var resourceServer =
                    new DNOA.ResourceServer(new DNOA.StandardAccessTokenAnalyzer(SignatureVerifier, Decrypter));
                var principal = resourceServer.GetPrincipal(actionContext.Request, RequiredScopes);
                var accessToken = resourceServer.GetAccessToken(actionContext.Request, RequiredScopes);
                if (principal != null)
                {
                    //_AuthenticateRequestService = new AuthenticateRequestService(
                    //    new WebHttpContext(HttpContext.Current), new WebAuthentication());
                    //var identity = _AuthenticateRequestService.GetOAuthClientIdentity(accessToken.ClientIdentifier);
                    //var userPrinciple = new AuthenticatedPrincipal(identity);

                    //// Things look good.  Set principal for the resource to use in identifying the user so it can act accordingly
                    Thread.CurrentPrincipal = principal;
                    HttpContext.Current.User = principal;

                    actionContext.Request.Properties.Add("RequestId", Guid.NewGuid());
                    actionContext.Request.Properties.Add("OAuthClientIdentifier", accessToken.ClientIdentifier);
                    // Don't understand why the call to GetPrincipal is setting actionContext.Response to be unauthorized
                    // even when the principal returned is non-null
                    // If I do this code the same way in a delegating handler, that doesn't happen
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
