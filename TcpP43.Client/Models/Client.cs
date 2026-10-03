using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Text;
using TcpP43.Common;
using TcpP43.Common.Models;

namespace TcpP43.Client.Models
{
    public class Client
    {
        private readonly IPAddress _serverIp;
        private readonly int _serverPort;
        public Client(IPAddress serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }
        private async Task<TcpClient> ConnectAsync()
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(_serverIp, _serverPort);
            return client;
        }

        public async Task<RegisterResponse> RegisterAsync(string login, string password)
        {
            using TcpClient client = await ConnectAsync();
            using NetworkStream stream = client.GetStream();
            await NetworkHelper.SendStringAsync(stream, CommandType.Register);
            var registerRequest = new RegisterRequest { Login = login, Password = password };
            await NetworkHelper.SendObjectAsync(stream, registerRequest);
            var registerResponse  = await NetworkHelper.ReceiveObjectAsync<RegisterResponse>(stream);
            return registerResponse;
        }
        public async Task<AuthResponse> AuthenticateAsync(string login, string password)
        {
            using TcpClient client = await ConnectAsync();
            using NetworkStream stream = client.GetStream();
            await NetworkHelper.SendStringAsync(stream, CommandType.Login);
            var authRequest = new AuthRequest { Login = login, Password = password };
            await NetworkHelper.SendObjectAsync(stream, authRequest);
            var authResponse = await NetworkHelper.ReceiveObjectAsync<AuthResponse>(stream);
            return authResponse;
        }
        public async Task<UserListResponse> GetUsersAsync()
        {
            using TcpClient client = await ConnectAsync();
            using NetworkStream stream = client.GetStream();
            await NetworkHelper.SendStringAsync(stream, CommandType.ListUsers);
            var response = await NetworkHelper.ReceiveObjectAsync<UserListResponse>(stream);
            return response;
        }
        public async Task<SendMessageResponse> SendMessageAsync(string from, string to, string message)
        {
            using TcpClient client = await ConnectAsync();
            using NetworkStream stream = client.GetStream();
            await NetworkHelper.SendStringAsync(stream, CommandType.SendMessage);
            var request = new SendMessageRequest
            {
                From = from,
                To = to,
                Message = message
            };
            await NetworkHelper.SendObjectAsync(stream, request);
            var response = await NetworkHelper.ReceiveObjectAsync<SendMessageResponse>(stream);
            return response;
        }
        public async Task<GetMessagesResponse> GetMessagesAsync(string login)
        {
            using TcpClient client = await ConnectAsync();
            using NetworkStream stream = client.GetStream();
            await NetworkHelper.SendStringAsync(stream, CommandType.GetMessages);
            await NetworkHelper.SendStringAsync(stream, login);
            var response = await NetworkHelper.ReceiveObjectAsync<GetMessagesResponse>(stream);
            return response;
        }
    }
}
