using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWeb.Watermark.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQWeb.Watermark.BackgroundServices
{//consume(subscriber) Tarafı servis olarak çalışacak
    public class ImageWaterMarkProcesssBackgroundService : BackgroundService//Uygulama ayağa kalkınca devamlı çalışacak service 
    {

        private readonly RabbitMQClientService _rabbitMQClientService; //not readonly ler sadece constructorda set edilir.
        private readonly ILogger<ImageWaterMarkProcesssBackgroundService> _logger;

        private IModel _channel;

        public ImageWaterMarkProcesssBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWaterMarkProcesssBackgroundService> logger)
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)//kaltım alınca geldi. Uygulama çalışmaya başlayınca çalışır
        {
            var consumerRabbit = new AsyncEventingBasicConsumer(_channel);

            _channel.BasicConsume(queue:RabbitMQClientService.QueueName,autoAck:false,consumer: consumerRabbit);

            //autoAck: Rabbit mq mesajı silinsin mi demek? Eğer true yaparsak evet işlem biter bitmez siler, false dersek işlem tamamlandı bilgisi gidince silinir.

            consumerRabbit.Received += ConsumerRabbit_Received;//+= tab tab ConsumerRabbit_Received metodunu aşağıda oluşturdu

            return Task.CompletedTask;

        }

        private  Task ConsumerRabbit_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                string jsonproductImageCreatedEvent = Encoding.UTF8.GetString(@event.Body.ToArray());
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(jsonproductImageCreatedEvent);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Imgs", productImageCreatedEvent.ImageName);


                using var img = Image.FromFile(path);
                using var graphic = Graphics.FromImage(img);

                var text = "sedat";
                var font = new Font(FontFamily.GenericMonospace, 10, FontStyle.Bold, GraphicsUnit.Pixel);//yazının fontu

                var textSize = graphic.MeasureString(text, font);//yazılacak yazı ve yazının olacağı şekil

                var color = Color.FromArgb(128, 255, 255, 255);

                var brush = new SolidBrush(color);//yazı yazma işlemi gerçekleştirme

                var position = new Point(img.Width - ((int)textSize.Width + 5), img.Height - ((int)textSize.Height + 5));//yazının yazılacağı pozisyon

                //örn elimde 300x500 resim varsa ve buna 12 birimlik yazı ekleyeceksek.  300-12+5,500-12+5=(283,483) konumunda olacak yazı

                graphic.DrawString(text, font, brush, position);//bunu kod yazarak yaptık ama watermark eklemek için kütüphaneler de var bilgin olsun

                img.Save("wwwroot/images/watermarkImgs/" + "watermark-" + productImageCreatedEvent.ImageName);//watewrmark+ eski resim ismi

                img.Dispose();
                graphic.Dispose();

                _channel.BasicAck(deliveryTag:@event.DeliveryTag,multiple:false);//mesajın ulaştı bilgisini ver demek: Buna göre silinecek kuyukta,multiple ise sadece bu veri için ilet demek 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return Task.CompletedTask;
         
        }

        public override Task StartAsync(CancellationToken cancellationToken)//override ile geldi uygulama ayağa kalkınca çalışır
        {

            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            //prefetchSize: hangi boyuttaki dosyaları işlesin, 0 hepsini alsın demek
            //prefetchCount: kaç tane işlesin: 1 diyerek sadece bir tane işlemesini söyledik
            //global: Consumerların verilen değere göre prefetchCount'taki veri kadar işleyeceğini belirtir. False dersek tüm consumerlar toplam prefetchCount kadar dinlerken. True ise consumerlar ayrı ayrı prefetchCount kadar dinleyebilir demek

            return base.StartAsync(cancellationToken);
        }

    }
}
