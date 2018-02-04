namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The create community command.
    /// </summary>
    public class CreateCommunityCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        [NotNull]
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCommunityCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public CreateCommunityCommand([NotNull] HCommunityManager communityManager)
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
            var parsed = ProtobufHelper.TryParse(CreateCommunityRequest.Parser, message.Message, out var result);
            if (!parsed)
            {
                // TODO: Send response
                return;
            }

            var manager = new HChannelManager(new ConcurrentDictionary<Guid, HChannel>());
            var community = new HCommunity(Guid.NewGuid(), result.CommunityName, manager, new ConcurrentDictionary<Guid, HChatClient>());
            var channel = new HChannel(Guid.NewGuid(), "General", community, new ConcurrentDictionary<Guid, HChatClient>(), DateTime.Now);
            
            await community.ChannelManager.AddItemTask(channel).ConfigureAwait(false);
            await community.AddItemTask(client).ConfigureAwait(false);
            await _communityManager.AddItemTask(community).ConfigureAwait(false);

            var response = new CreateCommunityResponse
            {
                CommunityId = community.Id.ToString(),
                CommunityName = community.Name
            }.ToByteString();

            await client.SendResponseTask(ResponseStatus.Created, RequestType.CreateCommunity, response).ConfigureAwait(false);
        }
    }
}