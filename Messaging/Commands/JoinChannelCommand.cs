using System;
using System.Threading.Tasks;
using ChatProtos.Networking;
using ChatProtos.Networking.Messages;
using Google.Protobuf;
using HServer.Networking;

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
            var joinRequest = JoinChannelRequest.Parser.ParseFrom(message.Message);
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

                if (channel?.AddClient(client) == true)
                {
                    client.AddChannel(channel);
                }
                await SendSuccessMessageTask(client, joinRequest.ChannelId); // TODO: Change to send actual channel ID

                Console.WriteLine("[SERVER] User {0} tried joining channel {1}.",
                    client.Id, joinRequest.ChannelId);
            }
        }

        private static async Task SendSuccessMessageTask(HChatClient client, string channelId)
        {
            var response = new ResponseMessage
            {
                Status = ResponseStatus.Success,
                Type = (int)RequestType.JoinChannel,
                Message = new JoinChannelResponse
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
                Type = (int)RequestType.JoinChannel
            }.ToByteArray());
        }

    }
}