namespace ChatServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ChatProtos.Data;
    using ChatProtos.Networking;

    using Google.Protobuf;

    using HServer.Networking;

    using JetBrains.Annotations;

    /// <summary>
    /// The community class.
    /// </summary>
    public class HCommunity
    {
        /// <summary>
        /// The community clients.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, HChatClient> _clients; // Clients

        /// <summary>
        /// Initializes a new instance of the <see cref="HCommunity"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="clients">
        /// The clients.
        /// </param>
        public HCommunity(Guid id, [NotNull] string name, [NotNull] ConcurrentDictionary<Guid, HChatClient> clients)
        {
            ChannelManager = new HChannelManager(new ConcurrentDictionary<Guid, HChannel>());
            Id = id;
            Name = name;
            _clients = clients;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HCommunity"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="channelManager">
        /// The channel manager.
        /// </param>
        /// <param name="clients">
        /// The clients.
        /// </param>
        public HCommunity(
            Guid id,
            [NotNull] string name,
            [NotNull] HChannelManager channelManager,
            [NotNull] ConcurrentDictionary<Guid, HChatClient> clients)
        {
            // Might be unused.
            ChannelManager = channelManager;
            Id = id;
            Name = name;
            _clients = clients;
        }

        /// <summary>
        /// Gets the channel manager.
        /// </summary>
        [NotNull]
        public HChannelManager ChannelManager { get; } // ChannelManager

        /// <summary>
        /// Gets the community id.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets or sets the community name.
        /// </summary>
        [NotNull]
        public string Name { get; set; }

        /// <summary>
        /// Adds client to the community.
        /// </summary>
        /// <param name="item">Client to be added</param>
        /// <returns>Boolean if client was added</returns>
        public bool AddItem([NotNull] HChatClient item)
        {
            return _clients.TryAdd(item.Id, item);
        }

        /// <summary>
        /// Removes client from the community and leaves every channel that the person has joined.
        /// </summary>
        /// <param name="item">
        /// Client to be removed
        /// </param>
        /// <returns>
        /// The <see cref="bool"/> if client was removed.
        /// </returns>
        public async Task<bool> RemoveItemTask([NotNull] HChatClient item)
        {
            await Task.Yield();
            if (!_clients.ContainsKey(item.Id))
            {
                return false;
            }
            var didRemove = ChannelManager.GetChannels().Select(channel => channel.RemoveItem(item));

            return didRemove.All(b => b) && _clients.TryRemove(item.Id, out var _);
        }

        /// <summary>
        /// Gets a client from the community.
        /// </summary>
        /// <param name="id">
        /// GUID of the client
        /// </param>
        /// <returns>
        /// The <see cref="HChatClient"/>.
        /// </returns>
        [CanBeNull]
        public HChatClient GetItem(Guid id)
        {
            _clients.TryGetValue(id, out var result);
            return result;
        }

        /// <summary>
        /// Checks whether community has a client with given id.
        /// </summary>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool HasClient(Guid clientId)
        {
            return _clients.ContainsKey(clientId);
        }

        /// <summary>
        /// Gets all clients of community.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public IEnumerable<HChatClient> GetItems()
        {
            return _clients.Values;
        }

        /// <summary>
        /// Sends a message to all clients in the community.
        /// </summary>
        /// <param name="status">
        /// The message status.
        /// </param>
        /// <param name="type">
        /// The message type.
        /// </param>
        /// <param name="message">
        /// The message byte string.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task SendToAllTask(ResponseStatus status, RequestType type, [CanBeNull] ByteString message)
        {
            var tasks = _clients.Values.Select(client => client.SendResponseTaskAsync(status, type, message));
            var enumerable = tasks as Task[] ?? tasks.ToArray();
            return enumerable.Length > 0 ? Task.WhenAll(enumerable) : Task.CompletedTask;
        }

        /// <summary>
        /// Gets a client from the community with string id.
        /// </summary>
        /// <param name="id">
        /// The string id.
        /// </param>
        /// <returns>
        /// The <see cref="HChatClient"/>.
        /// </returns>
        public HChatClient GetItem(string id)
        {
            var parsed = Guid.TryParse(id, out var guid);
            return !parsed ? null : GetItem(guid);
        }

        /// <summary>
        /// The get as protobuf.
        /// </summary>
        /// <returns>
        /// The <see cref="Community"/>.
        /// </returns>
        public Community GetAsProtobuf()
        {
            return new Community
                       {
                           Channels =
                               {
                                   ChannelManager.GetChannels()
                                       .Select(channel => channel.GetAsProto())
                               },
                           Id = Id.ToString(),
                           Name = Name,
                       };
        }
    }
}