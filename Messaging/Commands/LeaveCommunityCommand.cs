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
    /// The leave community command.
    /// </summary>
    public class LeaveCommunityCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaveCommunityCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public LeaveCommunityCommand([NotNull] HCommunityManager communityManager)
        {
            _communityManager = communityManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(LeaveCommunityRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Error,
                    RequestType.LeaveCommunity,
                    ByteString.Empty,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var response = new LeaveCommunityResponse { CommunityId = request.CommunityId }.ToByteString();

            var community = _communityManager.GetItem(request.CommunityId);
            if (community == null)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.NotFound,
                    RequestType.LeaveCommunity,
                    response,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var didRemove = await community.RemoveItemTask(client).ConfigureAwait(false);
            if (!didRemove)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Error,
                    RequestType.LeaveCommunity,
                    response,
                    message.Nonce).ConfigureAwait(false);
            }

            var clients = community.GetItems();
            var tasks = clients.Select(
                chatClient => chatClient.SendResponseTaskAsync(
                    ResponseStatus.Success,
                    RequestType.LeaveCommunity,
                    response));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}