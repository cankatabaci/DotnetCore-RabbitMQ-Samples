using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace DotnetCoreRabbitMQSamples
{
    class Program
    {
        public static string Hostname = ""; //HostAdress
        public static int Port = 8545; //port

        public static string QueueNameAppointment = "AppointmentQueue";
        public static string ExchangeNameAppointment = "AppointmentQueueExchange";

        public static string QueueNameAdmission = "AdmissionQueue";
        public static string ExchangeNameAdmission = "AdmissionQueueExchange";

        public static string VirtualHost = ""; //HostName
        public static string UserName = ""; //username
        public static string Password = ""; //password


        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = Hostname, Port = Port, VirtualHost = VirtualHost, UserName = UserName, Password = Password };


            // Options for Appointment
            Console.WriteLine("Hi from RabbitMQ... Please enter your message for *Appointment* queue length:");

            string lengthInputAppointment = Console.ReadLine();
            if (!int.TryParse(lengthInputAppointment, out int lengthAppointment))
            {
                Console.WriteLine("oopss... we accept length 5 for Appointment.. ");
                lengthAppointment = 5;
            }

            // Options for Admission
            Console.WriteLine("Hi from RabbitMQ... Please enter your message for *Admission* queue length:");

            string lengthInputAdmission = Console.ReadLine();
            if (!int.TryParse(lengthInputAdmission, out int lengthAdmission))
            {
                Console.WriteLine("oopss... we accept length 15 for Admission.. ");
                lengthAdmission = 15;
            }

            //Appointment queue connection and channel 
            using (IConnection connection = factory.CreateConnection())
            using (IModel channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueNameAppointment,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                for (int i = 0; i < lengthAppointment; i++)
                {
                    var model = new Transaction()
                    {
                        ID = i,
                        CODE = "APT" + i,
                        MESSAGE = "MESSAGE " + (i * 3),
                        TITLE = "TITLE " + (i * 9)
                    };


                    string message = JsonConvert.SerializeObject(model);
                    var body = Encoding.UTF8.GetBytes(message);

                    //channel.BasicPublish(exchange: ExchangeName,
                    //    routingKey: QueueName,
                    //    body: body,
                    //    basicProperties: null);

                    channel.BasicPublish(exchange: ExchangeNameAppointment,
                     routingKey: QueueNameAppointment,
                     basicProperties: null,
                     body: body);

                    Console.WriteLine($"Gönderilen işlem kodu: {model.CODE}");

                }

            }

            //Admission queue connectiond and chanel
            using (IConnection connection = factory.CreateConnection())
            using (IModel channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueNameAdmission,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                for (int i = 0; i < lengthAdmission; i++)
                {
                    var model = new Transaction()
                    {
                        ID = i,
                        CODE = "ADM" + i,
                        MESSAGE = "MESSAGE " + (i * 4),
                        TITLE = "TITLE " + (i * 8)
                    };


                    string message = JsonConvert.SerializeObject(model);
                    var body = Encoding.UTF8.GetBytes(message);


                    channel.BasicPublish(exchange: ExchangeNameAdmission,
                     routingKey: QueueNameAdmission,
                     basicProperties: null,
                     body: body);

                    Console.WriteLine($"Gönderilen işlem kodu: {model.CODE}");

                }

            }
            Console.ReadLine();
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
