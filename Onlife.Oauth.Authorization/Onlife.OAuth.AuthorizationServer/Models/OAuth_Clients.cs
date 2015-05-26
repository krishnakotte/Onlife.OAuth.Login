using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

namespace Onlife.OAuth.AuthorizationServer.Models
{
    public class OAuth_Clients : IClientDescription
    {
        public OAuth_Clients(OAuth_Client client)
        {
            Client = client;
        }

        public OAuth_Client Client { get; set; }
        public IList<OAuth_Scope> Scopes { get; set; }

        // These members are used internally by DotNetOpenAuth
        #region IClientDescription        

        Uri IClientDescription.DefaultCallback
        {
            get { return string.IsNullOrEmpty(Client.RedirectUrl) ? null : new Uri(Client.RedirectUrl); }
        }

        ClientType IClientDescription.ClientType
        {
            get { return (ClientType)Client.ClientType; }
        }
               
        bool IClientDescription.HasNonEmptySecret
        {
            get { return !string.IsNullOrEmpty(Client.ClientSecret); }
        }

        // Determines whether a callback URI included in a client's authorization request
        // is among those allowed callbacks for the registered client.        
        bool IClientDescription.IsCallbackAllowed(Uri cbUri)
        {
            if (string.IsNullOrEmpty(Client.RedirectUrl))
            {
                // No callback rules have been set up for this client.
                return true;
            }

            // For security purposes, we're requiring an identical match to what was configured for the client
            if (cbUri.ToString() == Client.RedirectUrl)
            {
                return true;
            }

            return false;
        }

        // Checks whether the specified client secret is correct.
        // Returns true if the secret matches the one in the authorization server's record for the client, false otherwise
        bool IClientDescription.IsValidClientSecret(string secret)
        {
            // Could hash this to avoid storing the secret in db
            return MessagingUtilities.EqualsConstantTime(secret, Client.ClientSecret);
        }

        #endregion
    }
}