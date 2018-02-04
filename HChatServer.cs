namespace ChatServer
{
    using ChatServer.Messaging.Commands;

    using HServer.HMessaging;

    /// <summary>
    /// The HChatServer class.
    /// </summary>
    public class HChatServer : HServer.HServer
    {
        public HCommunityManager CommunityManager { get; }
        public HClientManager ClientManager { get; }
        public ICommandRegistry<IChatServerCommand> CommandRegistry { get; }

        public HChatServer(int port, IMessageProcessor messageProcessor, ICommandRegistry<IChatServerCommand> registry, HClientManager clientManager, HCommunityManager communityManager) : base(port, messageProcessor)
        {
            CommandRegistry = registry;
            CommunityManager = communityManager;
            ClientManager = clientManager;
        }
    }
}