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
    /// The server Channel class.
    /// </summary>
    public class HChannel
    {
        /// <summary>
        /// The community channel belongs.
        /// </summary>
        [NotNull]
        public readonly HCommunity Community;

        /// <summary>
        /// The clients of the channel.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, HChatClient> _clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="HChannel"/> class.
        /// </summary>
        /// <param name="id">
        /// Channel Id.
        /// </param>
        /// <param name="name">
        /// Channel name.
        /// </param>
        /// <param name="parent">
        /// Community channel belongs to.
        /// </param>
        /// <param name="clients">
        /// The channel clients.
        /// </param>
        /// <param name="created">
        /// The channel creation date time.
        /// </param>
        public HChannel(Guid id, [NotNull] string name, [NotNull] HCommunity parent, [NotNull] ConcurrentDictionary<Guid, HChatClient> clients, DateTime created)
        {
            Id = id;
            Name = name;
            Community = parent;
            _clients = clients;
            Created = created;
        }

        /// <summary>
        /// Gets the Id of the Channel.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets or sets the name of the Channel.
        /// </summary>
        [NotNull]
        public string Name { get; set; }

        /// <summary>
        /// Gets the created date time.
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// Adds client to the community.
        /// </summary>
        /// <param name="item">
        /// The client.
        /// </param>
        /// <returns>
        /// Boolean and <see cref="Task"/>.
        /// </returns>
        public async Task<bool> AddItemTask([NotNull] HChatClient item)
        {
            // TODO: Maybe add permission checking
            await Task.Yield();
            return _clients.TryAdd(item.Id, item);
        }

        /// <summary>
        /// Removes client from the community.
        /// </summary>
        /// <param name="item">
        /// The client.
        /// </param>
        /// <returns>
        /// Boolean and <see cref="Task"/>.
        /// </returns>
        public async Task<bool> RemoveItemTask([NotNull] HChatClient item)
        {
            await Task.Yield();
            return _clients.TryRemove(item.Id, out var _);
        }

        /// <summary>
        /// Gets client with given id.
        /// </summary>
        /// <param name="id">
        /// The client id.
        /// </param>
        /// <returns>
        /// HChatClient <see cref="Task"/>.
        /// </returns>
        [ItemCanBeNull]
        public async Task<HChatClient> GetItemTask(Guid id)
        {
            await Task.Yield();
            _clients.TryGetValue(id, out var result);
            return result;
        }

        /// <summary>
        /// Gets client with given string id.
        /// </summary>
        /// <param name="id">
        /// The string id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<HChatClient> GetItemTask([CanBeNull] string id)
        {
            var parsed = Guid.TryParse(id, out var guid);
            if (!parsed)
            {
                return null;
            }

            return await GetItemTask(guid).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/> of HChatClients.
        /// </returns>
        [NotNull]
        public IEnumerable<HChatClient> GetItems()
        {
            return _clients.Values;
        }

        /// <summary>
        /// Gets all clients in User format.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/> of User's.
        /// </returns>
        [NotNull]
        public IEnumerable<User> GetUserItems()
        {
            return _clients.Values.Select(client => client.GetAsUser());
        }

        /// <summary>
        /// Checks if has client with given id.
        /// </summary>
        /// <param name="id">
        /// The client id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool HasItem(Guid id)
        {
            return _clients.ContainsKey(id);
        }

        /// <summary>
        /// Send to all using whole response message.
        /// </summary>
        /// <param name="message">
        /// The response message.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task SendToAllTask(ResponseMessage message)
        {
            Console.WriteLine("[SERVER] Sending message to everyone in channel: {0}", Name);
            var tasks = _clients.Values.Select(client => client.Connection.SendMessageTask(message.ToByteArray()));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Send to all using status, type and byte string.
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
        public async Task SendToAllTask(ResponseStatus status, RequestType type, ByteString message)
        {
            Console.WriteLine("[SERVER] Sending message to everyone in channel: {0}", Name);
            var tasks = _clients.Values.Select(client => client.SendResponseTask(status, type, message));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /*
        public static Predicate<HChannel> ByChannelName(string name)
        {
            return hChannel => hChannel.Name == name;
        }

        public static Predicate<HChannel> ByChannelId(string id)
        {
            return hChannel => hChannel.Id.ToString() == id;
        }*/
    }
}
