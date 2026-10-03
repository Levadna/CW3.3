using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using TcpP43.Client.Models;
using TcpP43.Common;

var serverIp = "192.168.1.110";
var serverPort = 5000;
var client = new Client(IPAddress.Parse(serverIp), serverPort);

Console.WriteLine("Client");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Register");
    Console.WriteLine("2. Login");
    Console.WriteLine("3. Exit");
    Console.Write("Choose: ");
    var choice = Console.ReadLine();
    if (choice == "1")
    {
        Console.Write("Login: ");
        var login = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();
        try
        {
            var response = await client.RegisterAsync(login, password);
            Console.WriteLine(response.Message);
            if (response.IsRegistered)
            {
                Console.WriteLine();
                Console.WriteLine("Now login.");
                var loginResponse = await client.AuthenticateAsync(login, password);
                Console.WriteLine(loginResponse.Message);
            }
        }
        catch (SocketException)
        {
            Console.WriteLine("Server connection error.");
        }
        catch (IOException)
        {
            Console.WriteLine("Server connection error.");
        }
    }
    else if (choice == "2")
    {
        Console.Write("Login: ");
        var login = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        var response = await client.AuthenticateAsync(login, password);
        Console.WriteLine(response.Message);
        if (response.IsAuthenticated)
        {
            bool chatActive = true;
            var pollingTask = Task.Run(async () =>
            {
                while (chatActive)
                {
                    await Task.Delay(3000);
                    try
                    {
                        var messages = await client.GetMessagesAsync(login);
                        foreach (var messageItem in messages.Messages)
                        {
                            Console.WriteLine();
                            Console.WriteLine("New message: " + messageItem);
                        }
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Server connection error.");
                    }
                    catch (IOException)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Server connection error.");
                    }
                }
            });
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== CHAT ===");
                Console.WriteLine("1. Show users");
                Console.WriteLine("2. Send message");
                Console.WriteLine("3. Logout");
                Console.Write("Choose: ");
                var chatChoice = Console.ReadLine();
                if (chatChoice == "1")
                {
                    try
                    {
                        var users = await client.GetUsersAsync();
                        Console.WriteLine("Users:");
                        foreach (var user in users.Users)
                        {
                            Console.WriteLine(user);
                        }
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Server connection error.");
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Server connection error.");
                    }
                }
                else if (chatChoice == "2")
                {
                    Console.Write("To: ");
                    var to = Console.ReadLine();
                    Console.Write("Message: ");
                    var message = Console.ReadLine();
                    try
                    {
                        var sendResponse = await client.SendMessageAsync(login, to, message);
                        Console.WriteLine(sendResponse.Message);
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Server connection error.");
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Server connection error.");
                    }
                }
                else if (chatChoice == "3")
                {
                    chatActive = false;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid choice.");
                }
            } await pollingTask;
        }
    }
    else if (choice == "3")
    {
        break;
    }
    else
    {
        Console.WriteLine("Invalid choice.");
    }
}