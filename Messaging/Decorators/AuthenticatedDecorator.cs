namespace ChatServer.Messaging.Decorators
{
    using System;
    using System.Threading.Tasks;

    using ChatProtos.Networking;

    using ChatServer.Messaging.Commands;

    using Google.Protobuf;

    using HServer.Networking;

    /// <summary>
    /// The decorator for authenticated users.
    /// </summary>
    public class AuthenticatedDecorator : ServerCommandDecorator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedDecorator"/> class.
        /// </summary>
        /// <param name="command">
        /// The chat server command implementation.
        /// </param>
        public AuthenticatedDecorator(IChatServerCommand command)
            : base(command)
        {
        }

        /// <summary>
        /// The execute task async.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task ExecuteTaskAsync(HChatClient client, RequestMessage message)
        {
            if (!client.Authenticated)
            {
                return client.SendResponseTaskAsync(
                    ResponseStatus.Unauthorized,
                    ByteString.Empty,
                    message);
            }
            return base.ExecuteTaskAsync(client, message);
        }
    }
}