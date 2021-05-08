using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMQWeb.Watermark.Services
{//Producer(Publisher) Tarafı
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(ProductImageCreatedEvent productImageCreatedEvent)
        {
            var channel = _rabbitMQClientService.Connect();

            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;//mesajlar kalıcı hale gelsin dedik

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingWaterMark, basicProperties: properties, body: bodyByte);

            //event ile mesaj farkı nedir? Mesela bu projede event kullandık

            //Mesajlarda işlenecek data taşınır ve istenen(gelecek) ata bellidir. Mesela ben wordtopdf,texttoexcel... yani vereceğim nesneden ne üretileceği bellidir. 
            //Eventte ise data yerine daha çok etkilenecek kullanıcıId,Resimİsmi vs gibi üzerinde ne yapılacağını bilmediği veriyi yollar ve consumer tarafı o bilgiyle birtakım işlemler yapaiblir. Mesela biz Resimİsmini gönderiyoruz ProductImageCreatedEvent sınıfıyla bu yoldaki resimle event watermark ekleyecek bu proje için ama daha farklı şeyler de yapabilirdi.

        }
    }
}


