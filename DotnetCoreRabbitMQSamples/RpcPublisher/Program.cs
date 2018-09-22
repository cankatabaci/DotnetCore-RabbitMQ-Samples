using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace RpcPublisher
{
    public class RpcClient
    {
        public static string Hostname = ""; //HostAdress
        public static int Port = 8545; //port

        public static string VirtualHost = ""; //HostName
        public static string UserName = ""; //username
        public static string Password = ""; //password

        public static string QueueNameRPC = "RpcQueue";
        public static string ExchangeNameRPC = "RpcQueueExchange";

        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public RpcClient()
        {
            var connectionFactory = new ConnectionFactory
            {
                Port = Port,
                HostName = Hostname,
                UserName = UserName,
                Password = Password,
                VirtualHost = VirtualHost
            };

            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
        }

        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: ExchangeNameRPC,
                routingKey: QueueNameRPC,
                basicProperties: props,
                body: messageBytes);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return respQueue.Take(); ;
        }

        public void Close()
        {
            connection.Close();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var rpcClient = new RpcClient();

            Console.WriteLine(" [x] Requesting fib(5)");
            var response = rpcClient.Call("5");

            Console.WriteLine(" [.] Got '{0}'", response);
            rpcClient.Close();
        }
    }
}
