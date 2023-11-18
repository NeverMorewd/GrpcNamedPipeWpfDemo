// See https://aka.ms/new-console-template for more information
using DemoServer;
using GrpcDotNetNamedPipes;
using Nevermore.Grpc.Ipc;

Console.WriteLine("starting...");
var server = new NamedPipeServer("53189515-EE63-427A-870D-F3D11BD96F36");
BeepService.BindService(server.ServiceBinder, new BeepGrpcService());
server.Start();

// BindService is internal to Grpc.Core.Api so you'll need reflection to do this call
Console.WriteLine("NamedPipeServer is running");
