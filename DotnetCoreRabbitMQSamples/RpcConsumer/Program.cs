using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RpcConsumer
{

    class Program
    {
        public static string Hostname = ""; //HostAdress
        public static int Port = 8545; //port

        public static string VirtualHost = ""; //HostName
        public static string UserName = ""; //username
        public static string Password = ""; //password

        public static string QueueNameRPC = "RpcQueue";
        public static string ExchangeNameRPC = "RpcQueueExchange";

        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory
            {
                Port = Port,
                HostName = Hostname,
                UserName = UserName,
                Password = Password,
                VirtualHost = VirtualHost
            };

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueNameRPC, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: QueueNameRPC, autoAck: false, consumer: consumer);

                Console.WriteLine(" [x] Awaiting RPC requests");

                consumer.Received += (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        int n = int.Parse(message);
                        Console.WriteLine(" [.] fib({0})", message);
                        Console.WriteLine("CorrelationID: " + replyProps.CorrelationId);
                        response = fib(n).ToString() + " CorrelationID: " + replyProps.CorrelationId;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                          basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                          multiple: false);
                    }
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static int fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return fib(n - 1) + fib(n - 2);
        }
    }
}
