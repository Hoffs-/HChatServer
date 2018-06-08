namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
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
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            var didParse = ProtobufHelper.TryParse(JoinChannelRequest.Parser, message.Message, out var request);
            if (!didParse)
            {
                await client.SendResponseTaskAsync(ResponseStatus.BadRequest, ByteString.Empty, message)
                    .ConfigureAwait(false);
            }

            var response = new JoinChannelResponse
                               {
                                   ChannelId = request.ChannelId,
                               }.ToByteString();
            
            var channel = _communityManager.TryGetChannel(request.ChannelId);
            if (channel == null)
            {
                Console.WriteLine("[SERVER] Community or channel not found");
                await client.SendResponseTaskAsync(ResponseStatus.NotFound, RequestType.JoinChannel, response)
                    .ConfigureAwait(false);
                return;
            }

            if (!channel.AddItem(client) || !client.AddChannel(channel))
            {
                // TODO: Check permissions or something
                // TODO: If user couldn't join the channel (permissions or something)
                Console.WriteLine("[SERVER] User {0} tried joining channel {1}.", client.Id, request.ChannelId);
                await client.SendResponseTaskAsync(ResponseStatus.Forbidden, response, message).ConfigureAwait(false);
                return;
            }

            Console.WriteLine("[SERVER] User {0} joined the channel {1}", client.GetDisplayName(), channel.Id);
            await client.SendResponseTaskAsync(ResponseStatus.Success, response, message).ConfigureAwait(false);
        }
    }
}