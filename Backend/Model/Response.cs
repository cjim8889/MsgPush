using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelePush.Backend.Context;

namespace TelePush.Backend.Model
{

    interface IResponse
    {
        Task SendResponse(TelegramContext telegramContext);
    }

    abstract class AbstractResponse : IResponse
    {
        public abstract Task SendResponse(TelegramContext telegramContext);
        public abstract long ChatId { get; set; }

        public AbstractResponse()
        {
            ChatId = 0;
        }
    }


    class ResponseFactory
    {
        public static TextResponse NewTextResponse(string response, long chatId)
        {
            return new TextResponse() { ResponseText = response, ChatId = chatId };
        }

        public static TextResponse NewTextResponse(string response)
        {
            return new TextResponse() { ResponseText = response };
        }

        public static PhotoResponse NewPhotoResponse(string id)
        {
            return new PhotoResponse() { PhotoIdentifier = id };
        }
    }

    class TextResponse : AbstractResponse
    {
        public string ResponseText { get; set; }
        public override long ChatId { get; set; }

        public override Task SendResponse(TelegramContext telegramContext)
        {
            return telegramContext.SendTextMessage(ResponseText, ChatId);
        }
    }

    class PhotoResponse : AbstractResponse
    {

        public override long ChatId { get; set; }
        public string PhotoIdentifier { get; set; }

        public override Task SendResponse(TelegramContext telegramContext)
        {
            return telegramContext.SendPhotoMessage(PhotoIdentifier, ChatId);
        }
    }
}
