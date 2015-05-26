using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using StructureMap.Attributes;
using System.Configuration;

namespace Onlife.OAuth.AuthorizationServer.Server
{
    public interface IAuthorizationServerSigningKeyManagerService
    {
        RSACryptoServiceProvider GetSigner();
    }

    public class AuthorizationServerSigningKeyManagerService : IAuthorizationServerSigningKeyManagerService
    {
        public RSACryptoServiceProvider GetSigner()
        {
            var signer = new RSACryptoServiceProvider();
            var base64EncodedKey = ConfigurationManager.AppSettings["ResourceServerDecryptionKey"];
            signer.FromXmlString(Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedKey)));
            return signer;
        } 
    }
}