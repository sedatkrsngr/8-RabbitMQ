using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQWeb.Watermark.BackgroundServices;
using RabbitMQWeb.Watermark.Models;
using RabbitMQWeb.Watermark.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.Watermark
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(s=>new ConnectionFactory() { //Rabbitmq sunucusuna program ayaða kalkarken bir kez baðlansak yeter
            Uri= new Uri(Configuration.GetConnectionString("ConStrRabbitMQ")),
            DispatchConsumersAsync=true  //asenkron olarak consumerda method kullanýyorsak bunu true yapmalýyýz.Background serviste asenkron kullandýk
            });;

            services.AddSingleton<RabbitMQClientService>();//Servisimiz proje ayaðý kalkarken bir kere ayaðý kalksa yeterli
            services.AddSingleton<RabbitMQPublisher>();//Servisimiz proje ayaðý kalkarken bir kere ayaðý kalksa yeterli
            services.AddHostedService<ImageWaterMarkProcesssBackgroundService>();//backround servisimiz de ayaðý kalksýn

            services.AddDbContext<AppDbContext>(opt=> {

                opt.UseInMemoryDatabase(databaseName: "YeniDb");//verilerimizið bu projede inmemoryde kaydedelim.
            });



            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Products}/{action=Index}/{id?}");
            });
        }
    }
}
