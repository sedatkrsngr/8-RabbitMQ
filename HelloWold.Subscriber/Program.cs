using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

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
                var channel = connection.CreateModel();

                // channel.QueueDeclare("Yenikuyruk", true, false, false);//publisher tarafında bu kuyruğun oluştuğundan eminsek burada yazmamıza gerek yok ama burada olması var olan kuyruğu etkilemez fakat kuyruk yoksa burada oluşturur. Ama birebir aynı olamlı eğer burada da oluşturacaksak parametreleri. Aşağıda kuyruğu her türlü çağıracağımızdan bir daha oluşturmanın bir zararı yok

                channel.BasicQos(0, 1, false);//prefetchSize:0 yaptık yani consumer herhangi bir boyuttan veri alabilir.,prefetchCount: consumerların kaç tane mesaj alabileceğini belirtiyoruz. Bool Global ise: false ise kaçtane consumer varsa herbirine verdiğimiz  prefetchCount kadar mesaj iletilir. eğer false yaparsak verdiğimiz prefetchCount tüm consumerlara toplamı  prefetchCount kardar olacak kadar mesaj verilir. Örn: prefetchCount: verdik 3 consumer var. 2,2,1 şeklinde consumerlara toplam 5 olacak şekilde iletilir. Yani ne kadar çok subscriber(Consumer) proje ayaktaysa o kadar dinleme hızlı biter

                var consumer = new EventingBasicConsumer(channel);

                channel.BasicConsume("Yenikuyruk", false, consumer);//kuyruk adı,autoAck:false Work Queue konusu için false yaptık ve eventte yeni tanım var


                consumer.Received += (object sender, BasicDeliverEventArgs e)=>
                    {
                    var message = Encoding.UTF8.GetString(e.Body.ToArray());
                        channel.BasicAck(e.DeliveryTag,false);//false yaptığımız autoAck işlemi için eğer mesaj teslim edildiyse kuyruktan sil demek

                    Console.WriteLine("Gelen Mesaj: " + message);

                };// bu şekilde kullanım daha pratik

                Console.ReadLine();
            }
        }


    }
}
