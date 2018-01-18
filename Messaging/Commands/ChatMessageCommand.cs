using System;
using System.Threading.Tasks;
using ChatProtos.Data;
using ChatProtos.Networking;
using ChatProtos.Networking.Messages;
using Google.Protobuf;
using HServer.Networking;

namespace ChatServer.Messaging.Commands
{
    public class ChatMessageServerCommand : IChatServerCommand
    {
        private readonly HChannelManager _channelManager;

        public ChatMessageServerCommand(HChannelManager channelManager)
        {
            _channelManager = channelManager;
        }

        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            var request = ChatMessageRequest.Parser.ParseFrom(message.Message);
            if (request.ChannelId == null || request.Message == null) return;
            var chatMessage = request.Message;
            var channel = await _channelManager.FindChannelById(request.ChannelId);
            if (channel == null || !channel.HasClient(client)) return;

            Console.WriteLine("[SERVER] Sending message to channel {0}", channel.Guid);
            await channel.SendToAll(
                new ResponseMessage
                {
                    Type = (int)RequestType.ChatMessage,
                    Message = new ChatMessageResponse
                    {
                        Message = new ChatMessage
                        {
                            AuthorId = chatMessage.AuthorId,
                            Id = chatMessage.Id,
                            Text = chatMessage.Text,
                            Timestamp = chatMessage.Timestamp
                        }
                    }.ToByteString()
                });
        }
    }
}