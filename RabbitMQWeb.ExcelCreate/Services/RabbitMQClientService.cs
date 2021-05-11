using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate.Services
{
    public class RabbitMQClientService : IDisposable // dispose olunca bağlantılar kapatılsın
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQClientService> _logger;

        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "excelDirectExchange";
        public static string RoutingExcel = "route-excel-file";
        public static string QueueName = "queue-excel-file";

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();

            if (_channel is { IsOpen: true })
            {
                return _channel;
            }
            else
            {
                //Direct Exchange belirli routing ile belirli bir kuyruğu hedef alır. Bu kuyruğu birden fazla consumer dinlerse verileri paylaştırır. Bundan dolayı verilerin bir kere işlenir. Verilerin işleneceğini düşünürsek işlem sürelerini birden fazla consumer ile kısaltabiliriz. Bu exchanhge de kuyruk ve exchange procuder(publisher) tarafından üretilir.

                //autoDelete:işi bitince silinsin mi?
                //durable: diskte kaydolsun mu?
                //exclusive: başka _channeldan erişim sağlanabilsin mi?
                //routingKey: İlgili keye sahip kuyruklara erişim sağlar
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
                _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, null);
                _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingExcel, null);
                _logger.LogInformation("RabbitMQ ile Bağlantı Sağlandı.");

                return _channel;
            }
        }

        public void Dispose()//? bağlantı varsa diyip şartı sağlıyorsa ardından belirtilen işlemi yapar
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile Bağlantı Sonlandı.");
        }
    }
}
