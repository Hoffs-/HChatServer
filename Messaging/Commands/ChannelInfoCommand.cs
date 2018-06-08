namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    using ChatProtos.Data;
    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer;
    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The channel info command.
    /// </summary>
    public class ChannelInfoCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelInfoCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public ChannelInfoCommand([NotNull] HCommunityManager communityManager)
        {
            this._communityManager = communityManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(ChannelInfoRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                Console.WriteLine("[SERVER] Couldn't parse protobuf or client not authenticated");
                await client.SendResponseTaskAsync(ResponseStatus.Error, ByteString.Empty, message)
                    .ConfigureAwait(false);
                return;
            }

            var response = new ChannelInfoResponse
                               {
                                   ChannelId = request.ChannelId,
                               };

            var channel = _communityManager.TryGetChannel(request.ChannelId);
            if (channel == null)
            {
                Console.WriteLine("[SERVER] Channel not found");
                await client.SendResponseTaskAsync(ResponseStatus.Error, response.ToByteString(), message)
                    .ConfigureAwait(false);
                return;
            }

            response.Channel = channel.GetAsProto();
            response.ChannelInfo = channel.GetAsChannelInfoProto();

            await client.SendResponseTaskAsync(ResponseStatus.Success, RequestType.ChannelInfo, response.ToByteString()).ConfigureAwait(false);
        }
    }
}