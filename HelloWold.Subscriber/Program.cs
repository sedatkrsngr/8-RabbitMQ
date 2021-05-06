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


                var consumer = new EventingBasicConsumer(channel);

                channel.BasicConsume("Yenikuyruk", true, consumer);//kuyruk adı,autoAck:true kuyruk mesajı gönderdiği an silsin kuyruktan false ise mesajın iletildiğinden emin olunduğunda silsin. Biz şimdilk true yapıyoruz.

                consumer.Received += Consumer_Received;// bu şekilde eventi oluşturduk += tab tab Consumer_Received oluştu

                Console.ReadLine();
            }
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs e)//oluşan method
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());

            Console.WriteLine("Gelen Mesaj: "+message);
        }
    }
}
