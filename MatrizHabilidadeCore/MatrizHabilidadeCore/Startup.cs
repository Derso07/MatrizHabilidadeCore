using MatrizHabilidadeCore.Services;
using MatrizHabilidadeDatabase.Models;
using MatrizHabilidadeDataBaseCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MatrizHabilidadeCore
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
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.TryAddTransient<CookieService>();

            string connectionString = "";

#if (DEBUG)
            connectionString = Configuration.GetConnectionString("DebugConnection");
#else
            connectionString = Configuration.GetConnectionString("ReleaseConnection");
#endif

            services.AddDbContext<DataBaseContext>(options =>
            {
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
            }, ServiceLifetime.Transient);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.Cookie.Name = "SuporteAnnimar";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/Usuario/Login";
                options.AccessDeniedPath = "/Error/AccessDenied";

                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(NivelAcesso.Administrador.ToString("g"), policy => policy.RequireClaim(ClaimTypes.Role, NivelAcesso.Administrador.ToString("g")));
                options.AddPolicy(NivelAcesso.Coordenador.ToString("g"), policy => policy.RequireClaim(ClaimTypes.Role, NivelAcesso.Coordenador.ToString("g")));
                options.AddPolicy(NivelAcesso.Funcionario.ToString("g"), policy => policy.RequireClaim(ClaimTypes.Role, NivelAcesso.Funcionario.ToString("g")));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseExceptionHandler("/Error");

            app.UseStatusCodePages(context =>
            {
                var request = context.HttpContext.Request;
                var response = context.HttpContext.Response;

                response.Redirect($"/Error/Status?origin={request.Path}&errorCode={response.StatusCode}");

                return Task.CompletedTask;
            });

            app.UseHsts();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
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
