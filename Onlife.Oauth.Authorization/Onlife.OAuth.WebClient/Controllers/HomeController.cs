using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Onlife.OAuth.WebClient.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Resources()
        {
            return View();
        }

        public static string ClientId { get { return ConfigurationManager.AppSettings["ClientId"]; } }
        public static string ClientSecret { get { return ConfigurationManager.AppSettings["ClientSecret"]; } }
        public static string ClientCallbackUrl { get { return ConfigurationManager.AppSettings["ClientCallbackUrl"]; } }
        public static string AccessTokenName { get { return ConfigurationManager.AppSettings["AccessTokenName"]; } }

        public dynamic GetAccessToken(string username, string password, string scopes, string clientid, string clientsecret)
        {
            if (Request.Cookies["bearer"] != null)
            {
                HttpCookie myCookie = new HttpCookie("bearer");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }

            if (Request.Cookies["bearer_refresh"] != null)
            {
                HttpCookie myCookie = new HttpCookie("bearer_refresh");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
            string grant_type = "password";
            var sb = new StringBuilder();
            sb.Append("scope=").Append(HttpUtility.UrlEncode(scopes));
            sb.Append("&grant_type=").Append(HttpUtility.UrlEncode(grant_type));
            sb.Append("&username=").Append(HttpUtility.UrlEncode(username));
            sb.Append("&password=").Append(HttpUtility.UrlEncode(password));
            sb.Append("&client_id=").Append(HttpUtility.UrlEncode(clientid));
            sb.Append("&client_secret=").Append(HttpUtility.UrlEncode(clientsecret));

            var dataToPost = sb.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(dataToPost);

            Uri uri = new Uri(ConfigurationManager.AppSettings["AuthorizationServerTokenUrl"]);
            var client = WebRequest.Create(uri);
            client.Method = "POST";
            client.ContentType = "application/x-www-form-urlencoded";
            client.ContentLength = bytes.Length;
            var bodyStream = client.GetRequestStream();
            bodyStream.Write(bytes, 0, bytes.Length);
            bodyStream.Close();

            using (WebResponse response = client.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var data = reader.ReadToEnd();
                    var dummyObject = new { access_token = "", expires_in = 0d, refresh_token = "", token_type = "" };
                    var accessTokenData = JsonConvert.DeserializeAnonymousType(data, dummyObject);
                    return accessTokenData;
                }
            }
        }

        private string getAccessTokenFromCookies()
        {
            if (Request.Cookies["bearer"] != null)
                return Request.Cookies["bearer"].Value;
            else
                return "";
        }

        public string ExecuteAPICall(string apiURL)
        {
            string accessToken = getAccessTokenFromCookies();

            //if (string.IsNullOrEmpty(accessToken))
            //    throw new Exception("Access Token invalid or empty");
            var baseUrl = apiURL;
            var req = WebRequest.Create(baseUrl);
            req.Headers.Add("Authorization", "Bearer " + accessToken);
            req.Method = "GET";
            req.ContentLength = 0;

            string content;
            try
            {
                var resp = req.GetResponse();
                content = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            }
            catch (WebException ex)
                {
                content = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                //content = String.Format("An error occurred, message was {0}", ex.Message);
            }

            return content;
        }

        public dynamic GetRefreshToken(string refreshtoken)
        {
            if (Request.Cookies["bearer"] != null)
            {
                HttpCookie myCookie = new HttpCookie("bearer");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }

            if (Request.Cookies["bearer_refresh"] != null)
            {
                HttpCookie myCookie = new HttpCookie("bearer_refresh");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
            string grant_type = "refresh_token";
            var sb = new StringBuilder();
            //sb.Append("scope=").Append(HttpUtility.UrlEncode(scopes));
            sb.Append("grant_type=").Append(HttpUtility.UrlEncode(grant_type));
            sb.Append("&refresh_token=").Append(HttpUtility.UrlEncode(refreshtoken));
            //sb.Append("&password=").Append(HttpUtility.UrlEncode(password));
            sb.Append("&client_id=").Append(HttpUtility.UrlEncode(ConfigurationManager.AppSettings["ClientId"]));
            sb.Append("&client_secret=").Append(HttpUtility.UrlEncode(ConfigurationManager.AppSettings["ClientSecret"]));

            var dataToPost = sb.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(dataToPost);

            Uri uri = new Uri(ConfigurationManager.AppSettings["AuthorizationServerRefreshTokenUrl"]);
            var client = WebRequest.Create(uri);
            client.Method = "POST";
            client.ContentType = "application/x-www-form-urlencoded";
            client.ContentLength = bytes.Length;
            var bodyStream = client.GetRequestStream();
            bodyStream.Write(bytes, 0, bytes.Length);
            bodyStream.Close();

            using (WebResponse response = client.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var data = reader.ReadToEnd();
                    var dummyObject = new { access_token = "", expires_in = 0d, refresh_token = "", token_type = "" };
                    var accessTokenData = JsonConvert.DeserializeAnonymousType(data, dummyObject);
                    return accessTokenData;
                }
            }
        }

        public ActionResult RefreshToken(string refreshtoken)
        {
            string msg = null;
            bool gotToken = false;
            try
            {
                //// This seems to throw ProtocolException if client is unauthorized rather than just returning no token
                //var state = ClientConfig.AuthorizationServerClient.ExchangeUserCredentialForToken(username, password, scopes);
                ////var tokens = ClientConfig.QueryAccessToken();
                //gotToken = state.AccessToken != null;

                //if (gotToken)
                //{
                //    var cookie = new HttpCookie(ClientConfig.AccessTokenName, state.AccessToken) { Expires = state.AccessTokenExpirationUtc.Value, Path = "/" };
                //    Response.Cookies.Add(cookie);
                //}
                dynamic accessToken = GetRefreshToken(refreshtoken);
                gotToken = accessToken.access_token != null;

                if (gotToken)
                {
                    DateTime expDt = new DateTime();
                    expDt.AddSeconds(accessToken.expires_in);
                    var cookie = new HttpCookie(accessToken.token_type, accessToken.access_token) { Expires = expDt, Path = "/" };
                    Response.Cookies.Add(cookie);
                    var refreshCookie = new HttpCookie(accessToken.token_type + "_refresh", accessToken.refresh_token) { Expires = expDt, Path = "/" };
                    Response.Cookies.Add(refreshCookie);
                }
                return RedirectToAction("Resources", "Home", new { msg = gotToken ? "Token Granted" : "Access Token Was Granted" + msg });

            }
            catch (Exception ex)
            {
                return RedirectToAction("Resources", "Home", new { msg = gotToken ? "Token Granted" : "No Access Token Was Granted" + msg });
            }

        }

        public ActionResult StartFlow(string username, string password, string scopes, string clientid, string clientsecret)
        {
            string msg = null;
            bool gotToken = false;
            try
            {
                //// This seems to throw ProtocolException if client is unauthorized rather than just returning no token
                //var state = ClientConfig.AuthorizationServerClient.ExchangeUserCredentialForToken(username, password, scopes);
                ////var tokens = ClientConfig.QueryAccessToken();
                //gotToken = state.AccessToken != null;

                //if (gotToken)
                //{
                //    var cookie = new HttpCookie(ClientConfig.AccessTokenName, state.AccessToken) { Expires = state.AccessTokenExpirationUtc.Value, Path = "/" };
                //    Response.Cookies.Add(cookie);
                //}
                dynamic accessToken = GetAccessToken(username, password, scopes, clientid, clientsecret);
                gotToken = accessToken.access_token != null;

                if (gotToken)
                {
                    DateTime expDt = new DateTime();
                    expDt.AddSeconds(accessToken.expires_in);
                    var cookie = new HttpCookie(accessToken.token_type, accessToken.access_token) { Expires = expDt, Path = "/" };
                    Response.Cookies.Add(cookie);
                    var refreshCookie = new HttpCookie(accessToken.token_type + "_refresh", accessToken.refresh_token) { Expires = expDt, Path = "/" };
                    Response.Cookies.Add(refreshCookie);
                }
                return RedirectToAction("Index", "Home", new { msg = gotToken ? "Token Granted" : "Access Token Was Granted" + msg });

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home", new { msg = gotToken ? "Token Granted" : "No Access Token Was Granted" + msg });
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}