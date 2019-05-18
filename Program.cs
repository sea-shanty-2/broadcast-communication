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
            
            while(true) {
                
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
                        FleckLog.Info($"Broadcast {channel.Id} rating updated");
                    }
                    catch (Exception ex)
                    {
                        FleckLog.Error($"Broadcast rating update error: {ex}");
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }
}
