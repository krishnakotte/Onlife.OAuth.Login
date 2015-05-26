using System;
using System.Collections.Generic;
using System.Linq;
using Onlife.OAuth.AuthorizationServer.Models;
using Onlife.OAuth.AuthorizationServer.Server.Base;

namespace Onlife.OAuth.AuthorizationServer
{
    public interface IOAuthClientService : IBaseService
    {
        OAuth_Clients GetClient(string clientIdentifier);
    }

    public class OAuthClientService : BaseService, IOAuthClientService
    {
        public OAuthClientService() : base()
        {
        }

        public OAuth_Clients GetClient(string clientIdentifier)
        {
            var client = new OAuth_Clients(DBContext.OAuth_Clients.FirstOrDefault(x => x.ClientIdentifier.ToLower().Equals(clientIdentifier)));

            if (client.Client == null)
                return null;

            //client.Scopes = DBContext.OAuth_ClientScopes.Where(x => x.ClientId == clientIdentifier).ToList();

            return client;
        }

       
    }
}