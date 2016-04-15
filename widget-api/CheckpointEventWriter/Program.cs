using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace CheckpointEventWriter
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = SetUpEventStoreConnection("EventStoreConnection");
            conn.AppendToStreamAsync("CheckpointRequests", ExpectedVersion.Any,
                new EventData(Guid.NewGuid(), "CheckpointRequested", false, new byte[0], new byte[0])).Wait();

        }

        private static IEventStoreConnection SetUpEventStoreConnection(string connectionStringName)
        {
            var eventStoreConnectionString =
                new Uri(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);
            var settings =
                ConnectionSettings.Create().KeepReconnecting().LimitRetriesForOperationTo(10);
            var eventStoreClient = EventStoreConnection.Create(settings, eventStoreConnectionString);
            eventStoreClient.ConnectAsync().Wait();

            return eventStoreClient;
        }
    }
}
