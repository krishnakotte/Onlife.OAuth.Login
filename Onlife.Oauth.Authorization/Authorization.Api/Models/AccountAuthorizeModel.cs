using System.Collections.Generic;
using DotNetOpenAuth.OAuth2.Messages;
using Onlife.OAuth.AuthorizationServer.Models;

namespace Authorization.Api.Models
{
    public class AccountAuthorizeModel
    {
        public OAuth_Clients Client { get; set; }
        public List<OAuth_Scope> Scopes { get; set; }
        public EndUserAuthorizationRequest AuthorizationRequest { get; set; }
    }
}