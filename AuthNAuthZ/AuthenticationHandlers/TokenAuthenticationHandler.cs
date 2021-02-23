using AuthNAuthZ.TokenProvider;
using DomainLayer.DomainFacade;
using DomainLayer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedServices.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AuthNAuthZ.AuthenticationHandlers
{
    public class AuthenticationSchemeConstants
    {
        public const string SAML2TokenAuthentication = "SAML2TokenAuthentication";
    }
    public class TokenAuthenticationHandlerSchemeOptions
        : AuthenticationSchemeOptions
    {
        public string CookieName { get; set; }
        public string LogoutUrl { get; set; }
        public bool? EnableTestUserLogin { get; set; }
    }

    public class TokenAuthenticationHandler
        : AuthenticationHandler<TokenAuthenticationHandlerSchemeOptions>
    {
        private IAuthenticationProviderApi AuthenticationProviderApi { get; set; }
        private IWebAppDomainFacade DomainFacade { get; set; }
        private ITokenHandler TokenProvider { get; set; }
        private string CookieName { get; set; }
        private string LogoutUrl { get; set; }
        private bool EnableTestUserLogin { get; set; }
        public TokenAuthenticationHandler(
            IOptionsMonitor<TokenAuthenticationHandlerSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAuthenticationProviderApi authenticationProviderApi,
            IWebAppDomainFacade domainFacade,
            ITokenHandler tokenProvider,
            IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            AuthenticationProviderApi = authenticationProviderApi;
            DomainFacade = domainFacade;
            TokenProvider = tokenProvider;
            var tokenAuthSettings = configuration.GetSection("TokenAuthSettings");
            CookieName = Options != null && !string.IsNullOrWhiteSpace(Options.CookieName) ? Options.CookieName :  tokenAuthSettings.GetValue<string>("CookieName");
            LogoutUrl = Options != null &&  !string.IsNullOrWhiteSpace(Options.LogoutUrl) ? Options.LogoutUrl : tokenAuthSettings.GetValue<string>("LogoutUrl");
            EnableTestUserLogin = Options != null &&  Options.EnableTestUserLogin is bool enableTestUserLogin ? enableTestUserLogin : tokenAuthSettings.GetValue<bool>("EnableTestUserLogin");
        }
        private string TokenCookieValue => Request.Cookies.TryGetValue(CookieName, out string value) ? value : null;
        private bool IsValidToken => AuthenticationProviderApi.IsValid(TokenCookieValue);
        private IEnumerable<TokenClaim> TokenClaims => IsValidToken? AuthenticationProviderApi.GetClaims(TokenCookieValue) : null;
        private string UserIdentifier => TokenClaims != null && TokenClaims.FirstOrDefault(c => c.Name == "UserIdentifier") is TokenClaim tc ? tc.Values.FirstOrDefault(): null;
        
        public void AuthenticateTestUser(Guid userGuid, HttpContext context)
        {
            
            if (DomainFacade.GetIdentity(userGuid) is IdentityData model)
            {
                SetAuthTokenCookie(model, context);
            }
        }
        public void Logout(HttpContext context = null)
        {
            ExpireAuthTokenCookie(context);
            Context.Response.Redirect(LogoutUrl);// redirect to your error page
        }
        private void SetAuthTokenCookie(IdentityData model, HttpContext context = null)
        {
            string token = TokenProvider.Create(new TokenClaim[] { new TokenClaim { Name = "UserIdentifier", Value = model.Guid.ToString() } });
            var response = context != null ? context.Response : Response;
            var request = context != null ? context.Request : Request;
            response.Cookies.Append(
            CookieName,
            token,
            new CookieOptions()
            {
                Expires= TokenProvider.GetExpirationDate(token),
                Secure= request.IsHttps,
                HttpOnly= true,
                SameSite = SameSiteMode.Lax
            }
            ); 
        }
        private void ExpireAuthTokenCookie(HttpContext context = null)
        {
            var response = context != null ? context.Response : Response;
            var request = context != null ? context.Request : Request;
            response.Cookies.Append(
            CookieName,
            "",
            new CookieOptions()
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                Secure = request.IsHttps,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            }
            );
        }
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Logout();
            return Task.CompletedTask;
        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (IsValidToken) 
            {
                if(Guid.TryParse(UserIdentifier, out Guid userGuid) && DomainFacade.GetIdentity(userGuid) is IdentityData model)
                {
                    // success case AuthenticationTicket generation
                    // happens from here

                    // create claims array from the model
                    var claims = new[] {
                    new Claim(ClaimTypes.Name, model.Name),
                    new Claim(ClaimTypes.Role, string.Join(',', model.Roles))
                    };

                    // generate claimsIdentity on the name of the class
                    var claimsIdentity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));

                    // generate AuthenticationTicket from the Identity
                    // and current authentication scheme
                    var ticket = new AuthenticationTicket(
                        new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);

                    // pass on the ticket to the middleware
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                ExpireAuthTokenCookie();
                return Task.FromResult(AuthenticateResult.Fail("No valid User found"));                
            } 
            else
            {
                ExpireAuthTokenCookie();
                return Task.FromResult(AuthenticateResult.Fail("Header Not Found."));
            }
            
        }
    
    }
}
