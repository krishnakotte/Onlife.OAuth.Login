using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Onlife.OAuth.AuthorizationServer.Server
{
    public static class ResourceServerKeyManager
    {
        // Responsible for providing the key to verify the token is intended for this resource
        public static RSACryptoServiceProvider GetDecrypter(string key = null)
        {
            if (key == null)
                key = ConfigurationManager.AppSettings["ResourceServerDecryptionKey"];
            var decrypter = new RSACryptoServiceProvider();
            var base64EncodedKey = key;
            decrypter.FromXmlString(Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedKey)));
            return decrypter;
        } 
    }
}