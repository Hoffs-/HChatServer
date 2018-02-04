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
        public async Task ExecuteTask(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                // TODO: Send response.
                return;
            }

            var parsed = ProtobufHelper.TryParse(LogoutRequest.Parser, message.Message, out var request);
            if (!parsed)
            {
                // TODO: Send response.
                return;
            }

            await client.TryDeauthenticatingTask().ConfigureAwait(false);
            await _clientManager.RemoveItemTask(client).ConfigureAwait(false);
            Console.WriteLine("[SERVER] After logout for client {0}", client.Id);
        }
    }
}