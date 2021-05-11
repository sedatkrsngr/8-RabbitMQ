using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQWeb.ExcelCreate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate
{
    public class Program
    {
     

        public static void Main(string[] args)
        {
            // CreateHostBuilder(args).Build().Run();//eski hali
            var host = CreateHostBuilder(args).Build();

            //burayý yazma amacýmýz veritabanýna migration yapmayý falan unuttuðumuzda program ayaðý kalkarken migation iþlemlerini yapsýn diye Not olarak dursun kenarda
            using (var scope = host.Services.CreateScope())
            {

                var appIdentityContext = scope.ServiceProvider.GetRequiredService<AppIdentityContext>();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                appIdentityContext.Database.Migrate();


                if (!appIdentityContext.Users.Any())//kullanýcý yoksa
                {
                    userManager.CreateAsync(new IdentityUser() { UserName = "denem2e", Email = "deneme@outlook.com" }, "Password12*").Wait();


                    userManager.CreateAsync(new IdentityUser() { UserName = "deneme32", Email = "deneme2@outlook.com" }, "Password12*").Wait();
                }


            }



            host.Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
