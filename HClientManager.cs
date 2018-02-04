namespace ChatServer
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using HServer;

    using JetBrains.Annotations;

    /// <summary>
    /// The client manager.
    /// </summary>
    public class HClientManager
    {
        /// <summary>
        /// The clients.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<HConnection, HChatClient> _clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="HClientManager"/> class.
        /// </summary>
        /// <param name="clients">
        /// The clients.
        /// </param>
        public HClientManager([NotNull] ConcurrentDictionary<HConnection, HChatClient> clients)
        {
            _clients = clients;
        }

        /// <summary>
        /// The add item task.
        /// </summary>
        /// <param name="item">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<bool> AddItemTask([NotNull] HChatClient item)
        {
            await Task.Yield();
            return _clients.TryAdd(item.Connection, item);
        }

        /// <summary>
        /// The remove item task.
        /// </summary>
        /// <param name="item">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<bool> RemoveItemTask([NotNull] HChatClient item)
        {
            await Task.Yield();
            return _clients.TryRemove(item.Connection, out var _);
        }

        /// <summary>
        /// The get item task.
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [ItemCanBeNull]
        public async Task<HChatClient> GetItemTask([NotNull] HConnection connection)
        {
            await Task.Yield();
            _clients.TryGetValue(connection, out var client);
            return client;
        }

        /// <summary>
        /// The get items task.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<IEnumerable<HChatClient>> GetItemsTask()
        {
            await Task.Yield();
            return _clients.Values;
        }
    }
}