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
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                // TODO: Send response
                return;
            }

            var parsed = ProtobufHelper.TryParse(DeleteCommunityRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                // TODO: Send response
                return;
            }

            var community = await _communityManager.GetItemTask(request.CommunityId).ConfigureAwait(false);
            if (community == null)
            {
                // TODO: Send response
                return;
            }

            await _communityManager.RemoveItemTask(community).ConfigureAwait(false);
            var clients = await community.GetItemsTask().ConfigureAwait(false);
            
            var response = new DeleteCommunityResponse
            {
                CommunityId = community.Id.ToString()
            }.ToByteString();
            var tasks = clients.Select(chatClient => client.SendResponseTask(ResponseStatus.Success, RequestType.DeleteCommunity, response));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}