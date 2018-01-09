using System;
using System.Threading.Tasks;
using ChatProtos.Networking;
using ChatProtos.Networking.Messages;
using CoreServer;
using CoreServer.HMessaging;

namespace ChatServer.Messaging.Commands
{
    public class JoinChannelServerCommand : IChatServerCommand
    {
        private readonly HChannelManager _channelManager;

        public JoinChannelServerCommand(HChannelManager channelManager)
        {
            _channelManager = channelManager;
        }

        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            var joinRequest = JoinChannelMessageRequest.Parser.ParseFrom(message.Message);
            if (!client.Authenticated)
            {
                Console.WriteLine("[SERVER] User {0} not authenticated to perform this action.",
                    client.Id);
            }
            else
            {
                HChannel channel = null;
                if (joinRequest.ChannelId != null)
                {
                    channel = _channelManager.FindChannelById(joinRequest.ChannelId);
                } 
                if (joinRequest.ChannelName != null && channel == null)
                {
                    channel = _channelManager.FindChannelByName(joinRequest.ChannelName);
                }

                if (channel?.AddClient(client) == true)
                {
                    // client.AddChannel(channel); // TODO: Fix this after moving to Interfaced logic
                }

                Console.WriteLine("[SERVER] User {0} tried joining channel {1}.",
                    client.Id, joinRequest.ChannelName);
            }
        }
    }
}