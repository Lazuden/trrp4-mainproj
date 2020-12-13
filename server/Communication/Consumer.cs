using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

namespace Communication
{
    public class Consumer
    {
        private readonly ConnectionFactory _factory;
        private const string ChatExchange = "chex";

        public Consumer(string hostName)
        {
            _factory = new ConnectionFactory() { HostName = hostName, UserName = "Jojo", Password = "Jojo" };
        }

        public bool IsRunning { get; private set; }

        public void Run(IMessageHandler handler, CancellationToken cancellationToken)
        {
            IsRunning = false;
            try
            {
                using var connection = _factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: ChatExchange, type: "fanout");

                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queueName, ChatExchange, "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    handler.Handle(message);
                };

                var consumerTag = channel.BasicConsume(
                    queue: queueName,
                    autoAck: true,
                    consumer: consumer);

                IsRunning = true;

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                using var resetEvent = new ManualResetEvent(false);
                cancellationToken.Register(() =>
                {
                    channel.BasicCancel(consumerTag);
                    resetEvent.Set();
                });

                resetEvent.WaitOne();
            }
            catch { }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
