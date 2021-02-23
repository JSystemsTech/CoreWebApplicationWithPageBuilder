using AuthNAuthZ.AuthenticationHandlers;
using AuthNAuthZ.Extensions;
using AuthNAuthZ.TokenProvider;
using DomainLayer.Connection;
using DomainLayer.CoreConfiguration;
using DomainLayer.DomainFacade;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedServices.ApiClient;
using System;
using System.Data.Common;

namespace CoreWebApplicationWithPageBuilder
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddControllersWithViews();
            services.AddSingleton(Configuration);
            services.ConfigureDbConnection("CoreWebApplicationDb", (cs, pn)=> new WebAppDbConnection(cs,pn));
            services.AddSingleton<IWebAppDomainFacade, WebAppDomainFacade>();
            services.ConfigureTokenProvider((settings, encryptionProvider) => new Saml2TokenHandler(settings, encryptionProvider));


            services.AddHttpClient<IAuthenticationProviderApi, AuthenticationProviderApi>(client =>
            {
                client.BaseAddress = new Uri("https://localhost/api/Token/");
            });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme
                    = AuthenticationSchemeConstants.SAML2TokenAuthentication;
            })
            .AddScheme<TokenAuthenticationHandlerSchemeOptions, TokenAuthenticationHandler>
                    (AuthenticationSchemeConstants.SAML2TokenAuthentication, op => { });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebAppDbConnection webAppDbConnection)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
            });
        }
    }
}
