using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace HelloWold.Publisher//producer diğer adı bu kısımda mesajı yayınlıyoruz
{
    public enum lognames
    {
        Critical=1,
        Error=2,
        Warning=3,
        Info=4
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

                //Direct Exchange de kuyruğu ve exchangeyi producer(publisher) oluşturur. Direct veriyi ilgili olan oluşturulmuş  kuyruğa verir. Örn: Elimizde veri grupları var ve her bir veri grubu belli bir kuyruğa iletilecek şekildedir. Bu projemizde mesaj seviyeleri enum olarak kendimiz oluşturduk ve bu enum seviyelerine göre kuyruklara mesaj ileticez. Eğer aynı kuyruk için iki sunucu ayaktaysa ikisine de verileri farklı olarak atar. Yani ne kadar çok sunucu kuyruğu dinlerse o kadar hızlı dinleme biter

               

                channel.ExchangeDeclare("logs-direct", type: ExchangeType.Direct, durable: true);

                Enum.GetNames(typeof(lognames)).ToList().ForEach(x=> {
                    var routeKey = $"route-{x}";//hangi mesaj hangi kuyruğa gidecek burdan belirliyoruz. Yani key ile veriyi göndereceğimiz kuyruğu biliyoruz
                    var queueName = $"direct-queue-{x}";//mesaj enumlarına göre bir kuyruklar oluşturduk

                    channel.QueueDeclare(queueName, true, false,false);//kuyrukadı,durable:false ise kuyruk memoryde tutulu true ise fiziksel olarak bir yere kaydolur. Biz true yaptık.,exclusive:true olursa sadece bu kanal üzerinden bu kuyruhğa erişilir. false ise başka kanallardan da erişilebilir ve biz false yaptık. autodelete: true ise eğer kuyruğa bağlı olan subscriber düşerse kuyruk silinsin demek.False yaptık. Kuyruk oluştur.

                    channel.QueueBind(queueName, "logs-direct",routeKey, null);//kuyruğu keyi ile exchange tanıtıyoruz

                });

                Enumerable.Range(1, 50).ToList().ForEach(x =>
                {
                    lognames log =(lognames) new Random().Next(1, 5);// 50 mesaj gönderirken rastgele sevilerde gönderelim
                    
                    string message = $"Message-{log}";

                    var messagebody = Encoding.UTF8.GetBytes(message);
                    var routeKey = $"route-{log}";

                    channel.BasicPublish("logs-direct", routeKey,null, messagebody);//exchange:logs-direct , rouitingKey:routeKey, IBasicProperties şimdilik null gönderelim. ve mesajımız // exchange routekeye sahip olan kuyruğa mesajı iletiyor
                    Console.WriteLine("mesaj gönderildi.");
                });


                Console.ReadLine();
            }

        }
    }
}
