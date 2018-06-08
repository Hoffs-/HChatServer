namespace ChatServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    /// The channel manager.
    /// </summary>
    public class HChannelManager
    {
        /// <summary>
        /// The max channel count.
        /// </summary>
        private const int MaxChannels = 50;

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
        /// A <see cref="bool"/> depending on success.
        /// </returns>
        public bool AddItem([NotNull] HChannel item)
        {
            if (_channels.Count > MaxChannels)
            {
                return false; // Should probably throw exception
            }

            return _channels.TryAdd(item.Id, item);
        }

        /// <summary>
        /// Removes channel.
        /// </summary>
        /// <param name="item">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/> if channel was removed.
        /// </returns>
        public bool RemoveItem([NotNull] HChannel item)
        {
            return _channels.TryRemove(item.Id, out var _);
        }

        /// <summary>
        /// Gets channel using GUID.
        /// </summary>
        /// <param name="id">
        /// The GUID.
        /// </param>
        /// <returns>
        /// The <see cref="HChannel"/>.
        /// </returns>
        [CanBeNull]
        public HChannel GetItem(Guid id)
        {
            _channels.TryGetValue(id, out var result);
            return result;
        }

        /// <summary>
        /// Gets all channels.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public IEnumerable<HChannel> GetChannels()
        {
            return _channels.Values.ToArray();
        }

        public String test()
        {
            return "a";
        }
    }
}
