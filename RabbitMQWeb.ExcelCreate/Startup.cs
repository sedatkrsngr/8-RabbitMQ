using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQWeb.ExcelCreate.Models;
using RabbitMQWeb.ExcelCreate.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate
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
            services.AddSingleton(s => new ConnectionFactory()
            { //Rabbitmq sunucusuna program ayaða kalkarken bir kez baðlansak yeter
                Uri = new Uri(Configuration.GetConnectionString("ConStrRabbitMQ")),
                DispatchConsumersAsync = true  //asenkron olarak consumerda method kullanýyorsak bunu true yapmalýyýz.Background serviste asenkron kullandýk
            });

            services.AddSingleton<RabbitMQClientService>();//Servisimiz proje ayaðý kalkarken bir kere ayaðý kalksa yeterli
            services.AddSingleton<RabbitMQPublisher>();//Servisimiz proje ayaðý kalkarken bir kere ayaðý kalksa yeterli


            services.AddDbContext<AppIdentityContext>(opt =>//Veritabaný baðlantýsý
            {
                opt.UseSqlServer(Configuration.GetConnectionString("ConStrSqlServer"));
            });

            services.AddIdentity<IdentityUser, IdentityRole>(opt =>//identiy kullaný emaili zorunlu kýldýk
            {
                opt.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<AppIdentityContext>();//hangi dbyi kullandýðýmýzý söyledik. Identity kursunda daha detaylý

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

            app.UseAuthentication();///üyelik sistemi olduðu için bunu da dahil ettik
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
