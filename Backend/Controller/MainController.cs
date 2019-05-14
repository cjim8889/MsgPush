using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;
using TelePush.Backend.Attribute;
using TelePush.Backend.Context;
using TelePush.Backend.Model;

namespace TelePush.Backend.Controller
{
    interface IControllerBase
    {
    }
    class MainController : IControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly TelegramContext telegramContext;
        public MainController(TelegramContext telegramContext, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.telegramContext = telegramContext;
        }

        [Command("start")]
        public IResponse Start(Message message)
        {
            return ResponseFactory.NewTextResponse($"Hi, 这里是Telegram Push Service Bot\n你的ID是: {message.From.Id}\n注册服务请访问https://push.oxifus.com\n\n\n");
        }


        [Command("wocao")]
        public IResponse wocao()
        {
            return ResponseFactory.NewPhotoResponse("https://images.unsplash.com/photo-1557820178-20186da06935?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=927&q=80");
        }

        [Command("version")]
        public IResponse Version()
        {
            return ResponseFactory.NewTextResponse($"v{configuration.GetSection("Version").Value}");
        }

        [Type(Core.DispatcherType.Photo)]
        public IResponse Photo(Message message)
        {
            Console.WriteLine(message.Photo[0].FileId);
            return ResponseFactory.NewPhotoResponse(message.Photo[0].FileId);
        }
    }
}
