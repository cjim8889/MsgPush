using System;
using System.IO;
using System.Threading.Tasks;
using Backend.Service;
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
            Console.WriteLine("Configuration Loaded");

            Factory.AddDependency<HookService>();
            Factory.AddDependency<IConfiguration>(configuration);
            Factory.AddDependency<TelegramContext>();
            Factory.AddDependency<Dispatcher>();
            //Factory.AddDependency<MqContext>();
            Console.WriteLine("Denpendencies injected");

            var server = Factory.InstantiateServer();
            Console.WriteLine("Server instance instantiated\nRunning...");
            server.Run();

        }
    }
}
