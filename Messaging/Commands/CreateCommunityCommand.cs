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
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Unauthorized,
                    RequestType.CreateCommunity,
                    ByteString.Empty,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var parsed = ProtobufHelper.TryParse(CreateCommunityRequest.Parser, message.Message, out var result);
            if (!parsed)
            {
                await client.SendResponseTaskAsync(
                    ResponseStatus.Error,
                    RequestType.CreateCommunity,
                    ByteString.Empty,
                    message.Nonce).ConfigureAwait(false);
                return;
            }

            var manager = new HChannelManager(new ConcurrentDictionary<Guid, HChannel>());
            var community = new HCommunity(
                Guid.NewGuid(),
                result.CommunityName,
                manager,
                new ConcurrentDictionary<Guid, HChatClient>());
            var channel = new HChannel(
                Guid.NewGuid(),
                "General",
                community,
                new ConcurrentDictionary<Guid, HChatClient>(),
                DateTime.Now);

            community.ChannelManager.AddItem(channel);
            community.AddItem(client);
            _communityManager.AddItem(community);

            var response =
                new CreateCommunityResponse { CommunityId = community.Id.ToString(), CommunityName = community.Name }
                    .ToByteString();

            await client.SendResponseTaskAsync(ResponseStatus.Created, RequestType.CreateCommunity, response)
                .ConfigureAwait(false);
        }
    }
}