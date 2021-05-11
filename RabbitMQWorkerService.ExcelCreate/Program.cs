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
                    //Startup i�leri workerservice te program.js i�inde ger�ekle�ir.
                    //WorkerService console ve .Net core aras�ndan bir yerde. Backround service burada g�zelce kullan�labilir. Sadece i� yapmak i�in kullan�lan bir projedir. Not al
                    IConfiguration Configuration = hostContext.Configuration;//Sonradan eklendi. 

                    services.AddDbContext<AdventureWorks2019Context>(opt =>//Veritaban� ba�lant�s� addscoped olarak al�nd� Bu y�zden AddScoped d���nda farkl� bir yerde constructorda e�lemek i�in IServiceProvider kullanmak gerekiyor. �rn. A�a��daki worker clas� i�erisinde kullanmam�z gerekiyor fakat worker addsingleton olarak �a�r�ld��� i�in direkt kullanamay�z. IServiceProvider burada i�imize yarayacakt�r
                    {
                        opt.UseSqlServer(Configuration.GetConnectionString("ConStrSqlServer"));
                    });
                    services.AddSingleton(s => new ConnectionFactory()//Connecttion factoru bir kere �al��s�n.
                    { //Rabbitmq sunucusuna program aya�a kalkarken bir kez ba�lansak yeter
                        Uri = new Uri(Configuration.GetConnectionString("ConStrRabbitMQ")),
                        DispatchConsumersAsync = true  //asenkron olarak consumerda method kullan�yorsak bunu true yapmal�y�z.Background serviste asenkron kulland�k
                    });

                    services.AddSingleton<RabbitMQClientService>();//Servisimiz proje aya�� kalkarken bir kere aya�� kalksa yeterli
                    services.AddHostedService<Worker>();//varsay�lan olarak gelen bu class i�erisinde yap�yoruz. �Stersek farkl� bir class ekleyip te buraya ekleyip i�lem yapabiliriz. Addsingleton olarak eklenmi� oluyor
                });
    }
}
