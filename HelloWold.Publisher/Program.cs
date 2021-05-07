using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace HelloWold.Publisher//producer diğer adı bu kısımda mesajı yayınlıyoruz
{
    public enum lognames
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Info = 4
    }
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);//projeyi aynı anda ayağıkaldırınca fanout mesajları subscriber(consumer) dinlemeden önce paylaşmamak için bu proje 1 sn sonra başlasın dedik.
            var factory = new ConnectionFactory();

            factory.Uri = new Uri("amqps://orztfhbr:61JVQ4JIieuwkzllmolkOId430Hfzyv4@grouse.rmq.cloudamqp.com/orztfhbr");

            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();//yeni bir kanal oluşturuyoruz

                //Topic Exchange de kuyruğu  producer(publisher) oluşturmaz.  veriyi routekey ile gönderir ve o route key kullanan consumerlar ilgili olan route sahib kuyruklardan veriyi alır. Topicte routekeyler birden fazla olabilir ve aralarında . kullanılır. Bu yüzden birden fazla route key içerisinde belirli değere göre kuyruklar mesajı iletebilir aşağıda örnek mevcut.
                //örn: Consumer belirli keye ait kuyrukları çağırmak istediğinde aşağıdaki gibi kullanım route keylerde kullanılır.
                // var routeKey = "*.Error.*";//Ortasında Error olan routekeye sahip kuyrukları dinleyelim
                //var routeKey = "*.*.Error";//sonunda Error olan routekeye sahip kuyrukları dinleyelim
               // var routeKey = "Error.#";//Başı Error olan routekeye sahip kuyrukları dinleyelim



                channel.ExchangeDeclare("logs-topic", type: ExchangeType.Topic, durable: true);

                Random random = new Random();
                Enumerable.Range(1, 50).ToList().ForEach(x =>
                {
                    lognames log1 = (lognames)random.Next(1, 5);
                    lognames log2 = (lognames)random.Next(1, 5);
                    lognames log3 = (lognames)random.Next(1, 5);


                    var routeKey = $"{log1}.{log2}.{log3}";//hangi mesaj hangi kuyruğa gidecek burdan belirliyoruz. Yani key ile veriyi göndereceğimiz kuyruğu biliyoruz. Topicte birden fazla key verebiliyoruz
                    string message = $"Message-{log1}-{log2}-{log3}";

                    var messagebody = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("logs-topic", routeKey, null, messagebody);//exchange:logs-direct , rouitingKey:routeKey, IBasicProperties şimdilik null gönderelim. ve mesajımız // exchange routekeye sahip olan kuyruğa mesajı iletiyor
                    Console.WriteLine("mesaj gönderildi.");
                });


                Console.ReadLine();
            }

        }
    }
}
