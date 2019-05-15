using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            
            var graphQlClient = new GraphQLHttpClient(Environment.GetEnvironmentVariable("API_URL"));
            //Console.WriteLine("GraphQLHttpClient started");
            
            // Continuously send ratings to gateway
            while (true)
            {
                var i = 0;
                foreach (var channel in server.Channels)
                {
                    // channel.Id
                    var cid = channel.Id;
                    // channel.PositiveRatings
                    var posRatings = channel.PositiveRatings;
                    // channel.NegativeRatings
                    var negRatings = channel.NegativeRatings;

                    var updateRequest = new GraphQLRequest(){
                        Query = "mutation BroadcastRatingsUpdate($id:ID!, $broadcast:BroadcastUpdateInputType!){ broadcasts { update(id: $id, broadcast: $broadcast) { id positiveRatings negativeRatings } } }",
                        OperationName = "BroadcastRatingsUpdate",
                        Variables = new {
                            id = cid,
                            broadcast = new {
                                positiveRatings = posRatings,
                                negativeRatings = negRatings
                            }
                        }
                    };

                    try
                    {
                        var response = await graphQlClient.SendMutationAsync(updateRequest);
                    }
                    catch (GraphQL.Client.Http.GraphQLHttpException ex)
                    {
                        Log.Error(ex, "BroadcastRatingsUpdate error.");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "BroadcastCommunication: Unhandled exception.");
                    }
                    Log.Error($"BroadcastRatingsUpdate: Id: {cid}, Pos: {posRatings}, Neg: {negRatings}");

                    i++;
                }
                Log.Error($"BroadcastCommunication: {i} ratings updated. {DateTime.Now.ToString()}");
                Thread.Sleep(10000);
            }
        }
    }
}
