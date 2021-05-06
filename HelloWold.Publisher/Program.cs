using RabbitMQ.Client;
using System;
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

                //FanoutExchange de kuyruğu producer(publisher) oluşturmaz. Consumer(subscriber) isterse veri için kuyruk oluşturur. Ve consumer kapandığında consumer ayarına göre kuyruk silinir. Örn: Hava saatlik hava durumu yayınlayan bir projem var exchange ile kaç kişi bu veriye ulaşacak bilmediğim için veriyi isteyen kişiler kuyruk oluşturup veriyi alabilir. Biz sadece yayınlamakla mükellefiz.Bu yüzden kuyruk oluşturma alanı yorum satırı haline getirildi. Fanout veriyi direkt olarak oluşturulmuş her kuyruğa verir. Yani tüm kuyruklar procuder(publisher) dan gelen veriyi aynı dinler. 

                // channel.QueueDeclare("Yenikuyruk", true, false,false);//kuyrukadı,durable:false ise kuyruk memoryde tutulu true ise fiziksel olarak bir yere kaydolur. Biz true yaptık.,exclusive:true olursa sadece bu kanal üzerinden bu kuyruhğa erişilir. false ise başka kanallardan da erişilebilir ve biz false yaptık. autodelete: true ise eğer kuyruğa bağlı olan subscriber düşerse kuyruk silinsin demek.False yaptık. Kuyruk oluştur.

                channel.ExchangeDeclare("logs-fanout",type:ExchangeType.Fanout,durable:true);

                Enumerable.Range(1, 50).ToList().ForEach(x=> {

                    string message = $"Message {x}";

                    var messagebody = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish("logs-fanout","", null, messagebody);//exchange:logs-fanout , rouitingKey:"", IBasicProperties şimdilik null gönderelim. ve mesajımız
                    Console.WriteLine("mesaj gönderildi.");
                });

                
                Console.ReadLine();
            }

        }
    }
}
