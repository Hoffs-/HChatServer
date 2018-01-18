using System;
using ChatProtos.Networking;
using ChatServer.Messaging;
using ChatServer.Messaging.Commands;
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
            
            
            Console.WriteLine();
            server.Run();
            
        }

        private static void RegisterDefaultCommands(HCommandRegistry<IChatServerCommand> registry,
            HChannelManager channelManager, HClientManager clientManager)
        {
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.Login), new LoginServerCommand(clientManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.Logout), new LogoutServerCommand(clientManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.JoinChannel), new JoinChannelServerCommand(channelManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.LeaveChannel), new LeaveChannelServerCommand(channelManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.AddRole), new AddRoleServerCommand());
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.RemoveRole), new RemoveRoleServerCommand());
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.BanUser), new BanUserServerCommand());
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.KickUser), new KickUserServerCommand());
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.UserInfo), new UserInfoServerCommand());
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.UpdateDisplayName), new UpdateDisplayNameServerCommand());
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.ChatMessage), new ChatMessageServerCommand(channelManager));
        }
    }
}