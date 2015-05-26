using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Authorization.Api.DependencyResolution;
using Authorization.Api.Helpers;
using Authorization.Api.Models;
using Onlife.OAuth.AuthorizationServer.Server;
using Newtonsoft.Json;
using StructureMap.Attributes;

namespace Authorization.Api.Attributes
{
    public class MetadataHandler : DelegatingHandler
    {
        private HttpContextWrapper _context;
        //private IApiLogger _apiLogger;


        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //var dependancy = request.Properties["MS_HttpContext"];
            //_context = new WebContext();
            //_context.UnitOfWork = new UnitOfWork();
            //_apiLogger = new ApiLogger(_context);

            return base.SendAsync(request, cancellationToken).ContinueWith(
             task =>
             {
                 _context = request.Properties["MS_HttpContext"] as HttpContextWrapper;

                 if (request.Properties.ContainsKey("MS_DependencyScope"))
                 {
                     var dependancy = request.Properties["MS_DependencyScope"] as StructureMapDependencyResolver;
                     //_apiLogger = dependancy.GetInstance<IApiLogger>();
                 }
                 else
                 {
                   
                     //_apiLogger = new ApiLogger(context);
                 }

                 object content;
                 var contentRecieved = task.Result.TryGetContentValue(out content);

                 HttpResponseMessage responseMessage;

                 if (content != null && (content is ApiResponse || content is ApiResponseError))
                     return task.Result;

                 if (!task.Result.IsSuccessStatusCode)
                 {
                     responseMessage = GetErrorResponse(content as HttpError, request, task.Result);
                 }
                 else
                 {
                     responseMessage = GetSuccessResponse(content, request, task.Result);
                 }

                 return responseMessage;
             }
             );
        }

        private HttpResponseMessage GetErrorResponse(HttpError error, HttpRequestMessage request, HttpResponseMessage response)
        {
            
            var requestInfo = ApiRequestHelper.GetRequestInfo(request);
            ApiResponseError responseWrapper = null;

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                responseWrapper = new ApiResponseError
                {
                    meta = new ApiResponseMetaError
                    {
                        success = false,
                        description = "The URI requested is invalid.",
                        error_code = "not-found",
                        error_message = "",
                        log_id = 0,
                        request_id = requestInfo.RequestId,
                        status_code = (int)response.StatusCode,
                        timestamp = DateTime.UtcNow
                    }
                };
            } 
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                responseWrapper = new ApiResponseError
                {
                    meta = new ApiResponseMetaError
                    {
                        success = false,
                        description = "Authentication credentials were missing or incorrect.",
                        error_code = "unauthorized",
                        error_message = "",
                        log_id = 0,
                        request_id = requestInfo.RequestId,
                        status_code = (int)response.StatusCode,
                        timestamp = DateTime.UtcNow
                    }
                };
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                responseWrapper = new ApiResponseError
                {
                    meta = new ApiResponseMetaError
                    {
                        success = false,
                        description = "The request was invalid or cannot be otherwise served.",
                        error_code = "bad-request",
                        error_message = "",
                        log_id = 0,
                        request_id = requestInfo.RequestId,
                        status_code = (int)response.StatusCode,
                        timestamp = DateTime.UtcNow
                    }
                };
            }

            if (responseWrapper == null)
            {
                var errorMessage = "";

                if (error == null)
                {
                    //log = logger.LogError("An unexpected server error has occurred.", Logger.AvailableTags.TypeUnhandled,
                    //    true, new {requestId = requestInfo.RequestId, statusCode = response.StatusCode});
                }
                else
                {
                    //log = logger.LogError(error.Message, Logger.AvailableTags.TypeUnhandled,
                    //    true, new { requestId = requestInfo.RequestId, statusCode = response.StatusCode });

                    if (_context != null && _context.IsDebuggingEnabled)
                        errorMessage = error.Message + "---" + error["StackTrace"];
                }

                //responseWrapper = new ApiResponseError
                //{
                //    meta = new ApiResponseMetaError
                //    {
                //        success = false,
                //        description = "An unexpected server error has occurred. Please contact support if error continues.",
                //        error_code = "unknown-error",
                //        error_message = errorMessage,
                //        log_id = log.GetLogId(),
                //        request_id = requestInfo.RequestId,
                //        status_code = (int) response.StatusCode,
                //        timestamp = DateTime.UtcNow
                //    }
                //};
            }

            try
            {
                //_apiLogger.Log(new DbTable.ApiAudit()
                //{
                //    IPAddress = _context != null ? _context.Request.UserHostAddress : "0.0.0.0",
                //    OAuthClientIdentifier = requestInfo.OAuthClientIdentifier,
                //    PersonId = requestInfo.PersonId,
                //    Request = request.ToString(),
                //    RequestMethod = request.Method.Method,
                //    RequestUri = request.RequestUri.AbsoluteUri,
                //    Response = response.ToString(),
                //    ResponseCode = (int) response.StatusCode,
                //    RequestId = requestInfo.RequestId.ToString(),
                //    ResponseErrorCode = responseWrapper.meta.error_code,
                //    ResponseErrorLogId = responseWrapper.meta.log_id.ToString(),
                //    ResponseSuccess = responseWrapper.meta.success
                //});
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, Logger.AvailableTags.TypeUnhandled,
                //           true, new { request = request.ToString(), response = response.ToString() });
            }

            return request.CreateResponse(response.StatusCode, responseWrapper);
        }

        private HttpResponseMessage GetSuccessResponse(object content, HttpRequestMessage request, HttpResponseMessage response)
        {
            var requestInfo = ApiRequestHelper.GetRequestInfo(request);
            var responseWrapper = new ApiResponse()
            {
                data = content,
                meta = new ApiResponseMeta()
                {
                    success = true,
                    request_id = requestInfo.RequestId,
                    status_code = response.StatusCode,
                    timestamp = DateTime.UtcNow
                }
            };

            try
            {
                //_apiLogger.Log(new DbTable.ApiAudit()
                //{
                //    IPAddress = _context != null ? _context.Request.UserHostAddress : "0.0.0.0",
                //    OAuthClientIdentifier = requestInfo.OAuthClientIdentifier,
                //    PersonId = requestInfo.PersonId,
                //    Request = request.ToString(),
                //    RequestMethod = request.Method.Method,
                //    RequestUri = request.RequestUri.AbsoluteUri,
                //    Response = response.ToString(),
                //    ResponseCode = (int)response.StatusCode,
                //    RequestId = requestInfo.RequestId.ToString(),
                //    ResponseSuccess = responseWrapper.meta.success
                //});
            }
            catch (Exception ex)
            {
                //var logger = new Logger();
            
                //logger.LogError(ex, Logger.AvailableTags.TypeUnhandled,
                //           true, new { request = request.ToString(), response = response.ToString() });
            }

            return request.CreateResponse(response.StatusCode, responseWrapper);
        }

        
    }
}