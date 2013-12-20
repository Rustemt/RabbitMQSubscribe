using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Subscriber.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            var factory = new ConnectionFactory() { Address = "192.168.1.111" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("logs", "fanout");

                    System.Console.WriteLine("Enter Queue Name:");
                    var name = System.Console.ReadLine();

                    var queueName = channel.QueueDeclare(name, true, false, false, null);

                    channel.QueueBind(queueName, "logs", "");
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, false, consumer);

                    System.Console.WriteLine(" [*] Waiting for logs." +
                                      "To exit press CTRL+C");
                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        System.Console.WriteLine(" [x] {0}", message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
            }
        }
    }
}
