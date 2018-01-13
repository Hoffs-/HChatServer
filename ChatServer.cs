using ChatServer.Messaging;
using ChatServer.Messaging.Commands;
using HServer.ChatProtos.Networking;
using HServer.HMessaging;


namespace ChatServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var registry = new HCommandRegistry<IChatServerCommand>();
            var clientManager = new HClientManager();
            var channelManager = new HChannelManager();
            var processor = new HChatMessageProcessor(registry, clientManager);
            var server = new HChatServer(4000, processor, registry, clientManager, channelManager);
            RegisterDefaultCommands(registry, channelManager, clientManager);
            server.Run();
        }

        private static void RegisterDefaultCommands(HCommandRegistry<IChatServerCommand> registry,
            HChannelManager channelManager, HClientManager clientManager)
        {
            registry.RegisterCommand(new HCommandIdentifier(RequestType.Login), new LoginServerCommand(clientManager));
            registry.RegisterCommand(new HCommandIdentifier(RequestType.Logout), new LogoutServerCommand(clientManager));
            registry.RegisterCommand(new HCommandIdentifier(RequestType.JoinChannel), new JoinChannelServerCommand(channelManager));
            registry.RegisterCommand(new HCommandIdentifier(RequestType.LeaveChannel), new LeaveChannelServerCommand(channelManager));
            registry.RegisterCommand(new HCommandIdentifier(RequestType.AddRole), new AddRoleServerCommand());
            registry.RegisterCommand(new HCommandIdentifier(RequestType.RemoveRole), new RemoveRoleServerCommand());
            registry.RegisterCommand(new HCommandIdentifier(RequestType.BanUser), new BanUserServerCommand());
            registry.RegisterCommand(new HCommandIdentifier(RequestType.KickUser), new KickUserServerCommand());
            registry.RegisterCommand(new HCommandIdentifier(RequestType.UserInfo), new UserInfoServerCommand());
            registry.RegisterCommand(new HCommandIdentifier(RequestType.UpdateDisplayName), new UpdateDisplayNameServerCommand());
            registry.RegisterCommand(new HCommandIdentifier(RequestType.ChatMessage), new ChatMessageServerCommand(channelManager));
        }
    }
}