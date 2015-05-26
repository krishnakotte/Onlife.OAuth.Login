using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Authorization.TestClient.Config;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;

namespace Authorization.TestClient.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index(bool? refresh)
        {
            var auth = (AuthorizationState) Session["Authorization"];

            if (refresh.GetValueOrDefault())
            {
                if (ClientConfig.AuthorizationServerClient.RefreshAuthorization(auth))
                {
                    Session["Authorization"] = auth;
                }
            }
            
            ViewBag.AuthExpires = auth.AccessTokenExpirationUtc.GetValueOrDefault().ToLongTimeString();

            var baseUrl = ConfigurationManager.AppSettings["ApiUrl"];
            var req = WebRequest.Create(baseUrl + (baseUrl.EndsWith("/") ? "" : "/") + "Me/Newsfeed");
            req.Headers.Add("Authorization", "Bearer " + auth.AccessToken);
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

            return View((object)content);
        }

        [AllowAnonymous]
        public ActionResult Welcome()
        {
            return View();
        }

        public ActionResult Logout()
        {
            Session.RemoveAll();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            var state = new AuthorizationState(new HashSet<string>() { "manage" });
            state.Callback = new Uri(Request.Url, Url.Action("LoginApi", "Home"));

            // Here DotNetOpenAuth figures out what the request should look like (i.e., builds appropriate url)
            var r = ClientConfig.AuthorizationServerClient.PrepareRequestUserAuthorization(state);
            return r.AsActionResult();   
        }

        [AllowAnonymous]
        public ActionResult RegisterDevice()
        {
            string msg = null;
            bool gotToken = false;
            var scopes = new HashSet<string>() {"manage"};

            if (Session["ClientAuthorization"] == null)
            {
                try
                {
                    // This seems to throw ProtocolException if client is unauthorized rather than just returning no token
                    var state = ClientConfig.AuthorizationServerClient.GetClientAccessToken(scopes);
                    gotToken = state.AccessToken != null;

                    if (gotToken)
                    {
                        Session["ClientAuthorization"] = state;
                        //var cookie = new HttpCookie(ClientConfig.AccessTokenName, state.AccessToken) { Expires = state.AccessTokenExpirationUtc.Value, Path = "/" };
                        //Response.Cookies.Add(cookie);
                    }
                }
                catch (ProtocolException ex)
                {
                    msg = ", Message was: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                }
            }

            ViewBag.Msg = msg;

            if (string.IsNullOrEmpty(msg))
            {
                var auth = (AuthorizationState)Session["ClientAuthorization"];

                ViewBag.Msg = auth.AccessTokenExpirationUtc.GetValueOrDefault().ToString();

                var baseUrl = ConfigurationManager.AppSettings["ApiUrl"];
                var req = WebRequest.Create(baseUrl + (baseUrl.EndsWith("/") ? "" : "/") + "Device/Register");
                req.Headers.Add("Authorization", "Bearer " + auth.AccessToken);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                var parameters = HttpUtility.ParseQueryString(String.Empty);
                parameters.Add("uuid", "TESTCLIENT-1");
                parameters.Add("appType", "TestClient-1");
                parameters.Add("appVersion", "0.0.1");

                var postString = parameters.ToString();
                var byteArray = Encoding.UTF8.GetBytes(postString);
                req.ContentLength = byteArray.Length;
                using (var dataStream = req.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

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

                return View((object)content);
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult LoginApi()
        {
            var accessTokenResponseState = ClientConfig.AuthorizationServerClient.ExchangeUserCredentialForToken("krishna", "kotte", new string[] { "manage" });
            var gotToken = accessTokenResponseState.AccessToken != null;

            if (gotToken)
            {
                Session["Authorization"] = accessTokenResponseState;
                //var cookie = new HttpCookie(ClientConfig.AccessTokenName, accessTokenResponseState.AccessToken) { Expires = accessTokenResponseState.AccessTokenExpirationUtc.Value, Path = "/" };
                //Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
