//PBT205 - TORRENS UNIVERSITY AUSTRALIA
//GROUP 1
//COMMAND LINE APP - TASK 1 - CHATTING APPLICATION

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace PBT205Chat
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Please specify a username:");
            var username = Console.ReadLine();
            var password = username;

            //user1, user2,....user n 
            //users in Rabbitmq are user1 and user2
            //you have tu put for instance 1= user1 and instance 2= user2....so on


            Console.WriteLine("Please specify a chat room name:");
            var roomName  = Console.ReadLine();
            var exchangeName = "chat2";
            //any chat room name, the same name is the same room for any instance

            // Create unique queue name for this instance
            var queueName = Guid.NewGuid().ToString();

            //Connect to RabbitMQ
            var factory = new ConnectionFactory();
            factory.Uri = new Uri($"amqp://{username}:{password}@localhost:5672");
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            // Declare exchange and queue
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, true, true, true);
            channel.QueueBind(queueName, exchangeName, roomName);
            // Subscribe to incoming messages
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, eventArgs) =>
            {
                var text = System.Text.Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var user = eventArgs.BasicProperties.UserId;
                Console.WriteLine(user + ": " + text);
            };

            channel.BasicConsume(queueName, true, consumer);
            // Read input
            var input = Console.ReadLine();
            while (input != "")
            {
                //Remove line
                // Console.SetCursorPosition(0, Console.CursorTop -1);
                //ClearCurrentConsoleLine();

                //Send outgoing message
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var props = channel.CreateBasicProperties();
                props.UserId = username;
                channel.BasicPublish(exchangeName, roomName, props, bytes);
                input = Console.ReadLine();
            }

            channel.Close();
            connection.Close();


        }
    }
}
