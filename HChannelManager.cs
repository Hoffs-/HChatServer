namespace ChatServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    /// The channel manager.
    /// </summary>
    public class HChannelManager
    {
        /// <summary>
        /// The channels.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, HChannel> _channels;

        /// <summary>
        /// Initializes a new instance of the <see cref="HChannelManager"/> class.
        /// </summary>
        /// <param name="channels">
        /// The channels.
        /// </param>
        public HChannelManager([NotNull] ConcurrentDictionary<Guid, HChannel> channels)
        {
            _channels = channels;
        }

        /// <summary>
        /// Adds channel.
        /// </summary>
        /// <param name="item">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<bool> AddItemTask([NotNull] HChannel item)
        {
            await Task.Yield();
            return _channels.TryAdd(item.Id, item);
        }

        /// <summary>
        /// Removes channel.
        /// </summary>
        /// <param name="item">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<bool> RemoveItemTask([NotNull] HChannel item)
        {
            await Task.Yield();
            return _channels.TryRemove(item.Id, out var _);
        }

        /// <summary>
        /// Gets channel using GUID.
        /// </summary>
        /// <param name="id">
        /// The GUID.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [ItemCanBeNull]
        public async Task<HChannel> GetItemTask(Guid id)
        {
            await Task.Yield();
            _channels.TryGetValue(id, out var result);
            return result;
        }

        /// <summary>
        /// Gets all channels.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<IEnumerable<HChannel>> GetChannelsTask()
        {
            await Task.Yield();
            return _channels.Values;
        }
    }
}
