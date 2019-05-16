using Backend.Service;
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
    class Server
    {
        //Can not have multiple dependencies with the same type signature (at the moment);

        private readonly IConfiguration configuration;
        private readonly MqContext mqContext;
        private readonly TelegramContext telegramContext;
        private readonly EventingBasicConsumer mqConsumer;
        private readonly Dispatcher dispatcher;
        private readonly HookService hookService;

        public Server(IConfiguration configuration, TelegramContext telegramContext, Dispatcher dispatcher, HookService hookService)
        {
            this.configuration = configuration;
            this.telegramContext = telegramContext;
            this.dispatcher = dispatcher;
            this.hookService = hookService;

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

        private async Task HandleMessagePush(BasicDeliverEventArgs ea)
        {
            var body = ea.Body;

            var message = Encoding.UTF8.GetString(body);
            var messageObj = JsonConvert.DeserializeObject<MqMessage>(message);

            var tasks = new List<Task<Telegram.Bot.Types.Message>>();

            foreach (long receiverId in messageObj.Receivers)
            {
                tasks.Add(telegramContext.SendTextMessage(messageObj.Content, receiverId));
            }

            await Task.WhenAll(tasks.ToArray());

            if (!string.IsNullOrWhiteSpace(messageObj.Hook))
            {
                foreach (var task in tasks)
                {
                    var m = await task;
                    hookService.AddHook(m.MessageId, messageObj.Hook);
                }
            }
        }

        private async void OnMqMessageReceived(object model, BasicDeliverEventArgs ea)
        {
            Console.WriteLine("Message Received from Mq");
            
            switch(ea.BasicProperties.Type)
            {
                case MqMessageType.Message:
                    await HandleMessagePush(ea);
                    break;
            }
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
