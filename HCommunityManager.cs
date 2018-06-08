namespace ChatServer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ChatProtos.Networking;
    using ChatProtos.Networking.Messages;

    using Google.Protobuf;

    using HServer.Networking;

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
        /// <param name="item">
        /// Community to be added
        /// </param>
        /// <returns>
        /// The <see cref="bool"/> if community was added.
        /// </returns>
        public bool AddItem([NotNull] HCommunity item)
        {
            return _communities.TryAdd(item.Id, item);
        }

        /// <summary>
        /// Removes a community.
        /// </summary>
        /// <param name="item">
        /// Community to be removed
        /// </param>
        /// <returns>
        /// The <see cref="bool"/> if community was removed.
        /// </returns>
        public bool RemoveItem([NotNull] HCommunity item)
        {
            return _communities.TryRemove(item.Id, out var _);
        }

        /// <summary>
        /// Gets a community with specified GUID.
        /// </summary>
        /// <param name="id">GUID of community</param>
        /// <returns>The <see cref="HCommunity"/></returns>
        [CanBeNull]
        public HCommunity GetItem(Guid id)
        {
            return _communities.TryGetValue(id, out var result) ? result : null;
        }

        /// <summary>
        /// Gets a community with specified string id.
        /// </summary>
        /// <param name="id">
        /// The string id of community.
        /// </param>
        /// <returns>
        /// The <see cref="HCommunity"/>.
        /// </returns>
        [CanBeNull]
        public HCommunity GetItem([CanBeNull] string id)
        {
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
        public IEnumerable<HCommunity> GetItemsTask()
        {
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
        /// A <see cref="HChannel"/>.
        /// </returns>
        [CanBeNull]
        public HChannel TryGetChannel([CanBeNull] string community, [CanBeNull] string channel)
        {
            var parsedCommunity = Guid.TryParse(community, out var communityId);
            var parsedChannel = Guid.TryParse(channel, out var channelId);
            if (!parsedCommunity || !parsedChannel)
            {
                return null;
            }

            var communityInstance = GetItem(communityId);

            var channelInstance = communityInstance?.ChannelManager.GetItem(channelId);
            return channelInstance;
        }

        /// <summary>
        /// Tries to get channel using given channel id string.
        /// </summary>
        /// <param name="channel">
        /// The channel id string.
        /// </param>
        /// <returns>
        /// A <see cref="HChannel"/>.
        /// </returns>
        [CanBeNull]
        public HChannel TryGetChannel([CanBeNull] string channel)
        {
            var parsed = Guid.TryParse(channel, out var id);
            if (!parsed)
            {
                return null;
            }

            return _communities.Values.Select(community => community.ChannelManager.GetItem(id)).FirstOrDefault(result => result != null);
        }
    }
}