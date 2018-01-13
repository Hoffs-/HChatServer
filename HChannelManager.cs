using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace ChatServer
{
    public class HChannelManager
    {
        private readonly ConcurrentDictionary<Guid, HChannel> _hChannels = new ConcurrentDictionary<Guid, HChannel>();

        public async Task CreateChannel(string name)
        {
            await Task.Yield();
            var channel = new HChannel(name);
            _hChannels.TryAdd(channel.Guid, channel);
        }

        [ItemCanBeNull]
        public async Task<HChannel> FindChannelByName(string name)
        {
            await Task.Yield();
            return _hChannels.Values.FirstOrDefault(channel => channel.Name == name);
        }

        [ItemCanBeNull]
        public async Task<HChannel> FindChannelById(string id)
        {
            await Task.Yield();
            if (!Guid.TryParse(id, out var guid)) return null;
            _hChannels.TryGetValue(guid, out var channel);
            return channel;
        }

        [ItemCanBeNull]
        public async Task<HChannel> FindChannelByGuid(Guid guid)
        {
            await Task.Yield();
            _hChannels.TryGetValue(guid, out var channel);
            return channel;
        }
    }
}
