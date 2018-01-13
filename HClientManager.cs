using System.Collections.Concurrent;
using System.Threading.Tasks;
using HServer;
using JetBrains.Annotations;

namespace ChatServer
{
    public class HClientManager
    {
        private readonly ConcurrentDictionary<HConnection, HChatClient> _chatClients = new ConcurrentDictionary<HConnection, HChatClient>();

        public void AddClient(HConnection connection, HChatClient client)
        {
            _chatClients.TryAdd(connection, client);
        }

        public void RemoveClient(HConnection connection)
        {
            _chatClients.TryRemove(connection, out var _);
        }

        [ItemCanBeNull]
        public async Task<HChatClient> GetClient([NotNull] HConnection connection)
        {
            await Task.Yield();
            _chatClients.TryGetValue(connection, out var client);
            return client;
        }
    }
}