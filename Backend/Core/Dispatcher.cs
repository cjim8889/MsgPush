using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelePush.Backend.Context;
using TelePush.Backend.Model;
using TelePush.Backend.Utility;

namespace TelePush.Backend.Core
{
    class Method
    {
        public bool IsCommand { get; set; }
        public string Command { get; set; }
        public DispatcherType Type { get; set; }
        public MethodInfo MethodInfo { get;  set; }
    }
    class Dispatcher
    {
        private IList<Method> methods;
        public delegate void MessageEventHandler(Message message);

        private readonly TelegramContext telegramContext;
        public Dispatcher(TelegramContext telegramContext)
        {
            this.telegramContext = telegramContext;
            this.telegramContext.OnMessage += new MessageEventHandler(Dispatch);
            methods = new List<Method>();
        }

        //Always dispatch to the first method that fits
        //Controllers are singleton
        //Pattern matching to be implemented
        public void Dispatch(Message message)
        {
            var (method, parameters) = SelectProcedure(message);
            if (method == null)
            {
                return;
            }

            var paramsLength = method.MethodInfo.GetParameters().Length;

            switch (paramsLength)
            {
                //0 parameter
                case 0:
                    parameters = new object[0];
                    break;
                //1 parameter which defaults to type Message
                case 1:
                    parameters = new object[] { parameters[0] };
                    break;
            }
            

            var targetControllerType = method.MethodInfo.DeclaringType;

            var target = Factory.GetInstance(targetControllerType);

            var response = (AbstractResponse)method.MethodInfo.Invoke(target, parameters);

            //Return if the resposne type if void
            if (response.GetType() == typeof(void))
            {
                return;
            }



            //Auto append chatid if null
            response.ChatId = response.ChatId == 0 ? message.Chat.Id : response.ChatId;


            Task.Run(() => { response.SendResponse(telegramContext); });
        }

        private (Method, object[]) SelectProcedure(Message message)
        {
            var type = (DispatcherType)message.Type;

            if (type == DispatcherType.Text)
            {
                if (message.Text.StartsWith('/'))
                {
                    var messageBody = message.Text.Substring(1);
                    var messageSplit = messageBody.Split(' ', 2);
                    object[] commandParams = new string[0];
                    if (messageSplit.Length > 1)
                    {
                        commandParams = messageSplit[1].Split(' ');
                    }

                    return (methods.Where(m => m.Command == messageSplit[0]).FirstOrDefault(), new object[] { message, commandParams });
                }
            }

            return (methods.Where(m => (m.Type == type || m.Type == DispatcherType.Any) && !m.IsCommand).FirstOrDefault(), new[] { message });
        }
        public void LoadInterface<I>()
        {
            var typeNames = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(I).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .Select(x => x.FullName).ToList();

            foreach (var typeName in typeNames)
            {
                var type = Type.GetType(typeName);

                Factory.AddDependency(type);
                Loader.LoadToList(methods, type);
            }
        }
    }
}
