using CoreServer;
using CoreServer.HMessaging;

namespace ChatServer
{
    public class HChatServer : HServer
    {
        public HChannelManager ChannelManager { get; } = new HChannelManager();

        public HChatServer(int port, ICommandRegistry commandRegistry, IMessageProcessor messageProcessor) : base(port, commandRegistry, messageProcessor)
        {
        }
    }
}