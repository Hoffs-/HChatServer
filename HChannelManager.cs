using System.Collections.Concurrent;
using System.Linq;
using CoreServer;
using JetBrains.Annotations;

namespace ChatServer
{
    public class HChannelManager
    {
        private readonly ConcurrentDictionary<string, HChannel> _hChannels = new ConcurrentDictionary<string, HChannel>();

        public void CreateChannel(string name)
        {
            var channel = new HChannel(name);
            _hChannels.TryAdd(channel.Guid.ToString(), channel);
        }

        [CanBeNull]
        public HChannel FindChannelByName(string name)
        {
            return _hChannels.Values.FirstOrDefault(channel => channel.Name == name);
        }

        [CanBeNull]
        public HChannel FindChannelById(string id)
        {
            _hChannels.TryGetValue(id, out var channel);
            return channel;
        }
    }
}
