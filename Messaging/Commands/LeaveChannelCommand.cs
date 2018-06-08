namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    /// <inheritdoc />
    /// <summary>
    /// Command called on LeaveChannel message.
    /// </summary>
    public class LeaveChannelServerCommand : IChatServerCommand
    {
        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            var didParse = ProtobufHelper.TryParse(LeaveChannelRequest.Parser, message.Message, out var request);
            if (!didParse || !Guid.TryParse(request.ChannelId, out _))
            {
                await client.SendResponseTaskAsync(ResponseStatus.BadRequest, ByteString.Empty, message)
                    .ConfigureAwait(false);
                return;
            }

            var response = new LeaveChannelResponse { ChannelId = request.ChannelId }.ToByteString();

            var channel = client.GetChannels()
                .FirstOrDefault(clientChannel => clientChannel.Id == Guid.Parse(request.ChannelId));
            if (channel == null)
            {
                Console.WriteLine("[SERVER] User {0} tried leaving non-existing channel.", client.Id);
                await client.SendResponseTaskAsync(ResponseStatus.NotFound, response, message).ConfigureAwait(false);
                return;
            }

            if (!channel.RemoveItem(client) || !client.RemoveChannel(channel))
            {
                Console.WriteLine("[SERVER] User {0} tried leaving channel {1}.", client.Id, request.ChannelId);
                await client.SendResponseTaskAsync(ResponseStatus.Error, response, message).ConfigureAwait(false);
            }

            await client.SendResponseTaskAsync(ResponseStatus.Success, response, message).ConfigureAwait(false);
            Console.WriteLine("[SERVER] User {0} left channel {1}.", client.Id, channel.Id);
        }
    }
}