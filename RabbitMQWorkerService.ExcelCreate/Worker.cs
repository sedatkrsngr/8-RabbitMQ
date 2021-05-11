using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWorkerService.ExcelCreate.Models;
using RabbitMQWorkerService.ExcelCreate.Services;
using Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWorkerService.ExcelCreate
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;//_contexti addsingleton olarak bu clasa uymadýðý için böyle alýcaz
        private readonly RabbitMQClientService _rabbitMQClientService;

        private IModel _channel;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, RabbitMQClientService rabbitMQClientService)
        {
            _serviceProvider = serviceProvider;
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
        }

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }


        public override Task StartAsync(CancellationToken cancellationToken)//baðlantý için eklendi
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);//boyutu önemli deðil,birer birer gönder,herbir subsribera
            return base.StartAsync(cancellationToken);
        }
        protected override  Task ExecuteAsync(CancellationToken stoppingToken)//Program çalýþmaya baþlayýnca
        {
            //ilk hali
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);//Workers service her 1 sn de bir çalýþýr. Uygulama ayaða kalkarken çalýþýr
            //}

            var consumer = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicConsume(RabbitMQClientService.QueueName,false,consumer);
            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);

            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));//shareddan aldýk

            using var ms = new MemoryStream();//exceli memoryda tutucam

            //ClosedSml Eklendi
            var wb = new XLWorkbook();

            var ds = new DataSet();

            ds.Tables.Add(GetTable("products"));//dataseti dolduruyoruz

            wb.Worksheets.Add(ds);

            wb.SaveAs(ms);

            MultipartFormDataContent content = new();
            content.Add(new ByteArrayContent(ms.ToArray()),"file",Guid.NewGuid().ToString()+".xlsx");//file Api içerisindeki IFormFile adý file olarak belirttik ondan. Guid ile verdiðimiz alan ikinci adý ama iþimize yaramaz

            var baseUrl = "https://localhost:44306/api/FilesAPI";

            using (var httpClient = new HttpClient())
            {
                var resp = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}", content);

                if (resp.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Ýþlem Baþarýlý");
                    _channel.BasicAck(@event.DeliveryTag,false);
                }
            }
           
        }

        private DataTable GetTable(string tableName)
        {

            List<RabbitMQWorkerService.ExcelCreate.Models.Product> products;

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();
                products = context.Products.ToList();
            }
            DataTable table = new DataTable()
            {
                TableName = tableName
            };

            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("Color", typeof(string));

            products.ForEach(x =>
            {
                table.Rows.Add(x.ProductId,x.Name,x.ProductNumber,x.Color);

            });

            return table;
        }

    }
}
