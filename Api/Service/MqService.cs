using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelePush.Api.Context;
using TelePush.Api.Model;

namespace TelePush.Api.Service
{
    public class MqService
    {
        private readonly MqContext mqContext;
        private readonly IConfiguration configuration;
        public MqService(MqContext mqContext, IConfiguration configuration)
        {
            this.mqContext = mqContext;
            this.configuration = configuration;
        }

        public async Task PushMessage(string message, List<long> receiver)
        {

            var messageJson = JsonConvert.SerializeObject(new MqMessage { Content = message, Receivers = receiver });
            var body = Encoding.UTF8.GetBytes(messageJson);

            var task = Task.Run(() => {
                mqContext.Channel.BasicPublish(exchange: "",
                    routingKey: configuration.GetSection("Mq:Key").Value,
                    basicProperties: null,
                    body: body);
                });


            await task;
        }

        public async Task PushMessage(string message, long receiver)
        {
            await PushMessage(message, new List<long> { receiver });
        }

        public async Task PushChallengeMessage(string challengeCode, long receiver)
        {
            await PushChallengeMessage(challengeCode, new List<long> { receiver });
        }

        public async Task PushChallengeMessage(string challengeCode, List<long> receiver)
        {

            string message = $"Challenge Code: {challengeCode}\n";

            await PushMessage(message, receiver);
        }

    }
}
