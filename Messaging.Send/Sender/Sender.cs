using System;
using RabbitMQ.Client;
using System.Text;
using System.Text.RegularExpressions;

namespace Sender
{
    

    public class RabbitMQSender
    {
        private static IConnection _connection;

        public static void Main(){
            Console.WriteLine("Enter your Name:");
            string userName = Console.ReadLine();
                        
            SendName(userName);
            
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();    
        }

        public static void SendName(string nameInput){
            if(ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                        channel.QueueDeclare(queue: "task_queues",
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

                        var body = Encoding.UTF8.GetBytes(nameInput);

                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        channel.BasicPublish(exchange: "",
                                            routingKey: "task_queues",
                                            basicProperties: properties,
                                            body: body); 

                        Console.WriteLine(" [x] Sent {0}", nameInput);                                                           
                                        
                }
            }
        }

        private static void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create connection: {ex.Message}");
            }
            
        }

        private static bool ConnectionExists()
        {
            if(_connection != null)
            {
                return true;
            }

            CreateConnection();
            return _connection != null;
        }

    }

}