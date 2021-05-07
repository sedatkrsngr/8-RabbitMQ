using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;


namespace HelloWold.Publisher//producer diğer adı bu kısımda mesajı yayınlıyoruz
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);//projeyi aynı anda ayağıkaldırınca fanout mesajları subscriber(consumer) dinlemeden önce paylaşmamak için bu proje 1 sn sonra başlasın dedik.
            var factory = new ConnectionFactory();

            factory.Uri = new Uri("amqps://orztfhbr:61JVQ4JIieuwkzllmolkOId430Hfzyv4@grouse.rmq.cloudamqp.com/orztfhbr");

            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();

                channel.ExchangeDeclare("logs-header",type:ExchangeType.Headers,durable:true);

                Dictionary<string, object> headers = new Dictionary<string, object>();
                headers.Add("format", "pdf");
                headers.Add("shape", "a4");

                var properties = channel.CreateBasicProperties();
                properties.Headers = headers;
                properties.Persistent = true;//Artık mesajlar kalıcı hale geliyor Header exchange kullanmasakta header kısmında bu bilgiyi gönderirsek mesajlar kalıcı hale gelir. Bu şekilde rabbit mq restart yese dahi mesajlar kaybolmayacak

                //var product = new Product { Id=1,Name="Kalem",Price=100,Stock=10 }; 
             //   var productJsonString = JsonSerializer.Serialize(product);  //kompleks tipi  stringe ardından da byte çevirerek alırız            

            //    channel.BasicPublish("logs-header", string.Empty, properties, Encoding.UTF8.GetBytes(productJsonString));//exchane,routeKey,properties,message

                Console.WriteLine("Mesaj Gönderildi.");
                Console.ReadLine();
            }

        }
    }
}
