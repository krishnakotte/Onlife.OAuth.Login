using System;
using System.Security.Cryptography;
using System.Text;
using Onlife.OAuth.AuthorizationServer.Server;

namespace Onlife.OAuth.AuthorizationServer.Models
{
    public class OAuth_ResourceModel
    {
        public OAuth_ResourceModel(OAuth_Resource resource)
        {
            Resource = resource;
        }

        public OAuth_Resource Resource
        {
            get;
            set; 
        }

        private RSACryptoServiceProvider _publicTokenEncrypter;
        public RSACryptoServiceProvider PublicTokenEncrypter
        {
            get
            {
                if (_publicTokenEncrypter == null)
                {
                    lock (this)
                    {
                        _publicTokenEncrypter = ResourceServerKeyManager.GetDecrypter(Resource.PublicTokenEncryptionKey);
                    }
                }
                return _publicTokenEncrypter;
            }
        }
    }
}