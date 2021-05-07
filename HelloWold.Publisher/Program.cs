using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var channel = connection.CreateModel();//yeni bir kanal oluşturuyoruz

                //Header Exchange de  routekeyler mesajın headerında key value şeklinde producer(publish)den gönderilir. Kuyruk yine consumer tarafında oluşması daha mantıklı Bu sfer verileri alırken key value değerleri gönderirken consumer(subscriber tarafında) headerda gönderdiğimiz verileri alabilmek için o tarftada gönderdiğimiz key value değerleri kuyrukları çağırmak için yazılmalı. O tarafta ekstradan gönderilen key value değerlerinin hepsini mi kuyruğun headerinde arasın yoksa sadece herhangi biri olursa da o kuyruğu dinlesin mi? İşte bunun için "x-match" key var. Eğer "all" olarak value değerini  verirsek consumer tarafında. Consumerda Gönderdiğimiz tüm key value değerlerine sahip kuyruğu dinler. "any" yaparsak ise sadece bir tanesi olsa dahi o kuyruğu dinleyebiliriz.
                //Dictionary<string, object> headers = new Dictionary<string, object>();
                //headers.Add("format", "pdf");
                //headers.Add("shape", "a4");
                //headers.Add("x-match", "all");

                channel.ExchangeDeclare("logs-header",type:ExchangeType.Headers,durable:true);

                Dictionary<string, object> headers = new Dictionary<string, object>();
                headers.Add("format", "pdf");
                headers.Add("shape", "a4");

                var properties = channel.CreateBasicProperties();
                properties.Headers = headers;

                channel.BasicPublish("logs-header", string.Empty, properties, Encoding.UTF8.GetBytes("header mesajım"));//exchane,routeKey,properties,message

                Console.WriteLine("Mesaj Gönderildi.");
                Console.ReadLine();
            }

        }
    }
}
