using System;
using System.Collections.Generic;
using System.Linq;
using Onlife.OAuth.AuthorizationServer.Models;
using DotNetOpenAuth.Messaging.Bindings;
using DotNetOpenAuth.OAuth2;
using DotNetOpenAuth.OAuth2.ChannelElements;
using DotNetOpenAuth.OAuth2.Messages;
using StructureMap.Attributes;

namespace Onlife.OAuth.AuthorizationServer.Server
{
    // Our implementation of the authorization server, which is passed to the ctor of the 
    // DotNetOpenAuth AuthroizationServer class.  The DotNetOpenAuth impl calls methods in this
    // class when it sees fit.
    public class AuthorizationServerHost : IAuthorizationServerHost
    {

        //private readonly ClientRepository _clientRepository = new ClientRepository();
        //private readonly ResourceRepository _resourceRepository = new ResourceRepository();
        //private readonly ICryptoKeyStore _cryptoKeyRepository = new SymmetricCryptoKeyRepository();
        //private readonly INonceStore _nonceRepository = new NonceRepository();
        //private readonly AuthorizationRepository _authorizationRepository = new AuthorizationRepository();
        //private readonly AuthorizationServerSigningKeyManager _tokenSigner = new AuthorizationServerSigningKeyManager();


        [SetterProperty]
        public ISymmetricCryptoKeyService _SymmetricCryptoKeyService { get; set; }

        [SetterProperty]
        public INonceStoreService _NonceStoreService { get; set; }

        private IOAuthClientService _OAuthClientService = new OAuthClientService();

        public IOAuthResourceService _OAuthResourceService = new OAuthResourceService();

        public ICryptoKeyStore CryptoKeyStore { get { return new SymmetricCryptoKeyService(); } }
        public INonceStore NonceStore { get { return _NonceStoreService; } }

        // Generate an access token, given parameters in request that tell use what scopes to include,
        // and thus what resource's encryption key to use in addition to the authroization server key
        public AccessTokenResult CreateAccessToken(IAccessTokenRequest accessTokenRequestMessage)
        {
            var accessToken = new AuthorizationServerAccessToken { Lifetime = TimeSpan.FromSeconds(60) }; // could parameterize lifetime
            var scope = accessTokenRequestMessage.Scope;
            if (scope == null || !scope.Any())
            {
                scope = new HashSet<string>()
                {
                    "basic"
                };
            }

            var targetResource = _OAuthResourceService.FindWithSupportedScopes(scope);

            accessToken.ResourceServerEncryptionKey = targetResource.PublicTokenEncrypter;
            accessToken.AccessTokenSigningKey = new AuthorizationServerSigningKeyManagerService().GetSigner();

            var result = new AccessTokenResult(accessToken);
            return result;
        }

        // Lookup client given an identifier
        public IClientDescription GetClient(string clientIdentifier)
        {
            IClientDescription client = _OAuthClientService.GetClient(clientIdentifier);
            if (client == null)
            {
                throw new ArgumentOutOfRangeException("clientIdentifier");
            }
            return client;
        }

        // Determine whether the given authorization is still ok
        public bool IsAuthorizationValid(IAuthorizationDescription authorization)
        {
            // If db precision exceeds token time precision (which is common), the following query would
            // often disregard a token that is minted immediately after the authorization record is stored in the db.
            // To compensate for this, we'll increase the timestamp on the token's issue date by 1 second.
            //var user = UnitOfWork.Users.SingleByLogin(authorization.User);
            //var grantedAuths = UnitOfWork.OAuth_Authorization.FindCurrent(authorization.ClientIdentifier, user.UserId,
            //                                                        authorization.UtcIssued + TimeSpan.FromSeconds(1)).ToList();

            //if (!grantedAuths.Any())
            //{
            //    // No granted authorizations prior to the issuance of this token, so it must have been revoked.
            //    // Even if later authorizations restore this client's ability to call in, we can't allow
            //    // access tokens issued before the re-authorization because the revoked authorization should
            //    // effectively and permanently revoke all access and refresh tokens.
            //    return false;
            //}

            //// Determine the set of all scopes the user has authorized for this client
            //var grantedScopes = new HashSet<string>(OAuthUtilities.ScopeStringComparer);
            //foreach (var auth in grantedAuths)
            //{
            //    grantedScopes.UnionWith(OAuthUtilities.SplitScopes(auth.Scope));
            //}

            // See if what's requested is authorized
            //return authorization.Scope.IsSubsetOf(grantedScopes);
            return true;
        }

        // Used during client credentials flow.  Before we get here, the client and secret will already have been verified
        // We're also ensuring the scopes requested are ok to give the client
        public AutomatedAuthorizationCheckResponse CheckAuthorizeClientCredentialsGrant(IAccessTokenRequest accessRequest)
        {
            // Find the client
            var client = _OAuthClientService.GetClient(accessRequest.ClientIdentifier);

            // Check if the scopes that are being requested are a subset of the scopes the user is authorized for.
            // If not, that means that the user has requested at least one scope it is not authorized for
            var clientIsAuthorizedForRequestedScopes = accessRequest.Scope.IsSubsetOf(client.Scopes.Select(x => x.Identifier));

            // The token request is approved when the client is authorized for the requested scopes
            var isApproved = clientIsAuthorizedForRequestedScopes;

            return new AutomatedAuthorizationCheckResponse(accessRequest, isApproved);
        }

        public AutomatedUserAuthorizationCheckResponse CheckAuthorizeResourceOwnerCredentialGrant(string userName, string password, IAccessTokenRequest accessRequest)
        {
            if (!accessRequest.ClientAuthenticated)
                throw new Exception("Unable to authenticate the client credentials");
            if (userName == "krishna" && password == "kotte")
            {
                // Find the client
                var client = _OAuthClientService.GetClient(accessRequest.ClientIdentifier);
                // Check if the scopes that are being requested are a subset of the scopes the user is authorized for.
                // If not, that means that the user has requested at least one scope it is not authorized for
                //var clientIsAuthorizedForRequestedScopes = accessRequest.Scope.IsSubsetOf(client.Scopes.Select(x => x.Identifier));

                //// The token request is approved when the client is authorized for the requested scopes
                //var isApproved = clientIsAuthorizedForRequestedScopes;
                //if (!isApproved)
                //    return new AutomatedUserAuthorizationCheckResponse(accessRequest, false, userName);
                //else
                    return new AutomatedUserAuthorizationCheckResponse(accessRequest, true, userName);
            }

            return new AutomatedUserAuthorizationCheckResponse(accessRequest, false, userName);
        }
    }
}