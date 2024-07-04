// See https://aka.ms/new-console-template for more information
using DemoServer;
using Google.Protobuf;
using GrpcDotNetNamedPipes;
using Nevermore.Grpc.Ipc;
using System.Diagnostics;

Console.WriteLine("GrpcNamedPipeServer is starting...");
var pipeName = Guid.NewGuid().ToString();
var server = new NamedPipeServer(pipeName);
BeepService.BindService(server.ServiceBinder, new BeepGrpcService());
server.Error += Server_Error;

void Server_Error(object? sender, NamedPipeErrorEventArgs e)
{
    if (e.Error is ObjectDisposedException)
    {
        //ignore
        return;
    }
    if (e.Error is InvalidProtocolBufferException rawDataException)
    {
        Console.WriteLine($"{nameof(InvalidProtocolBufferException)}:{rawDataException.Message}");
        return;
    }
    Console.WriteLine($"NamedPipeServerError:{e.Error}");
}

server.Start();

Console.WriteLine("GrpcNamedPipeServer is running!");
Console.WriteLine("Start client...");
try
{
    var client = Process.Start("DemoClient.exe", pipeName);
    client.EnableRaisingEvents = true;
    client.Exited += Client_Exited;
    Console.WriteLine($"DemoClient pid is {client.Id}!");
}
catch(Exception ex)
{
    Console.WriteLine($"Fail to start DemoClient.Please try to restart manually!");
    Console.WriteLine(ex);
}

void Client_Exited(object? sender, EventArgs e)
{
    if (sender is Process process)
    {
        Console.WriteLine("DemoClient has exited...");
    }
}