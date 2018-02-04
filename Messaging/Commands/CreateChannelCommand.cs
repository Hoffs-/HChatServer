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
    /// The create channel command.
    /// </summary>
    public class CreateChannelCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        [NotNull]
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateChannelCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public CreateChannelCommand([NotNull] HCommunityManager communityManager)
        {
            _communityManager = communityManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(CreateChannelRequest.Parser, message.Message, out var request);
            if (!parsed || !client.Authenticated)
            {
                // TODO: Send response
                return;
            }

            var community = await _communityManager.GetItemTask(request.CommunityId);
            
            if (community == null || !community.HasClient(client.Id)) 
            {
                // TODO: Also check for permission
                // TODO: Send reponse
                return;
            }

            var channel = new HChannel(Guid.NewGuid(), request.ChannelName, community, new ConcurrentDictionary<Guid, HChatClient>(), DateTime.UtcNow);
            await channel.AddItemTask(client).ConfigureAwait(false);
            await community.ChannelManager.AddItemTask(channel).ConfigureAwait(false);
            
            var response = new CreateChannelResponse
            {
                ChannelId = channel.Id.ToString(),
                ChannelName = channel.Name
            }.ToByteString();

            await community.SendToAllTask(ResponseStatus.Created, RequestType.CreateChannel, response).ConfigureAwait(false);
        }
    }
}