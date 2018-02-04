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
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                // TODO: Send response
                return;
            }

            var parsed = ProtobufHelper.TryParse(DeleteChannelRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                // TODO: Send reponse
                return;
            }

            var channel = await _communityManager.TryGetChannelTask(request.ChannelId).ConfigureAwait(false);
            if (channel == null)
            {
                // TODO: Send response
                return;
            }

            var community = channel.Community;
            await community.ChannelManager.RemoveItemTask(channel).ConfigureAwait(false);

            var response = new DeleteChannelResponse
            {
                ChannelId = channel.Id.ToString()
            }.ToByteString();

            await community.SendToAllTask(ResponseStatus.Success, RequestType.DeleteCommunity, response).ConfigureAwait(false);
        }
    }
}