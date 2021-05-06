using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace HelloWold.Publisher//producer diğer adı bu kısımda mesajı yayınlıyoruz
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();

            factory.Uri = new Uri("amqps://orztfhbr:61JVQ4JIieuwkzllmolkOId430Hfzyv4@grouse.rmq.cloudamqp.com/orztfhbr");

            using (var connection = factory.CreateConnection())
            {
                var channel = connection.CreateModel();//yeni bir kanal oluşturuyoruz

                channel.QueueDeclare("Yenikuyruk", true, false,false);//kuyrukadı,durable:false ise kuyruk memoryde tutulu true ise fiziksel olarak bir yere kaydolur. Biz true yaptık.,exclusive:true olursa sadece bu kanal üzerinden bu kuyruhğa erişilir. false ise başka kanallardan da erişilebilir ve biz false yaptık. autodelete: true ise eğer kuyruğa bağlı olan subscriber düşerse kuyruk silinsin demek.False yaptık. Kuyruk oluştur.


                Enumerable.Range(1, 50).ToList().ForEach(x=> {//Tek seferde çalıştığında 50 mesaj atıyor. Bu tek seferde iletilecek mesaj sayısını iyi ayarlamak gerekiyor. Eğer mesajın işlenmesi uzun sürüyorsa daha az sayıda tek seferde göndermek daha mantıklı olacaktır.

                    string message = $"Message {x}";//mesajımız

                    var messagebody = Encoding.UTF8.GetBytes(message);//Rabbitmq ya mesajları byte dizisi olarak göndeririz. Bu sayede herşeyi gönderebiliriz. pdf,resim,word herşey

                    channel.BasicPublish(string.Empty, "Yenikuyruk", null, messagebody);//exchange kullanmadığımız için string.empty olarak boş gönderdik eğer boş gönderirsek rouitingKey kuyruk adımız olmalı bizimki YeniKuyruk, IBasicProperties şimdilik null gönderelim. ve mesajımız
                    Console.WriteLine("mesaj gönderildi.");
                });

                
                Console.ReadLine();//console kapanmasın ki izleyelim
            }

        }
    }
}
