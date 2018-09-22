using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace ConsumerAppointment
{
    class Program
    {
        public static string Hostname = ""; //HostAdress
        public static int Port = 8545; //port


        public static string QueueNameAppointment = "AppointmentQueue";
        public static string ExchangeNameAppointment = "AppointmentQueueExchange";

        public static string VirtualHost = ""; //HostName
        public static string UserName = ""; //username
        public static string Password = ""; //password

        static void Main(string[] args)
        {

            var factory = new ConnectionFactory() { HostName = Hostname, Port = Port, VirtualHost = VirtualHost, UserName = UserName, Password = Password };

            Console.WriteLine("Appointment kuyruğundan merhaba)");
            using (IConnection connection = factory.CreateConnection())
            using (IModel channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueNameAppointment,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Transaction transaction = JsonConvert.DeserializeObject<Transaction>(message);
                    Console.WriteLine($" ID: {transaction.ID} Code: {transaction.CODE} Title: {transaction.TITLE} Message: {transaction.MESSAGE}");
                };
                channel.BasicConsume(queue: QueueNameAppointment,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine("Appointment'lar alındı...)");
                Console.ReadLine();
            }
        }
    }

    public class Transaction
    {
        public int ID { get; set; }
        public string CODE { get; set; }
        public string TITLE { get; set; }
        public string MESSAGE { get; set; }
    }
}
