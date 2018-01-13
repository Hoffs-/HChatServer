using System;
using System.Threading.Tasks;
using Google.Protobuf;
using HServer.ChatProtos.Networking;
using HServer.ChatProtos.Networking.Messages;

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
                await SendErrorMessageTask(client);
            }
            else
            {
                HChannel channel = null;
                if (joinRequest.ChannelId != null)
                {
                    channel = await _channelManager.FindChannelById(joinRequest.ChannelId);
                } 
                if (joinRequest.ChannelName != null && channel == null)
                {
                    channel = await _channelManager.FindChannelByName(joinRequest.ChannelName);
                }

                if (channel?.AddClient(client) == true)
                {
                    client.AddChannel(channel);
                }
                await SendSuccessMessageTask(client, joinRequest.ChannelId); // TODO: Change to send actual channel ID

                Console.WriteLine("[SERVER] User {0} tried joining channel {1}.",
                    client.Id, joinRequest.ChannelName);
            }
        }

        private static async Task SendSuccessMessageTask(HChatClient client, string channelId)
        {
            var response = new ResponseMessage
            {
                Status = ResponseStatus.Success,
                Type = RequestType.JoinChannel,
                Message = new JoinChannelMessageResponse
                {
                    ChannelId = channelId
                }.ToByteString()
            };
            await client.Connection.SendAyncTask(response.ToByteArray());
        }

        private static async Task SendErrorMessageTask(HChatClient client)
        {
            await client.Connection.SendAyncTask(new ResponseMessage
            {
                Status = ResponseStatus.Error,
                Type = RequestType.JoinChannel
            }.ToByteArray());
        }

    }
}