using System;
using System.Net;

namespace Authorization.Api.Models
{
    public class ApiResponse
    {
        public ApiResponseMeta meta { get; set; }
        public object data { get; set; }
    }

    public class ApiResponseMeta
    {
        public DateTime timestamp { get; set; }
        public HttpStatusCode status_code { get; set; }
        public Guid request_id { get; set; }
        public bool success { get; set; }
    }

    public class ApiResponseError
    {
        public ApiResponseMetaError meta { get; set; }
    }

    public class ApiResponseMetaError
    {
        public DateTime timestamp { get; set; }
        public int status_code { get; set; }
        public long log_id { get; set; }
        public string error_code { get; set; }
        public string error_message { get; set; }
        public Guid request_id { get; set; }
        public bool success { get; set; }
        public string description { get; set; }
    }

    public class ApiRequestInfo
    {
        public Guid RequestId { get; set; }
        public int? PersonId { get; set; }
        public string OAuthClientIdentifier { get; set; }
    }
}