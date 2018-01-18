using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatProtos.Networking;
using Google.Protobuf;
using HServer.Networking;

namespace ChatServer
{
    public class HChannel
    {
        private readonly BlockingCollection<HChatClient> _joinedClients = new BlockingCollection<HChatClient>();
        public Guid Guid { get; } = Guid.NewGuid();
        public string Name { get; private set; }

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

        // Maybe force these calls to be async too? In case they get blocked.
        public bool AddClient(HChatClient client)
        {
            // TODO: Check for permissions/invitation
            try
            {
                _joinedClients.TryAdd(client);
                return true;
            } catch (ArgumentException)
            {
                Console.WriteLine("[SERVER] User already in the channel.");
                return false;
            }

        }

        public bool RemoveClient(HChatClient client)
        {
            if (!_joinedClients.Contains(client)) return false;
            
            _joinedClients.TryTake(out client);
            return true;
        }

        public bool HasClient(HChatClient client)
        {
            return _joinedClients.Contains(client);
        }

        public List<HChatClient> GetClients()
        {
            return _joinedClients.ToList();
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
