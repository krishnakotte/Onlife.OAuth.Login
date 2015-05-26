using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Routing;
using Authorization.Api.Helpers;

namespace Authorization.Api.Attributes
{
    public class FormHandler : DelegatingHandler
    {
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (request.Content != null && request.Content.Headers.ContentType != null &&
                request.Content.Headers.ContentType.MediaType == "application/x-www-form-urlencoded")
            {
                var routeData = request.Properties["MS_HttpRouteData"] as IHttpRouteData;

                var form = request.Content.ReadAsFormDataAsync().Result;
                foreach (var key in form.AllKeys)
                {
                    routeData.Values.Add(new KeyValuePair<string, object>(key, form[key]));
                }
            }
            else if (request.Content.IsMimeMultipartContent("form-data"))
            {
                var provider = new InMemoryMultipartFormDataStreamProvider();

                try
                {
                    var routeData = request.Properties["MS_HttpRouteData"] as IHttpRouteData;
                    NameValueCollection form = null;
                    List<HttpContent> files = null;
                    Task.Factory
                        .StartNew(() =>
                        {
                            var result = request.Content.ReadAsMultipartAsync(provider).Result;
                            form = result.FormData;
                            files = result.Files;
                        },
                            CancellationToken.None,
                            TaskCreationOptions.LongRunning, // guarantees separate thread
                            TaskScheduler.Default)
                        .Wait();

                        foreach (var key in form.AllKeys)
                        {
                            routeData.Values.Add(new KeyValuePair<string, object>(key, form[key]));
                        }
                }
                catch (Exception ex)
                {
                    //skip exceptions
                }
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}