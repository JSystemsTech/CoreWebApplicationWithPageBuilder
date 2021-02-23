using AuthNAuthZ.AuthenticationHandlers;
using CoreWebApplicationWithPageBuilder.Models;
using DomainLayer.DomainFacade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWebApplicationWithPageBuilder.Controllers
{

    [Authorize(AuthenticationSchemes = AuthenticationSchemeConstants.SAML2TokenAuthentication)]
    public abstract class BaseController : Controller
    {
        protected IServiceProvider Services { get; private set; }
        protected TokenAuthenticationHandler TokenAuthenticationHandler => (TokenAuthenticationHandler)Services.GetService(typeof(TokenAuthenticationHandler));
        protected IWebAppDomainFacade DomainFacade => (IWebAppDomainFacade)Services.GetService(typeof(IWebAppDomainFacade));
        public BaseController(IServiceProvider services)
        {
            Services = services;
        }
        protected void LogoutUser() => TokenAuthenticationHandler.Logout(HttpContext);
    }
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        
        public HomeController(ILogger<HomeController> logger, IServiceProvider services):base(services)
        {
            _logger = logger;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(new IndexViewModel { Users = DomainFacade.GetIdentities()});
        }
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TestUserLogin(Guid UserGuid)
        {
            TokenAuthenticationHandler.AuthenticateTestUser(UserGuid, HttpContext);

            return View("Login");
        }
        [AllowAnonymous]
        public IActionResult Logout()
        {
            return View();
        }
        public IActionResult AuthenticatedPage()
        {
            return View();
        }
    }
}
