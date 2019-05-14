using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelePush.Backend.Context;
using TelePush.Backend.Core;
using TelePush.Backend.Model;
using TelePush.Backend.Utility;

namespace TelePush.Backend
{
    class TelegramServer
    {
        //Can not have multiple dependencies with the same type signature (at the moment);

        private readonly IConfiguration configuration;
        private readonly MqContext mqContext;
        private readonly TelegramContext telegramContext;
        private readonly EventingBasicConsumer mqConsumer;
        private readonly Dispatcher dispatcher;

        public TelegramServer(IConfiguration configuration, TelegramContext telegramContext, Dispatcher dispatcher)
        {
            this.configuration = configuration;
            this.telegramContext = telegramContext;
            this.dispatcher = dispatcher;

            //this.mqContext = mqContext;
            //mqConsumer = CreateEventConsumer();
            //mqConsumer.Received += OnMqMessageReceived;
        }

        public void AddControllers<I>()
        {
            dispatcher.LoadInterface<I>();
        }

        private EventingBasicConsumer CreateEventConsumer()
        {
            return new EventingBasicConsumer(mqContext.Channel);
        }

        private void OnMqMessageReceived(object model, BasicDeliverEventArgs ea)
        {
            Console.WriteLine("Message Received from Mq");
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            var messageObj = JsonConvert.DeserializeObject<MqMessage>(message);

            var tasks = new List<Task>();

            foreach (long receiverId in messageObj.Receivers)
            {
                tasks.Add(telegramContext.SendTextMessage(messageObj.Content, receiverId));
            }

            Task.WaitAll(tasks.ToArray());
        }

        public void Run()
        {
            var tasks = new List<Task>();
            //tasks.Add(Task.Run(() => mqContext.Channel.BasicConsume(queue: configuration.GetSection("Mq:Key").Value, autoAck: true, consumer: mqConsumer)));
            tasks.Add(Task.Run(() =>
            {
                telegramContext.TelegramBotClient.StartReceiving();
                Thread.Sleep(int.MaxValue);
            }));


            Task.WaitAll(tasks.ToArray());
        }
    }
}
