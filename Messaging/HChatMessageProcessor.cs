using System;
using System.Threading.Tasks;
using ChatProtos.Networking;
using ChatServer.Messaging.Commands;
using HServer;
using HServer.HMessaging;
using HServer.Networking;

namespace ChatServer.Messaging
{
    public class HChatMessageProcessor : IMessageProcessor
    {
        private readonly ICommandRegistry<IChatServerCommand> _commandRegistry;
        private readonly HClientManager _clientManager;

        public HChatMessageProcessor(ICommandRegistry<IChatServerCommand> commandRegistry, HClientManager clientManager)
        {
            _commandRegistry = commandRegistry;
            _clientManager = clientManager;
        }

        public async Task ProcessMessageTask(HConnection connection, byte[] message)
        {
            var requestMessage = RequestMessage.Parser.ParseFrom(message);
            var client = await _clientManager.GetClient(connection) ?? new HChatClient(connection);
            var command = await _commandRegistry.GetCommand(new HCommandIdentifier((int)requestMessage.Type));
            Console.WriteLine("Server processing command {0}", command?.ToString());
            if (command != null)
            {
                try
                {
                    await command.ExecuteTask(client, requestMessage);
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine("Command not implemented.");
                }
            }
        }
    }
}