namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    /// <inheritdoc />
    /// <summary>
    /// The create channel command.
    /// </summary>
    public class CreateChannelCommand : IChatServerCommand
    {
        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(CreateChannelRequest.Parser, message.Message, out var request);
            if (!parsed || !Guid.TryParse(request.CommunityId, out _))
            {
                await client.SendResponseTaskAsync(ResponseStatus.BadRequest, ByteString.Empty, message)
                    .ConfigureAwait(false);
                return;
            }

            var community = client.GetCommunities()
                .FirstOrDefault(clientCommunity => clientCommunity.Id == Guid.Parse(request.CommunityId));
            if (community == null || !community.HasClient(client.Id))
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.NotFound,
                    RequestType.CreateChannel,
                    ByteString.Empty,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var channel = new HChannel(
                Guid.NewGuid(),
                request.ChannelName,
                community,
                new ConcurrentDictionary<Guid, HChatClient>(),
                DateTime.UtcNow);
            if (!community.ChannelManager.AddItem(channel) || !channel.AddItem(client))
            {
                await client.SendResponseTaskAsync(ResponseStatus.Error, ByteString.Empty, message)
                    .ConfigureAwait(false);
                return;
            }

            var response = new CreateChannelResponse { ChannelId = channel.Id.ToString(), ChannelName = channel.Name }
                .ToByteString();
            await community.SendToAllTask(ResponseStatus.Created, RequestType.CreateChannel, response)
                .ConfigureAwait(false);
        }
    }
}