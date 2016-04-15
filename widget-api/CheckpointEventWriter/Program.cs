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
            eventStoreClient.Closed += EventStoreClientOnClosed;
            eventStoreClient.ErrorOccurred += EventStoreClientOnErrorOccurred;
            eventStoreClient.Connected += EventStoreClientOnConnected;
            eventStoreClient.Disconnected += EventStoreClientOnDisconnected;
            eventStoreClient.Reconnecting += EventStoreClientOnReconnecting;
            eventStoreClient.ConnectAsync().Wait();

            return eventStoreClient;
        }

        private static void EventStoreClientOnReconnecting(object sender, ClientReconnectingEventArgs clientReconnectingEventArgs)
        {
            Console.WriteLine("Event Store reconnecting");
        }

        private static void EventStoreClientOnClosed(object sender, ClientClosedEventArgs clientClosedEventArgs)
        {
            Console.WriteLine("Event Store connection closed");
        }

        private static void EventStoreClientOnDisconnected(object sender, ClientConnectionEventArgs clientConnectionEventArgs)
        {
            Console.WriteLine(
                $"Event Store disconnected from endpoint {clientConnectionEventArgs.RemoteEndPoint.Address}:{clientConnectionEventArgs.RemoteEndPoint.Port}");
        }

        private static void EventStoreClientOnConnected(object sender, ClientConnectionEventArgs clientConnectionEventArgs)
        {
            Console.WriteLine(
                $"Event Store connected {clientConnectionEventArgs.RemoteEndPoint.Address}:{clientConnectionEventArgs.RemoteEndPoint.Port}");
        }

        private static void EventStoreClientOnErrorOccurred(object sender, ClientErrorEventArgs clientErrorEventArgs)
        {
            if (clientErrorEventArgs.Exception != null)
            {
                Console.WriteLine("Event Store error occurred: {0}", clientErrorEventArgs.Exception.Message);
            }
            else
            {
                Console.WriteLine("Unknown Event Store error");
            }
        }
    }
}
