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
    using ChatServer.Messaging.Decorators;

    using HServer;
    using HServer.HMessaging;

    using JetBrains.Annotations;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var registry = new HCommandRegistry<IChatServerCommand>();
            var clientManager = new HClientManager(new ConcurrentDictionary<HConnection, HChatClient>());
            var communityManager = new HCommunityManager(new ConcurrentDictionary<Guid, HCommunity>());
            AddTestCommunities(communityManager);
            var processor = new HChatMessageProcessor(registry, clientManager);
            var server = new HChatServer(int.Parse(args[0]), processor, registry, clientManager, null);
            RegisterDefaultCommands(registry, communityManager, clientManager);

            Console.WriteLine();
            server.Run();
        }

        private static void AddTestCommunities(HCommunityManager communityManager)
        {
            var community = new HCommunity(
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                "Wu-Tang",
                new ConcurrentDictionary<Guid, HChatClient>());

            var channel1 = new HChannel(
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                "Ol' Dirty Bastard",
                community,
                new ConcurrentDictionary<Guid, HChatClient>(),
                new DateTime());
            var channel2 = new HChannel(
                Guid.Parse("00000000-0000-0000-0000-000000000003"),
                "Ghostface Killah",
                community,
                new ConcurrentDictionary<Guid, HChatClient>(),
                new DateTime());

            community.ChannelManager.AddItem(channel1);
            community.ChannelManager.AddItem(channel2);
            communityManager.AddItem(community);
        }

        private static void RegisterDefaultCommands(
            [NotNull] ICommandRegistry<IChatServerCommand> registry,
            [NotNull] HCommunityManager communityManager,
            [NotNull] HClientManager clientManager)
        {
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.Login),
                new LoginServerCommand(clientManager));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.Logout),
                new AuthenticatedDecorator(new LogoutCommand(clientManager)));

            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.CreateChannel),
                new AuthenticatedDecorator(new CreateChannelCommand()));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.DeleteChannel),
                new AuthenticatedDecorator(new DeleteChannelCommand(communityManager)));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.JoinChannel),
                new AuthenticatedDecorator(new JoinChannelServerCommand(communityManager)));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.LeaveChannel),
                new AuthenticatedDecorator(new LeaveChannelServerCommand()));

            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.CreateCommunity),
                new AuthenticatedDecorator(new CreateCommunityCommand(communityManager)));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.DeleteCommunity),
                new AuthenticatedDecorator(new DeleteCommunityCommand(communityManager)));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.JoinCommunity),
                new AuthenticatedDecorator(new JoinCommunityCommand(communityManager)));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.LeaveCommunity),
                new AuthenticatedDecorator(new LeaveCommunityCommand(communityManager)));

            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.AddRole),
                new AuthenticatedDecorator(new AddRoleServerCommand()));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.RemoveRole),
                new AuthenticatedDecorator(new RemoveRoleCommand()));

            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.BanUser),
                new AuthenticatedDecorator(new BanUserServerCommand()));
            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.KickUser),
                new AuthenticatedDecorator(new KickUserCommand(communityManager)));

            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.UserInfo),
                new AuthenticatedDecorator(new UserInfoServerCommand()));

            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.UpdateDisplayName),
                new AuthenticatedDecorator(new UpdateDisplayNameCommand()));

            registry.RegisterCommand(
                new HCommandIdentifier((int)RequestType.ChatMessage),
                new AuthenticatedDecorator(new ChatMessageServerCommand(communityManager)));
        }
    }
}