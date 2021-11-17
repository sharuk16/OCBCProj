using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace PFD_Challenge_1
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
            // Add a default in-memory implementation of distributed cache
            services.AddDistributedMemoryCache();
            // Add the session service
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews();
            services.AddAuthentication(configureOptions:options =>
               {
                   options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                   options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                   options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, configureOptions:options =>
                {
                    options.ClientId = "7fmcr9u16mjtpbt17gam0f6sm";
                    options.ClientSecret = "12dt1cg3l8hco6jmd699c03u0a22jdqp41s31d6ptetinrnskcoq";
                    options.Authority = "https://cognito-idp.ap-southeast-1.amazonaws.com/ap-southeast-1_HwJpsrg0r";
                    options.Scope.Add(item: "openid");
                    options.Scope.Add(item: "profile");
                    options.Scope.Add(item: "email");
                    options.ResponseType = "code";
                    //options.Scope.Add(item: "")
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        NameClaimType = "cognito:user"
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
