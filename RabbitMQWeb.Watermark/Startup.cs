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
            services.AddSingleton(s=>new ConnectionFactory() { //Rabbitmq sunucusuna program aya�a kalkarken bir kez ba�lansak yeter
            Uri= new Uri(Configuration.GetConnectionString("ConStrRabbitMQ")),
            DispatchConsumersAsync=true  //asenkron olarak consumerda method kullan�yorsak bunu true yapmal�y�z.Background serviste asenkron kulland�k
            });;

            services.AddSingleton<RabbitMQClientService>();//Servisimiz proje aya�� kalkarken bir kere aya�� kalksa yeterli
            services.AddSingleton<RabbitMQPublisher>();//Servisimiz proje aya�� kalkarken bir kere aya�� kalksa yeterli
            services.AddHostedService<ImageWaterMarkProcesssBackgroundService>();//backround servisimiz de aya�� kalks�n

            services.AddDbContext<AppDbContext>(opt=> {

                opt.UseInMemoryDatabase(databaseName: "YeniDb");//verilerimizi� bu projede inmemoryde kaydedelim.
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
