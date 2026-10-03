using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Text;
using TcpP43.Common;
using TcpP43.Common.Models;

namespace TcpP43.Server.Models
{
    public class ServerCore
    {
        private Dictionary<string, string> users = new Dictionary<string, string>();
        private Dictionary<string, List<string>> messages = new Dictionary<string, List<string>>();
        public async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                var commandType = await NetworkHelper.ReceiveStringAsync(stream);
                Console.WriteLine($"Received command: {commandType}");
                switch (commandType)
                {
                    case CommandType.Register:
                        await HandleRegisterCommandAsync(stream);
                        break;
                    case CommandType.Login:
                        await HandleLoginCommandAsync(stream);
                        break;
                    case CommandType.ListUsers:
                        await HandleListUsersCommandAsync(stream);
                        break;
                    case CommandType.SendMessage:
                        await HandleSendMessageCommandAsync(stream);
                        break;
                    case CommandType.GetMessages:
                        await HandleGetMessagesCommandAsync(stream);
                        break;
                    default:
                        break;
                }
                client.Close();
            }
            catch (SocketException)
            {
                Console.WriteLine("Client connection error.");
            }
            catch (IOException)
            {
                Console.WriteLine("Client connection error.");
            }
        }
        private async Task HandleListUsersCommandAsync(NetworkStream stream)
        {
            var response = new UserListResponse
            {
                Users = users.Keys.ToList()
            };
            await NetworkHelper.SendObjectAsync(stream, response);
        }
        private async Task HandleRegisterCommandAsync(NetworkStream stream)
        {
            var registerRequest = await NetworkHelper.ReceiveObjectAsync<RegisterRequest>(stream);
            Console.WriteLine($"Received register request: Login={registerRequest.Login}, Password={registerRequest.Password}");
            var registerResponse = new RegisterResponse();
            if (users.ContainsKey(registerRequest.Login))
            {
                registerResponse.IsRegistered = false;
                registerResponse.Message = "Login already exists.";
            }
            else
            {
                users.Add(registerRequest.Login, registerRequest.Password);
                registerResponse.IsRegistered = true;
                registerResponse.Message = "Registration successful.";
            }
            await NetworkHelper.SendObjectAsync(stream, registerResponse);
        }
        private async Task HandleLoginCommandAsync(NetworkStream stream)
        {
            var authRequest = await NetworkHelper.ReceiveObjectAsync<AuthRequest>(stream);
            Console.WriteLine($"Received login request: Login={authRequest.Login}, Password={authRequest.Password}");
            var authResponse = new AuthResponse();
            if (!users.ContainsKey(authRequest.Login))
            {
                authResponse.IsAuthenticated = false;
                authResponse.Message = "Invalid login.";
            }
            else if (users[authRequest.Login] != authRequest.Password)
            {
                authResponse.IsAuthenticated = false;
                authResponse.Message = "Invalid password.";
            }
            else
            {
                authResponse.IsAuthenticated = true;
                authResponse.Message = "Authentication successful.";
            }
            await NetworkHelper.SendObjectAsync(stream, authResponse);
        }
        private async Task HandleSendMessageCommandAsync(NetworkStream stream)
        {
            var request = await NetworkHelper.ReceiveObjectAsync<SendMessageRequest>(stream);
            var response = new SendMessageResponse();
            if (!users.ContainsKey(request.To))
            {
                response.IsSent = false;
                response.Message = "User not found.";
            }
            else if (request.From == request.To)
            {
                response.IsSent = false;
                response.Message = "You cannot send a message to yourself.";
            }
            else
            {
                if (!messages.ContainsKey(request.To))
                {
                    messages.Add(request.To, new List<string>());
                }
                messages[request.To].Add(request.From + ": " + request.Message);
                response.IsSent = true;
                response.Message = "Message sent.";
            }
            await NetworkHelper.SendObjectAsync(stream, response);
        }
        private async Task HandleGetMessagesCommandAsync(NetworkStream stream)
        {
            var login = await NetworkHelper.ReceiveStringAsync(stream);
            var response = new GetMessagesResponse();
            if (messages.ContainsKey(login))
            {
                response.Messages = messages[login];
                messages[login] = new List<string>();
            }
            else
            {
                response.Messages = new List<string>();
            }
            await NetworkHelper.SendObjectAsync(stream, response);
        }
    }
}
