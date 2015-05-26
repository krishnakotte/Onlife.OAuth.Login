using System;
using System.Net.Http;
using System.Web.Http;
using Authorization.Api.Attributes;
using Newtonsoft.Json.Linq;
using StructureMap.Attributes;
using System.Web;

namespace Authorization.Api.Helpers
{
    public class BaseApiController : ApiController
    {
        [SetterProperty]
        public HttpContext _HttpContext { get; set; }
        
        //private ILogger _logger;
        //public ILogger Logger
        //{
        //    get { return _logger ?? (_logger = new Logger()); }
        //    set { _logger = value; }
        //}

        protected T GetPostParam<T>(string key)
        {
            var p = Request.Content.ReadAsAsync<JObject>();
            return (T)Convert.ChangeType(p.Result[key], typeof(T)); // example conversion, could be null...
        }
    }
}
