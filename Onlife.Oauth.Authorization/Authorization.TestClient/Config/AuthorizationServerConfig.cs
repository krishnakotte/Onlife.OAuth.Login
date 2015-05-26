using System;
using System.Configuration;

namespace Authorization.TestClient.Config
{
    public static class AuthorizationServerConfig
    {
        public static Uri AuthorizationEndpoint = new Uri(ConfigurationManager.AppSettings["AuthorizationServerAuthorizeUrl"]);
        public static Uri TokenEndpoint = new Uri(ConfigurationManager.AppSettings["AuthorizationServerTokenUrl"]);
    }
}