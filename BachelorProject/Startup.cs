using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BachelorProject.Models;
using BachelorProject.Utility;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BachelorProject
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("BachelorProjectDBConnection")));
            // registrování služby pro systém identit (nezbytné pro funkcionality spojené s uživateli a rolemi)
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            // nastavení požadavků na hesla
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<AppDbContext>();

            // registrování služby spojené s MVC architekturou
            services.AddMvc(options => {
                // nastavení autorizační politiky - všechny metody budou přístupny pouze přihlášeným uživatelům (nebudou-li označeny dodatečnými atributy)
                var authorizationPolicy = new AuthorizationPolicyBuilder()
                                                .RequireAuthenticatedUser()
                                                .Build();
                options.Filters.Add(new AuthorizeFilter(authorizationPolicy));
            });

            // zaregistrování rozhraní a tříd pro práci s databází
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
            services.AddScoped<IGameTypeRepository, SQLGameTypeRepository>();
            services.AddScoped<IAdditionalGameRepository, SQLAdditionalGameRepository>();
            //services.AddScoped<IVoucherTypeRepository, SQLVoucherTypeRepository>();
            services.AddScoped<IVoucherRepository, SQLVoucherRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // pokud je prostředí "Development" je v případě chyby zobrazována stránka s detailem chyby, jinak je zobrazena obecná errorová stránka
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            // umožnění práce se statickými soubory (složka "wwwroot")
            app.UseStaticFiles();
            // využití autentizace
            app.UseAuthentication();
            // nastavení routování
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=AdditionalGame}/{action=Index}/{id?}");
            });

            
        }
    }
}
