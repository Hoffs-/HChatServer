namespace ChatServer.Messaging.Commands
{
    using System.Linq;
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The delete community command.
    /// </summary>
    public class DeleteCommunityCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        [NotNull]
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCommunityCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public DeleteCommunityCommand([NotNull] HCommunityManager communityManager)
        {
            _communityManager = communityManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(DeleteCommunityRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                await client.SendResponseTaskAsync(ResponseStatus.Error, ByteString.Empty, message)
                    .ConfigureAwait(false);
                return;
            }

            var response = new DeleteCommunityResponse
                               {
                                   CommunityId = request.CommunityId
                               }.ToByteString();

            var community = _communityManager.GetItem(request.CommunityId);
            if (community == null)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.NotFound,
                    RequestType.DeleteChannel,
                    response,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var clients = community.GetItems();
            var tasks = clients.Select(chatClient => client.SendResponseTaskAsync(ResponseStatus.Success, RequestType.DeleteCommunity, response));
            await Task.WhenAll(tasks).ConfigureAwait(false);
            _communityManager.RemoveItem(community);
        }
    }
}