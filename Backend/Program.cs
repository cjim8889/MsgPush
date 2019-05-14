using System.IO;
using Microsoft.Extensions.Configuration;
using TelePush.Backend.Context;
using TelePush.Backend.Core;
using TelePush.Backend.Utility;

namespace TelePush.Backend
{
    class Program
    {
        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            Factory.AddDependency<IConfiguration>(configuration);
            Factory.AddDependency<TelegramContext>();
            Factory.AddDependency<Dispatcher>();
            //Factory.AddDependency<MqContext>();

            var server = Factory.InstantiateServer();

            server.Run();

        }
    }
}
