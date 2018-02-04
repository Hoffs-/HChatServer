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
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                // TODO: Send response.
                return;
            }

            var parsed = ProtobufHelper.TryParse(JoinCommunityRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                // TODO: Send response.
                return;
            }

            var community = await _communityManager.GetItemTask(request.CommunityId);
            if (community == null)
            {
                // TOOD: Send response.
                return;
            }

            // TODO: Check community password
            var didJoin = await community.AddItemTask(client).ConfigureAwait(false);

            var response = new JoinCommunityResponse
            {
                CommunityId = community.Id.ToString()
            }.ToByteString();

            if (didJoin)
            {
                await client.SendResponseTask(ResponseStatus.Success, RequestType.JoinCommunity, response).ConfigureAwait(false);
            }
            else
            {
                await client.SendResponseTask(ResponseStatus.Forbidden, RequestType.JoinCommunity, response)
                    .ConfigureAwait(false);
            }
        }
    }
}