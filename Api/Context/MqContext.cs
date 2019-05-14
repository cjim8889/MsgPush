using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace TelePush.Api.Context
{
    public class MqContext
    {
        public MqContext(IConfiguration configuration)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.Uri = new Uri(configuration.GetSection("Mq:ConnectionString").Value);

            Connection = connectionFactory.CreateConnection();
            Channel = Connection.CreateModel();

            Channel.QueueDeclare(queue: configuration.GetSection("Mq:Key").Value,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

        }

        ~MqContext()
        {
            Channel.Close();
            Connection.Close();
        }

        public IConnection Connection { get; }
        public IModel Channel { get; }

    }
}