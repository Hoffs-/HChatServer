namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The Join Channel server command.
    /// </summary>
    public class JoinChannelServerCommand : IChatServerCommand
    {
        /// <summary>
        /// The server Community Manager.
        /// </summary>
        [NotNull]
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinChannelServerCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server <see cref="HCommunityManager"/> instance.
        /// </param>
        public JoinChannelServerCommand([NotNull] HCommunityManager communityManager)
        {
            _communityManager = communityManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            var joinRequest = JoinChannelRequest.Parser.ParseFrom(message.Message);

            if (!client.Authenticated)
            {
                Console.WriteLine("[SERVER] User {0} not authenticated to perform this action.", client.Id);
                await SendResponse(client, ResponseStatus.Unauthorized, joinRequest.ChannelId).ConfigureAwait(false);
                return;
            }

            var channel = await _communityManager.TryGetChannelTask(joinRequest.ChannelId).ConfigureAwait(false);

            if (channel == null)
            {
                Console.WriteLine("[SERVER] Community or channel not found");
                await SendResponse(client, ResponseStatus.Error, joinRequest.ChannelId).ConfigureAwait(false);
                return;
            }

            if (await channel.AddItemTask(client).ConfigureAwait(false))
            {
                client.AddChannel(channel);
                await SendResponse(client, ResponseStatus.Success, joinRequest.ChannelId).ConfigureAwait(false);
                Console.WriteLine("[SERVER] User {0} joined the channel {1}", client.GetDisplayName(), channel.Id);
                return;
            }

            // TODO: If user couldn't join the channel (permissions or something)
            Console.WriteLine("[SERVER] User {0} tried joining channel {1}.", client.Id, joinRequest.ChannelId);
        }

        /// <summary>
        /// Sends a response to client.
        /// </summary>
        /// <param name="client">
        /// The receiving client.
        /// </param>
        /// <param name="status">
        /// The response status.
        /// </param>
        /// <param name="id">
        /// The channel id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private static async Task SendResponse([NotNull] HChatClient client, ResponseStatus status, [CanBeNull] string id)
        {
            var responseContents = new JoinChannelResponse { ChannelId = id }.ToByteString();
            await client.SendResponseTask(status, RequestType.JoinChannel, responseContents).ConfigureAwait(false);
        }
    }
}