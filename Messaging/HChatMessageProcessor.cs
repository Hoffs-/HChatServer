namespace ChatServer.Messaging
{
    using System;
    using System.Threading.Tasks;

    using ChatServer.Messaging.Commands;

    using Google.Protobuf;

    using HServer;
    using HServer.HMessaging;
    using HServer.Networking;

    using JetBrains.Annotations;

    /// <summary>
    /// MessageProcessor for HChat messages.
    /// </summary>
    public class HChatMessageProcessor : IMessageProcessor
    {
        /// <summary>
        /// Chat Server Command registry.
        /// </summary>
        [NotNull]
        private readonly ICommandRegistry<IChatServerCommand> _commandRegistry;

        /// <summary>
        /// Instance of server ClientManager.
        /// </summary>
        [NotNull]
        private readonly HClientManager _clientManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HChatMessageProcessor"/> class.
        /// </summary>
        /// <param name="commandRegistry">
        /// The command registry.
        /// </param>
        /// <param name="clientManager">
        /// The client manager.
        /// </param>
        public HChatMessageProcessor([NotNull] ICommandRegistry<IChatServerCommand> commandRegistry, [NotNull] HClientManager clientManager)
        {
            _commandRegistry = commandRegistry;
            _clientManager = clientManager;
        }

        /// <summary>
        /// The message processing task.
        /// </summary>
        /// <param name="connection">
        /// Client connection.
        /// </param>
        /// <param name="message">
        /// Received message.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        public async Task ProcessMessageTask(HConnection connection, byte[] message)
        {
            try
            {
                var requestMessage = RequestMessage.Parser.ParseFrom(message);
                var client = await _clientManager.GetItemTask(connection).ConfigureAwait(false) ?? new HChatClient(connection, DateTime.Now);
                var command = await _commandRegistry.GetCommand(new HCommandIdentifier((int)requestMessage.Type)).ConfigureAwait(false);
                Console.WriteLine("[SERVER] Processing command {0}", command?.ToString());
                if (command != null)
                {
                    try
                    {
                        await command.ExecuteTask(client, requestMessage).ConfigureAwait(false);
                    }
                    catch (NotImplementedException)
                    {
                        Console.WriteLine("[SERVER] Command not implemented.");
                    }
                }
            }
            catch (InvalidProtocolBufferException e)
            {
                Console.WriteLine("Invalid protobuf");
            }
        }
    }
}