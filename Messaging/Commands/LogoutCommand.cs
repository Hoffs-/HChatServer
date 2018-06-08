using Google.Protobuf;

namespace ChatServer.Messaging.Commands
{
    using System;
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using HServer;
    using HServer.Networking;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The Logout server command.
    /// </summary>
    public class LogoutCommand : IChatServerCommand
    {
        /// <summary>
        /// The Server client manager.
        /// </summary>
        [NotNull]
        private readonly HClientManager _clientManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutCommand"/> class.
        /// </summary>
        /// <param name="clientManager">
        /// The Server client manager.
        /// </param>
        public LogoutCommand([NotNull] HClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        /// <inheritdoc />
        public async Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            var parsed = ProtobufHelper.TryParse(LogoutRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                await client.SendResponseTaskAsync(ResponseStatus.Error, RequestType.Logout, ByteString.Empty, message.Nonce).ConfigureAwait(false);
                return;
            }

            await _clientManager.RemoveItemTask(client).ConfigureAwait(false);
            var response = new LogoutResponse
            {
                UserId = client.Id.ToString(),
            }.ToByteString();
            await client.SendResponseTaskAsync(ResponseStatus.Success, RequestType.Logout, response, message.Nonce).ConfigureAwait(false);
            await client.TryDeauthenticatingTask().ConfigureAwait(false);
            Console.WriteLine("[SERVER] After logout for client {0}", client.Id);
        }
    }
}