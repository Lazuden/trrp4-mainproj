using RabbitMQ.Client;
using System.Text;

namespace Communication
{
    public class Producer
    {
        private readonly ConnectionFactory _factory;
        private const string ChatExchange = "chex";

        public Producer(string hostName)
        {
            _factory = new ConnectionFactory() { HostName = hostName, UserName = "Jojo", Password = "Jojo" };
        }

        public void Send(string message)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: ChatExchange, type: "fanout");

            channel.BasicPublish(
               exchange: ChatExchange,
               routingKey: "",
               basicProperties: null,
               body: Encoding.UTF8.GetBytes(message));
        }
    }
}
