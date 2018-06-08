namespace ChatServer.Messaging.Commands
{
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The delete channel command.
    /// </summary>
    public class DeleteChannelCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteChannelCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public DeleteChannelCommand([NotNull] HCommunityManager communityManager)
        {
            _communityManager = communityManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Unauthorized,
                    RequestType.DeleteChannel,
                    ByteString.Empty,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var parsed = ProtobufHelper.TryParse(DeleteChannelRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Error,
                    RequestType.DeleteChannel,
                    ByteString.Empty,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var response = new DeleteChannelResponse { ChannelId = request.ChannelId }.ToByteString();

            var channel = _communityManager.TryGetChannel(request.ChannelId);
            if (channel == null)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.NotFound,
                    RequestType.DeleteChannel,
                    response,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var community = channel.Community;
            var removed = community.ChannelManager.RemoveItem(channel);

            if (!removed)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Error,
                    RequestType.DeleteChannel,
                    response,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            await community.SendToAllTask(ResponseStatus.Success, RequestType.DeleteCommunity, response)
                .ConfigureAwait(false);
        }
    }
}