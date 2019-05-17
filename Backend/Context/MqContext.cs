using System;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace TelePush.Backend.Context
{
    public static class MqMessageType
    {
        public const string Message = "Message";
        public const string HookAction = "HookAction";
        public const string HookValidation = "HookValidation";
    }

    class MqContext
    {

        public MqContext(IConfiguration configuration)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(configuration.GetSection("Mq:ConnectionString").Value);

            Connection = factory.CreateConnection();
            Channel = Connection.CreateModel();

            Channel.QueueDeclare(queue: configuration.GetSection("Mq:Key").Value,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

        }

        ~MqContext()
        {
            Connection.Close();
            Channel.Close();
        }


        public IModel Channel { get; }
        public IConnection Connection { get; }

    }
}
