namespace ChatServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
        /// <param name="channelManager">
        /// The channel manager.
        /// </param>
        /// <param name="clients">
        /// The clients.
        /// </param>
        public HCommunity(Guid id, [NotNull] string name, [NotNull] HChannelManager channelManager, [NotNull] ConcurrentDictionary<Guid, HChatClient> clients)
        {
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
        public async Task<bool> AddItemTask([NotNull] HChatClient item)
        {
            await Task.Yield();
            return _clients.TryAdd(item.Id, item);
        }

        /// <summary>
        /// Removes client from the community and leaves every channel that the person has joined.
        /// </summary>
        /// <param name="item">Client to be removed</param>
        /// <returns>Boolean if client was removed</returns>
        public async Task<bool> RemoveItemTask([NotNull] HChatClient item)
        {
            await Task.Yield();
            if (!_clients.ContainsKey(item.Id))
            {
                return false;
            }
            var items = await ChannelManager.GetChannelsTask().ConfigureAwait(false);
            var tasks = items.Select(channel => channel.RemoveItemTask(item));
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return _clients.TryRemove(item.Id, out var _);
        }

        /// <summary>
        /// Gets a client from the community.
        /// </summary>
        /// <param name="id">GUID of the client</param>
        /// <returns>HChatClient or null if client wasn't found</returns>
        [ItemCanBeNull]
        public async Task<HChatClient> GetItemTask(Guid id)
        {
            await Task.Yield();
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
        public async Task<IEnumerable<HChatClient>> GetItemsTask()
        {
            await Task.Yield();
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
        public async Task SendToAllTask(ResponseStatus status, RequestType type, [CanBeNull] ByteString message)
        {
            var tasks = _clients.Values.Select(client => client.SendResponseTask(status, type, message));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a client from the community with string id.
        /// </summary>
        /// <param name="id">
        /// The string id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<HChatClient> GetItemTask(string id)
        {
            var parsed = Guid.TryParse(id, out var guid);
            if (!parsed)
            {
                return null;
            }
            return await GetItemTask(guid).ConfigureAwait(false);
        }
    }
}