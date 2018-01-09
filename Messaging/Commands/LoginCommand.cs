using System;
using System.Threading.Tasks;
using ChatProtos.Networking;
using ChatProtos.Networking.Messages;
using CoreServer;
using CoreServer.HMessaging;
using Google.Protobuf;

namespace ChatServer.Messaging.Commands
{
    public class LoginServerCommand : IChatServerCommand
    {
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (client.Authenticated)
            {
                await SendErrorMessageTask(client);
                return;
            }

            var loginRequest = LoginMessageRequest.Parser.ParseFrom(message.Message);
            var result = await client.TryAuthenticatingTask(loginRequest.Username, loginRequest.Password, loginRequest.Token);
            Console.WriteLine("[SERVER] After login for client {0}: {1} {2}", client.Id, result.Item1, result.Item2);
            if (client.Authenticated)
            {
                await SendSuccessMessageTask(client);
            }
            else
            {
                await SendErrorMessageTask(client);
            }
        }

        private static async Task SendErrorMessageTask(HChatClient client)
        {
            await client.Connection.SendAyncTask(new ResponseMessage
            {
                Status = ResponseStatus.Error,
                Type = RequestType.Login
            }.ToByteArray());
        }

        private static async Task SendSuccessMessageTask(HChatClient client)
        {
            await client.Connection.SendAyncTask(new ResponseMessage
            {
                Status = ResponseStatus.Success,
                Type = RequestType.Login,
                Message = new LoginMessageResponse
                {
                    Token = client.Token,
                    UserId = client.Id
                }.ToByteString()
            }.ToByteArray());
        }
    }
}