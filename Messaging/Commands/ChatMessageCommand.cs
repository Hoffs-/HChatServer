using System.Text;

namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Linq;
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
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(ChatMessageRequest.Parser, message.Message, out var request);
            if (!parsed || !Guid.TryParse(request.ChannelId, out var _))
            {
                await client.SendResponseTaskAsync(ResponseStatus.BadRequest, ByteString.Empty, message).ConfigureAwait(false);
                return;
            }

            var chatMessage = request.Message;
            var channel = client.GetChannels().FirstOrDefault(userChannel => userChannel.Id == Guid.Parse(request.ChannelId));
            if (channel?.HasItem(client.Id) != true)
            {
                Console.WriteLine("[SERVER] User tried sending in channel that he isnt in");
                await client.SendResponseTaskAsync(ResponseStatus.NotFound, ByteString.Empty, message)
                    .ConfigureAwait(false);
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