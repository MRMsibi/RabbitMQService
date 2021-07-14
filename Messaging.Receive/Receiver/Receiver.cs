using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Receiver
{
    class RabbitMQReceiver
    {
        private static IConnection _connection;
        private static ConnectionFactory _factory;
        
        public static void Main(){
            CreateConnection();
            ProcessName();
        }

        public static void ProcessName()
        {
            if(ConnectionExists())
            {
                using(var connection = _factory.CreateConnection())
                using(var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "task_queues",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received +=  async (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" Hello {0}, I am your father!", message);

                        //Console.WriteLine(" [x] Done");

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        await Task.Yield();
                    };
                    string tag = channel.BasicConsume(queue: "task_queues",
                                                      autoAck: false,
                                                      consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }

        public static void CreateConnection()
        {
            try{
                _factory = new ConnectionFactory{
                    HostName = "localhost", 
                    UserName = "guest", 
                    Password = "guest"
                };
                _factory.DispatchConsumersAsync = true;
                _connection = _factory.CreateConnection();
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

        private static bool isValidName(string nameInput)
        {
            bool isValid = true;
            if(string.IsNullOrEmpty(nameInput))
                isValid = false;
            else
            {
                isValid = Regex.IsMatch(nameInput, @"^[a-zA-Z]+$");

                foreach (char c in nameInput)
                {
                    if(!Char.IsLetter(c))
                        isValid = false;
                }
            }
            return isValid;    
        }
    }
}
