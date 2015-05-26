using System;
using System.Net.Http;
using Authorization.Api.Models;
using Onlife.OAuth.AuthorizationServer.Server;

namespace Authorization.Api.Helpers
{
    public class ApiRequestHelper
    {
        public static ApiRequestInfo GetRequestInfo(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("RequestId"))
            {
                return new ApiRequestInfo()
                {
                    RequestId = (Guid)request.Properties["RequestId"],
                    OAuthClientIdentifier = request.Properties.ContainsKey("OAuthClientIdentifier")
                        ? request.Properties["OAuthClientIdentifier"].ToString()
                        : ""
                };
            }
            else
            {
                try
                {
                    var resourceServer = new DotNetOpenAuth.OAuth2.ResourceServer(new DotNetOpenAuth.OAuth2.StandardAccessTokenAnalyzer(
                        new AuthorizationServerSigningKeyManagerService().GetSigner(), ResourceServerKeyManager.GetDecrypter()));
                    var accessToken = resourceServer.GetAccessToken(request);

                    return new ApiRequestInfo()
                    {
                        RequestId = Guid.NewGuid(),
                        OAuthClientIdentifier = accessToken.ClientIdentifier,
                        PersonId = (int?)null
                    };
                }
                catch (Exception)
                {
                    //authorization could not be gathered
                    return new ApiRequestInfo()
                    {
                        RequestId = Guid.NewGuid(),
                        OAuthClientIdentifier = null,
                        PersonId = (int?)null
                    };
                }
            }
        } 
    }
}