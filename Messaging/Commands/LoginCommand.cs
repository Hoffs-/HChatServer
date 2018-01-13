using System;
using System.Threading.Tasks;
using Google.Protobuf;
using HServer.ChatProtos.Networking;
using HServer.ChatProtos.Networking.Messages;

namespace ChatServer.Messaging.Commands
{
    public class LoginServerCommand : IChatServerCommand
    {
        private readonly HClientManager _clientManager;

        public LoginServerCommand(HClientManager clientManager)
        {
            _clientManager = clientManager;
        }
        
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (client.Authenticated) // If client is already authenticated return error.
            {
                await SendErrorMessageTask(client);
                return;
            }

            var loginRequest = LoginMessageRequest.Parser.ParseFrom(message.Message);
            var result = await client.TryAuthenticatingTask(loginRequest.Username, loginRequest.Password, loginRequest.Token);
            Console.WriteLine("[SERVER] After login for client {0}: {1} {2}", client.Id, result.Item1, result.Item2);
            if (client.Authenticated)
            {
                // TODO: Set id/token/etc for client somewhere.
                _clientManager.AddClient(client.Connection, client);
                await SendSuccessMessageTask(client);
            }
            else
            {
                await SendErrorMessageTask(client);
            }
        }

        private static async Task SendSuccessMessageTask(HChatClient client)
        {
            var response = new ResponseMessage()
            {
                Status = ResponseStatus.Success,
                Type = RequestType.Login,
                Message = new LoginMessageResponse
                {
                    Token = client.Token,
                    UserId = client.Id
                }.ToByteString()
            };
            await client.Connection.SendAyncTask(response.ToByteArray());
        }

        private static async Task SendErrorMessageTask(HChatClient client)
        {
            await client.Connection.SendAyncTask(new ResponseMessage
            {
                Status = ResponseStatus.Error,
                Type = RequestType.Login
            }.ToByteArray());
        }

    }
}