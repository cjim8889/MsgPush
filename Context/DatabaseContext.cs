using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace MsgPush.Context
{
    public class DatabaseContext
    {
        public DatabaseContext(IConfiguration configuration)
        {
            Client = new MongoClient(configuration.GetSection("Mongo:ConnectionString").Value);
            Database = Client.GetDatabase(configuration.GetSection("Mongo:Database").Value);
        }



        public IMongoDatabase Database { get; }
        public MongoClient Client { get; }


    }
}
