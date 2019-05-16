using Backend.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;

namespace TelePush.Backend.Context
{
    class TelegramContext
    {
        public event Core.Dispatcher.MessageEventHandler OnMessage;
        public TelegramContext(IConfiguration configuration)
        {
            TelegramBotClient = new TelegramBotClient(configuration.GetSection("Telegram_Token").Value);
            TelegramBotClient.OnMessage += Bot_OnMessage;
        }

        ~TelegramContext()
        {
            TelegramBotClient.StopReceiving();
        }

        public ITelegramBotClient TelegramBotClient { get; }

        private void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            OnMessage(message);
        }

        public async Task SendPhotoMessage(string url, long chatId)
        {
            await TelegramBotClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: url
                );

            Console.WriteLine($"Photo Message Sent {chatId}");
        }


        public async Task<Telegram.Bot.Types.Message> SendTextMessage(string message, long chatId)
        {
            try
            {
                var m = await TelegramBotClient.SendTextMessageAsync(
                  chatId: chatId,
                  text: message
                );

                Console.WriteLine($"Text Message Sent {chatId}");

                return m;
            }
            catch(ChatNotFoundException e)
            {
                Console.WriteLine($"Fail to find the chatId {chatId} {e.Message}");

                return null;
            }           
        }
    }
}
