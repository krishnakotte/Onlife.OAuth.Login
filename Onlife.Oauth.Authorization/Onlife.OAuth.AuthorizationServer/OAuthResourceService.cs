using System;
using System.Collections.Generic;
using System.Linq;
using Onlife.OAuth.AuthorizationServer.Models;
using StructureMap.Attributes;
using Onlife.OAuth.AuthorizationServer.Server.Base;

namespace Onlife.OAuth.AuthorizationServer
{
    public interface IOAuthResourceService : IBaseService
    {
        OAuth_ResourceModel FindWithSupportedScopes(HashSet<string> scopes);
    }

    public class OAuthResourceService : BaseService, IOAuthResourceService
    {
        public OAuthResourceService() : base()
        {
        }

        public OAuth_ResourceModel FindWithSupportedScopes(HashSet<string> scopes)
        {
            //return new OAuth_Resource(DBContext.OAuth_Resources.FindWithSupportedScopes(scopes));
            OAuth_Resource obj = DBContext.OAuth_Resources.FirstOrDefault();
            return new OAuth_ResourceModel(obj);            
        }
    }
}