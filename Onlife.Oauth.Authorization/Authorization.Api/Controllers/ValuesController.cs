using Authorization.Api.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Authorization.Api.Controllers
{
    public class ValuesController : ApiController
    {
        //
        // GET: /Values/

        [OAuthUserAuthorizationScopeAttribute]
        public string[] GetNumbers()
        {
            string[] sampStrv = new string[] {"1","100","1000","9999"};
            return sampStrv;
        }

        [OAuthUserAuthorizationScopeAttribute]
        public string[] GetFruits()
        {
            string[] sampStrv = new string[] { "Orange", "IApple", "Blackberry", "Banana" };
            return sampStrv;
        }

        public string[] GetAnimals()
        {
            string[] sampStrv = new string[] { "Giraffe", "Lion", "Horse", "Deer" , "More Animals ..."};
            return sampStrv;
        }

    }
}
