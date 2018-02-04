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
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                // TODO: Send response
                return;
            }

            var parsed = ProtobufHelper.TryParse(LeaveCommunityRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                // TODO: Send response
                return;
            }

            var community = await _communityManager.GetItemTask(request.CommunityId);
            if (community == null)
            {
                // TODO: Send response
                return;
            }

            await community.RemoveItemTask(client).ConfigureAwait(false);

            var response = new LeaveCommunityResponse { CommunityId = community.Id.ToString() }.ToByteString();

            var clients = await community.GetItemsTask().ConfigureAwait(false);
            var tasks = clients.Select(
                chatClient => chatClient.SendResponseTask(
                    ResponseStatus.Success,
                    RequestType.LeaveCommunity,
                    response));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}