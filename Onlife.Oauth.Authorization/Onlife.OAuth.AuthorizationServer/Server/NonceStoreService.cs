using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging.Bindings;
using StructureMap.Attributes;
using Onlife.OAuth.AuthorizationServer.Server.Base;
using Onlife.OAuth.AuthorizationServer.Models;

namespace Onlife.OAuth.AuthorizationServer.Server
{
    public interface INonceStoreService : IBaseService, INonceStore
    {
    }

    public class NonceStoreService : BaseService, INonceStoreService
    {
        public NonceStoreService()
        {
        }

        /// <summary>
        /// Stores a given nonce and timestamp.
        /// </summary>
        /// <param name="context">The context, or namespace, within which the
        /// <paramref name="nonce"/> must be unique.
        /// The context SHOULD be treated as case-sensitive.
        /// The value will never be <c>null</c> but may be the empty string.</param>
        /// <param name="nonce">A series of random characters.</param>
        /// <param name="timestampUtc">The UTC timestamp that together with the nonce string make it unique
        /// within the given <paramref name="context"/>.
        /// The timestamp may also be used by the data store to clear out old nonces.</param>
        /// <returns>
        /// True if the context+nonce+timestamp (combination) was not previously in the database.
        /// False if the nonce was stored previously with the same timestamp and context.
        /// </returns>
        /// <remarks>
        /// The nonce must be stored for no less than the maximum time window a message may
        /// be processed within before being discarded as an expired message.
        /// </remarks>
        public bool StoreNonce(string context, string nonce, DateTime timestampUtc)
        {
            var nonceToAdd = new OAuth_Nonce()
            {
                Context = context,
                Code = nonce,
                Timestamp = timestampUtc
            };
            try
            {
                DBContext.OAuth_Nonces.InsertOnSubmit(nonceToAdd);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}