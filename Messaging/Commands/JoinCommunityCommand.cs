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
    /// The join community command.
    /// </summary>
    public class JoinCommunityCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        [NotNull]
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinCommunityCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public JoinCommunityCommand([NotNull] HCommunityManager communityManager)
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
                    ByteString.Empty,
                    message).ConfigureAwait(false);
                return;
            }

            var parsed = ProtobufHelper.TryParse(JoinCommunityRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Error,
                    ByteString.Empty,
                    message).ConfigureAwait(false);
                return;
            }

            var response = new JoinCommunityResponse
            {
                CommunityId = request.CommunityId,
            }.ToByteString();

            var community = _communityManager.GetItem(request.CommunityId);
            if (community == null)
            {
                await client.SendResponseTaskAsync(ResponseStatus.NotFound, response, message)
                    .ConfigureAwait(false);
                return;
            }

            // TODO: Check community password
            var didJoin = community.AddItem(client);
            if (!didJoin)
            {
                await client.SendResponseTaskAsync(ResponseStatus.Forbidden, response, message)
                    .ConfigureAwait(false);
                return;
            }

            var didAdd = client.AddCommunity(community);
            if (didAdd)
            {
                await client.SendResponseTaskAsync(ResponseStatus.Success, response, message).ConfigureAwait(false);
            }
            else
            {
                await community.RemoveItemTask(client).ConfigureAwait(false);
                await client.SendResponseTaskAsync(ResponseStatus.Error, response, message)
                    .ConfigureAwait(false);
            }
        }
    }
}