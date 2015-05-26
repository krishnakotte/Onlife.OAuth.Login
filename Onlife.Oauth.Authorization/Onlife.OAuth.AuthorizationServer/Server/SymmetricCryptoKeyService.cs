using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging.Bindings;
using Onlife.OAuth.AuthorizationServer.Server.Base;
using Onlife.OAuth.AuthorizationServer.Models;

namespace Onlife.OAuth.AuthorizationServer.Server
{
    public interface ISymmetricCryptoKeyService : IBaseService, ICryptoKeyStore
    {
        
    }

    public class SymmetricCryptoKeyService : BaseService, ISymmetricCryptoKeyService
    {
        public SymmetricCryptoKeyService() 
        {
        }


        public CryptoKey GetKey(string bucket, string handle)
        {
            var keys = DBContext.OAuth_SymmetricCryptoKeys.Where(k => k.Bucket == bucket && k.Handle == handle).ToList();
            // Perform a case senstive match
            IEnumerable<CryptoKey> matches = from key in keys
                                             where string.Equals(key.Bucket, bucket, StringComparison.Ordinal)
                                             && string.Equals(key.Handle, handle, StringComparison.Ordinal)
                                             select new CryptoKey(key.Secret.ToArray(), key.ExpiresDate.ToUniversalTime());
            return matches.FirstOrDefault();
        }

        public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket)
        {
            var keys = DBContext.OAuth_SymmetricCryptoKeys.Where(key => key.Bucket == bucket).OrderByDescending(x => x.ExpiresDate).ToList()
                .Select(key => new KeyValuePair<string, CryptoKey>(key.Handle, new CryptoKey(key.Secret.ToArray(), key.ExpiresDate.ToUniversalTime())));
            return keys.ToList();
        }

        public void StoreKey(string bucket, string handle, CryptoKey key)
        {
            var keyRow = new OAuth_SymmetricCryptoKey
            {
                Bucket = bucket,
                Handle = handle,
                Secret = key.Key,
                ExpiresDate = key.ExpiresUtc
            };

            DBContext.OAuth_SymmetricCryptoKeys.InsertOnSubmit(keyRow);
        }

        public void RemoveKey(string bucket, string handle)
        {
            var match = DBContext.OAuth_SymmetricCryptoKeys.FirstOrDefault(k => k.Bucket == bucket && k.Handle == handle);
            if (match != null)
            {
                DBContext.OAuth_SymmetricCryptoKeys.DeleteOnSubmit(match);
            }
        }
    }
}