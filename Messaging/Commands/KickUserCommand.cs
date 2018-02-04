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
    /// The kick user command.
    /// </summary>
    public class KickUserCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        [NotNull]
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="KickUserCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public KickUserCommand([NotNull] HCommunityManager communityManager)
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

            var parsed = ProtobufHelper.TryParse(KickUserRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                // TODO: Send response.
                return;
            }

            switch (request.Scope)
            {
                case ActionScope.Channel:
                    await RemoveFromChannel(client, request.UserId, request.ChannelId, request.Reason).ConfigureAwait(false);
                    break;
                case ActionScope.Community:
                    await RemoveFromCommunity(client, request.UserId, request.CommunityId, request.Reason).ConfigureAwait(false);
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Remove client from channel.
        /// </summary>
        /// <param name="sender">
        /// The sender client.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <param name="channelId">
        /// The channel id.
        /// </param>
        /// <param name="reason">
        /// The kick reason.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task RemoveFromChannel([NotNull] HChatClient sender, [CanBeNull] string clientId, [CanBeNull] string channelId, [CanBeNull] string reason)
        {
            var channel = await _communityManager.TryGetChannelTask(channelId).ConfigureAwait(false);
            if (channel == null)
            {
                // TODO: Send response.
                return;
            }

            // TODO: Check permission of client who is sending
            var client = await channel.GetItemTask(clientId).ConfigureAwait(false);
            if (client == null)
            {
                // TODO: Send response.
                return;
            }

            var response = new KickUserResponse
            {
                ChannelId = channel.Id.ToString(),
                Reason = reason,
                Scope = ActionScope.Channel,
                UserId = client.Id.ToString()
            }.ToByteString();

            if (await channel.RemoveItemTask(client).ConfigureAwait(false))
            {
                await channel.SendToAllTask(ResponseStatus.Success, RequestType.KickUser, response).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Removes client from community.
        /// </summary>
        /// <param name="sender">
        /// The sender client.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <param name="communityId">
        /// The community id.
        /// </param>
        /// <param name="reason">
        /// The reason.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task RemoveFromCommunity([NotNull] HChatClient sender, [CanBeNull] string clientId, [CanBeNull] string communityId, [CanBeNull] string reason)
        {
            var community = await _communityManager.GetItemTask(communityId).ConfigureAwait(false);
            if (community == null)
            {
                // TODO: Send response.
                return;
            }

            // TODO: Check permission of client who is sending
            var client = await community.GetItemTask(clientId).ConfigureAwait(false);
            if (client == null)
            {
                // TODO: Send response.
                return;
            }

            var response = new KickUserResponse
            {
                CommunityId = community.Id.ToString(),
                Reason = reason,
                Scope = ActionScope.Community,
                UserId = client.Id.ToString()
            }.ToByteString();

            if (await community.RemoveItemTask(client).ConfigureAwait(false))
            {
                await community.SendToAllTask(ResponseStatus.Success, RequestType.KickUser, response).ConfigureAwait(false);
            }
        }
    }
}