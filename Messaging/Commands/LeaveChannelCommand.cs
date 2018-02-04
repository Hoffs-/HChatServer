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
    /// Command called on LeaveChannel message.
    /// </summary>
    public class LeaveChannelServerCommand : IChatServerCommand
    {
        /// <summary>
        /// The Community channel manager.
        /// </summary>
        [NotNull]
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaveChannelServerCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The Community channel manager.
        /// </param>
        public LeaveChannelServerCommand([NotNull] HCommunityManager communityManager)
        {
            _communityManager = communityManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            var leaveRequest = LeaveChannelRequest.Parser.ParseFrom(message.Message);
            if (!client.Authenticated)
            {
                Console.WriteLine("[SERVER] User {0} not authenticated to perform this action.", client.Id);
                await SendReponseTask(client, ResponseStatus.Unauthorized, leaveRequest.ChannelId).ConfigureAwait(false);
                return;
            }

            var channel = await _communityManager.TryGetChannelTask(leaveRequest.ChannelId).ConfigureAwait(false);

            if (channel == null)
            {
                Console.WriteLine("[SERVER] User {0} tried leaving non-existing channel.", client.Id);
                await SendReponseTask(client, ResponseStatus.Error, leaveRequest.ChannelId).ConfigureAwait(false);
                return;
            }

            if (!channel.HasItem(client.Id))
            {
                // TODO: Send response.
                return;
            }

            if (await channel.RemoveItemTask(client).ConfigureAwait(false))
            {
                Console.WriteLine("[SERVER] User {0} left channel {1}.", client.Id, channel.Id);
                client.RemoveChannel(channel);
                await SendReponseTask(client, ResponseStatus.Success, leaveRequest.ChannelId).ConfigureAwait(false);
            }

            Console.WriteLine("[SERVER] User {0} tried leaving channel {1}.", client.Id, leaveRequest.ChannelId);
        }

        /// <summary>
        /// Sends response to client.
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
        private static async Task SendReponseTask([NotNull] HChatClient client, ResponseStatus status, [CanBeNull] string id)
        {
            var leaveResponse = new LeaveChannelResponse { ChannelId = id }.ToByteString();
            await client.SendResponseTask(status, RequestType.LeaveChannel, leaveResponse).ConfigureAwait(false);
        }
    }
}