using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fleck;
using WebSocketServer = BroadcastCommunication.Sockets.WebSocketServer;
using GraphQL.Client;
using GraphQL.Client.Http;
using GraphQL.Common.Request;
using Serilog;


namespace BroadcastCommunication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new WebSocketServer("ws://0.0.0.0:4040")
            {
                RestartAfterListenError = true
            };
            server.Start();
            
            // Continuously send ratings to gateway
            var graphQlClient = new GraphQLHttpClient(Environment.GetEnvironmentVariable("API_URL"));
            while (true)
            {
                foreach (var channel in server.Channels)
                {
                    var updateRequest = new GraphQLRequest(){
                        Query = "mutation BroadcastRatingsUpdate($id:ID!, $broadcast:BroadcastUpdateInputType!){ broadcasts { update(id: $id, broadcast: $broadcast) { id positiveRatings negativeRatings } } }",
                        OperationName = "BroadcastRatingsUpdate",
                        Variables = new {
                            id = channel.Id,
                            broadcast = new {
                                positiveRatings = channel.PositiveRatings,
                                negativeRatings = channel.NegativeRatings
                            }
                        }
                    };

                    try
                    {
                        var response = await graphQlClient.SendMutationAsync(updateRequest);
                    }
                    catch (Exception ex)
                    {
                        FleckLog.Error($"BroadcastRatingsUpdate error: {ex}");
                    }
                }
                
                Thread.Sleep(10000);
            }
        }
    }
}
