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
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(ChannelInfoRequest.Parser, message.Message, out var infoRequest);
            if (!parsed || !client.Authenticated)
            {
                Console.WriteLine("[SERVER] Couldn't parse protobuf or client not authenticated");
                return;
            }

            var channel = await _communityManager.TryGetChannelTask(infoRequest.ChannelId);
            if (channel == null)
            {
                Console.WriteLine("[SERVER] Channel not found");
                return;
            }


            var response = new ChannelInfoResponse
            {
                ChannelId = infoRequest.ChannelId,
                Channel = new Channel
                {
                    Id = channel.Id.ToString(),
                    Name = channel.Name
                },
                ChannelInfo = new ChannelInfo
                {
                    Created = channel.Created.ToString(CultureInfo.InvariantCulture),
                    Users = { channel.GetUserItems() }
                }
            };
            await client.SendResponseTask(ResponseStatus.Success, RequestType.ChannelInfo, response.ToByteString()).ConfigureAwait(false);
        }
    }
}