using System;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using WebSocketServer = BroadcastCommunication.Sockets.WebSocketServer;
using GraphQL.Client.Http;
using GraphQL.Common.Request;

namespace BroadcastCommunication
{
    class Program
    {
        private const string Query = 
        @"mutation BroadcastRatingsUpdate(
        $id: ID!
        $broadcast: BroadcastUpdateInputType!
        $activity: Boolean
        ) {
        broadcasts {
            update(id: $id, activity: $activity, broadcast: $broadcast) {
            id
            positiveRatings
            negativeRatings
            }
        }
        }";

        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://0.0.0.0:4040")
            {
                RestartAfterListenError = true
            };

            server.Start();

            // Send ratings to gateway every 10 seconds
            var apiClient = new GraphQLHttpClient(Environment.GetEnvironmentVariable("API_URL"));
            
            var timer = new Timer((object stateInfo) =>
            {
                foreach (var channel in server.Channels)
                {
                    var updateRequest = new GraphQLRequest()
                    {
                        Query = Query,
                        OperationName = "BroadcastRatingsUpdate",
                        Variables = new
                        {
                            id = channel.Id,
                            activity = false,
                            broadcast = new
                            {
                                positiveRatings = channel.PositiveRatings,
                                negativeRatings = channel.NegativeRatings
                            }
                        }
                    };

                    try
                    {
                        apiClient.SendMutationAsync(updateRequest).Wait();
                    }
                    catch (Exception ex)
                    {
                        FleckLog.Error($"BroadcastRatingsUpdate error: {ex}");
                    }
                }
            }, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));

            // Keep the service running
            while(true) {
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }
}
