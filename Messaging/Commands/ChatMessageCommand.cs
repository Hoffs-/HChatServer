namespace ChatServer.Messaging.Commands
{
    using System;
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
    /// Command executed for ChatMessage
    /// </summary>
    public class ChatMessageServerCommand : IChatServerCommand
    {
        /// <summary>
        /// The server community manager.
        /// </summary>
        [NotNull]
        private readonly HCommunityManager _communityManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessageServerCommand"/> class.
        /// </summary>
        /// <param name="communityManager">
        /// The server community manager.
        /// </param>
        public ChatMessageServerCommand([NotNull] HCommunityManager communityManager)
        {
            _communityManager = communityManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(ChatMessageRequest.Parser, message.Message, out var request);
            if (!parsed || !client.Authenticated)
            {
                // TODO: Send response
                return;
            }

            var chatMessage = request.Message;

            var channel = await _communityManager.TryGetChannelTask(request.ChannelId).ConfigureAwait(false);

            if (channel?.HasItem(client.Id) != true)
            {
                // TODO: Send response
                Console.WriteLine("[SERVER] User tried sending in channel that he isnt in");
                return;
            }

            Console.WriteLine("[SERVER] Sending message to channel {0}", channel.Id);
            
            var chatResponse = new ChatMessageResponse
            {
                Message = new ChatMessage
                {
                    AuthorId = chatMessage.AuthorId,
                    Id = chatMessage.Id,
                    Text = chatMessage.Text,
                    Timestamp = chatMessage.Timestamp
                }
            }.ToByteString();

            await channel.SendToAllTask(ResponseStatus.Success, RequestType.ChatMessage, chatResponse).ConfigureAwait(false);
        }
    }
}