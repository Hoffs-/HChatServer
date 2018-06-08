namespace ChatServer.Messaging.Decorators
{
    using System.Threading.Tasks;

    using ChatServer.Messaging.Commands;

    using HServer.Networking;

    /// <summary>
    /// The server command decorator.
    /// </summary>
    public abstract class ServerCommandDecorator : IChatServerCommand
    {
        /// <summary>
        /// The chat server command implementation.
        /// </summary>
        private readonly IChatServerCommand command;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCommandDecorator"/> class.
        /// </summary>
        /// <param name="command">
        /// The chat server command implementation.
        /// </param>
        protected ServerCommandDecorator(IChatServerCommand command)
        {
            this.command = command;
        }

        /// <summary>
        /// The execute task.
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
            return command.ExecuteTaskAsync(client, message);
        }
    }
}