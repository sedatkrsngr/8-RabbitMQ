using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace HelloWold.Subscriber//consumer diğer adı
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var factory = new ConnectionFactory();

            factory.Uri = new Uri("amqps://orztfhbr:61JVQ4JIieuwkzllmolkOId430Hfzyv4@grouse.rmq.cloudamqp.com/orztfhbr");

            using (var connection = factory.CreateConnection())
            {
               // var routeKey = "*.Error.*";//Ortasında Error olan routekeye sahip kuyrukları dinleyelim
                //var routeKey = "*.*.Error";//sonunda Error olan routekeye sahip kuyrukları dinleyelim
                var routeKey = "Error.#";//Başı Error olan routekeye sahip kuyrukları dinleyelim
                var channel = connection.CreateModel();

                channel.BasicQos(0, 1, false);//prefetchSize:0 yaptık yani consumer herhangi bir boyuttan veri alabilir.,prefetchCount: consumerların kaç tane mesaj alabileceğini belirtiyoruz. Bool Global ise: false ise kaçtane consumer varsa herbirine verdiğimiz  prefetchCount kadar mesaj iletilir. eğer false yaparsak verdiğimiz prefetchCount tüm consumerlara toplamı  prefetchCount kardar olacak kadar mesaj verilir. Örn: prefetchCount: verdik 3 consumer var. 2,2,1 şeklinde consumerlara toplam 5 olacak şekilde iletilir. Yani ne kadar çok subscriber(Consumer) proje ayaktaysa o kadar dinleme hızlı biter
                var randomQueueName = channel.QueueDeclare().QueueName;//random olarak kuyruk adı oluşturur. İşi bitince silinir.

                channel.QueueBind(randomQueueName, "logs-topic",routeKey,null);//Birden fazla  kez projeyi ayağı kaldırınca aynı kuyruk ismi olması sıkıntı yaratır diye random isimler veriyoruz ve exchange bağlıyoruz, subscribe(consumer) down olduğunda kuyruk böylelikle otomatik silinir. Böylelikle her ayrı proje ayrı kuyrukta veriyi alacak ama eğer belli bir projede veriyi çekeceksek kendimiz de isim verebiliriz ve bu kuyruğa gönderilmiş mesajlar kalıcı olacaktır ve bu kuyruğa gelen verileri consumer down olsa dahi başka bir consumerdan bu kuyruk adına ulaşarak veriyi alabiliriz.

               

                var consumer = new EventingBasicConsumer(channel);

                channel.BasicConsume(randomQueueName, false, consumer);//kuyruk adı,autoAck:false Work Queue konusu için false yaptık ve eventte yeni tanım var
                Console.WriteLine("Mesajlar dinleniyor");

                consumer.Received += (object sender, BasicDeliverEventArgs e) =>
                    {
                        var message = Encoding.UTF8.GetString(e.Body.ToArray());

                        Console.WriteLine("Gelen Mesaj: " + message);
                        channel.BasicAck(e.DeliveryTag, false);//false yaptığımız autoAck işlemi için eğer mesaj teslim edildiyse kuyruktan sil demek
                        
                    };// bu şekilde kullanım daha pratik

                Console.ReadLine();
            }
        }


    }
}
