// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using ProtoBuf.Grpc.Client;
using Shared.Contracts;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GrpcGreeterClientCodeFirst
{

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using (var channel = GrpcChannel.ForAddress("https://localhost:7214", new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebHandler(new HttpClientHandler())
            }))
            {
                var client = channel.CreateGrpcService<IGreeterService>();

                // Normal request
                var reply = await client.SayHelloAsync(
                    new HelloRequest { Name = "GreeterClient" });
                Console.WriteLine($"Greeting: {reply.Message}");
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}