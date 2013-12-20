using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_8;
using ReferenceData.API;

namespace Subscriber.Console
{
    class Program
    {
        static void Main()
        {
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            var factory = new ConnectionFactory { Address = "192.168.1.111" };
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
                        var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var json = Encoding.UTF8.GetString(body);

                        try
                        {
                            var list = Serializer.Deserialize(json);

                            foreach (var entity in list)
                            {
                                string s = "<entity.Name>";
                                if (entity.GetType().IsAssignableFrom(typeof (Agency)))
                                {
                                    s = "Agency.Name=" + ((Agency) entity).Name;
                                }
                                else if (entity.GetType().IsAssignableFrom(typeof (Person)))
                                {
                                    s = "Person.Name=" + ((Person) entity).Name;
                                }
                                System.Console.WriteLine(" [x] {0}", s);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
            }
        }
    }
}
