using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Authorization.Api.Attributes;
using Authorization.Api.Models;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2.Messages;
using Onlife.OAuth.AuthorizationServer;
using StructureMap.Attributes;
using DNOA = DotNetOpenAuth.OAuth2;
using System.Configuration;
using Onlife.OAuth.AuthorizationServer.Utilities;
using System.ComponentModel.DataAnnotations;
using Onlife.OAuth.AuthorizationServer.Server;

namespace Authorization.Api.Controllers
{
    public class OAuthController : Controller
    {
        [SetterProperty]
        public DNOA.IAuthorizationServerHost _AuthorizationServerHost { get; set; }
        [SetterProperty]
        public IOAuthClientService _OAuthClientService { get; set; }

        [SetterProperty]
        public IOAuthResourceService _OAuthResourceService { get; set; }
        //[SetterProperty]
        //public ILoginManager _LoginManager { get; set; }
        private readonly DNOA.AuthorizationServer _authorizationServer = new DNOA.AuthorizationServer(new AuthorizationServerHost());        
        [SetterProperty]
        public IOAuthAuthorizationService _OAuthAuthorizationService { get; set; }

        public ActionResult Token()
        {
            //var authorizationServer = new DNOA.AuthorizationServer(_AuthorizationServerHost);
            return _authorizationServer.HandleTokenRequest(Request).AsActionResult();
        }

        public ActionResult RefreshToken()
        {
            //var authorizationServer = new DNOA.AuthorizationServer(_AuthorizationServerHost);
            return _authorizationServer.HandleTokenRequest(Request).AsActionResult();            
        }

        // Prompts the user to authorize a client to access the user's private data.
        // If user is not already authenticated by the resource, user will be redirected to login first and then
        // come back here to authorize the client
        [ResourceAuthenticated, AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Authorize(bool auto)
        {
            var authorizationServer = new DNOA.AuthorizationServer(_AuthorizationServerHost);

            // Have DotNetOpenAuth read the info we need out of the request
            EndUserAuthorizationRequest pendingRequest = authorizationServer.ReadAuthorizationRequest();
            if (pendingRequest == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Missing authorization request.");
            }

            // Make sure the client is one we recognize
            var requestingClient = _OAuthClientService.GetClient(pendingRequest.ClientIdentifier);
            if (requestingClient == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Invalid request");
            }

            // Ensure client is allowed to use the requested scopes
            if (!pendingRequest.Scope.IsSubsetOf(requestingClient.Scopes.Select(x => x.Identifier)))
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Invalid request");
            }

            if (!pendingRequest.Scope.Any())
            {
                pendingRequest.Scope.Add("basic");
            }

            var requestedResource = _OAuthResourceService.FindWithSupportedScopes(pendingRequest.Scope);

            //auto-approve, meaning user will not see page for approving scopes since it is an internal communication
            //this could be decided using a setting from the resources table
            if (auto)
            {
                ////get user to send back
                //var user = _LoginManager.GetUserByLogin(User.Identity.Name);

                //_OAuthAuthorizationService.CreateAuthorization(new CreateAuthorizationOptions
                //{
                //    ClientId = requestingClient.Client.OAuth_ClientId,
                //    Scope = DNOA.OAuthUtilities.JoinScopes(pendingRequest.Scope),
                //    UserId = user.UserId,
                //    ResourceId = requestedResource.Resource.OAuth_ResourceId
                //});

                // Have DotNetOpenAuth generate an approval to send back to the client
                IDirectedProtocolMessage authRequest = authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
                return authorizationServer.Channel.PrepareResponse(authRequest).AsActionResult();
            }       

            // Show user the authorization page by which they can authorize this client to access their
            // data within the resource determined by the requested scopes
            var model = new AccountAuthorizeModel
            {
                Client = requestingClient,
                Scopes = requestingClient.Scopes.Where(x => pendingRequest.Scope.Contains(x.Identifier)).ToList(),
                AuthorizationRequest = pendingRequest
            };

            return View(model);
        }

        /// Processes the user's response as to whether to authorize a Client to access his/her private data.
        [ResourceAuthenticated(Order = 1), HttpPost, ValidateAntiForgeryToken(Order = 2)]
        public ActionResult ProcessAuthorization(bool isApproved)
        {
            var authorizationServer = new DNOA.AuthorizationServer(_AuthorizationServerHost);

            // Have DotNetOpenAuth read the info we need out of the request
            EndUserAuthorizationRequest pendingRequest = authorizationServer.ReadAuthorizationRequest();
            if (pendingRequest == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Missing authorization request.");
            }

            // Make sure the client is one we recognize
            var requestingClient = _OAuthClientService.GetClient(pendingRequest.ClientIdentifier);
            if (requestingClient == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Invalid request");
            }

            // Make sure the resource is defined, it definitely should be due to the ResourceAuthenticated attribute
            var requestedResource = _OAuthResourceService.FindWithSupportedScopes(pendingRequest.Scope);
            if (requestedResource == null)
            {
                throw new HttpException(Convert.ToInt32(HttpStatusCode.BadRequest), "Invalid request");
            }

            // See if authorization of this client was approved by the user
            // At this point, the user either agrees to the entire scope requested by the client or none of it.  
            // If we gave capability for user to reduce scope to give client less access, some changes would be required here
            IDirectedProtocolMessage authRequest;
            if (isApproved)
            {
                // Add user to our repository if this is their first time
                //var user = _LoginManager.GetUserByLogin(User.Identity.Name);
                //if (requestingUser == null)
                //{
                //    requestingUser = new User { Id = User.Identity.Name, CreateDateUtc = DateTime.UtcNow };
                //    _userRepository.Insert(requestingUser);
                //    _userRepository.Save();
                //}

                // The authorization we file in our database lasts until the user explicitly revokes it.
                // You can cause the authorization to expire by setting the ExpirationDateUTC
                // property in the below created ClientAuthorization.
                _OAuthAuthorizationService.CreateAuthorization(new CreateAuthorizationOptions
                {
                    ClientId = requestingClient.Client.OAuth_ClientId,
                    Scope = DNOA.OAuthUtilities.JoinScopes(pendingRequest.Scope),
                    UserId = 0,
                    ResourceId = requestedResource.Resource.OAuth_ResourceId
                });

                // Have DotNetOpenAuth generate an approval to send back to the client
                authRequest = authorizationServer.PrepareApproveAuthorizationRequest(pendingRequest, User.Identity.Name);
            }
            else
            {
                // Have DotNetOpenAuth generate a rejection to send back to the client
                authRequest = authorizationServer.PrepareRejectAuthorizationRequest(pendingRequest);
                // The PrepareResponse call below is giving an error of "The following required parameters were missing from the DotNetOpenAuth.OAuth2.Messages.EndUserAuthorizationFailedResponse message: error"
                // unless I do this.....
                var msg = (EndUserAuthorizationFailedResponse)authRequest;
                msg.Error = "User denied your request";
            }

            // This will redirect to the client app using their defined callback, so they can handle
            // the approval or rejection as they see fit
            return authorizationServer.Channel.PrepareResponse(authRequest).AsActionResult();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.CreateUrl = string.Format("{0}/User/SignUp?returnUrl={1}", "",
                    Url.Encode(returnUrl));
            ViewBag.ForgotPasswordUrl = string.Format("{0}/User/ForgotPassword?returnUrl={1}", "",
                    Url.Encode(returnUrl));

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            try
            {
                //_LoginManager.ValidateLogin(model.Email, model.Password);

                // Corresponds to shared secret the authorization server knows about for this resource
                string encryptionKey = ConfigurationManager.AppSettings["OAuth_ResourceAuthenticationKey"];

                // Build token with info the authorization server needs to know
                var tokenContent = model.Email + ";" + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                var encryptedToken = EncodingUtility.Encode(tokenContent, encryptionKey);

                // Redirect back to the authorization server, including the authentication token 
                // Name of authentication token corresponds to that known by the authorization server
                returnUrl += (returnUrl.Contains("?") ? "&" : "?");
                returnUrl += "cb-resource-authentication-token=" + encryptedToken;
                var url = new Uri(returnUrl);
                var redirectUrl = url.ToString();

                // URL Encode the values of the querystring parameters
                if (url.Query.Length > 1)
                {
                    var helper = new System.Web.Mvc.UrlHelper(HttpContext.Request.RequestContext);
                    var qsParts = HttpUtility.ParseQueryString(url.Query);
                    redirectUrl = url.GetLeftPart(UriPartial.Path) + "?" + String.Join("&", qsParts.AllKeys.Select(x => x + "=" + helper.Encode(qsParts[x])));
                }

                return Redirect(redirectUrl);
            }
            catch (ValidationException ex)
            {
                //ex.AddToModelState(ViewData.ModelState);
            }

            return View(model);
        }
    }
}
