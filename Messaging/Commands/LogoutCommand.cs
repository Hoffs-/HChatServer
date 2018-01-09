using System;
using System.Threading.Tasks;
using ChatProtos.Networking;
using ChatProtos.Networking.Messages;
using CoreServer;
using CoreServer.HMessaging;

namespace ChatServer.Messaging.Commands
{
    public class LogoutServerCommand : IChatServerCommand
    {
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            var logoutRequest = LogoutMessageRequest.Parser.ParseFrom(message.Message);
            await client.TryDeauthenticatingTask();
            Console.WriteLine("[SERVER] After logout for client {0}", client.Id);
        }
    }
}