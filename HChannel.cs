using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ChatProtos.Networking;
using Google.Protobuf;

namespace CoreServer
{
    public class HChannel
    {
        private readonly HashSet<HChatClient> _joinedClients = new HashSet<HChatClient>();
        public Guid Guid { get; } = Guid.NewGuid();
        public string Name { get; private set; }
        private readonly object _lock = new object();

        public HChannel(string name)
        {
            Name = name;
        }

        public async Task SendToAll(ResponseMessage message)
        {
            Console.WriteLine("[SERVER] Sending message to everyone in channel: {0}", Name);
            var tasks = _joinedClients.Select(async client => await client.Connection.SendAyncTask(message.ToByteArray()));
            await Task.WhenAll(tasks);
        }

        public bool AddClient(HChatClient client)
        {
            lock (_lock) // Add a check for permissions or if was invited.
            {
                try
                {
                    _joinedClients.Add(client);
                    return true;
                } catch (ArgumentException)
                {
                    Console.WriteLine("[SERVER] User already in the channel.");
                    return false;
                }
            }

        }

        public bool RemoveClient(HChatClient client)
        {
            lock (_lock)
            {
                if (_joinedClients.Contains(client))
                {
                    _joinedClients.Remove(client);
                    return true;
                } else
                {
                    return false;
                }
            }
        }

        public bool HasClient(HChatClient client)
        {
            lock (_lock)
            {
                return _joinedClients.Contains(client);
            }
        }

        public List<HChatClient> GetClients()
        {
            lock (_lock)
            {
                return _joinedClients.ToList();
            }
        }

        public static Predicate<HChannel> ByChannelName(string name)
        {
            return hChannel => hChannel.Name == name;
        }

        public static Predicate<HChannel> ByChannelId(string id)
        {
            return hChannel => hChannel.Guid.ToString() == id;
        }
    }
}
