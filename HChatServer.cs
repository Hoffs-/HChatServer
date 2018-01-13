using ChatServer.Messaging.Commands;
using HServer.HMessaging;

namespace ChatServer
{
    public class HChatServer : HServer.HServer
    {
        public HChannelManager ChannelManager { get; }
        public HClientManager ClientManager { get; }
        public ICommandRegistry<IChatServerCommand> CommandRegistry { get; }

        public HChatServer(int port, IMessageProcessor messageProcessor, ICommandRegistry<IChatServerCommand> registry, HClientManager clientManager, HChannelManager channelManager) : base(port, messageProcessor)
        {
            CommandRegistry = registry;
            ChannelManager = channelManager;
            ClientManager = clientManager;
        }
    }
}