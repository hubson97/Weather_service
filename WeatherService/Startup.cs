using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using WeatherService.Data;
using WeatherService.Hubs;
using WeatherService.Quartz;
using Quartz.Spi;

namespace WeatherService
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

            services.AddHostedService<QuartzHostedService>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("AzureDbConnection"))); //AzureDbConnection    LocalSqlServerConnection
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddRazorPages();

            services.AddSingleton<IJobFactory, QuartzJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<DownloadWeatherInfoJob>();
            services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(DownloadWeatherInfoJob), "DownloadWeatherInfoJob", "* 0/7 * * * ?")); //* 0/7 * * * ? - co 7 minut; //* * 12/6 * * ? - co 6 godzin sstartuj¹c o 12 w poludnie


            services.AddAuthentication()
                .AddGoogle(opt =>
                {
                    opt.ClientId = "827170507763-8b7lcal3h8ak2s7a5lf3m9a46s4hgoe4.apps.googleusercontent.com";
                    opt.ClientSecret = "p2ZKEux0MZBheOaDETYW20RS";
                });
                //.AddMicrosoftAccount(opt =>
                //{
                //    opt.ClientId = "";
                //    opt.ClientSecret = ""; 
                //});

            services.AddSignalR()
                    .AddJsonProtocol(opt=>
                    {
                        //opt.PayloadSerializerOptions.PropertyNamingPolicy = null;
                    });




        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<WeatherHub>("/weatherHub");
            });
        }
    }
}
