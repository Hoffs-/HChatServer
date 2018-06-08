namespace ChatServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
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
        public HChannel(
            Guid id,
            [NotNull] string name,
            [NotNull] HCommunity parent,
            [NotNull] ConcurrentDictionary<Guid, HChatClient> clients,
            DateTime created)
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
        /// A <see cref="bool"/> if client was added.
        /// </returns>
        public bool AddItem([NotNull] HChatClient item)
        {
            // TODO: Maybe add permission checking
            return _clients.TryAdd(item.Id, item);
        }

        /// <summary>
        /// Removes client from the community.
        /// </summary>
        /// <param name="item">
        /// The client.
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> if client was removed.
        /// </returns>
        public bool RemoveItem([NotNull] HChatClient item)
        {
            return _clients.TryRemove(item.Id, out var _);
        }

        /// <summary>
        /// Gets client with given id.
        /// </summary>
        /// <param name="id">
        /// The client id.
        /// </param>
        /// <returns>
        /// A <see cref="HChatClient"/>.
        /// </returns>
        [CanBeNull]
        public HChatClient GetItem(Guid id)
        {
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
        [CanBeNull]
        public HChatClient GetItem([CanBeNull] string id)
        {
            var parsed = Guid.TryParse(id, out var guid);
            return !parsed ? null : GetItem(guid);
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
            return _clients.Values.Select(client => client.GetAsUserProto());
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
        public Task SendToAllTask(ResponseMessage message)
        {
            Console.WriteLine("[SERVER] Sending message to everyone in channel: {0}", Name);
            var tasks = _clients.Values.Select(client => client.Connection.SendMessageTask(message.ToByteArray()));
            return Task.WhenAll(tasks);
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
        public Task SendToAllTask(ResponseStatus status, RequestType type, ByteString message)
        {
            Console.WriteLine("[SERVER] Sending message to everyone in channel: {0}", Name);
            var tasks = _clients.Values.Select(client => client.SendResponseTaskAsync(status, type, message));
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Get as protobuf object.
        /// </summary>
        /// <returns>
        /// The protobuf <see cref="Channel"/>.
        /// </returns>
        // ReSharper disable once StyleCop.SA1650
        public Channel GetAsProto()
        {
            return new Channel { Id = Id.ToString(), Name = Name, };
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

        /// <summary>
        /// Gets channel as channel info proto.
        /// </summary>
        /// <returns>
        /// The <see cref="ChannelInfo"/>.
        /// </returns>
        public ChannelInfo GetAsChannelInfoProto()
        {
            return new ChannelInfo
                       {
                           Created = Created.ToString(CultureInfo.InvariantCulture),
                           Users = { _clients.Values.Select(client => client.GetAsUserProto()) }
                       };
        }
    }
}