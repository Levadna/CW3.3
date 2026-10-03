 using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using TcpP43.Common;
using TcpP43.Server.Models;

Console.WriteLine("Server");

int port = 5000;
IPAddress ipAddress = IPAddress.Parse("192.168.1.110");

TcpListener listener = new TcpListener(ipAddress, port);


listener.Start();
Console.WriteLine($"Server started on {ipAddress}:{port}");


//var serverCore = new ServerCore()
//{
//    FileRoot = @"D:\Презенатція\server"
//};
var serverCore = new ServerCore();

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
    _ = Task.Run(() => serverCore.HandleClientAsync(client));
}


//TcpListener listener = new TcpListener(ipAddress, port);

//listener.Start();
//Console.WriteLine($"Server started on {ipAddress}:{port}");

//while (true)
//{
//    TcpClient client = await listener.AcceptTcpClientAsync();
//    Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

//    _ = Task.Run(() => HandleClientAsync(client));
//}

//async Task HandleClientAsync(TcpClient client)
//{ 
//    using NetworkStream stream = client.GetStream();

//    var receivedInt = await NetworkHelper.ReceiveIntAsync(stream);
//    Console.WriteLine($"Received integer from client: {receivedInt}");
//    //using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
//    //using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

//    //string? message = await reader.ReadLineAsync();

//    //string response = $"Server received: {message}";
//    //writer.WriteLineAsync(message);

//    client.Close();
//}