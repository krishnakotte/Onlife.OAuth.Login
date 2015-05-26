using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Web;
using Onlife.OAuth.AuthorizationServer.Server;
using StructureMap.Attributes;
using DNOA = DotNetOpenAuth.OAuth2;
using Onlife.OAuth.AuthorizationServer.Server.Base;
using Onlife.OAuth.AuthorizationServer.Models;
using StructureMap;

namespace Onlife.OAuth.AuthorizationServer
{
    public interface IOAuthAuthorizationService
    {

        void CreateAuthorization(CreateAuthorizationOptions options);
        AuthenticatedUserData AuthenticateUser(HttpRequestBase request, HttpContext httpContext, string[] scopes);
    }

    public class OAuthAuthorizationService : BaseService, IOAuthAuthorizationService
    {
        //[SetterProperty]
        //public IUserManager _UserManager { get; set; }

        public OAuthAuthorizationService() 
        {
        }

        public void CreateAuthorization(CreateAuthorizationOptions options)
        {
            DBContext.OAuth_Authorizations.InsertOnSubmit(new OAuth_Authorization()
            {
                ClientId = options.ClientId,
                Scope = options.Scope,
                UserId = options.UserId,
                ResourceId = options.ResourceId
            });
        }

        public AuthenticatedUserData AuthenticateUser(HttpRequestBase request, HttpContext httpContext, string[] scopes)
        {
            AuthenticatedUserData userData = null;

            // Bail if no auth header or the header isn't bearing a token for us
            var authHeader = request.Headers["Authorization"];
            if (authHeader == null || string.IsNullOrEmpty(authHeader))
            {
                throw new AuthenticationException("Authorization header is missing.");
            }
            if (!authHeader.StartsWith("Bearer "))
            {
                throw new AuthenticationException("Bearer value is missing for Authorization header.");
            }

            // Have the DotNetOpenAuth resource server inspect the provided request using the configured keys
            // This checks both that the token is ok and that the token grants the scope required by
            // the required scope parameters to this attribute
            var Decrypter = ResourceServerKeyManager.GetDecrypter();
            var SignatureVerifier = new AuthorizationServerSigningKeyManagerService().GetSigner();

            var resourceServer = new DotNetOpenAuth.OAuth2.ResourceServer(new DNOA.StandardAccessTokenAnalyzer(SignatureVerifier, Decrypter));
            var principal = resourceServer.GetPrincipal(request, scopes);
            var accessToken = resourceServer.GetAccessToken(request, scopes);
            if (principal != null)
            {
                var login = principal.Identity.Name;
                userData = new AuthenticatedUserData("User Id goes here", accessToken.ClientIdentifier);
            }
            else
            {
                throw new AuthenticationException();
            }

            return userData;
        }
    }

    public class CreateAuthorizationOptions
    {
        public int ClientId { get; set; }
        public string Scope { get; set; }
        public int UserId { get; set; }
        public int ResourceId { get; set; }
    }
}