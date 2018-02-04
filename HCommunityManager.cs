namespace ChatServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    /// Manager for Communities.
    /// </summary>
    public class HCommunityManager
    {
        /// <summary>
        /// HCommunities that exist.
        /// </summary>
        [NotNull]
        private readonly ConcurrentDictionary<Guid, HCommunity> _communities;

        /// <summary>
        /// Initializes a new instance of the <see cref="HCommunityManager"/> class.
        /// </summary>
        /// <param name="communities">
        /// The HCommunities.
        /// </param>
        public HCommunityManager([NotNull] ConcurrentDictionary<Guid, HCommunity> communities)
        {
            _communities = communities;
        }

        /// <summary>
        /// Adds a community.
        /// </summary>
        /// <param name="item">Community to be added</param>
        /// <returns>Boolean if community was added</returns>
        public async Task<bool> AddItemTask([NotNull] HCommunity item)
        {
            await Task.Yield();
            return _communities.TryAdd(item.Id, item);
        }

        /// <summary>
        /// Removes a community.
        /// </summary>
        /// <param name="item">Community to be removed</param>
        /// <returns>Boolean if community was removed</returns>
        public async Task<bool> RemoveItemTask([NotNull] HCommunity item)
        {
            await Task.Yield();
            return _communities.TryRemove(item.Id, out var _);
        }

        /// <summary>
        /// Gets a community with specified GUID.
        /// </summary>
        /// <param name="id">GUID of community</param>
        /// <returns>The <see cref="Task"/> with HCommunity result if it was found or null otherwise</returns>
        [ItemCanBeNull]
        public async Task<HCommunity> GetItemTask(Guid id)
        {
            await Task.Yield();
            return _communities.TryGetValue(id, out var result) ? result : null;
        }

        /// <summary>
        /// Gets a community with specified string id.
        /// </summary>
        /// <param name="id">
        /// The string id of community.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [ItemCanBeNull]
        public async Task<HCommunity> GetItemTask([CanBeNull] string id)
        {
            await Task.Yield();
            var parsed = Guid.TryParse(id, out var result);
            if (!parsed)
            {
                return null;
            }

            return _communities.TryGetValue(result, out var community) ? community : null;
        }

        /// <summary>
        /// Gets the <see cref="HCommunity"/> list.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<IEnumerable<HCommunity>> GetItemsTask()
        {
            await Task.Yield();
            return _communities.Values;
        }

        /// <summary>
        /// Tries to get the channel from community.
        /// </summary>
        /// <param name="community">
        /// The community id in string.
        /// </param>
        /// <param name="channel">
        /// The channel id in string.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> with <see cref="HChannel"/> item.
        /// </returns>
        [ItemCanBeNull]
        public async Task<HChannel> TryGetChannelTask([CanBeNull] string community, [CanBeNull] string channel)
        {
            var parsedCommunity = Guid.TryParse(community, out var communityId);
            var parsedChannel = Guid.TryParse(channel, out var channelId);
            if (!parsedCommunity || !parsedChannel)
            {
                return null;
            }

            var communityInstance = await GetItemTask(communityId).ConfigureAwait(false);
            if (communityInstance == null)
            {
                return null;
            }

            var channelInstance = await communityInstance.ChannelManager.GetItemTask(channelId).ConfigureAwait(false);
            return channelInstance;
        }

        /// <summary>
        /// Tries to get channel using given channel id string.
        /// </summary>
        /// <param name="channel">
        /// The channel id string.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> with <see cref="HChannel"/> item.
        /// </returns>
        [ItemCanBeNull]
        public async Task<HChannel> TryGetChannelTask([CanBeNull] string channel)
        {
            var parsed = Guid.TryParse(channel, out var id);
            if (!parsed)
            {
                return null;
            }

            foreach (var community in _communities.Values)
            {
                var result = await community.ChannelManager.GetItemTask(id).ConfigureAwait(false);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}