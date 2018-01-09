using CoreServer.HMessaging;


namespace ChatServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var registry = new HCommandRegistry();
            var processor = new HMessageProcessor(registry);
            var server = new HChatServer(4000, registry, processor);
            RegisterDefaultCommands(server);
            server.Run();
        }

        private static void RegisterDefaultCommands(HChatServer server)
        {
            /*
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.Login), new LoginServerCommand());
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.Logout), new LogoutServerCommand());
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.JoinChannel), new JoinChannelServerCommand(server.ChannelManager));
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.LeaveChannel), new LeaveChannelServerCommand(server.ChannelManager));
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.AddRole), new AddRoleServerCommand());
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.RemoveRole), new RemoveRoleServerCommand());
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.BanUser), new BanUserServerCommand());
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.KickUser), new KickUserServerCommand());
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.UserInfo), new UserInfoServerCommand());
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.UpdateDisplayName), new UpdateDisplayNameServerCommand());
            server.CommandRegistry.RegisterCommand(new HCommandIdentifier(RequestType.ChatMessage), new ChatMessageServerCommand(server.ChannelManager));
            */
        }
    }
}