using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Onlife.OAuth.AuthorizationServer
{
    public class AuthenticatedUserData
    {
        public string UserId { get; set; }

        public string ClientId { get; set; }

        public AuthenticatedUserData()
        {

        }

        public AuthenticatedUserData(string userId, string clientId)
        {
            UserId = userId;
            ClientId = clientId;
        }
    }
}
