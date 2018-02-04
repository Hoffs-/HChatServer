// ReSharper disable StyleCop.SA1634
// ReSharper disable StyleCop.SA1600
// ReSharper disable StyleCop.SA1600
namespace ChatServer
{
    using System;
    using System.Collections.Concurrent;

    using ChatProtos.Networking;

    using ChatServer.Messaging;
    using ChatServer.Messaging.Commands;

    using HServer;
    using HServer.HMessaging;

    using JetBrains.Annotations;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var registry = new HCommandRegistry<IChatServerCommand>();
            var clientManager = new HClientManager(new ConcurrentDictionary<HConnection, HChatClient>());
            var channelManager = new HCommunityManager(new ConcurrentDictionary<Guid, HCommunity>());
            var processor = new HChatMessageProcessor(registry, clientManager);
            var server = new HChatServer(4000, processor, registry, clientManager, null);
            RegisterDefaultCommands(registry, channelManager, clientManager);
            
            Console.WriteLine();
            server.Run();
            
        }

        private static void RegisterDefaultCommands(
            [NotNull] ICommandRegistry<IChatServerCommand> registry,
            [NotNull] HCommunityManager communityManager,
            [NotNull] HClientManager clientManager)
        {
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.Login), new LoginServerCommand(clientManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.Logout), new LogoutCommand(clientManager));
            
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.CreateChannel), new CreateChannelCommand(communityManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.DeleteChannel), new DeleteChannelCommand(communityManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.JoinChannel), new JoinChannelServerCommand(communityManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.LeaveChannel), new LeaveChannelServerCommand(communityManager));

            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.CreateCommunity), new CreateCommunityCommand(communityManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.DeleteCommunity), new DeleteCommunityCommand(communityManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.JoinCommunity), new JoinCommunityCommand(communityManager));
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.LeaveCommunity), new LeaveCommunityCommand(communityManager));

            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.AddRole), new AddRoleServerCommand());
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.RemoveRole), new RemoveRoleCommand());
            
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.BanUser), new BanUserServerCommand());
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.KickUser), new KickUserCommand(communityManager));
            
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.UserInfo), new UserInfoServerCommand());
            
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.UpdateDisplayName), new UpdateDisplayNameCommand());
            
            registry.RegisterCommand(new HCommandIdentifier((int)RequestType.ChatMessage), new ChatMessageServerCommand(communityManager));
        }
    }
}