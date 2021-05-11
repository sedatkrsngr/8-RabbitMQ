using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQWorkerService.ExcelCreate.Models;
using RabbitMQWorkerService.ExcelCreate.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWorkerService.ExcelCreate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //Startup iþleri workerservice te program.js içinde gerçekleþir.
                    //WorkerService console ve .Net core arasýndan bir yerde. Backround service burada güzelce kullanýlabilir. Sadece iþ yapmak için kullanýlan bir projedir. Not al
                    IConfiguration Configuration = hostContext.Configuration;//Sonradan eklendi. 

                    services.AddDbContext<AdventureWorks2019Context>(opt =>//Veritabaný baðlantýsý addscoped olarak alýndý Bu yüzden AddScoped dýþýnda farklý bir yerde constructorda eþlemek için IServiceProvider kullanmak gerekiyor. Örn. Aþaðýdaki worker clasý içerisinde kullanmamýz gerekiyor fakat worker addsingleton olarak çaðrýldýðý için direkt kullanamayýz. IServiceProvider burada iþimize yarayacaktýr
                    {
                        opt.UseSqlServer(Configuration.GetConnectionString("ConStrSqlServer"));
                    });
                    services.AddSingleton(s => new ConnectionFactory()//Connecttion factoru bir kere çalýþsýn.
                    { //Rabbitmq sunucusuna program ayaða kalkarken bir kez baðlansak yeter
                        Uri = new Uri(Configuration.GetConnectionString("ConStrRabbitMQ")),
                        DispatchConsumersAsync = true  //asenkron olarak consumerda method kullanýyorsak bunu true yapmalýyýz.Background serviste asenkron kullandýk
                    });

                    services.AddSingleton<RabbitMQClientService>();//Servisimiz proje ayaðý kalkarken bir kere ayaðý kalksa yeterli
                    services.AddHostedService<Worker>();//varsayýlan olarak gelen bu class içerisinde yapýyoruz. ÝStersek farklý bir class ekleyip te buraya ekleyip iþlem yapabiliriz. Addsingleton olarak eklenmiþ oluyor
                });
    }
}
