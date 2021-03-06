using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soran1957Try20210328
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(100);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection(); 
            app.UseRouting();
            app.UseStaticFiles();
            app.UseSession();

            DataSource.Connect("./wwwroot/");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    string home_id = "w20070417_7_1744";
                    string id = home_id;

                    //await context.Response.WriteAsync("Hello World!");
                    var pars =  context.Request.Query;
                    var session = context.Session;

                    //string val = session.GetString("my");
                    //session.SetString("my", "hel--lo! " + DateTime.Now.ToShortTimeString());
                    
                    var construct = new Construct(pars, session);
                    var html = construct.ConstructPage();
                    await context.Response.WriteAsync("<!DOCTYPE html>\n" + html.ToString());
                });
            });
        }
    }
}
